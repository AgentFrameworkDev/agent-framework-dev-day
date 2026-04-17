using AgentFrameworkDev.Config;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Lab1;

/// <summary>
/// Loads a declarative movie trivia quiz agent from YAML and uses it to ask and grade a trivia question.
/// </summary>
public static class Program
{
   private const string AgentDefinitionFileName = "movie-trivia-agent.yaml";

   public static async Task Main(string[] args)
   {
      var agentDefinitionPath = args.Length > 0 ? args[0] : AgentDefinitionFileName;
      var agentDefinition = LoadAgentDefinition(agentDefinitionPath);

      var config = FoundryClientFactory.GetConfiguration();
      IChatClient chatClient = FoundryClientFactory.CreateChatClient(config);
      var agentFactory = new ChatClientPromptAgentFactory(chatClient);
      var agent = await agentFactory.CreateFromYamlAsync(agentDefinition);

      Console.WriteLine("Movie Trivia Quiz Agent");
      Console.WriteLine($"Definition: {Path.GetFileName(agentDefinitionPath)}");
      Console.WriteLine();

      var question = await AskQuestionAsync(agent);
      Console.WriteLine("Question:");
      Console.WriteLine(question);
      Console.WriteLine();

      Console.Write("Your answer: ");
      var answer = Console.ReadLine()?.Trim();

      if (string.IsNullOrWhiteSpace(answer))
      {
         Console.WriteLine();
         Console.WriteLine("No answer provided. Exiting.");
         return;
      }

      var evaluation = await GradeAnswerAsync(agent, question, answer);

      Console.WriteLine();
      Console.WriteLine("Result:");
      Console.WriteLine(evaluation);
   }

   /// <summary>
   /// Loads the YAML definition from the current directory or build output folder.
   /// </summary>
   private static string LoadAgentDefinition(string agentDefinitionPath)
   {
      string[] candidatePaths = Path.IsPathRooted(agentDefinitionPath)
         ? [agentDefinitionPath]
         :
         [
            Path.Combine(Directory.GetCurrentDirectory(), agentDefinitionPath),
            Path.Combine(AppContext.BaseDirectory, agentDefinitionPath)
         ];

      foreach (var candidatePath in candidatePaths)
      {
         if (File.Exists(candidatePath))
         {
            return File.ReadAllText(candidatePath);
         }
      }

      throw new FileNotFoundException(
         $"Could not find the agent definition file '{agentDefinitionPath}'.",
         agentDefinitionPath);
   }

   /// <summary>
   /// Asks the agent for a single movie trivia question.
   /// </summary>
   private static async Task<string> AskQuestionAsync(AIAgent agent)
   {
      var response = await agent.RunAsync(
         "Ask exactly one movie trivia question. Do not reveal the answer until I respond.");

      var question = response.ToString()?.Trim();
      if (string.IsNullOrWhiteSpace(question))
      {
         throw new InvalidOperationException("The agent did not return a trivia question.");
      }

      return question;
   }

   /// <summary>
   /// Asks the agent to evaluate the user's answer against the trivia question.
   /// </summary>
   private static async Task<string> GradeAnswerAsync(AIAgent agent, string question, string answer)
   {
      var evaluationPrompt = $$"""
      The trivia question you asked was:
      {{question}}

      My answer is:
      {{answer}}

      Tell me whether my answer is correct.
      If it is correct, start with "Correct:" and give a short explanation.
      If it is incorrect, start with "Incorrect:", then provide the correct answer and a short explanation.
      """;

      var response = await agent.RunAsync(evaluationPrompt);
      var evaluation = response.ToString()?.Trim();

      if (string.IsNullOrWhiteSpace(evaluation))
      {
         throw new InvalidOperationException("The agent did not return an evaluation.");
      }

      return evaluation;
   }
}
