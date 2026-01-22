"""
Ordinal agent for answering questions that require finding items based on order/position.
"""
import json
from typing import Annotated
from agent_framework import ChatAgent, ai_function
from agent_framework.azure import AzureOpenAIChatClient

from services import SearchService


ORDINAL_AGENT_INSTRUCTIONS = """
You are a specialist in answering ordinal/order-based questions about IT support tickets.

## Database Schema
The database contains IT support tickets with these fields:
- Id: unique identifier
- Create_Date: ticket creation date (use for ordering by time)
- Subject: ticket subject
- Body: ticket question/description
- Answer: ticket response/solution
- Type: ticket type (values: "Incident", "Request", "Problem", "Change")
- Queue: department name (values: "Human Resources", "IT", "Finance", "Operations", "Sales", "Marketing", "Engineering", "Support")
- Priority: priority level (values: "high", "medium", "low")
- Language: ticket language
- Business_Type: business category
- Tags: categorization tags

When you receive a question asking about items based on their position or order:
1. Use the ordinal_search function to retrieve relevant tickets with ordering
2. Analyze the search results to identify items at the requested position (ordered by Create_Date)
3. Provide a clear answer identifying the specific item(s) at that position
4. Cite specific details from the tickets using their IDs

Example response format:

Based on the search results, here is the last issue for the HR department:

**Ticket INC002345**: Employee onboarding portal access issue
- Created: 2024-01-15
- Priority: Medium
- Type: Incident
- Queue: Human Resources
- Subject: Employee onboarding portal access issue
- Description: New employee unable to access onboarding portal after account creation

This is the most recent ticket (by Create_Date) logged for the Human Resources department.

Be precise and base your answer strictly on the search results.
Clearly explain which position/order criterion was used.
"""


def create_ordinal_search_function(search_service: SearchService):
    """
    Factory function to create an ordinal search AI function with the search service.
    
    Args:
        search_service: Initialized SearchService instance
        
    Returns:
        AI function for ordinal searches
    """
    
    @ai_function
    async def ordinal_search(
        user_question: Annotated[str, "User question requiring finding items based on order/position"]
    ) -> str:
        """
        Answers ordinal questions by searching tickets and identifying items at specific positions
        (first, last, most recent, oldest, top, bottom, nth item).
        """
        # Parse the question to identify the ordinal criterion and search criteria
        parse_prompt = f"""
Analyze this question and extract the ordinal information and search criteria:

Question: {user_question}

Extract:
1. ORDINAL_TYPE: The ordinal/position requested (first, last, most recent, oldest, second, third, top N, bottom N, etc.)
2. SEARCH_QUERY: The main search criteria/topic
3. ODATA_FILTER: OData filter expression for any field constraints (Type, Queue, Priority, etc.)
4. SORT_ORDER: How to sort the results by Create_Date to find the ordinal position (asc or desc, based on what makes sense for the ordinal)

Database Schema for OData filters:
- Id: unique identifier
- Create_Date: ticket creation date (use for ordering by time)
- Subject: ticket subject
- Body: ticket question/description
- Answer: ticket response/solution
- Type: string (values: "Incident", "Request", "Problem", "Change")
- Queue: string (department name, values: "Human Resources", "IT", "Finance", "Operations", "Sales", "Marketing", "Engineering", "Support")
- Priority: string (values: "high", "medium", "low")
- Language: ticket language
- Business_Type: business category
- Tags: collection of strings

Format your response as JSON:
{{
    "ordinal_type": "the ordinal position requested (e.g., 'last', 'first', 'most recent', '3rd')",
    "search_query": "the search topic/criteria",
    "odata_filter": "OData filter expression or null if none needed",
    "sort_order": "desc for last/most recent/latest, asc for first/oldest/earliest",
    "position_index": "0 for first/oldest, -1 for last/most recent, or specific 0-based index",
    "explanation": "brief explanation of the logic"
}}

Examples:
- "What is the last issue for the HR department?" 
  -> {{"ordinal_type": "last", "search_query": "issues", "odata_filter": "Queue eq 'Human Resources'", "sort_order": "desc", "position_index": 0, "explanation": "Find issues for HR, sorted by Create_Date descending (most recent first), take first (which is last chronologically)"}}
  
- "What was the first high priority incident?"
  -> {{"ordinal_type": "first", "search_query": "incidents", "odata_filter": "Priority eq 'high' and Type eq 'Incident'", "sort_order": "asc", "position_index": 0, "explanation": "Find high priority incidents, sorted by Create_Date ascending (oldest first), take first"}}

- "Show me the 3rd ticket for IT department"
  -> {{"ordinal_type": "3rd", "search_query": "tickets", "odata_filter": "Queue eq 'IT'", "sort_order": "desc", "position_index": 2, "explanation": "Find IT tickets sorted by Create_Date descending, take the 3rd one (index 2)"}}

- "What is the most recent Surface problem?"
  -> {{"ordinal_type": "most recent", "search_query": "Surface problem", "odata_filter": null, "sort_order": "desc", "position_index": 0, "explanation": "Find Surface problems, sorted by Create_Date descending (most recent first)"}}

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
            ordinal_type = parsed["ordinal_type"]
            search_query = parsed["search_query"]
            odata_filter = parsed.get("odata_filter")
            sort_order = parsed.get("sort_order", "desc")
            position_index = int(parsed.get("position_index", 0))
            explanation = parsed.get("explanation", "")
        except Exception as e:
            return (
                f"Question: {user_question}\n\n"
                f"Error: Unable to parse the ordinal question. Please rephrase your question.\n"
                f"Details: {str(e)}"
            )
        
        # Clean up OData filter
        if odata_filter and ("null" in str(odata_filter).lower() or not odata_filter.strip()):
            odata_filter = None
        
        # Execute search with filter
        search_results = search_service.search_tickets_with_filter(
            search_query, 
            odata_filter=odata_filter,
            top_k=20  # Get enough results to find the ordinal position
        )
        
        # Check if any results found
        if not search_results:
            filter_info = f" with filter: {odata_filter}" if odata_filter else ""
            return (
                f"Question: {user_question}\n\n"
                f"No tickets were found matching the search criteria: '{search_query}'{filter_info}"
            )
        
        # Handle position index
        if position_index == -1 or ordinal_type.lower() in ["last", "most recent", "latest", "newest"]:
            # For "last" with desc sort, we want the first item
            # For "last" with asc sort, we want the last item
            if sort_order == "desc":
                target_index = 0
            else:
                target_index = len(search_results) - 1
        else:
            target_index = position_index
        
        # Ensure the position exists
        if target_index >= len(search_results):
            return (
                f"Question: {user_question}\n\n"
                f"Only {len(search_results)} tickets were found, but position {target_index + 1} was requested.\n"
                f"Available positions: 1 to {len(search_results)}"
            )
        
        # Get the target ticket and some context
        target_ticket = search_results[target_index]
        all_results_json = json.dumps(search_results, indent=2, ensure_ascii=False)
        target_ticket_json = json.dumps(target_ticket, indent=2, ensure_ascii=False)
        
        filter_info = f"\nApplied Filter: {odata_filter}" if odata_filter else "\nNo filter applied (semantic search only)"
        
        analysis_prompt = f"""
Based on the following IT support tickets, answer the ordinal question.

Question: {user_question}

Search Logic: {explanation}
- Ordinal requested: {ordinal_type}
- Sort order: {sort_order}
- Position index: {target_index} (0-based)
- Total results found: {len(search_results)}{filter_info}

The ticket at the requested position:
{target_ticket_json}

All retrieved tickets for context:
{all_results_json}

Provide a detailed answer that:
1. Clearly identifies the ticket at the requested ordinal position
2. Provides comprehensive details about that specific ticket
3. Explains why this ticket is at that position (e.g., "This is the most recent because...")
4. Mentions any relevant context from other tickets if helpful

Format your response clearly with the ticket ID and all relevant details.
Base your answer strictly on the search results provided.
"""
        
        return analysis_prompt
    
    return ordinal_search


def create_ordinal_agent(
    chat_client: AzureOpenAIChatClient,
    search_service: SearchService
) -> ChatAgent:
    """
    Create the ordinal specialist agent.
    
    Args:
        chat_client: Azure OpenAI chat client
        search_service: Search service for ticket queries
        
    Returns:
        Configured ordinal ChatAgent with search capabilities
    """
    # Create the AI function with the search service
    ordinal_search_fn = create_ordinal_search_function(search_service)
    
    return chat_client.create_agent(
        instructions=ORDINAL_AGENT_INSTRUCTIONS,
        name="ordinal_agent",
        tools=[ordinal_search_fn],
    )
