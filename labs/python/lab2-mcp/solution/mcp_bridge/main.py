"""
MCP Bridge - HTTP/SSE MCP Server that wraps REST API.

This server exposes MCP tools over HTTP/SSE that call the REST API backend.
"""
import asyncio
import json
import httpx
from pathlib import Path
from contextvars import ContextVar
from dotenv import load_dotenv
from mcp.server import Server
from mcp.server.streamable_http import StreamableHTTPServerTransport
from mcp.types import Tool, TextContent
import uvicorn

# Context variable to pass Authorization header to tool calls
current_auth_header: ContextVar[str | None] = ContextVar('current_auth_header', default=None)

# Load .env file
env_path = Path(__file__).parent.parent.parent.parent / '.env'
load_dotenv(env_path)

# REST API base URL
REST_API_URL = "http://localhost:5060"

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
    
    # Get the Authorization header from context
    auth_header = current_auth_header.get()
    headers = {}
    if auth_header:
        headers["Authorization"] = auth_header
        print(f"[TOOL CALL] Forwarding Authorization header")
    else:
        print(f"[TOOL CALL] No Authorization header to forward")
    
    async with httpx.AsyncClient() as client:
        if name == "GetAllTickets":
            max_results = arguments.get("maxResults", 5)
            try:
                response = await client.get(f"{REST_API_URL}/api/tickets?maxResults={max_results}", headers=headers)
                if response.status_code == 200:
                    result = response.text
                    print(f"[TOOL CALL] GetAllTickets result: {result[:200]}...")
                    return [TextContent(type="text", text=result)]
                return [TextContent(type="text", text="Failed to retrieve tickets")]
            except Exception as e:
                print(f"[TOOL CALL] Error: {e}")
                return [TextContent(type="text", text=f"Error calling REST API: {str(e)}")]
        
        elif name == "GetTicket":
            ticket_id = arguments.get("ticket_id", "")
            try:
                response = await client.get(f"{REST_API_URL}/api/tickets/{ticket_id}", headers=headers)
                if response.status_code == 200:
                    result = response.text
                    print(f"[TOOL CALL] GetTicket result: {result[:200]}...")
                    return [TextContent(type="text", text=result)]
                return [TextContent(type="text", text=f"Ticket '{ticket_id}' not found")]
            except Exception as e:
                print(f"[TOOL CALL] Error: {e}")
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
                    result = f"Ticket '{ticket_id}' status updated to '{status}'"
                    print(f"[TOOL CALL] UpdateTicket result: {result}")
                    return [TextContent(type="text", text=result)]
                return [TextContent(type="text", text=f"Ticket '{ticket_id}' not found")]
            except Exception as e:
                print(f"[TOOL CALL] Error: {e}")
                return [TextContent(type="text", text=f"Error calling REST API: {str(e)}")]
    
    return [TextContent(type="text", text=f"Unknown tool: {name}")]


# Global transport and session - single instance like .NET
transport = None
server_task = None


async def ensure_server_running():
    """Ensure the MCP server is running with the transport."""
    global transport, server_task
    
    if transport is None:
        transport = StreamableHTTPServerTransport(
            mcp_session_id=None,  # Let the transport manage session
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
                    import traceback
                    traceback.print_exc()
        
        import asyncio
        server_task = asyncio.create_task(run_server())
        await asyncio.sleep(0.1)  # Give it time to start
        print("[MCP] Server started")
    
    return transport


async def handle_mcp(scope, receive, send):
    """Handle /mcp endpoint using Streamable HTTP transport."""
    method = scope.get("method", "GET")
    headers = dict(scope.get("headers", []))
    session_id = headers.get(b"mcp-session-id", b"").decode() or None
    
    # Capture Authorization header and set in context
    auth_header = headers.get(b"authorization", b"").decode() or None
    token = current_auth_header.set(auth_header)
    
    print(f"[MCP] {method} request, session_id: {session_id}, auth_header: {'present' if auth_header else 'None'}")
    
    t = await ensure_server_running()
    
    try:
        await t.handle_request(scope, receive, send)
        print(f"[MCP] Request handled successfully")
    except Exception as e:
        print(f"[MCP] Error: {e}")
        import traceback
        traceback.print_exc()
    finally:
        current_auth_header.reset(token)


# Create the main ASGI app
async def app(scope, receive, send):
    """Main ASGI application."""
    if scope["type"] == "lifespan":
        while True:
            message = await receive()
            if message["type"] == "lifespan.startup":
                print("[STARTUP] MCP Bridge Server starting...")
                await send({"type": "lifespan.startup.complete"})
            elif message["type"] == "lifespan.shutdown":
                print("[SHUTDOWN] Shutting down...")
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
    
    print(f"[APP] {method} {path}")
    
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
    port = 5070
    url = f"http://localhost:{port}"
    
    print("=" * 80)
    print("              MCP Bridge Server (HTTP/SSE Transport)                     ")
    print("=" * 80)
    print()
    print(f"Server URL:      {url}")
    print(f"MCP Endpoint:    {url}/mcp (GET=SSE, POST=messages)")
    print(f"Health Check:    {url}/health")
    print()
    print("Available MCP Tools:")
    print("   - GetAllTickets : Gets all support tickets from REST API")
    print("   - GetTicket     : Gets a support ticket by ID")
    print("   - UpdateTicket  : Updates a support ticket status")
    print()
    print(f"MCP Server ready! Connect your MCP client to {url}/mcp")
    print()
    
    uvicorn.run(app, host="0.0.0.0", port=port)
