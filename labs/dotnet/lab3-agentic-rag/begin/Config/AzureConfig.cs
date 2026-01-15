using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Lab3.Config;

/// <summary>
/// Configuration for Azure services.
/// </summary>
public class AzureConfig
{
    // Azure AI Search
    public string SearchEndpoint { get; set; } = string.Empty;
    public string SearchApiKey { get; set; } = string.Empty;
    public string SearchIndexName { get; set; } = string.Empty;

    // Azure OpenAI
    public string OpenAIEndpoint { get; set; } = string.Empty;
    public string OpenAIApiVersion { get; set; } = string.Empty;
    public string ChatModel { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = string.Empty;

    // Credentials
    public DefaultAzureCredential Credential { get; } = new DefaultAzureCredential();

    /// <summary>
    /// Load configuration from environment variables and configuration sources.
    /// </summary>
    public static AzureConfig FromConfiguration()
    {
        return new AzureConfig
        {
            SearchEndpoint = Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT") ?? string.Empty,
            SearchApiKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_API_KEY") ?? string.Empty,
            SearchIndexName = Environment.GetEnvironmentVariable("AZURE_SEARCH_INDEX_NAME") ?? string.Empty,
            OpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty,
            OpenAIApiVersion = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_VERSION") ?? string.Empty,
            ChatModel = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME") ?? string.Empty,
            EmbeddingModel = Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBEDDING_DEPLOYMENT") ?? string.Empty,
        };
    }

    /// <summary>
    /// Validate that all required configuration is present.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SearchEndpoint))
            throw new InvalidOperationException("AZURE_SEARCH_ENDPOINT configuration is required");

        if (string.IsNullOrWhiteSpace(SearchApiKey))
            throw new InvalidOperationException("AZURE_SEARCH_API_KEY configuration is required");

        if (string.IsNullOrWhiteSpace(SearchIndexName))
            throw new InvalidOperationException("AZURE_SEARCH_INDEX_NAME configuration is required");

        if (string.IsNullOrWhiteSpace(EmbeddingModel))
            throw new InvalidOperationException("AZURE_OPENAI_EMBEDDING_DEPLOYMENT configuration is required");

        if (string.IsNullOrWhiteSpace(ChatModel))
            throw new InvalidOperationException("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME configuration is required");

        if (string.IsNullOrWhiteSpace(OpenAIEndpoint))
            throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT configuration is required");
    }
}
