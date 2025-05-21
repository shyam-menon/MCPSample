using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Azure.AI.OpenAI;
using Azure;
using MyFirstServerMCP.Models;

namespace MyFirstServerMCP.Services;

public class NlpService
{
    private readonly Kernel? _kernel;
    private readonly McpService _mcpService;
    private readonly ILogger<NlpService> _logger;
    private readonly IChatCompletionService? _chatCompletionService;

    public NlpService(McpService mcpService, IConfiguration configuration, ILogger<NlpService> logger)
    {
        _mcpService = mcpService;
        _logger = logger;

        // Get Azure OpenAI configuration from environment variables
        var apiKey = Environment.GetEnvironmentVariable("AZURE_API_KEY");
        var endpoint = Environment.GetEnvironmentVariable("AZURE_ENDPOINT");
        var deploymentName = Environment.GetEnvironmentVariable("AZURE_DEPLOYMENT_NAME") ?? "GPT-4o";
        
        // Debug: Log the actual values (mask the API key for security)
        string maskedKey = !string.IsNullOrEmpty(apiKey) ? $"{apiKey.Substring(0, 4)}...{apiKey.Substring(apiKey.Length - 4)}" : "<not set>";
        _logger.LogInformation("DEBUGGING - Azure OpenAI Configuration - API Key: {ApiKey}, Endpoint: {Endpoint}, DeploymentName: {DeploymentName}", 
            maskedKey, endpoint ?? "<not set>", deploymentName);
        
        // Log the configuration values (without the actual API key for security)
        _logger.LogInformation("Azure OpenAI Configuration - Endpoint: {Endpoint}, DeploymentName: {DeploymentName}", 
            endpoint, deploymentName);

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint))
        {
            _logger.LogWarning("Azure OpenAI API key or endpoint not found in environment variables. NLP Service will not function properly.");
            _logger.LogInformation("Please set the AZURE_API_KEY and AZURE_ENDPOINT environment variables.");
            _kernel = null;
            _chatCompletionService = null;
        }
        else
        {
            try
            {
                // Initialize Semantic Kernel using the approach from the working console app
                var builder = Kernel.CreateBuilder();
                
                _logger.LogInformation("Initializing Azure OpenAI with deployment: {DeploymentName}", deploymentName);
                
                // Use the same approach as the working console app
                _logger.LogInformation("Using simplified Azure OpenAI configuration");
                
                // Add Azure OpenAI chat completion service with explicit deployment name in both parameters
                builder.AddAzureOpenAIChatCompletion(
                    deploymentName,  // First parameter: deployment name
                    endpoint,        // Second parameter: endpoint
                    apiKey,          // Third parameter: API key
                    deploymentName); // Fourth parameter: model ID (same as deployment name)
                
                _logger.LogInformation("Successfully configured chat completion service");
                
                _kernel = builder.Build();
                
                // Get the chat completion service
                _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
                
                _logger.LogInformation("Semantic Kernel initialized successfully with Azure OpenAI.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Semantic Kernel. NLP Service will not function properly.");
                _logger.LogError("Error details: {Message}", ex.ToString());
                _kernel = null;
                _chatCompletionService = null;
            }
        }
    }

    public async Task<object> ProcessNaturalLanguageQuery(string query)
    {
        if (_kernel == null || _chatCompletionService == null)
        {
            _logger.LogWarning("Azure OpenAI credentials not configured properly");
            return new { error = "Azure OpenAI credentials not configured. Please set AZURE_API_KEY and AZURE_ENDPOINT environment variables." };
        }
        
        // Add a timeout to the OpenAI request to prevent long-hanging connections
        var timeout = TimeSpan.FromSeconds(10);

        try
        {
            // Get information about available tools
            var toolInfo = _mcpService.GetToolInfo();
            
            // Create the system prompt that explains the available tools
            string systemPrompt = CreateSystemPrompt(toolInfo);
            
            // Create chat history with the system prompt
            var chatHistory = new ChatHistory(systemPrompt);
            
            // Add user message
            chatHistory.AddUserMessage(query);

            // Configure the execution settings
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.0f, // More deterministic responses
                MaxTokens = 800
                // Note: Timeout is handled via CancellationToken
            };

            try
            {
                // Create a cancellation token with timeout
                using var cts = new CancellationTokenSource(timeout);
                
                // Get the response from the model
                var completionResult = await _chatCompletionService.GetChatMessageContentAsync(
                    chatHistory, 
                    executionSettings, 
                    _kernel,
                    cts.Token);

                if (completionResult.Content == null)
                {
                    return new { error = "Model returned no content" };
                }

                var content = completionResult.Content;
                _logger.LogInformation("Model response: {Content}", content);

                // Try to extract tool call information
                return ParseToolCall(content);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Azure OpenAI request timed out after {Timeout} seconds", timeout.TotalSeconds);
                return new { error = $"Request to Azure OpenAI timed out after {timeout.TotalSeconds} seconds. The service might be unavailable or the endpoint might be incorrect." };
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error connecting to Azure OpenAI");
                return new { error = $"Could not connect to Azure OpenAI service: {httpEx.Message}. Please check your network connection and endpoint configuration." };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing natural language query");
            return new { error = $"An error occurred while processing your query: {ex.Message}" };
        }
    }

    private string CreateSystemPrompt(List<ToolInfo> tools)
    {
        var toolDescriptions = string.Join("\n", tools.Select(t => 
            $"- {t.Name}: {t.Description}\n  Parameters: {string.Join(", ", t.Parameters.Select(p => $"{p.Name}: {p.Type}{(p.Required ? " (required)" : "")}"))}"));

        return $@"
You are an assistant that helps users interact with a set of tools through natural language.
Your job is to understand the user's request and determine which tool to call with what parameters.

Available tools:
{toolDescriptions}

For each user request, respond ONLY with a JSON object in the following format:
{{
  ""tool"": ""toolName"",
  ""params"": {{
    ""paramName1"": ""paramValue1"",
    ""paramName2"": ""paramValue2""
  }}
}}

Do not include any explanations or any other text outside of the JSON. The JSON must be valid and match exactly the required tool parameters.
";
    }

    private object ParseToolCall(string content)
    {
        try
        {
            // Extract JSON from markdown code blocks if present
            string jsonContent = ExtractJsonFromMarkdown(content);
            _logger.LogInformation("Extracted JSON content: {JsonContent}", jsonContent);
            
            // Try to parse the content as JSON
            var response = JsonDocument.Parse(jsonContent);
            
            if (response.RootElement.TryGetProperty("tool", out var toolElement) &&
                response.RootElement.TryGetProperty("params", out var paramsElement))
            {
                var toolName = toolElement.GetString();
                if (string.IsNullOrEmpty(toolName))
                {
                    return new { error = "No tool name specified in the response." };
                }

                // Execute the tool with the parameters
                var result = _mcpService.ExecuteTool(toolName, paramsElement);
                return new { result };
            }
            
            return new { error = "Invalid response format from model." };
        }
        catch (JsonException ex)
        {
            // If it's not valid JSON, return the raw content with error details
            _logger.LogError(ex, "JSON parsing error");
            return new { error = $"Could not parse response as JSON: {ex.Message}", content };
        }
    }
    
    private string ExtractJsonFromMarkdown(string content)
    {
        // Check if the content is wrapped in markdown code fences
        if (content.Contains("```json") || content.Contains("```"))
        {
            _logger.LogInformation("Detected markdown code block in response");
            
            // Find the start of the JSON content (after ```json or just ```)
            int startIndex = content.IndexOf("```json");
            if (startIndex >= 0)
            {
                startIndex += 7; // Length of "```json"
            }
            else
            {
                startIndex = content.IndexOf("```");
                if (startIndex >= 0)
                {
                    startIndex += 3; // Length of "```"
                }
            }
            
            // Find the end of the JSON content (before the closing ```)
            int endIndex = content.LastIndexOf("```");
            
            if (startIndex >= 0 && endIndex > startIndex)
            {
                // Extract the JSON content between the code fences
                return content.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }
        
        // If no code fences found or extraction failed, return the original content
        return content.Trim();
    }
}
