# MyFirstServerMCP

This is a Model Context Protocol (MCP) server implemented using Server-Sent Events (SSE) transport.

## Features

- **Server-Sent Events Transport**: Uses HTTP with Server-Sent Events for real-time communication
- **Todo Management**: Create, read, update, and delete todo items
- **ITSM Incidents**: Create and manage IT service management incidents
- **Weather Information**: Get current weather and forecasts (mock data)
- **Echo Tools**: Simple echo and reverse echo functionality
- **Natural Language Processing**: Interact with tools using natural language via Azure OpenAI

## How to Use

1. Start the server by running the project:
```
cd C:\Code\Personal\MCPSample\MyFirstServerMCP
dotnet run
```

2. The server will be listening at `http://localhost:5000`.
3. Open a web browser and navigate to `http://localhost:5000` to access the test client.
4. Connect to the SSE endpoint by clicking "Connect to SSE" in the client.
5. Execute tools by selecting the tool and entering parameters in JSON format.
6. Alternatively, use the Natural Language Query panel to interact with tools using everyday language.

## Configuration

To configure Azure OpenAI for natural language processing, set the following environment variables:

```shell
AZURE_API_KEY=your-api-key-here
AZURE_ENDPOINT=https://your-resource-name.openai.azure.com/
AZURE_DEPLOYMENT_NAME=GPT-4o
```

If `AZURE_DEPLOYMENT_NAME` is not set, it will default to "GPT-4o".

## MCP Tool Interface

The following tools are available via the MCP interface:

### Echo Tools
- `echoTool_Echo`: Echoes a message back to the client
- `echoTool_ReverseEcho`: Echoes a message in reverse back to the client

### Todo Tools
- `todoTool_GetTodos`: Get a list of all todo items
- `todoTool_GetTodo`: Get a specific todo item by ID
- `todoTool_CreateTodo`: Create a new todo item
- `todoTool_UpdateTodo`: Update an existing todo item
- `todoTool_DeleteTodo`: Delete a todo item

### ITSM Tools
- `itsmTool_GetAllIncidents`: Get a list of all ITSM incidents
- `itsmTool_GetIncident`: Get a specific incident by ID
- `itsmTool_CreateIncident`: Create a new ITSM incident
- `itsmTool_UpdateIncident`: Update an existing ITSM incident

### Weather Tools
- `weatherTool_GetWeather`: Get current weather for a location
- `weatherTool_GetWeatherForecast`: Get a weather forecast for a location

## Setting up in VS Code or other MCP clients

To use this server with VS Code or other MCP clients that support SSE transport, configure the client to connect to the SSE endpoint:

```json
{
  "servers": {
    "MyFirstServerMCP": {
      "type": "sse",
      "url": "http://localhost:5000/mcp/sse",
      "executionUrl": "http://localhost:5000/mcp"
    }
  }
}
```

## Project Structure

- **Controllers**: Contains the `McpController` that handles HTTP requests and SSE communication
- **Models**: Data models for Todo items, ITSM incidents, and Weather data
- **Services**: Business logic for Todo and ITSM services
- **Tools**: MCP tools exposed to clients
- **wwwroot**: Contains the test client interface
