"""
MCP Local Server - Python implementation using STDIO transport.

This server exposes ticket management tools via MCP (same tools as MCP Bridge).
"""
import asyncio
import json
from pathlib import Path
from mcp.server import Server
from mcp.server.stdio import stdio_server
from mcp.types import Tool, TextContent


def load_tickets() -> dict[str, dict]:
    """Load tickets by traversing up directories to find assets/tickets.json."""
    current = Path(__file__).resolve().parent
    while current != current.parent:
        data_file = current / "assets" / "tickets.json"
        if data_file.exists():
            print(f"Found tickets at: {data_file}", file=__import__('sys').stderr)
            with open(data_file, "r") as f:
                ticket_list = json.load(f)
                print(f"Loaded {len(ticket_list)} tickets", file=__import__('sys').stderr)
                return {t["id"]: t for t in ticket_list}
        current = current.parent
    print("Warning: assets/tickets.json not found", file=__import__('sys').stderr)
    return {}


# In-memory ticket storage loaded from shared JSON file
tickets: dict[str, dict] = load_tickets()

# Create MCP server
server = Server("mcp-local-server")


@server.list_tools()
async def list_tools() -> list[Tool]:
    """List all available tools (same as MCP Bridge)."""
    return [
        Tool(
            name="GetAllTickets",
            description="Gets all support tickets with optional limit",
            inputSchema={
                "type": "object",
                "properties": {
                    "maxResults": {
                        "type": "integer",
                        "description": "Maximum number of tickets to return (default: 5)",
                        "default": 5
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="GetTicket",
            description="Gets a support ticket by ID",
            inputSchema={
                "type": "object",
                "properties": {
                    "ticket_id": {
                        "type": "string",
                        "description": "The ticket ID (e.g., TICKET-001)"
                    }
                },
                "required": ["ticket_id"]
            }
        ),
        Tool(
            name="UpdateTicket",
            description="Updates a support ticket status",
            inputSchema={
                "type": "object",
                "properties": {
                    "ticket_id": {
                        "type": "string",
                        "description": "The ticket ID"
                    },
                    "status": {
                        "type": "string",
                        "description": "The new status (Open, In Progress, Resolved, Closed)"
                    }
                },
                "required": ["ticket_id", "status"]
            }
        ),
    ]


@server.call_tool()
async def call_tool(name: str, arguments: dict) -> list[TextContent]:
    """Handle tool calls."""
    
    if name == "GetAllTickets":
        max_results = arguments.get("maxResults", 5)
        all_tickets = list(tickets.values())[:max_results]
        return [TextContent(type="text", text=json.dumps(all_tickets, indent=2))]
    
    elif name == "GetTicket":
        ticket_id = arguments.get("ticket_id", "")
        if ticket_id in tickets:
            ticket = tickets[ticket_id]
            return [TextContent(type="text", text=json.dumps(ticket, indent=2))]
        return [TextContent(type="text", text=f"Ticket '{ticket_id}' not found")]
    
    elif name == "UpdateTicket":
        ticket_id = arguments.get("ticket_id", "")
        status = arguments.get("status", "")
        if ticket_id in tickets:
            tickets[ticket_id]["status"] = status
            return [TextContent(type="text", text=f"Ticket '{ticket_id}' status updated to '{status}'")]
        return [TextContent(type="text", text=f"Ticket '{ticket_id}' not found")]
    
    return [TextContent(type="text", text=f"Unknown tool: {name}")]


async def main():
    """Run the MCP server using STDIO transport."""
    async with stdio_server() as (read_stream, write_stream):
        await server.run(read_stream, write_stream, server.create_initialization_options())


if __name__ == "__main__":
    asyncio.run(main())
