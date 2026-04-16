// Copyright (c) Microsoft. All rights reserved.
// MCP Tools for Customer Support Ticket operations (MCP Bridge -> REST API)
// These tools forward requests to the REST API backend

using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace McpBridge.Tools;

/// <summary>
/// MCP Tools for reading and updating customer support tickets via REST API.
/// Same tools as Python mcp_bridge for consistency.
/// </summary>
[McpServerToolType]
public sealed class TicketTools
{
    private readonly ILogger<TicketTools> _logger;
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new() 
    { 
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    // REST API base URL
    private const string REST_API_URL = "http://localhost:5060";

    public TicketTools(ILogger<TicketTools> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    /// <summary>
    /// Gets all support tickets from the REST API with optional limit.
    /// </summary>
    [McpServerTool]
    [Description("Gets all support tickets from the REST API with optional limit")]
    public async Task<string> GetAllTickets(
        [Description("Maximum number of tickets to return (default: 5)")] 
        int maxResults = 5)
    {
        _logger.LogInformation("GetAllTickets called with maxResults: {MaxResults}", maxResults);

        try
        {
            var response = await _httpClient.GetAsync($"{REST_API_URL}/api/tickets");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var tickets = JsonSerializer.Deserialize<JsonElement[]>(content, JsonOptions);
                var limited = tickets?.Take(maxResults).ToArray();
                _logger.LogInformation("GetAllTickets returned {Count} tickets", limited?.Length ?? 0);
                return JsonSerializer.Serialize(limited, JsonOptions);
            }
            return JsonSerializer.Serialize(new { Success = false, Message = "Failed to retrieve tickets" }, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling REST API");
            return JsonSerializer.Serialize(new { Success = false, Message = $"Error calling REST API: {ex.Message}" }, JsonOptions);
        }
    }

    /// <summary>
    /// Gets a support ticket by ID from the REST API.
    /// </summary>
    [McpServerTool]
    [Description("Gets a support ticket by ID from the REST API")]
    public async Task<string> GetTicket(
        [Description("The ticket ID (e.g., TICKET-001)")] 
        string ticketId)
    {
        _logger.LogInformation("GetTicket called with ticketId: {TicketId}", ticketId);

        try
        {
            var response = await _httpClient.GetAsync($"{REST_API_URL}/api/tickets/{ticketId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("GetTicket returned: {Content}", content.Substring(0, Math.Min(200, content.Length)));
                return content;
            }
            return JsonSerializer.Serialize(new { Success = false, Message = $"Ticket '{ticketId}' not found" }, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling REST API");
            return JsonSerializer.Serialize(new { Success = false, Message = $"Error calling REST API: {ex.Message}" }, JsonOptions);
        }
    }

    /// <summary>
    /// Updates a support ticket status via the REST API.
    /// </summary>
    [McpServerTool]
    [Description("Updates a support ticket status via the REST API")]
    public async Task<string> UpdateTicket(
        [Description("The ticket ID")] 
        string ticketId,
        [Description("The new status (Open, InProgress, Resolved, Closed)")] 
        string status)
    {
        _logger.LogInformation("UpdateTicket called - ticketId: {TicketId}, status: {Status}", ticketId, status);

        try
        {
            // Use lowercase property name to match ASP.NET Core's default camelCase JSON serialization
            var content = new StringContent(
                JsonSerializer.Serialize(new { status }),
                System.Text.Encoding.UTF8,
                "application/json"
            );
            
            var response = await _httpClient.PutAsync($"{REST_API_URL}/api/tickets/{ticketId}", content);
            _logger.LogInformation("UpdateTicket response: {StatusCode}", response.StatusCode);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("UpdateTicket response body: {Body}", responseContent);
                return responseContent;
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("UpdateTicket failed: {Error}", errorContent);
            return JsonSerializer.Serialize(new { Success = false, Message = $"Failed to update ticket '{ticketId}': {errorContent}" }, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling REST API");
            return JsonSerializer.Serialize(new { Success = false, Message = $"Error calling REST API: {ex.Message}" }, JsonOptions);
        }
    }
}
