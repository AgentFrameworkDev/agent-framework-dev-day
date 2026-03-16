"""
MCP Bridge - Streamable HTTP MCP Server that wraps REST API.

This server exposes MCP tools over Streamable HTTP that call the REST API backend.

NOTE: This file is pre-completed for the lab exercises.
The MCP Bridge and Remote Server are provided as working examples
to demonstrate Streamable HTTP transport calling a REST API.

Architecture:
   AgentClient -> MCP Bridge (:5070) -> REST API (:5060)
"""
import asyncio
from contextvars import ContextVar
import httpx
from mcp.server import Server
from mcp.server.streamable_http import StreamableHTTPServerTransport
from mcp.types import Tool, TextContent

# REST API base URL
REST_API_URL = "http://localhost:5060"

# Context var to propagate Authorization header from HTTP request into tool calls
current_auth_header: ContextVar[str | None] = ContextVar("current_auth_header", default=None)

# Create MCP server
mcp = Server("mcp-bridge")


@mcp.list_tools()
async def list_tools() -> list[Tool]:
    """List all available tools."""
    return [
        Tool(
            name="GetAllTickets",
            description="Gets all support tickets from the REST API with optional limit",
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


@mcp.call_tool()
async def call_tool(name: str, arguments: dict) -> list[TextContent]:
    """Handle tool calls by forwarding to REST API."""

    print(f"[TOOL CALL] Tool: {name}, Arguments: {arguments}")

    # Forward Authorization header if present
    auth_header = current_auth_header.get()
    headers = {}
    if auth_header:
        headers["Authorization"] = auth_header

    async with httpx.AsyncClient() as client:
        if name == "GetAllTickets":
            max_results = arguments.get("maxResults", 5)
            try:
                response = await client.get(
                    f"{REST_API_URL}/api/tickets?maxResults={max_results}",
                    headers=headers
                )
                if response.status_code == 200:
                    return [TextContent(type="text", text=response.text)]
                return [TextContent(type="text", text="Failed to retrieve tickets")]
            except Exception as e:
                return [TextContent(type="text", text=f"Error calling REST API: {str(e)}")]

        elif name == "GetTicket":
            ticket_id = arguments.get("ticket_id", "")
            try:
                response = await client.get(
                    f"{REST_API_URL}/api/tickets/{ticket_id}",
                    headers=headers
                )
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
                    json={"status": status},
                    headers=headers
                )
                if response.status_code == 200:
                    return [TextContent(type="text", text=f"Ticket '{ticket_id}' status updated to '{status}'")]
                return [TextContent(type="text", text=f"Ticket '{ticket_id}' not found")]
            except Exception as e:
                return [TextContent(type="text", text=f"Error calling REST API: {str(e)}")]

    return [TextContent(type="text", text=f"Unknown tool: {name}")]


# Global transport instance
transport = None
server_task = None


async def ensure_server_running():
    """Ensure the MCP server is running with the Streamable HTTP transport."""
    global transport, server_task

    if transport is None:
        transport = StreamableHTTPServerTransport(
            mcp_session_id=None,
            is_json_response_enabled=True
        )

        async def run_server():
            async with transport.connect() as streams:
                print("[MCP] Server connected and running...")
                try:
                    await mcp.run(
                        streams[0],
                        streams[1],
                        mcp.create_initialization_options()
                    )
                except Exception as e:
                    print(f"[MCP] Server error: {e}")

        server_task = asyncio.create_task(run_server())
        await asyncio.sleep(0.1)
        print("[MCP] Server started")

    return transport


async def handle_mcp(scope, receive, send):
    """Handle /mcp endpoint using Streamable HTTP transport."""
    method = scope.get("method", "GET")
    headers = dict(scope.get("headers", []))

    # Capture Authorization header and set in context
    auth_header = headers.get(b"authorization", b"").decode() or None
    token = current_auth_header.set(auth_header)

    t = await ensure_server_running()

    try:
        await t.handle_request(scope, receive, send)
    except Exception as e:
        print(f"[MCP] Error: {e}")
    finally:
        current_auth_header.reset(token)


async def app(scope, receive, send):
    """Main ASGI application."""
    if scope["type"] == "lifespan":
        while True:
            message = await receive()
            if message["type"] == "lifespan.startup":
                await send({"type": "lifespan.startup.complete"})
            elif message["type"] == "lifespan.shutdown":
                global transport, server_task
                if transport:
                    await transport.terminate()
                await send({"type": "lifespan.shutdown.complete"})
                return
        return

    if scope["type"] != "http":
        return

    path = scope.get("path", "")
    method = scope.get("method", "GET")

    if path == "/mcp" or path == "/mcp/":
        await handle_mcp(scope, receive, send)
    elif path == "/" and method == "GET":
        from starlette.responses import JSONResponse
        response = JSONResponse({
            "name": "MCP Bridge Server",
            "version": "1.0.0",
            "transport": "Streamable HTTP",
            "endpoint": "/mcp",
            "tools": ["GetAllTickets", "GetTicket", "UpdateTicket"]
        })
        await response(scope, receive, send)
    elif path == "/health" and method == "GET":
        from starlette.responses import JSONResponse
        response = JSONResponse({"status": "healthy", "server": "MCP Bridge Server"})
        await response(scope, receive, send)
    else:
        from starlette.responses import Response
        response = Response(content="Not Found", status_code=404)
        await response(scope, receive, send)


if __name__ == "__main__":
    import uvicorn
    print("Starting MCP Bridge Server on http://localhost:5070")
    print("MCP endpoint: http://localhost:5070/mcp")
    uvicorn.run(app, host="0.0.0.0", port=5070)
