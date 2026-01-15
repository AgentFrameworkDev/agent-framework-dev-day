"""
MCP Local Server - Python implementation using STDIO transport.

This server exposes configuration and ticket management tools via MCP.

============================================================================
EXERCISE 2: Create the MCP Server
============================================================================
In this exercise, you will set up a Python MCP server using STDIO transport.
The server will expose tools for configuration and ticket management.

TODO: Uncomment the code below step by step as instructed in EXERCISES.md
============================================================================
"""
import asyncio
import json
from mcp.server import Server
from mcp.server.stdio import stdio_server
from mcp.types import Tool, TextContent

# ============================================================================
# In-memory storage (pre-configured - no changes needed)
# ============================================================================
config_store: dict[str, str] = {
    "theme": "dark",
    "language": "en",
    "timeout": "30"
}

ticket_store: dict[str, dict] = {
    "TICKET-001": {"id": "TICKET-001", "title": "Login issue", "status": "Open", "description": "Cannot login to the system"},
    "TICKET-002": {"id": "TICKET-002", "title": "Performance problem", "status": "In Progress", "description": "System is running slowly"},
}

# ============================================================================
# STEP 2.1: Create MCP server instance
# Uncomment the line below to create the server
# ============================================================================
# server = Server("mcp-local-server")


# ============================================================================
# STEP 2.2: Define the list_tools handler
# This handler returns all available tools to clients
# Uncomment the entire function below
# ============================================================================
# @server.list_tools()
# async def list_tools() -> list[Tool]:
#     """List all available tools."""
#     return [
#         Tool(
#             name="GetConfig",
#             description="Gets a configuration value by key",
#             inputSchema={
#                 "type": "object",
#                 "properties": {
#                     "key": {
#                         "type": "string",
#                         "description": "The configuration key"
#                     }
#                 },
#                 "required": ["key"]
#             }
#         ),
#         Tool(
#             name="UpdateConfig",
#             description="Updates a configuration value",
#             inputSchema={
#                 "type": "object",
#                 "properties": {
#                     "key": {
#                         "type": "string",
#                         "description": "The configuration key"
#                     },
#                     "value": {
#                         "type": "string",
#                         "description": "The new value"
#                     }
#                 },
#                 "required": ["key", "value"]
#             }
#         ),
#         Tool(
#             name="GetTicket",
#             description="Gets a support ticket by ID",
#             inputSchema={
#                 "type": "object",
#                 "properties": {
#                     "ticket_id": {
#                         "type": "string",
#                         "description": "The ticket ID (e.g., TICKET-001)"
#                     }
#                 },
#                 "required": ["ticket_id"]
#             }
#         ),
#         Tool(
#             name="UpdateTicket",
#             description="Updates a support ticket status",
#             inputSchema={
#                 "type": "object",
#                 "properties": {
#                     "ticket_id": {
#                         "type": "string",
#                         "description": "The ticket ID"
#                     },
#                     "status": {
#                         "type": "string",
#                         "description": "The new status (Open, In Progress, Resolved, Closed)"
#                     }
#                 },
#                 "required": ["ticket_id", "status"]
#             }
#         ),
#     ]


# ============================================================================
# STEP 2.3: Define the call_tool handler
# This handler executes tool calls from clients
# Uncomment the entire function below
# ============================================================================
# @server.call_tool()
# async def call_tool(name: str, arguments: dict) -> list[TextContent]:
#     """Handle tool calls."""
#     
#     if name == "GetConfig":
#         key = arguments.get("key", "")
#         if key in config_store:
#             return [TextContent(type="text", text=f"Configuration '{key}' = '{config_store[key]}'")]
#         return [TextContent(type="text", text=f"Configuration key '{key}' not found")]
#     
#     elif name == "UpdateConfig":
#         key = arguments.get("key", "")
#         value = arguments.get("value", "")
#         config_store[key] = value
#         return [TextContent(type="text", text=f"Configuration '{key}' updated to '{value}'")]
#     
#     elif name == "GetTicket":
#         ticket_id = arguments.get("ticket_id", "")
#         if ticket_id in ticket_store:
#             ticket = ticket_store[ticket_id]
#             return [TextContent(type="text", text=json.dumps(ticket, indent=2))]
#         return [TextContent(type="text", text=f"Ticket '{ticket_id}' not found")]
#     
#     elif name == "UpdateTicket":
#         ticket_id = arguments.get("ticket_id", "")
#         status = arguments.get("status", "")
#         if ticket_id in ticket_store:
#             ticket_store[ticket_id]["status"] = status
#             return [TextContent(type="text", text=f"Ticket '{ticket_id}' status updated to '{status}'")]
#         return [TextContent(type="text", text=f"Ticket '{ticket_id}' not found")]
#     
#     return [TextContent(type="text", text=f"Unknown tool: {name}")]


# ============================================================================
# STEP 2.4: Define the main function with STDIO transport
# Uncomment the entire function below
# ============================================================================
# async def main():
#     """Run the MCP server using STDIO transport."""
#     async with stdio_server() as (read_stream, write_stream):
#         await server.run(read_stream, write_stream, server.create_initialization_options())


# ============================================================================
# STEP 2.5: Run the server
# Uncomment the lines below and REMOVE the placeholder code
# ============================================================================
# if __name__ == "__main__":
#     asyncio.run(main())

# Placeholder - REMOVE after uncommenting above
if __name__ == "__main__":
    print("Exercise 2 not completed. Please uncomment the code above.", file=__import__('sys').stderr)
