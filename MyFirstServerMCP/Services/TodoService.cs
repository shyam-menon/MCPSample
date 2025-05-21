using MyFirstServerMCP.Models;

namespace MyFirstServerMCP.Services;

public class TodoService
{
    // In-memory storage for demo
    private static readonly List<TodoItem> _todos = new List<TodoItem>();
    private static int _nextId = 1;
    
    public List<TodoItem> GetAll()
    {
        return _todos.ToList();
    }
    
    public TodoItem? GetById(int id)
    {
        return _todos.FirstOrDefault(t => t.Id == id);
    }
    
    public TodoItem Create(string title)
    {
        var newTodo = new TodoItem
        {
            Id = _nextId++,
            Title = title,
            IsCompleted = false
        };
        
        _todos.Add(newTodo);
        return newTodo;
    }
    
    public TodoItem? Update(int id, string? title = null, bool? isCompleted = null)
    {
        var todo = GetById(id);
        if (todo == null)
            return null;
            
        if (title != null)
            todo.Title = title;
            
        if (isCompleted.HasValue)
            todo.IsCompleted = isCompleted.Value;
            
        return todo;
    }
    
    public bool Delete(int id)
    {
        var todo = GetById(id);
        if (todo == null)
            return false;
            
        return _todos.Remove(todo);
    }
}
