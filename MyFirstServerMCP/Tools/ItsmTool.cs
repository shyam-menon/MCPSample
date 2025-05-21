using System.ComponentModel;
using MyFirstServerMCP.Models;
using MyFirstServerMCP.Services;

namespace MyFirstServerMCP.Tools;

[McpServerToolType]
public class ItsmTool
{
    private readonly ItsmService _itsmService;
    
    public ItsmTool(ItsmService itsmService)
    {
        _itsmService = itsmService;
    }
    
    [McpServerTool, Description("Create a new ITSM incident")]
    public ItsmIncident CreateIncident(string title, string description, string? priority = null, string? assignedTo = null)
    {
        return _itsmService.CreateIncident(title, description, priority, assignedTo);
    }
    
    [McpServerTool, Description("Retrieve incident details")]
    public ItsmIncident? GetIncident(int id)
    {
        return _itsmService.GetIncidentById(id);
    }
    
    [McpServerTool, Description("Get all incidents")]
    public List<ItsmIncident> GetAllIncidents()
    {
        return _itsmService.GetAllIncidents();
    }
    
    [McpServerTool, Description("Update an existing incident")]
    public ItsmIncident? UpdateIncident(
        int id, 
        string? title = null, 
        string? description = null, 
        string? priority = null, 
        string? status = null, 
        string? assignedTo = null)
    {
        return _itsmService.UpdateIncident(id, title, description, priority, status, assignedTo);
    }
}
