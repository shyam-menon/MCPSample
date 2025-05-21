using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using MyFirstServerMCP.Services;

namespace MyFirstServerMCP.Controllers
{
    public class QueryRequest
    {
        public string Query { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("mcp/[controller]")]
    public class NlpController : ControllerBase
    {
        private readonly NlpService _nlpService;
        private readonly ILogger<NlpController> _logger;

        public NlpController(NlpService nlpService, ILogger<NlpController> logger)
        {
            _nlpService = nlpService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessQuery([FromBody] QueryRequest request)
        {
            try {
                _logger.LogInformation("Received request: {RequestJson}", System.Text.Json.JsonSerializer.Serialize(request));
            } catch (Exception ex) {
                _logger.LogError(ex, "Error serializing request");
            }
            
            if (request == null)
            {
                _logger.LogWarning("Request is null");
                return BadRequest(new { error = "Request is null." });
            }
            
            if (string.IsNullOrEmpty(request.Query))
            {
                _logger.LogWarning("Query is empty or null");
                return BadRequest(new { error = "Query is required." });
            }

            try
            {
                _logger.LogInformation("Processing natural language query: {Query}", request.Query);
                var result = await _nlpService.ProcessNaturalLanguageQuery(request.Query);
                _logger.LogInformation("Result: {Result}", System.Text.Json.JsonSerializer.Serialize(result));
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing natural language query: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
