namespace MyFirstServerMCP.Models;

public class ItsmIncident
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string Status { get; set; } = "New"; // New, InProgress, Resolved, Closed
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ResolvedAt { get; set; }
}
