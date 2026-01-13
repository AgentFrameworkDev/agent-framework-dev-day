using System.Text.Json;
using Lab3.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace Lab3.Agents;

/// <summary>
/// Yes/No agent for answering binary questions about IT support tickets.
/// </summary>
public static class YesNoAgent
{
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

    public static AIAgent Create(ChatClient chatClient, SearchService searchService)
    {
        var searchFunction = CreateSearchFunction(searchService);

        return chatClient.CreateAIAgent(
            instructions: Instructions,
            name: "yes_no_agent",
            tools: new[] { AIFunctionFactory.Create(searchFunction) }
        );
    }
}
