using System.Text.Json;
using System.Text.Json.Serialization;
using Lab3Solution.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace Lab3Solution.Agents;

/// <summary>
/// Intersection agent for answering questions that require items matching multiple criteria.
/// </summary>
public static class IntersectionAgent
{
    private const string Instructions = """
        You are a specialist in answering intersection questions about IT support tickets.

        When you receive a question asking about items that match BOTH criterion A AND criterion B:
        1. Use the intersection_search function to retrieve relevant tickets
        2. Analyze the results to find items matching ALL specified criteria
        3. Provide a clear list of matching items
        4. Cite specific examples from the tickets using their IDs

        Example response format:

        Based on the search results, here are Dell XPS issues where the user tried Win + Ctrl + Shift + B:

        1. **Ticket INC001234**: Dell XPS 15 black screen issue
           - Description: User reports black screen, attempted Win+Ctrl+Shift+B with no success
           - Status: Requires hardware diagnostics

        2. **Ticket INC001567**: Dell XPS 13 display not responding
           - Description: Display frozen, user tried Win+Ctrl+Shift+B reset, issue persists
           - Status: Display driver reinstallation resolved the issue

        **Summary**: Found 2 Dell XPS issues where users attempted the Win+Ctrl+Shift+B graphics reset.

        Be precise and base your answer strictly on the search results.
        Clearly explain what criteria were matched.
        """;

    private static Func<string, Task<string>> CreateSearchFunction(SearchService searchService)
    {
        return async (string userQuestion) =>
        {
            // Parse the question to identify the multiple search criteria
            var parsePrompt = $@"
                Analyze this question and extract the multiple search criteria:

                Question: {userQuestion}

                Extract all the distinct criteria that items must match. Format your response as JSON:
                {{
                    ""criterion_1"": ""first search criterion"",
                    ""criterion_2"": ""second search criterion"",
                    ""additional_criteria"": [""any other criteria if present""],
                    ""combined_search"": ""a single combined search query that includes all criteria"",
                    ""explanation"": ""brief explanation of what we're looking for""
                }}

                Examples:
                - ""What issues are for Dell XPS laptops and the user tried Win + Ctrl + Shift + B?""
                  -> {{""criterion_1"": ""Dell XPS laptops"", ""criterion_2"": ""Win + Ctrl + Shift + B"", ""additional_criteria"": [], ""combined_search"": ""Dell XPS Win Ctrl Shift B graphics reset"", ""explanation"": ""Find Dell XPS issues where users tried the graphics reset keystroke""}}
                  
                - ""Which Surface tickets involve battery problems and high priority?""
                  -> {{""criterion_1"": ""Surface"", ""criterion_2"": ""battery problems"", ""additional_criteria"": [""high priority""], ""combined_search"": ""Surface battery high priority"", ""explanation"": ""Find high priority Surface tickets with battery issues""}}

                Respond ONLY with the JSON object.
                ";

            try
            {
                // Call LLM to parse the question
                var parseResponse = await searchService.ChatClient.AsIChatClient().GetResponseAsync(parsePrompt);
                var responseText = parseResponse.Messages.FirstOrDefault()?.Text ?? "{}";

                // Remove markdown code blocks if present
                if (responseText.Contains("```json"))
                {
                    responseText = responseText.Split("```json")[1].Split("```")[0].Trim();
                }
                else if (responseText.Contains("```"))
                {
                    responseText = responseText.Split("```")[1].Split("```")[0].Trim();
                }

                var parsed = JsonSerializer.Deserialize<IntersectionParse>(responseText);
                if (parsed == null) throw new Exception("Failed to parse intersection question");

                var criterion1 = parsed.Criterion1;
                var criterion2 = parsed.Criterion2;
                var combinedSearch = $"'{criterion1}' AND '{criterion2}'";
                var explanation = parsed.Explanation;

                // Perform combined search to find items matching all criteria
                var combinedResults = await searchService.SearchTicketsAsync(combinedSearch, topK: 20, includeSemanticSearch: false);

                // Also perform individual searches to verify intersection
                var search1Results = await searchService.SearchTicketsAsync(criterion1, topK: 50, includeSemanticSearch: false);
                var search2Results = await searchService.SearchTicketsAsync(criterion2, topK: 50, includeSemanticSearch: false);

                // Get IDs from each search
                var search1Ids = search1Results.Select(r => r.Id).ToHashSet();
                var search2Ids = search2Results.Select(r => r.Id).ToHashSet();

                // Find intersection: items that appear in BOTH individual searches
                var intersectionIds = search1Ids.Intersect(search2Ids).ToHashSet();

                // Filter combined results to only include items in the intersection
                // Also include items from combined search that match well
                var intersectionResults = combinedResults.Where(result =>
                    intersectionIds.Contains(result.Id) ||
                    ((result.Subject.Contains(criterion1, StringComparison.OrdinalIgnoreCase) ||
                      result.Body.Contains(criterion1, StringComparison.OrdinalIgnoreCase)) &&
                     (result.Subject.Contains(criterion2, StringComparison.OrdinalIgnoreCase) ||
                      result.Body.Contains(criterion2, StringComparison.OrdinalIgnoreCase)))
                ).ToList();

                // Remove duplicates based on ID
                var uniqueResults = intersectionResults
                    .GroupBy(r => r.Id)
                    .Select(g => g.First())
                    .ToList();

                // Check if any results found
                if (uniqueResults.Count == 0)
                {
                    return $"""
                        Question: {userQuestion}

                        Search Logic: {explanation}
                        - Found {search1Results.Count} tickets matching "{criterion1}"
                        - Found {search2Results.Count} tickets matching "{criterion2}"
                        - Intersection: 0 tickets match BOTH criteria

                        No tickets were found that match all specified criteria.
                        """;
                }

                // Build analysis prompt with intersection results
                var resultsJson = JsonSerializer.Serialize(uniqueResults, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return $@"
                    Based on the following IT support tickets, answer the intersection question.

                    Question: {userQuestion}

                    Search Logic: {explanation}
                    - Found {search1Results.Count} tickets matching ""{criterion1}""
                    - Found {search2Results.Count} tickets matching ""{criterion2}""
                    - Intersection: {uniqueResults.Count} tickets match ALL criteria

                    Tickets that match all criteria:
                    {resultsJson}

                    Provide a detailed answer that:
                    1. Lists the tickets that meet ALL criteria
                    2. Provides brief descriptions of each ticket highlighting how they match the criteria
                    3. Groups similar issues together if appropriate
                    4. Summarizes the findings

                    Format your response clearly with ticket IDs and descriptions.
                    Base your answer strictly on the search results provided.
                    ";
            }
            catch (Exception ex)
            {
                return $"""
                    Question: {userQuestion}

                    Error: Unable to parse the intersection question. Please rephrase your question.
                    Details: {ex.Message}
                    """;
            }
        };
    }

    private record IntersectionParse(
        [property: JsonPropertyName("criterion_1")] string Criterion1,
        [property: JsonPropertyName("criterion_2")] string Criterion2,
        [property: JsonPropertyName("additional_criteria")] List<string>? AdditionalCriteria,
        [property: JsonPropertyName("combined_search")] string CombinedSearch,
        [property: JsonPropertyName("explanation")] string Explanation);

    public static AIAgent Create(ChatClient chatClient, SearchService searchService)
    {
        var searchFunction = CreateSearchFunction(searchService);

        return chatClient.CreateAIAgent(
            instructions: Instructions,
            name: "intersection_agent",
            tools: new[] { AIFunctionFactory.Create(searchFunction) }
        );
    }
}