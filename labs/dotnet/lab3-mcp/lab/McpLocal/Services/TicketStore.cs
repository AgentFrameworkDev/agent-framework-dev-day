// Copyright (c) Microsoft. All rights reserved.
// In-memory store for customer support tickets (Local MCP)
// TEMPLATE: This provides the same ticket store for local STDIO-based MCP

using McpLocal.Models;

namespace McpLocal.Services;

/// <summary>
/// In-memory store for customer support tickets.
/// In a real application, this would be backed by a database or local file.
/// </summary>
public class TicketStore
{
    private readonly Dictionary<string, SupportTicket> _tickets = new(StringComparer.OrdinalIgnoreCase);

    public TicketStore()
    {
        // Seed with sample tickets (local store has different data)
        SeedSampleTickets();
    }

    private void SeedSampleTickets()
    {
        var tickets = new[]
        {
            new SupportTicket
            {
                Id = "LOCAL-001",
                CustomerId = "LCUST-101",
                CustomerName = "Alice Cooper",
                Subject = "Password reset not working",
                Description = "I clicked the reset link but it says it's expired.",
                Status = TicketStatus.Open,
                Priority = TicketPriority.Medium,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new SupportTicket
            {
                Id = "LOCAL-002",
                CustomerId = "LCUST-102",
                CustomerName = "Bob Martinez",
                Subject = "Cannot upload files",
                Description = "Getting a 413 error when trying to upload documents.",
                Status = TicketStatus.InProgress,
                Priority = TicketPriority.High,
                AssignedTo = "dev-team",
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new SupportTicket
            {
                Id = "LOCAL-003",
                CustomerId = "LCUST-103",
                CustomerName = "Carol White",
                Subject = "Request for invoice",
                Description = "Need invoice for last month's subscription.",
                Status = TicketStatus.Open,
                Priority = TicketPriority.Low,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        foreach (var ticket in tickets)
        {
            _tickets[ticket.Id] = ticket;
        }
    }

    public SupportTicket? GetTicket(string ticketId)
    {
        return _tickets.TryGetValue(ticketId, out var ticket) ? ticket : null;
    }

    public IReadOnlyList<SupportTicket> GetAllTickets()
    {
        return _tickets.Values.OrderByDescending(t => t.CreatedAt).ToList();
    }

    public IReadOnlyList<SupportTicket> GetTicketsByStatus(TicketStatus status)
    {
        return _tickets.Values
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToList();
    }

    public TicketResult UpdateTicket(string ticketId, UpdateTicketRequest request)
    {
        if (!_tickets.TryGetValue(ticketId, out var ticket))
        {
            return new TicketResult
            {
                Success = false,
                Message = $"Ticket '{ticketId}' not found."
            };
        }

        // Apply updates
        if (request.Status.HasValue)
            ticket.Status = request.Status.Value;

        if (request.Priority.HasValue)
            ticket.Priority = request.Priority.Value;

        if (request.AssignedTo != null)
            ticket.AssignedTo = request.AssignedTo;

        if (request.Resolution != null)
            ticket.Resolution = request.Resolution;

        ticket.UpdatedAt = DateTime.UtcNow;

        return new TicketResult
        {
            Success = true,
            Message = $"Ticket '{ticketId}' updated successfully.",
            Ticket = ticket
        };
    }
}
