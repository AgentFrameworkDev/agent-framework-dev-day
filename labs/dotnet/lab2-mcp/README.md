# Lab 3 - Model Context Protocol (MCP) Workshop

This lab demonstrates how to build and consume MCP (Model Context Protocol) servers using Microsoft Agent Framework in .NET 10.

## Learning Goals

1. **Build Local MCP Servers** - Using STDIO transport (.NET)
2. **Build Remote MCP Servers** - Using HTTP/SSE transport that calls REST APIs
3. **Consume MCP Servers** - From AI Agents using Microsoft Agent Framework

## � Interactive Notebook

To explore MCP concepts interactively, open and run the Jupyter notebook:
```bash
cd lab
jupyter notebook mcp-concepts.ipynb
```
Or open `lab/mcp-concepts.ipynb` directly in VS Code.

## �📝 Lab Exercises

For hands-on exercises, see **[lab/EXERCISES.md](lab/EXERCISES.md)**.

## Architecture

```
                                    LOCAL MCP (STDIO)
                              +-----------------------------+
                              |                             |
+------------------+          |   +-----------------------+ |
|                  | ---------+-->|      McpLocal         | |
|  McpAgentClient  |          |   |      (STDIO)          | |
|                  |          |   +-----------------------+ |
|   (Consumes      |          |                             |
|    MCP Servers)  |          +-----------------------------+
|                  |
+--------+---------+
         |
         |                    REMOTE MCP (HTTP/SSE -> REST)
         |                    +---------------------------------------------+
         |                    |                                             |
         |   HTTP/SSE         |   +-------------+   HTTP    +-------------+ |
         +------------------->|   |  McpBridge  | --------> |RemoteServer | |
                              |   |  Port: 5070 |           |  Port: 5060 | |
                              |   |  /sse       |           | (REST API)  | |
                              |   +-------------+           +-------------+ |
                              |                                             |
                              +---------------------------------------------+
```

## Project Structure

```
lab3-mcp/
+-- McpLab.sln
+-- README.md
+-- MCP-Concepts.ipynb           # Educational notebook
+-- appsettings.Local.json       # Shared config (in parent dotnet folder)
+-- McpAgentClient/              # AI Agent that consumes MCP servers
+-- McpLocal/                    # Local MCP Server (.NET, STDIO)
+-- McpBridge/                   # MCP Bridge (HTTP/SSE -> REST API)
+-- RemoteServer/                # REST API backend
```

## Components

| Project | Transport | Port | Description |
|---------|-----------|------|-------------|
| **McpLocal** | STDIO | N/A | Local .NET MCP with Config & Ticket tools |
| **RemoteServer** | HTTP | 5060 | REST API backend (no MCP) |
| **McpBridge** | HTTP/SSE | 5070 | MCP server that wraps REST API |
| **McpAgentClient** | N/A | N/A | AI Agent consuming all MCP servers |

## MCP Tools

### McpLocal (Local .NET MCP Server)
- `GetConfig` - Get configuration value by key
- `UpdateConfig` - Update configuration value
- `GetTicket` - Get support ticket by ID
- `UpdateTicket` - Update support ticket status

### McpBridge (calls REST API)
- `GetTicket` - Get ticket via REST API
- `UpdateTicket` - Update ticket via REST API

## Running the Demo

### Prerequisites

1. **.NET 10 SDK** installed
2. **Azure OpenAI** resource with a deployed model

### Configuration

Create or update `appsettings.Local.json` in the `dotnet` folder with your Azure OpenAI credentials:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini",
    
    // Option 1: API Key authentication
    "ApiKey": "your-api-key",
    
    // Option 2: Service Principal authentication
    "TenantId": "",
    "ClientId": "",
    "ClientSecret": "",
    
    // Option 3: Managed Identity authentication
    "UseManagedIdentity": false,
    "ManagedIdentityClientId": ""
  }
}
```

Alternatively, set environment variables:
```powershell
$env:AZURE_OPENAI_ENDPOINT = "https://your-resource.openai.azure.com/"
$env:AZURE_OPENAI_DEPLOYMENT_NAME = "gpt-4o-mini"
$env:AZURE_OPENAI_API_KEY = "your-api-key"
```

### Start the Servers

**Option 1: Local MCP Server only (Demo 1)**
```powershell
# Terminal 1: Run the Agent Client
cd McpAgentClient
dotnet run
# Select option 1 for Local MCP Server
```

**Option 2: Remote MCP Server (Demo 3)**
```powershell
# Terminal 1: Start REST API (port 5060)
cd RemoteServer
dotnet run

# Terminal 2: Start MCP Bridge (port 5070)
cd McpBridge
dotnet run

# Terminal 3: Run the Agent Client
cd McpAgentClient
dotnet run
# Select option 3 for Remote MCP Server
```

### Agent Client Menu

When you run the McpAgentClient, you'll see:
```
===================================================================
Select a demo to run:
  1. Local MCP Server (.NET EXE via STDIO)
  2. Local MCP Server (Node.js via STDIO)
  3. Remote MCP Server (HTTP/SSE)
  4. All MCP Servers Combined
  5. Exit
===================================================================
```

### Chat Commands

During a chat session:
- Type your questions to interact with the AI agent
- Type `back` to return to the main menu
- Type `exit` or `quit` to exit the application

## Key Concepts

### Transport Types

| Transport | Use Case | Example |
|-----------|----------|---------|
| **STDIO** | Local servers on same machine | `dotnet run` spawns subprocess |
| **HTTP/SSE** | Remote servers over network | `http://localhost:5070/sse` |

### MCP Tool Definition

```csharp
[McpServerToolType]
public sealed class ConfigurationTools
{
    [McpServerTool]
    [Description("Gets a configuration value by key")]
    public string GetConfig(
        [Description("The configuration key")] string? key = null)
    {
        // Implementation
    }
}
```

### MCP Client Usage

```csharp
// STDIO transport (local)
var client = await McpClientFactory.CreateAsync(
    new StdioClientTransport(new StdioClientTransportOptions
    {
        Command = "dotnet",
        Arguments = ["run", "--project", "McpLocal/McpLocal.csproj"]
    }));

// HTTP/SSE transport (remote)
var client = await McpClientFactory.CreateAsync(
    new SseClientTransport(new SseClientTransportOptions
    {
        Endpoint = new Uri("http://localhost:5070/sse")
    }));
```

### Authentication Options

The McpAgentClient supports multiple authentication methods:

1. **API Key** - Set `AzureOpenAI:ApiKey` or `AZURE_OPENAI_API_KEY`
2. **Service Principal** - Set `TenantId`, `ClientId`, `ClientSecret`
3. **Managed Identity** - Set `UseManagedIdentity` to `true`
4. **Azure CLI** - Default fallback for local development

## Learn More

- See [MCP-Concepts.ipynb](MCP-Concepts.ipynb) for detailed explanations
- [MCP Specification](https://modelcontextprotocol.io/)
- [Microsoft Agent Framework](https://learn.microsoft.com/en-us/agent-framework/)
