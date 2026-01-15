// Copyright (c) Microsoft. All rights reserved.
// In-memory store for customer support tickets

using RemoteServer.Models;

namespace RemoteServer.Services;

/// <summary>
/// In-memory store for customer support tickets.
/// In a real application, this would be backed by a database.
/// </summary>
public class TicketStore
{
    private readonly Dictionary<string, SupportTicket> _tickets = new(StringComparer.OrdinalIgnoreCase);

    public TicketStore()
    {
        // Seed with sample tickets
        SeedSampleTickets();
    }

    private void SeedSampleTickets()
    {
        var tickets = new[]
        {
            new SupportTicket
            {
                Id = "TKT-001",
                CustomerId = "CUST-101",
                CustomerName = "John Smith",
                Subject = "Cannot login to my account",
                Description = "I'm getting an 'invalid credentials' error even though I'm sure my password is correct.",
                Status = TicketStatus.Open,
                Priority = TicketPriority.High,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new SupportTicket
            {
                Id = "TKT-002",
                CustomerId = "CUST-102",
                CustomerName = "Sarah Johnson",
                Subject = "Billing discrepancy",
                Description = "I was charged twice for my subscription this month.",
                Status = TicketStatus.InProgress,
                Priority = TicketPriority.Critical,
                AssignedTo = "billing-team",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new SupportTicket
            {
                Id = "TKT-003",
                CustomerId = "CUST-103",
                CustomerName = "Mike Wilson",
                Subject = "Feature request: Dark mode",
                Description = "Would love to have a dark mode option in the app.",
                Status = TicketStatus.Open,
                Priority = TicketPriority.Low,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new SupportTicket
            {
                Id = "TKT-004",
                CustomerId = "CUST-104",
                CustomerName = "Emily Brown",
                Subject = "App crashes on startup",
                Description = "After the latest update, the app crashes immediately when I open it.",
                Status = TicketStatus.Resolved,
                Priority = TicketPriority.High,
                AssignedTo = "dev-team",
                Resolution = "Fixed in version 2.1.5. User advised to update the app.",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
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
