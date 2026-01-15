// Copyright (c) Microsoft. All rights reserved.
// Human-in-the-Loop Workflow Demo

// ============================================================================
// EXERCISE 4: Human-in-the-Loop Workflow Demo
// ============================================================================
// This demonstrates an interactive workflow for customer support that:
// 1. Receives a customer support ticket
// 2. AI agent analyzes and drafts a response
// 3. Pauses to request human supervisor review/approval
// 4. Allows supervisor to approve, edit, or escalate the ticket
// 5. Finalizes and sends the response or escalates to management
// ============================================================================

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using WorkflowLab.Common;

namespace WorkflowLab.HumanInTheLoop;

/// <summary>
/// Human-in-the-Loop Workflow Demo - Customer Support Ticket Review System
/// </summary>
public static class HumanInTheLoopWorkflowDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Human-in-the-Loop Workflow Demo ===");
        Console.WriteLine("=== Customer Support Ticket Review System ===");
        Console.WriteLine();
        Console.WriteLine("This workflow demonstrates AI-assisted ticket handling with human oversight:");
        Console.WriteLine("  Ticket -> AI Draft -> [Human Review] -> Final Response");
        Console.WriteLine();
        Console.WriteLine("A supervisor reviews AI-generated responses before they are sent to customers.");
        Console.WriteLine();

        // ============================================================================
        // STEP 4.4: Set up the Azure OpenAI client
        // Uncomment the line below
        // ============================================================================
        // var chatClient = AzureOpenAIClientFactory.CreateChatClient();

        // ============================================================================
        // STEP 4.5: Build the workflow
        // Uncomment the line below
        // ============================================================================
        // var workflow = BuildWorkflow(chatClient);

        // Sample support ticket
        var sampleTicket = new SupportTicket(
            TicketId: "TKT-78542",
            CustomerId: "CUST-12345",
            CustomerName: "Sarah Johnson",
            Subject: "Request for full refund on subscription",
            Description: "I signed up for the annual premium plan last week but the features don't work as advertised. " +
                         "The video conferencing keeps dropping and the file storage is extremely slow. " +
                         "I want a full refund and to cancel my subscription immediately.",
            Priority: TicketPriority.High
        );

        Console.WriteLine("Incoming Support Ticket:");
        Console.WriteLine($"   Ticket ID: {sampleTicket.TicketId}");
        Console.WriteLine($"   Customer: {sampleTicket.CustomerName} ({sampleTicket.CustomerId})");
        Console.WriteLine($"   Priority: {sampleTicket.Priority}");
        Console.WriteLine($"   Subject: {sampleTicket.Subject}");
        Console.WriteLine($"   Description: {sampleTicket.Description}");
        Console.WriteLine();

        // ============================================================================
        // STEP 4.6: Execute the workflow with human review handling
        // Uncomment the execution code below and REMOVE the placeholder
        // ============================================================================
        // await using StreamingRun handle = await InProcessExecution.StreamAsync(workflow, sampleTicket);
        // await foreach (WorkflowEvent evt in handle.WatchStreamAsync())
        // {
        //     switch (evt)
        //     {
        //         case RequestInfoEvent requestInputEvt:
        //             // Handle human supervisor review request
        //             ExternalResponse response = HandleSupervisorReview(requestInputEvt.Request);
        //             await handle.SendResponseAsync(response);
        //             break;
        // 
        //         case WorkflowOutputEvent outputEvt:
        //             // The workflow has completed
        //             Console.WriteLine();
        //             Console.WriteLine($"{outputEvt.Data}");
        //             Console.WriteLine();
        //             Console.WriteLine("Human-in-the-loop workflow completed!");
        //             return;
        //     }
        // }

        // Placeholder - REMOVE after uncommenting above
        Console.WriteLine("Exercise 4 not completed. Please uncomment the code in HumanInTheLoopWorkflowDemo.cs and Executors.cs");
    }

    // ============================================================================
    // STEP 4.7: Implement BuildWorkflow method
    // Uncomment the entire method below
    // ============================================================================
    // private static Workflow BuildWorkflow(IChatClient chatClient)
    // {
    //     // Create the AI agent for drafting responses
    //     ChatClientAgent draftAgent = new(
    //         chatClient,
    //         name: "DraftAgent",
    //         instructions: """
    //             You are an experienced customer support specialist. Your job is to:
    //             1. Analyze the support ticket
    //             2. Categorize it (BILLING, TECHNICAL, REFUND, GENERAL)
    //             3. Draft a professional, empathetic response
    //             
    //             For refund requests:
    //             - Acknowledge the customer's frustration
    //             - Explain the refund policy (14-day money-back guarantee)
    //             - Offer alternatives if applicable (troubleshooting, downgrade)
    //             - Be professional but empathetic
    //             
    //             Keep your response to 3-5 sentences. Be concise but helpful.
    //             """
    //     );
    // 
    //     // Create executors
    //     var ticketIntake = new HumanInTheLoopTicketIntakeExecutor();
    //     var draftBridge = new DraftBridgeExecutor();
    //     RequestPort supervisorReviewPort = RequestPort.Create<SupervisorReviewRequest, SupervisorDecision>("SupervisorReview");
    //     var finalizeExecutor = new FinalizeExecutor();
    // 
    //     // Build the workflow
    //     return new WorkflowBuilder(ticketIntake)
    //         .AddEdge(ticketIntake, draftAgent)
    //         .AddEdge(draftAgent, draftBridge)
    //         .AddEdge(draftBridge, supervisorReviewPort)
    //         .AddEdge(supervisorReviewPort, finalizeExecutor)
    //         .WithOutputFrom(finalizeExecutor)
    //         .Build();
    // }

    // ============================================================================
    // STEP 4.8: Implement HandleSupervisorReview method
    // Uncomment the entire method below
    // ============================================================================
    // private static ExternalResponse HandleSupervisorReview(ExternalRequest request)
    // {
    //     var reviewRequest = request.DataAs<SupervisorReviewRequest>();
    // 
    //     if (reviewRequest == null)
    //     {
    //         throw new ArgumentException("Invalid review request.");
    //     }
    // 
    //     Console.WriteLine();
    //     Console.WriteLine("=====================================================================");
    //     Console.WriteLine("            SUPERVISOR REVIEW REQUIRED                               ");
    //     Console.WriteLine("=====================================================================");
    //     Console.WriteLine();
    //     Console.WriteLine($"Ticket: {reviewRequest.TicketId}");
    //     Console.WriteLine($"Category: {reviewRequest.Category}");
    //     Console.WriteLine($"Priority: {reviewRequest.Priority}");
    //     Console.WriteLine();
    //     Console.WriteLine("AI-Generated Draft Response:");
    //     Console.WriteLine("---------------------------------------------------------------------");
    //     Console.WriteLine(reviewRequest.DraftResponse);
    //     Console.WriteLine("---------------------------------------------------------------------");
    //     Console.WriteLine();
    //     Console.WriteLine("Actions:");
    //     Console.WriteLine("  [1] Approve - Send this response to the customer");
    //     Console.WriteLine("  [2] Edit - Modify the response before sending");
    //     Console.WriteLine("  [3] Escalate - Escalate to management for review");
    //     Console.WriteLine();
    // 
    //     while (true)
    //     {
    //         Console.Write("Enter your choice (1-3): ");
    //         var choice = Console.ReadLine()?.Trim();
    // 
    //         switch (choice)
    //         {
    //             case "1":
    //                 Console.WriteLine();
    //                 Console.WriteLine("Response approved. Sending to customer...");
    //                 return request.CreateResponse(new SupervisorDecision(ReviewAction.Approve, null, "Approved as-is"));
    // 
    //             case "2":
    //                 Console.WriteLine();
    //                 Console.WriteLine("Enter your modified response (press Enter twice to finish):");
    //                 var lines = new List<string>();
    //                 string? line;
    //                 while (!string.IsNullOrEmpty(line = Console.ReadLine()))
    //                 {
    //                     lines.Add(line);
    //                 }
    //                 var modifiedResponse = string.Join(Environment.NewLine, lines);
    //                 Console.WriteLine("Modified response saved. Sending to customer...");
    //                 return request.CreateResponse(new SupervisorDecision(ReviewAction.Edit, modifiedResponse, "Edited by supervisor"));
    // 
    //             case "3":
    //                 Console.WriteLine();
    //                 Console.Write("Enter escalation reason: ");
    //                 var reason = Console.ReadLine() ?? "Escalated by supervisor";
    //                 Console.WriteLine("Ticket escalated to management.");
    //                 return request.CreateResponse(new SupervisorDecision(ReviewAction.Escalate, null, reason));
    // 
    //             default:
    //                 Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.");
    //                 break;
    //         }
    //     }
    // }
}
