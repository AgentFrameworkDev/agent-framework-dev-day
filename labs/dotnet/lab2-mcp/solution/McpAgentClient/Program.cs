// Copyright (c) Microsoft. All rights reserved.
// MCP Workshop - Agent Client Demo
// This demonstrates how an AI Agent can consume both Local and Remote MCP servers

using System.ClientModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;

Console.WriteLine("================================================================");
Console.WriteLine("           MCP Workshop - Agent Client Demo                    ");
Console.WriteLine("      Demonstrating Local and Remote MCP Servers              ");
Console.WriteLine("================================================================");
Console.WriteLine();

var configPath = FindConfigPath(AppContext.BaseDirectory);
Console.WriteLine($"📁 Config path: {configPath}");
Console.WriteLine($"📄 Config file: {Path.Combine(configPath, "appsettings.Local.json")}");
Console.WriteLine();

// Build configuration from appsettings.Local.json and environment variables
// The config file is copied to the output directory during build
var configuration = new ConfigurationBuilder()
    .SetBasePath(configPath)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

// Get Azure OpenAI configuration (environment variables take precedence over appsettings.json)
var endpoint = configuration["AZURE_OPENAI_ENDPOINT"]
    ?? configuration["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("Azure OpenAI endpoint is not set. Set AZURE_OPENAI_ENDPOINT environment variable or AzureOpenAI:Endpoint in appsettings.json.");

var deploymentName = configuration["AZURE_OPENAI_DEPLOYMENT_NAME"]
    ?? configuration["AZURE_AI_MODEL_DEPLOYMENT_NAME"]
    ?? configuration["AzureOpenAI:DeploymentName"]
    ?? "gpt-4o-mini";

// Create Azure OpenAI client with appropriate authentication
var azureOpenAIClient = CreateAzureOpenAIClient(configuration, endpoint);

Console.WriteLine($"Using Azure OpenAI endpoint: {endpoint}");
Console.WriteLine($"Deployment: {deploymentName}");
Console.WriteLine();

// Menu for demo selection
bool running = true;
while (running)
{
    Console.WriteLine("===================================================================");
    Console.WriteLine("Select a demo to run:");
    Console.WriteLine("  1. Local MCP Server (.NET EXE via STDIO)");
    Console.WriteLine("  2. Remote MCP Server (HTTP/SSE)");
    Console.WriteLine("  3. Exit");
    Console.WriteLine("===================================================================");
    Console.Write("Enter choice (1-3): ");

    var choice = Console.ReadLine()?.Trim();
    Console.WriteLine();

    bool returnToMenu = true;
    
    try
    {
        switch (choice)
        {
            case "1":
                returnToMenu = await DemoLocalDotNetMcp(configuration, endpoint, deploymentName);
                break;
            case "2":
                returnToMenu = await DemoRemoteMcp(configuration, endpoint, deploymentName);
                break;
            case "3":
                running = false;
                continue;
            default:
                Console.WriteLine("Invalid choice. Please enter 1-3.");
                continue;
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.WriteLine($"   Stack: {ex.StackTrace}");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
        Console.Clear();
        continue;
    }

    // Exit application if user typed 'exit' or 'quit' in the session
    if (!returnToMenu)
    {
        running = false;
        continue;
    }

    // Clear screen and show menu again
    Console.Clear();
}

Console.WriteLine("Goodbye!");

// Create Azure OpenAI client with the appropriate authentication method based on configuration.
// Supports: API Key, Service Principal (ClientSecretCredential), Managed Identity, and Azure CLI fallback.
// Configuration priority: Environment variables > appsettings.json
static AzureOpenAIClient CreateAzureOpenAIClient(IConfiguration configuration, string endpoint)
{
    // Get values from environment variables first, then fall back to appsettings.json
    var apiKey = configuration["AZURE_OPENAI_API_KEY"] ?? configuration["AzureOpenAI:ApiKey"];
    var tenantId = configuration["AZURE_TENANT_ID"] ?? configuration["AzureOpenAI:TenantId"];
    var clientId = configuration["AZURE_CLIENT_ID"] ?? configuration["AzureOpenAI:ClientId"];
    var clientSecret = configuration["AZURE_CLIENT_SECRET"] ?? configuration["AzureOpenAI:ClientSecret"];
    var useManagedIdentity = configuration["AZURE_USE_MANAGED_IDENTITY"] ?? configuration["AzureOpenAI:UseManagedIdentity"];
    var managedIdentityClientId = configuration["AZURE_MANAGED_IDENTITY_CLIENT_ID"] ?? configuration["AzureOpenAI:ManagedIdentityClientId"];

    // Option 1: API Key authentication
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        Console.WriteLine("Authentication: API Key");
        return new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
    }

    // Option 2: Service Principal (Client Secret) authentication
    if (!string.IsNullOrWhiteSpace(tenantId) &&
        !string.IsNullOrWhiteSpace(clientId) &&
        !string.IsNullOrWhiteSpace(clientSecret))
    {
        Console.WriteLine("Authentication: Service Principal (ClientSecretCredential)");
        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        return new AzureOpenAIClient(new Uri(endpoint), credential);
    }

    // Option 3: Managed Identity authentication
    if (!string.IsNullOrWhiteSpace(useManagedIdentity) &&
        useManagedIdentity.Equals("true", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Authentication: Managed Identity");
        // If ManagedIdentityClientId is provided, use user-assigned managed identity; otherwise, use system-assigned
        var credential = !string.IsNullOrWhiteSpace(managedIdentityClientId)
            ? new ManagedIdentityCredential(managedIdentityClientId)
            : new ManagedIdentityCredential();
        return new AzureOpenAIClient(new Uri(endpoint), credential);
    }

    // Option 4: Default - Azure CLI credential (for local development)
    Console.WriteLine("Authentication: Azure CLI (default for local development)");
    return new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential());
}

/// <summary>
/// Demo 1: Local .NET MCP Server via STDIO
/// </summary>
/// <returns>True to return to menu, false to exit application</returns>
static async Task<bool> DemoLocalDotNetMcp(IConfiguration configuration, string endpoint, string deploymentName)
{
    Console.WriteLine("===============================================================");
    Console.WriteLine("       Demo 1: Local .NET MCP Server (STDIO Transport)        ");
    Console.WriteLine("===============================================================");
    Console.WriteLine();

    Console.WriteLine("Connecting to Local .NET MCP Server...");

    // Get the solution directory (4 levels up from bin/Debug/net10.0)
    var solutionDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    var mcpLocalProject = Path.Combine(solutionDir, "McpLocal", "McpLocal.csproj");

    // Create MCP client for local .NET server using STDIO transport
    var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
    {
        Name = "LocalDotNetMcpServer",
        Command = "dotnet",
        Arguments = ["run", "--project", mcpLocalProject],
    });

    await using var mcpClient = await McpClient.CreateAsync(clientTransport);

    Console.WriteLine("Connected to Local .NET MCP Server");

    // List available tools
    var tools = await mcpClient.ListToolsAsync();
    Console.WriteLine($"Available tools ({tools.Count}):");
    foreach (var tool in tools)
    {
        Console.WriteLine($"   - {tool.Name}: {tool.Description}");
    }
    Console.WriteLine();

    // Create IChatClient with MCP tools for function invocation
    // Use AsIChatClient() to convert Azure.AI.OpenAI ChatClient to Microsoft.Extensions.AI IChatClient
    var chatClient = CreateAzureOpenAIClient(configuration, endpoint)
        .GetChatClient(deploymentName)
        .AsIChatClient()
        .AsBuilder()
        .UseFunctionInvocation()
        .Build();

    // Create ChatOptions with MCP tools
    var chatOptions = new ChatOptions
    {
        Tools = [.. tools.Select(t => (AITool)t)]
    };

    // Interactive session
    return await RunInteractiveSession(chatClient, chatOptions, "Local .NET MCP");
}

/// <summary>
/// Demo 2: Remote MCP Server via HTTP/SSE (MCP Bridge -> REST API)
/// </summary>
/// <returns>True to return to menu, false to exit application</returns>
static async Task<bool> DemoRemoteMcp(IConfiguration configuration, string endpoint, string deploymentName)
{
    Console.WriteLine("===============================================================");
    Console.WriteLine("      Demo 2: Remote MCP Bridge (HTTP -> REST API)            ");
    Console.WriteLine("===============================================================");
    Console.WriteLine();
    Console.WriteLine("Architecture:");
    Console.WriteLine("   AgentClient -> MCP Bridge (:5070) -> REST API (:5060)");
    Console.WriteLine();

    Console.WriteLine("Connecting to MCP Bridge at http://localhost:5070/mcp...");
    Console.WriteLine("   (Make sure both REST API :5060 and MCP Bridge :5070 are running)");

    // Create MCP client for MCP Bridge using HTTP transport (Streamable HTTP)
    var clientTransport = new HttpClientTransport(new HttpClientTransportOptions
    {
        Endpoint = new Uri("http://localhost:5070/mcp"),
    });

    await using var mcpClient = await McpClient.CreateAsync(clientTransport);

    Console.WriteLine("Connected to MCP Bridge");

    // List available tools
    var tools = await mcpClient.ListToolsAsync();
    Console.WriteLine($"Available tools ({tools.Count}):");
    foreach (var tool in tools)
    {
        Console.WriteLine($"   - {tool.Name}: {tool.Description}");
    }
    Console.WriteLine();

    // Create IChatClient with MCP tools for function invocation
    // Use AsIChatClient() to convert Azure.AI.OpenAI ChatClient to Microsoft.Extensions.AI IChatClient
    var chatClient = CreateAzureOpenAIClient(configuration, endpoint)
        .GetChatClient(deploymentName)
        .AsIChatClient()
        .AsBuilder()
        .UseFunctionInvocation()
        .Build();

    // Create ChatOptions with MCP tools
    var chatOptions = new ChatOptions
    {
        Tools = [.. tools.Select(t => (AITool)t)]
    };

    // Interactive session
    return await RunInteractiveSession(chatClient, chatOptions, "Remote MCP Bridge");
}

/// <summary>
/// Run an interactive session with the chat client
/// </summary>
/// <returns>True to return to menu, false to exit application</returns>
static async Task<bool> RunInteractiveSession(IChatClient chatClient, ChatOptions chatOptions, string serverName)
{
    Console.WriteLine($"Starting interactive session with {serverName}");
    Console.WriteLine("   Type 'back' to return to the main menu");
    Console.WriteLine("   Type 'exit' or 'quit' to exit the application");
    Console.WriteLine("   Example prompts:");
    Console.WriteLine("   - Get all tickets");
    Console.WriteLine("   - What is the status of TICKET-001?");
    Console.WriteLine("   - Update TICKET-002 status to Resolved");
    Console.WriteLine();

    // Maintain conversation history with system message
    var conversation = new List<ChatMessage>
    {
        new(ChatRole.System, "You are a support ticket management assistant. Help users get and update support tickets using the available MCP tools.")
    };

    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("You: ");
        Console.ResetColor();
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(input))
            continue;

        if (input.Equals("back", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Returning to main menu...");
            return true; // Return to menu
        }

        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("quit", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Exiting application...");
            return false; // Exit application
        }

        try
        {
            // Add user message to conversation
            conversation.Add(new ChatMessage(ChatRole.User, input));

            Console.WriteLine();
            var response = await chatClient.GetResponseAsync(conversation, chatOptions);
            
            // Add assistant response to conversation for context
            conversation.Add(new ChatMessage(ChatRole.Assistant, response.Text ?? ""));
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Agent: ");
            Console.ResetColor();
            Console.WriteLine(response.Text);
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
        }
    }
}
static string FindConfigPath(string startPath)
{
    var currentDir = new DirectoryInfo(startPath);

    // Traverse up to find the 'dotnet' folder
    while (currentDir != null)
    {
        if (currentDir.Name.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        {
            // Found dotnet folder, return path with appsettings.Local.json
            return currentDir.FullName;
        }

        currentDir = currentDir.Parent;
    }

    // Fallback to start path if dotnet folder not found
    return startPath;
}