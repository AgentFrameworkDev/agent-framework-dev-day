using System.Text.Json;
using Lab3Solution.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace Lab3Solution.Agents;

/// <summary>
/// Semantic Search agent for answering questions using semantic similarity search.
/// </summary>
public static class SemanticSearchAgent
{
    private const string Instructions = """
        You are a specialist in answering questions using semantic search on IT support tickets.

        When you receive a question:
        1. Use the semantic_search function to retrieve relevant tickets
        2. Analyze the search results carefully
        3. Provide a clear, concise answer based on the tickets
        4. Cite specific examples from the tickets using their IDs
        5. If the tickets contain solutions, summarize them

        Be thorough and cite all relevant tickets. Group similar issues together when appropriate.
        """;

    private static Func<string, Task<string>> CreateSearchFunction(SearchService searchService)
    {
        return async (string userQuestion) =>
        {
            var searchResults = await searchService.SearchTicketsAsync(userQuestion, topK: 10);

            if (searchResults.Count == 0)
            {
                return $"Question: {userQuestion}\n\nNo relevant tickets were found in the database matching this query.";
            }

            var resultsJson = JsonSerializer.Serialize(searchResults, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return $"""
                Based on the following IT support tickets, answer the user's question comprehensively.

                Question: {userQuestion}

                Relevant Tickets:
                {resultsJson}

                Provide a detailed answer that:
                1. Summarizes the key information from the tickets
                2. Groups similar issues or solutions together
                3. Cites specific ticket IDs as examples
                4. Includes any solutions or resolutions mentioned in the tickets

                Format your response in a clear, structured way with bullet points or sections as appropriate.
                Base your answer strictly on the evidence from the search results provided.
                """;
        };
    }

    public static AIAgent Create(ChatClient chatClient, SearchService searchService)
    {
        var searchFunction = CreateSearchFunction(searchService);

        return chatClient.CreateAIAgent(
            instructions: Instructions,
            name: "semantic_search_agent",
            tools: new[] { AIFunctionFactory.Create(searchFunction) }
        );
    }
}
