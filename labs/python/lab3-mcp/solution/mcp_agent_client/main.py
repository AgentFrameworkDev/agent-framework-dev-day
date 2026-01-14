"""
MCP Agent Client - AI Agent that consumes MCP servers.

Demonstrates:
1. Local MCP via STDIO transport
2. Remote MCP via HTTP/SSE transport
"""
import asyncio
import os
import sys
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client
from mcp.client.sse import sse_client
from openai import AzureOpenAI
from azure.identity import AzureCliCredential


async def demo_local_mcp():
    """Demo: Connect to local MCP server via STDIO."""
    print("\n" + "=" * 60)
    print("Demo 1: Local MCP Server (STDIO)")
    print("=" * 60)
    
    server_params = StdioServerParameters(
        command=sys.executable,
        args=["-m", "mcp_local_server.main"],
        cwd=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    )
    
    async with stdio_client(server_params) as (read, write):
        async with ClientSession(read, write) as session:
            await session.initialize()
            
            # List available tools
            tools = await session.list_tools()
            print("\nAvailable tools:")
            for tool in tools.tools:
                print(f"  - {tool.name}: {tool.description}")
            
            # Call GetConfig tool
            print("\nCalling GetConfig('theme')...")
            result = await session.call_tool("GetConfig", {"key": "theme"})
            print(f"Result: {result.content[0].text}")
            
            # Call UpdateConfig tool
            print("\nCalling UpdateConfig('theme', 'light')...")
            result = await session.call_tool("UpdateConfig", {"key": "theme", "value": "light"})
            print(f"Result: {result.content[0].text}")
            
            # Call GetTicket tool
            print("\nCalling GetTicket('TICKET-001')...")
            result = await session.call_tool("GetTicket", {"ticket_id": "TICKET-001"})
            print(f"Result: {result.content[0].text}")


async def demo_remote_mcp():
    """Demo: Connect to remote MCP server via HTTP/SSE."""
    print("\n" + "=" * 60)
    print("Demo 2: Remote MCP Server (HTTP/SSE → REST API)")
    print("=" * 60)
    
    url = "http://localhost:5070/sse"
    print(f"\nConnecting to {url}...")
    
    try:
        async with sse_client(url) as (read, write):
            async with ClientSession(read, write) as session:
                await session.initialize()
                
                # List available tools
                tools = await session.list_tools()
                print("\nAvailable tools:")
                for tool in tools.tools:
                    print(f"  - {tool.name}: {tool.description}")
                
                # Call GetTicket tool (calls REST API)
                print("\nCalling GetTicket('TICKET-001') via REST API...")
                result = await session.call_tool("GetTicket", {"ticket_id": "TICKET-001"})
                print(f"Result: {result.content[0].text}")
                
                # Call UpdateTicket tool
                print("\nCalling UpdateTicket('TICKET-001', 'Resolved') via REST API...")
                result = await session.call_tool("UpdateTicket", {"ticket_id": "TICKET-001", "status": "Resolved"})
                print(f"Result: {result.content[0].text}")
    except Exception as e:
        print(f"\nError: {e}")
        print("Make sure the MCP Bridge (port 5070) and REST API (port 5060) are running.")


async def demo_with_ai_agent():
    """Demo: Use MCP tools with Azure OpenAI agent."""
    print("\n" + "=" * 60)
    print("Demo 3: AI Agent with MCP Tools")
    print("=" * 60)
    
    endpoint = os.environ.get("AZURE_OPENAI_ENDPOINT")
    deployment = os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o-mini")
    
    if not endpoint:
        print("\nSkipping AI demo - AZURE_OPENAI_ENDPOINT not set")
        return
    
    # Get Azure credential
    credential = AzureCliCredential()
    token = credential.get_token("https://cognitiveservices.azure.com/.default")
    
    client = AzureOpenAI(
        azure_endpoint=endpoint,
        api_key=token.token,
        api_version="2024-02-15-preview"
    )
    
    # Connect to local MCP to get tools
    server_params = StdioServerParameters(
        command=sys.executable,
        args=["-m", "mcp_local_server.main"],
        cwd=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    )
    
    async with stdio_client(server_params) as (read, write):
        async with ClientSession(read, write) as session:
            await session.initialize()
            
            # Get tools for OpenAI
            mcp_tools = await session.list_tools()
            openai_tools = [
                {
                    "type": "function",
                    "function": {
                        "name": tool.name,
                        "description": tool.description,
                        "parameters": tool.inputSchema
                    }
                }
                for tool in mcp_tools.tools
            ]
            
            # Chat with AI
            messages = [
                {"role": "system", "content": "You are a helpful assistant with access to configuration and ticket tools."},
                {"role": "user", "content": "What is the current theme configuration?"}
            ]
            
            print("\nUser: What is the current theme configuration?")
            
            response = client.chat.completions.create(
                model=deployment,
                messages=messages,
                tools=openai_tools
            )
            
            # Handle tool calls
            if response.choices[0].message.tool_calls:
                for tool_call in response.choices[0].message.tool_calls:
                    print(f"\nAI calling tool: {tool_call.function.name}")
                    import json
                    args = json.loads(tool_call.function.arguments)
                    result = await session.call_tool(tool_call.function.name, args)
                    print(f"Tool result: {result.content[0].text}")
            else:
                print(f"\nAI: {response.choices[0].message.content}")


async def main():
    """Run all demos."""
    print("=" * 60)
    print("MCP Agent Client - Python Demo")
    print("=" * 60)
    
    # Demo 1: Local MCP
    await demo_local_mcp()
    
    # Demo 2: Remote MCP
    await demo_remote_mcp()
    
    # Demo 3: AI Agent
    await demo_with_ai_agent()
    
    print("\n" + "=" * 60)
    print("All demos completed!")
    print("=" * 60)


if __name__ == "__main__":
    asyncio.run(main())
