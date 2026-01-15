// Copyright (c) Microsoft. All rights reserved.
// Workflow Lab - Azure OpenAI Client Factory

// ============================================================================
// EXERCISE 1: Configure Azure OpenAI Client
// ============================================================================
// This file creates the Azure OpenAI client with multiple authentication options.
// Complete the TODO sections to enable AI functionality.
// ============================================================================

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
    /// Gets the configuration, loading from appsettings.json and environment variables.
    /// </summary>
    private static IConfiguration Configuration => _configuration ??= new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    /// <summary>
    /// Creates an Azure OpenAI chat client with support for multiple authentication methods.
    /// </summary>
    public static IChatClient CreateChatClient()
    {
        // ============================================================================
        // STEP 1.1: Get endpoint from configuration
        // Uncomment the lines below to read the endpoint
        // ============================================================================
        // var endpoint = GetConfigValue("AzureOpenAI:Endpoint", "AZURE_OPENAI_ENDPOINT")
        //     ?? throw new InvalidOperationException(
        //         "Azure OpenAI endpoint is not configured. " +
        //         "Set 'AzureOpenAI:Endpoint' in appsettings.json or 'AZURE_OPENAI_ENDPOINT' environment variable.");

        // Placeholder - REMOVE after uncommenting above
        var endpoint = "https://YOUR-RESOURCE.openai.azure.com/";

        // ============================================================================
        // STEP 1.2: Get deployment name from configuration
        // Uncomment the line below
        // ============================================================================
        // var deploymentName = GetConfigValue("AzureOpenAI:DeploymentName", "AZURE_OPENAI_DEPLOYMENT_NAME") 
        //     ?? "gpt-4o-mini";

        // Placeholder - REMOVE after uncommenting above
        var deploymentName = "gpt-4o-mini";

        // ============================================================================
        // STEP 1.3: Create the client with authentication
        // Uncomment the appropriate authentication block below
        // ============================================================================

        // Option 1: API Key authentication
        // var apiKey = GetConfigValue("AzureOpenAI:ApiKey", "AZURE_OPENAI_API_KEY");
        // if (!string.IsNullOrEmpty(apiKey))
        // {
        //     Console.WriteLine("Using API Key authentication");
        //     return new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey))
        //         .GetChatClient(deploymentName)
        //         .AsIChatClient();
        // }

        // Option 2: Service Principal authentication
        // var tenantId = GetConfigValue("AzureOpenAI:TenantId", "AZURE_TENANT_ID");
        // var clientId = GetConfigValue("AzureOpenAI:ClientId", "AZURE_CLIENT_ID");
        // var clientSecret = GetConfigValue("AzureOpenAI:ClientSecret", "AZURE_CLIENT_SECRET");
        // if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        // {
        //     Console.WriteLine("Using Service Principal authentication");
        //     var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        //     return new AzureOpenAIClient(new Uri(endpoint), credential)
        //         .GetChatClient(deploymentName)
        //         .AsIChatClient();
        // }

        // Option 3: Managed Identity / DefaultAzureCredential (fallback)
        // Console.WriteLine("Using Managed Identity / DefaultAzureCredential authentication");
        // return new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
        //     .GetChatClient(deploymentName)
        //     .AsIChatClient();

        // Placeholder - REMOVE after uncommenting above
        throw new NotImplementedException("Exercise 1 not completed. Please uncomment the authentication code above.");
    }

    /// <summary>
    /// Gets a configuration value, checking appsettings.json key first, then environment variable.
    /// </summary>
    private static string? GetConfigValue(string configKey, string envVarName)
    {
        return Configuration[envVarName] ?? Configuration[configKey];
    }
}
