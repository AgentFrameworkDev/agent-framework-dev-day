// Copyright (c) Microsoft. All rights reserved.
// Customer Support Ticket Model for Local MCP Server

namespace McpLocal.Models;

/// <summary>
/// Represents a customer support ticket.
/// </summary>
public class SupportTicket
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public string? AssignedTo { get; set; }
    public string? Resolution { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum TicketStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Result of a ticket operation.
/// </summary>
public class TicketResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public SupportTicket? Ticket { get; set; }
}

/// <summary>
/// Request to update a ticket.
/// </summary>
public class UpdateTicketRequest
{
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public string? AssignedTo { get; set; }
    public string? Resolution { get; set; }
}
