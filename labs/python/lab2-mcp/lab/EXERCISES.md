# MCP Workshop Lab Exercises (Python)

Welcome to the Model Context Protocol (MCP) Workshop! In this hands-on lab, you'll learn how to:
- Configure Azure OpenAI credentials
- Create an MCP Server with tools
- Define MCP tools with decorators
- Connect to MCP servers from an AI Agent client
- Use both local (STDIO) and remote (HTTP/SSE) transports

## Prerequisites

- Python 3.10+ installed
- Azure OpenAI resource with deployment
- VS Code with Python extension

## Lab Structure

```
lab/
├── mcp_agent_client/     # AI Agent that consumes MCP servers (Exercises 1, 3, 4)
├── mcp_local_server/     # Local MCP server with STDIO transport (Exercise 2)
├── mcp_bridge/           # Remote MCP server with HTTP/SSE transport (pre-completed)
├── mcp_remote_server/    # Backend REST API (pre-completed)
├── mcp-concepts.ipynb    # Educational notebook
└── EXERCISES.md          # This file

# Note: requirements.txt is located at labs/python/requirements.txt
```

---

## Setup

Before starting the exercises, set up your Python environment:

```bash
# Navigate to the python labs folder (parent of lab3-mcp)
cd labs/python

# Create virtual environment
python -m venv .venv

# Activate (Windows)
.venv\Scripts\activate

# Activate (Linux/Mac)
source .venv/bin/activate

# Install dependencies (requirements.txt is in labs/python/)
pip install -r requirements.txt

# Navigate to the lab folder
cd lab3-mcp/lab
```

---

## Exercise 1: Configure Azure OpenAI Credentials

**Objective:** Set up the Azure OpenAI configuration so the AI Agent can connect to the LLM.

### Step 1.1: Set environment variables

Set the following environment variables:

**Windows (PowerShell):**
```powershell
$env:AZURE_OPENAI_ENDPOINT = "https://YOUR-RESOURCE.openai.azure.com/"
$env:AZURE_OPENAI_DEPLOYMENT_NAME = "gpt-4o-mini"
```

**Linux/Mac:**
```bash
export AZURE_OPENAI_ENDPOINT="https://YOUR-RESOURCE.openai.azure.com/"
export AZURE_OPENAI_DEPLOYMENT_NAME="gpt-4o-mini"
```

> **Note:** Replace `YOUR-RESOURCE` with your actual Azure OpenAI resource name.

### Step 1.2: Update main.py to load configuration

Open `mcp_agent_client/main.py` and find **STEP 1.1**. Uncomment the following lines:

```python
# endpoint = os.environ.get("AZURE_OPENAI_ENDPOINT")
# deployment = os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o-mini")
# 
# if not endpoint:
#     raise ValueError("AZURE_OPENAI_ENDPOINT environment variable is not set")
```

**Delete** the placeholder lines:
```python
endpoint = "https://YOUR-RESOURCE.openai.azure.com/"
deployment = "gpt-4o-mini"
```

### Step 1.3: Create the Azure OpenAI client function

Find **STEP 1.2** and uncomment the entire `create_openai_client` function.

### Verify

Run to verify configuration loads:
```bash
cd lab
python -m mcp_agent_client.main
```

You should see your endpoint displayed (not the placeholder).

---

## Exercise 2: Create the MCP Server

**Objective:** Set up the MCP server that will expose tools via STDIO transport.

### Step 2.1: Create the server instance

Open `mcp_local_server/main.py` and find **STEP 2.1**. Uncomment:

```python
# server = Server("mcp-local-server")
```

### Step 2.2: Define the list_tools handler

Find **STEP 2.2** and uncomment the entire `list_tools` function:

```python
# @server.list_tools()
# async def list_tools() -> list[Tool]:
#     """List all available tools."""
#     return [
#         Tool(
#             name="GetConfig",
#             ...
#         ),
#         ...
#     ]
```

### Step 2.3: Define the call_tool handler

Find **STEP 2.3** and uncomment the entire `call_tool` function:

```python
# @server.call_tool()
# async def call_tool(name: str, arguments: dict) -> list[TextContent]:
#     """Handle tool calls."""
#     ...
```

### Step 2.4: Define the main function

Find **STEP 2.4** and uncomment the entire `main` function:

```python
# async def main():
#     """Run the MCP server using STDIO transport."""
#     async with stdio_server() as (read_stream, write_stream):
#         await server.run(read_stream, write_stream, server.create_initialization_options())
```

### Step 2.5: Run the server

Find **STEP 2.5** and uncomment:

```python
# if __name__ == "__main__":
#     asyncio.run(main())
```

**Delete** the placeholder lines at the bottom:
```python
if __name__ == "__main__":
    print("Exercise 2 not completed...", file=__import__('sys').stderr)
```

### Verify

The MCP server will be started automatically by the agent client. No manual start needed.

---

## Exercise 3: Connect to Local MCP Server

**Objective:** Create an AI Agent that connects to the local MCP server and uses its tools.

### Step 3: Enable the Local MCP Demo

Open `mcp_agent_client/main.py` and find **EXERCISE 3**. 

First, uncomment the entire `demo_local_mcp` function (it starts with `# async def demo_local_mcp`).

The function includes:
- **Step 3.1:** Create STDIO server parameters
- **Step 3.2:** Connect using stdio_client
- **Step 3.3:** Initialize the session
- **Step 3.4:** List available tools
- **Step 3.5:** Call MCP tools

Then, in the `main()` function under `if choice == "1":`, uncomment:

```python
# await demo_local_mcp()
```

### Verify

Run the agent and select option 1:
```bash
cd lab
python -m mcp_agent_client.main
```

Select **1. Local MCP Server** and you should see:
- Available tools listed
- GetConfig result
- UpdateConfig result
- GetTicket result

---

## Exercise 4: Connect to Remote MCP Server (HTTP/SSE)

**Objective:** Connect to an MCP server that communicates via HTTP/SSE and calls a REST API backend.

### Prerequisites

First, start the backend servers in **separate terminals**:

**Terminal 1 - Start REST API:**
```bash
cd lab
python -m mcp_remote_server.main
```

**Terminal 2 - Start MCP Bridge:**
```bash
cd lab
python -m mcp_bridge.main
```

### Step 4: Enable the Remote MCP Demo

Open `mcp_agent_client/main.py` and find **EXERCISE 4**.

First, uncomment the entire `demo_remote_mcp` function (it starts with `# async def demo_remote_mcp`).

The function includes:
- **Step 4.1:** Define SSE endpoint URL
- **Step 4.2:** Connect using sse_client
- **Step 4.3:** Initialize the session
- **Step 4.4:** List available tools
- **Step 4.5:** Call MCP tools via REST API

Then, in the `main()` function under `if choice == "2":`, uncomment:

```python
# await demo_remote_mcp()
```

### Verify

Run the agent and select option 2:
```bash
cd lab
python -m mcp_agent_client.main
```

Select **2. Remote MCP Server** and you should see:
- Connection to SSE endpoint
- Available tools listed
- GetTicket result (from REST API)
- UpdateTicket result (via REST API)

---

## Completed Solution

If you get stuck, refer to the complete working solution in `python/lab3-mcp/solution/`.

---

## Summary

Congratulations! You've learned how to:

| Exercise | Concept |
|----------|---------|
| 1 | Configure Azure OpenAI credentials |
| 2 | Create an MCP server with STDIO transport |
| 3 | Connect to local MCP servers via STDIO |
| 4 | Connect to remote MCP servers via HTTP/SSE |

### Key Takeaways

- **MCP** standardizes how AI agents connect to tools
- **STDIO transport** is used for local subprocess communication
- **HTTP/SSE transport** is used for remote server communication
- **Tools** are defined using the `@server.list_tools()` and `@server.call_tool()` decorators
- **Tool schemas** help the LLM understand when and how to use each tool

---

## Next Steps

- Explore creating custom MCP tools
- Add authentication to remote MCP servers
- Integrate MCP with your existing applications
- Review the MCP specification at [modelcontextprotocol.io](https://modelcontextprotocol.io)
