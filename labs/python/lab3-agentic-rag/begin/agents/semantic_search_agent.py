"""
Semantic Search agent for answering questions using semantic similarity search.
"""
import json
from typing import Annotated
from agent_framework import ChatAgent, ai_function
from agent_framework.azure import AzureOpenAIChatClient

from services import SearchService


SEMANTIC_SEARCH_AGENT_INSTRUCTIONS = """
You are a specialist in answering questions using semantic search on IT support tickets.

When you receive a question:
1. Use the semantic_search function to retrieve relevant tickets
2. Analyze the search results carefully
3. Provide a clear, concise answer based on the tickets
4. Cite specific examples from the tickets using their IDs
5. If the tickets contain solutions, summarize them

Example response format:

Based on the support tickets, here are the issues reported with Surface devices:

1. **Display Issues**: Multiple tickets report screen flickering and display problems
   - Ticket INC001234: Surface Pro 7 screen flickering intermittently
   - Ticket INC001567: Surface Laptop display goes black randomly

2. **Battery Problems**: Several users experiencing battery drain
   - Ticket INC002345: Surface Book battery draining quickly even when idle

**Common Solutions**:
- Update display drivers to latest version
- Check for Windows updates
- Reset battery calibration settings

Be thorough and cite all relevant tickets. Group similar issues together when appropriate.
If no relevant tickets are found, clearly state that no matching information was found.
"""


def create_semantic_search_function(search_service: SearchService):
    """
    Factory function to create a semantic search AI function with the search service.
    
    Args:
        search_service: Initialized SearchService instance
        
    Returns:
        AI function for semantic searches
    """
    
    @ai_function
    def semantic_search(
        user_question: Annotated[str, "User question requiring semantic search"]
    ) -> str:
        """
        Answers questions by performing semantic search on the IT support ticket database.
        Returns relevant tickets and their solutions.
        """
        # Execute search with more results for better coverage
        search_results = search_service.search_tickets(user_question, top_k=10)
        
        # Check if any results found
        if not search_results:
            return (
                f"Question: {user_question}\n\n"
                "No relevant tickets were found in the database matching this query."
            )
        
        # Build context prompt with search results
        results_json = json.dumps(search_results, indent=2, ensure_ascii=False)
        
        context_prompt = f"""
Based on the following IT support tickets, answer the user's question comprehensively.

Question: {user_question}

Relevant Tickets:
{results_json}

Provide a detailed answer that:
1. Summarizes the key information from the tickets
2. Groups similar issues or solutions together
3. Cites specific ticket IDs as examples
4. Includes any solutions or resolutions mentioned in the tickets

Format your response in a clear, structured way with bullet points or sections as appropriate.
Base your answer strictly on the evidence from the search results provided.
"""
        
        return context_prompt
    
    return semantic_search


def create_semantic_search_agent(
    chat_client: AzureOpenAIChatClient,
    search_service: SearchService
) -> ChatAgent:
    """
    Create the semantic search specialist agent.
    
    Args:
        chat_client: Azure OpenAI chat client
        search_service: Search service for ticket queries
        
    Returns:
        Configured semantic search ChatAgent with search capabilities
    """
    # Create the AI function with the search service
    semantic_search_fn = create_semantic_search_function(search_service)
    
    return chat_client.create_agent(
        instructions=SEMANTIC_SEARCH_AGENT_INSTRUCTIONS,
        name="semantic_search_agent",
        tools=[semantic_search_fn],
    )
