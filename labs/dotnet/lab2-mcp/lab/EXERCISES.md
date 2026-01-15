# MCP Workshop Lab Exercises

Welcome to the Model Context Protocol (MCP) Workshop! In this hands-on lab, you'll learn how to:
- Configure Azure OpenAI credentials
- Create an MCP Server with tools
- Define MCP tools with attributes
- Connect to MCP servers from an AI Agent client
- Use both local (STDIO) and remote (HTTP/SSE) transports

## Prerequisites

- .NET 10 SDK installed
- Azure OpenAI resource with deployment
- Visual Studio 2022 or VS Code with C# extension

## Lab Structure

```
lab/
├── EXERCISES.md          # This file
├── mcp-concepts.ipynb    # Educational notebook
├── McpExercises.sln      # Solution file
├── McpAgentClient/       # AI Agent that consumes MCP servers (Exercises 1, 4, 5)
├── McpLocal/             # Local MCP server with STDIO transport (Exercises 2, 3)
├── McpBridge/            # Remote MCP server with HTTP/SSE transport
└── RemoteServer/         # Backend REST API
```

---

## Exercise 1: Configure Azure OpenAI Credentials

**Objective:** Set up the Azure OpenAI configuration so the AI Agent can connect to the LLM.

### Step 1.1: Create the configuration file

Create a file named `appsettings.Local.json` in the `dotnet` folder (parent of lab):

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
    "DeploymentName": "gpt-4o-mini",
    "ApiKey": "YOUR-API-KEY"
  }
}
```

> **Note:** Replace `YOUR-RESOURCE` and `YOUR-API-KEY` with your actual values.

### Step 1.2: Update Program.cs to load configuration

Open `McpAgentClient/Program.cs` and find **STEP 1.2**. Uncomment the following lines:

```csharp
// var endpoint = configuration["AZURE_OPENAI_ENDPOINT"]
//     ?? configuration["AzureOpenAI:Endpoint"]
//     ?? throw new InvalidOperationException("Azure OpenAI endpoint is not set.");
// 
// var deploymentName = configuration["AZURE_OPENAI_DEPLOYMENT_NAME"]
//     ?? configuration["AzureOpenAI:DeploymentName"]
//     ?? "gpt-4o-mini";
```

**Delete** the placeholder lines:
```csharp
var endpoint = "https://YOUR-RESOURCE.openai.azure.com/";
var deploymentName = "gpt-4o-mini";
```

### Step 1.3: Create the Azure OpenAI client

Find **STEP 1.3** and uncomment:
```csharp
// var azureOpenAIClient = CreateAzureOpenAIClient(configuration, endpoint);
```

### Verify

Build and run to verify configuration loads:
```bash
cd McpAgentClient
dotnet build
dotnet run
```

You should see your endpoint displayed (not the placeholder).

---

## Exercise 2: Create the MCP Server Host

**Objective:** Set up the MCP server that will expose tools via STDIO transport.

### Step 2.1: Create the host builder

Open `McpLocal/Program.cs` and find **STEP 2.1**. Uncomment:

```csharp
// HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(settings: null);
```

### Step 2.2: Configure logging

Find **STEP 2.2** and uncomment:

```csharp
// builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
// builder.Logging.SetMinimumLevel(LogLevel.Information);
```

### Step 2.3: Register services

Find **STEP 2.3** and uncomment:

```csharp
// builder.Services.AddSingleton<ConfigurationStore>();
// builder.Services.AddSingleton<TicketStore>();
```

### Step 2.4: Register MCP Server

Find **STEP 2.4** and uncomment:

```csharp
// builder.Services
//     .AddMcpServer()
//     .WithStdioServerTransport()
//     .WithToolsFromAssembly();
```

### Step 2.5: Build and run

Find **STEP 2.5** and uncomment:

```csharp
// await builder.Build().RunAsync();
```

**Delete** the placeholder lines at the bottom:
```csharp
Console.Error.WriteLine("Exercise not completed. Please uncomment the code above.");
await Task.Delay(1000);
```

### Verify

Build the MCP server:
```bash
cd McpLocal
dotnet build
```

---

## Exercise 3: Define MCP Tools

**Objective:** Create tools that can be called by AI agents using MCP attributes.

### Step 3.1: Mark the tool class

Open `McpLocal/Tools/ConfigurationTools.cs` and find **STEP 3.1**. Uncomment:

```csharp
// [McpServerToolType]
```

### Step 3.2: Create the GetConfig tool

Find **STEP 3.2** and uncomment the entire `GetConfig` method:

```csharp
// [McpServerTool]
// [Description("Gets a configuration value by key...")]
// public string GetConfig(
//     [Description("The configuration key to retrieve...")] 
//     string? key = null)
// {
//     ... (entire method body)
// }
```

### Step 3.3: Create the UpdateConfig tool

Find **STEP 3.3** and uncomment the entire `UpdateConfig` method:

```csharp
// [McpServerTool]
// [Description("Updates a configuration value...")]
// public string UpdateConfig(
//     [Description("The configuration key to update...")] 
//     string key,
//     [Description("The new value...")] 
//     string value)
// {
//     ... (entire method body)
// }
```

### Verify

Build to ensure tools are properly defined:
```bash
cd McpLocal
dotnet build
```

---

## Exercise 4: Connect to Local MCP Server

**Objective:** Create an AI Agent that connects to the local MCP server and uses its tools.

### Step 4: Enable the Local MCP Demo

Open `McpAgentClient/Program.cs` and find **EXERCISE 4** in the switch statement. Uncomment:

```csharp
// continueToMenu = await DemoLocalDotNetMcp(configuration, endpoint, deploymentName);
```

Then find the `DemoLocalDotNetMcp` method definition and uncomment the **entire method** (it starts with `// static async Task<bool> DemoLocalDotNetMcp`).

The method includes:
- **Step 4.1:** Path to McpLocal project
- **Step 4.2:** Create STDIO transport
- **Step 4.3:** List available tools
- **Step 4.4:** Create AI Agent with MCP tools
- **Step 4.5:** Run interactive session

### Verify

Run the agent and select option 1:
```bash
cd McpAgentClient
dotnet run
```

Select **1. Local MCP Server** and try these prompts:
- "Get all configurations"
- "What is the value of app.name?"
- "Update feature.darkMode to true"
- "Get the updated configuration for feature.darkMode"

**Test the AI Chat:** When prompted, enter:
```
whats in ticket LOCAL-001
```

The AI should use the MCP tools to retrieve and display the ticket information.

---

## Exercise 5: Connect to Remote MCP Server (HTTP/SSE)

**Objective:** Connect to an MCP server that communicates via HTTP/SSE and calls a REST API backend.

### Prerequisites

First, start the backend servers in **separate terminals**:

**Terminal 1 - Start REST API:**
```bash
cd RemoteServer
dotnet run
```

**Terminal 2 - Start MCP Bridge:**
```bash
cd McpBridge
dotnet run
```

### Step 5: Enable the Remote MCP Demo

Open `McpAgentClient/Program.cs` and find **EXERCISE 5** in the switch statement. Uncomment:

```csharp
// continueToMenu = await DemoRemoteMcp(configuration, endpoint, deploymentName);
```

Then find the `DemoRemoteMcp` method definition and uncomment the **entire method** (it starts with `// static async Task<bool> DemoRemoteMcp`).

The method includes:
- **Step 5.1:** Create SSE transport
- **Step 5.2:** List available tools
- **Step 5.3:** Create AI Agent with MCP tools
- **Step 5.4:** Run interactive session

### Verify

Run the agent and select option 2:
```bash
cd McpAgentClient
dotnet run
```

Select **2. Remote MCP Server** and try these prompts:
- "Get all tickets"
- "Show me open tickets"
- "Get ticket TKT-001"
- "Update ticket TKT-001 status to InProgress"
- "Assign ticket TKT-003 to support-team"

**Test the AI Chat:** When prompted, enter:
```
whats in ticket TKT-001
```

The AI should use the remote MCP tools to retrieve and display the ticket information via the REST API.

---

## Completed Solution

If you get stuck, refer to the complete working solution in `dotnet/lab3-mcp/solution/`.

---

## Summary

Congratulations! You've learned how to:

| Exercise | Concept |
|----------|---------|
| 1 | Configure Azure OpenAI credentials |
| 2 | Create an MCP server host with STDIO transport |
| 3 | Define MCP tools using `[McpServerTool]` attributes |
| 4 | Connect to local MCP servers via STDIO |
| 5 | Connect to remote MCP servers via HTTP/SSE |

### Key Takeaways

- **MCP** standardizes how AI agents connect to tools
- **STDIO transport** is used for local subprocess communication
- **HTTP/SSE transport** is used for remote server communication
- **Tools** are methods decorated with `[McpServerTool]`
- **Tool descriptions** help the LLM understand when and how to use each tool

---

## Next Steps

- Explore creating custom MCP tools
- Add authentication to remote MCP servers
- Integrate MCP with your existing applications
- Review the MCP specification at [modelcontextprotocol.io](https://modelcontextprotocol.io)
