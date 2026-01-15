# Lab 3 - Model Context Protocol (MCP) Workshop (Python)

This lab demonstrates how to build and consume MCP (Model Context Protocol) servers using Python.

## 🎯 Learning Goals

1. **Build Local MCP Servers** - Using STDIO transport
2. **Build Remote MCP Servers** - Using HTTP/SSE transport that calls REST APIs
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
└────────┬─────────┘          REMOTE MCP (HTTP/SSE → REST)
         │                    ┌─────────────────────────────────────────────┐
         │                    │                                             │
         │   HTTP/SSE         │   ┌─────────────┐   HTTP    ┌─────────────┐ │
         └───────────────────►│   │ mcp_bridge  │ ────────► │mcp_remote   │ │
                              │   │ Port: 5070  │           │server :5060 │ │
                              │   │ /sse        │           │(REST API)   │ │
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
├── begin/                          # Lab exercises (incomplete code)
│   ├── EXERCISES.md              # Step-by-step exercises
│   └── ...                       # Code to complete
├── solution/                     # Complete working solution
├── mcp-concepts.ipynb            # Educational notebook
├── mcp_agent_client/             # AI Agent that consumes MCP servers
│   └── main.py
├── mcp_local_server/             # Local MCP Server (STDIO)
│   └── main.py
├── mcp_bridge/                   # MCP Bridge (HTTP/SSE → REST API)
│   └── main.py
└── mcp_remote_server/            # REST API backend (FastAPI)
    └── main.py
```

## Components

| Project | Transport | Port | Description |
|---------|-----------|------|-------------|
| **mcp_local_server** | STDIO | N/A | Local Python MCP with Config & Ticket tools |
| **mcp_remote_server** | HTTP | 5060 | REST API backend (FastAPI) |
| **mcp_bridge** | HTTP/SSE | 5070 | MCP server that wraps REST API |
| **mcp_agent_client** | N/A | N/A | AI Agent consuming all MCP servers |

## MCP Tools

### mcp_local_server (Local Python MCP Server)
- `GetConfig` - Get configuration value by key
- `UpdateConfig` - Update configuration value
- `GetTicket` - Get support ticket by ID
- `UpdateTicket` - Update support ticket status

### mcp_bridge (calls REST API)
- `GetTicket` - Get ticket via REST API
- `UpdateTicket` - Update ticket via REST API

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

# Navigate to the lab
cd lab2-mcp
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

### Option 1: Local MCP Server only (Demo 1)

```bash
# Run the Agent Client (starts local MCP server automatically)
cd mcp_agent_client
python main.py
```

### Option 2: Remote MCP Server (Demo 2)

```bash
# Terminal 1: Start REST API (port 5060)
cd mcp_remote_server
uvicorn main:app --port 5060

# Terminal 2: Start MCP Bridge (port 5070)
cd mcp_bridge
python main.py

# Terminal 3: Run the Agent Client
cd mcp_agent_client
python main.py
```

## 📖 Key Concepts

### Transport Types

| Transport | Use Case | Example |
|-----------|----------|---------|
| **STDIO** | Local servers on same machine | `python -m mcp_local_server.main` |
| **HTTP/SSE** | Remote servers over network | `http://localhost:5070/sse` |

### MCP Tool Definition (Python)

```python
from mcp.server import Server
from mcp.types import Tool, TextContent

server = Server("mcp-local-server")

@server.list_tools()
async def list_tools() -> list[Tool]:
    return [
        Tool(
            name="GetConfig",
            description="Gets a configuration value by key",
            inputSchema={
                "type": "object",
                "properties": {
                    "key": {"type": "string", "description": "The configuration key"}
                },
                "required": ["key"]
            }
        )
    ]

@server.call_tool()
async def call_tool(name: str, arguments: dict) -> list[TextContent]:
    if name == "GetConfig":
        key = arguments.get("key", "")
        return [TextContent(type="text", text=f"Value for {key}")]
```

### MCP Client Usage (Python)

```python
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client
from mcp.client.sse import sse_client

# STDIO transport (local)
server_params = StdioServerParameters(
    command="python",
    args=["-m", "mcp_local_server.main"]
)
async with stdio_client(server_params) as (read, write):
    async with ClientSession(read, write) as session:
        await session.initialize()
        tools = await session.list_tools()

# HTTP/SSE transport (remote)
async with sse_client("http://localhost:5070/sse") as (read, write):
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

- See [mcp-concepts.ipynb](mcp-concepts.ipynb) for detailed explanations
- [MCP Python SDK](https://github.com/modelcontextprotocol/python-sdk)
- [MCP Specification](https://modelcontextprotocol.io/)
- [Azure OpenAI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/)

## 📦 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `mcp` | >=1.0.0 | MCP Python SDK |
| `httpx` | >=0.27.0 | HTTP client for REST calls |
| `httpx-sse` | >=0.4.0 | SSE support |
| `openai` | >=1.50.0 | Azure OpenAI client |
| `azure-identity` | >=1.17.0 | Azure authentication |
| `fastapi` | >=0.115.0 | REST API framework |
| `uvicorn` | >=0.30.0 | ASGI server |
| `pydantic` | >=2.9.0 | Data validation |

