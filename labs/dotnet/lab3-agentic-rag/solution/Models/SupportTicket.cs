namespace Lab3Solution.Models;

/// <summary>
/// Represents a support ticket from the search index.
/// </summary>
public class SupportTicket
{
    public string Id { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}
