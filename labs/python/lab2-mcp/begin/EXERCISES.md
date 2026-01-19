# MCP Workshop Lab Exercises (Python) - UPDATED

Welcome to the Model Context Protocol (MCP) Workshop! In this hands-on lab, you'll learn how to:
- Configure Azure OpenAI credentials via JSON-based .env file
- Create an MCP Server with tools
- Define MCP tools with decorators
- Connect to MCP servers from an AI Agent client
- Use both local (STDIO) and remote (HTTP/SSE) transports

## Prerequisites

- Python 3.10+ installed
- Azure OpenAI resource with deployment
- VS Code with Python extension
- Virtual environment activated

## Lab Structure

```
lab2-mcp/begin/
├── mcp_agent_client/     # AI Agent that consumes MCP servers (Exercises 1, 3, 4)
├── mcp_local_server/     # Local MCP server with STDIO transport (Exercise 2)
├── mcp_bridge/           # Remote MCP server with HTTP/SSE transport (pre-completed)
├── mcp_remote_server/    # Backend REST API (pre-completed)
├── mcp-concepts.ipynb    # Educational notebook
├── EXERCISES.md          # Original instructions (outdated)
└── EXERCISES(NEW).md     # This file - Updated instructions

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
python -m venv venv

# Activate (Windows)
venv\Scripts\activate

# Activate (Linux/Mac)
source venv/bin/activate

# Install dependencies
pip install -r requirements.txt
```

---

## Exercise 1: Configure Azure OpenAI Credentials

**Objective:** Set up the Azure OpenAI configuration via a JSON-based .env file and uncomment the configuration code.

### Current State in Begin Folder
The code in `mcp_agent_client/main.py` has:
- ✅ `find_config_path()` function **already implemented** (line 27)
- ✅ Environment variables are automatically loaded using **python-dotenv** on startup (lines 42-49)
- ❌ Environment variable reading code is **COMMENTED OUT** (lines 62-68)
- ❌ Placeholder values are being used instead (lines 70-71)
- ❌ `create_openai_client()` function is **COMMENTED OUT** (lines 83-105)

### Step 1.1: Create the .env file

**IMPORTANT:** The code uses standard **python-dotenv** library for environment variables.

Create or update the file at `labs/python/.env` with the following standard format:

```bash
# Azure OpenAI Configuration
AZURE_OPENAI_ENDPOINT=https://your-resource-name.openai.azure.com/
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o-mini

# Authentication - API Key (recommended)
AZURE_OPENAI_API_KEY=your-api-key-here
```

**Notes:**
- ✅ Comments are allowed (lines starting with `#`)
- ✅ Standard KEY=VALUE format (no quotes needed unless value has spaces)
- ✅ Much simpler than JSON format
- Replace `your-resource-name` with your actual Azure OpenAI resource name
- The API key is recommended; Azure CLI authentication is used as fallback

### Step 1.2: Uncomment environment variable reading

Open `mcp_agent_client/main.py` and find **STEP 1.1** (around line 62).

**Uncomment these lines:**
```python
endpoint = os.environ.get("AZURE_OPENAI_ENDPOINT")
deployment = os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME") or \
             os.environ.get("AZURE_AI_MODEL_DEPLOYMENT_NAME") or \
             "gpt-4o-mini"

if not endpoint:
    raise ValueError("AZURE_OPENAI_ENDPOINT environment variable is not set")
```

**Then DELETE the placeholder lines below them (lines 70-71):**
```python
# Placeholder values - REPLACE after uncommenting above
endpoint = "https://YOUR-RESOURCE.openai.azure.com/"
deployment = "gpt-4o-mini"
```

### Step 1.3: Uncomment the Azure OpenAI client function

Find **STEP 1.2** (around line 83) and **uncomment the entire function:**
```python
def create_openai_client() -> AzureOpenAI:
    """Create Azure OpenAI client with API key or Azure CLI credentials."""
    # Try using API key from environment first
    api_key = os.environ.get("AZURE_OPENAI_API_KEY")
    
    if api_key:
        # Use API key authentication
        return AzureOpenAI(
            azure_endpoint=endpoint,
            api_key=api_key,
            api_version="2024-02-15-preview"
        )
    else:
        # Fallback to Azure CLI credentials
        credential = AzureCliCredential()
        token = credential.get_token("https://cognitiveservices.azure.com/.default")
        
        return AzureOpenAI(
            azure_endpoint=endpoint,
            api_key=token.token,
            api_version="2024-02-15-preview"
        )
```

**Note:** This function now supports both API key and Azure CLI authentication, avoiding tenant mismatch issues.

### Step 1.4: Verify the configuration

Navigate to the lab folder and run:

```bash
cd lab2-mcp/begin
python -m mcp_agent_client.main
```

**Expected output:**
```
Loaded environment variables from: C:\...\labs\python/.env
============================================================
       MCP Workshop - Agent Client Demo (Python)
   Demonstrating Local and Remote MCP Servers
============================================================

Using Azure OpenAI endpoint: https://your-resource-name.openai.azure.com/
Deployment: gpt-4o-mini
```

If you see "Loaded environment variables" and your actual endpoint (not the placeholder), Exercise 1 is complete!

**Common Errors:**
- `Warning: .env file not found` → Create the .env file at `labs/python/.env`
- `ValueError: AZURE_OPENAI_ENDPOINT environment variable is not set` → Check your .env file has the correct KEY=VALUE format
- Azure tenant mismatch error → Make sure you have `AZURE_OPENAI_API_KEY` in your .env file

---

## Exercise 2: Create the MCP Server

**Objective:** Set up the MCP server that will expose tools via STDIO transport.

### Current State
❌ All code in `mcp_local_server/main.py` is **commented out** - you need to uncomment it.

⚠️ **IMPORTANT:** Exercise 3 (Local MCP demo) **requires** Exercise 2 to be completed first, as it connects to this server.

### Step 2.1: Create the server instance

Open `mcp_local_server/main.py` and find **STEP 2.1** (around line 37). 

**Uncomment** this line:
```python
server = Server("mcp-local-server")
```

### Step 2.2: Define the list_tools handler

Find **STEP 2.2** (around line 46) and **uncomment the entire function**:

```python
@server.list_tools()
async def list_tools() -> list[Tool]:
    """List all available tools."""
    return [
        Tool(
            name="GetConfig",
            ...
        ),
        ...
    ]
```

The function should span approximately 50 lines defining 4 tools: GetConfig, UpdateConfig, GetTicket, UpdateTicket.

### Step 2.3: Define the call_tool handler

Find **STEP 2.3** (around line 123) and **uncomment the entire function**:

```python
@server.call_tool()
async def call_tool(name: str, arguments: dict) -> list[TextContent]:
    """Handle tool calls."""
    ...
```

This function handles the actual execution of the tools (about 30 lines).

### Step 2.4: Define the main function

Find **STEP 2.4** (around line 164) and **uncomment the entire function**:

```python
async def main():
    """Run the MCP server using STDIO transport."""
    async with stdio_server() as (read_stream, write_stream):
        await server.run(read_stream, write_stream, server.create_initialization_options())
```

### Step 2.5: Run the server

Find **STEP 2.5** (around line 171) and **uncomment these lines**:

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

## Exercise 3: Connect to Local MCP Server

**Objective:** Enable the AI Agent client to connect to the local MCP server and use its tools.

### Current State
❌ The `demo_local_mcp()` function is **COMMENTED OUT** (around line 113).
❌ The function call in `main()` is also **commented out** (around line 290).

### Prerequisites
⚠️ **You must complete Exercise 2 first** - the local MCP server must be functional.

### Step 3.1: Uncomment the demo_local_mcp function

Open `mcp_agent_client/main.py` and find **EXERCISE 3** (around line 113).

**Uncomment the entire `demo_local_mcp()` function** - it should be about 50 lines of code from line 113 to line 162, including:
- STEP 3.1: Create STDIO server parameters
- STEP 3.2: Connect using stdio_client
- STEP 3.3: Initialize the session
- STEP 3.4: List available tools
- STEP 3.5: Call GetConfig, UpdateConfig, and GetTicket tools

### Step 3.2: Enable the Local MCP Demo in main()

Find the `main()` function (around line 290) and look for this section:
```python
if choice == "1":
    # ================================================================
    # EXERCISE 3: Uncomment to enable Local MCP Demo
    # ================================================================
    # await demo_local_mcp()
    print("Exercise 3 not completed. Please uncomment the demo_local_mcp function and call.")
```

**Uncomment** the function call and **delete** the print statement:
```python
if choice == "1":
    # ================================================================
    # EXERCISE 3: Uncomment to enable Local MCP Demo
    # ================================================================
    await demo_local_mcp()
```

### Verify

Run the agent client:
```bash
cd lab2-mcp/begin
python -m mcp_agent_client.main
```

When the menu appears, select **1. Local MCP Server**.

**Expected output:**
```
============================================================
Demo: Local MCP Server (STDIO)
============================================================

Available tools:
  - GetConfig: Gets a configuration value by key
  - UpdateConfig: Updates a configuration value
  - GetTicket: Gets a support ticket by ID
  - UpdateTicket: Updates a support ticket status

Calling GetConfig('theme')...
Result: Configuration 'theme' = 'dark'

Calling UpdateConfig('theme', 'light')...
Result: Configuration 'theme' updated to 'light'

Calling GetTicket('TICKET-001')...
Result: {
  "id": "TICKET-001",
  "title": "Login issue",
  "status": "Open",
  "description": "Cannot login to the system"
}
```

**Common Errors:**
- `ModuleNotFoundError: No module named 'mcp_local_server'` → You're not in the correct directory (should be in `lab2-mcp/begin`)
- Connection hangs or times out → Exercise 2 server code has syntax errors or wasn't uncommented properly

---

## Exercise 4: Connect to Remote MCP Server (HTTP/SSE)

**Objective:** Connect to an MCP server that communicates via HTTP/SSE and calls a REST API backend.

### Current State
❌ The `demo_remote_mcp()` function is **commented out** (around line 167).
❌ The function call in `main()` is also **commented out** (around line 298).

### Prerequisites

⚠️ Before running Exercise 4, you must start two backend servers in **separate terminals**:

**Terminal 1 - Start REST API Server:**
```bash
cd labs/python/lab2-mcp/begin
python -m mcp_remote_server.main
```

Keep this running. You should see:
```
Starting REST API server on http://localhost:5060
```

**Terminal 2 - Start MCP Bridge Server:**
```bash
cd labs/python/lab2-mcp/begin
python -m mcp_bridge.main
```

Keep this running. You should see:
```
Starting MCP Bridge with SSE on http://localhost:5070
```

### Step 4.1: Uncomment the demo_remote_mcp function

Open `mcp_agent_client/main.py` and find **EXERCISE 4** (around line 167).

**Uncomment the entire `demo_remote_mcp()` function** - it should be about 40 lines of code from line 167 to line 202, including:
- STEP 4.1: Define SSE endpoint URL
- STEP 4.2: Connect using sse_client
- STEP 4.3: Initialize the session
- STEP 4.4: List available tools
- STEP 4.5: Call GetTicket and UpdateTicket tools via REST API
- Error handling for connection issues

### Step 4.2: Enable the Remote MCP Demo

In the `main()` function (around line 298), find this section:
```python
elif choice == "2":
    # ================================================================
    # EXERCISE 4: Uncomment to enable Remote MCP Demo
    # ================================================================
    # await demo_remote_mcp()
    print("Exercise 4 not completed. Please uncomment the demo_remote_mcp function and call.")
```

**Uncomment** the function call and **delete** the print statement:
```python
elif choice == "2":
    # ================================================================
    # EXERCISE 4: Uncomment to enable Remote MCP Demo
    # ================================================================
    await demo_remote_mcp()
```

### Verify

**Make sure both backend servers from Prerequisites are still running!**

In a **third terminal**, run the agent client:
```bash
cd labs/python/lab2-mcp/begin
python -m mcp_agent_client.main
```

When the menu appears, select **2. Remote MCP Server**.

**Expected output:**
```
============================================================
Demo: Remote MCP Server (HTTP/SSE → REST API)
============================================================

Connecting to http://localhost:5070/sse...

Available tools:
  - GetTicket: Gets a support ticket by ID (via REST API)
  - UpdateTicket: Updates a support ticket status (via REST API)

Calling GetTicket('TICKET-001') via REST API...
Result: {
  "id": "TICKET-001",
  "title": "Login issue",
  "status": "Open"
}

Calling UpdateTicket('TICKET-001', 'Resolved') via REST API...
Result: Ticket 'TICKET-001' status updated to 'Resolved'
```

**Common Errors:**
- `Error: Connection refused` → Backend servers (Terminal 1 & 2) are not running
- `Error: Make sure the MCP Bridge (port 5070) and REST API (port 5060) are running` → Check both servers are running and ports are not blocked

---

## Bonus Exercise: AI Agent with MCP Tools

**Objective:** Use MCP tools with Azure OpenAI to create an intelligent agent.

### Current State
❌ The `demo_with_ai_agent()` function is **commented out** (around line 207).
❌ The function call in `main()` is also **commented out** (around line 305).

### Prerequisites
✅ Exercise 1 completed (Azure OpenAI configured)
✅ Exercise 2 completed (Local MCP server working)

### Step: Enable the AI Agent Demo

Open `mcp_agent_client/main.py`:

1. Find and **uncomment the entire `demo_with_ai_agent()` function** (around line 207, spans about 60 lines)
   - This includes creating the OpenAI client, connecting to local MCP, converting tools to OpenAI format, and running a demo query

2. In the `main()` function (around line 305), find this section and **uncomment** the function call:

```python
elif choice == "3":
    # ================================================================
    # BONUS: Uncomment to enable AI Agent Demo
    # ================================================================
    await demo_with_ai_agent()
```

### Verify

Run the agent and select **3. AI Agent with MCP Tools**:

```bash
cd lab2-mcp/begin
python -m mcp_agent_client.main
```

**Expected output:**
```
============================================================
Demo: AI Agent with MCP Tools
============================================================

User: What is the current theme configuration?

AI calling tool: GetConfig
Tool result: Configuration 'theme' = 'light'
```

The AI will automatically choose which MCP tools to use based on your query!

---

## Completed Solution

If you get stuck, refer to the complete working solution in:
```
labs/python/lab2-mcp/solution/
```

---

## Summary

Congratulations! You've learned how to:

| Exercise | Concept | Status in Begin Folder |
|----------|---------|----------------------|
| 1 | Configure Azure OpenAI with standard .env file | ⚠️ dotenv loading implemented, config code needs uncommenting |
| 2 | Create an MCP server with STDIO transport | ❌ All code needs uncommenting |
| 3 | Connect to local MCP servers via STDIO | ❌ All code needs uncommenting |
| 4 | Connect to remote MCP servers via HTTP/SSE | ❌ All code needs uncommenting |
| Bonus | Use MCP tools with Azure OpenAI | ❌ All code needs uncommenting |

### Key Takeaways

- **MCP** standardizes how AI agents connect to tools
- **STDIO transport** is used for local subprocess communication (Exercise 3)
- **HTTP/SSE transport** is used for remote server communication (Exercise 4)
- **Environment configuration** uses standard python-dotenv library with KEY=VALUE format
- **Tool handlers** are defined using `@server.list_tools()` and `@server.call_tool()` decorators
- **Tool schemas** help the LLM understand when and how to use each tool
- **Hybrid authentication** supports both API key (recommended) and Azure CLI credentials

### Architecture Flow

```
Exercise 3 (Local):
AI Agent Client → STDIO → Local MCP Server → In-Memory Data

Exercise 4 (Remote):
AI Agent Client → HTTP/SSE → MCP Bridge → REST API → In-Memory Data
```

---

## Troubleshooting

### .env File Not Found
**Error:** `Warning: .env file not found at ...`

**Solution:** Create the `.env` file at `labs/python/.env` in standard dotenv format:
```bash
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o-mini
AZURE_OPENAI_API_KEY=your-api-key
```

### Environment Variable Not Set
**Error:** `ValueError: AZURE_OPENAI_ENDPOINT environment variable is not set`

**Solution:** 
1. Check `.env` file exists at `labs/python/.env`
2. Verify the format is `KEY=VALUE` (no quotes needed unless value has spaces)
3. Make sure variable names are correct (case-sensitive)

### Azure Tenant Mismatch
**Error:** `Token tenant does not match resource tenant`

**Solution:** Add `AZURE_OPENAI_API_KEY` to your .env file - the updated code will use API key authentication instead of Azure CLI

### Module Not Found
**Error:** `ModuleNotFoundError: No module named 'mcp_local_server'`

**Solution:** 
1. Make sure you're in the correct directory: `labs/python/lab2-mcp/begin`
2. Run commands with `python -m module_name.main` format

### Connection Refused (Exercise 4)
**Error:** `Error: Connection refused` or timeout

**Solution:**
1. Verify Terminal 1 shows: `Starting REST API server on http://localhost:5060`
2. Verify Terminal 2 shows: `Starting MCP Bridge with SSE on http://localhost:5070`
3. Check firewalls are not blocking ports 5060 and 5070

---

## Next Steps

- Explore creating custom MCP tools for your use cases
- Add authentication to remote MCP servers
- Implement resource providers (prompts, context)
- Integrate MCP with your existing applications
- Review the MCP specification at [modelcontextprotocol.io](https://modelcontextprotocol.io)

## Questions?

If you encounter issues not covered in this guide:
1. Check the solution folder for working code: `labs/python/lab2-mcp/solution/`
2. Review the `mcp-concepts.ipynb` notebook for conceptual understanding
3. Verify all prerequisites are met (Python version, dependencies, Azure OpenAI access)