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
    private static string? _configPath;

    /// <summary>
    /// Gets the configuration, loading from appsettings.Local.json in the dotnet folder and environment variables.
    /// </summary>
    private static IConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                _configPath = FindConfigPath(AppContext.BaseDirectory);
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(_configPath)
                    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
            }
            return _configuration;
        }
    }

    /// <summary>
    /// Finds the dotnet folder by traversing up the directory tree.
    /// </summary>
    private static string FindConfigPath(string startPath)
    {
        var currentDir = new DirectoryInfo(startPath);

        // Traverse up to find the 'dotnet' folder
        while (currentDir != null)
        {
            if (currentDir.Name.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            {
                return currentDir.FullName;
            }
            currentDir = currentDir.Parent;
        }

        // Fallback to start path if dotnet folder not found
        return startPath;
    }

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
        // Force configuration loading to set _configPath
        _ = Configuration;
        
        // Display config location
        Console.WriteLine($"📁 Config path: {_configPath}");
        Console.WriteLine($"📄 Config file: {Path.Combine(_configPath ?? "", "appsettings.Local.json")}");
        Console.WriteLine();

        // Get endpoint (required)
        var endpoint = GetConfigValue("AzureOpenAI:Endpoint", "AZURE_OPENAI_ENDPOINT")
            ?? throw new InvalidOperationException(
                "Azure OpenAI endpoint is not configured. " +
                "Set 'AzureOpenAI:Endpoint' in appsettings.json or 'AZURE_OPENAI_ENDPOINT' environment variable.");

        // Get deployment name (optional, default: gpt-4o-mini)
        // Check in order: AZURE_OPENAI_DEPLOYMENT_NAME, AZURE_AI_MODEL_DEPLOYMENT_NAME, AzureOpenAI:DeploymentName
        var deploymentName = Configuration["AZURE_OPENAI_DEPLOYMENT_NAME"] 
            ?? Configuration["AZURE_AI_MODEL_DEPLOYMENT_NAME"]
            ?? Configuration["AzureOpenAI:DeploymentName"]
            ?? "gpt-4o-mini";

        Console.WriteLine($"✅ Configuration loaded");
        Console.WriteLine($"   Endpoint: {endpoint}");
        Console.WriteLine($"   Deployment: {deploymentName}");
        Console.WriteLine();

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
    /// Gets a configuration value, checking environment variable first, then config keys.
    /// Environment variables take precedence if both are set.
    /// </summary>
    private static string? GetConfigValue(string appSettingsKey, string environmentVariable)
    {
        // Check environment variable first (highest precedence)
        var envValue = Environment.GetEnvironmentVariable(environmentVariable);
        if (!string.IsNullOrEmpty(envValue))
        {
            return envValue;
        }

        // Check config file with environment variable key (e.g., AZURE_OPENAI_ENDPOINT)
        var configEnvValue = Configuration[environmentVariable];
        if (!string.IsNullOrEmpty(configEnvValue))
        {
            return configEnvValue;
        }

        // Fall back to nested config key (e.g., AzureOpenAI:Endpoint)
        return Configuration[appSettingsKey];
    }
}
