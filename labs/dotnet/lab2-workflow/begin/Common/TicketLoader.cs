using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace WorkflowLab.Common;

/// <summary>
/// Utility class for loading tickets from the external JSON data file.
/// Supports loading all tickets, querying by ID, or selecting a random ticket.
/// The tickets path must be configured in appsettings.Local.json via TICKETS_PATH.
/// </summary>
public static class TicketLoader
{
    private static IConfiguration? _configuration;
    private static string? _configPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Gets the configuration, loading from appsettings.Local.json in the dotnet folder.
    /// </summary>
    private static IConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                _configPath = FindConfigPath(AppContext.BaseDirectory);
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(_configPath)
                    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
            }
            return _configuration;
        }
    }

    /// <summary>
    /// Finds the dotnet folder by traversing up the directory tree.
    /// </summary>
    private static string FindConfigPath(string startPath)
    {
        var currentDir = new DirectoryInfo(startPath);
        while (currentDir != null)
        {
            if (currentDir.Name.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            {
                return currentDir.FullName;
            }
            currentDir = currentDir.Parent;
        }
        return startPath;
    }

    /// <summary>
    /// Gets the tickets path. First checks TICKETS_PATH config/env var.
    /// If not set, traverses up the directory tree to find the 'assets' folder containing tickets.json.
    /// </summary>
    private static string GetTicketsPath()
    {
        var ticketsPath = Configuration["TICKETS_PATH"];
        
        if (!string.IsNullOrWhiteSpace(ticketsPath))
        {
            // Resolve relative paths from the config directory
            if (!Path.IsPathRooted(ticketsPath))
            {
                ticketsPath = Path.Combine(_configPath ?? AppContext.BaseDirectory, ticketsPath);
            }
            return Path.GetFullPath(ticketsPath);
        }

        // Auto-discover: traverse up from config path to find assets/tickets.json
        var searchStart = _configPath ?? AppContext.BaseDirectory;
        var current = new DirectoryInfo(searchStart);
        while (current != null)
        {
            var candidate = Path.Combine(current.FullName, "assets", "tickets.json");
            if (File.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }
            current = current.Parent;
        }

        throw new InvalidOperationException(
            "Could not find tickets.json. " +
            "Either set 'TICKETS_PATH' in appsettings.Local.json or as an environment variable, " +
            "or ensure an 'assets' folder containing 'tickets.json' exists in a parent directory.");
    }

    /// <summary>
    /// Loads all tickets from the JSON file.
    /// </summary>
    public static async Task<List<SupportTicket>> LoadAllTicketsAsync()
    {
        var resolvedPath = GetTicketsPath();

        if (!File.Exists(resolvedPath))
        {
            throw new FileNotFoundException($"Tickets file not found at: {resolvedPath}");
        }

        var json = await File.ReadAllTextAsync(resolvedPath);
        var ticketDtos = JsonSerializer.Deserialize<List<TicketDto>>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize tickets.");

        return ticketDtos.Select(MapToSupportTicket).ToList();
    }

    /// <summary>
    /// Gets a ticket by its ID.
    /// </summary>
    public static async Task<SupportTicket?> GetTicketByIdAsync(string ticketId)
    {
        var tickets = await LoadAllTicketsAsync();
        return tickets.FirstOrDefault(t => 
            t.TicketId.Equals(ticketId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a random ticket from the collection.
    /// </summary>
    public static async Task<SupportTicket> GetRandomTicketAsync()
    {
        var tickets = await LoadAllTicketsAsync();
        if (tickets.Count == 0)
        {
            throw new InvalidOperationException("No tickets found in the data file.");
        }

        var random = new Random();
        return tickets[random.Next(tickets.Count)];
    }

    /// <summary>
    /// Gets a ticket by index (1-based for user friendliness).
    /// </summary>
    public static async Task<SupportTicket> GetTicketByIndexAsync(int index)
    {
        var tickets = await LoadAllTicketsAsync();
        if (index < 1 || index > tickets.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), 
                $"Index must be between 1 and {tickets.Count}.");
        }

        return tickets[index - 1];
    }

    /// <summary>
    /// Lists all available tickets (for display purposes).
    /// </summary>
    public static async Task DisplayAvailableTicketsAsync()
    {
        var tickets = await LoadAllTicketsAsync();
        Console.WriteLine("Available tickets:");
        Console.WriteLine(new string('-', 60));
        for (int i = 0; i < tickets.Count; i++)
        {
            var t = tickets[i];
            Console.WriteLine($"  [{i + 1}] {t.TicketId} - {t.Subject} ({t.Priority})");
        }
        Console.WriteLine(new string('-', 60));
    }

    private static SupportTicket MapToSupportTicket(TicketDto dto)
    {
        var priority = dto.Priority?.ToUpperInvariant() switch
        {
            "LOW" => TicketPriority.Low,
            "MEDIUM" => TicketPriority.Medium,
            "HIGH" => TicketPriority.High,
            "CRITICAL" => TicketPriority.Critical,
            _ => TicketPriority.Medium
        };

        return new SupportTicket(
            TicketId: dto.Id ?? "UNKNOWN",
            CustomerId: dto.CustomerId ?? "UNKNOWN",
            CustomerName: dto.CustomerName ?? "Unknown Customer",
            Subject: dto.Subject ?? "No Subject",
            Description: dto.Description ?? "No Description",
            Priority: priority
        );
    }

    /// <summary>
    /// Internal DTO for JSON deserialization.
    /// </summary>
    private sealed record TicketDto(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("customerId")] string? CustomerId,
        [property: JsonPropertyName("customerName")] string? CustomerName,
        [property: JsonPropertyName("subject")] string? Subject,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("priority")] string? Priority,
        [property: JsonPropertyName("assignedTo")] string? AssignedTo
    );
}
