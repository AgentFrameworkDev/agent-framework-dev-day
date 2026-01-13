# Step 3: Create the Count Search Agent

## Learning Objectives
1. Learn about the count question type
2. Learn a prompt that will classify a question as a counting question
3. Implement the agent to answer a count question

## Prerequisites
1. Must complete [step 2](step2.md) first

## Question Type Details

In the [Mintaka: A Complex, Natural, and Multilingual Dataset for End-to-End Question Answering](https://aclanthology.org/2022.coling-1.138.pdf) whitepaper:

> COUNT: questions where the answer requires counting. For example, Q: How many astronauts have been elected to Congress? A: 4

This question is different than the normal semantic search because we want a numeric result with an optional explaination.

In our dataset some count questions could be:
- "How many consultants have reported access problems?"
- "How many HR team members reported file sync problems?"
- "How many tickets mention both 'HR system' and 'consultant'?"
- "How many third-party integrations have access problems?"
- "How many problems required system restart to fix?"

## Classifier Prompt to recognize a count question

1. In your VS Code, open the [classifier_agent.py](./agents/classifier_agent.py) file, go to line 71 to find the count related portion.
```shell
   - When "and" combines database field values (Priority=high, Queue=HR, Type=Incident), these are FILTERS, not intersection
   - Keywords: "how many", "number of", "count of", "total", "how much"
   - Examples:
     - "How many tickets were logged for Human Resources?" ✓ COUNT_AGENT
     - "How many tickets were logged and Incidents for Human Resources and low priority?" ✓ COUNT_AGENT (Type=Incident AND Queue=HR AND Priority=low - all filters!)
     - "What is the total number of open tickets?" ✓ COUNT_AGENT
     - "Count of high priority incidents for IT?" ✓ COUNT_AGENT (Priority=high AND Type=Incident AND Queue=IT - all filters!)Human Resources and low priority?" ✓ COUNT_AGENT (and = filters)
     - "What is the total number of open tickets?" ✓ COUNT_AGENT
     - "Count of high priority incidents" ✓ COUNT_AGENT
```

This prompt has been modified a few times after some testing to get to the current state. When you ask questions that require a count, there is information in that question that can be used to filter the data in order to get to a count. This means the majority of the prompt is a few-shot prompt with examples the LLM can use to infer the question is a count question.

## Implement the Count Agent

Now we'll use the pattern we used for the yes_no_agent to implement a count_agent.

1. In the agents folder, create a file named `count_agent.py`
2. Add the following import statements at the top:

```python
import json
from typing import Annotated
from agent_framework import ChatAgent, ai_function
from agent_framework.azure import AzureOpenAIChatClient

from services import SearchService
```

3. Next, below the imports add the following Instructions:
```python
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
```

As you can see, these instructions tell the LLM that we want a clear count as a number as part of the answer.

4. Next add the following code for the logic to perform the logic with the search index:

```python
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
```

This logic does a little more than the yes/no agent - mainly because we need to make an LLM call to get the filter to use. It performs these steps:
1. defines a prompt to use with an LLM call in order to have an OData filter generated from the user's question
2. make an LLM call to get an OData filter or no filter
3. use the filter (if there is one) and call the search index
4. if there aren't any search results, then the answer is 0
5. if there were answers, then create a prompt that includes the answers with question and the count, so the LLM can determine from the agent result what to return to the user.

Next, you need a to creat an agent that will provide the creation of the agent.

5. At the end of the file, paste the following code:
```python
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

```

As with the yes/no agent, you now need to add it to the [agent_factory.py](./agents/agent_factory.py) and [main.py](main.py).

6. Open the [agent_factory.py](./agents/agent_factory.py) file, add the import for the agent toward the top where the semantic_agent is:
```python
from agents import (
    classifier_agent,
    semantic_search_agent,
    yes_no_agent,
    count_agent
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
             "count": count_agent.create_count_agent(self.chat_client, self.search_service),
            # TODO: Add more agents here as needed
        }
```
8. Next, open the [main.py](main.py) file and find the two places the workflow is defined and modify it to the following:
```python
    workflow = (
        HandoffBuilder(
            name="agentic_rag_workflow",
            participants=[agents["classifier"], agents["semantic_search"], agents["yes_no"], agents["count"]],
        )
        .set_coordinator(agents["classifier"])
        .build()
    )
```

You can now run the main.py in your debugger or on the command line:
```shell
python main.py
```

Look through the results for Query 3/7, the result shoould look something like this:
```text
--- Query 3/7 ---
User: How many tickets were logged and Incidents for Human Resources and low priority?

  count_agent: Count: 3

Breakdown:
- 3 Incident tickets logged for Human Resources with low priority

Examples:
- Ticket 1d7f7c8d-9468-4cfe-b244-b326d584c53c: Consultant Login Difficulties with HR System - Low priority
- Ticket 6d3d077c-8e54-48ee-81c6-68951ef6f384: Challenges Accessing the HR System for Consultants - Low priority
- Ticket 1849d3bc-fee0-441c-aff1-679a2eaff048: Google Drive Access Issue for HR team - Low priority

Note: This count is based on available search results. The actual total may be higher if more tickets exist in the database.
```

We finally have an answer to that question the other approaches in the notebooks couldn't answer!

## [< Go to back to Step 2: Create the Yes/No Agent](step2.md) | [Go to Step 4: Create the Remaining Agents >](step4.md)