# Step 2: Create the Yes/No Agent

## Learning Objectives
1. Learn about the yes/no question type
2. Learn a prompt that will classify a question as a yes/no question
3. Implement the agent to answer a yes/no question

## Prerequisites
1. Must complete [step 1](step1.md) first

## Question Type Details

Before we can implement an agent to specifically answer the Yes/No question, let's understand it a little more and see some examples.

In the [Mintaka: A Complex, Natural, and Multilingual Dataset for End-to-End Question Answering](https://aclanthology.org/2022.coling-1.138.pdf) whitepaper:
> YES/NO: questions whwere the answer is Yes or No. For example, Q: Haas Lady Gaga ever made a song with Ariana Grande? A: Yes

This question is different than the normal semantic search mainly it the fact that we do want either "yes" or "no" with some optional explaination.

In our dataset some yes/no questions could be:
- "Are there any synchronization problems with Google Drive?"
- "Are external users unable to access the HR portal?"
- "Have file synchronization services been failing?"
- "Is this a recurring problem with consultant access?"
- "Are firewall rules blocking external consultant access?"

## Classifier Prompt to recognize a yes/no question

1. In your VS Code, open the [classifier_agent.py](./agents/classifier_agent.py) file.

At the top of this file, you can see the prompt that will categorize (ie. classify) the user's question into the different question types we are interested in for this exercise.

2. At the top of the file, you can see there is a brief description of what the agent does and some detail about the search index:
```shell
You are a query classification system for an IT support ticket database. 
Your task is to route questions to specialist search agents based on the user question.

## Database Schema
The database contains IT support tickets with these fields:
- Id: unique identifier
- Subject: ticket subject
- Body: ticket question/description
- Answer: ticket response/solution
- Type: ticket type (values: "Incident", "Request", "Problem", "Change")
- Queue: department name (values: "Human Resources", "IT", "Finance", "Operations", "Sales", "Marketing", "Engineering", "Support")
- Priority: priority level (values: "high", "medium", "low")
- Language: ticket language
- Business_Type: business category
- Tags: categorization tags

**IMPORTANT**: When "and" combines field values (Type, Queue, Priority), these are FILTERS for counting/searching, NOT separate items to compare.

```

3. Go to line 70, to see the specific portion of the prompt for the yes/no questions:
```shell
**YES_NO_AGENT**: Simple yes/no questions (expect "yes" or "no" as answer)
   - Keywords: "is", "are", "can", "does", "do", "will", "should", "any" (WITHOUT negation)
   - Examples:
     - "Are there any issues for Dell XPS laptops?"
     - "Is my account locked?"
     - "Can I access the VPN?"
     - "Does the printer support color printing?"
     - "Do we have problems with Surface devices?"

```

This all means, you won't need to modify the classifier for the yes/no questions to classified correctly - you just need to implement the agent and wire it up so it can be routed to. Let's do that now.

## Implement the Yes/No Agent

The agents have a common layout to make it easier to understand their contents, there are three parts to each agent:
- The agent instructions
- The function that will be used for a tool (this is where all the work is done)
- The creation method

1. In the agents folder, create a file named `yes_no_agent.py`

2. Add the import statements we'll need at the top:

```python
import json
from typing import Annotated
from agent_framework import ChatAgent, ai_function
from agent_framework.azure import AzureOpenAIChatClient

from services import SearchService
```

3. Next, below the imports add the Instructions:
```python
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
```

As you can see, the instruction indicate we want a clear yes or no as part of the answer.

4. Now add the following code for the logic to perform the lookup and answering of the question:
```python
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

```

This logic follows these steps:
1. perform a search (currently is a semantic search)
2. if there were no results, then the answer is no
3. if there were answers, then create a prompt that includes those answers with the quesiton (ie. RAG), so the LLM can determine from the agent result if it is a yes or no

Next, you need a method to create an agent that will use the instruction and the above function.

5. Towards the bottom of the file, paste the following code:
```python
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

```

Now that we have the agent, we need to do two other things:
- Add code to the [agent_factory.py](./agents/agent_factory.py) to be able to create the agent
- Add the agent to the workflows in the [main.py](main.py) - NOTE: there are two of them (one for demo/debugging and the other for interactive mode)

6. Open the [agent_factory.py](./agents/agent_factory.py) file, add the import for the agent toward the top where the semantic_agent is:
```python
from agents import (
    classifier_agent,
    semantic_search_agent,
    yes_no_agent
)
```

7. Then modify the `create_all_agents()` method to look like the following:
```python
    def create_all_agents(self) -> dict[str, ChatAgent]:
        """
        Create all agents needed for the system.
        
        Returns:
            Dictionary mapping agent names to ChatAgent instances
        """
        return {
            "classifier": classifier_agent.create_classifier_agent(self.chat_client),
            "semantic_search": semantic_search_agent.create_semantic_search_agent(self.chat_client, self.search_service),
             "yes_no": yes_no_agent.create_yes_no_agent(self.chat_client, self.search_service),
            # TODO: Add more agents here as needed
        }
```

8. Next, open the [main.py](main.py) file and find the two places the workflow is defined and modify it to the following:
```python
    workflow = (
        HandoffBuilder(
            name="agentic_rag_workflow",
            participants=[agents["classifier"], agents["semantic_search"], agents["yes_no"]],
        )
        .set_coordinator(agents["classifier"])
        .build()
    )
```

You can now run the main.py in your debugger or on the command line:
```shell
python main.py
```

Look through the results for Query 2/7, the result shoould look something like this:
```text
--- Query 2/7 ---

User: Are there any issues for Dell XPS laptops?

  yes_no_agent: Answer: YES

Explanation: There are multiple tickets documenting issues with Dell XPS laptops, specifically the XPS 13 9310 model. Common problems include performance slowdowns, overheating, intermittent freezing, and sluggishness when handling intensive tasks.

Examples:
- Ticket b4232048-5751-412e-8c54-a60a367c1299: Reports intermittent freezing and sluggish performance on Dell XPS 13.
- Ticket 571c4802-df0b-4042-a87e-e92dfa86c9bd: User experienced significant overheating issues affecting performance.
- Ticket 2d8e256d-a4df-486a-b5d6-ff944dbe9f6e: Severe slowdowns and occasional freezing that hinder productivity.
- Ticket 6bf0c065-3383-48fa-bfed-0c2610c82b4b: Persistent high-performance issues requiring urgent support.

```

## [< Go to back to Step 1: Get Started](step1.md) | [Go to Step 3: Create the Count Search Agent >](step3.md)