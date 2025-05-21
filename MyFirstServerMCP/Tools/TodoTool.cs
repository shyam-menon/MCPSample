using System.ComponentModel;
using MyFirstServerMCP.Models;
using MyFirstServerMCP.Services;

namespace MyFirstServerMCP.Tools;

[McpServerToolType]
public class TodoTool
{
    private readonly TodoService _todoService;
    
    public TodoTool(TodoService todoService)
    {
        _todoService = todoService;
    }
    
    [McpServerTool, Description("Get all todo items")]
    public List<TodoItem> GetTodos()
    {
        return _todoService.GetAll();
    }
    
    [McpServerTool, Description("Get a todo item by ID")]
    public TodoItem? GetTodo(int id)
    {
        return _todoService.GetById(id);
    }
    
    [McpServerTool, Description("Create a new todo item")]
    public TodoItem CreateTodo(string title)
    {
        return _todoService.Create(title);
    }
    
    [McpServerTool, Description("Update an existing todo item")]
    public TodoItem? UpdateTodo(int id, string? title = null, bool? isCompleted = null)
    {
        return _todoService.Update(id, title, isCompleted);
    }
    
    [McpServerTool, Description("Delete a todo item")]
    public bool DeleteTodo(int id)
    {
        return _todoService.Delete(id);
    }
}
