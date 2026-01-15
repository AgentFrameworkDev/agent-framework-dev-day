// Copyright (c) Microsoft. All rights reserved.
// Sequential Workflow Executors

// ============================================================================
// EXERCISE 2: Create Sequential Workflow Executors
// ============================================================================
// Executors are the building blocks of workflows. They process inputs and
// send messages to the next step in the workflow.
// ============================================================================

using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using WorkflowLab.Common;

namespace WorkflowLab.Sequential;

// ============================================================================
// STEP 2.1: Create TicketIntakeExecutor
// This executor receives tickets and formats them for the AI agent.
// Uncomment the entire class below.
// ============================================================================
// /// <summary>
// /// Executor that handles ticket intake and validation.
// /// </summary>
// internal sealed class TicketIntakeExecutor() : Executor<SupportTicket>("TicketIntake")
// {
//     public override async ValueTask HandleAsync(SupportTicket ticket, IWorkflowContext context, CancellationToken cancellationToken = default)
//     {
//         // Validate the ticket
//         if (string.IsNullOrWhiteSpace(ticket.Subject) || string.IsNullOrWhiteSpace(ticket.Description))
//         {
//             throw new ArgumentException("Support ticket must have a subject and description.");
//         }
// 
//         // Format the ticket for the AI categorization agent
//         var ticketText = $"""
//             Ticket ID: {ticket.TicketId}
//             Customer ID: {ticket.CustomerId}
//             Customer Name: {ticket.CustomerName}
//             Priority: {ticket.Priority}
//             Subject: {ticket.Subject}
//             Description: {ticket.Description}
//             """;
// 
//         // Send to the next executor (AI categorization agent)
//         await context.SendMessageAsync(new ChatMessage(ChatRole.User, ticketText), cancellationToken);
//         await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken);
//     }
// }

// ============================================================================
// STEP 2.2: Create CategorizationBridgeExecutor
// This executor processes AI categorization output and prepares for response generation.
// Uncomment the entire class below.
// ============================================================================
// /// <summary>
// /// Bridge executor that processes categorization output and prepares for response generation.
// /// </summary>
// internal sealed class CategorizationBridgeExecutor() : Executor<List<ChatMessage>>("CategorizationBridge")
// {
//     public override async ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
//     {
//         var categorizationResult = messages.LastOrDefault()?.Text ?? "Unknown category";
// 
//         Console.WriteLine($"   Categorization: {categorizationResult}");
// 
//         // Prepare prompt for response agent with categorization context
//         var responsePrompt = $"""
//             Based on the following ticket categorization, generate a customer response:
//             
//             Categorization Result: {categorizationResult}
//             
//             Please generate an appropriate customer support response.
//             """;
// 
//         await context.SendMessageAsync(new ChatMessage(ChatRole.User, responsePrompt), cancellationToken);
//         await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken);
//     }
// }

// ============================================================================
// STEP 2.3: Create ResponseBridgeExecutor
// This executor processes the final AI response and yields output.
// Uncomment the entire class below.
// ============================================================================
// /// <summary>
// /// Bridge executor that processes the final response from the AI agent.
// /// </summary>
// internal sealed class ResponseBridgeExecutor() : Executor<List<ChatMessage>>("ResponseBridge")
// {
//     public override async ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
//     {
//         var response = messages.LastOrDefault()?.Text ?? "Unable to generate response.";
//         await context.YieldOutputAsync(response, cancellationToken);
//     }
// }

// Placeholder classes - REMOVE after uncommenting above
internal sealed class TicketIntakeExecutor() : Executor<SupportTicket>("TicketIntake")
{
    public override ValueTask HandleAsync(SupportTicket ticket, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Exercise 2.1 not completed.");
    }
}

internal sealed class CategorizationBridgeExecutor() : Executor<List<ChatMessage>>("CategorizationBridge")
{
    public override ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Exercise 2.2 not completed.");
    }
}

internal sealed class ResponseBridgeExecutor() : Executor<List<ChatMessage>>("ResponseBridge")
{
    public override ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Exercise 2.3 not completed.");
    }
}
