using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using WorkflowLab.Common;

namespace WorkflowLab.Sequential;

/// <summary>
/// Executor that handles ticket intake and validation.
/// </summary>
internal sealed class TicketIntakeExecutor() : Executor<SupportTicket>("TicketIntake")
{
    public override async ValueTask HandleAsync(SupportTicket ticket, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Validate the ticket
        if (string.IsNullOrWhiteSpace(ticket.Subject) || string.IsNullOrWhiteSpace(ticket.Description))
        {
            throw new ArgumentException("Support ticket must have a subject and description.");
        }

        // Format the ticket for the AI categorization agent
        var ticketText = $"""
            Ticket ID: {ticket.TicketId}
            Customer ID: {ticket.CustomerId}
            Customer Name: {ticket.CustomerName}
            Priority: {ticket.Priority}
            Subject: {ticket.Subject}
            Description: {ticket.Description}
            """;

        // Send to the next executor (AI categorization agent)
        await context.SendMessageAsync(new ChatMessage(ChatRole.User, ticketText), cancellationToken);
        await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken);
    }
}

/// <summary>
/// Bridge executor that processes categorization output and prepares for response generation.
/// </summary>
internal sealed class CategorizationBridgeExecutor() : Executor<List<ChatMessage>>("CategorizationBridge")
{
    public override async ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var categorizationResult = messages.LastOrDefault()?.Text ?? "Unknown category";

        Console.WriteLine($"   Categorization: {categorizationResult}");

        // Prepare prompt for response agent with categorization context
        var responsePrompt = $"""
            Based on the following ticket categorization, generate a customer response:
            
            Categorization Result: {categorizationResult}
            
            Please generate an appropriate customer support response.
            """;

        await context.SendMessageAsync(new ChatMessage(ChatRole.User, responsePrompt), cancellationToken);
        await context.SendMessageAsync(new TurnToken(emitEvents: true), cancellationToken);
    }
}

/// <summary>
/// Bridge executor that processes the final response from the AI agent.
/// </summary>
internal sealed class ResponseBridgeExecutor() : Executor<List<ChatMessage>>("ResponseBridge")
{
    public override async ValueTask HandleAsync(List<ChatMessage> messages, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var response = messages.LastOrDefault()?.Text ?? "Unable to generate response.";
        await context.YieldOutputAsync(response, cancellationToken);
    }
}
