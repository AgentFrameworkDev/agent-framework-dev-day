// Copyright (c) Microsoft. All rights reserved.
// MCP Tools that call the REST API via HTTP
// This is the bridge between MCP protocol and REST API

using System.ComponentModel;
using System.Text.Json;
using McpBridge.Models;
using ModelContextProtocol.Server;

namespace McpBridge.Tools;

/// <summary>
/// MCP Tools that call the REST API via HTTP.
/// This demonstrates how MCP servers act as bridges to existing REST services.
/// </summary>
[McpServerToolType]
public sealed class TicketTools
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TicketTools> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() 
    { 
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public TicketTools(IHttpClientFactory httpClientFactory, ILogger<TicketTools> logger)
    {
        _httpClient = httpClientFactory.CreateClient("RestApi");
        _logger = logger;
    }

    /// <summary>
    /// Gets a support ticket by ID via REST API call.
    /// </summary>
    [McpServerTool]
    [Description("Gets a customer support ticket by calling the REST API. If no ID is provided, returns all tickets. Example IDs: TKT-001, TKT-002")]
    public async Task<string> GetTicket(
        [Description("The ticket ID to retrieve (e.g., TKT-001). Leave empty to get all tickets.")] 
        string? ticketId = null,
        [Description("Optional status filter: Open, InProgress, Resolved, or Closed")] 
        string? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetTicket via REST API - ID: '{TicketId}', Status: '{Status}'", 
            ticketId ?? "(all)", statusFilter ?? "(none)");

        try
        {
            string url;
            if (!string.IsNullOrWhiteSpace(ticketId))
            {
                // Get specific ticket
                url = $"/api/tickets/{ticketId}";
                _logger.LogInformation("Calling REST API: GET {Url}", _httpClient.BaseAddress + url);
                
                var response = await _httpClient.GetAsync(url, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Serialize(new
                    {
                        Success = false,
                        Message = $"Ticket '{ticketId}' not found. HTTP {(int)response.StatusCode}"
                    }, JsonOptions);
                }

                var ticket = await response.Content.ReadFromJsonAsync<SupportTicket>(JsonOptions, cancellationToken);
                return JsonSerializer.Serialize(new
                {
                    Success = true,
                    Message = $"Found ticket '{ticketId}' via REST API",
                    Ticket = ticket
                }, JsonOptions);
            }
            else
            {
                // Get all tickets (with optional status filter)
                url = string.IsNullOrWhiteSpace(statusFilter) 
                    ? "/api/tickets"
                    : $"/api/tickets?status={statusFilter}";
                    
                _logger.LogInformation("Calling REST API: GET {Url}", _httpClient.BaseAddress + url);
                
                var tickets = await _httpClient.GetFromJsonAsync<List<SupportTicket>>(url, JsonOptions, cancellationToken);
                
                return JsonSerializer.Serialize(new
                {
                    Success = true,
                    Message = $"Found {tickets?.Count ?? 0} ticket(s) via REST API",
                    Tickets = tickets
                }, JsonOptions);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "REST API call failed");
            return JsonSerializer.Serialize(new
            {
                Success = false,
                Message = $"Failed to call REST API: {ex.Message}. Is the REST API server running?"
            }, JsonOptions);
        }
    }

    /// <summary>
    /// Updates a support ticket via REST API call.
    /// </summary>
    [McpServerTool]
    [Description("Updates a customer support ticket by calling the REST API. You can change status, priority, assignee, or resolution.")]
    public async Task<string> UpdateTicket(
        [Description("The ticket ID to update (required, e.g., TKT-001)")] 
        string ticketId,
        [Description("New status: Open, InProgress, Resolved, or Closed")] 
        string? status = null,
        [Description("New priority: Low, Medium, High, or Critical")] 
        string? priority = null,
        [Description("Person or team to assign the ticket to")] 
        string? assignedTo = null,
        [Description("Resolution notes (typically added when resolving/closing)")] 
        string? resolution = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UpdateTicket via REST API - ID: '{TicketId}'", ticketId);

        if (string.IsNullOrWhiteSpace(ticketId))
        {
            return JsonSerializer.Serialize(new
            {
                Success = false,
                Message = "Ticket ID is required."
            }, JsonOptions);
        }

        try
        {
            // Build update request
            var request = new UpdateTicketRequest();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<TicketStatus>(status, true, out var statusValue))
                    request.Status = statusValue;
                else
                    return JsonSerializer.Serialize(new
                    {
                        Success = false,
                        Message = $"Invalid status '{status}'. Valid: Open, InProgress, Resolved, Closed"
                    }, JsonOptions);
            }

            if (!string.IsNullOrWhiteSpace(priority))
            {
                if (Enum.TryParse<TicketPriority>(priority, true, out var priorityValue))
                    request.Priority = priorityValue;
                else
                    return JsonSerializer.Serialize(new
                    {
                        Success = false,
                        Message = $"Invalid priority '{priority}'. Valid: Low, Medium, High, Critical"
                    }, JsonOptions);
            }

            if (assignedTo != null)
                request.AssignedTo = assignedTo;

            if (resolution != null)
                request.Resolution = resolution;

            var url = $"/api/tickets/{ticketId}";
            _logger.LogInformation("Calling REST API: PUT {Url}", _httpClient.BaseAddress + url);

            var response = await _httpClient.PutAsJsonAsync(url, request, JsonOptions, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<TicketResult>(JsonOptions, cancellationToken);

            return JsonSerializer.Serialize(result, JsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "REST API call failed");
            return JsonSerializer.Serialize(new
            {
                Success = false,
                Message = $"Failed to call REST API: {ex.Message}. Is the REST API server running?"
            }, JsonOptions);
        }
    }
}
