// Copyright (c) Microsoft. All rights reserved.
// Sequential Workflow Demo

// ============================================================================
// EXERCISE 2: Sequential Workflow Demo
// ============================================================================
// This demonstrates a sequential AI-powered workflow that processes customer
// support tickets through a linear pipeline:
// 1. Ticket Intake -> 2. AI Categorization -> 3. AI Response Generation
// ============================================================================

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using WorkflowLab.Common;

namespace WorkflowLab.Sequential;

/// <summary>
/// Sequential Workflow Demo - Customer Support Ticket System
/// </summary>
public static class SequentialWorkflowDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Sequential Workflow Demo - Customer Support Ticket System ===");
        Console.WriteLine();
        Console.WriteLine("This workflow demonstrates sequential processing of support tickets:");
        Console.WriteLine("  1. Ticket Intake -> 2. AI Categorization -> 3. AI Response Generation");
        Console.WriteLine();

        // ============================================================================
        // STEP 2.4: Set up the Azure OpenAI client
        // Uncomment the line below
        // ============================================================================
        // var chatClient = AzureOpenAIClientFactory.CreateChatClient();

        // ============================================================================
        // STEP 2.5: Create executors
        // Uncomment the lines below
        // ============================================================================
        // var ticketIntake = new TicketIntakeExecutor();
        // var categorizationBridge = new CategorizationBridgeExecutor();
        // var responseBridge = new ResponseBridgeExecutor();

        // ============================================================================
        // STEP 2.6: Create AI Agents
        // Uncomment the AI agent definitions below
        // ============================================================================
        // // AI Categorization Agent - categorizes the ticket
        // ChatClientAgent categorizationAgent = new(
        //     chatClient,
        //     name: "CategorizationAgent",
        //     instructions: """
        //         You are a customer support ticket categorization specialist.
        //         Analyze the incoming support ticket and categorize it into one of these categories:
        //         - BILLING: Payment issues, invoices, subscription, refunds
        //         - TECHNICAL: Software bugs, errors, performance issues, how-to questions
        //         - GENERAL: Account inquiries, feedback, general questions
        //         
        //         Respond with a JSON object in this exact format:
        //         {"category": "CATEGORY_NAME", "priority": "HIGH|MEDIUM|LOW", "summary": "brief summary"}
        //         
        //         Keep your response concise and only output the JSON.
        //         """
        // );

        // // AI Response Agent - generates the customer response
        // ChatClientAgent responseAgent = new(
        //     chatClient,
        //     name: "ResponseAgent",
        //     instructions: """
        //         You are a friendly and professional customer support representative.
        //         Based on the ticket category and details provided, generate a helpful response to the customer.
        //         
        //         Guidelines:
        //         - Be empathetic and professional
        //         - Acknowledge the customer's issue
        //         - Provide relevant next steps or solutions
        //         - Keep the response concise (3-4 sentences)
        //         - Include a reference ticket number format: TKT-XXXXX
        //         """
        // );

        // ============================================================================
        // STEP 2.7: Build the sequential workflow using WorkflowBuilder
        // Uncomment the workflow building code below
        // ============================================================================
        // var workflow = new WorkflowBuilder(ticketIntake)
        //     .AddEdge(ticketIntake, categorizationAgent)
        //     .AddEdge(categorizationAgent, categorizationBridge)
        //     .AddEdge(categorizationBridge, responseAgent)
        //     .AddEdge(responseAgent, responseBridge)
        //     .WithOutputFrom(responseBridge)
        //     .Build();

        // Sample customer support ticket
        var sampleTicket = new SupportTicket(
            TicketId: "TKT-12345",
            CustomerId: "CUST-12345",
            CustomerName: "John Smith",
            Subject: "Unable to access my account after password reset",
            Description: "I tried to reset my password yesterday but now I cannot log in. " +
                         "I've tried multiple times and keep getting an 'invalid credentials' error. " +
                         "This is urgent as I need to access my billing information.",
            Priority: TicketPriority.High
        );

        Console.WriteLine("Incoming Support Ticket:");
        Console.WriteLine($"   Ticket ID: {sampleTicket.TicketId}");
        Console.WriteLine($"   Customer: {sampleTicket.CustomerName} ({sampleTicket.CustomerId})");
        Console.WriteLine($"   Priority: {sampleTicket.Priority}");
        Console.WriteLine($"   Subject: {sampleTicket.Subject}");
        Console.WriteLine($"   Description: {sampleTicket.Description[..Math.Min(80, sampleTicket.Description.Length)]}...");
        Console.WriteLine();

        // ============================================================================
        // STEP 2.8: Execute the workflow
        // Uncomment the execution code below and REMOVE the placeholder
        // ============================================================================
        // Console.WriteLine("Processing ticket through sequential workflow...");
        // Console.WriteLine();
        // 
        // await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, sampleTicket);
        // await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        // {
        //     if (evt is WorkflowOutputEvent output)
        //     {
        //         Console.WriteLine("=== Generated Customer Response ===");
        //         Console.WriteLine(output.Data);
        //     }
        // }
        // 
        // Console.WriteLine();
        // Console.WriteLine("Sequential workflow completed!");

        // Placeholder - REMOVE after uncommenting above
        Console.WriteLine("Exercise 2 not completed. Please uncomment the code in SequentialWorkflowDemo.cs and Executors.cs");
    }
}
