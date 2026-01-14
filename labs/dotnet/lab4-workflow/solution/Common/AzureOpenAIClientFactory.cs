using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace WorkflowLab.Common;

/// <summary>
/// Factory class for creating Azure OpenAI chat clients with multiple authentication options.
/// Supports configuration from appsettings.json and environment variables.
/// </summary>
public static class AzureOpenAIClientFactory
{
    private static IConfiguration? _configuration;

    /// <summary>
    /// Gets the configuration, loading from appsettings.json, appsettings.Development.json, and environment variables.
    /// </summary>
    private static IConfiguration Configuration => _configuration ??= new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    /// <summary>
    /// Creates an Azure OpenAI chat client with support for multiple authentication methods:
    /// 1. API Key authentication (AzureOpenAI:ApiKey or AZURE_OPENAI_API_KEY)
    /// 2. Service Principal authentication (TenantId, ClientId, ClientSecret)
    /// 3. Managed Identity / DefaultAzureCredential (fallback)
    /// 
    /// Configuration can come from appsettings.json or environment variables.
    /// Environment variables take precedence over appsettings.json.
    /// </summary>
    public static IChatClient CreateChatClient()
    {
        // Get endpoint (required)
        var endpoint = GetConfigValue("AzureOpenAI:Endpoint", "AZURE_OPENAI_ENDPOINT")
            ?? throw new InvalidOperationException(
                "Azure OpenAI endpoint is not configured. " +
                "Set 'AzureOpenAI:Endpoint' in appsettings.json or 'AZURE_OPENAI_ENDPOINT' environment variable.");

        // Get deployment name (optional, default: gpt-4o-mini)
        var deploymentName = GetConfigValue("AzureOpenAI:DeploymentName", "AZURE_OPENAI_DEPLOYMENT_NAME") 
            ?? "gpt-4o-mini";

        // Option 1: API Key authentication
        var apiKey = GetConfigValue("AzureOpenAI:ApiKey", "AZURE_OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Using API Key authentication");
            return new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey))
                .GetChatClient(deploymentName)
                .AsIChatClient();
        }

        // Option 2: Service Principal authentication (Tenant ID, Client ID, Client Secret)
        var tenantId = GetConfigValue("AzureOpenAI:TenantId", "AZURE_TENANT_ID");
        var clientId = GetConfigValue("AzureOpenAI:ClientId", "AZURE_CLIENT_ID");
        var clientSecret = GetConfigValue("AzureOpenAI:ClientSecret", "AZURE_CLIENT_SECRET");

        if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        {
            Console.WriteLine("Using Service Principal authentication");
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            return new AzureOpenAIClient(new Uri(endpoint), credential)
                .GetChatClient(deploymentName)
                .AsIChatClient();
        }

        // Option 3: Managed Identity / DefaultAzureCredential (fallback)
        Console.WriteLine("Using Managed Identity / DefaultAzureCredential authentication");
        return new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
            .GetChatClient(deploymentName)
            .AsIChatClient();
    }

    /// <summary>
    /// Gets a configuration value, checking appsettings.json key first, then environment variable.
    /// Environment variables take precedence if both are set.
    /// </summary>
    private static string? GetConfigValue(string appSettingsKey, string environmentVariable)
    {
        // Check environment variable first (higher precedence)
        var envValue = Environment.GetEnvironmentVariable(environmentVariable);
        if (!string.IsNullOrEmpty(envValue))
        {
            return envValue;
        }

        // Fall back to appsettings.json
        return Configuration[appSettingsKey];
    }
}
