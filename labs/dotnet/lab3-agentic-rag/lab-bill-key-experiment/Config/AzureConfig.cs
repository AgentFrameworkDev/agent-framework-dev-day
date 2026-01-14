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
    public string OpenAIApiKey { get; set; } = string.Empty;
    public string OpenAIApiVersion { get; set; } = string.Empty;
    public string ChatModel { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = string.Empty;
    
    // Credentials
    public DefaultAzureCredential Credential { get; } = new DefaultAzureCredential();
    
    /// <summary>
    /// Load configuration from environment variables and configuration sources.
    /// </summary>
    public static AzureConfig FromConfiguration(IConfiguration configuration)
    {
        return new AzureConfig
        {
            SearchEndpoint = configuration["AzureSearch:Endpoint"] ?? string.Empty,
            SearchApiKey = configuration["AzureSearch:ApiKey"] ?? string.Empty,
            SearchIndexName = configuration["AzureSearch:IndexName"] ?? string.Empty,
            OpenAIEndpoint = configuration["AzureOpenAI:Endpoint"] ?? string.Empty,
            OpenAIApiKey = configuration["AzureOpenAI:ApiKey"] ?? string.Empty,
            OpenAIApiVersion = configuration["AzureOpenAI:ApiVersion"] ?? string.Empty,
            ChatModel = configuration["AzureOpenAI:ChatDeploymentName"] ?? string.Empty,
            EmbeddingModel = configuration["AzureOpenAI:EmbeddingDeploymentName"] ?? string.Empty,
        };
    }
    
    /// <summary>
    /// Validate that all required configuration is present (including OpenAI settings).
    /// Use ValidateSearchOnly() if OpenAI is configured via Foundry credentials.
    /// </summary>
    public void Validate()
    {
        ValidateSearchOnly();

        if (string.IsNullOrWhiteSpace(EmbeddingModel))
            throw new InvalidOperationException("AZURE_OPENAI_EMBEDDING_DEPLOYMENT configuration is required");

        if (string.IsNullOrWhiteSpace(ChatModel))
            throw new InvalidOperationException("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME configuration is required");

        if (string.IsNullOrWhiteSpace(OpenAIApiKey))
            throw new InvalidOperationException("AZURE_OPENAI_API_KEY configuration is required");

        if (string.IsNullOrWhiteSpace(OpenAIEndpoint))
            throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT configuration is required");
    }

    /// <summary>
    /// Validate only Azure Search configuration.
    /// Use this when OpenAI is configured via Foundry credentials.
    /// </summary>
    public void ValidateSearchOnly()
    {
        if (string.IsNullOrWhiteSpace(SearchEndpoint))
            throw new InvalidOperationException("AZURE_SEARCH_ENDPOINT configuration is required");

        if (string.IsNullOrWhiteSpace(SearchApiKey))
            throw new InvalidOperationException("AZURE_SEARCH_API_KEY configuration is required");

        if (string.IsNullOrWhiteSpace(SearchIndexName))
            throw new InvalidOperationException("AZURE_SEARCH_INDEX_NAME configuration is required");
    }
}
