"""
MCP Agent Client - AI Agent that consumes MCP servers.

============================================================================
EXERCISES 1, 3, 4: MCP Agent Client
============================================================================
This file contains exercises for:
- Exercise 1: Configure Azure OpenAI credentials
- Exercise 3: Connect to Local MCP Server
- Exercise 4: Connect to Remote MCP Server (HTTP/SSE)

Follow the instructions in EXERCISES.md to complete each exercise.
============================================================================
"""
import asyncio
import os
import sys
import json
from pathlib import Path
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client
from mcp.client.sse import sse_client
from openai import AzureOpenAI
from azure.identity import AzureCliCredential


def find_config_path(start_path: str) -> str:
    """Find the 'python' folder by traversing up from start_path."""
    current_dir = Path(start_path)
    
    while current_dir is not None:
        if current_dir.name.lower() == "python":
            return str(current_dir)
        if current_dir.parent == current_dir:
            break
        current_dir = current_dir.parent
    
    # Fallback to start path if python folder not found
    return start_path


def load_env_file(env_path: str) -> dict:
    """Load environment variables from .env file (JSON format)."""
    env_file = Path(env_path) / ".env"
    
    if not env_file.exists():
        return {}
    
    try:
        with open(env_file, 'r') as f:
            content = f.read()
            env_vars = json.loads(content)
            
            # Set environment variables
            for key, value in env_vars.items():
                os.environ[key] = str(value)
            
            return env_vars
    except (json.JSONDecodeError, IOError) as e:
        print(f"Warning: Failed to load .env file: {e}")
        return {}


# Load environment variables from .env file
config_path = find_config_path(os.path.dirname(os.path.abspath(__file__)))
env_vars = load_env_file(config_path)
if env_vars:
    print(f"Loaded {len(env_vars)} environment variables from: {config_path}/.env")


print("=" * 60)
print("       MCP Workshop - Agent Client Demo (Python)")
print("   Demonstrating Local and Remote MCP Servers")
print("=" * 60)
print()


# ============================================================================
# EXERCISE 1: Configure Azure OpenAI
# ============================================================================
# STEP 1.1: Get Azure OpenAI credentials from environment
# Uncomment the lines below to read from environment variables
# ============================================================================
# endpoint = os.environ.get("AZURE_OPENAI_ENDPOINT")
# deployment = os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME") or \
#              os.environ.get("AZURE_AI_MODEL_DEPLOYMENT_NAME") or \
#              "gpt-4o-mini"
# 
# if not endpoint:
#     raise ValueError("AZURE_OPENAI_ENDPOINT environment variable is not set")

# Placeholder values - REPLACE after uncommenting above
endpoint = "https://YOUR-RESOURCE.openai.azure.com/"
deployment = "gpt-4o-mini"

print(f"Using Azure OpenAI endpoint: {endpoint}")
print(f"Deployment: {deployment}")
print()


# ============================================================================
# STEP 1.2: Create Azure OpenAI client function
# Uncomment the entire function below
# ============================================================================
# def create_openai_client() -> AzureOpenAI:
#     """Create Azure OpenAI client with Azure CLI credentials."""
#     credential = AzureCliCredential()
#     token = credential.get_token("https://cognitiveservices.azure.com/.default")
#     
#     return AzureOpenAI(
#         azure_endpoint=endpoint,
#         api_key=token.token,
#         api_version="2024-02-15-preview"
#     )


# ============================================================================
# EXERCISE 3: Connect to Local MCP Server
# ============================================================================
# Uncomment the entire function below
# ============================================================================
# async def demo_local_mcp():
#     """Demo: Connect to local MCP server via STDIO."""
#     print("\n" + "=" * 60)
#     print("Demo: Local MCP Server (STDIO)")
#     print("=" * 60)
#     
#     # STEP 3.1: Create STDIO server parameters
#     server_params = StdioServerParameters(
#         command=sys.executable,
#         args=["-m", "mcp_local_server.main"],
#         cwd=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
#     )
#     
#     # STEP 3.2: Connect to the server using stdio_client
#     async with stdio_client(server_params) as (read, write):
#         async with ClientSession(read, write) as session:
#             # STEP 3.3: Initialize the session
#             await session.initialize()
#             
#             # STEP 3.4: List available tools
#             tools = await session.list_tools()
#             print("\nAvailable tools:")
#             for tool in tools.tools:
#                 print(f"  - {tool.name}: {tool.description}")
#             
#             # STEP 3.5: Call GetConfig tool
#             print("\nCalling GetConfig('theme')...")
#             result = await session.call_tool("GetConfig", {"key": "theme"})
#             print(f"Result: {result.content[0].text}")
#             
#             # Call UpdateConfig tool
#             print("\nCalling UpdateConfig('theme', 'light')...")
#             result = await session.call_tool("UpdateConfig", {"key": "theme", "value": "light"})
#             print(f"Result: {result.content[0].text}")
#             
#             # Call GetTicket tool
#             print("\nCalling GetTicket('TICKET-001')...")
#             result = await session.call_tool("GetTicket", {"ticket_id": "TICKET-001"})
#             print(f"Result: {result.content[0].text}")


# ============================================================================
# EXERCISE 4: Connect to Remote MCP Server (HTTP/SSE)
# ============================================================================
# Uncomment the entire function below
# ============================================================================
# async def demo_remote_mcp():
#     """Demo: Connect to remote MCP server via HTTP/SSE."""
#     print("\n" + "=" * 60)
#     print("Demo: Remote MCP Server (HTTP/SSE ? REST API)")
#     print("=" * 60)
#     
#     # STEP 4.1: Define the SSE endpoint URL
#     url = "http://localhost:5070/sse"
#     print(f"\nConnecting to {url}...")
#     
#     try:
#         # STEP 4.2: Connect using sse_client
#         async with sse_client(url) as (read, write):
#             async with ClientSession(read, write) as session:
#                 # STEP 4.3: Initialize the session
#                 await session.initialize()
#                 
#                 # STEP 4.4: List available tools
#                 tools = await session.list_tools()
#                 print("\nAvailable tools:")
#                 for tool in tools.tools:
#                     print(f"  - {tool.name}: {tool.description}")
#                 
#                 # STEP 4.5: Call GetTicket tool (calls REST API)
#                 print("\nCalling GetTicket('TICKET-001') via REST API...")
#                 result = await session.call_tool("GetTicket", {"ticket_id": "TICKET-001"})
#                 print(f"Result: {result.content[0].text}")
#                 
#                 # Call UpdateTicket tool
#                 print("\nCalling UpdateTicket('TICKET-001', 'Resolved') via REST API...")
#                 result = await session.call_tool("UpdateTicket", {"ticket_id": "TICKET-001", "status": "Resolved"})
#                 print(f"Result: {result.content[0].text}")
#     except Exception as e:
#         print(f"\nError: {e}")
#         print("Make sure the MCP Bridge (port 5070) and REST API (port 5060) are running.")


# ============================================================================
# Demo with AI Agent (Bonus - uses MCP tools with Azure OpenAI)
# ============================================================================
# async def demo_with_ai_agent():
#     """Demo: Use MCP tools with Azure OpenAI agent."""
#     print("\n" + "=" * 60)
#     print("Demo: AI Agent with MCP Tools")
#     print("=" * 60)
#     
#     if endpoint.startswith("https://YOUR"):
#         print("\nSkipping AI demo - AZURE_OPENAI_ENDPOINT not configured")
#         return
#     
#     # Get Azure OpenAI client
#     client = create_openai_client()
#     
#     # Connect to local MCP to get tools
#     server_params = StdioServerParameters(
#         command=sys.executable,
#         args=["-m", "mcp_local_server.main"],
#         cwd=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
#     )
#     
#     async with stdio_client(server_params) as (read, write):
#         async with ClientSession(read, write) as session:
#             await session.initialize()
#             
#             # Get tools for OpenAI
#             mcp_tools = await session.list_tools()
#             openai_tools = [
#                 {
#                     "type": "function",
#                     "function": {
#                         "name": tool.name,
#                         "description": tool.description,
#                         "parameters": tool.inputSchema
#                     }
#                 }
#                 for tool in mcp_tools.tools
#             ]
#             
#             # Chat with AI
#             messages = [
#                 {"role": "system", "content": "You are a helpful assistant with access to configuration and ticket tools."},
#                 {"role": "user", "content": "What is the current theme configuration?"}
#             ]
#             
#             print("\nUser: What is the current theme configuration?")
#             
#             response = client.chat.completions.create(
#                 model=deployment,
#                 messages=messages,
#                 tools=openai_tools
#             )
#             
#             # Handle tool calls
#             if response.choices[0].message.tool_calls:
#                 for tool_call in response.choices[0].message.tool_calls:
#                     print(f"\nAI calling tool: {tool_call.function.name}")
#                     args = json.loads(tool_call.function.arguments)
#                     result = await session.call_tool(tool_call.function.name, args)
#                     print(f"Tool result: {result.content[0].text}")
#             else:
#                 print(f"\nAI: {response.choices[0].message.content}")


# ============================================================================
# Main Menu - Demo Selection
# ============================================================================
async def main():
    """Main menu for demo selection."""
    while True:
        print("=" * 60)
        print("Select a demo to run:")
        print("  1. Local MCP Server (Python via STDIO)")
        print("  2. Remote MCP Server (HTTP/SSE)")
        print("  3. AI Agent with MCP Tools (Bonus)")
        print("  4. Exit")
        print("=" * 60)
        
        choice = input("Enter choice (1-4): ").strip()
        print()
        
        if choice == "1":
            # ================================================================
            # EXERCISE 3: Uncomment to enable Local MCP Demo
            # ================================================================
            # await demo_local_mcp()
            print("Exercise 3 not completed. Please uncomment the demo_local_mcp function and call.")
            
        elif choice == "2":
            # ================================================================
            # EXERCISE 4: Uncomment to enable Remote MCP Demo
            # ================================================================
            # await demo_remote_mcp()
            print("Exercise 4 not completed. Please uncomment the demo_remote_mcp function and call.")
            
        elif choice == "3":
            # ================================================================
            # BONUS: Uncomment to enable AI Agent Demo
            # ================================================================
            # await demo_with_ai_agent()
            print("Bonus exercise not completed. Please uncomment the demo_with_ai_agent function and call.")
            
        elif choice == "4":
            print("Goodbye!")
            break
        else:
            print("Invalid choice. Please enter 1-4.")
        
        print()


if __name__ == "__main__":
    asyncio.run(main())
