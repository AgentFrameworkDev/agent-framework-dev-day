"""
Yes/No agent for answering binary questions about IT support tickets.
"""
import json
from typing import Annotated
from agent_framework import ChatAgent, ai_function
from agent_framework.azure import AzureOpenAIChatClient

from services import SearchService


YES_NO_AGENT_INSTRUCTIONS = """
You are a specialist in answering yes/no questions about IT support tickets.

When you receive a question:
1. Use the yes_or_no_search function to retrieve relevant tickets
2. Analyze the search results carefully
3. Provide a clear YES or NO answer
4. Give a brief explanation (1-2 sentences)
5. Cite specific examples from the tickets using their IDs

Example response format:

Answer: YES

Explanation: There are multiple tickets documenting issues with Dell XPS laptops 
in the database, primarily related to display and battery problems.

Examples:
- Ticket INC001234: User reported Dell XPS 15 screen flickering issue
- Ticket INC001567: Dell XPS 13 battery not charging properly

Be precise and base your answer strictly on the evidence from the search results.
If no relevant tickets are found, answer NO with an appropriate explanation.
"""


def create_yes_no_search_function(search_service: SearchService):
    """
    Factory function to create a yes/no search AI function with the search service.
    
    Args:
        search_service: Initialized SearchService instance
        
    Returns:
        AI function for yes/no searches
    """
    
    @ai_function
    def yes_or_no_search(
        user_question: Annotated[str, "User question to answer with yes or no"]
    ) -> str:
        """
        Answers yes or no questions by searching the IT support ticket database.
        Provides explanation and relevant examples from search results.
        """
        # Execute search with more results for better coverage
        search_results = search_service.search_tickets(user_question, top_k=5)
        
        # Check if any results found
        if not search_results:
            return (
                f"Question: {user_question}\n\n"
                "Answer: NO\n\n"
                "Explanation: There are no relevant tickets in the database matching this query."
            )
        
        # Build analysis prompt with search results
        results_json = json.dumps(search_results, indent=2, ensure_ascii=False)
        
        analysis_prompt = f"""
Based on the following IT support tickets, answer the question with YES or NO, 
then provide a clear explanation and cite specific examples from the tickets.

Question: {user_question}

Relevant Tickets:
{results_json}

Format your response as:

Answer: [YES or NO]

Explanation: [Brief explanation of why, 1-2 sentences]

Examples:
- Ticket [ID]: [Relevant detail from the ticket]
- Ticket [ID]: [Another relevant detail]

If the tickets show mixed evidence, clarify in your explanation.
Base your answer strictly on the evidence from the search results provided.
"""
        
        return analysis_prompt
    
    return yes_or_no_search


def create_yes_no_agent(
    chat_client: AzureOpenAIChatClient,
    search_service: SearchService
) -> ChatAgent:
    """
    Create the yes/no specialist agent.
    
    Args:
        chat_client: Azure OpenAI chat client
        search_service: Search service for ticket queries
        
    Returns:
        Configured yes/no ChatAgent with search capabilities
    """
    # Create the AI function with the search service
    yes_no_search_fn = create_yes_no_search_function(search_service)
    
    return chat_client.create_agent(
        instructions=YES_NO_AGENT_INSTRUCTIONS,
        name="yes_no_agent",
        tools=[yes_no_search_fn],
    )
