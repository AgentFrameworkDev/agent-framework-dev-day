// Copyright (c) Microsoft. All rights reserved.
// In-memory store for customer support tickets (Local MCP)
// Loads tickets from shared data/tickets.json file

using System.Text.Json;
using McpLocal.Models;

namespace McpLocal.Services;

/// <summary>
/// In-memory store for customer support tickets.
/// Loads initial data from shared data/tickets.json file.
/// </summary>
public class TicketStore
{
    private readonly Dictionary<string, SupportTicket> _tickets = new(StringComparer.OrdinalIgnoreCase);
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TicketStore()
    {
        LoadTicketsFromFile();
    }

    private void LoadTicketsFromFile()
    {
        // Find the data folder (root/data/tickets.json)
        var baseDir = AppContext.BaseDirectory;
        var dataFile = FindDataFile(baseDir);
        
        if (dataFile != null && File.Exists(dataFile))
        {
            try
            {
                var json = File.ReadAllText(dataFile);
                var ticketDtos = JsonSerializer.Deserialize<TicketDto[]>(json, JsonOptions);
                
                if (ticketDtos != null)
                {
                    foreach (var dto in ticketDtos)
                    {
                        var ticket = new SupportTicket
                        {
                            Id = dto.Id,
                            CustomerId = dto.CustomerId ?? "",
                            CustomerName = dto.CustomerName ?? "",
                            Subject = dto.Subject ?? "",
                            Description = dto.Description ?? "",
                            Status = ParseStatus(dto.Status),
                            Priority = ParsePriority(dto.Priority),
                            AssignedTo = dto.AssignedTo,
                            CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 5)),
                            UpdatedAt = DateTime.UtcNow
                        };
                        _tickets[ticket.Id] = ticket;
                    }
                    Console.Error.WriteLine($"Loaded {_tickets.Count} tickets from {dataFile}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error loading tickets from file: {ex.Message}");
            }
        }
        
        Console.Error.WriteLine("Warning: Could not load tickets from data/tickets.json, using empty store");
    }

    private static string? FindDataFile(string startPath)
    {
        // Try to find data/tickets.json by traversing up directories
        var current = new DirectoryInfo(startPath);
        while (current != null)
        {
            var dataFile = Path.Combine(current.FullName, "data", "tickets.json");
            if (File.Exists(dataFile))
                return dataFile;
            
            // Also check if we're in dotnet folder and need to go up one more level
            if (current.Name.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            {
                var rootDataFile = Path.Combine(current.Parent?.FullName ?? "", "data", "tickets.json");
                if (File.Exists(rootDataFile))
                    return rootDataFile;
            }
            
            current = current.Parent;
        }
        return null;
    }

    private static TicketStatus ParseStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "open" => TicketStatus.Open,
        "inprogress" or "in progress" => TicketStatus.InProgress,
        "resolved" => TicketStatus.Resolved,
        "closed" => TicketStatus.Closed,
        _ => TicketStatus.Open
    };

    private static TicketPriority ParsePriority(string? priority) => priority?.ToLowerInvariant() switch
    {
        "low" => TicketPriority.Low,
        "medium" => TicketPriority.Medium,
        "high" => TicketPriority.High,
        "critical" => TicketPriority.Critical,
        _ => TicketPriority.Medium
    };

    // DTO for JSON deserialization
    private class TicketDto
    {
        public string Id { get; set; } = "";
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? AssignedTo { get; set; }
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
