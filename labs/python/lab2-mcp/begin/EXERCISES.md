# MCP Workshop Lab Exercises (Python)

Welcome to the Model Context Protocol (MCP) Workshop! In this hands-on lab, you'll learn how to:
- Configure Azure OpenAI credentials via JSON-based .env file
- Create a Local MCP Server with tools using STDIO transport
- Define MCP tools with decorators
- Connect an AI Agent client to the local MCP server
- Build a REST API backend and wrap it with a Remote MCP Bridge
- Connect an AI Agent to a Remote MCP Server via Streamable HTTP
- Use MCP tools in an interactive AI chat session

## Prerequisites

- Python 3.10+ installed
- Azure OpenAI resource with deployment
- VS Code with Python extension
- Virtual environment activated

## Lab Structure

```
lab2-mcp/begin/
├── mcp_agent_client/     # AI Agent that consumes MCP servers (Exercises 1, 3, 6)
├── mcp_local_server/     # Local MCP server with STDIO transport (Exercise 2)
├── mcp_remote_server/    # REST API backend (Exercise 4)
├── mcp_bridge/           # MCP Bridge - Streamable HTTP wrapping REST API (Exercise 5)
├── mcp-concepts.ipynb    # Educational notebook
└── EXERCISES.md          # This file - exercise instructions

# Note: requirements.txt is located at labs/python/requirements.txt
# Note: .env file should be at labs/python/.env
```

---

## Setup

Before starting the exercises, set up your Python environment:

```bash
# Navigate to the python labs folder
cd labs/python

# Create virtual environment (if not already created)
python -m venv .venv

# Activate (Windows)
.venv\Scripts\activate

# Activate (Linux/Mac)
source .venv/bin/activate

# Install dependencies
pip install -r requirements.txt
```

---

## Exercise 1: Configure Azure OpenAI Credentials

**Objective:** Set up the Azure OpenAI configuration via a JSON-based .env file and uncomment the configuration code in the agent client.

### Current State in Begin Folder
The code in `mcp_agent_client/main.py` has:
- ✅ `find_config_path()` and `load_env_file()` functions **already implemented**
- ✅ `validate_env_config()` function structure is in place
- ❌ Environment variable reading code inside `validate_env_config()` is **COMMENTED OUT** (STEP 1.1)
- ❌ Placeholder values are being printed instead
- ❌ `create_azure_ai_client()` function is **COMMENTED OUT** (STEP 1.2)

### Step 1.1: Create the .env file

Create or update the file at `labs/python/.env` with JSON format:

```json
{
    "AZURE_OPENAI_ENDPOINT": "https://your-resource-name.openai.azure.com/",
    "AZURE_OPENAI_DEPLOYMENT_NAME": "gpt-4o-mini",
    "AZURE_OPENAI_API_KEY": "your-api-key-here"
}
```

**Notes:**
- Replace `your-resource-name` with your actual Azure OpenAI resource name
- The API key is recommended; Azure CLI authentication is used as fallback

### Step 1.2: Uncomment environment variable reading

Open `mcp_agent_client/main.py` and find **STEP 1.1** inside the `validate_env_config()` function.

**Uncomment these lines:**
```python
endpoint = os.environ.get("AZURE_AI_PROJECT_ENDPOINT") or os.environ.get("AZURE_OPENAI_ENDPOINT")
deployment = os.environ.get("AZURE_AI_MODEL_DEPLOYMENT_NAME") or os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o-mini")

print(f"\nAzure OpenAI Configuration:")
print(f"  Endpoint: {endpoint or 'Not set'}")
print(f"  Deployment: {deployment}")

if not endpoint:
    print("\n⚠ Warning: Azure OpenAI endpoint not configured. AI demos will be skipped.")
```

**Then DELETE the placeholder lines below them:**
```python
# Placeholder values - REPLACE after uncommenting above
print(f"\nAzure OpenAI Configuration:")
print(f"  Endpoint: https://YOUR-RESOURCE.openai.azure.com/")
print(f"  Deployment: gpt-4o-mini")
```

### Step 1.3: Uncomment the Azure OpenAI client function

Find **STEP 1.2** and **uncomment the entire `create_azure_ai_client()` function**.

This function supports multiple authentication methods:
1. **API Key** (highest priority) - uses `AZURE_OPENAI_API_KEY`
2. **Service Principal** - uses `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`
3. **Azure CLI** fallback - uses `az login` credentials

### Step 1.4: Verify the configuration

Navigate to the lab folder and run:

```bash
cd lab2-mcp/begin
python -m mcp_agent_client.main
```

**Expected output:**
```
============================================================
           MCP Workshop - Agent Client Demo
        Demonstrating Local MCP Server (STDIO)
============================================================

Environment Configuration Validation
============================================================

Config path: C:\...\labs\python

✓ .env file loaded successfully from: C:\...\labs\python
  Loaded 3 environment variables:
    - AZURE_OPENAI_ENDPOINT: https://your-resource.openai.azure.com/
    - AZURE_OPENAI_DEPLOYMENT_NAME: gpt-4o-mini
    - AZURE_OPENAI_API_KEY: ****

Azure OpenAI Configuration:
  Endpoint: https://your-resource.openai.azure.com/
  Deployment: gpt-4o-mini
```

If you see your actual endpoint (not the placeholder), Exercise 1 is complete!

**Common Errors:**
- `No .env file found` → Create the .env file at `labs/python/.env`
- `Warning: Failed to load .env file` → Check your .env file has valid JSON format
- Azure tenant mismatch → Make sure you have `AZURE_OPENAI_API_KEY` in your .env file

---

## Exercise 2: Create the Local MCP Server

**Objective:** Set up the MCP server that will expose ticket management tools via STDIO transport.

### Current State
❌ All code in `mcp_local_server/main.py` is **commented out** - you need to uncomment it.

⚠️ **IMPORTANT:** Exercise 3 (Local MCP demo) **requires** Exercise 2 to be completed first, as it connects to this server.

### Step 2.1: Create the server instance

Open `mcp_local_server/main.py` and find **STEP 2.1**.

**Uncomment** this line:
```python
server = Server("mcp-local-server")
```

### Step 2.2: Define the list_tools handler

Find **STEP 2.2** and **uncomment the entire function**:

```python
@server.list_tools()
async def list_tools() -> list[Tool]:
    """List all available tools."""
    return [
        Tool(name="GetAllTickets", ...),
        Tool(name="GetTicket", ...),
        Tool(name="UpdateTicket", ...),
    ]
```

The function defines 3 tools: GetAllTickets, GetTicket, and UpdateTicket.

### Step 2.3: Define the call_tool handler

Find **STEP 2.3** and **uncomment the entire function**:

```python
@server.call_tool()
async def call_tool(name: str, arguments: dict) -> list[TextContent]:
    """Handle tool calls."""
    ...
```

This function handles the actual execution of the tools.

### Step 2.4: Define the main function

Find **STEP 2.4** and **uncomment the entire function**:

```python
async def main():
    """Run the MCP server using STDIO transport."""
    async with stdio_server() as (read_stream, write_stream):
        await server.run(read_stream, write_stream, server.create_initialization_options())
```

### Step 2.5: Run the server

Find **STEP 2.5** and **uncomment these lines**:

```python
if __name__ == "__main__":
    asyncio.run(main())
```

**Then DELETE** the placeholder code below it:
```python
# Placeholder - REMOVE after uncommenting above
if __name__ == "__main__":
    print("Exercise 2 not completed...", file=__import__('sys').stderr)
```

### Verify

The MCP server will be started automatically by the agent client in Exercise 3. You don't need to run it manually, but you can test it's valid by checking for syntax errors:

```bash
python -m py_compile mcp_local_server/main.py
```

No output means success!

---

## Exercise 3: Connect to Local MCP Server with AI Agent

**Objective:** Enable the AI Agent client to connect to the local MCP server, discover tools, and use them in an interactive chat session.

### Current State
❌ The `demo_local_mcp()` function is **COMMENTED OUT** in `mcp_agent_client/main.py`
❌ The function call in `main()` is also **commented out**

### Prerequisites
⚠️ **You must complete Exercises 1 and 2 first:**
- Exercise 1: Azure OpenAI must be configured
- Exercise 2: Local MCP server must be functional

### Step 3.1: Uncomment the demo_local_mcp function

Open `mcp_agent_client/main.py` and find **EXERCISE 3**.

**Uncomment the entire `demo_local_mcp()` function**, which includes:
- **STEP 3.1:** Create STDIO server parameters pointing to `mcp_local_server.main`
- **STEP 3.2:** Connect using `stdio_client`
- **STEP 3.3:** Initialize the MCP session
- **STEP 3.4:** List available tools from the server
- **STEP 3.5:** Create AI client and run interactive session

### Step 3.2: Enable the Local MCP Demo in main()

In the `main()` function, find the menu handler for choice `"1"`:

```python
if choice == "1":
    # ===========================================================
    # EXERCISE 3: Uncomment to enable Local MCP Demo
    # ===========================================================
    # exit_app = await demo_local_mcp()
    print("Exercise 3 not completed. Please uncomment the demo_local_mcp function and call.")
```

**Uncomment** the function call and **delete** the print statement:
```python
if choice == "1":
    exit_app = await demo_local_mcp()
```

### Step 3.3: Verify

Run the agent client:
```bash
cd lab2-mcp/begin
python -m mcp_agent_client.main
```

When the menu appears, select **1. Local MCP Server Demo**.

**Expected output:**
```
============================================================
       Demo: Local MCP Server (STDIO Transport)
============================================================

Connecting to Local Python MCP Server...
Connected to Local Python MCP Server

Available tools (3):
   - GetAllTickets: Gets all support tickets with optional limit
   - GetTicket: Gets a support ticket by ID
   - UpdateTicket: Updates a support ticket status

Authentication: API Key (Azure OpenAI)
Starting interactive session with Local Python MCP

You: Get all tickets
Agent: Here are the current support tickets: ...

You: What is the status of TICKET-001?
Agent: TICKET-001 "Login issue" is currently Open.

You: Update TICKET-002 status to Resolved
Agent: TICKET-002 has been updated to Resolved.
```

**Common Errors:**
- `ModuleNotFoundError: No module named 'mcp_local_server'` → You're not in the correct directory (should be in `lab2-mcp/begin`)
- Connection hangs or times out → Exercise 2 server code has syntax errors or wasn't uncommented properly
- `Could not create Azure AI client` → Exercise 1 not completed (check .env file)

---

## Completed Solution

If you get stuck, refer to the complete working solution in:
```
labs/python/lab2-mcp/solution/
```

---

## Exercise 4: Create the REST API Backend

**Objective:** Build a FastAPI REST API that manages ticket data. This API will serve as the backend that the MCP Bridge wraps in Exercise 5.

### Current State
❌ All endpoint code in `mcp_remote_server/main.py` is **commented out** - you need to uncomment it.

⚠️ **IMPORTANT:** Exercise 5 (MCP Bridge) and Exercise 6 (Remote MCP demo) **require** Exercise 4 to be completed first.

### Step 4.1: Define the Pydantic models

Open `mcp_remote_server/main.py` and find **STEP 4.1**.

**Uncomment** the two model classes:
```python
class Ticket(BaseModel):
    id: str
    customerId: str | None = None
    customerName: str | None = None
    subject: str | None = None
    description: str
    status: str
    priority: str | None = None
    assignedTo: str | None = None


class TicketUpdate(BaseModel):
    status: str
```

### Step 4.2: Define the API endpoints

Find **STEP 4.2** and **uncomment all the endpoint functions**:
- `GET /` - Root endpoint returning API info
- `GET /api/tickets` - Get all tickets (with `maxResults` query parameter)
- `GET /api/tickets/{ticket_id}` - Get a specific ticket
- `PUT /api/tickets/{ticket_id}` - Update a ticket's status

### Step 4.3: Run the server

Find **STEP 4.3** and **uncomment** the run lines:
```python
if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=5060)
```

**Then DELETE** the placeholder code below.

### Verify

Start the REST API server:
```bash
cd lab2-mcp/begin
python -m mcp_remote_server.main
```

**Expected output:**
```
Starting REST API Server on http://localhost:5060
```

Test it in a **new terminal**:
```bash
curl http://localhost:5060/api/tickets
```

You should see a JSON array of tickets. Keep the server running for Exercise 5.

---

## Exercise 5: Create the MCP Bridge (Streamable HTTP)

**Objective:** Build an MCP server that uses Streamable HTTP transport to expose tools that forward calls to the REST API backend.

### Current State
❌ All code in `mcp_bridge/main.py` is **commented out** - you need to uncomment it.

### Prerequisites
⚠️ **Exercise 4 must be completed first** - the REST API must be running.

### Architecture
```
Agent Client -> MCP Bridge (:5070/mcp) -> REST API (:5060/api/...)
                Streamable HTTP            HTTP/JSON
```

### Step 5.1: Create the MCP server instance

Open `mcp_bridge/main.py` and find **STEP 5.1**.

**Uncomment** this line:
```python
mcp = Server("mcp-bridge")
```

### Step 5.2: Define the list_tools handler

Find **STEP 5.2** and **uncomment the entire function**. It defines 3 tools:
- `GetAllTickets` - Forwards to `GET /api/tickets`
- `GetTicket` - Forwards to `GET /api/tickets/{id}`
- `UpdateTicket` - Forwards to `PUT /api/tickets/{id}`

### Step 5.3: Define the call_tool handler

Find **STEP 5.3** and **uncomment the entire function**.

This handler uses `httpx.AsyncClient` to forward each tool call to the corresponding REST API endpoint.

### Step 5.4: Set up the Streamable HTTP transport

Find **STEP 5.4** and **uncomment all the code**. This includes:
- `ensure_server_running()` - Creates the `StreamableHTTPServerTransport` and starts the MCP server
- `handle_mcp()` - Routes `/mcp` requests to the transport
- `app()` - Main ASGI application with routing for `/mcp`, `/`, and `/health`

### Step 5.5: Run the server

Find **STEP 5.5** and **uncomment** the run block. **Delete** the placeholder code.

### Verify

**Make sure the REST API from Exercise 4 is still running on port 5060!**

In a **new terminal**, start the MCP Bridge:
```bash
cd lab2-mcp/begin
python -m mcp_bridge.main
```

**Expected output:**
```
============================================================
       MCP Bridge Server (Streamable HTTP Transport)
============================================================
  Server URL:   http://localhost:5070
  MCP Endpoint: http://localhost:5070/mcp
  Health Check: http://localhost:5070/health

  Available MCP Tools:
     - GetAllTickets
     - GetTicket
     - UpdateTicket
```

Test the health endpoint:
```bash
curl http://localhost:5070/health
```

Keep both servers running for Exercise 6.

---

## Exercise 6: Connect to Remote MCP Server

**Objective:** Enable the AI Agent client to connect to the remote MCP Bridge via Streamable HTTP and use its tools interactively.

### Current State
❌ The `demo_remote_mcp()` function is **COMMENTED OUT** in `mcp_agent_client/main.py`
❌ The function call in `main()` is also **commented out**

### Prerequisites
⚠️ **You must complete Exercises 1, 4, and 5 first:**
- Exercise 1: Azure OpenAI must be configured
- Exercise 4: REST API must be running on port 5060
- Exercise 5: MCP Bridge must be running on port 5070

### Step 6.1: Uncomment the demo_remote_mcp function

Open `mcp_agent_client/main.py` and find **EXERCISE 6**.

**Uncomment the entire `demo_remote_mcp()` function**, which includes:
- **STEP 6.1:** Connect using `streamablehttp_client` to `http://localhost:5070/mcp`
- **STEP 6.2:** Initialize the MCP session
- **STEP 6.3:** List available tools from the bridge
- **STEP 6.4:** Create AI client and run interactive session

### Step 6.2: Enable the Remote MCP Demo in main()

In the `main()` function, find the menu handler for choice `"2"`:

```python
elif choice == "2":
    # ===========================================================
    # EXERCISE 6: Uncomment to enable Remote MCP Demo
    # ===========================================================
    # exit_app = await demo_remote_mcp()
    print("Exercise 6 not completed...")
```

**Uncomment** the function call and **delete** the print statement.

### Step 6.3: Verify

**Make sure both backend servers are still running!**
- Terminal 1: REST API on port 5060
- Terminal 2: MCP Bridge on port 5070

In a **third terminal**, run the agent client:
```bash
cd lab2-mcp/begin
python -m mcp_agent_client.main
```

When the menu appears, select **2. Remote MCP Server Demo**.

**Expected output:**
```
============================================================
      Demo: Remote MCP Bridge (Streamable HTTP -> REST API)
============================================================

Architecture:
   AgentClient -> MCP Bridge (:5070) -> REST API (:5060)

Connecting to MCP Bridge at http://localhost:5070/mcp...
Connected to MCP Bridge

Available tools (3):
   - GetAllTickets: Gets all support tickets from the REST API
   - GetTicket: Gets a support ticket by ID from the REST API
   - UpdateTicket: Updates a support ticket status via the REST API

You: Get all tickets
Agent: Here are the tickets from the REST API: ...
```

**Common Errors:**
- `Error: Connection refused` → Backend servers (port 5060 and 5070) are not running
- `Make sure the MCP Bridge and REST API are running` → Start both servers first

---

## Summary

Congratulations! You've learned how to:

| Exercise | Concept | Status in Begin Folder |
|----------|---------|----------------------|
| 1 | Configure Azure OpenAI with JSON .env file | ⚠️ env loading implemented, config code needs uncommenting |
| 2 | Create a Local MCP server with STDIO transport | ❌ All code needs uncommenting |
| 3 | Connect AI Agent to local MCP server | ❌ All code needs uncommenting |
| 4 | Create REST API backend (FastAPI) | ❌ All code needs uncommenting |
| 5 | Create MCP Bridge with Streamable HTTP transport | ❌ All code needs uncommenting |
| 6 | Connect AI Agent to remote MCP server | ❌ All code needs uncommenting |

### Key Takeaways

- **MCP** standardizes how AI agents connect to tools
- **STDIO transport** is used for local subprocess communication
- **Streamable HTTP transport** is used for remote server communication
- **Tool handlers** are defined using `@server.list_tools()` and `@server.call_tool()` decorators
- **Tool schemas** help the LLM understand when and how to use each tool
- The AI agent dynamically **discovers tools** from the MCP server at runtime
- **Interactive chat sessions** let the AI choose which tools to call based on user queries
- **MCP Bridge pattern** wraps existing REST APIs without modifying the backend

### Architecture Flow

```
Exercise 3 (Local):
AI Agent Client → STDIO → Local MCP Server → In-Memory Ticket Data

Exercise 6 (Remote):
AI Agent Client → Streamable HTTP → MCP Bridge (:5070) → REST API (:5060) → Ticket Data
```

---

## Troubleshooting

### .env File Not Found
**Error:** `No .env file found or failed to load`

**Solution:** Create the `.env` file at `labs/python/.env` in JSON format:
```json
{
    "AZURE_OPENAI_ENDPOINT": "https://your-resource.openai.azure.com/",
    "AZURE_OPENAI_DEPLOYMENT_NAME": "gpt-4o-mini",
    "AZURE_OPENAI_API_KEY": "your-api-key"
}
```

### Azure Tenant Mismatch
**Error:** `Token tenant does not match resource tenant`

**Solution:** Add `AZURE_OPENAI_API_KEY` to your .env file - the code will use API key authentication instead of Azure CLI

### Module Not Found
**Error:** `ModuleNotFoundError: No module named 'mcp_local_server'`

**Solution:**
1. Make sure you're in the correct directory: `labs/python/lab2-mcp/begin`
2. Run commands with `python -m module_name.main` format

### MCP Server Connection Issues
**Error:** Connection hangs, times out, or shows subprocess errors

**Solution:**
1. Verify Exercise 2 is fully completed (all steps uncommented)
2. Check for syntax errors: `python -m py_compile mcp_local_server/main.py`
3. Ensure `assets/tickets.json` exists in the workspace

### Connection Refused (Exercises 5/6)
**Error:** `Error: Connection refused` or timeout when connecting to remote MCP

**Solution:**
1. Verify the REST API is running: `curl http://localhost:5060/`
2. Verify the MCP Bridge is running: `curl http://localhost:5070/health`
3. Check firewalls are not blocking ports 5060 and 5070
4. Ensure both servers completed without syntax errors

---

## Next Steps

After completing this lab:
- Add more tools to the local server (e.g., CreateTicket, DeleteTicket)
- Add authentication to the MCP Bridge
- Connect your agent to multiple MCP servers simultaneously
- Review the MCP specification at [modelcontextprotocol.io](https://modelcontextprotocol.io)
- Try Lab 2 - Workflow for agent orchestration patterns
