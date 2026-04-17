using System.ClientModel;
using Azure.AI.Projects;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;

namespace AgentFrameworkDev.Config;

/// <summary>
/// Configuration details for creating Microsoft Foundry clients.
/// </summary>
public sealed record FoundryClientConfiguration(
    Uri Endpoint,
    TokenCredential Credential,
    Uri? AzureOpenAIEndpoint = null,
    string? AzureOpenAIApiKey = null,
    string? AzureOpenAIChatDeploymentName = null)
{
    public bool HasAzureOpenAIKeyAuth =>
        AzureOpenAIEndpoint is not null &&
        !string.IsNullOrWhiteSpace(AzureOpenAIApiKey) &&
        !string.IsNullOrWhiteSpace(AzureOpenAIChatDeploymentName);
}

/// <summary>
/// Factory for creating Microsoft Foundry clients with automatic configuration discovery.
/// Prefers the repo-local appsettings.Local.json used by the labs and falls back to parent-directory discovery.
/// </summary>
public static class FoundryClientFactory
{
    private const string DefaultConfigFileName = "appsettings.Local.json";

    /// <summary>
    /// Loads configuration from the repo-local appsettings.Local.json used by the dotnet labs.
    /// Environment variables are still available as fallback values, but the JSON file wins when both are present.
    /// Returns details needed to create any Foundry client.
    /// </summary>
    /// <param name="configFileName">The configuration file name to search for. Defaults to appsettings.Local.json.</param>
    /// <returns>Configuration containing endpoint, deployment name, and credential.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required configuration values are missing.</exception>
    public static FoundryClientConfiguration GetConfiguration(string configFileName = DefaultConfigFileName)
    {
        var configPath = ResolveConfigPath(configFileName);
        var basePath = Path.GetDirectoryName(configPath)!;

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddEnvironmentVariables()
            .AddJsonFile(Path.GetFileName(configPath), optional: false, reloadOnChange: false);

        var configuration = configurationBuilder.Build();

        var endpoint = configuration["AZURE_AI_PROJECT_ENDPOINT"]
            ?? throw new InvalidOperationException(
                $"Azure AI endpoint not configured in {configPath}.");

        var credential = CreateCredential(configuration);
        var azureOpenAIEndpoint = TryGetUri(configuration["AZURE_OPENAI_ENDPOINT"]);
        var azureOpenAIApiKey = configuration["AZURE_OPENAI_API_KEY"];
        var azureOpenAIChatDeploymentName = configuration["AZURE_OPENAI_CHAT_DEPLOYMENT_NAME"]
            ?? configuration["AZURE_OPENAI_DEPLOYMENT_NAME"]
            ?? configuration["AZURE_AI_MODEL_DEPLOYMENT_NAME"];

        return new FoundryClientConfiguration(
            new Uri(endpoint),
            credential,
            azureOpenAIEndpoint,
            azureOpenAIApiKey,
            azureOpenAIChatDeploymentName);
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
    /// Creates an IChatClient for the local declarative agent.
    /// Prefers Azure OpenAI API-key auth from the shared lab config and falls back to Foundry project auth.
    /// </summary>
    public static IChatClient CreateChatClient(FoundryClientConfiguration? config = null)
    {
        config ??= GetConfiguration();

        if (config.HasAzureOpenAIKeyAuth)
        {
            var openAIClient = new OpenAIClient(
                new ApiKeyCredential(config.AzureOpenAIApiKey!),
                new OpenAIClientOptions
                {
                    Endpoint = EnsureAzureOpenAIV1Endpoint(config.AzureOpenAIEndpoint!)
                });

            return openAIClient
                .GetChatClient(config.AzureOpenAIChatDeploymentName!)
                .AsIChatClient();
        }

#pragma warning disable OPENAI001
        return CreateProjectClient(config)
            .GetProjectOpenAIClient()
            .GetResponsesClient()
            .AsIChatClient();
#pragma warning restore OPENAI001
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

    private static Uri? TryGetUri(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null;

    private static Uri EnsureAzureOpenAIV1Endpoint(Uri endpoint)
    {
        if (endpoint.AbsolutePath.Equals("/openai/v1", StringComparison.OrdinalIgnoreCase) ||
            endpoint.AbsolutePath.Equals("/openai/v1/", StringComparison.OrdinalIgnoreCase))
        {
            return endpoint;
        }

        return new Uri(endpoint, "/openai/v1/");
    }

    private static string ResolveConfigPath(string fileName)
    {
        var preferredPaths = new[]
        {
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", fileName)),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", fileName))
        };

        foreach (var preferredPath in preferredPaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(preferredPath))
            {
                return preferredPath;
            }
        }

        var discoveredBasePath = FindConfigDirectory(fileName);
        if (discoveredBasePath is not null)
        {
            return Path.Combine(discoveredBasePath, fileName);
        }

        throw new InvalidOperationException(
            $"Could not find {fileName}. Expected the lab config at ../../{fileName} relative to the project directory.");
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
