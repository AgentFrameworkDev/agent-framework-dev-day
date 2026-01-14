// Copyright (c) Microsoft. All rights reserved.
// REST API Controller for Customer Support Tickets

using RemoteServer.Models;
using RemoteServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace RemoteServer.Controllers;

/// <summary>
/// REST API endpoints for managing customer support tickets.
/// These endpoints can be called directly or via MCP tools.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly TicketStore _ticketStore;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(TicketStore ticketStore, ILogger<TicketsController> logger)
    {
        _ticketStore = ticketStore;
        _logger = logger;
    }

    /// <summary>
    /// Gets all support tickets.
    /// </summary>
    /// <param name="status">Optional filter by status (Open, InProgress, Resolved, Closed)</param>
    [HttpGet]
    public ActionResult<IEnumerable<SupportTicket>> GetTickets([FromQuery] TicketStatus? status = null)
    {
        _logger.LogInformation("GET /api/tickets called with status filter: {Status}", status?.ToString() ?? "all");

        var tickets = status.HasValue
            ? _ticketStore.GetTicketsByStatus(status.Value)
            : _ticketStore.GetAllTickets();

        return Ok(tickets);
    }

    /// <summary>
    /// Gets a specific ticket by ID.
    /// </summary>
    /// <param name="id">The ticket ID (e.g., TKT-001)</param>
    [HttpGet("{id}")]
    public ActionResult<SupportTicket> GetTicket(string id)
    {
        _logger.LogInformation("GET /api/tickets/{TicketId} called", id);

        var ticket = _ticketStore.GetTicket(id);
        if (ticket == null)
        {
            return NotFound(new { message = $"Ticket '{id}' not found." });
        }

        return Ok(ticket);
    }

    /// <summary>
    /// Updates a support ticket.
    /// </summary>
    /// <param name="id">The ticket ID to update</param>
    /// <param name="request">The update request containing fields to modify</param>
    [HttpPut("{id}")]
    [HttpPatch("{id}")]
    public ActionResult<TicketResult> UpdateTicket(string id, [FromBody] UpdateTicketRequest request)
    {
        _logger.LogInformation("PUT /api/tickets/{TicketId} called", id);

        var result = _ticketStore.UpdateTicket(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
