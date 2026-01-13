"""
Intersection agent for answering questions that require items matching multiple criteria.
"""
import json
from typing import Annotated
from agent_framework import ChatAgent, ai_function
from agent_framework.azure import AzureOpenAIChatClient

from services import SearchService


INTERSECTION_AGENT_INSTRUCTIONS = """
You are a specialist in answering intersection questions about IT support tickets.

When you receive a question asking about items that match BOTH criterion A AND criterion B:
1. Use the intersection_search function to retrieve relevant tickets
2. Analyze the results to find items matching ALL specified criteria
3. Provide a clear list of matching items
4. Cite specific examples from the tickets using their IDs

Example response format:

Based on the search results, here are Dell XPS issues where the user tried Win + Ctrl + Shift + B:

1. **Ticket INC001234**: Dell XPS 15 black screen issue
   - Description: User reports black screen, attempted Win+Ctrl+Shift+B with no success
   - Status: Requires hardware diagnostics

2. **Ticket INC001567**: Dell XPS 13 display not responding
   - Description: Display frozen, user tried Win+Ctrl+Shift+B reset, issue persists
   - Status: Display driver reinstallation resolved the issue

**Summary**: Found 2 Dell XPS issues where users attempted the Win+Ctrl+Shift+B graphics reset.

Be precise and base your answer strictly on the search results.
Clearly explain what criteria were matched.
"""


def create_intersection_search_function(search_service: SearchService):
    """
    Factory function to create an intersection search AI function with the search service.
    
    Args:
        search_service: Initialized SearchService instance
        
    Returns:
        AI function for intersection searches
    """
    
    @ai_function
    async def intersection_search(
        user_question: Annotated[str, "User question requiring finding items that match multiple criteria"]
    ) -> str:
        """
        Answers intersection questions by finding items that match ALL specified criteria.
        Useful for questions like "What X are for Y and Z?" or "Which items match A and B?"
        """
        # Parse the question to identify the multiple search criteria
        parse_prompt = f"""
Analyze this question and extract the multiple search criteria:

Question: {user_question}

Extract all the distinct criteria that items must match. Format your response as JSON:
{{
    "criterion_1": "first search criterion",
    "criterion_2": "second search criterion",
    "additional_criteria": ["any other criteria if present"],
    "combined_search": "a single combined search query that includes all criteria",
    "explanation": "brief explanation of what we're looking for"
}}

Examples:
- "What issues are for Dell XPS laptops and the user tried Win + Ctrl + Shift + B?" 
  -> {{"criterion_1": "Dell XPS laptops", "criterion_2": "Win + Ctrl + Shift + B", "additional_criteria": [], "combined_search": "Dell XPS Win Ctrl Shift B graphics reset", "explanation": "Find Dell XPS issues where users tried the graphics reset keystroke"}}
  
- "Which Surface tickets involve battery problems and high priority?"
  -> {{"criterion_1": "Surface", "criterion_2": "battery problems", "additional_criteria": ["high priority"], "combined_search": "Surface battery high priority", "explanation": "Find high priority Surface tickets with battery issues"}}

Respond ONLY with the JSON object.
"""
        
        # Call LLM to parse the question
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
            criterion_1 = parsed["criterion_1"]
            criterion_2 = parsed["criterion_2"]
            combined_search = f"'{criterion_1}' AND '{criterion_2}'"
            explanation = parsed.get("explanation", "")
        except Exception as e:
            return (
                f"Question: {user_question}\n\n"
                f"Error: Unable to parse the intersection question. Please rephrase your question.\n"
                f"Details: {str(e)}"
            )
        
        # Perform combined search to find items matching all criteria
        combined_results = search_service.search_tickets(combined_search, top_k=20, include_semantic_search=False)
        
        # Also perform individual searches to verify intersection
        search_1_results = search_service.search_tickets(criterion_1, top_k=50, include_semantic_search=False)
        search_2_results = search_service.search_tickets(criterion_2, top_k=50, include_semantic_search=False)
        
        # Get IDs from each search
        search_1_ids = {result["id"] for result in search_1_results}
        search_2_ids = {result["id"] for result in search_2_results}
        
        # Find intersection: items that appear in BOTH individual searches
        intersection_ids = search_1_ids & search_2_ids
        
        # Filter combined results to only include items in the intersection
        # Also include items from combined search that match well
        intersection_results = [
            result for result in combined_results
            if result["id"] in intersection_ids or 
               (criterion_1.lower() in result.get("subject", "").lower() or 
                criterion_1.lower() in result.get("body", "").lower()) and
               (criterion_2.lower() in result.get("subject", "").lower() or 
                criterion_2.lower() in result.get("body", "").lower())
        ]
        
        # Remove duplicates based on ID
        seen_ids = set()
        unique_results = []
        for result in intersection_results:
            if result["id"] not in seen_ids:
                seen_ids.add(result["id"])
                unique_results.append(result)
        
        # Check if any results found
        if not unique_results:
            return (
                f"Question: {user_question}\n\n"
                f"Search Logic: {explanation}\n"
                f"- Found {len(search_1_results)} tickets matching '{criterion_1}'\n"
                f"- Found {len(search_2_results)} tickets matching '{criterion_2}'\n"
                f"- Intersection: 0 tickets match BOTH criteria\n\n"
                f"No tickets were found that match all specified criteria."
            )
        
        # Build analysis prompt with intersection results
        results_json = json.dumps(unique_results, indent=2, ensure_ascii=False)
        
        analysis_prompt = f"""
Based on the following IT support tickets, answer the intersection question.

Question: {user_question}

Search Logic: {explanation}
- Found {len(search_1_results)} tickets matching "{criterion_1}"
- Found {len(search_2_results)} tickets matching "{criterion_2}"
- Intersection: {len(unique_results)} tickets match ALL criteria

Tickets that match all criteria:
{results_json}

Provide a detailed answer that:
1. Lists the tickets that meet ALL criteria
2. Provides brief descriptions of each ticket highlighting how they match the criteria
3. Groups similar issues together if appropriate
4. Summarizes the findings

Format your response clearly with ticket IDs and descriptions.
Base your answer strictly on the search results provided.
"""
        
        return analysis_prompt
    
    return intersection_search


def create_intersection_agent(
    chat_client: AzureOpenAIChatClient,
    search_service: SearchService
) -> ChatAgent:
    """
    Create the intersection specialist agent.
    
    Args:
        chat_client: Azure OpenAI chat client
        search_service: Search service for ticket queries
        
    Returns:
        Configured intersection ChatAgent with search capabilities
    """
    # Create the AI function with the search service
    intersection_search_fn = create_intersection_search_function(search_service)
    
    return chat_client.create_agent(
        instructions=INTERSECTION_AGENT_INSTRUCTIONS,
        name="intersection_agent",
        tools=[intersection_search_fn],
    )
