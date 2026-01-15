using System.ClientModel;
using System.Diagnostics;
using AgentFrameworkDev.Config;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;

namespace Lab0;

public static class Program
{
   public static async Task Main(string[] args)
   {
      // Parse command-line arguments
      var verbose = args.Any(a => string.Equals(a, "--verbose", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "-v", StringComparison.OrdinalIgnoreCase));
      var force = args.Any(a => string.Equals(a, "--force", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "--overwrite", StringComparison.OrdinalIgnoreCase));

      // --------- FIRST STEP ----------
      // ASK LAB INSTRUCTOR FOR THE PASSWORD
      var password = "𝒜𝒮𝒦 𝒴𝒪𝒰ℛ ℒ𝒜ℬ ℐ𝒩𝒮𝒯ℛ𝒰𝒞𝒯𝒪ℛ ℱ𝒪ℛ 𝒯ℋℰ 𝒫𝒜𝒮𝒮𝒲𝒪ℛ𝒟";
      // LAB STEP 1: CHANGE THE PASSWORD

      var configger = new ConfigureLabKeys(password, verbose);
      configger.RandomizeDecryptDistribute(overwriteExisting: force);

      // Load configuration and create client using the factory
      var config = FoundryClientFactory.GetConfiguration();
      var aiProjectClient = FoundryClientFactory.CreateProjectClient(config);

      Console.WriteLine("Welcome to Agent Framework Dev Day!");
      Console.WriteLine("===================================");
      Console.WriteLine($"Endpoint: {config.Endpoint}");
      Console.WriteLine($"Deployment: {config.DeploymentName}");
      Console.WriteLine($"Auth: {(config.Credential is ClientSecretCredential ? "Service Principal" : "Default Azure Credential")}");
      Console.WriteLine();

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
      catch (ClientResultException ex)
      {
         Console.WriteLine("An error occurred while calling the Foundry AI service.");
         Console.WriteLine($"Message: {ex.Message}");

         var raw = ex.GetRawResponse();
         if (raw is not null)
         {
            Console.WriteLine($"Reason phrase: {raw.ReasonPhrase}");
            Console.WriteLine($"Error body: {raw.Content}");
            Console.WriteLine("——— Response headers ———");
            foreach (var kv in raw.Headers)
            {
               Console.WriteLine($"{kv.Key}: {kv.Value}");
            }
         }
      }
      catch (Exception ex)
      {
         Console.WriteLine("An error occurred while running the agent:");
         Console.WriteLine(ex.ToString());
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
