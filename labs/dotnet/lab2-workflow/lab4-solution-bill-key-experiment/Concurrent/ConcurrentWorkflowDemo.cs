using AgentFrameworkDev.Config;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace WorkflowLab.Concurrent;

/// <summary>
/// Concurrent Workflow Demo - Multi-Agent Customer Support
/// 
/// This demonstrates a concurrent workflow that:
/// 1. Takes a customer question as input
/// 2. Sends the question to multiple specialist agents simultaneously
/// 3. Collects and combines responses from all agents
/// 
/// Concepts covered:
/// - Fan-out Edges (distribute to multiple agents)
/// - Fan-in Edges (collect from multiple agents)
/// - AI Agent Integration
/// - Turn Tokens
/// - Streaming Execution
/// </summary>
public static class ConcurrentWorkflowDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Concurrent Workflow Demo - Multi-Agent Support ===");
        Console.WriteLine();
        Console.WriteLine("This workflow demonstrates parallel processing with multiple AI agents:");
        Console.WriteLine("  Customer Question -> [Billing Expert + Technical Expert] -> Combined Response");
        Console.WriteLine();

        // Set up the Microsoft Foundry chat client
        var chatClient = (await FoundryClientFactory.CreateChatClientAsync()).AsIChatClient();

        // Create specialized AI agents
        ChatClientAgent billingExpert = new(
            chatClient,
            name: "BillingExpert",
            instructions: """
                You are an expert in billing and subscription matters.
                Analyze the customer's question from a billing perspective.
                If the question is not billing-related, briefly acknowledge and defer to other specialists.
                Keep responses concise (2-3 sentences).
                """
        );

        ChatClientAgent technicalExpert = new(
            chatClient,
            name: "TechnicalExpert",
            instructions: """
                You are an expert in technical support and troubleshooting.
                Analyze the customer's question from a technical perspective.
                If the question is not technical, briefly acknowledge and defer to other specialists.
                Keep responses concise (2-3 sentences).
                """
        );

        // Create executors
        var startExecutor = new ConcurrentStartExecutor();
        var aggregationExecutor = new ConcurrentAggregationExecutor();

        // Build the workflow with fan-out and fan-in edges
        var workflow = new WorkflowBuilder(startExecutor)
            .AddFanOutEdge(startExecutor, targets: [billingExpert, technicalExpert])
            .AddFanInEdge(sources: [billingExpert, technicalExpert], aggregationExecutor)
            .WithOutputFrom(aggregationExecutor)
            .Build();

        // Sample customer question
        var customerQuestion = "My subscription was charged twice this month and the app keeps crashing when I try to view my invoice.";

        Console.WriteLine("Customer Question:");
        Console.WriteLine($"   \"{customerQuestion}\"");
        Console.WriteLine();
        Console.WriteLine("Sending question to Billing Expert and Technical Expert simultaneously...");
        Console.WriteLine();

        // Execute the workflow in streaming mode
        await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, customerQuestion);
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is WorkflowOutputEvent output)
            {
                Console.WriteLine("=== Combined Expert Responses ===");
                Console.WriteLine(output.Data);
            }
        }

        Console.WriteLine();
        Console.WriteLine("Concurrent workflow completed!");
    }
}
