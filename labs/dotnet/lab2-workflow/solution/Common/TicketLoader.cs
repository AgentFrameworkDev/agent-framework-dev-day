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
    /// Gets the configured tickets path from TICKETS_PATH in appsettings.Local.json.
    /// </summary>
    private static string GetTicketsPath()
    {
        var ticketsPath = Configuration["TICKETS_PATH"];
        
        if (string.IsNullOrWhiteSpace(ticketsPath))
        {
            throw new InvalidOperationException(
                "TICKETS_PATH is not configured. " +
                "Set 'TICKETS_PATH' in appsettings.Local.json or as an environment variable. " +
                "Example: \"TICKETS_PATH\": \"..\\\\data\\\\tickets.json\"");
        }

        // Resolve relative paths from the config directory
        if (!Path.IsPathRooted(ticketsPath))
        {
            ticketsPath = Path.Combine(_configPath ?? AppContext.BaseDirectory, ticketsPath);
        }

        return Path.GetFullPath(ticketsPath);
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
