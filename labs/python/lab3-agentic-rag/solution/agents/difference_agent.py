"""
Difference agent for answering questions that require finding items in one set but not in another.
"""
import json
from typing import Annotated
from agent_framework import ChatAgent, ai_function
from agent_framework.azure import AzureOpenAIChatClient

from services import SearchService


DIFFERENCE_AGENT_INSTRUCTIONS = """
You are a specialist in answering difference/exclusion questions about IT support tickets.

When you receive a question asking about items that match one criterion but NOT another:
1. Use the difference_search function to retrieve relevant tickets
2. Analyze the two sets of results (items matching main criteria vs. items matching exclusion criteria)
3. Identify items that appear in the first set but NOT in the second
4. Provide a clear list of the differences
5. Cite specific examples from the tickets using their IDs

Example response format:

Based on the search results, here are Dell XPS issues that do NOT mention Windows:

1. **Ticket INC001234**: Dell XPS 15 touchpad not responding
   - Description: Physical touchpad hardware issue, requires replacement

2. **Ticket INC001567**: Dell XPS 13 battery swelling
   - Description: Battery hardware defect, safety recall initiated

3. **Ticket INC002345**: Dell XPS keyboard backlight malfunction
   - Description: LED backlight circuit failure

**Summary**: Found 3 Dell XPS issues that do not mention Windows. These are primarily hardware-related problems rather than software issues.

Be precise and base your answer strictly on the search results.
Clearly explain what was excluded and why.
"""


def create_difference_search_function(search_service: SearchService):
    """
    Factory function to create a difference search AI function with the search service.
    
    Args:
        search_service: Initialized SearchService instance
        
    Returns:
        AI function for difference searches
    """
    
    @ai_function
    async def difference_search(
        user_question: Annotated[str, "User question requiring finding differences between two sets"]
    ) -> str:
        """
        Answers difference questions by performing two searches and finding items in the first set
        that are NOT in the second set. Useful for questions like "Which X does not mention Y?"
        """
        # Parse the question to identify the main search and exclusion criteria
        parse_prompt = f"""
Analyze this question and extract two search queries:

Question: {user_question}

Extract:
1. MAIN_SEARCH: The primary topic/category to search for
2. EXCLUSION_TERM: The term/concept that should NOT appear in the results

Format your response as JSON:
{{
    "main_search": "the primary search query",
    "exclusion_term": "the term to exclude",
    "explanation": "brief explanation of the logic"
}}

Examples:
- "Which Dell XPS Issue does not mention Windows?" 
  -> {{"main_search": "Dell XPS Issue", "exclusion_term": "Windows", "explanation": "Find Dell XPS issues, exclude those mentioning Windows"}}
  
- "What Surface problems don't involve the battery?"
  -> {{"main_search": "Surface problems", "exclusion_term": "battery", "explanation": "Find Surface problems, exclude those involving battery"}}

Respond ONLY with the JSON object.
"""
        
        # Call LLM to parse the question
        from agent_framework import ChatMessage
        parse_response = await search_service.chat_client.get_response(
            messages=parse_prompt
        )
        
        try:
            # Extract JSON from response
            response_text = parse_response.messages[0].text.strip()
            # Remove markdown code blocks if present
            if "```json" in response_text:
                response_text = response_text.split("```json")[1].split("```")[0].strip()
            elif "```" in response_text:
                response_text = response_text.split("```")[1].split("```")[0].strip()
            
            parsed = json.loads(response_text)
            main_search = parsed["main_search"]
            exclusion_term = parsed["exclusion_term"]
            explanation = parsed.get("explanation", "")
        except Exception as e:
            return (
                f"Question: {user_question}\n\n"
                f"Error: Unable to parse the difference question. Please rephrase your question.\n"
                f"Details: {str(e)}"
            )
        
        # Perform first search: get all items matching main criteria
        main_results = search_service.search_tickets(main_search, top_k=20)
        
        # Perform second search: get items matching main criteria AND exclusion term
        combined_query = f"{main_search} {exclusion_term}"
        exclusion_results = search_service.search_tickets(combined_query, top_k=20)
        
        # Get IDs from exclusion results
        exclusion_ids = {result["id"] for result in exclusion_results}
        
        # Find difference: items in main_results but not in exclusion_results
        difference_results = [
            result for result in main_results 
            if result["id"] not in exclusion_ids
        ]
        
        # Check if any results found
        if not main_results:
            return (
                f"Question: {user_question}\n\n"
                f"No tickets were found matching the main search criteria: '{main_search}'"
            )
        
        if not difference_results:
            return (
                f"Question: {user_question}\n\n"
                f"All tickets matching '{main_search}' also mention '{exclusion_term}'.\n"
                f"No differences found."
            )
        
        # Build analysis prompt with difference results
        results_json = json.dumps(difference_results, indent=2, ensure_ascii=False)
        
        analysis_prompt = f"""
Based on the following IT support tickets, answer the difference question.

Question: {user_question}

Search Logic: {explanation}
- Found {len(main_results)} tickets matching "{main_search}"
- Found {len(exclusion_results)} tickets that also mention "{exclusion_term}"
- Difference: {len(difference_results)} tickets match "{main_search}" but do NOT mention "{exclusion_term}"

Tickets that match the difference criteria:
{results_json}

Provide a detailed answer that:
1. Lists the tickets that meet the difference criteria (in main set but NOT in exclusion set)
2. Provides brief descriptions of each ticket
3. Groups similar issues together if appropriate
4. Summarizes the findings

Format your response clearly with ticket IDs and descriptions.
Base your answer strictly on the search results provided.
"""
        
        return analysis_prompt
    
    return difference_search


def create_difference_agent(
    chat_client: AzureOpenAIChatClient,
    search_service: SearchService
) -> ChatAgent:
    """
    Create the difference specialist agent.
    
    Args:
        chat_client: Azure OpenAI chat client
        search_service: Search service for ticket queries
        
    Returns:
        Configured difference ChatAgent with search capabilities
    """
    # Create the AI function with the search service
    difference_search_fn = create_difference_search_function(search_service)
    
    return chat_client.create_agent(
        instructions=DIFFERENCE_AGENT_INSTRUCTIONS,
        name="difference_agent",
        tools=[difference_search_fn],
    )
