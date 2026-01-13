using System.Text.Json;
using System.Text.Json.Serialization;
using Lab3.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace Lab3.Agents;

/// <summary>
/// Comparative agent for answering questions that compare multiple items.
/// </summary>
public static class ComparativeAgent
{
    private const string Instructions = """
        You are a specialist in answering comparative questions about IT support tickets.

        When you receive a question comparing multiple items (A vs B):
        1. Use the comparative_search function to retrieve and compare relevant tickets
        2. Analyze the counts and details for each item being compared
        3. Provide a clear comparison with specific numbers
        4. Cite specific examples from the tickets using their IDs
        5. Declare which item has more issues/occurrences

        Be precise with counts and base your answer strictly on the search results.
        """;

    private static Func<string, Task<string>> CreateSearchFunction(SearchService searchService)
    {
        return async (string userQuestion) =>
        {
            var parsePrompt = $@"
                Analyze this comparison question and extract the items being compared:

                Question: {userQuestion}

                Extract all items being compared. Format your response as JSON:
                {{
                    ""item_1"": ""first item to compare"",
                    ""item_2"": ""second item to compare"",
                    ""additional_items"": [""any other items if present""],
                    ""comparison_type"": ""what is being compared"",
                    ""explanation"": ""brief explanation""
                }}

                Respond ONLY with the JSON object.
                ";

            try
            {
                var parseResponse = await searchService.ChatClient.AsIChatClient().GetResponseAsync(parsePrompt);
                var responseText = parseResponse.Messages.FirstOrDefault()?.Text ?? "{}";
                
                // Clean markdown code blocks
                if (responseText.Contains("```json"))
                {
                    responseText = responseText.Split("```json")[1].Split("```")[0].Trim();
                }
                else if (responseText.Contains("```"))
                {
                    responseText = responseText.Split("```")[1].Split("```")[0].Trim();
                }

                var parsed = JsonSerializer.Deserialize<ComparisonParse>(responseText);
                if (parsed == null) throw new Exception("Failed to parse comparison");

                var allItems = new List<string> { parsed.Item1, parsed.Item2 };
                allItems.AddRange(parsed.AdditionalItems ?? new List<string>());

                // Perform separate searches for each item
                var comparisonResults = new Dictionary<string, ComparisonResult>();

                foreach (var item in allItems)
                {
                    var results = await searchService.SearchTicketsAsync(item, topK: 100, includeSemanticSearch: false);
                    comparisonResults[item] = new ComparisonResult
                    {
                        Count = results.Count,
                        Tickets = results.Take(10).ToList()
                    };
                }

                var totalResults = comparisonResults.Values.Sum(r => r.Count);
                if (totalResults == 0)
                {
                    return $"Question: {userQuestion}\n\nNo tickets were found for any of the items being compared.";
                }

                var resultsJson = JsonSerializer.Serialize(comparisonResults, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var summary = string.Join("\n", comparisonResults.Select(kvp => $"- {kvp.Key}: {kvp.Value.Count} tickets"));

                return $"""
                    Based on the following IT support tickets, answer the comparison question.

                    Question: {userQuestion}

                    Search Results Summary:
                    {summary}

                    Detailed Ticket Data:
                    {resultsJson}

                    Provide a detailed comparative analysis showing counts, examples, and which item has more.
                    """;
            }
            catch (Exception ex)
            {
                return $"Error: Unable to parse the comparison question. {ex.Message}";
            }
        };
    }

    private record ComparisonParse(
        [property: JsonPropertyName("item_1")] string Item1,
        [property: JsonPropertyName("item_2")] string Item2,
        [property: JsonPropertyName("additional_items")] List<string>? AdditionalItems,
        [property: JsonPropertyName("comparison_type")] string ComparisonType,
        [property: JsonPropertyName("explanation")] string Explanation);

    private class ComparisonResult
    {
        public int Count { get; set; }
        public List<Models.SupportTicket> Tickets { get; set; } = new();
    }

    public static AIAgent Create(ChatClient chatClient, SearchService searchService)
    {
        var searchFunction = CreateSearchFunction(searchService);

        return chatClient.CreateAIAgent(
            instructions: Instructions,
            name: "comparative_agent",
            tools: new[] { AIFunctionFactory.Create(searchFunction) }
        );
    }
}
