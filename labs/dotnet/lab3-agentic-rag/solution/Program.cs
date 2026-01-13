using Azure;
using Azure.AI.OpenAI;
using Lab3.Agents;
using Lab3.Config;
using Lab3.Services;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Lab3;

/// <summary>
/// Agentic RAG application for IT support ticket search.
/// 
/// This application uses the Microsoft Agent Framework with a Handoff orchestration
/// pattern to route user questions to specialized search agents based on query type.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Check for interactive mode flag
        if (args.Contains("--interactive") || args.Contains("-i"))
        {
            await InteractiveModeAsync();
        }
        else
        {
            await DemoModeAsync();
        }
    }

    static async Task DemoModeAsync()
    {
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("AGENTIC RAG - IT SUPPORT TICKET SEARCH");
        Console.WriteLine(new string('=', 60));

        // Load and validate configuration
        Console.WriteLine("\n[1/5] Loading configuration...");
        var configuration = BuildConfiguration();
        var config = AzureConfig.FromConfiguration(configuration);

        try
        {
            config.Validate();
            Console.WriteLine("✓ Configuration loaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Configuration error: {ex.Message}");
            return;
        }

        // Initialize Azure OpenAI chat client
        Console.WriteLine("\n[2/5] Initializing Azure OpenAI client...");
        var chatClient = new AzureOpenAIClient(
                    new Uri(config.OpenAIEndpoint),
                    new AzureKeyCredential(config.OpenAIApiKey)
                 )
                .GetChatClient(config.ChatModel);
        Console.WriteLine("✓ Chat client initialized");

        // Initialize search service
        Console.WriteLine("\n[3/5] Initializing Azure AI Search service...");
        var searchService = new SearchService(config, chatClient);
        Console.WriteLine("✓ Search service initialized");

        // Create agents
        Console.WriteLine("\n[4/5] Creating agents...");
        var agentFactory = new AgentFactory(chatClient, searchService);
        var agents = agentFactory.CreateAllAgents();
        Console.WriteLine($"✓ Created {agents.Count} agents: {string.Join(", ", agents.Keys)}");

        // Build workflow with handoff orchestration
        Console.WriteLine("\n[5/5] Building workflow...");
        var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(agents["classifier"])
            .WithHandoffs(agents["classifier"], [agents["yes_no"], agents["semantic_search"], agents["count"], agents["comparative"], agents["difference"], agents["intersection"], agents["multi_hop"]])
            .WithHandoffs([agents["yes_no"], agents["semantic_search"], agents["count"], agents["comparative"], agents["difference"], agents["intersection"], agents["multi_hop"]], agents["classifier"])
            .Build();
        Console.WriteLine("✓ Workflow built successfully");
        
        // Example questions to test
        var testQuestions = new[]
        {
            "What problems are there with Surface devices?", //  (Simple question) 
            "Are there any issues for Dell XPS laptops?", // (Yes/No)
            "How many tickets were logged and Incidents for Human Resources and low priority?", //  (Count)
            "Do we have more issues with MacBook Air computers or Dell XPS laptops?", // (Comparative)
            "Which Dell XPS issue does not mention Windows?", // (Difference)
            "What issues are for Dell XPS laptops and the user tried Win + Ctrl + Shift + B?", // (Intersection)
            "What department had consultants with Login Issues?",  // (Multi-hop)
        };

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("RUNNING TEST QUERIES");
        Console.WriteLine(new string('=', 60));

        // Display test questions
        List<ChatMessage> messages = [];
        for (int i = 0; i < testQuestions.Length; i++)
        {
            Console.WriteLine($"\n--- Query {i + 1}/{testQuestions.Length} ---");
            Console.WriteLine($"User: {testQuestions[i]}");
            messages.Add(new(ChatRole.User, testQuestions[i]));
            messages.AddRange(await RunWorkflowAsync(workflow, messages));
            Console.WriteLine();
        }

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("DEMO COMPLETE");
        Console.WriteLine(new string('=', 60));
    }

    static async Task InteractiveModeAsync()
    {
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("AGENTIC RAG - INTERACTIVE MODE");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("\nType 'quit' or 'exit' to end the session\n");

        // Initialize system
        var configuration = BuildConfiguration();
        var config = AzureConfig.FromConfiguration(configuration);
        config.Validate();

        var chatClient = new AzureOpenAIClient(
                            new Uri(config.OpenAIEndpoint),
                            new AzureKeyCredential(config.OpenAIApiKey)
                         )
                        .GetChatClient(config.ChatModel);

        var searchService = new SearchService(config, chatClient);
        var agentFactory = new AgentFactory(chatClient, searchService);
        var agents = agentFactory.CreateAllAgents();

        var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(agents["classifier"])
            .WithHandoffs(agents["classifier"], [agents["yes_no"], agents["semantic_search"], agents["count"], agents["comparative"], agents["difference"], agents["intersection"], agents["multi_hop"]])
            .WithHandoffs([agents["yes_no"], agents["semantic_search"], agents["count"], agents["comparative"], agents["difference"], agents["intersection"], agents["multi_hop"]], agents["classifier"])
            .Build();

        Console.WriteLine("✓ System ready\n");

        // Interactive loop
        List<ChatMessage> messages = [];
        while (true)
        {
            try
            {
                Console.Write("You: ");
                var userInput = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(userInput))
                    continue;

                if (userInput.Equals("quit", StringComparison.OrdinalIgnoreCase) ||
                    userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    userInput.Equals("q", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("\nGoodbye!");
                    break;
                }

                Console.WriteLine();
                //Console.WriteLine($"Processing query: {userInput}");
                messages.Add(new(ChatRole.User, userInput));
                messages.AddRange(await RunWorkflowAsync(workflow, messages));
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error: {ex.Message}\n");
            }
        }
    }

    static async Task<List<ChatMessage>> RunWorkflowAsync(Workflow workflow, List<ChatMessage> messages)
    {
        string? lastExecutorId = null;

        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is AgentRunUpdateEvent e)
            {
                if (e.ExecutorId != lastExecutorId)
                {
                    lastExecutorId = e.ExecutorId;
                    Console.WriteLine();
                    Console.WriteLine(e.ExecutorId);
                }

                Console.Write(e.Update.Text);
                if (e.Update.Contents.OfType<FunctionCallContent>().FirstOrDefault() is FunctionCallContent call)
                {
                    Console.WriteLine();
                    Console.WriteLine($"  [Calling function '{call.Name}' with arguments: {JsonSerializer.Serialize(call.Arguments)}]");
                }
            }
            else if (evt is WorkflowOutputEvent output)
            {
                Console.WriteLine();
                return output.As<List<ChatMessage>>()!;
            }
        }

        return [];
    }

    static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>(optional: true);

        return builder.Build();
    }
}
