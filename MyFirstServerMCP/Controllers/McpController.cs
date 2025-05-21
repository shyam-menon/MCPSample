using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using MyFirstServerMCP;

namespace MyFirstServerMCP.Controllers;

[ApiController]
[Route("[controller]")]
public class McpController : ControllerBase
{
    private readonly McpService _mcpService;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public McpController(McpService mcpService)
    {
        _mcpService = mcpService;
    }

    [HttpPost]
    public IActionResult ExecuteTool([FromBody] JsonDocument request)
    {
        try
        {
            if (!request.RootElement.TryGetProperty("tool", out var toolElement) ||
                !request.RootElement.TryGetProperty("params", out var paramsElement))
            {
                return BadRequest(new { error = "Invalid request format. Requires 'tool' and 'params' properties." });
            }

            var toolName = toolElement.GetString();
            
            if (string.IsNullOrEmpty(toolName))
            {
                return BadRequest(new { error = "Tool name must be provided." });
            }

            var result = _mcpService.ExecuteTool(toolName, paramsElement);
            return Ok(new { result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("sse")]
    public async Task StreamSSE()
    {
        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";

        await Response.Body.FlushAsync();

        using var cts = new CancellationTokenSource();
        HttpContext.RequestAborted.Register(() => cts.Cancel());

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                // Simple heartbeat event to keep connection alive
                string data = $"data: {{\"type\":\"heartbeat\",\"timestamp\":\"{DateTime.UtcNow:O}\"}}\n\n";
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                await Response.Body.WriteAsync(bytes, 0, bytes.Length, cts.Token);
                await Response.Body.FlushAsync(cts.Token);

                await Task.Delay(5000, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected, this is expected
        }
    }
}
