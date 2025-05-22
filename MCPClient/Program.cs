using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;

namespace MCPClient
{
    public class Program
    {
        private static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        private static readonly ILogger Logger = LoggerFactory.CreateLogger<Program>();
        private static readonly HttpClient HttpClient = new HttpClient();

        public static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                ShowUsage();
                return;
            }

            string transportType = args[0].ToLower();

            try
            {
                // Create an MCP client based on the transport type
                IMcpClient client;
                if (transportType == "stdio")
                {
                    if (args.Length < 2)
                    {
                        Console.WriteLine("For stdio transport, you must provide a command");
                        ShowUsage();
                        return;
                    }

                    string command = args[1];
                    string[] arguments = args.Skip(2).ToArray();

                    client = new StdioMcpClient(command, arguments);
                    Logger.LogInformation($"Created stdio transport with command: {command} {string.Join(" ", arguments)}");
                }
                else if (transportType == "sse")
                {
                    if (args.Length < 2)
                    {
                        Console.WriteLine("For SSE transport, you must provide a URL");
                        ShowUsage();
                        return;
                    }

                    string url = args[1];
                    client = new SseMcpClient(url);
                    Logger.LogInformation($"Created SSE transport with URL: {url}");
                }
                else
                {
                    Console.WriteLine($"Unsupported transport type: {transportType}");
                    ShowUsage();
                    return;
                }

                // Connect to the server
                await client.ConnectAsync();
                Logger.LogInformation("Connected to MCP server successfully");

                // List available tools
                var tools = await client.GetToolsAsync();
                Logger.LogInformation("Available tools:");
                foreach (var tool in tools)
                {
                    Logger.LogInformation($"- {tool.Name}: {tool.Description}");
                }

                // Interactive mode for tool execution
                await RunInteractiveModeAsync(client);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error: {ex.Message}");
                Logger.LogDebug(ex.ToString());
                return;
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  MCPClient <transport-type> [options]");
            Console.WriteLine("  transport-type: 'stdio' or 'sse'");
            Console.WriteLine("  For stdio: MCPClient stdio <command> [arguments...]");
            Console.WriteLine("  For SSE: MCPClient sse <url>");
        }

        private static async Task RunInteractiveModeAsync(IMcpClient client)
        {
            Console.WriteLine("\nInteractive Mode - Enter commands in the format: <tool-name> <param1=value1> <param2=value2> ...");
            Console.WriteLine("Type 'exit' to quit");

            while (true)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit")
                {
                    break;
                }

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 1)
                {
                    Console.WriteLine("Invalid command format");
                    continue;
                }

                string toolName = parts[0];
                var parameters = new Dictionary<string, object?>();

                // Check if the tool requires parameters
                if (toolName == "ItsmTool.GetIncident" && parts.Length == 1)
                {
                    Console.WriteLine("This tool requires an 'id' parameter. Example: ItsmTool.GetIncident id=1");
                    continue;
                }
                else if (toolName == "TodoTool.GetTodo" && parts.Length == 1)
                {
                    Console.WriteLine("This tool requires an 'id' parameter. Example: TodoTool.GetTodo id=1");
                    continue;
                }
                else if (toolName == "TodoTool.DeleteTodo" && parts.Length == 1)
                {
                    Console.WriteLine("This tool requires an 'id' parameter. Example: TodoTool.DeleteTodo id=1");
                    continue;
                }
                else if (toolName == "TodoTool.CreateTodo" && parts.Length == 1)
                {
                    Console.WriteLine("This tool requires 'title' and 'description' parameters. Example: TodoTool.CreateTodo title=\"New Task\" description=\"Task details\"");
                    continue;
                }
                else if (toolName == "WeatherTool.GetWeather" && parts.Length == 1)
                {
                    Console.WriteLine("This tool requires a 'location' parameter. Example: WeatherTool.GetWeather location=\"New York\"");
                    continue;
                }

                // Parse parameters in the format param=value
                var paramParts = parts.Skip(1).ToArray();
                if (paramParts.Length > 0)
                {
                    // Check if we have parameters in the format param=value
                    bool hasNamedParams = paramParts.Any(p => p.Contains('='));
                    
                    if (hasNamedParams)
                    {
                        // Parse parameters in the format param=value
                        foreach (var paramPart in paramParts)
                        {
                            int equalsIndex = paramPart.IndexOf('=');
                            if (equalsIndex > 0)
                            {
                                string paramName = paramPart.Substring(0, equalsIndex);
                                string paramValue = paramPart.Substring(equalsIndex + 1);
                                
                                // Remove quotes if present
                                if (paramValue.StartsWith('"') && paramValue.EndsWith('"'))
                                {
                                    paramValue = paramValue.Substring(1, paramValue.Length - 2);
                                }
                                
                                // Convert to appropriate type if needed
                                if (int.TryParse(paramValue, out int intValue) && 
                                    (toolName.Contains("GetIncident") || toolName.Contains("GetTodo") || 
                                     toolName.Contains("DeleteTodo") || paramName == "id"))
                                {
                                    parameters[paramName] = intValue;
                                }
                                else
                                {
                                    parameters[paramName] = paramValue;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Handle special case for EchoTool.Echo and EchoTool.ReverseEcho with a single unnamed parameter
                        if (toolName.Contains("Echo"))
                        {
                            // Join all remaining parts as the message parameter
                            string message = string.Join(" ", paramParts);
                            // Remove quotes if present
                            if (message.StartsWith('"') && message.EndsWith('"'))
                            {
                                message = message.Substring(1, message.Length - 2);
                            }
                            parameters["message"] = message;
                        }
                        else
                        {
                            Console.WriteLine("Parameters must be in the format param=value");
                            continue;
                        }
                    }
                }

                try
                {
                    var result = await client.InvokeToolAsync(toolName, parameters);
                    Console.WriteLine("Result:");
                    foreach (var content in result)
                    {
                        if (content.Type == "text")
                        {
                            Console.WriteLine(content.Text);
                        }
                        else
                        {
                            Console.WriteLine($"[{content.Type}]: {JsonSerializer.Serialize(content)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }

    // Define interfaces and classes for our MCP client implementation
    public interface IMcpClient
    {
        Task ConnectAsync();
        Task<List<McpTool>> GetToolsAsync();
        Task<List<McpContent>> InvokeToolAsync(string toolName, Dictionary<string, object?> parameters);
    }

    public class McpTool
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class McpContent
    {
        public string Type { get; set; } = "text";
        public string Text { get; set; } = "";
        public JsonElement? Data { get; set; }
    }

    // Implementation for stdio-based MCP servers
    public class StdioMcpClient : IMcpClient
    {
        private readonly string _command;
        private readonly string[] _arguments;
        private Process? _process;
        private readonly ILogger _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<StdioMcpClient>();

        public StdioMcpClient(string command, string[] arguments)
        {
            _command = command;
            _arguments = arguments;
        }

        public async Task ConnectAsync()
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _command,
                    Arguments = string.Join(" ", _arguments),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            _process.Start();
            _logger.LogInformation($"Started process: {_command} {string.Join(" ", _arguments)}");

            // Wait a moment to ensure the process is ready
            await Task.Delay(500);
        }

        public async Task<List<McpTool>> GetToolsAsync()
        {
            if (_process == null)
            {
                throw new InvalidOperationException("Not connected to an MCP server. Call ConnectAsync first.");
            }

            // Send a request to list tools
            var request = new { jsonrpc = "2.0", id = 1, method = "mcp.listTools" };
            string requestJson = JsonSerializer.Serialize(request);

            await _process.StandardInput.WriteLineAsync(requestJson);
            await _process.StandardInput.FlushAsync();

            // Read the response
            string? responseJson = await _process.StandardOutput.ReadLineAsync();
            if (string.IsNullOrEmpty(responseJson))
            {
                return new List<McpTool>();
            }

            try
            {
                var response = JsonSerializer.Deserialize<JsonElement>(responseJson);
                if (response.TryGetProperty("result", out var result) && 
                    result.TryGetProperty("tools", out var toolsArray))
                {
                    var tools = new List<McpTool>();
                    foreach (var tool in toolsArray.EnumerateArray())
                    {
                        string name = tool.GetProperty("name").GetString() ?? "";
                        string description = "";
                        if (tool.TryGetProperty("description", out var desc))
                        {
                            description = desc.GetString() ?? "";
                        }

                        tools.Add(new McpTool { Name = name, Description = description });
                    }
                    return tools;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error parsing listTools response: {ex.Message}");
            }

            return new List<McpTool>();
        }

        public async Task<List<McpContent>> InvokeToolAsync(string toolName, Dictionary<string, object?> parameters)
        {
            if (_process == null)
            {
                throw new InvalidOperationException("Not connected to an MCP server. Call ConnectAsync first.");
            }

            // Send a request to invoke the tool
            var request = new 
            { 
                jsonrpc = "2.0", 
                id = 2, 
                method = "mcp.callTool",
                @params = new { name = toolName, arguments = parameters }
            };
            string requestJson = JsonSerializer.Serialize(request);

            await _process.StandardInput.WriteLineAsync(requestJson);
            await _process.StandardInput.FlushAsync();

            // Read the response
            string? responseJson = await _process.StandardOutput.ReadLineAsync();
            if (string.IsNullOrEmpty(responseJson))
            {
                return new List<McpContent>();
            }

            try
            {
                var response = JsonSerializer.Deserialize<JsonElement>(responseJson);
                if (response.TryGetProperty("result", out var result) && 
                    result.TryGetProperty("content", out var contentArray))
                {
                    var contents = new List<McpContent>();
                    foreach (var content in contentArray.EnumerateArray())
                    {
                        string type = content.GetProperty("type").GetString() ?? "text";
                        string text = "";
                        if (content.TryGetProperty("text", out var textElement))
                        {
                            text = textElement.GetString() ?? "";
                        }

                        contents.Add(new McpContent { Type = type, Text = text });
                    }
                    return contents;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error parsing callTool response: {ex.Message}");
            }

            return new List<McpContent>();
        }
    }

    // Implementation for SSE-based MCP servers
    public class SseMcpClient : IMcpClient
    {
        private readonly string _url;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SseMcpClient>();

        public SseMcpClient(string url)
        {
            _url = url;
        }

        public Task ConnectAsync()
        {
            _logger.LogInformation($"Connected to SSE MCP server at {_url}");
            return Task.CompletedTask;
        }

        public async Task<List<McpTool>> GetToolsAsync()
        {
            try
            {
                // For MyFirstServerMCP, we'll use a hardcoded list of tools based on the actual implementation
                // In a real implementation, you would discover these tools dynamically
                var tools = new List<McpTool>
                {
                    // EchoTool methods
                    new McpTool { Name = "EchoTool.Echo", Description = "Echoes the message back to the client" },
                    new McpTool { Name = "EchoTool.ReverseEcho", Description = "Echoes in reverse the message sent by the client" },
                    
                    // TodoTool methods
                    new McpTool { Name = "TodoTool.GetTodos", Description = "Get all todo items" },
                    new McpTool { Name = "TodoTool.GetTodo", Description = "Get a todo item by ID" },
                    new McpTool { Name = "TodoTool.CreateTodo", Description = "Create a new todo item" },
                    new McpTool { Name = "TodoTool.UpdateTodo", Description = "Update an existing todo item" },
                    new McpTool { Name = "TodoTool.DeleteTodo", Description = "Delete a todo item" },
                    
                    // ItsmTool methods
                    new McpTool { Name = "ItsmTool.CreateIncident", Description = "Create a new ITSM incident" },
                    new McpTool { Name = "ItsmTool.GetIncident", Description = "Retrieve incident details" },
                    new McpTool { Name = "ItsmTool.GetAllIncidents", Description = "Get all incidents" },
                    new McpTool { Name = "ItsmTool.UpdateIncident", Description = "Update an existing incident" },
                    
                    // WeatherTool methods
                    new McpTool { Name = "WeatherTool.GetWeather", Description = "Get current weather for a location" },
                    new McpTool { Name = "WeatherTool.GetWeatherForecast", Description = "Get weather forecast for the next few days" }
                };
                
                return tools;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting tools: {ex.Message}");
            }

            return new List<McpTool>();
        }

        public async Task<List<McpContent>> InvokeToolAsync(string toolName, Dictionary<string, object?> parameters)
        {
            try
            {
                // For MyFirstServerMCP, the expected format is different
                // It expects { "tool": "toolName", "params": { ... } }
                // For debugging, let's log the parameters we're sending
                Console.WriteLine($"Sending parameters: {JsonSerializer.Serialize(parameters)}");
                
                // Create a dynamic object for the params to ensure proper JSON serialization
                var paramsObj = new System.Dynamic.ExpandoObject() as IDictionary<string, object?>;
                foreach (var param in parameters)
                {
                    paramsObj[param.Key] = param.Value;
                }
                
                var request = new 
                { 
                    tool = toolName,
                    @params = paramsObj
                };
                var content = new StringContent(JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_url, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error invoking tool: {response.StatusCode}");
                    return new List<McpContent>();
                }

                string responseJson = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

                if (responseObj.TryGetProperty("result", out var result))
                {
                    // Create a content item from the result
                    var contents = new List<McpContent>
                    {
                        new McpContent 
                        { 
                            Type = "text", 
                            Text = result.ToString() 
                        }
                    };
                    return contents;
                }
                else if (responseObj.TryGetProperty("error", out var error))
                {
                    _logger.LogError($"Server returned error: {error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error invoking tool: {ex.Message}");
            }

            return new List<McpContent>();
        }
    }
}
