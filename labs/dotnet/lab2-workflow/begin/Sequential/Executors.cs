using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using WorkflowLab.Common;

namespace WorkflowLab.Sequential;

// ========================================================================
// STEP 2: Implement Sequential Workflow Executors
// ========================================================================
// TODO: Create three executor classes for the sequential workflow pipeline:
//
// 1. TicketIntakeExecutor - Receives a SupportTicket, formats it as text,
//    and sends it to the next executor (AI categorization agent)
//
// 2. CategorizationBridgeExecutor - Receives AI categorization output
//    (List<ChatMessage>), extracts the result, and prepares a prompt
//    for the response agent
//
// 3. ResponseBridgeExecutor - Receives AI response output (List<ChatMessage>),
//    extracts the final response text, and yields it as workflow output
//
// Hints:
// - Executors inherit from Executor<T> where T is the input type
// - Use [SendsMessage(typeof(ChatMessage))] and [SendsMessage(typeof(TurnToken))]
//   attributes to declare what messages an executor sends
// - Use [YieldsOutput(typeof(string))] to declare workflow output
// - context.SendMessageAsync() sends to the next executor
// - context.YieldOutputAsync() produces workflow output
// - new TurnToken(emitEvents: true) triggers the AI agent to respond
//
// [SendsMessage(typeof(ChatMessage))]
// [SendsMessage(typeof(TurnToken))]
// internal sealed class TicketIntakeExecutor() : Executor<SupportTicket>("TicketIntake")
// {
//     public override async ValueTask HandleAsync(SupportTicket ticket, IWorkflowContext context, CancellationToken cancellationToken = default)
//     {
//         if (string.IsNullOrWhiteSpace(ticket.Subject) || string.IsNullOrWhiteSpace(ticket.Description))
//         {
//             throw new ArgumentException("Support ticket must have a subject and description.");
//         }
//
//         var ticketText = $"""
//             Ticket ID: {ticket.TicketId}
//             Customer ID: {ticket.CustomerId}
//             Customer Name: {ticket.CustomerName}
//             Priority: {ticket.Priority}
//             Subject: {ticket.Subject}
//             Description: {ticket.Description}
//             """;
//
//         await context.SendMessageAsync(new ChatMessage(ChatRole.User, ticketText), cancellationToken);
//         await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken);
//     }
// }
//
// [SendsMessage(typeof(ChatMessage))]
// [SendsMessage(typeof(TurnToken))]
// internal sealed class CategorizationBridgeExecutor() : Executor<List<ChatMessage>>("CategorizationBridge")
// {
//     public override async ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
//     {
//         var categorizationResult = messages.LastOrDefault()?.Text ?? "Unknown category";
//
//         Console.WriteLine($"   Categorization: {categorizationResult}");
//
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
//
// [YieldsOutput(typeof(string))]
// internal sealed class ResponseBridgeExecutor() : Executor<List<ChatMessage>>("ResponseBridge")
// {
//     public override async ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
//     {
//         var response = messages.LastOrDefault()?.Text ?? "Unable to generate response.";
//         await context.YieldOutputAsync(response, cancellationToken);
//     }
// }
// ========================================================================

// Placeholder classes so the project compiles - replace with your implementation above
[SendsMessage(typeof(ChatMessage))]
[SendsMessage(typeof(TurnToken))]
internal sealed class TicketIntakeExecutor() : Executor<SupportTicket>("TicketIntake")
{
    public override ValueTask HandleAsync(SupportTicket ticket, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("STEP 2: Implement TicketIntakeExecutor");
    }
}

[SendsMessage(typeof(ChatMessage))]
[SendsMessage(typeof(TurnToken))]
internal sealed class CategorizationBridgeExecutor() : Executor<List<ChatMessage>>("CategorizationBridge")
{
    public override ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("STEP 2: Implement CategorizationBridgeExecutor");
    }
}

[YieldsOutput(typeof(string))]
internal sealed class ResponseBridgeExecutor() : Executor<List<ChatMessage>>("ResponseBridge")
{
    public override ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("STEP 2: Implement ResponseBridgeExecutor");
    }
}
