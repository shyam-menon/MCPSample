# MCP Client

This is a C# client application for connecting to Model Context Protocol (MCP) servers using both stdio-based and SSE-based transports.

## Overview

The Model Context Protocol (MCP) is an open protocol that standardizes how applications provide context to Large Language Models (LLMs). This client application allows you to connect to any MCP-compliant server and interact with the tools it provides.

This implementation uses a custom MCP client that communicates directly with MCP servers using the JSON-RPC protocol, without relying on specific SDK namespaces that might change between versions.

## Connecting to MyFirstMCP and MyFirstServerMCP

### Connecting to MyFirstMCP

MyFirstMCP is a stdio-based MCP server. To connect to it:

```bash
# First, build and run MyFirstMCP in one terminal
cd c:\Code\Personal\MCPSample\MyFirstMCP
dotnet build
dotnet run

# Then, in another terminal, connect to it using the MCPClient
cd c:\Code\Personal\MCPSample\MCPClient
dotnet run stdio dotnet c:\Code\Personal\MCPSample\MyFirstMCP\bin\Debug\net8.0\MyFirstMCP.dll
```

### Connecting to MyFirstServerMCP

MyFirstServerMCP is a web-based MCP server. To connect to it:

```bash
# First, build and run MyFirstServerMCP in one terminal
cd c:\Code\Personal\MCPSample\MyFirstServerMCP
dotnet build
dotnet run

# Then, in another terminal, connect to it using the MCPClient
cd c:\Code\Personal\MCPSample\MCPClient
dotnet run sse http://localhost:5000/Mcp
# Or if running on the default HTTPS port:
# dotnet run sse https://localhost:7274/Mcp
```

## Prerequisites

- .NET 8.0 SDK or later
- Access to an MCP server (either stdio-based or SSE-based)

## Getting Started

### Building the Client

```bash
cd MCPClient
dotnet build
```

### Running the Client

The client supports two transport types:

#### 1. Stdio Transport

```bash
dotnet run stdio <command> [arguments...]
```

Example:

```bash
# Connect to the "everything" sample server
dotnet run stdio npx -y @modelcontextprotocol/server-everything
```

#### 2. SSE Transport

```bash
dotnet run sse <url>
```

Example:

```bash
# Connect to an SSE-based MCP server
dotnet run sse https://your-sse-server-url.com/mcp
```

## Interactive Mode

Once connected to an MCP server, the client enters interactive mode where you can:

1. View all available tools provided by the server
2. Execute tools with parameters

### Executing Tools

To execute a tool, use the following format:

```text
<tool-name> <param1=value1> <param2=value2> ...
```

Example:

```text
echo message=Hello World
```

Type `exit` to quit the interactive mode.

## Extending for LLM Integration

To integrate this MCP client with an LLM, you would typically:

1. Connect to the MCP server using the client
2. Retrieve available tools using `GetToolsAsync()`
3. Pass these tools to your LLM framework
4. When the LLM decides to use a tool, call `InvokeToolAsync()` with the appropriate parameters
5. Process the tool results and provide them back to the LLM

This pattern allows LLMs to access external tools and data sources through the MCP protocol.

## Resources

- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [MCP Official Documentation](https://modelcontextprotocol.io/)
- [Protocol Specification](https://spec.modelcontextprotocol.io/)
