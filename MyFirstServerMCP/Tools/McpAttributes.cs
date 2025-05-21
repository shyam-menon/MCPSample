using System;

namespace MyFirstServerMCP.Tools 
{
    [AttributeUsage(AttributeTargets.Class)]
    public class McpServerToolTypeAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class McpServerToolAttribute : Attribute { }
}
