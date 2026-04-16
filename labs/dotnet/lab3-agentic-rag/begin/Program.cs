using Azure.AI.OpenAI;
using Azure.Identity;
using Lab3.Agents;
using Lab3.Config;
using Lab3.Services;
using Lab3.Workflows;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace Lab3;

/// <summary>
/// Agentic RAG application for IT support ticket search.
///
/// This application uses the Microsoft Agent Framework with a WorkflowBuilder
/// pattern using structured output, executors, and switch-case routing to
/// route user questions to specialized search agents based on query type.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        if (args.Contains("--interactive") || args.Contains("-i"))
        {
            await InteractiveModeAsync();
        }
        else
        {
            await DemoModeAsync();
        }
    }

    static Workflow BuildWorkflow(Dictionary<string, AIAgent> agents)
    {
        // Create executors
        var classifierExecutor = new ClassifierExecutor(agents["classifier"]);
        var semanticSearchExecutor = new SpecialistExecutor("SemanticSearch", agents["semantic_search"]);

        // Build workflow with switch-case routing
        var builder = new WorkflowBuilder(classifierExecutor);
        builder.AddSwitch(classifierExecutor, sb => sb
            .AddCase(CategoryConditions.Is("semantickSearch"), semanticSearchExecutor)
            .WithDefault(semanticSearchExecutor)
        )
        .WithOutputFrom(semanticSearchExecutor);

        return builder.Build();
    }

    static async Task DemoModeAsync()
    {
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("AGENTIC RAG - IT SUPPORT TICKET SEARCH");
        Console.WriteLine(new string('=', 60));

        // Load and validate configuration
        Console.WriteLine("\n[1/5] Loading configuration...");
        BuildConfiguration();
        var config = AzureConfig.FromConfiguration();

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
                    new DefaultAzureCredential()
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

        // Build workflow with switch-case routing
        Console.WriteLine("\n[5/5] Building workflow...");
        var workflow = BuildWorkflow(agents);
        Console.WriteLine("✓ Workflow built successfully");
        
        // Example questions to test
        var testQuestions = new[]
        {
            "What problems are there with Surface devices?",                                  // Semantic search
            "Are there any issues for Dell XPS laptops?",                                     // Yes/No
            "How many tickets were logged and Incidents for Human Resources and low priority?", // Count
            "Do we have more issues with MacBook Air computers or Dell XPS laptops?",          // Comparative
            "Which Dell XPS issue does not mention Windows?",                                  // Difference
            "What issues are for Dell XPS laptops and the user tried Win + Ctrl + Shift + B?", // Intersection
            "What department had consultants with Login Issues?",                              // Multi-hop
        };

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("RUNNING TEST QUERIES");
        Console.WriteLine(new string('=', 60));

        for (int i = 0; i < testQuestions.Length; i++)
        {
            Console.WriteLine($"\n--- Query {i + 1}/{testQuestions.Length} ---");
            Console.WriteLine($"User: {testQuestions[i]}");
            await RunWorkflowAsync(workflow, testQuestions[i]);
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
        BuildConfiguration();
        var config = AzureConfig.FromConfiguration();
        config.Validate();

        var chatClient = new AzureOpenAIClient(
                            new Uri(config.OpenAIEndpoint),
                            new DefaultAzureCredential()
                         )
                        .GetChatClient(config.ChatModel);

        var searchService = new SearchService(config, chatClient);
        var agentFactory = new AgentFactory(chatClient, searchService);
        var agents = agentFactory.CreateAllAgents();

        var workflow = BuildWorkflow(agents);

        Console.WriteLine("✓ System ready\n");

        // Interactive loop — each query is stateless
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
                await RunWorkflowAsync(workflow, userInput);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Error: {ex.Message}\n");
            }
        }
    }

    static async Task RunWorkflowAsync(Workflow workflow, string question)
    {
        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(
            workflow, new ChatMessage(ChatRole.User, question));

        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is WorkflowOutputEvent output)
            {
                Console.WriteLine(output.Data);
            }
        }
    }


    static string? FindConfigDirectory(string fileName)
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, fileName)))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        return null;
    }
    public const string DefaultConfigFileName = "appsettings.Local.json";
    static void BuildConfiguration()
    {
        var basePath = FindConfigDirectory(DefaultConfigFileName)
            ?? throw new InvalidOperationException(
                $"Could not find {DefaultConfigFileName} in current directory or any parent directory.");

        // Load configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        foreach (var kvp in configuration.AsEnumerable())
        {
            if (!string.IsNullOrEmpty(kvp.Value))
            {
                Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
            }
        }
    }
}
