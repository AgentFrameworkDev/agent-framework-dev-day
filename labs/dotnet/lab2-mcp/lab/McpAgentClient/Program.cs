// Copyright (c) Microsoft. All rights reserved.
// MCP Workshop - Agent Client Demo
// This demonstrates how an AI Agent can consume MCP servers

// ============================================================================
// EXERCISES 1, 4, 5: MCP Agent Client
// ============================================================================
// This file contains exercises for:
// - Exercise 1: Configure Azure OpenAI credentials
// - Exercise 4: Connect to Local MCP Server and create AI Agent
// - Exercise 5: Connect to Remote MCP Server (HTTP/SSE)
//
// Follow the instructions in EXERCISES.md to complete each exercise.
// ============================================================================

using System.ClientModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;

Console.WriteLine("================================================================");
Console.WriteLine("           MCP Workshop - Agent Client Demo                    ");
Console.WriteLine("      Demonstrating Local and Remote MCP Servers              ");
Console.WriteLine("================================================================");
Console.WriteLine();

// ============================================================================
// EXERCISE 1: Configure Azure OpenAI
// ============================================================================
// STEP 1.1: Load configuration from appsettings.Local.json
// The FindConfigPath helper finds the dotnet folder where config is stored
// ============================================================================
var configPath = FindConfigPath(AppContext.BaseDirectory);
var configuration = new ConfigurationBuilder()
    .SetBasePath(configPath)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

// ============================================================================
// STEP 1.2: Get Azure OpenAI endpoint and deployment name
// Uncomment the lines below to read from configuration
// ============================================================================
// var endpoint = configuration["AZURE_OPENAI_ENDPOINT"]
//     ?? configuration["AzureOpenAI:Endpoint"]
//     ?? throw new InvalidOperationException("Azure OpenAI endpoint is not set.");
// 
// var deploymentName = configuration["AZURE_OPENAI_DEPLOYMENT_NAME"]
//     ?? configuration["AzureOpenAI:DeploymentName"]
//     ?? "gpt-4o-mini";

// Placeholder values - REPLACE after uncommenting above
var endpoint = "https://YOUR-RESOURCE.openai.azure.com/";
var deploymentName = "gpt-4o-mini";

// ============================================================================
// STEP 1.3: Create Azure OpenAI client
// Uncomment the line below to create the client with authentication
// ============================================================================
// var azureOpenAIClient = CreateAzureOpenAIClient(configuration, endpoint);

Console.WriteLine($"Using Azure OpenAI endpoint: {endpoint}");
Console.WriteLine($"Deployment: {deploymentName}");
Console.WriteLine();

// ============================================================================
// Main Menu - Demo Selection
// ============================================================================
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

    bool continueToMenu = true;
    
    try
    {
        switch (choice)
        {
            case "1":
                // ============================================================================
                // EXERCISE 4: Uncomment to enable Local MCP Demo
                // ============================================================================
                // continueToMenu = await DemoLocalDotNetMcp(configuration, endpoint, deploymentName);
                Console.WriteLine("Exercise 4 not completed. Please uncomment the code in Program.cs");
                break;
            case "2":
                // ============================================================================
                // EXERCISE 5: Uncomment to enable Remote MCP Demo
                // ============================================================================
                // continueToMenu = await DemoRemoteMcp(configuration, endpoint, deploymentName);
                Console.WriteLine("Exercise 5 not completed. Please uncomment the code in Program.cs");
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

    if (continueToMenu)
    {
        running = false;
        continue;
    }

    Console.Clear();
}

Console.WriteLine("Goodbye!");

// ============================================================================
// HELPER: Create Azure OpenAI Client
// This method supports multiple authentication methods
// ============================================================================
static AzureOpenAIClient CreateAzureOpenAIClient(IConfiguration configuration, string endpoint)
{
    var apiKey = configuration["AZURE_OPENAI_API_KEY"] ?? configuration["AzureOpenAI:ApiKey"];
    var tenantId = configuration["AZURE_TENANT_ID"] ?? configuration["AzureOpenAI:TenantId"];
    var clientId = configuration["AZURE_CLIENT_ID"] ?? configuration["AzureOpenAI:ClientId"];
    var clientSecret = configuration["AZURE_CLIENT_SECRET"] ?? configuration["AzureOpenAI:ClientSecret"];

    // Option 1: API Key authentication
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        Console.WriteLine("Authentication: API Key");
        return new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
    }

    // Option 2: Service Principal authentication
    if (!string.IsNullOrWhiteSpace(tenantId) &&
        !string.IsNullOrWhiteSpace(clientId) &&
        !string.IsNullOrWhiteSpace(clientSecret))
    {
        Console.WriteLine("Authentication: Service Principal");
        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        return new AzureOpenAIClient(new Uri(endpoint), credential);
    }

    // Option 3: Default - Azure CLI credential
    Console.WriteLine("Authentication: Azure CLI (default)");
    return new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential());
}

// ============================================================================
// EXERCISE 4: Local .NET MCP Server via STDIO
// ============================================================================
// Uncomment this entire method to enable the Local MCP demo
// ============================================================================
// static async Task<bool> DemoLocalDotNetMcp(IConfiguration configuration, string endpoint, string deploymentName)
// {
//     Console.WriteLine("===============================================================");
//     Console.WriteLine("       Demo 1: Local .NET MCP Server (STDIO Transport)        ");
//     Console.WriteLine("===============================================================");
//     Console.WriteLine();
// 
//     Console.WriteLine("Connecting to Local .NET MCP Server...");
// 
//     // STEP 4.1: Get the path to McpLocal project
//     var solutionDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
//     var mcpLocalProject = Path.Combine(solutionDir, "McpLocal", "McpLocal.csproj");
// 
//     // STEP 4.2: Create MCP client with STDIO transport
//     var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
//     {
//         Name = "LocalDotNetMcpServer",
//         Command = "dotnet",
//         Arguments = ["run", "--project", mcpLocalProject],
//     });
// 
//     await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport);
//     Console.WriteLine("Connected to Local .NET MCP Server");
// 
//     // STEP 4.3: List available tools
//     var tools = await mcpClient.ListToolsAsync();
//     Console.WriteLine($"Available tools ({tools.Count}):");
//     foreach (var tool in tools)
//     {
//         Console.WriteLine($"   - {tool.Name}: {tool.Description}");
//     }
//     Console.WriteLine();
// 
//     // STEP 4.4: Create AI Agent with MCP tools
//     AIAgent agent = CreateAzureOpenAIClient(configuration, endpoint)
//         .GetChatClient(deploymentName)
//         .AsIChatClient()
//         .CreateAIAgent(
//             instructions: "You are a configuration management assistant. Help users get and update configurations using the available MCP tools.",
//             tools: [.. tools.Cast<AITool>()]);
// 
//     // STEP 4.5: Run interactive session
//     return await RunInteractiveSession(agent, "Local .NET MCP");
// }

// ============================================================================
// EXERCISE 5: Remote MCP Server via HTTP/SSE
// ============================================================================
// Uncomment this entire method to enable the Remote MCP demo
// ============================================================================
// static async Task<bool> DemoRemoteMcp(IConfiguration configuration, string endpoint, string deploymentName)
// {
//     Console.WriteLine("===============================================================");
//     Console.WriteLine("      Demo 2: Remote MCP Bridge (HTTP/SSE -> REST API)        ");
//     Console.WriteLine("===============================================================");
//     Console.WriteLine();
//     Console.WriteLine("Architecture:");
//     Console.WriteLine("   AgentClient -> MCP Bridge (:5070) -> REST API (:5060)");
//     Console.WriteLine();
// 
//     Console.WriteLine("Connecting to MCP Bridge at http://localhost:5070/sse...");
//     Console.WriteLine("   (Make sure both REST API :5060 and MCP Bridge :5070 are running)");
// 
//     // STEP 5.1: Create MCP client with SSE transport
//     var clientTransport = new SseClientTransport(new SseClientTransportOptions
//     {
//         Name = "McpBridge",
//         Endpoint = new Uri("http://localhost:5070/sse"),
//     });
// 
//     await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport);
//     Console.WriteLine("Connected to MCP Bridge");
// 
//     // STEP 5.2: List available tools
//     var tools = await mcpClient.ListToolsAsync();
//     Console.WriteLine($"Available tools ({tools.Count}):");
//     foreach (var tool in tools)
//     {
//         Console.WriteLine($"   - {tool.Name}: {tool.Description}");
//     }
//     Console.WriteLine();
// 
//     // STEP 5.3: Create AI Agent with MCP tools
//     AIAgent agent = CreateAzureOpenAIClient(configuration, endpoint)
//         .GetChatClient(deploymentName)
//         .AsIChatClient()
//         .CreateAIAgent(
//             instructions: "You are a support ticket management assistant. Help users manage tickets using the available MCP tools.",
//             tools: [.. tools.Cast<AITool>()]);
// 
//     // STEP 5.4: Run interactive session
//     return await RunInteractiveSession(agent, "Remote MCP Bridge");
// }

// ============================================================================
// HELPER: Interactive chat session with the agent
// ============================================================================
static async Task<bool> RunInteractiveSession(AIAgent agent, string serverName)
{
    Console.WriteLine($"Starting interactive session with {serverName}");
    Console.WriteLine("   Type 'back' to return to the main menu");
    Console.WriteLine("   Type 'exit' or 'quit' to exit the application");
    Console.WriteLine("   Example prompts:");
    Console.WriteLine("   - Get all configurations");
    Console.WriteLine("   - What is the value of app.name?");
    Console.WriteLine("   - Update feature.darkMode to true");
    Console.WriteLine();

    var thread = agent.GetNewThread();

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
            return true;
        }

        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("quit", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Exiting application...");
            return false;
        }

        try
        {
            Console.WriteLine();
            var response = await agent.RunAsync(input, thread);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Agent: ");
            Console.ResetColor();
            Console.WriteLine(response);
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

// ============================================================================
// HELPER: Find the configuration file path
// ============================================================================
static string FindConfigPath(string startPath)
{
    var currentDir = new DirectoryInfo(startPath);

    while (currentDir != null)
    {
        if (currentDir.Name.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        {
            return currentDir.FullName;
        }
        currentDir = currentDir.Parent;
    }

    return startPath;
}
