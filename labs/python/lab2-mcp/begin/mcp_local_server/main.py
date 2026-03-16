"""
MCP Local Server - Python implementation using STDIO transport.

This server exposes ticket management tools via MCP.

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
from pathlib import Path
from mcp.server import Server
from mcp.server.stdio import stdio_server
from mcp.types import Tool, TextContent


def load_tickets() -> dict[str, dict]:
    """Load tickets from shared data/tickets.json file."""
    data_file = Path(__file__).parent.parent.parent / "data" / "tickets.json"
    if data_file.exists():
        with open(data_file, "r") as f:
            ticket_list = json.load(f)
            return {t["id"]: t for t in ticket_list}
    # Fallback if file not found
    return {
        "TICKET-001": {"id": "TICKET-001", "customerName": "Alice Johnson", "subject": "Login issue", "description": "Cannot login to the system", "status": "Open", "priority": "High"},
        "TICKET-002": {"id": "TICKET-002", "customerName": "Bob Smith", "subject": "Performance problem", "description": "System is running slowly", "status": "In Progress", "priority": "Medium"},
        "TICKET-003": {"id": "TICKET-003", "customerName": "Carol White", "subject": "Data sync error", "description": "Data not syncing properly", "status": "Open", "priority": "High"},
    }


# ============================================================================
# In-memory storage (pre-configured - no changes needed)
# ============================================================================
ticket_store: dict[str, dict] = load_tickets()

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
#             name="GetAllTickets",
#             description="Gets all support tickets with optional limit",
#             inputSchema={
#                 "type": "object",
#                 "properties": {
#                     "maxResults": {
#                         "type": "integer",
#                         "description": "Maximum number of tickets to return (default: 5)",
#                         "default": 5
#                     }
#                 },
#                 "required": []
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
#     if name == "GetAllTickets":
#         max_results = arguments.get("maxResults", 5)
#         result = list(ticket_store.values())[:max_results]
#         return [TextContent(type="text", text=json.dumps(result, indent=2))]
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
