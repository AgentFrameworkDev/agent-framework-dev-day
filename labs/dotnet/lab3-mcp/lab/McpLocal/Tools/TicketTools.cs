// Copyright (c) Microsoft. All rights reserved.
// MCP Tools for Customer Support Ticket operations (Local MCP Server)

using System.ComponentModel;
using System.Text.Json;
using McpLocal.Models;
using McpLocal.Services;
using ModelContextProtocol.Server;

namespace McpLocal.Tools;

/// <summary>
/// MCP Tools for reading and updating customer support tickets.
/// </summary>
[McpServerToolType]
public sealed class TicketTools
{
    private readonly TicketStore _store;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public TicketTools(TicketStore store)
    {
        _store = store;
    }

    /// <summary>
    /// Gets a support ticket by ID, or lists all tickets if no ID is provided.
    /// </summary>
    [McpServerTool]
    [Description("Gets a customer support ticket by ID. If no ID is provided, returns all tickets. You can also filter by status. Example IDs: LOCAL-001, LOCAL-002")]
    public string GetTicket(
        [Description("The ticket ID to retrieve (e.g., LOCAL-001). Leave empty to get all tickets.")] 
        string? ticketId = null,
        [Description("Optional status filter: Open, InProgress, Resolved, or Closed")] 
        string? statusFilter = null)
    {
        Console.Error.WriteLine($"GetTicket called - ID: '{ticketId ?? "(all)"}', StatusFilter: '{statusFilter ?? "(none)"}'");

        // If specific ticket ID requested
        if (!string.IsNullOrWhiteSpace(ticketId))
        {
            var ticket = _store.GetTicket(ticketId);
            if (ticket == null)
            {
                return JsonSerializer.Serialize(new TicketResult
                {
                    Success = false,
                    Message = $"Ticket '{ticketId}' not found."
                }, JsonOptions);
            }

            return JsonSerializer.Serialize(new TicketResult
            {
                Success = true,
                Message = $"Found ticket '{ticketId}'",
                Ticket = ticket
            }, JsonOptions);
        }

        // Return all tickets, optionally filtered by status
        IReadOnlyList<SupportTicket> tickets;
        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<TicketStatus>(statusFilter, true, out var status))
        {
            tickets = _store.GetTicketsByStatus(status);
            Console.Error.WriteLine($"Returning {tickets.Count} tickets with status '{status}'");
        }
        else
        {
            tickets = _store.GetAllTickets();
            Console.Error.WriteLine($"Returning all {tickets.Count} tickets");
        }

        return JsonSerializer.Serialize(new
        {
            Success = true,
            Message = $"Found {tickets.Count} ticket(s)",
            Tickets = tickets
        }, JsonOptions);
    }

    /// <summary>
    /// Updates a support ticket's status, priority, assignment, or resolution.
    /// </summary>
    [McpServerTool]
    [Description("Updates a customer support ticket. You can change the status, priority, assignee, or add a resolution. Status values: Open, InProgress, Resolved, Closed. Priority values: Low, Medium, High, Critical")]
    public string UpdateTicket(
        [Description("The ticket ID to update (required, e.g., LOCAL-001)")] 
        string ticketId,
        [Description("New status: Open, InProgress, Resolved, or Closed")] 
        string? status = null,
        [Description("New priority: Low, Medium, High, or Critical")] 
        string? priority = null,
        [Description("Person or team to assign the ticket to")] 
        string? assignedTo = null,
        [Description("Resolution notes (typically added when resolving/closing)")] 
        string? resolution = null)
    {
        Console.Error.WriteLine($"UpdateTicket called - ID: '{ticketId}'");

        if (string.IsNullOrWhiteSpace(ticketId))
        {
            return JsonSerializer.Serialize(new TicketResult
            {
                Success = false,
                Message = "Ticket ID is required."
            }, JsonOptions);
        }

        // Build update request
        var request = new UpdateTicketRequest();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<TicketStatus>(status, true, out var statusValue))
                request.Status = statusValue;
            else
                return JsonSerializer.Serialize(new TicketResult
                {
                    Success = false,
                    Message = $"Invalid status '{status}'. Valid values: Open, InProgress, Resolved, Closed"
                }, JsonOptions);
        }

        if (!string.IsNullOrWhiteSpace(priority))
        {
            if (Enum.TryParse<TicketPriority>(priority, true, out var priorityValue))
                request.Priority = priorityValue;
            else
                return JsonSerializer.Serialize(new TicketResult
                {
                    Success = false,
                    Message = $"Invalid priority '{priority}'. Valid values: Low, Medium, High, Critical"
                }, JsonOptions);
        }

        if (assignedTo != null)
            request.AssignedTo = assignedTo;

        if (resolution != null)
            request.Resolution = resolution;

        var result = _store.UpdateTicket(ticketId, request);
        Console.Error.WriteLine($"UpdateTicket result: {result.Success} - {result.Message}");

        return JsonSerializer.Serialize(result, JsonOptions);
    }
}
