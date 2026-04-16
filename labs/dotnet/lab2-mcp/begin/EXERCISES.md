# MCP Lab Exercises (.NET)

## Overview

This lab demonstrates the **Model Context Protocol (MCP)** with .NET, showing how AI agents can consume both local and remote MCP servers for customer support ticket management.

## Architecture

```
+-----------------+   STDIO    +----------------+
|  McpAgentClient | ---------> |   McpLocal     |
|  (AI Agent)     |            | (Local MCP)    |
+-----------------+            +----------------+
        |
        |  HTTP/SSE
        v
+----------------+   HTTP/REST   +----------------+
|   McpBridge    | ------------> |  RemoteServer  |
| (MCP Gateway)  |              |  (REST API)    |
+----------------+              +----------------+
```

## Exercises

### STEP 1: Create MCP Client for Local Server (STDIO Transport)
**File:** `McpAgentClient/Program.cs` - `DemoLocalDotNetMcp` method

Create a `StdioClientTransport` and use `McpClient.CreateAsync()` to connect to the local .NET MCP server.

### STEP 2: Build Local MCP Server Host
**File:** `McpLocal/Program.cs`

Build a .NET host with MCP server using STDIO transport. Register `TicketStore` as a singleton and use `WithStdioServerTransport()` and `WithToolsFromAssembly()`.

### STEP 4: Create IChatClient with Function Invocation
**File:** `McpAgentClient/Program.cs` - `DemoLocalDotNetMcp` method

Create an `IChatClient` using Azure OpenAI with `.UseFunctionInvocation()` to enable the AI agent to call MCP tools automatically.

### STEP 5: Create MCP Client for Remote Server (HTTP Transport)
**File:** `McpAgentClient/Program.cs` - `DemoRemoteMcp` method

Create an `HttpClientTransport` and use `McpClient.CreateAsync()` to connect to the MCP Bridge server.

## Running the Lab

1. **Start Remote Server** (REST API on port 5060):
   ```bash
   cd RemoteServer && dotnet run
   ```

2. **Start MCP Bridge** (HTTP/SSE on port 5070):
   ```bash
   cd McpBridge && dotnet run
   ```

3. **Start Agent Client** (interactive menu):
   ```bash
   cd McpAgentClient && dotnet run
   ```

Or use the convenience script:
```bash
# Windows
start_all.cmd

# macOS/Linux
./start_all.sh
```

## Key Concepts

- **STDIO Transport**: Local MCP server runs as a subprocess, communicating via stdin/stdout
- **HTTP Transport**: Remote MCP server accessible via HTTP (Streamable HTTP)
- **MCP Bridge**: Wraps a REST API as an MCP server
- **IChatClient**: Microsoft.Extensions.AI abstraction for chat clients
- **Function Invocation**: Automatic tool calling by the AI agent
