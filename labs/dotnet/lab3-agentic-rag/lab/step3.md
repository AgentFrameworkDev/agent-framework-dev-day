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

This question is different than the normal semantic search because we want a numeric result with an optional explanation.

In our dataset some count questions could be:
- "How many consultants have reported access problems?"
- "How many HR team members reported file sync problems?"
- "How many tickets mention both 'HR system' and 'consultant'?"
- "How many third-party integrations have access problems?"
- "How many problems required system restart to fix?"

## Classifier Prompt to recognize a count question

1. In your VS Code, open the [ClassifierAgent.cs](./Agents/ClassifierAgent.cs) file, go to line 82 to find the count related portion.
```shell
   - When "and" combines database field values (Priority=high, Queue=HR, Type=Incident), these are FILTERS, not intersection
   - Keywords: "how many", "number of", "count of", "total", "how much"
   - Examples:
     - "How many tickets were logged for Human Resources?" ✓ COUNT_AGENT
     - "How many tickets were logged and Incidents for Human Resources and low priority?" ✓ COUNT_AGENT (Type=Incident AND Queue=HR AND Priority=low - all filters!)
     - "What is the total number of open tickets?" ✓ COUNT_AGENT
     - "Count of high priority incidents for IT?" ✓ COUNT_AGENT (Priority=high AND Type=Incident AND Queue=IT - all filters!) Human Resources and low priority?" ✓ COUNT_AGENT (and = filters)
     - "What is the total number of open tickets?" ✓ COUNT_AGENT
     - "Count of high priority incidents" ✓ COUNT_AGENT
```

This prompt has been modified a few times after some testing to get to the current state. When you ask questions that require a count, there is information in that question that can be used to filter the data in order to get to a count. This means the majority of the prompt is a few-shot prompt with examples the LLM can use to infer the question is a count question.

## Implement the Count Agent

Now we'll use the pattern we used for the YesNoAgent to implement a CountAgent.

1. In the agents folder, create a class named `CountAgent.cs`
2. Add the following using statements at the top and make the class static:

```c#
using System.Text.Json;
using Lab3.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace Lab3.Agents;

public static class CountAgent
```

3. Next, below the using statements add the following Instructions:
```c#
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

```c#
    private static Func<string, Task<string>> CreateSearchFunction(SearchService searchService)
    {
        return async (string userQuestion) =>
        {
            // Generate OData filter using LLM
            var filterPrompt = $"""
                You are an expert at converting natural language questions into OData filter expressions for Azure AI Search.

                Database Schema:
                - Type: string (values: "Incident", "Request", "Problem", "Change")
                - Queue: string (department name like "Human Resources", "IT", "Finance", etc.)
                - Priority: string (values: "high", "medium", "low")
                - Business_Type: string
                - Tags: collection of strings

                Question: {userQuestion}

                Generate ONLY the OData filter expression. Use proper OData syntax:
                - String equality: field eq 'value'
                - AND conditions: field1 eq 'value1' and field2 eq 'value2'

                Examples:
                - "Incidents for Human Resources with low priority" -> Type eq 'Incident' and Queue eq 'Human Resources' and Priority eq 'low'
                - "High priority tickets for IT" -> Priority eq 'high' and Queue eq 'IT'

                If no filters are needed, respond with: NO_FILTER

                OData filter:
                """;

            string? odataFilter = null;
            try
            {
                var filterResponse = await searchService.ChatClient.AsIChatClient().GetResponseAsync(filterPrompt);
                var filterText = filterResponse.Messages.FirstOrDefault()?.Text ?? "NO_FILTER";
                
                if (!filterText.Contains("NO_FILTER") && !string.IsNullOrWhiteSpace(filterText))
                {
                    odataFilter = filterText.Trim();
                }
            }
            catch
            {
                odataFilter = null;
            }

            // Execute search with filter
            var searchResults = await searchService.SearchTicketsWithFilterAsync(
                userQuestion,
                odataFilter,
                topK: 50
            );

            if (searchResults.Count == 0)
            {
                return $"""
                    Question: {userQuestion}

                    Count: 0

                    No tickets were found matching the specified criteria.
                    """;
            }

            var resultsJson = JsonSerializer.Serialize(searchResults, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var filterInfo = !string.IsNullOrEmpty(odataFilter)
                ? $"\nApplied Filter: {odataFilter}"
                : "\nNo filter applied (semantic search only)";

            return $"""
                Based on the following IT support tickets, answer the counting question.

                Question: {userQuestion}{filterInfo}

                Relevant Tickets Found (showing up to 50):
                {resultsJson}

                Format your response as:

                Count: [NUMBER]

                Breakdown:
                - [Description of what was counted]

                Examples (list a few matching tickets):
                - Ticket [ID]: [Brief description with relevant fields]

                Base your count strictly on tickets that match ALL criteria in the question.
                """;
        };
    }
```

This logic does a little more than the yes/no agent - mainly because we need to make an LLM call to get the filter to use. It performs these steps:
1. defines a prompt to use with an LLM call in order to have an OData filter generated from the user's question
2. make an LLM call to get an OData filter or no filter
3. use the filter (if there is one) and call the search index
4. if there aren't any search results, then the answer is 0
5. if there were answers, then create a prompt that includes the answers with question, so the LLM can determine from the agent result what to return to the user.

Next, you need a to creat an agent that will provide the creation of the agent.

5. At the end of the file, paste the following code:
```c#
public static AIAgent Create(ChatClient chatClient, SearchService searchService)
{
    var searchFunction = CreateSearchFunction(searchService);

    return chatClient.CreateAIAgent(
        instructions: Instructions,
        name: "count_agent",
        tools: new[] { AIFunctionFactory.Create(searchFunction) }
    );
}
```

As with the yes/no agent, you now need to add it to the [AgentFactory.cs](./Agents/AgentFactory.cs) and [Program.cs](Program.cs).

6. Open the [AgentFactory.cs](./Agents/AgentFactory.cs) file, modify the `CreateAllAgents()` method to look like the following:
```c#
public Dictionary<string, AIAgent> CreateAllAgents()
{
    return new Dictionary<string, AIAgent>
    {
        ["classifier"] = ClassifierAgent.Create(_chatClient),
        ["semantic_search"] = SemanticSearchAgent.Create(_chatClient, _searchService),
        ["yes_no"] = YesNoAgent.Create(_chatClient, _searchService),
        ["count"] = CountAgent.Create(_chatClient, _searchService)
        // TODO: Add more agents here as needed
    };
}

```

7. Next, open the [Program.cs](Program.cs) file and find the two places the workflow is defined and modify it to the following:
```c#
    var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(agents["classifier"])
        .WithHandoffs(agents["classifier"], [agents["semantic_search"], agents["yes_no"], agents["count"]])
        .WithHandoffs([agents["semantic_search"], agents["yes_no"], agents["count"]], agents["classifier"])
        .Build();
```

You can now run the console application in your debugger or on the command line:
```shell
dotnet run
```

Look through the results for Query 3/7, the result shoould look something like this:
```text
--- Query 3/7 ---
User: How many tickets were logged and Incidents for Human Resources and low priority?

classifier_agent_91ee9cd45faa404984ceb909c152bfb6

  [Calling function 'handoff_to_3' with arguments: {"reasonForHandoff":"User requested the count of tickets filtered by type \u0027Incident\u0027, queue \u0027Human Resources\u0027, and priority \u0027low\u0027. This is a counting query with database field filters."}]

count_agent_1de78c28d61d45978c475335aca5132d
Count: 3

Breakdown:
- 3 Incident tickets logged for Human Resources with low priority

Examples:
- Ticket INC001234: HR system login issue - Low priority
- Ticket INC001567: HR portal access problem - Low priority
- Ticket INC002345: HR database sync issue - Low priority

These tickets reflect common low-priority incidents affecting the Human Resources department. If you need more details or a breakdown by issue type, let me know!
```

We finally have an answer to that question the other approaches in the notebooks couldn't answer!

## [< Go to back to Step 2: Create the Yes/No Agent](step2.md) | [Go to Step 4: Create the Remaining Agents >](step4.md)