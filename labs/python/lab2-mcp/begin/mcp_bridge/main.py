"""
MCP Bridge - Streamable HTTP MCP Server that wraps REST API.

This server exposes MCP tools over Streamable HTTP that call the REST API backend.

============================================================================
EXERCISE 5: Create the MCP Bridge
============================================================================
In this exercise, you will create an MCP server that uses Streamable HTTP
transport to expose tools that forward calls to the REST API backend.

Architecture:
   Agent Client -> MCP Bridge (:5070) -> REST API (:5060)

TODO: Uncomment the code below step by step as instructed in EXERCISES.md
============================================================================
"""
import asyncio
import json
import httpx
from pathlib import Path
from contextvars import ContextVar
from mcp.server import Server
from mcp.server.streamable_http import StreamableHTTPServerTransport
from mcp.types import Tool, TextContent
import uvicorn

# REST API base URL
REST_API_URL = "http://localhost:5060"


# ============================================================================
# STEP 5.1: Create MCP server instance
# Uncomment the line below
# ============================================================================
# mcp = Server("mcp-bridge")


# ============================================================================
# STEP 5.2: Define the list_tools handler
# Uncomment the entire function below
# ============================================================================
# @mcp.list_tools()
# async def list_tools() -> list[Tool]:
#     """List all available tools."""
#     return [
#         Tool(
#             name="GetAllTickets",
#             description="Gets all support tickets from the REST API with optional limit",
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
#             description="Gets a support ticket by ID from the REST API",
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
#             description="Updates a support ticket status via the REST API",
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
# STEP 5.3: Define the call_tool handler
# This handler forwards tool calls to the REST API backend
# Uncomment the entire function below
# ============================================================================
# @mcp.call_tool()
# async def call_tool(name: str, arguments: dict) -> list[TextContent]:
#     """Handle tool calls by forwarding to REST API."""
#
#     print(f"[TOOL CALL] Tool: {name}, Arguments: {arguments}")
#
#     async with httpx.AsyncClient() as client:
#         if name == "GetAllTickets":
#             max_results = arguments.get("maxResults", 5)
#             try:
#                 response = await client.get(f"{REST_API_URL}/api/tickets?maxResults={max_results}")
#                 if response.status_code == 200:
#                     return [TextContent(type="text", text=response.text)]
#                 return [TextContent(type="text", text="Failed to retrieve tickets")]
#             except Exception as e:
#                 return [TextContent(type="text", text=f"Error calling REST API: {str(e)}")]
#
#         elif name == "GetTicket":
#             ticket_id = arguments.get("ticket_id", "")
#             try:
#                 response = await client.get(f"{REST_API_URL}/api/tickets/{ticket_id}")
#                 if response.status_code == 200:
#                     return [TextContent(type="text", text=response.text)]
#                 return [TextContent(type="text", text=f"Ticket '{ticket_id}' not found")]
#             except Exception as e:
#                 return [TextContent(type="text", text=f"Error calling REST API: {str(e)}")]
#
#         elif name == "UpdateTicket":
#             ticket_id = arguments.get("ticket_id", "")
#             status = arguments.get("status", "")
#             try:
#                 response = await client.put(
#                     f"{REST_API_URL}/api/tickets/{ticket_id}",
#                     json={"status": status}
#                 )
#                 if response.status_code == 200:
#                     return [TextContent(type="text", text=f"Ticket '{ticket_id}' status updated to '{status}'")]
#                 return [TextContent(type="text", text=f"Ticket '{ticket_id}' not found")]
#             except Exception as e:
#                 return [TextContent(type="text", text=f"Error calling REST API: {str(e)}")]
#
#     return [TextContent(type="text", text=f"Unknown tool: {name}")]


# ============================================================================
# STEP 5.4: Set up Streamable HTTP transport and ASGI app
# Uncomment all the code below
# ============================================================================
# transport = None
# server_task = None
#
#
# async def ensure_server_running():
#     """Ensure the MCP server is running with the transport."""
#     global transport, server_task
#
#     if transport is None:
#         transport = StreamableHTTPServerTransport(
#             mcp_session_id=None,
#             is_json_response_enabled=True
#         )
#
#         async def run_server():
#             async with transport.connect() as streams:
#                 print("[MCP] Server connected and running...")
#                 try:
#                     await mcp.run(
#                         streams[0],
#                         streams[1],
#                         mcp.create_initialization_options()
#                     )
#                 except Exception as e:
#                     print(f"[MCP] Server error: {e}")
#                     import traceback
#                     traceback.print_exc()
#
#         server_task = asyncio.create_task(run_server())
#         await asyncio.sleep(0.1)
#         print("[MCP] Server started")
#
#     return transport
#
#
# async def handle_mcp(scope, receive, send):
#     """Handle /mcp endpoint using Streamable HTTP transport."""
#     t = await ensure_server_running()
#     try:
#         await t.handle_request(scope, receive, send)
#     except Exception as e:
#         print(f"[MCP] Error: {e}")
#
#
# async def app(scope, receive, send):
#     """Main ASGI application."""
#     if scope["type"] == "lifespan":
#         while True:
#             message = await receive()
#             if message["type"] == "lifespan.startup":
#                 print("[STARTUP] MCP Bridge Server starting...")
#                 await send({"type": "lifespan.startup.complete"})
#             elif message["type"] == "lifespan.shutdown":
#                 print("[SHUTDOWN] Shutting down...")
#                 global transport, server_task
#                 if transport:
#                     await transport.terminate()
#                 await send({"type": "lifespan.shutdown.complete"})
#                 return
#         return
#
#     if scope["type"] != "http":
#         return
#
#     path = scope.get("path", "")
#     method = scope.get("method", "GET")
#
#     if path == "/mcp" or path == "/mcp/":
#         await handle_mcp(scope, receive, send)
#     elif path == "/" and method == "GET":
#         from starlette.responses import JSONResponse
#         response = JSONResponse({
#             "name": "MCP Bridge Server",
#             "transport": "Streamable HTTP",
#             "endpoint": "/mcp",
#             "tools": ["GetAllTickets", "GetTicket", "UpdateTicket"]
#         })
#         await response(scope, receive, send)
#     elif path == "/health" and method == "GET":
#         from starlette.responses import JSONResponse
#         response = JSONResponse({"status": "healthy", "server": "MCP Bridge Server"})
#         await response(scope, receive, send)
#     else:
#         from starlette.responses import Response
#         response = Response(content="Not Found", status_code=404)
#         await response(scope, receive, send)


# ============================================================================
# STEP 5.5: Run the server
# Uncomment the lines below and REMOVE the placeholder code
# ============================================================================
# if __name__ == "__main__":
#     port = 5070
#     url = f"http://localhost:{port}"
#     print("=" * 60)
#     print("       MCP Bridge Server (Streamable HTTP Transport)")
#     print("=" * 60)
#     print(f"  Server URL:   {url}")
#     print(f"  MCP Endpoint: {url}/mcp")
#     print(f"  Health Check: {url}/health")
#     print()
#     print("  Available MCP Tools:")
#     print("     - GetAllTickets")
#     print("     - GetTicket")
#     print("     - UpdateTicket")
#     print()
#     uvicorn.run(app, host="0.0.0.0", port=port)

# Placeholder - REMOVE after uncommenting above
if __name__ == "__main__":
    print("Exercise 5 not completed. Please uncomment the code above.")
