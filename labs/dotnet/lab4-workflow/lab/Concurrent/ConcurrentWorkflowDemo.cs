// Copyright (c) Microsoft. All rights reserved.
// Concurrent Workflow Demo

// ============================================================================
// EXERCISE 3: Concurrent Workflow Demo
// ============================================================================
// This demonstrates a concurrent workflow that sends questions to multiple
// specialist agents simultaneously (fan-out) and combines their responses (fan-in).
// ============================================================================

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowLab.Common;

namespace WorkflowLab.Concurrent;

/// <summary>
/// Concurrent Workflow Demo - Multi-Agent Customer Support
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

        // ============================================================================
        // STEP 3.3: Set up the Azure OpenAI client
        // Uncomment the line below
        // ============================================================================
        // var chatClient = AzureOpenAIClientFactory.CreateChatClient();

        // ============================================================================
        // STEP 3.4: Create specialized AI agents
        // Uncomment the AI agent definitions below
        // ============================================================================
        // ChatClientAgent billingExpert = new(
        //     chatClient,
        //     name: "BillingExpert",
        //     instructions: """
        //         You are an expert in billing and subscription matters.
        //         Analyze the customer's question from a billing perspective.
        //         If the question is not billing-related, briefly acknowledge and defer to other specialists.
        //         Keep responses concise (2-3 sentences).
        //         """
        // );
        // 
        // ChatClientAgent technicalExpert = new(
        //     chatClient,
        //     name: "TechnicalExpert",
        //     instructions: """
        //         You are an expert in technical support and troubleshooting.
        //         Analyze the customer's question from a technical perspective.
        //         If the question is not technical, briefly acknowledge and defer to other specialists.
        //         Keep responses concise (2-3 sentences).
        //         """
        // );

        // ============================================================================
        // STEP 3.5: Create executors
        // Uncomment the lines below
        // ============================================================================
        // var startExecutor = new ConcurrentStartExecutor();
        // var aggregationExecutor = new ConcurrentAggregationExecutor();

        // ============================================================================
        // STEP 3.6: Build the workflow with fan-out and fan-in edges
        // Uncomment the workflow building code below
        // ============================================================================
        // var workflow = new WorkflowBuilder(startExecutor)
        //     .AddFanOutEdge(startExecutor, targets: [billingExpert, technicalExpert])
        //     .AddFanInEdge(sources: [billingExpert, technicalExpert], aggregationExecutor)
        //     .WithOutputFrom(aggregationExecutor)
        //     .Build();

        // Sample customer question
        var customerQuestion = "My subscription was charged twice this month and the app keeps crashing when I try to view my invoice.";

        Console.WriteLine("Customer Question:");
        Console.WriteLine($"   \"{customerQuestion}\"");
        Console.WriteLine();

        // ============================================================================
        // STEP 3.7: Execute the workflow in streaming mode
        // Uncomment the execution code below and REMOVE the placeholder
        // ============================================================================
        // Console.WriteLine("Sending question to Billing Expert and Technical Expert simultaneously...");
        // Console.WriteLine();
        // 
        // await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, customerQuestion);
        // await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        // {
        //     if (evt is WorkflowOutputEvent output)
        //     {
        //         Console.WriteLine("=== Combined Expert Responses ===");
        //         Console.WriteLine(output.Data);
        //     }
        // }
        // 
        // Console.WriteLine();
        // Console.WriteLine("Concurrent workflow completed!");

        // Placeholder - REMOVE after uncommenting above
        Console.WriteLine("Exercise 3 not completed. Please uncomment the code in ConcurrentWorkflowDemo.cs and Executors.cs");
    }
}
