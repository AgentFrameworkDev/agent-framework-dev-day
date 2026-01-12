using System.Diagnostics;
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
      var basePath = FindConfigDirectory("appsettings.Local.json")
          ?? throw new InvalidOperationException("Could not find appsettings.Local.json in any parent directory");

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

      Console.WriteLine("Welcome to Agent Framework Dev Day!");
      Console.WriteLine("===================================");
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

      // Randomly select a model from available deployments
      var (selectedModel, underlyingModelName) = await GetRandomAvailableModelAsync(aiProjectClient);
      Console.WriteLine($"Randomly selected model: {selectedModel} ({underlyingModelName})");

      // Create an agent with simple instructions
      const string agentName = "HelloWorldAgent";
      const string agentInstructions = "You are a friendly assistant that gives concise responses.";

      Console.WriteLine("Creating agent...");
      AIAgent agent = await aiProjectClient.CreateAIAgentAsync(
          name: agentName,
          model: selectedModel,
          instructions: agentInstructions
      );
      Console.WriteLine($"Agent '{agentName}' created successfully! using model '{selectedModel}'");
      Console.WriteLine();

      double elapsedSeconds = 0;
      try
      {
         // Send a simple prompt and get response
         // const string prompt = "Tell me one a joke or fact about .NET or Python!";
         const string prompt = "Tell me one a joke OR interesting fact about .NET or Python!";
         Console.WriteLine($"User: {prompt}");
         Console.WriteLine();

         // Run the agent (non-streaming for simplicity and to get token usage)
         var stopwatch = Stopwatch.StartNew();
         var response = await agent.RunAsync(prompt);
         stopwatch.Stop();
         elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

         Console.WriteLine($"Agent: {response}");
         Console.WriteLine();

         // Display token usage information
         Console.WriteLine("========================================");
         Console.WriteLine("Token Usage:");
         if (response.Usage != null)
         {
            Console.WriteLine($"  {response.Usage.InputTokenCount,6} Input Tokens");
            Console.WriteLine($"+ {response.Usage.OutputTokenCount,6} Output Tokens (including {response.Usage.ReasoningTokenCount} Reasoning Tokens)");
            Console.WriteLine($"= {response.Usage.TotalTokenCount,6} Total Tokens");
         }
         else
         {
            Console.WriteLine("  Token usage information not available");
         }

         // Output the models used
         Console.WriteLine();
         Console.WriteLine("Models Used:");
         Console.WriteLine("========================================");
      }
      finally
      {
         // Cleanup - delete the agent
         Console.WriteLine();
         Console.WriteLine("Cleaning up agent...");
         await aiProjectClient.Agents.DeleteAgentAsync(agent.Name);
         Console.WriteLine("Agent deleted successfully!");

         Console.WriteLine($"The model used by the agent was: {selectedModel} ({underlyingModelName}) and took {elapsedSeconds:F2} seconds.");
      }

      // // List available model deployments
      // Console.WriteLine();
      // await ListAvailableModelsAsync(aiProjectClient);

      // // Get a random available model deployment
      // var randomModel = await GetRandomAvailableModelAsync(aiProjectClient);
      // Console.WriteLine();
      // Console.WriteLine($"Randomly selected model deployment: {randomModel}");
   }

   /// <summary>
   /// Searches for a configuration file starting from the current directory and walking up parent directories.
   /// </summary>
   private static string? FindConfigDirectory(string fileName)
   {
      var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

      while (directory != null)
      {
         var configPath = Path.Combine(directory.FullName, fileName);
         if (File.Exists(configPath))
         {
            return directory.FullName;
         }
         directory = directory.Parent;
      }

      return null;
   }

   /// <summary>
   /// Lists available model deployments in the Azure AI Foundry project.
   /// </summary>
   private static async Task ListAvailableModelsAsync(AIProjectClient client)
   {
      Console.WriteLine("Available Model Deployments:");
      Console.WriteLine(new string('-', 50));

      await foreach (var deployment in client.Deployments.GetDeploymentsAsync())
      {
         if (deployment is ModelDeployment model)
         {
            Console.WriteLine($"  {model.Name,-30} {model.ModelName}");
         }
      }
   }

   /// <summary>
   /// Randomly selects and returns one deployment name (model.Name) and corresponding underlying model name (model.ModelName) from among those provisioned
   /// at <param>client</param> Foundry project.
   /// </summary>
   private static async Task<(string DeploymentName, string ModelName)> GetRandomAvailableModelAsync(AIProjectClient client)
   {
      var availableModels = new List<(string DeploymentName, string ModelName)>();

      await foreach (var deployment in client.Deployments.GetDeploymentsAsync())
      {
         if (deployment is ModelDeployment model)
         {
            availableModels.Add((model.Name, model.ModelName));
         }
      }

      if (availableModels.Count == 0)
      {
         throw new InvalidOperationException("No model deployments available in the Azure AI Foundry project.");
      }

      return availableModels[Random.Shared.Next(availableModels.Count)];
   }
}
