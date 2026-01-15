using System.ClientModel;
using System.Diagnostics;
using AgentFrameworkDev.Config;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;

namespace Lab0;

/// <summary>
/// Represents the result of running a model with metrics.
/// </summary>
public record ModelResult(
   string DeploymentName,
   string ModelName,
   bool Success,
   long InputTokens,
   long OutputTokens,
   long InferenceTokens,
   long TotalTokens,
   double Seconds,
   string Response,
   string? ErrorMessage = null
);

public static class Program
{
   private const string Prompt = "Summarize the vibe of programming in .NET or C# in 2026 in no more than 3 words";
   private const string AgentInstructions = "You are a friendly assistant that gives concise responses.";

   public static async Task Main(string[] args)
   {
      // Parse command-line arguments
      var verbose = args.Any(a => string.Equals(a, "--verbose", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "-v", StringComparison.OrdinalIgnoreCase));
      var force = args.Any(a => string.Equals(a, "--force", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "--overwrite", StringComparison.OrdinalIgnoreCase));

      // --------- FIRST STEP ----------
      // ASK LAB INSTRUCTOR FOR THE PASSWORD
      var password = "𝒜𝒮𝒦 𝒴𝒪𝒰ℛ ℒ𝒜ℬ ℐ𝒩𝒮𝒯ℛ𝒰𝒞𝒯𝒪ℛ ℱ𝒪ℛ 𝒯ℋℰ 𝒫𝒜𝒮𝒮𝒲𝒪ℛ𝒟";

      // LAB STEP 1: CHANGE THE PASSWORD
   //  password = "replace this with the real password given by your lab instructor";

      var configger = new ConfigureLabKeys(password, verbose);
      configger.RandomizeDecryptDistribute(overwriteExisting: force);

      // Load configuration and create client using the factory
      var config = FoundryClientFactory.GetConfiguration();
      var aiProjectClient = FoundryClientFactory.CreateProjectClient(config);

      Console.WriteLine("Welcome to Agent Framework Dev Day!");
      Console.WriteLine("===================================");
      Console.WriteLine($"Endpoint: {config.Endpoint}");
      Console.WriteLine($"Auth: {(config.Credential is ClientSecretCredential ? "Service Principal" : "Default Azure Credential")}");
      Console.WriteLine();
      Console.WriteLine($"Prompt: \"{Prompt}\"");
      Console.WriteLine();

      // Get all chat-capable models (filter out embedding models)
      var chatModels = await GetChatCompletionModelsAsync(aiProjectClient, verbose);
      
      if (chatModels.Count == 0)
      {
         Console.WriteLine("No chat-capable model deployments found.");
         return;
      }

      Console.WriteLine($"Found {chatModels.Count} chat-capable model(s). Running comparison...");
      Console.WriteLine();

      // Run each model sequentially and collect results
      var results = new List<ModelResult>();
      
      foreach (var (deploymentName, modelName) in chatModels)
      {
         var result = await RunModelAsync(aiProjectClient, deploymentName, modelName, verbose);
         results.Add(result);
      }

      // Output the comparison table
      PrintComparisonTable(results);
   }

   /// <summary>
   /// Gets all chat-capable model deployments (filters out embedding models).
   /// </summary>
   private static async Task<List<(string DeploymentName, string ModelName)>> GetChatCompletionModelsAsync(
      AIProjectClient client, bool verbose)
   {
      var chatModels = new List<(string DeploymentName, string ModelName)>();
      var skippedModels = new List<(string DeploymentName, string ModelName, string Reason)>();

      await foreach (var deployment in client.Deployments.GetDeploymentsAsync())
      {
         if (deployment is ModelDeployment model)
         {
            var nameLower = model.Name.ToLowerInvariant();
            var modelNameLower = model.ModelName?.ToLowerInvariant() ?? "";

            // Skip embedding models
            if (nameLower.Contains("embedding") || modelNameLower.Contains("embedding"))
            {
               skippedModels.Add((model.Name, model.ModelName ?? "unknown", "embedding model"));
               continue;
            }

            // Skip other non-chat models (whisper, dall-e, tts, etc.)
            if (nameLower.Contains("whisper") || modelNameLower.Contains("whisper") ||
                nameLower.Contains("dall-e") || modelNameLower.Contains("dall-e") ||
                nameLower.Contains("tts") || modelNameLower.Contains("tts"))
            {
               skippedModels.Add((model.Name, model.ModelName ?? "unknown", "non-chat model"));
               continue;
            }

            chatModels.Add((model.Name, model.ModelName ?? "unknown"));
         }
      }

      if (verbose && skippedModels.Count > 0)
      {
         Console.WriteLine("Skipped models:");
         foreach (var (name, underlying, reason) in skippedModels)
         {
            Console.WriteLine($"  {name} ({underlying}) - {reason}");
         }
         Console.WriteLine();
      }

      return chatModels;
   }

   /// <summary>
   /// Runs a single model with the prompt and returns metrics.
   /// </summary>
   private static async Task<ModelResult> RunModelAsync(
      AIProjectClient client, 
      string deploymentName, 
      string modelName,
      bool verbose)
   {
      if (verbose)
      {
         Console.WriteLine($"Running: {deploymentName} ({modelName})...");
      }

      AIAgent? agent = null;
      try
      {
         // Create agent for this model - keep name short (max 63 chars)
         var agentName = $"Cmp{Guid.NewGuid().ToString("N")[..8]}";
         agent = await client.CreateAIAgentAsync(
            name: agentName,
            model: deploymentName,
            instructions: AgentInstructions
         );

         // Run and time the prompt
         var stopwatch = Stopwatch.StartNew();
         var response = await agent.RunAsync(Prompt);
         stopwatch.Stop();

         var inputTokens = response.Usage?.InputTokenCount ?? 0;
         var outputTokens = response.Usage?.OutputTokenCount ?? 0;
         var inferenceTokens = response.Usage?.ReasoningTokenCount ?? 0;
         var totalTokens = response.Usage?.TotalTokenCount ?? 0;
         var seconds = stopwatch.Elapsed.TotalSeconds;
         var responseText = response.ToString()?.Trim() ?? "";

         if (verbose)
         {
            Console.WriteLine($"  Completed in {seconds:F2}s - {responseText}");
         }

         return new ModelResult(
            DeploymentName: deploymentName,
            ModelName: modelName,
            Success: true,
            InputTokens: inputTokens,
            OutputTokens: outputTokens,
            InferenceTokens: inferenceTokens,
            TotalTokens: totalTokens,
            Seconds: seconds,
            Response: responseText
         );
      }
      catch (ClientResultException ex)
      {
         var errorMsg = ex.Message;
         if (verbose)
         {
            Console.WriteLine($"  ERROR: {errorMsg}");
         }
         return new ModelResult(
            DeploymentName: deploymentName,
            ModelName: modelName,
            Success: false,
            InputTokens: 0,
            OutputTokens: 0,
            InferenceTokens: 0,
            TotalTokens: 0,
            Seconds: 0,
            Response: "",
            ErrorMessage: errorMsg
         );
      }
      catch (Exception ex)
      {
         var errorMsg = ex.Message;
         if (verbose)
         {
            Console.WriteLine($"  ERROR: {errorMsg}");
         }
         return new ModelResult(
            DeploymentName: deploymentName,
            ModelName: modelName,
            Success: false,
            InputTokens: 0,
            OutputTokens: 0,
            InferenceTokens: 0,
            TotalTokens: 0,
            Seconds: 0,
            Response: "",
            ErrorMessage: errorMsg
         );
      }
      finally
      {
         // Cleanup agent
         if (agent != null)
         {
            try
            {
               await client.Agents.DeleteAgentAsync(agent.Name);
            }
            catch
            {
               // Ignore cleanup errors
            }
         }
      }
   }

   /// <summary>
   /// Prints the comparison table with all model results.
   /// </summary>
   private static void PrintComparisonTable(List<ModelResult> results)
   {
      // Calculate column widths
      var maxModelLen = Math.Max(5, results.Max(r => r.DeploymentName.Length));
      var maxResponseLen = 50; // Truncate responses to this length

      // Print header
      Console.WriteLine();
      Console.WriteLine($"{"MODEL".PadRight(maxModelLen)}  {"IN",5}  {"OUT",5}  {"(REAS)",6}  {"TOTAL",6}  {"SECS",6}  RESPONSE");
      Console.WriteLine(new string('=', maxModelLen + 5 + 5 + 6 + 6 + 6 + maxResponseLen + 14));

      // Print each result
      foreach (var result in results)
      {
         var model = result.DeploymentName.PadRight(maxModelLen);

         if (result.Success)
         {
            var response = TruncateResponse(result.Response, maxResponseLen);
            Console.WriteLine($"{model}  {result.InputTokens,5}  {result.OutputTokens,5}  {result.InferenceTokens,6}  {result.TotalTokens,6}  {result.Seconds,6:F2}  {response}");
         }
         else
         {
            var errorMsg = TruncateResponse($"ERROR: {result.ErrorMessage}", maxResponseLen);
            Console.WriteLine($"{model}  {"--",5}  {"--",5}  {"--",6}  {"--",6}  {"--",6}  {errorMsg}");
         }
      }

      Console.WriteLine();
      Console.WriteLine("Legend: IN=Input Tokens, OUT=Output Tokens, (REAS)=Reasoning Tokens, TOTAL=Total Tokens, SECS=Runtime");
   }

   /// <summary>
   /// Truncates a response string to the specified max length.
   /// </summary>
   private static string TruncateResponse(string response, int maxLength)
   {
      if (string.IsNullOrEmpty(response))
         return "";
      
      // Replace newlines with spaces for table display
      response = response.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
      
      if (response.Length <= maxLength)
         return response;
      
      return response[..(maxLength - 3)] + "...";
   }
}
