// Copyright (c) Microsoft. All rights reserved.
// MCP Tools for Customer Support Ticket operations (Local MCP Server)
// Same tools as McpBridge for consistency

using System.ComponentModel;
using System.Text.Json;
using McpLocal.Models;
using McpLocal.Services;
using ModelContextProtocol.Server;

namespace McpLocal.Tools;

/// <summary>
/// MCP Tools for reading and updating customer support tickets.
/// Same tools as McpBridge for API consistency.
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
    /// Gets all support tickets with optional limit.
    /// </summary>
    [McpServerTool]
    [Description("Gets all support tickets with optional limit")]
    public string GetAllTickets(
        [Description("Maximum number of tickets to return (default: 5)")] 
        int maxResults = 5)
    {
        Console.Error.WriteLine($"GetAllTickets called - maxResults: {maxResults}");

        var tickets = _store.GetAllTickets();
        var result = tickets.Take(maxResults).ToList();
        
        Console.Error.WriteLine($"Returning {result.Count} tickets");
        return JsonSerializer.Serialize(result, JsonOptions);
    }

    /// <summary>
    /// Gets a support ticket by ID.
    /// </summary>
    [McpServerTool]
    [Description("Gets a support ticket by ID")]
    public string GetTicket(
        [Description("The ticket ID (e.g., TICKET-001)")] 
        string ticketId)
    {
        Console.Error.WriteLine($"GetTicket called - ID: '{ticketId}'");

        if (string.IsNullOrWhiteSpace(ticketId))
        {
            return JsonSerializer.Serialize(new { error = "ticketId is required" }, JsonOptions);
        }

        var ticket = _store.GetTicket(ticketId);
        if (ticket == null)
        {
            return JsonSerializer.Serialize(new { error = $"Ticket '{ticketId}' not found" }, JsonOptions);
        }

        return JsonSerializer.Serialize(ticket, JsonOptions);
    }

    /// <summary>
    /// Updates a support ticket status.
    /// </summary>
    [McpServerTool]
    [Description("Updates a support ticket status")]
    public string UpdateTicket(
        [Description("The ticket ID")] 
        string ticketId,
        [Description("The new status (Open, In Progress, Resolved, Closed)")] 
        string status)
    {
        Console.Error.WriteLine($"UpdateTicket called - ID: '{ticketId}', Status: '{status}'");

        if (string.IsNullOrWhiteSpace(ticketId))
        {
            return JsonSerializer.Serialize(new { error = "ticketId is required" }, JsonOptions);
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            return JsonSerializer.Serialize(new { error = "status is required" }, JsonOptions);
        }

        // Parse status
        TicketStatus? newStatus = status.ToLowerInvariant() switch
        {
            "open" => TicketStatus.Open,
            "in progress" or "inprogress" => TicketStatus.InProgress,
            "resolved" => TicketStatus.Resolved,
            "closed" => TicketStatus.Closed,
            _ => null
        };

        if (newStatus == null)
        {
            return JsonSerializer.Serialize(new { error = $"Invalid status '{status}'. Use: Open, In Progress, Resolved, Closed" }, JsonOptions);
        }

        var result = _store.UpdateTicket(ticketId, new UpdateTicketRequest { Status = newStatus.Value });
        
        if (result.Success)
        {
            return $"Ticket '{ticketId}' status updated to '{status}'";
        }

        return JsonSerializer.Serialize(new { error = result.Message }, JsonOptions);
    }
}
