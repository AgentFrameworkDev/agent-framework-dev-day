// Copyright (c) Microsoft. All rights reserved.
// Human-in-the-Loop Workflow Executors

// ============================================================================
// EXERCISE 4: Create Human-in-the-Loop Workflow Executors
// ============================================================================
// These executors handle ticket intake, draft processing, and finalization
// with human review in the middle.
// ============================================================================

using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using WorkflowLab.Common;

namespace WorkflowLab.HumanInTheLoop;

// ============================================================================
// STEP 4.1: Create HumanInTheLoopTicketIntakeExecutor
// This executor handles ticket intake and sends to AI agent.
// Uncomment the entire class below.
// ============================================================================
// /// <summary>
// /// Executor that handles ticket intake and sends to AI agent.
// /// </summary>
// internal sealed class HumanInTheLoopTicketIntakeExecutor() : Executor<SupportTicket>("TicketIntake")
// {
//     public static SupportTicket? CurrentTicket { get; private set; }
// 
//     public override async ValueTask HandleAsync(SupportTicket ticket, IWorkflowContext context, CancellationToken cancellationToken = default)
//     {
//         CurrentTicket = ticket;
// 
//         Console.WriteLine("Processing ticket...");
//         Console.WriteLine();
// 
//         var ticketText = $"""
//             Support Ticket Analysis Request:
//             
//             Ticket ID: {ticket.TicketId}
//             Customer: {ticket.CustomerName} (ID: {ticket.CustomerId})
//             Priority: {ticket.Priority}
//             Subject: {ticket.Subject}
//             
//             Customer Message:
//             {ticket.Description}
//             
//             Please analyze this ticket and draft an appropriate response.
//             """;
// 
//         await context.SendMessageAsync(new ChatMessage(ChatRole.User, ticketText), cancellationToken);
//         await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken);
//     }
// }

// ============================================================================
// STEP 4.2: Create DraftBridgeExecutor
// This executor processes AI draft and requests supervisor review.
// Uncomment the entire class below.
// ============================================================================
// /// <summary>
// /// Bridge executor that processes AI draft and requests supervisor review.
// /// </summary>
// internal sealed class DraftBridgeExecutor() : Executor<List<ChatMessage>>("DraftBridge")
// {
//     public override async ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
//     {
//         var draftResponse = messages.LastOrDefault()?.Text ?? "Unable to generate response.";
//         var ticket = HumanInTheLoopTicketIntakeExecutor.CurrentTicket!;
// 
//         Console.WriteLine("AI Draft Generated:");
//         Console.WriteLine($"   {draftResponse[..Math.Min(100, draftResponse.Length)]}...");
//         Console.WriteLine();
// 
//         // Determine category based on ticket content
//         var category = ticket.Subject.Contains("refund", StringComparison.OrdinalIgnoreCase) ? "REFUND" :
//                       ticket.Subject.Contains("technical", StringComparison.OrdinalIgnoreCase) ? "TECHNICAL" :
//                       ticket.Subject.Contains("billing", StringComparison.OrdinalIgnoreCase) ? "BILLING" : "GENERAL";
// 
//         // Request supervisor review
//         var reviewRequest = new SupervisorReviewRequest(
//             TicketId: ticket.TicketId,
//             Category: category,
//             Priority: ticket.Priority.ToString(),
//             DraftResponse: draftResponse
//         );
// 
//         await context.SendMessageAsync(reviewRequest, cancellationToken);
//     }
// }

// ============================================================================
// STEP 4.3: Create FinalizeExecutor
// This executor finalizes the response based on supervisor decision.
// Uncomment the entire class below.
// ============================================================================
// /// <summary>
// /// Executor that finalizes the response based on supervisor decision.
// /// </summary>
// internal sealed class FinalizeExecutor() : Executor<SupervisorDecision>("Finalize")
// {
//     public override async ValueTask HandleAsync(SupervisorDecision decision, IWorkflowContext context, CancellationToken cancellationToken = default)
//     {
//         var ticket = HumanInTheLoopTicketIntakeExecutor.CurrentTicket!;
// 
//         string finalMessage = decision.Action switch
//         {
//             ReviewAction.Approve => $"Response approved and sent to customer {ticket.CustomerName} for ticket {ticket.TicketId}. " +
//                                    $"Status: RESOLVED. Notes: {decision.Notes}",
// 
//             ReviewAction.Edit => $"Modified response sent to customer {ticket.CustomerName} for ticket {ticket.TicketId}. " +
//                                 $"Status: RESOLVED. Notes: {decision.Notes}",
// 
//             ReviewAction.Escalate => $"Ticket {ticket.TicketId} has been escalated to management. " +
//                                     $"Customer: {ticket.CustomerName}. Reason: {decision.Notes}. Status: ESCALATED",
// 
//             _ => "Unknown action taken."
//         };
// 
//         await context.YieldOutputAsync(finalMessage, cancellationToken);
//     }
// }

// Placeholder classes - REMOVE after uncommenting above
internal sealed class HumanInTheLoopTicketIntakeExecutor() : Executor<SupportTicket>("TicketIntake")
{
    public static SupportTicket? CurrentTicket { get; private set; }

    public override ValueTask HandleAsync(SupportTicket ticket, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        CurrentTicket = ticket;
        throw new NotImplementedException("Exercise 4.1 not completed.");
    }
}

internal sealed class DraftBridgeExecutor() : Executor<List<ChatMessage>>("DraftBridge")
{
    public override ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Exercise 4.2 not completed.");
    }
}

internal sealed class FinalizeExecutor() : Executor<SupervisorDecision>("Finalize")
{
    public override ValueTask HandleAsync(SupervisorDecision decision, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Exercise 4.3 not completed.");
    }
}
