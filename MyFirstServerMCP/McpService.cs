using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using MyFirstServerMCP.Services;
using MyFirstServerMCP.Tools;

namespace MyFirstServerMCP;

public class McpService
{
    private readonly TodoService _todoService;
    private readonly ItsmService _itsmService;
    private readonly Dictionary<string, (Type ClassType, MethodInfo Method, bool IsStatic, string Description)> _tools = new();

    public McpService(TodoService todoService, ItsmService itsmService)
    {
        _todoService = todoService;
        _itsmService = itsmService;
        RegisterTools();
    }

    private void RegisterTools()
    {
        // Register all tools from the Tools namespace
        var toolTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.Namespace == "MyFirstServerMCP.Tools" && t.GetCustomAttribute<McpServerToolTypeAttribute>() != null);

        foreach (var toolType in toolTypes)
        {
            var methods = toolType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<McpServerToolAttribute>() != null);

            foreach (var method in methods)
            {
                // Format the tool name as "className_methodName" (lowercase first letter of class name)
                var className = toolType.Name;
                var formattedClassName = char.ToLowerInvariant(className[0]) + className.Substring(1);
                var toolName = $"{formattedClassName}_{method.Name}";

                // Get description from DescriptionAttribute if available
                var descriptionAttribute = method.GetCustomAttribute<DescriptionAttribute>();
                var description = descriptionAttribute?.Description ?? "No description available";

                _tools[toolName] = (toolType, method, method.IsStatic, description);
            }
        }
    }

    public List<ToolInfo> GetToolInfo()
    {
        var toolInfoList = new List<ToolInfo>();

        foreach (var (toolName, (classType, methodInfo, isStatic, description)) in _tools)
        {
            var parameters = methodInfo.GetParameters().Select(p => new ParameterInfo
            {
                Name = p.Name ?? "",
                Type = p.ParameterType.Name,
                Required = !p.IsOptional
            }).ToList();

            toolInfoList.Add(new ToolInfo
            {
                Name = toolName,
                Description = description,
                Parameters = parameters
            });
        }

        return toolInfoList;
    }

    public object ExecuteTool(string toolName, JsonElement parameters)
    {
        if (!_tools.TryGetValue(toolName, out var toolInfo))
        {
            throw new Exception($"Tool '{toolName}' not found");
        }

        // Create instance of the tool class if needed
        object? instance = null;
        if (!toolInfo.IsStatic)
        {
            var toolType = toolInfo.ClassType;
            
            if (toolType == typeof(TodoTool))
            {
                instance = new TodoTool(_todoService);
            }
            else if (toolType == typeof(ItsmTool))
            {
                instance = new ItsmTool(_itsmService);
            }
            else
            {
                // For other tool classes without specific constructor requirements
                instance = Activator.CreateInstance(toolType);
            }
        }

        // Parse parameters and call method
        var parameterValues = ParseParameters(toolInfo.Method, parameters);
        var result = toolInfo.Method.Invoke(instance, parameterValues);

        // Handle async results (Task<T>)
        if (result is Task task)
        {
            // Wait for the task to complete and get the result
            task.GetAwaiter().GetResult();
            
            // Get the Result property if it's a Task<T>
            if (task.GetType().IsGenericType)
            {
                var resultProperty = task.GetType().GetProperty("Result");
                return resultProperty?.GetValue(task) ?? string.Empty;
            }
            
            return string.Empty;  // For Task without result
        }

        return result ?? string.Empty;
    }

    private object[] ParseParameters(MethodInfo method, JsonElement parametersJson)
    {
        var parameters = method.GetParameters();
        var paramValues = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            if (parametersJson.TryGetProperty(param.Name!, out var jsonValue))
            {
                // Convert the JSON value to the parameter type
                paramValues[i] = JsonSerializer.Deserialize(jsonValue.GetRawText(), param.ParameterType, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
                    ?? param.DefaultValue!;
            }
            else if (param.HasDefaultValue)
            {
                paramValues[i] = param.DefaultValue!;
            }
            else
            {
                throw new Exception($"Required parameter '{param.Name}' not provided for tool");
            }
        }

        return paramValues;
    }
}

public class ToolInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ParameterInfo> Parameters { get; set; } = new();
}

public class ParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Required { get; set; }
}
