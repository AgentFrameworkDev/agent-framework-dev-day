using Azure.AI.Projects;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Lab0;

/// <summary>
/// Configuration details for creating Microsoft Foundry clients.
/// </summary>
public sealed record FoundryClientConfiguration(
    Uri Endpoint,
    string DeploymentName,
    TokenCredential Credential
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

        return new FoundryClientConfiguration(
            new Uri(endpoint),
            deploymentName,
            credential
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
