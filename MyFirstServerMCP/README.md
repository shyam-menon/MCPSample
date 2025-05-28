# MyFirstServerMCP

This is a Model Context Protocol (MCP) server implemented using Server-Sent Events (SSE) transport.

## Setup After Cloning

1. Create a `.env` file from the template:
   ```powershell
   Copy-Item .env.template .env
   ```

2. Edit the `.env` file to add your Azure OpenAI credentials:
   ```
   AZURE_API_KEY=your-api-key-here
   AZURE_ENDPOINT=https://your-resource-name.openai.azure.com/
   AZURE_DEPLOYMENT_NAME=GPT-4o
   ```

3. Build and run the server (see below for options)

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

2. The server will be listening at `http://localhost:5001`.
3. Open a web browser and navigate to `http://localhost:5001` to access the test client.
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

## Docker Support

You can run this MCP server in a Docker container:

### Using Docker Compose (Recommended)

1. Copy the `.env.template` file to `.env` and add your Azure OpenAI credentials:
   ```powershell
   Copy-Item .env.template .env
   ```

2. Edit the `.env` file with your actual credentials.

3. Run the server using Docker Compose:
   ```powershell
   docker-compose up -d
   ```

### Using Docker Directly

Build the Docker image:
```powershell
docker build -t myfirstservermcp .
```

Run the container with environment variables:
```powershell
docker run -d -p 5001:5001 `
  -e AZURE_API_KEY="your-api-key" `
  -e AZURE_ENDPOINT="https://your-resource-name.openai.azure.com/" `
  -e AZURE_DEPLOYMENT_NAME="GPT-4o" `
  --name mcpserver myfirstservermcp
```

### Port Configuration

The application uses different ports for local development and Docker:
- Local Development: Port 5000
- Docker Container: Port 5001

This allows you to run both the local server and Docker container simultaneously.

### Accessing the MCP Server

Your MCP server will be accessible at:

Local Development:
- Web interface: http://localhost:5000
- MCP endpoint: http://localhost:5000/mcp
- SSE endpoint: http://localhost:5000/mcp/sse

Docker Container:
- Web interface: http://localhost:5001
- MCP endpoint: http://localhost:5001/mcp
- SSE endpoint: http://localhost:5001/mcp/sse

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

### For Local Development:

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

### For Docker Container:

```json
{
  "servers": {
    "MyFirstServerMCP-Docker": {
      "type": "sse",
      "url": "http://localhost:5001/mcp/sse",
      "executionUrl": "http://localhost:5001/mcp"
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
