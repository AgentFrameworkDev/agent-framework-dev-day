"""
Comparative agent for answering questions that compare multiple items.
"""
import json
from typing import Annotated
from agent_framework import ChatAgent, ai_function
from agent_framework.azure import AzureOpenAIChatClient

from services import SearchService


COMPARATIVE_AGENT_INSTRUCTIONS = """
You are a specialist in answering comparative questions about IT support tickets.

When you receive a question comparing multiple items (A vs B):
1. Use the comparative_search function to retrieve and compare relevant tickets
2. Analyze the counts and details for each item being compared
3. Provide a clear comparison with specific numbers
4. Cite specific examples from the tickets using their IDs
5. Declare which item has more issues/occurrences

Example response format:

**Comparison: MacBook Air vs Dell XPS**

**MacBook Air Issues: 15 tickets**
- Battery drain problems (5 tickets)
- Display flickering (4 tickets)
- Keyboard issues (3 tickets)
- Other issues (3 tickets)

Examples:
- Ticket INC001234: MacBook Air battery draining rapidly
- Ticket INC001567: MacBook Air screen flickering issue

**Dell XPS Issues: 23 tickets**
- Touchpad not responding (8 tickets)
- Battery swelling (6 tickets)
- Display problems (5 tickets)
- Other issues (4 tickets)

Examples:
- Ticket INC002345: Dell XPS touchpad frozen
- Ticket INC002890: Dell XPS 15 battery swelling

**Conclusion**: Dell XPS laptops have more reported issues (23 tickets) compared to MacBook Air computers (15 tickets), with a difference of 8 additional tickets.

Be precise with counts and base your answer strictly on the search results.
Always clearly state which option has more issues.
"""


def create_comparative_search_function(search_service: SearchService):
    """
    Factory function to create a comparative search AI function with the search service.
    
    Args:
        search_service: Initialized SearchService instance
        
    Returns:
        AI function for comparative searches
    """
    
    @ai_function
    async def comparative_search(
        user_question: Annotated[str, "User question requiring comparison between multiple items"]
    ) -> str:
        """
        Answers comparative questions by searching for multiple items and comparing their counts.
        Useful for questions like "Do we have more issues with X or Y?"
        """
        # Parse the question to identify the items being compared
        parse_prompt = f"""
Analyze this comparison question and extract the items being compared:

Question: {user_question}

Extract all items being compared. Format your response as JSON:
{{
    "item_1": "first item to compare",
    "item_2": "second item to compare",
    "additional_items": ["any other items if present"],
    "comparison_type": "what is being compared (e.g., issue count, ticket count, occurrences)",
    "explanation": "brief explanation of the comparison"
}}

Examples:
- "Do we have more issues with MacBook Air computers or Dell XPS laptops?" 
  -> {{"item_1": "MacBook Air", "item_2": "Dell XPS", "additional_items": [], "comparison_type": "issue count", "explanation": "Compare number of issues for MacBook Air vs Dell XPS"}}
  
- "Which has more tickets: Surface Pro or iPad?"
  -> {{"item_1": "Surface Pro", "item_2": "iPad", "additional_items": [], "comparison_type": "ticket count", "explanation": "Compare ticket counts for Surface Pro vs iPad"}}

- "Are there more high priority incidents for HR, IT, or Finance?"
  -> {{"item_1": "HR", "item_2": "IT", "additional_items": ["Finance"], "comparison_type": "high priority incidents", "explanation": "Compare high priority incident counts across departments"}}

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
            item_1 = parsed["item_1"]
            item_2 = parsed["item_2"]
            additional_items = parsed.get("additional_items", [])
            comparison_type = parsed.get("comparison_type", "count")
            explanation = parsed.get("explanation", "")
            
            # Combine all items to compare
            all_items = [item_1, item_2] + additional_items
        except Exception as e:
            return (
                f"Question: {user_question}\n\n"
                f"Error: Unable to parse the comparison question. Please rephrase your question.\n"
                f"Details: {str(e)}"
            )
        
        # Perform separate searches for each item
        comparison_results = {}
        
        for item in all_items:
            search_results = search_service.search_tickets(item, top_k=100, include_semantic_search=False)
            comparison_results[item] = {
                "count": len(search_results),
                "tickets": search_results[:10]  # Keep top 10 for examples
            }
        
        # Check if any results found
        total_results = sum(result["count"] for result in comparison_results.values())
        if total_results == 0:
            return (
                f"Question: {user_question}\n\n"
                f"No tickets were found for any of the items being compared.\n"
                f"Items searched: {', '.join(all_items)}"
            )
        
        # Build comparison summary
        comparison_summary = []
        for item, data in comparison_results.items():
            comparison_summary.append(f"- {item}: {data['count']} tickets")
        
        comparison_summary_text = "\n".join(comparison_summary)
        
        # Build analysis prompt with comparison results
        results_json = json.dumps(comparison_results, indent=2, ensure_ascii=False)
        
        analysis_prompt = f"""
Based on the following IT support tickets, answer the comparison question.

Question: {user_question}

Comparison Logic: {explanation}
Type: {comparison_type}

Search Results Summary:
{comparison_summary_text}

Detailed Ticket Data:
{results_json}

Provide a detailed comparative analysis that:
1. Shows the count for each item being compared
2. Provides breakdown of issue types for each item if relevant
3. Cites specific ticket IDs as examples (2-3 per item)
4. Clearly states which item has MORE issues/tickets
5. Includes the difference in counts

Format your response with:
- Clear headings for each item
- Counts prominently displayed
- Specific ticket examples
- A conclusion stating which has more and by how much

Base your answer strictly on the search results provided.
"""
        
        return analysis_prompt
    
    return comparative_search


def create_comparative_agent(
    chat_client: AzureOpenAIChatClient,
    search_service: SearchService
) -> ChatAgent:
    """
    Create the comparative specialist agent.
    
    Args:
        chat_client: Azure OpenAI chat client
        search_service: Search service for ticket queries
        
    Returns:
        Configured comparative ChatAgent with search capabilities
    """
    # Create the AI function with the search service
    comparative_search_fn = create_comparative_search_function(search_service)
    
    return chat_client.create_agent(
        instructions=COMPARATIVE_AGENT_INSTRUCTIONS,
        name="comparative_agent",
        tools=[comparative_search_fn],
    )
