namespace WorkflowLab.Common;

/// <summary>
/// Represents a customer support ticket.
/// </summary>
public sealed record SupportTicket(
    string TicketId,
    string CustomerId,
    string CustomerName,
    string Subject,
    string Description,
    TicketPriority Priority = TicketPriority.Medium
);

/// <summary>
/// Ticket priority levels.
/// </summary>
public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}
