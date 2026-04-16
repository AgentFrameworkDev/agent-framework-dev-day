using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using WorkflowLab.Common;

namespace WorkflowLab.HumanInTheLoop;

// ========================================================================
// STEP 4: Implement Human-in-the-Loop Workflow Executors
// ========================================================================
// TODO: Create three executor classes for the human-in-the-loop workflow:
//
// 1. HumanInTheLoopTicketIntakeExecutor - Receives a SupportTicket, stores it
//    as a static property (for access by other executors), formats it, and
//    sends to the AI draft agent
//
// 2. DraftBridgeExecutor - Receives AI draft response (List<ChatMessage>),
//    extracts the draft, determines category, and sends a SupervisorReviewRequest
//    to the RequestPort for human review
//
// 3. FinalizeExecutor - Receives SupervisorDecision and produces the final
//    workflow output based on the supervisor's action (approve/edit/escalate)
//
// Hints:
// - HumanInTheLoopTicketIntakeExecutor uses a static CurrentTicket property
// - DraftBridgeExecutor sends a SupervisorReviewRequest message
// - FinalizeExecutor uses switch expression on decision.Action
// - Use new TurnToken() (without emitEvents) for HITL
//
// [SendsMessage(typeof(ChatMessage))]
// [SendsMessage(typeof(TurnToken))]
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
//         await context.SendMessageAsync(new TurnToken(), cancellationToken);
//     }
// }
//
// [SendsMessage(typeof(SupervisorReviewRequest))]
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
//         var category = ticket.Subject.Contains("refund", StringComparison.OrdinalIgnoreCase) ? "REFUND" :
//                       ticket.Subject.Contains("technical", StringComparison.OrdinalIgnoreCase) ? "TECHNICAL" :
//                       ticket.Subject.Contains("billing", StringComparison.OrdinalIgnoreCase) ? "BILLING" : "GENERAL";
//
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
//
// [YieldsOutput(typeof(string))]
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
// ========================================================================

// Placeholder classes so the project compiles - replace with your implementation above
[SendsMessage(typeof(ChatMessage))]
[SendsMessage(typeof(TurnToken))]
internal sealed class HumanInTheLoopTicketIntakeExecutor() : Executor<SupportTicket>("TicketIntake")
{
    public static SupportTicket? CurrentTicket { get; private set; }

    public override ValueTask HandleAsync(SupportTicket ticket, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("STEP 4: Implement HumanInTheLoopTicketIntakeExecutor");
    }
}

[SendsMessage(typeof(SupervisorReviewRequest))]
internal sealed class DraftBridgeExecutor() : Executor<List<ChatMessage>>("DraftBridge")
{
    public override ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("STEP 4: Implement DraftBridgeExecutor");
    }
}

[YieldsOutput(typeof(string))]
internal sealed class FinalizeExecutor() : Executor<SupervisorDecision>("Finalize")
{
    public override ValueTask HandleAsync(SupervisorDecision decision, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("STEP 4: Implement FinalizeExecutor");
    }
}
