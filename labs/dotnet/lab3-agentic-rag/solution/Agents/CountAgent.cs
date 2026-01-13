using System.Text.Json;
using Lab3.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace Lab3.Agents;

/// <summary>
/// Count agent for answering questions that require counting tickets with filters.
/// </summary>
public static class CountAgent
{
    private const string Instructions = """
        You are a specialist in answering counting questions about IT support tickets.

        When you receive a question:
        1. Use the count_search function to retrieve and count relevant tickets
        2. Analyze the search results to get an accurate count
        3. Provide a clear COUNT number
        4. Give a brief breakdown or explanation
        5. Cite specific examples from the tickets using their IDs

        Be precise with the count and base your answer strictly on the search results.
        """;

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

    public static AIAgent Create(ChatClient chatClient, SearchService searchService)
    {
        var searchFunction = CreateSearchFunction(searchService);

        return chatClient.CreateAIAgent(
            instructions: Instructions,
            name: "count_agent",
            tools: new[] { AIFunctionFactory.Create(searchFunction) }
        );
    }
}
