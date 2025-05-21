using MyFirstServerMCP.Models;

namespace MyFirstServerMCP.Services;

public class ItsmService
{
    // In-memory storage for demo
    private static readonly List<ItsmIncident> _incidents = new List<ItsmIncident>();
    private static int _nextId = 1;
    
    public List<ItsmIncident> GetAllIncidents()
    {
        return _incidents.ToList();
    }
    
    public ItsmIncident? GetIncidentById(int id)
    {
        return _incidents.FirstOrDefault(i => i.Id == id);
    }
    
    public ItsmIncident CreateIncident(string title, string description, string? priority = null, string? assignedTo = null)
    {
        var newIncident = new ItsmIncident
        {
            Id = _nextId++,
            Title = title,
            Description = description,
            Priority = priority ?? "Medium",
            AssignedTo = assignedTo,
            Status = "New"
        };
        
        _incidents.Add(newIncident);
        return newIncident;
    }
    
    public ItsmIncident? UpdateIncident(
        int id, 
        string? title = null, 
        string? description = null, 
        string? priority = null, 
        string? status = null, 
        string? assignedTo = null)
    {
        var incident = GetIncidentById(id);
        if (incident == null)
            return null;
            
        if (title != null)
            incident.Title = title;
            
        if (description != null)
            incident.Description = description;
            
        if (priority != null)
            incident.Priority = priority;
            
        if (status != null)
        {
            incident.Status = status;
            
            // If status is being set to Resolved or Closed and wasn't before, set ResolvedAt
            if ((status == "Resolved" || status == "Closed") && incident.ResolvedAt == null)
            {
                incident.ResolvedAt = DateTime.Now;
            }
        }
            
        if (assignedTo != null)
            incident.AssignedTo = assignedTo;
            
        return incident;
    }
    
    public bool DeleteIncident(int id)
    {
        var incident = GetIncidentById(id);
        if (incident == null)
            return false;
            
        return _incidents.Remove(incident);
    }
}
