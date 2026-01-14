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

1. In your VS Code, open the [ClassifierAgent.cs](./Agents/ClassifierAgent.cs) file.

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

3. Go to line 73, to see the specific portion of the prompt for the yes/no questions:
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

1. In the Agents folder, create a class named `YesNoAgent.cs`

2. Add the using statements we'll need at the top and make sure the class is a **static** class:

```c#
using System.Text.Json;
using Lab3.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace Lab3.Agents;

public static class YesNoAgent
```

3. Next, in the YesNoAgent class add the Instructions:
```c#
    private const string Instructions = """
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
        """;
```

As you can see, the instruction indicate we want a clear yes or no as part of the answer.

4. Now add the following code for the logic to perform the lookup and answering of the question:
```c#
private static Func<string, Task<string>> CreateSearchFunction(SearchService searchService)
{
    return async (string userQuestion) =>
    {
        var searchResults = await searchService.SearchTicketsAsync(userQuestion, topK: 5);

        if (searchResults.Count == 0)
        {
            return $"""
                Question: {userQuestion}

                Answer: NO

                Explanation: There are no relevant tickets in the database matching this query.
                """;
        }

        var resultsJson = JsonSerializer.Serialize(searchResults, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return $"""
            Based on the following IT support tickets, answer the question with YES or NO, 
            then provide a clear explanation and cite specific examples from the tickets.

            Question: {userQuestion}

            Relevant Tickets:
            {resultsJson}

            Format your response as:

            Answer: [YES or NO]

            Explanation: [Brief explanation of why, 1-2 sentences]

            Examples:
            - Ticket [ID]: [Relevant detail from the ticket]
            - Ticket [ID]: [Another relevant detail]

            Base your answer strictly on the evidence from the search results provided.
            """;
    };
}
```

This logic follows these steps:
1. perform a search (currently is a semantic search with topK=5)
2. if there are no results, then the answer is no
3. if there are answers, then create a prompt that includes those answers with the quesiton (ie. RAG), so the LLM can determine from the agent result if it is a yes or no

Next, you need a method to create an agent that will use the instruction and the above function.

5. Towards the bottom of the file, paste the following code:
```c#
public static AIAgent Create(ChatClient chatClient, SearchService searchService)
{
    var searchFunction = CreateSearchFunction(searchService);

    return chatClient.CreateAIAgent(
        instructions: Instructions,
        name: "yes_no_agent",
        tools: new[] { AIFunctionFactory.Create(searchFunction) }
    );
}
```

Now that we have the agent, we need to do two other things:
- Add code to the [AgentFactory.cs](./Agents/AgentFactory.cs) to be able to create the agent
- Add the agent to the workflows in the [Program.cs](Program.cs) - NOTE: there are two of them (one for demo/debugging and the other for interactive mode)

6. Open the [AgentFactory.cs](./Agents/AgentFactory.cs) file, modify the `CreateAllAgents()` method to look like the following:
```c#
public Dictionary<string, AIAgent> CreateAllAgents()
{
    return new Dictionary<string, AIAgent>
    {
        ["classifier"] = ClassifierAgent.Create(_chatClient),
        ["semantic_search"] = SemanticSearchAgent.Create(_chatClient, _searchService),
        ["yes_no"] = YesNoAgent.Create(_chatClient, _searchService),
        // TODO: Add more agents here as needed
    };
}
```

7. Next, open the [Program.cs](Program.cs) file and find the two places the workflow is defined and modify it to the following:
```c#
    var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(agents["classifier"])
        .WithHandoffs(agents["classifier"], [agents["semantic_search"], agents["yes_no"]])
        .WithHandoffs([agents["semantic_search"], agents["yes_no"]], agents["classifier"])
        .Build();
```

You can now run the console application in your debugger or on the command line:
```shell
dotnet run
```

Look through the results for Query 2/7, the result shoould look something like this:
```text
--- Query 2/7 ---
User: Are there any issues for Dell XPS laptops?

classifier_agent_cf17ccafac96419ca5095ae04143c84f

  [Calling function 'handoff_to_1' with arguments: {"reasonForHandoff":"User asked a simple yes/no question about the existence of issues for Dell XPS laptops."}]

yes_no_agent_0f0d3bfdac4942ecae526eaea6236921
Answer: YES

Explanation: There are documented support tickets indicating issues with Dell XPS laptops, including problems related to screen flickering and battery charging.

Examples:
- Ticket INC001234: User reported Dell XPS 15 screen flickering issue.
- Ticket INC001567: Dell XPS 13 battery not charging properly.

```

## [< Go to back to Step 1: Get Started](step1.md) | [Go to Step 3: Create the Count Search Agent >](step3.md)