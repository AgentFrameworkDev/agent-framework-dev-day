"""
Count agent for answering questions that require counting tickets with filters.
"""
import json
from typing import Annotated
from agent_framework import ChatAgent, ai_function
from agent_framework.azure import AzureOpenAIChatClient

from services import SearchService


COUNT_AGENT_INSTRUCTIONS = """
You are a specialist in answering counting questions about IT support tickets.

When you receive a question:
1. Use the count_search function to retrieve and count relevant tickets
2. Analyze the search results to get an accurate count
3. Provide a clear COUNT number
4. Give a brief breakdown or explanation
5. Cite specific examples from the tickets using their IDs

Example response format:

Count: 3

Breakdown:
- 3 Incident tickets logged for Human Resources with low priority

Examples:
- Ticket INC001234: HR system login issue - Low priority
- Ticket INC001567: HR portal access problem - Low priority
- Ticket INC002345: HR database sync issue - Low priority

Be precise with the count and base your answer strictly on the search results.
If the search doesn't return enough results to be confident, mention that the actual count might be higher.
"""


def create_count_search_function(search_service: SearchService):
    """
    Factory function to create a count search AI function with the search service.
    
    Args:
        search_service: Initialized SearchService instance
        
    Returns:
        AI function for count searches
    """
    
    @ai_function
    async def count_search(
        user_question: Annotated[str, "User question requiring counting items"]
    ) -> str:
        """
        Answers counting questions by searching and counting tickets in the IT support database.
        Returns count with breakdown and examples.
        """
        # Generate OData filter using LLM
        filter_prompt = f"""
You are an expert at converting natural language questions into OData filter expressions for Azure AI Search.

Database Schema:
- Type: string (values: "Incident", "Request", "Problem", "Change")
- Queue: string (department name like "Human Resources", "IT", "Finance", etc.)
- Priority: string (values: "high", "medium", "low")
- Business_Type: string
- Tags: collection of strings

Question: {user_question}

Generate ONLY the OData filter expression. Use proper OData syntax:
- String equality: field eq 'value'
- AND conditions: field1 eq 'value1' and field2 eq 'value2'
- Case-sensitive string matching

Examples:
- "Incidents for Human Resources with low priority" -> Type eq 'Incident' and Queue eq 'Human Resources' and Priority eq 'low'
- "High priority tickets for IT" -> Priority eq 'high' and Queue eq 'IT'

If no filters are needed, respond with: NO_FILTER

OData filter:
"""
        
        # Call LLM to generate filter
        from agent_framework import ChatMessage
        try:

            filter_response = await search_service.chat_client.get_response(
                messages=filter_prompt
            )
            odata_filter = filter_response.messages[0].text.strip()
        except Exception as e:
            odata_filter = "NO_FILTER"
        
        # Clean up the response
        if "NO_FILTER" in odata_filter or not odata_filter:
            odata_filter = None
        
        # Execute search with filter and get more results for accurate counting
        search_results = search_service.search_tickets_with_filter(
            user_question, 
            odata_filter=odata_filter,
            top_k=50  # Get more results for better count accuracy
        )
        
        # Check if any results found
        if not search_results:
            return (
                f"Question: {user_question}\n\n"
                "Count: 0\n\n"
                "No tickets were found matching the specified criteria."
            )
        
        # Build analysis prompt with search results
        results_json = json.dumps(search_results, indent=2, ensure_ascii=False)
        filter_info = f"\nApplied Filter: {odata_filter}" if odata_filter else "\nNo filter applied (semantic search only)"
        
        count_prompt = f"""
Based on the following IT support tickets, answer the counting question.

Question: {user_question}{filter_info}

Relevant Tickets Found (showing up to 50):
{results_json}

IMPORTANT: Analyze these tickets carefully and count only those that match ALL the criteria in the question.
Pay attention to:
- Type field (Incident, Request, Problem, etc.)
- Queue/Department field 
- Priority field (high, medium, low)
- Any other specific criteria mentioned

Format your response as:

Count: [NUMBER]

Breakdown:
- [Description of what was counted]

Examples (list a few matching tickets):
- Ticket [ID]: [Brief description with relevant fields]
- Ticket [ID]: [Brief description with relevant fields]

Note: This count is based on the top 10 search results. The actual total may be higher if more tickets exist in the database.

Base your count strictly on tickets that match ALL criteria in the question.
"""
        
        return count_prompt
    
    return count_search


def create_count_agent(
    chat_client: AzureOpenAIChatClient,
    search_service: SearchService
) -> ChatAgent:
    """
    Create the count specialist agent.
    
    Args:
        chat_client: Azure OpenAI chat client
        search_service: Search service for ticket queries
        
    Returns:
        Configured count ChatAgent with search capabilities
    """
    # Create the AI function with the search service
    count_search_fn = create_count_search_function(search_service)
    
    return chat_client.create_agent(
        instructions=COUNT_AGENT_INSTRUCTIONS,
        name="count_agent",
        tools=[count_search_fn],
    )
