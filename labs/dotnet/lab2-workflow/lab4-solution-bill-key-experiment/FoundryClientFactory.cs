using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using OpenAI.Embeddings;

namespace AgentFrameworkDev.Config;

/// <summary>
/// Configuration details for creating Microsoft Foundry clients and Azure Search.
/// </summary>
public sealed record FoundryClientConfiguration(
    Uri Endpoint,
    string DeploymentName,
    TokenCredential Credential,
    string? EmbeddingDeploymentName = null,
    string? SearchEndpoint = null,
    string? SearchApiKey = null,
    string? SearchIndexName = null
);

/// <summary>
/// Factory for creating Microsoft Foundry clients with automatic configuration discovery.
/// Searches parent directories for appsettings.Local.json and supports environment variable overrides.
/// </summary>
public static class FoundryClientFactory
{
    private const string DefaultConfigFileName = "appsettings.Local.json";

    /// <summary>
    /// Loads configuration from appsettings.Local.json (searching parent directories)
    /// and environment variables. Returns details needed to create any Foundry client.
    /// </summary>
    /// <param name="configFileName">The configuration file name to search for. Defaults to appsettings.Local.json.</param>
    /// <returns>Configuration containing endpoint, deployment name, and credential.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required configuration values are missing.</exception>
    public static FoundryClientConfiguration GetConfiguration(string configFileName = DefaultConfigFileName)
    {
        var basePath = FindConfigDirectory(configFileName)
            ?? throw new InvalidOperationException(
                $"Could not find {configFileName} in current directory or any parent directory.");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(configFileName, optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var endpoint = configuration["AZURE_AI_PROJECT_ENDPOINT"]
            ?? throw new InvalidOperationException(
                "Azure AI endpoint not configured. Set AZURE_AI_PROJECT_ENDPOINT in config or environment.");

        var deploymentName = configuration["AZURE_AI_MODEL_DEPLOYMENT_NAME"]
            ?? throw new InvalidOperationException(
                "Azure AI deployment not configured. Set AZURE_AI_MODEL_DEPLOYMENT_NAME in config or environment.");

        var credential = CreateCredential(configuration);
        var embeddingDeployment = configuration["AZURE_AI_EMBEDDING_DEPLOYMENT_NAME"];

        // Azure Search configuration (optional - check both flat and nested keys)
        var searchEndpoint = configuration["AZURE_SEARCH_ENDPOINT"] 
            ?? configuration["AzureSearch:Endpoint"];
        var searchApiKey = configuration["AZURE_SEARCH_API_KEY"] 
            ?? configuration["AzureSearch:ApiKey"];
        var searchIndexName = configuration["AZURE_SEARCH_INDEX_NAME"] 
            ?? configuration["AzureSearch:IndexName"];

        return new FoundryClientConfiguration(
            new Uri(endpoint),
            deploymentName,
            credential,
            embeddingDeployment,
            searchEndpoint,
            searchApiKey,
            searchIndexName
        );
    }

    /// <summary>
    /// Creates an AIProjectClient using the provided configuration or by loading configuration automatically.
    /// </summary>
    /// <param name="config">Optional configuration. If null, calls GetConfiguration() to load automatically.</param>
    /// <returns>A configured AIProjectClient ready for use.</returns>
    public static AIProjectClient CreateProjectClient(FoundryClientConfiguration? config = null)
    {
        config ??= GetConfiguration();
        return new AIProjectClient(config.Endpoint, config.Credential);
    }

    /// <summary>
    /// Retrieves the Azure OpenAI endpoint from the Foundry Project's connections.
    /// </summary>
    /// <param name="config">Optional configuration. If null, calls GetConfiguration() to load automatically.</param>
    /// <returns>The Azure OpenAI endpoint URI.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no Azure OpenAI connection is found.</exception>
    public static Task<Uri> GetOpenAIEndpointAsync(FoundryClientConfiguration? config = null)
    {
        config ??= GetConfiguration();
        var projectClient = new AIProjectClient(config.Endpoint, config.Credential);

        // Get the Azure OpenAI connection using the client type name
        var connection = projectClient.GetConnection(typeof(AzureOpenAIClient).FullName!);

        if (!connection.TryGetLocatorAsUri(out Uri? uri) || uri is null)
        {
            throw new InvalidOperationException(
                "No Azure OpenAI connection found in the Foundry Project. " +
                "Ensure an Azure OpenAI connection is configured in your Azure AI Foundry project.");
        }

        // Return just the host portion as the endpoint
        return Task.FromResult(new Uri($"https://{uri.Host}"));
    }

    /// <summary>
    /// Creates an AzureOpenAIClient using Foundry credentials.
    /// Automatically discovers the Azure OpenAI endpoint from the project's connections.
    /// </summary>
    /// <param name="config">Optional configuration. If null, calls GetConfiguration() to load automatically.</param>
    /// <returns>A configured AzureOpenAIClient ready for use.</returns>
    public static async Task<AzureOpenAIClient> CreateOpenAIClientAsync(FoundryClientConfiguration? config = null)
    {
        config ??= GetConfiguration();
        var openAiEndpoint = await GetOpenAIEndpointAsync(config);
        return new AzureOpenAIClient(openAiEndpoint, config.Credential);
    }

    /// <summary>
    /// Creates a ChatClient for the configured chat deployment.
    /// Automatically discovers the Azure OpenAI endpoint from the Foundry project.
    /// </summary>
    /// <param name="config">Optional configuration. If null, calls GetConfiguration() to load automatically.</param>
    /// <returns>A configured ChatClient ready for use.</returns>
    public static async Task<ChatClient> CreateChatClientAsync(FoundryClientConfiguration? config = null)
    {
        config ??= GetConfiguration();
        var client = await CreateOpenAIClientAsync(config);
        return client.GetChatClient(config.DeploymentName);
    }

    /// <summary>
    /// Creates an EmbeddingClient for the configured embedding deployment.
    /// Automatically discovers the Azure OpenAI endpoint from the Foundry project.
    /// Falls back to the main deployment if no embedding deployment is configured.
    /// </summary>
    /// <param name="config">Optional configuration. If null, calls GetConfiguration() to load automatically.</param>
    /// <returns>A configured EmbeddingClient ready for use.</returns>
    public static async Task<EmbeddingClient> CreateEmbeddingClientAsync(FoundryClientConfiguration? config = null)
    {
        config ??= GetConfiguration();
        var client = await CreateOpenAIClientAsync(config);
        var embeddingDeployment = config.EmbeddingDeploymentName ?? config.DeploymentName;
        return client.GetEmbeddingClient(embeddingDeployment);
    }

    /// <summary>
    /// Validates that Azure Search configuration is present.
    /// </summary>
    /// <param name="config">The configuration to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when required Azure Search values are missing.</exception>
    public static void ValidateSearchConfiguration(FoundryClientConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(config.SearchEndpoint))
            throw new InvalidOperationException("AZURE_SEARCH_ENDPOINT configuration is required");

        if (string.IsNullOrWhiteSpace(config.SearchApiKey))
            throw new InvalidOperationException("AZURE_SEARCH_API_KEY configuration is required");

        if (string.IsNullOrWhiteSpace(config.SearchIndexName))
            throw new InvalidOperationException("AZURE_SEARCH_INDEX_NAME configuration is required");
    }

    private static TokenCredential CreateCredential(IConfiguration configuration)
    {
        var tenantId = configuration["AZURE_TENANT_ID"];
        var clientId = configuration["AZURE_CLIENT_ID"];
        var clientSecret = configuration["AZURE_CLIENT_SECRET"];

        if (!string.IsNullOrEmpty(tenantId) &&
            !string.IsNullOrEmpty(clientId) &&
            !string.IsNullOrEmpty(clientSecret))
        {
            return new ClientSecretCredential(tenantId, clientId, clientSecret);
        }

        return new DefaultAzureCredential();
    }

    private static string? FindConfigDirectory(string fileName)
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, fileName)))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        return null;
    }
}
