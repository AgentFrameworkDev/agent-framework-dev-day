using Azure.AI.Projects;
using Azure.Core;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;

namespace Lab0;

public static class Program
{
    public static async Task Main(string[] args)
    {
        // Build configuration with priority: Environment Variables > appsettings.Local.json
        // Config file is in the parent directory (labs/dotnet/)
        var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var configPath = Path.Combine(basePath, "appsettings.Local.json");
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        // Get configuration values (env vars take precedence)
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_AGENT_ENDPOINT")
            ?? configuration["AzureAI:ProjectEndpoint"]
            ?? throw new InvalidOperationException("Azure AI endpoint not configured. Set AZURE_OPENAI_AGENT_ENDPOINT env var or AzureAI:ProjectEndpoint in appsettings.Local.json");

        var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT")
            ?? configuration["AzureAI:ModelDeploymentName"]
            ?? throw new InvalidOperationException("Azure AI deployment not configured. Set AZURE_OPENAI_DEPLOYMENT env var or AzureAI:ModelDeploymentName in appsettings.Local.json");

        // Get optional service principal credentials from config
        var tenantId = configuration["Azure:TenantId"];
        var clientId = configuration["Azure:ClientId"];
        var clientSecret = configuration["Azure:ClientSecret"];

        Console.WriteLine("Hello World Agent Framework Application");
        Console.WriteLine("========================================");
        Console.WriteLine($"Endpoint: {endpoint}");
        Console.WriteLine($"Deployment: {deployment}");
        Console.WriteLine();

        // Create credential - use service principal if configured, otherwise fall back to DefaultAzureCredential
        // DefaultAzureCredential will try: Environment, Managed Identity, Visual Studio, Azure CLI, etc.
        TokenCredential credential = !string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret)
            ? new ClientSecretCredential(tenantId, clientId, clientSecret)
            : new DefaultAzureCredential();

        Console.WriteLine($"Auth: {(credential is ClientSecretCredential ? "Service Principal" : "Default Azure Credential")}");
        Console.WriteLine();

        // Create client to interact with Azure AI Foundry
        var aiProjectClient = new AIProjectClient(new Uri(endpoint), credential);

        // Create an agent with simple instructions
        const string agentName = "HelloWorldAgent";
        const string agentInstructions = "You are a friendly assistant that gives concise responses.";

        Console.WriteLine("Creating agent...");
        AIAgent agent = await aiProjectClient.CreateAIAgentAsync(
            name: agentName,
            model: deployment,
            instructions: agentInstructions
        );
        Console.WriteLine($"Agent '{agentName}' created successfully!");
        Console.WriteLine();

        try
        {
            // Send a simple prompt and get response
            const string prompt = "Tell me one a joke or fact about .NET or Python!";
            Console.WriteLine($"User: {prompt}");
            Console.WriteLine();

            // Run the agent (non-streaming for simplicity and to get token usage)
            var response = await agent.RunAsync(prompt);

            Console.WriteLine($"Agent: {response}");
            Console.WriteLine();

            // Display token usage information
            Console.WriteLine("========================================");
            Console.WriteLine("Token Usage:");
            if (response.Usage != null)
            {
                Console.WriteLine($"  Input Tokens:  {response.Usage.InputTokenCount}");
                Console.WriteLine($"  Output Tokens: {response.Usage.OutputTokenCount}");
                Console.WriteLine($"  Total Tokens:  {response.Usage.TotalTokenCount}");
            }
            else
            {
                Console.WriteLine("  Token usage information not available");
            }
        }
        finally
        {
            // Cleanup - delete the agent
            Console.WriteLine();
            Console.WriteLine("Cleaning up agent...");
            await aiProjectClient.Agents.DeleteAgentAsync(agent.Name);
            Console.WriteLine("Agent deleted successfully!");
        }
    }
}
