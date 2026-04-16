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
    /// 1. API Key authentication (AZURE_OPENAI_API_KEY)
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

        // ========================================================================
        // STEP 1: Create Azure OpenAI IChatClient with authentication
        // ========================================================================
        // TODO: Create and return an IChatClient connected to Azure OpenAI
        //
        // Hints:
        // - Get endpoint from GetConfigValue("AZURE_OPENAI_ENDPOINT") (required, throw if null)
        // - Get deployment from GetConfigValue("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini"
        // - Try API Key auth first: GetConfigValue("AZURE_OPENAI_API_KEY")
        //   If set, use new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey))
        // - Try Service Principal next: TenantId, ClientId, ClientSecret
        //   If all set, use new ClientSecretCredential(tenantId, clientId, clientSecret)
        // - Fallback to DefaultAzureCredential
        // - Convert to IChatClient: .GetChatClient(deploymentName).AsIChatClient()
        //
        // var endpoint = GetConfigValue("AZURE_OPENAI_ENDPOINT")
        //     ?? throw new InvalidOperationException(
        //         "Azure OpenAI endpoint is not configured. " +
        //         "Set 'AZURE_OPENAI_ENDPOINT' in appsettings.Local.json or as an environment variable.");
        //
        // var deploymentName = GetConfigValue("AZURE_OPENAI_DEPLOYMENT_NAME") 
        //     ?? GetConfigValue("AZURE_AI_MODEL_DEPLOYMENT_NAME")
        //     ?? "gpt-4o-mini";
        //
        // Console.WriteLine($"✅ Configuration loaded");
        // Console.WriteLine($"   Endpoint: {endpoint}");
        // Console.WriteLine($"   Deployment: {deploymentName}");
        // Console.WriteLine();
        //
        // // Option 1: API Key authentication
        // var apiKey = GetConfigValue("AZURE_OPENAI_API_KEY");
        // if (!string.IsNullOrEmpty(apiKey))
        // {
        //     Console.WriteLine("Using API Key authentication");
        //     return new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey))
        //         .GetChatClient(deploymentName)
        //         .AsIChatClient();
        // }
        //
        // // Option 2: Service Principal authentication
        // var tenantId = GetConfigValue("AZURE_TENANT_ID");
        // var clientId = GetConfigValue("AZURE_CLIENT_ID");
        // var clientSecret = GetConfigValue("AZURE_CLIENT_SECRET");
        //
        // if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        // {
        //     Console.WriteLine("Using Service Principal authentication");
        //     var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        //     return new AzureOpenAIClient(new Uri(endpoint), credential)
        //         .GetChatClient(deploymentName)
        //         .AsIChatClient();
        // }
        //
        // // Option 3: DefaultAzureCredential (fallback)
        // Console.WriteLine("Using Managed Identity / DefaultAzureCredential authentication");
        // return new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        //     .GetChatClient(deploymentName)
        //     .AsIChatClient();
        // ========================================================================
        throw new NotImplementedException("STEP 1: Create Azure OpenAI IChatClient with authentication");
    }

    /// <summary>
    /// Gets a configuration value, checking environment variable first, then config file.
    /// Environment variables take precedence if both are set.
    /// </summary>
    private static string? GetConfigValue(string key)
    {
        // Check environment variable first (highest precedence)
        var envValue = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrEmpty(envValue))
        {
            return envValue;
        }

        // Check config file
        return Configuration[key];
    }
}
