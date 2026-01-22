"""
Superlative agent for answering questions that require finding max/min values.
"""
import json
from typing import Annotated
from agent_framework import ChatAgent, ai_function
from agent_framework.azure import AzureOpenAIChatClient

from services import SearchService


SUPERLATIVE_AGENT_INSTRUCTIONS = """
You are a specialist in answering superlative (max/min) questions about IT support tickets.

## Database Schema
The database contains IT support tickets with these fields:
- Id: unique identifier
- Create_Date: ticket creation date
- Subject: ticket subject
- Body: ticket question/description
- Answer: ticket response/solution
- Type: ticket type (values: "Incident", "Request", "Problem", "Change")
- Queue: department name (values: "Human Resources", "IT", "Finance", "Operations", "Sales", "Marketing", "Engineering", "Support")
- Priority: priority level (values: "high", "medium", "low")
- Language: ticket language
- Business_Type: business category
- Tags: categorization tags

When you receive a question asking about maximum, minimum, most, least, highest, lowest, etc.:
1. Use the superlative_search function to retrieve and analyze relevant tickets
2. Group and count tickets by the relevant dimension
3. Identify the item(s) that satisfy the superlative condition
4. Provide a clear answer with supporting data

Example response format:

Based on the search results, the department with the most high priority incidents is:

**IT Department** - 15 high priority incidents

Breakdown by department:
1. IT: 15 incidents
2. Human Resources: 8 incidents
3. Finance: 5 incidents
4. Operations: 3 incidents

The IT department has nearly twice as many high priority incidents as the next highest department (Human Resources).

Be precise and base your answer strictly on the search results.
Provide counts and rankings to support your answer.
"""


def create_superlative_search_function(search_service: SearchService):
    """
    Factory function to create a superlative search AI function with the search service.
    
    Args:
        search_service: Initialized SearchService instance
        
    Returns:
        AI function for superlative searches
    """
    
    @ai_function
    async def superlative_search(
        user_question: Annotated[str, "User question requiring finding max/min values"]
    ) -> str:
        """
        Answers superlative questions by searching tickets and finding max/min values
        (most, least, highest, lowest, maximum, minimum).
        """
        # Parse the question to identify the superlative criterion
        parse_prompt = f"""
Analyze this question and extract the superlative information and search criteria:

Question: {user_question}

Extract:
1. SUPERLATIVE_TYPE: The superlative requested (most, least, highest, lowest, maximum, minimum, greatest, fewest, etc.)
2. SEARCH_QUERY: The main search criteria/topic (what type of tickets to search)
3. GROUP_BY_FIELD: The field to group by for comparison (Queue, Type, Priority, etc.)
4. ODATA_FILTER: OData filter expression for any additional constraints (optional)
5. AGGREGATION: What to aggregate (count, or a specific field)

Database Schema for OData filters:
- Id: unique identifier
- Create_Date: ticket creation date
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
    "superlative_type": "the superlative requested (e.g., 'most', 'least', 'highest')",
    "search_query": "the search topic/criteria",
    "group_by_field": "the field to group results by (e.g., 'Queue', 'Type', 'Priority')",
    "odata_filter": "OData filter expression or null if none needed",
    "aggregation": "count or specific field to aggregate",
    "explanation": "brief explanation of the logic"
}}

Examples:
- "Which department has the most high priority incidents?" 
  -> {{"superlative_type": "most", "search_query": "incidents", "group_by_field": "Queue", "odata_filter": "Priority eq 'high' and Type eq 'Incident'", "aggregation": "count", "explanation": "Count incidents grouped by Queue, filtered by high priority, find max count"}}
  
- "What ticket type has the fewest problems?"
  -> {{"superlative_type": "fewest", "search_query": "problems", "group_by_field": "Type", "odata_filter": null, "aggregation": "count", "explanation": "Count all tickets grouped by Type, find min count"}}

- "Which priority level has the most Surface issues?"
  -> {{"superlative_type": "most", "search_query": "Surface issues", "group_by_field": "Priority", "odata_filter": null, "aggregation": "count", "explanation": "Search for Surface issues, group by Priority, find max count"}}

- "What queue handles the least number of requests?"
  -> {{"superlative_type": "least", "search_query": "requests", "group_by_field": "Queue", "odata_filter": "Type eq 'Request'", "aggregation": "count", "explanation": "Count Request type tickets grouped by Queue, find min count"}}

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
            superlative_type = parsed["superlative_type"]
            search_query = parsed["search_query"]
            group_by_field = parsed["group_by_field"]
            odata_filter = parsed.get("odata_filter")
            aggregation = parsed.get("aggregation", "count")
            explanation = parsed.get("explanation", "")
        except Exception as e:
            return (
                f"Question: {user_question}\n\n"
                f"Error: Unable to parse the superlative question. Please rephrase your question.\n"
                f"Details: {str(e)}"
            )
        
        # Clean up OData filter
        if odata_filter and ("null" in str(odata_filter).lower() or not odata_filter.strip()):
            odata_filter = None
        
        # Execute search with filter - get more results for accurate aggregation
        search_results = search_service.search_tickets_with_filter(
            search_query, 
            odata_filter=odata_filter,
            top_k=50  # Get more results for better aggregation accuracy
        )
        
        # Check if any results found
        if not search_results:
            filter_info = f" with filter: {odata_filter}" if odata_filter else ""
            return (
                f"Question: {user_question}\n\n"
                f"No tickets were found matching the search criteria: '{search_query}'{filter_info}"
            )
        
        # Group results by the specified field
        groups = {}
        for ticket in search_results:
            field_value = ticket.get(group_by_field, "Unknown")
            if field_value not in groups:
                groups[field_value] = []
            groups[field_value].append(ticket)
        
        # Calculate counts for each group
        group_counts = {k: len(v) for k, v in groups.items()}
        
        # Sort by count
        is_max = superlative_type.lower() in ["most", "highest", "maximum", "greatest", "largest"]
        sorted_groups = sorted(group_counts.items(), key=lambda x: x[1], reverse=is_max)
        
        # Get the superlative result
        if sorted_groups:
            superlative_value, superlative_count = sorted_groups[0]
        else:
            return (
                f"Question: {user_question}\n\n"
                f"Unable to determine superlative - no groups found for field '{group_by_field}'"
            )
        
        # Get example tickets from the superlative group
        example_tickets = groups.get(superlative_value, [])[:5]
        examples_json = json.dumps(example_tickets, indent=2, ensure_ascii=False)
        
        # Build ranking breakdown
        ranking_breakdown = "\n".join([f"  {i+1}. {k}: {v} tickets" for i, (k, v) in enumerate(sorted_groups)])
        
        filter_info = f"\nApplied Filter: {odata_filter}" if odata_filter else "\nNo filter applied (semantic search only)"
        
        analysis_prompt = f"""
Based on the following IT support ticket analysis, answer the superlative question.

Question: {user_question}

Search Logic: {explanation}
- Superlative requested: {superlative_type}
- Grouped by: {group_by_field}
- Total tickets analyzed: {len(search_results)}{filter_info}

**Result**: The {group_by_field} with the {superlative_type} is: **{superlative_value}** with {superlative_count} tickets

Ranking by {group_by_field}:
{ranking_breakdown}

Example tickets from {superlative_value}:
{examples_json}

Provide a detailed answer that:
1. Clearly states which {group_by_field} has the {superlative_type} (the answer is {superlative_value})
2. Provides the count ({superlative_count})
3. Shows the ranking breakdown for context
4. Mentions a few example tickets from the winning group

Format your response clearly with the superlative result highlighted.
Base your answer strictly on the data provided.
"""
        
        return analysis_prompt
    
    return superlative_search


def create_superlative_agent(
    chat_client: AzureOpenAIChatClient,
    search_service: SearchService
) -> ChatAgent:
    """
    Create the superlative specialist agent.
    
    Args:
        chat_client: Azure OpenAI chat client
        search_service: Search service for ticket queries
        
    Returns:
        Configured superlative ChatAgent with search capabilities
    """
    # Create the AI function with the search service
    superlative_search_fn = create_superlative_search_function(search_service)
    
    return chat_client.create_agent(
        instructions=SUPERLATIVE_AGENT_INSTRUCTIONS,
        name="superlative_agent",
        tools=[superlative_search_fn],
    )
