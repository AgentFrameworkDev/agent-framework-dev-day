using System.Text.Json;
using System.Text.Json.Serialization;
using Lab3.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace Lab3.Agents;

/// <summary>
/// Difference agent for answering questions that require finding items in one set but not in another.
/// </summary>
public static class DifferenceAgent
{
    private const string Instructions = """
        You are a specialist in answering difference/exclusion questions about IT support tickets.

        When you receive a question asking about items that match one criterion but NOT another:
        1. Use the difference_search function to retrieve relevant tickets
        2. Analyze the two sets of results (items matching main criteria vs. items matching exclusion criteria)
        3. Identify items that appear in the first set but NOT in the second
        4. Provide a clear list of the differences
        5. Cite specific examples from the tickets using their IDs

        Example response format:

        Based on the search results, here are Dell XPS issues that do NOT mention Windows:

        1. **Ticket INC001234**: Dell XPS 15 touchpad not responding
           - Description: Physical touchpad hardware issue, requires replacement

        2. **Ticket INC001567**: Dell XPS 13 battery swelling
           - Description: Battery hardware defect, safety recall initiated

        3. **Ticket INC002345**: Dell XPS keyboard backlight malfunction
           - Description: LED backlight circuit failure

        **Summary**: Found 3 Dell XPS issues that do not mention Windows. These are primarily hardware-related problems rather than software issues.

        Be precise and base your answer strictly on the search results.
        Clearly explain what was excluded and why.
        """;

    private static Func<string, Task<string>> CreateSearchFunction(SearchService searchService)
    {
        return async (string userQuestion) =>
        {
            // Parse the question to identify the main search and exclusion criteria
            var parsePrompt = $@"
                Analyze this question and extract two search queries:

                Question: {userQuestion}

                Extract:
                1. MAIN_SEARCH: The primary topic/category to search for
                2. EXCLUSION_TERM: The term/concept that should NOT appear in the results

                Format your response as JSON:
                {{
                    ""main_search"": ""the primary search query"",
                    ""exclusion_term"": ""the term to exclude"",
                    ""explanation"": ""brief explanation of the logic""
                }}

                Examples:
                - ""Which Dell XPS Issue does not mention Windows?"" 
                  -> {{""main_search"": ""Dell XPS Issue"", ""exclusion_term"": ""Windows"", ""explanation"": ""Find Dell XPS issues, exclude those mentioning Windows""}}
                  
                - ""What Surface problems don't involve the battery?""
                  -> {{""main_search"": ""Surface problems"", ""exclusion_term"": ""battery"", ""explanation"": ""Find Surface problems, exclude those involving battery""}}
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

                var parsed = JsonSerializer.Deserialize<DifferenceParse>(responseText);
                if (parsed == null) throw new Exception("Failed to parse difference question");

                var mainSearch = parsed.MainSearch;
                var exclusionTerm = parsed.ExclusionTerm;
                var explanation = parsed.Explanation;

                // Perform first search: get all items matching main criteria
                var mainResults = await searchService.SearchTicketsAsync(mainSearch, topK: 20);

                // Perform second search: get items matching main criteria AND exclusion term
                var combinedQuery = $"{mainSearch} {exclusionTerm}";
                var exclusionResults = await searchService.SearchTicketsAsync(combinedQuery, topK: 20);

                // Get IDs from exclusion results
                var exclusionIds = exclusionResults.Select(r => r.Id).ToHashSet();

                // Find difference: items in main_results but not in exclusion_results
                var differenceResults = mainResults
                    .Where(result => !exclusionIds.Contains(result.Id))
                    .ToList();

                // Check if any results found
                if (mainResults.Count == 0)
                {
                    return $"""
                        Question: {userQuestion}

                        No tickets were found matching the main search criteria: '{mainSearch}'
                        """;
                }

                if (differenceResults.Count == 0)
                {
                    return $"""
                        Question: {userQuestion}

                        All tickets matching '{mainSearch}' also mention '{exclusionTerm}'.
                        No differences found.
                        """;
                }

                // Build analysis prompt with difference results
                var resultsJson = JsonSerializer.Serialize(differenceResults, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return $@"
                    Based on the following IT support tickets, answer the difference question.

                    Question: {userQuestion}

                    Search Logic: {explanation}
                    - Found {mainResults.Count} tickets matching ""{mainSearch}""
                    - Found {exclusionResults.Count} tickets that also mention ""{exclusionTerm}""
                    - Difference: {differenceResults.Count} tickets match ""{mainSearch}"" but do NOT mention ""{exclusionTerm}""

                    Tickets that match the difference criteria:
                    {resultsJson}

                    Provide a detailed answer that:
                    1. Lists the tickets that meet the difference criteria (in main set but NOT in exclusion set)
                    2. Provides brief descriptions of each ticket
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

                    Error: Unable to parse the difference question. Please rephrase your question.
                    Details: {ex.Message}
                    """;
            }
        };
    }

    private record DifferenceParse(
        [property: JsonPropertyName("main_search")] string MainSearch,
        [property: JsonPropertyName("exclusion_term")] string ExclusionTerm,
        [property: JsonPropertyName("explanation")] string Explanation);

    public static AIAgent Create(ChatClient chatClient, SearchService searchService)
    {
        var searchFunction = CreateSearchFunction(searchService);

        return chatClient.CreateAIAgent(
            instructions: Instructions,
            name: "difference_agent",
            tools: new[] { AIFunctionFactory.Create(searchFunction) }
        );
    }
}