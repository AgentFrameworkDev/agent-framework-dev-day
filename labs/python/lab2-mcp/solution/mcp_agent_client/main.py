"""
MCP Agent Client - AI Agent that consumes MCP servers.

Demonstrates:
1. Local MCP via STDIO transport
2. Remote MCP via HTTP/SSE transport
"""
import asyncio
import json
import os
import sys
from pathlib import Path
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client
from mcp.client.sse import sse_client
from openai import AzureOpenAI
from azure.identity import AzureCliCredential, ClientSecretCredential
from azure.ai.projects import AIProjectClient


# ANSI color codes for terminal output
class Colors:
    CYAN = '\033[96m'
    GREEN = '\033[92m'
    RED = '\033[91m'
    RESET = '\033[0m'


def print_colored(text: str, color: str, end: str = '\n'):
    """Print text with color."""
    print(f"{color}{text}{Colors.RESET}", end=end)


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


def validate_env_config() -> bool:
    """Validate that required environment variables are loaded."""
    print("\n" + "=" * 60)
    print("Environment Configuration Validation")
    print("=" * 60)
    
    # Find and load config from python folder
    config_path = find_config_path(os.path.dirname(os.path.abspath(__file__)))
    print(f"\nConfig path: {config_path}")
    
    env_vars = load_env_file(config_path)
    
    if env_vars:
        print(f"\n✓ .env file loaded successfully from: {config_path}")
        print(f"  Loaded {len(env_vars)} environment variables:")
        for key in env_vars.keys():
            # Mask sensitive values
            if "SECRET" in key or "KEY" in key or "PASSWORD" in key:
                print(f"    - {key}: ****")
            else:
                value = env_vars.get(key, "")
                display_value = value[:50] + "..." if len(str(value)) > 50 else value
                print(f"    - {key}: {display_value}")
    else:
        print(f"\n✗ No .env file found or failed to load from: {config_path}")
        return False
    
    # Check for Azure OpenAI configuration
    endpoint = os.environ.get("AZURE_AI_PROJECT_ENDPOINT") or os.environ.get("AZURE_OPENAI_ENDPOINT")
    deployment = os.environ.get("AZURE_AI_MODEL_DEPLOYMENT_NAME") or os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o-mini")
    
    print(f"\nAzure OpenAI Configuration:")
    print(f"  Endpoint: {endpoint or 'Not set'}")
    print(f"  Deployment: {deployment}")
    
    if not endpoint:
        print("\n⚠ Warning: Azure OpenAI endpoint not configured. AI demos will be skipped.")
    
    print("=" * 60)
    return True


async def demo_local_mcp() -> bool:
    """Demo: Connect to local MCP server via STDIO with interactive session."""
    print("\n" + "=" * 60)
    print("       Demo 1: Local MCP Server (STDIO Transport)")
    print("=" * 60)
    print()
    
    print("Connecting to Local Python MCP Server...")
    
    server_params = StdioServerParameters(
        command=sys.executable,
        args=["-m", "mcp_local_server.main"],
        cwd=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    )
    
    async with stdio_client(server_params) as (read, write):
        async with ClientSession(read, write) as session:
            await session.initialize()
            
            print("Connected to Local Python MCP Server")
            
            # List available tools
            tools = await session.list_tools()
            print(f"\nAvailable tools ({len(tools.tools)}):")
            for tool in tools.tools:
                print(f"   - {tool.name}: {tool.description}")
            print()
            
            # Create AI client and run interactive session
            client, deployment, client_type = create_azure_ai_client()
            if not client:
                print("\nSkipping interactive session - Could not create Azure AI client")
                return True
            
            # Get tools for OpenAI
            openai_tools = [
                {
                    "type": "function",
                    "function": {
                        "name": tool.name,
                        "description": tool.description,
                        "parameters": tool.inputSchema
                    }
                }
                for tool in tools.tools
            ]
            
            # Run interactive session
            return await run_interactive_session(client, deployment, client_type, session, openai_tools, "Local Python MCP")


async def demo_remote_mcp() -> bool:
    """Demo: Connect to remote MCP server via HTTP/SSE with interactive session."""
    print("\n" + "=" * 60)
    print("      Demo 2: Remote MCP Bridge (HTTP/SSE -> REST API)")
    print("=" * 60)
    print()
    print("Architecture:")
    print("   AgentClient -> MCP Bridge (:5070) -> REST API (:5060)")
    print()
    
    url = "http://localhost:5070/sse"
    print(f"Connecting to MCP Bridge at {url}...")
    print("   (Make sure both REST API :5060 and MCP Bridge :5070 are running)")
    
    try:
        async with sse_client(url) as (read, write):
            async with ClientSession(read, write) as session:
                await session.initialize()
                
                print("Connected to MCP Bridge")
                
                # List available tools
                tools = await session.list_tools()
                print(f"\nAvailable tools ({len(tools.tools)}):")
                for tool in tools.tools:
                    print(f"   - {tool.name}: {tool.description}")
                print()
                
                # Create AI client and run interactive session
                client, deployment, client_type = create_azure_ai_client()
                if not client:
                    print("\nSkipping interactive session - Could not create Azure AI client")
                    return True
                
                # Get tools for OpenAI
                openai_tools = [
                    {
                        "type": "function",
                        "function": {
                            "name": tool.name,
                            "description": tool.description,
                            "parameters": tool.inputSchema
                        }
                    }
                    for tool in tools.tools
                ]
                
                # Run interactive session
                return await run_interactive_session(client, deployment, client_type, session, openai_tools, "Remote MCP Bridge")
    except Exception as e:
        print_colored(f"\nError: {e}", Colors.RED)
        print("Make sure the MCP Bridge (port 5070) and REST API (port 5060) are running.")
        return True


def create_azure_ai_client():
    """
    Create Azure AI client with appropriate authentication.
    Supports: API Key, Service Principal (ClientSecretCredential), and Azure CLI fallback.
    Configuration priority: AZURE_OPENAI_* > AZURE_AI_PROJECT_* > Azure CLI
    """
    # Get Azure OpenAI configuration (prioritize AZURE_OPENAI_* first)
    openai_endpoint = os.environ.get("AZURE_OPENAI_ENDPOINT")
    openai_api_key = os.environ.get("AZURE_OPENAI_API_KEY")
    openai_deployment = os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME")
    
    # Fallback to AI Project configuration
    project_endpoint = os.environ.get("AZURE_AI_PROJECT_ENDPOINT")
    project_deployment = os.environ.get("AZURE_AI_MODEL_DEPLOYMENT_NAME")
    
    # Determine which endpoint and deployment to use
    deployment = openai_deployment or project_deployment or "gpt-4o-mini"
    
    # Get service principal credentials
    tenant_id = os.environ.get("AZURE_TENANT_ID")
    client_id = os.environ.get("AZURE_CLIENT_ID")
    client_secret = os.environ.get("AZURE_CLIENT_SECRET")
    
    # Option 1: Azure OpenAI with API Key (highest priority)
    if openai_endpoint and openai_api_key:
        print("Authentication: API Key (Azure OpenAI)")
        client = AzureOpenAI(
            azure_endpoint=openai_endpoint,
            api_key=openai_api_key,
            api_version="2024-02-15-preview"
        )
        return client, deployment, "openai"
    
    # Option 2: Azure OpenAI with Service Principal
    if openai_endpoint and tenant_id and client_id and client_secret:
        print("Authentication: Service Principal (Azure OpenAI)")
        credential = ClientSecretCredential(
            tenant_id=tenant_id,
            client_id=client_id,
            client_secret=client_secret
        )
        token = credential.get_token("https://cognitiveservices.azure.com/.default")
        client = AzureOpenAI(
            azure_endpoint=openai_endpoint,
            api_key=token.token,
            api_version="2024-02-15-preview"
        )
        return client, deployment, "openai"
    
    # Option 3: AI Project with Service Principal
    if project_endpoint and tenant_id and client_id and client_secret:
        print("Authentication: Service Principal (AIProjectClient)")
        credential = ClientSecretCredential(
            tenant_id=tenant_id,
            client_id=client_id,
            client_secret=client_secret
        )
        client = AIProjectClient(
            endpoint=project_endpoint,
            credential=credential
        )
        return client, deployment, "project"
    
    # Option 4: AI Project with Azure CLI (fallback for local development)
    if project_endpoint:
        print("Authentication: Azure CLI (AIProjectClient)")
        credential = AzureCliCredential()
        client = AIProjectClient(
            endpoint=project_endpoint,
            credential=credential
        )
        return client, deployment, "project"
    
    return None, deployment, None


async def run_interactive_session(client, deployment: str, client_type: str, mcp_session: ClientSession, openai_tools: list, server_name: str) -> bool:
    """
    Run an interactive session with the agent.
    Returns True to exit application, False to return to menu.
    """
    print(f"Starting interactive session with {server_name}")
    print("   Type 'back' to return to the main menu")
    print("   Type 'exit' or 'quit' to exit the application")
    print("   Example prompts:")
    print("   - Get all configurations")
    print("   - What is the value of app.name?")
    print("   - Update feature.darkMode to true")
    print()
    
    # Get the appropriate chat client based on client type
    if client_type == "openai":
        # AzureOpenAI client - use chat.completions directly
        chat_client = client.chat.completions
    else:
        # AIProjectClient - get chat completions client from inference
        chat_client = client.inference.get_chat_completions_client()
    
    # Conversation history
    messages = [
        {"role": "system", "content": "You are a configuration management assistant. Help users get and update configurations using the available MCP tools."}
    ]
    
    while True:
        print_colored("You: ", Colors.CYAN, end="")
        user_input = input().strip()
        
        if not user_input:
            continue
        
        if user_input.lower() == "back":
            print("Returning to main menu...")
            return False  # Return to menu (don't exit app)
        
        if user_input.lower() in ("exit", "quit"):
            print("Exiting application...")
            return True  # Exit application
        
        try:
            print()
            
            # Add user message to conversation
            messages.append({"role": "user", "content": user_input})
            
            # Get response from AI (different method based on client type)
            if client_type == "openai":
                response = chat_client.create(
                    model=deployment,
                    messages=messages,
                    tools=openai_tools
                )
            else:
                response = chat_client.complete(
                    model=deployment,
                    messages=messages,
                    tools=openai_tools
                )
            
            assistant_message = response.choices[0].message
            
            # Handle tool calls
            if assistant_message.tool_calls:
                # Add assistant message with tool calls
                messages.append({
                    "role": "assistant",
                    "content": assistant_message.content or "",
                    "tool_calls": [
                        {
                            "id": tc.id,
                            "type": "function",
                            "function": {
                                "name": tc.function.name,
                                "arguments": tc.function.arguments
                            }
                        }
                        for tc in assistant_message.tool_calls
                    ]
                })
                
                # Process each tool call
                tool_results = []
                for tool_call in assistant_message.tool_calls:
                    args = json.loads(tool_call.function.arguments)
                    result = await mcp_session.call_tool(tool_call.function.name, args)
                    tool_result = result.content[0].text
                    tool_results.append({
                        "role": "tool",
                        "tool_call_id": tool_call.id,
                        "content": tool_result
                    })
                
                # Add tool results to messages
                messages.extend(tool_results)
                
                # Get final response after tool calls
                if client_type == "openai":
                    final_response = chat_client.create(
                        model=deployment,
                        messages=messages
                    )
                else:
                    final_response = chat_client.complete(
                        model=deployment,
                        messages=messages
                    )
                
                final_content = final_response.choices[0].message.content
                messages.append({"role": "assistant", "content": final_content})
                
                print_colored("Agent: ", Colors.GREEN, end="")
                print(final_content)
            else:
                # No tool calls, just text response
                content = assistant_message.content
                messages.append({"role": "assistant", "content": content})
                
                print_colored("Agent: ", Colors.GREEN, end="")
                print(content)
            
            print()
            
        except Exception as e:
            print_colored(f"Error: {e}", Colors.RED)


async def main():
    """Run demos with menu selection (like .NET Program.cs)."""
    print("=" * 60)
    print("           MCP Workshop - Agent Client Demo")
    print("      Demonstrating Local and Remote MCP Servers")
    print("=" * 60)
    print()
    
    # Validate environment configuration
    if not validate_env_config():
        print("\n⚠ Environment validation failed. Some features may not work.")
    
    print()
    
    # Menu for demo selection
    running = True
    while running:
        print("=" * 60)
        print("Select a demo to run:")
        print("  1. Local MCP Server (Python via STDIO)")
        print("  2. Remote MCP Server (HTTP/SSE)")
        print("  3. Exit")
        print("=" * 60)
        
        choice = input("Enter choice (1-3): ").strip()
        print()
        
        exit_app = False
        
        try:
            if choice == "1":
                exit_app = await demo_local_mcp()
            elif choice == "2":
                exit_app = await demo_remote_mcp()
            elif choice == "3":
                running = False
                continue
            else:
                print("Invalid choice. Please enter 1-3.")
                continue
        except Exception as e:
            print_colored(f"Error: {e}", Colors.RED)
            print()
            input("Press any key to continue...")
            # Clear and continue
            print("\n" * 2)
            continue
        
        # Exit application if user typed 'exit' or 'quit' in the session
        if exit_app:
            running = False
            continue
        
        # Clear screen for next iteration
        print("\n" * 2)
    
    print("Goodbye!")


if __name__ == "__main__":
    asyncio.run(main())
