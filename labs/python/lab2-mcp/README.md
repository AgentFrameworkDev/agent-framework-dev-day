# Lab 2 - Model Context Protocol (MCP) Workshop (Python)

This lab demonstrates how to build and consume MCP (Model Context Protocol) servers using Python with the Agent Framework RC4 packages.

## 🎯 Learning Goals

1. **Build Local MCP Servers** - Using STDIO transport
2. **Build Remote MCP Servers** - Using Streamable HTTP transport that calls REST APIs
3. **Consume MCP Servers** - From AI Agents using Azure OpenAI

## 🏗️ Architecture

```
                                    LOCAL MCP (STDIO)
                              ┌─────────────────────────────┐
                              │                             │
┌──────────────────┐          │   ┌───────────────────────┐ │
│                  │ ─────────┼──►│ mcp_local_server      │ │
│  mcp_agent_client│          │   │ (STDIO)               │ │
│                  │          │   └───────────────────────┘ │
│   (Consumes      │          │                             │
│    MCP Servers)  │          └─────────────────────────────┘
│                  │
└────────┬─────────┘          REMOTE MCP (Streamable HTTP → REST)
         │                    ┌─────────────────────────────────────────────┐
         │                    │                                             │
         │  Streamable HTTP   │   ┌─────────────┐   HTTP    ┌─────────────┐ │
         └───────────────────►│   │ mcp_bridge  │ ────────► │mcp_remote   │ │
                              │   │ Port: 5070  │           │server :5060 │ │
                              │   │ /mcp        │           │(REST API)   │ │
                              │   └─────────────┘           └─────────────┘ │
                              │                                             │
                              └─────────────────────────────────────────────┘
```

## 📓 Interactive Notebook

To explore MCP concepts interactively, open and run the Jupyter notebook:
```bash
cd begin
jupyter notebook mcp-concepts.ipynb
```
Or open `begin/mcp-concepts.ipynb` directly in VS Code.

## 📝 Lab Exercises

For hands-on exercises, see **[begin/EXERCISES.md](begin/EXERCISES.md)**.

## 📁 Project Structure

```
lab2-mcp/
├── README.md
├── data/
│   └── tickets.json              # Shared ticket data (loaded by all servers)
├── begin/                          # Lab exercises (incomplete code)
│   ├── EXERCISES.md              # Step-by-step exercises
│   └── ...                       # Code to complete
└── solution/                     # Complete working solution
    ├── start_all.cmd             # Windows: launch all services
    ├── start_all.sh              # macOS/Linux: launch all services
    ├── mcp_agent_client/         # AI Agent that consumes MCP servers
    │   └── main.py
    ├── mcp_local_server/         # Local MCP Server (STDIO)
    │   └── main.py
    ├── mcp_bridge/               # MCP Bridge (Streamable HTTP → REST API)
    │   └── main.py
    └── mcp_remote_server/        # REST API backend (FastAPI)
        └── main.py
```

## Components

| Project | Transport | Port | Description |
|---------|-----------|------|-------------|
| **mcp_local_server** | STDIO | N/A | Local Python MCP with Ticket tools |
| **mcp_remote_server** | HTTP | 5060 | REST API backend (FastAPI) |
| **mcp_bridge** | Streamable HTTP | 5070 | MCP server that wraps REST API |
| **mcp_agent_client** | N/A | N/A | AI Agent consuming all MCP servers |

## MCP Tools

### All MCP Servers expose the same tools:
- `GetAllTickets` - Get all support tickets (with optional `maxResults` limit)
- `GetTicket` - Get support ticket by ID (e.g., TICKET-001)
- `UpdateTicket` - Update support ticket status (Open, In Progress, Resolved, Closed)

## Sample Tickets

| ID | Subject | Status | Description |
|----|---------|--------|-------------|
| TICKET-001 | Login issue | Open | Cannot login to the system |
| TICKET-002 | Performance problem | In Progress | System is running slowly |
| TICKET-003 | Data sync error | Open | Data not syncing properly |

## 🚀 Setup

### Prerequisites

- Python 3.10+
- Azure OpenAI resource with a deployed model (for AI demo)

### Installation

```bash
# Navigate to the python labs folder
cd labs/python

# Create virtual environment
python -m venv .venv
source .venv/bin/activate  # Linux/Mac
.venv\Scripts\activate     # Windows

# Install dependencies (from labs/python folder)
pip install -r requirements.txt

# Navigate to the solution lab
cd lab2-mcp/solution
```

### Configuration

Set environment variables for Azure OpenAI (optional, for AI demo):

```bash
# Required for AI demo
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"

# Optional (default: gpt-4o-mini)
export AZURE_OPENAI_DEPLOYMENT_NAME="gpt-4o-mini"

# Authentication (choose one):
# Option 1: API Key
export AZURE_OPENAI_API_KEY="your-api-key"

# Option 2: Azure CLI (no env vars needed, uses az login)
```

## 🚀 Running the Demo

### Quick Start (Windows)

```cmd
cd solution
start_all.cmd
```

This launches:
- MCP Remote Server (REST API) on port 5060
- MCP Bridge (Streamable HTTP) on port 5070
- MCP Agent Client (interactive menu)

### Quick Start (macOS / Linux)

```bash
cd solution
chmod +x start_all.sh   # Make executable (first time only)
./start_all.sh
```

### Manual Start

**Terminal 1 - REST API Backend:**
```bash
cd solution
python -m mcp_remote_server.main
```

**Terminal 2 - MCP Bridge:**
```bash
cd solution
python -m mcp_bridge.main
```

**Terminal 3 - Agent Client:**
```bash
cd solution
python -m mcp_agent_client.main
```

## 💬 Usage

1. Run `start_all.cmd` or start services manually
2. In the Agent Client, select a demo:
   - **Option 1**: Local MCP Server (STDIO) - standalone, no external services needed
   - **Option 2**: Remote MCP Bridge (Streamable HTTP) - requires REST API and Bridge running
3. Interact with the AI agent using natural language:
   - "Get all tickets"
   - "What is the status of TICKET-001?"
   - "Update TICKET-002 status to Resolved"
4. Type `back` to return to menu, `exit` to quit

## 📖 Key Concepts

### Transport Types

| Transport | Use Case | Example |
|-----------|----------|---------|
| **STDIO** | Local servers on same machine | `python -m mcp_local_server.main` |
| **Streamable HTTP** | Remote servers over network | `http://localhost:5070/mcp` |

### MCP Tool Definition (Python)

```python
from mcp.server import Server
from mcp.types import Tool, TextContent

server = Server("mcp-local-server")

@server.list_tools()
async def list_tools() -> list[Tool]:
    return [
        Tool(
            name="GetAllTickets",
            description="Gets all support tickets with optional limit",
            inputSchema={
                "type": "object",
                "properties": {
                    "maxResults": {"type": "integer", "description": "Max tickets to return", "default": 5}
                },
                "required": []
            }
        )
    ]

@server.call_tool()
async def call_tool(name: str, arguments: dict) -> list[TextContent]:
    if name == "GetAllTickets":
        max_results = arguments.get("maxResults", 5)
        results = list(tickets.values())[:max_results]
        return [TextContent(type="text", text=json.dumps(results, indent=2))]
```

### MCP Client Usage (Python)

```python
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client
from mcp.client.streamable_http import streamablehttp_client

# STDIO transport (local)
server_params = StdioServerParameters(
    command="python",
    args=["-m", "mcp_local_server.main"]
)
async with stdio_client(server_params) as (read, write):
    async with ClientSession(read, write) as session:
        await session.initialize()
        tools = await session.list_tools()

# Streamable HTTP transport (remote)
async with streamablehttp_client("http://localhost:5070/mcp") as (read, write, _):
    async with ClientSession(read, write) as session:
        await session.initialize()
        result = await session.call_tool("GetTicket", {"ticket_id": "TICKET-001"})
```

### Integrating with Azure OpenAI

```python
from openai import AzureOpenAI

# Get tools for OpenAI
mcp_tools = await session.list_tools()
openai_tools = [
    {
        "type": "function",
        "function": {
            "name": tool.name,
            "description": tool.description,
            "parameters": tool.inputSchema
        }
    }
    for tool in mcp_tools.tools
]

# Chat with AI using MCP tools
response = client.chat.completions.create(
    model=deployment,
    messages=messages,
    tools=openai_tools
)

# Handle tool calls
if response.choices[0].message.tool_calls:
    for tool_call in response.choices[0].message.tool_calls:
        args = json.loads(tool_call.function.arguments)
        result = await session.call_tool(tool_call.function.name, args)
```

## 📚 Learn More

- See [begin/mcp-concepts.ipynb](begin/mcp-concepts.ipynb) for detailed explanations
- [MCP Python SDK](https://github.com/modelcontextprotocol/python-sdk)
- [MCP Specification](https://modelcontextprotocol.io/)
- [Azure OpenAI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/)

## 📦 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `agent-framework-core` | 1.0.0rc4 | Agent Framework Core |
| `agent-framework-azure-ai` | 1.0.0rc4 | Agent Framework Azure AI |
| `mcp` | >=1.0.0 | MCP Python SDK |
| `httpx` | >=0.27.0 | HTTP client for REST calls |
| `openai` | >=1.50.0 | Azure OpenAI client |
| `azure-identity` | >=1.17.0 | Azure authentication |
| `azure-ai-projects` | latest | Azure AI Projects client |
| `fastapi` | >=0.115.0 | REST API framework |
| `uvicorn` | >=0.30.0 | ASGI server |
| `pydantic` | >=2.9.0 | Data validation |
