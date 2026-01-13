using System.Text.Json;
using System.Text.Json.Serialization;
using Lab3.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace Lab3.Agents;
public static class MultiHopAgent
{
    private const string Instructions = """
        You are a specialist in answering multi-hop reasoning questions about IT support tickets.

        When you receive a question that requires finding information A and then extracting information B:
        1. Use the multi_hop_search function to retrieve relevant tickets
        2. Analyze the tickets to extract the specific information requested
        3. Provide a clear answer to the question
        4. Cite specific examples from the tickets using their IDs

        Example response format:

        Based on the IT support tickets, the following departments had consultants with Login Issues:

        **Human Resources Department**
        - Ticket INC001234: HR consultant unable to access system
        - Ticket INC001567: External HR consultant login failed

        **Finance Department**  
        - Ticket INC002345: Finance consultant VPN login issue

        **Summary**: 2 departments (Human Resources and Finance) had consultants experiencing login issues.

        Be precise and base your answer strictly on the search results.
        Focus on answering the specific question asked (e.g., "What department" means list the departments).
        """;

    private static Func<string, Task<string>> CreateSearchFunction(SearchService searchService)
    {
        return async (string userQuestion) =>
        {
            // Parse the question to identify the search query and target extraction
            var parsePrompt = $@"
                Analyze this multi-hop reasoning question and extract:

                Question: {userQuestion}

                Extract:
                1. SEARCH_QUERY: What to search for (the main topic/constraint)
                2. TARGET_FIELD: What information to extract from the results (what the question is actually asking for)
                3. REASONING: Brief explanation of the multi-hop logic

                Format your response as JSON:
                {{
                    ""search_query"": ""what to search for in tickets"",
                    ""target_field"": ""what field/information to extract (e.g., department, priority, type, user, etc.)"",
                    ""reasoning"": ""brief explanation of the multi-hop reasoning""
                }}

                Examples:
                - ""What department had consultants with Login Issues?"" 
                  -> {{""search_query"": ""consultants Login Issues"", ""target_field"": ""department (Queue field)"", ""reasoning"": ""Search for tickets about consultants with login issues, then extract which departments those tickets belong to""}}
                  
                - ""Which priority level has the most printer problems?""
                  -> {{""search_query"": ""printer problems"", ""target_field"": ""priority level (Priority field)"", ""reasoning"": ""Search for printer problem tickets, then analyze their priority levels""}}

                - ""What ticket type do Surface issues usually get classified as?""
                  -> {{""search_query"": ""Surface issues"", ""target_field"": ""ticket type (Type field)"", ""reasoning"": ""Search for Surface-related tickets, then determine their ticket types""}}

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

                var parsed = JsonSerializer.Deserialize<MultiHopParse>(responseText);
                if (parsed == null) throw new Exception("Failed to parse multi-hop question");

                var searchQuery = parsed.SearchQuery;
                var targetField = parsed.TargetField;
                var reasoning = parsed.Reasoning;

                // Perform the initial search
                var searchResults = await searchService.SearchTicketsAsync(searchQuery, topK: 20);

                // Check if any results found
                if (searchResults.Count == 0)
                {
                    return $"""
                        Question: {userQuestion}

                        Search Query: {searchQuery}
                        No tickets were found matching the search criteria.
                        """;
                }

                // Build analysis prompt with search results
                var resultsJson = JsonSerializer.Serialize(searchResults, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return $"""
                    Based on the following IT support tickets, answer the multi-hop reasoning question.

                    Question: {userQuestion}

                    Multi-Hop Reasoning: {reasoning}
                    - Search performed: "{searchQuery}"
                    - Target information to extract: {targetField}
                    - Found {searchResults.Count} relevant tickets

                    Relevant Tickets:
                    {resultsJson}

                    Provide a detailed answer that:
                    1. Directly answers the question by extracting the {targetField} from the tickets
                    2. Groups results by the target field (e.g., if asking about departments, group by department)
                    3. Cites specific ticket IDs as evidence
                    4. Provides a summary with counts or key findings

                    IMPORTANT: Focus on answering what the question is asking for ({targetField}), not just describing the tickets.
                    If the question asks "What department", list the departments.
                    If the question asks "Which priority", list the priority levels.

                    Format your response clearly with the extracted information prominently displayed.
                    Base your answer strictly on the search results provided.
                    """;
            }
            catch (Exception ex)
            {
                return $"""
                    Question: {userQuestion}

                    Error: Unable to parse the multi-hop question. Please rephrase your question.
                    Details: {ex.Message}
                    """;
            }
        };
    }

    private record MultiHopParse(
        [property: JsonPropertyName("search_query")] string SearchQuery,
        [property: JsonPropertyName("target_field")] string TargetField,
        [property: JsonPropertyName("reasoning")] string Reasoning);
    
    public static AIAgent Create(ChatClient chatClient, SearchService searchService)
    {
        var searchFunction = CreateSearchFunction(searchService);

        return chatClient.CreateAIAgent(
            instructions: Instructions,
            name: "multi_hop_agent",
            tools: new[] { AIFunctionFactory.Create(searchFunction) }
        );
    }
}