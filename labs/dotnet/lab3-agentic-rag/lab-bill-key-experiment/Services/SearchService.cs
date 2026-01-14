using System.Text.Json;
using AgentFrameworkDev.Config;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Lab3.Models;
using OpenAI.Chat;
using OpenAI.Embeddings;

namespace Lab3.Services;

/// <summary>
/// Service for searching IT support tickets in Azure AI Search.
/// </summary>
public class SearchService
{
    private readonly ChatClient _chatClient;
    private readonly SearchClient _searchClient;
    private readonly EmbeddingClient _embeddingClient;

    public SearchService(SearchClient searchClient, ChatClient chatClient, EmbeddingClient embeddingClient)
    {
        _searchClient = searchClient;
        _chatClient = chatClient;
        _embeddingClient = embeddingClient;
    }

    /// <summary>
    /// Generate embedding vector for the given text.
    /// </summary>
    public async Task<ReadOnlyMemory<float>> GetEmbeddingAsync(string text)
    {
        try
        {
            var response = await _embeddingClient.GenerateEmbeddingAsync(text);
            return response.Value.ToFloats();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate embedding: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Perform hybrid search on IT support tickets.
    /// Combines keyword search with vector search for optimal results.
    /// </summary>
    public async Task<List<SupportTicket>> SearchTicketsAsync(
        string query,
        int topK = 5,
        List<string>? selectFields = null,
        bool includeSemanticSearch = true)
    {
        selectFields ??= new List<string>
        {
            "Id", "Subject", "Body", "Answer", "Business_Type",
            "Type", "Queue", "Priority", "Tags"
        };

        SearchOptions searchOptions = new()
        {
            Size = topK,
        };

        foreach (var field in selectFields)
        {
            searchOptions.Select.Add(field);
        }

        if (includeSemanticSearch)
        {
            // Get embedding for the query
            var queryVector = await GetEmbeddingAsync(query);

            // Create vector query for semantic search
            var vectorQuery = new VectorizedQuery(queryVector)
            {
                // Search across body and answer embeddings
                Fields = { "BodyEmbeddings", "AnswerEmbeddings" }
            };

            searchOptions.VectorSearch = new VectorSearchOptions
            {
                Queries = { vectorQuery }
            };
        }

        // Perform search
        var searchResults = await _searchClient.SearchAsync<SearchDocument>(query, searchOptions);

        // Format results
        var formattedResults = new List<SupportTicket>();
        await foreach (var result in searchResults.Value.GetResultsAsync())
        {
            var doc = result.Document;
            var ticket = new SupportTicket
            {
                Id = doc["Id"]?.ToString() ?? string.Empty,
                Subject = doc["Subject"]?.ToString() ?? string.Empty,
                Body = doc["Body"]?.ToString() ?? string.Empty,
                Answer = doc["Answer"]?.ToString() ?? string.Empty,
                Type = doc["Type"]?.ToString() ?? string.Empty,
                Department = doc["Queue"]?.ToString() ?? string.Empty,
                Priority = doc["Priority"]?.ToString() ?? string.Empty,
                BusinessType = doc["Business_Type"]?.ToString() ?? string.Empty,
            };

            // Handle tags array
            if (doc.TryGetValue("Tags", out var tagsObj) && tagsObj is IEnumerable<object> tags)
            {
                ticket.Tags = tags.Select(t => t?.ToString() ?? string.Empty).ToList();
            }

            formattedResults.Add(ticket);
        }

        return formattedResults;
    }

    /// <summary>
    /// Perform hybrid search on IT support tickets with optional OData filter.
    /// </summary>
    public async Task<List<SupportTicket>> SearchTicketsWithFilterAsync(
        string query,
        string? odataFilter = null,
        int topK = 5,
        List<string>? selectFields = null)
    {
        selectFields ??= new List<string>
        {
            "Id", "Subject", "Body", "Answer", "Business_Type",
            "Type", "Queue", "Priority", "Tags"
        };

        // Get embedding for the query
        var queryVector = await GetEmbeddingAsync(query);

        // Create vector query for semantic search
        var vectorQuery = new VectorizedQuery(queryVector)
        {
            Fields = { "BodyEmbeddings", "AnswerEmbeddings" }
        };

        SearchOptions searchOptions = new()
        {
            Size = topK,
            Filter = odataFilter,
            VectorSearch = new VectorSearchOptions
            {
                Queries = { vectorQuery }
            }
        };

        foreach (var field in selectFields)
        {
            searchOptions.Select.Add(field);
        }

        // Perform search
        var searchResults = await _searchClient.SearchAsync<SearchDocument>(query, searchOptions);

        // Format results
        var formattedResults = new List<SupportTicket>();
        await foreach (var result in searchResults.Value.GetResultsAsync())
        {
            var doc = result.Document;
            var ticket = new SupportTicket
            {
                Id = doc["Id"]?.ToString() ?? string.Empty,
                Subject = doc["Subject"]?.ToString() ?? string.Empty,
                Body = doc["Body"]?.ToString() ?? string.Empty,
                Answer = doc["Answer"]?.ToString() ?? string.Empty,
                Type = doc["Type"]?.ToString() ?? string.Empty,
                Department = doc["Queue"]?.ToString() ?? string.Empty,
                Priority = doc["Priority"]?.ToString() ?? string.Empty,
                BusinessType = doc["Business_Type"]?.ToString() ?? string.Empty,
            };

            if (doc.TryGetValue("Tags", out var tagsObj) && tagsObj is IEnumerable<object> tags)
            {
                ticket.Tags = tags.Select(t => t?.ToString() ?? string.Empty).ToList();
            }

            formattedResults.Add(ticket);
        }

        return formattedResults;
    }

    /// <summary>
    /// Perform search and return results as JSON string.
    /// </summary>
    public async Task<string> SearchTicketsJsonAsync(string query, int topK = 5)
    {
        var results = await SearchTicketsAsync(query, topK);
        return JsonSerializer.Serialize(results, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Get the chat client for agent operations.
    /// </summary>
    public ChatClient ChatClient => _chatClient;
}

static class SearchDocumentExtensions
{
    public static T? GetValueOrDefault<T>(this SearchDocument document, string key)
    {
        return document.TryGetValue(key, out var value) && value is T typedValue
            ? typedValue
            : default;
    }
}
