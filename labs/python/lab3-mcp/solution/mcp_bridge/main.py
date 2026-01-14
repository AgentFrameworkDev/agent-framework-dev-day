"""
MCP Bridge - HTTP/SSE MCP Server that wraps REST API.

This server exposes MCP tools over HTTP/SSE that call the REST API backend.
"""
import asyncio
import httpx
from mcp.server import Server
from mcp.server.sse import SseServerTransport
from mcp.types import Tool, TextContent
from starlette.applications import Starlette
from starlette.routing import Route
from starlette.responses import JSONResponse

# REST API base URL
REST_API_URL = "http://localhost:5060"

# Create MCP server
server = Server("mcp-bridge")


@server.list_tools()
async def list_tools() -> list[Tool]:
    """List all available tools."""
    return [
        Tool(
            name="GetTicket",
            description="Gets a support ticket by ID from the REST API",
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
            description="Updates a support ticket status via the REST API",
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
    """Handle tool calls by forwarding to REST API."""
    
    async with httpx.AsyncClient() as client:
        if name == "GetTicket":
            ticket_id = arguments.get("ticket_id", "")
            try:
                response = await client.get(f"{REST_API_URL}/api/tickets/{ticket_id}")
                if response.status_code == 200:
                    return [TextContent(type="text", text=response.text)]
                return [TextContent(type="text", text=f"Ticket '{ticket_id}' not found")]
            except Exception as e:
                return [TextContent(type="text", text=f"Error calling REST API: {str(e)}")]
        
        elif name == "UpdateTicket":
            ticket_id = arguments.get("ticket_id", "")
            status = arguments.get("status", "")
            try:
                response = await client.put(
                    f"{REST_API_URL}/api/tickets/{ticket_id}",
                    json={"status": status}
                )
                if response.status_code == 200:
                    return [TextContent(type="text", text=f"Ticket '{ticket_id}' status updated to '{status}'")]
                return [TextContent(type="text", text=f"Ticket '{ticket_id}' not found")]
            except Exception as e:
                return [TextContent(type="text", text=f"Error calling REST API: {str(e)}")]
    
    return [TextContent(type="text", text=f"Unknown tool: {name}")]


# SSE transport for HTTP/SSE connections
sse_transport = SseServerTransport("/sse")


async def handle_sse(request):
    """Handle SSE connections."""
    async with sse_transport.connect_sse(
        request.scope, request.receive, request._send
    ) as streams:
        await server.run(
            streams[0], streams[1], server.create_initialization_options()
        )


async def handle_root(request):
    """Root endpoint."""
    return JSONResponse({
        "name": "MCP Bridge Server",
        "transport": "HTTP/SSE",
        "endpoint": "/sse",
        "tools": ["GetTicket", "UpdateTicket"]
    })


# Create Starlette app
app = Starlette(
    routes=[
        Route("/", handle_root),
        Route("/sse", handle_sse),
    ]
)


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=5070)
