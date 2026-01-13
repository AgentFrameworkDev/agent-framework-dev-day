"""
Multi-hop agent for answering questions requiring multi-step reasoning.
"""
import json
from typing import Annotated
from agent_framework import ChatAgent, ai_function
from agent_framework.azure import AzureOpenAIChatClient

from services import SearchService


MULTI_HOP_AGENT_INSTRUCTIONS = """
You are a specialist in answering multi-hop reasoning questions about IT support tickets.

When you receive a question that requires finding information A and then extracting information B:
1. Use the multi_hop_search function to retrieve relevant tickets
2. Analyze the tickets to extract the specific information requested
3. Provide a clear answer to the question
4. Cite specific examples from the tickets using their IDs

Example response format:

Based on the IT support tickets, the following departments had consultants with Login Issues:

**Human Resources Department**
- Ticket INC001234: HR consultant unable to access system
- Ticket INC001567: External HR consultant login failed

**Finance Department**  
- Ticket INC002345: Finance consultant VPN login issue

**Summary**: 2 departments (Human Resources and Finance) had consultants experiencing login issues.

Be precise and base your answer strictly on the search results.
Focus on answering the specific question asked (e.g., "What department" means list the departments).
"""


def create_multi_hop_search_function(search_service: SearchService):
    """
    Factory function to create a multi-hop search AI function with the search service.
    
    Args:
        search_service: Initialized SearchService instance
        
    Returns:
        AI function for multi-hop searches
    """
    
    @ai_function
    async def multi_hop_search(
        user_question: Annotated[str, "User question requiring multi-hop reasoning"]
    ) -> str:
        """
        Answers multi-hop questions by searching for one concept and extracting different 
        information from those results. Useful for questions like "What X had Y?" where 
        you need to find Y first, then determine X from those results.
        """
        # Parse the question to identify the search query and target extraction
        parse_prompt = f"""
Analyze this multi-hop reasoning question and extract:

Question: {user_question}

Extract:
1. SEARCH_QUERY: What to search for (the main topic/constraint)
2. TARGET_FIELD: What information to extract from the results (what the question is actually asking for)
3. REASONING: Brief explanation of the multi-hop logic

Format your response as JSON:
{{
    "search_query": "what to search for in tickets",
    "target_field": "what field/information to extract (e.g., department, priority, type, user, etc.)",
    "reasoning": "brief explanation of the multi-hop reasoning"
}}

Examples:
- "What department had consultants with Login Issues?" 
  -> {{"search_query": "consultants Login Issues", "target_field": "department (Queue field)", "reasoning": "Search for tickets about consultants with login issues, then extract which departments those tickets belong to"}}
  
- "Which priority level has the most printer problems?"
  -> {{"search_query": "printer problems", "target_field": "priority level (Priority field)", "reasoning": "Search for printer problem tickets, then analyze their priority levels"}}

- "What ticket type do Surface issues usually get classified as?"
  -> {{"search_query": "Surface issues", "target_field": "ticket type (Type field)", "reasoning": "Search for Surface-related tickets, then determine their ticket types"}}

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
            search_query = parsed["search_query"]
            target_field = parsed["target_field"]
            reasoning = parsed.get("reasoning", "")
        except Exception as e:
            return (
                f"Question: {user_question}\n\n"
                f"Error: Unable to parse the multi-hop question. Please rephrase your question.\n"
                f"Details: {str(e)}"
            )
        
        # Perform the initial search
        search_results = search_service.search_tickets(search_query, top_k=20)
        
        # Check if any results found
        if not search_results:
            return (
                f"Question: {user_question}\n\n"
                f"Search Query: {search_query}\n"
                f"No tickets were found matching the search criteria."
            )
        
        # Build analysis prompt with search results
        results_json = json.dumps(search_results, indent=2, ensure_ascii=False)
        
        analysis_prompt = f"""
Based on the following IT support tickets, answer the multi-hop reasoning question.

Question: {user_question}

Multi-Hop Reasoning: {reasoning}
- Search performed: "{search_query}"
- Target information to extract: {target_field}
- Found {len(search_results)} relevant tickets

Relevant Tickets:
{results_json}

Provide a detailed answer that:
1. Directly answers the question by extracting the {target_field} from the tickets
2. Groups results by the target field (e.g., if asking about departments, group by department)
3. Cites specific ticket IDs as evidence
4. Provides a summary with counts or key findings

IMPORTANT: Focus on answering what the question is asking for ({target_field}), not just describing the tickets.
If the question asks "What department", list the departments.
If the question asks "Which priority", list the priority levels.

Format your response clearly with the extracted information prominently displayed.
Base your answer strictly on the search results provided.
"""
        
        return analysis_prompt
    
    return multi_hop_search


def create_multi_hop_agent(
    chat_client: AzureOpenAIChatClient,
    search_service: SearchService
) -> ChatAgent:
    """
    Create the multi-hop reasoning specialist agent.
    
    Args:
        chat_client: Azure OpenAI chat client
        search_service: Search service for ticket queries
        
    Returns:
        Configured multi-hop ChatAgent with search capabilities
    """
    # Create the AI function with the search service
    multi_hop_search_fn = create_multi_hop_search_function(search_service)
    
    return chat_client.create_agent(
        instructions=MULTI_HOP_AGENT_INSTRUCTIONS,
        name="multi_hop_agent",
        tools=[multi_hop_search_fn],
    )
