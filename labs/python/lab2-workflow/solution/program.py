"""
Workflow Lab - Main Entry Point

This lab demonstrates three key workflow patterns using Python with Azure OpenAI:

1. Sequential Workflow: Process tickets through a linear pipeline
   - Ticket Intake -> AI Categorization -> AI Response Generation

2. Concurrent Workflow: Fan-out to multiple agents simultaneously
   - Question -> [Billing Expert + Technical Expert] -> Combined Response

3. Human-in-the-Loop Workflow: AI assistance with human oversight
   - Ticket -> AI Draft -> [Human Review/Approval] -> Final Response

All demos use a Customer Support Ticket System as the example scenario.
"""

import asyncio
import sys
import os
import json
from pathlib import Path


def _find_python_folder() -> Path:
    """Traverse up from the current file to find the 'python' folder."""
    current = Path(__file__).resolve().parent
    while current != current.parent:
        if current.name.lower() == "python":
            return current
        current = current.parent
    # Fallback to script's directory
    return Path(__file__).resolve().parent


def load_env_from_root():
    """Load environment variables from .env file in the python folder."""
    python_dir = _find_python_folder()
    env_file = python_dir / ".env"
    
    if env_file.exists():
        print(f"Loading configuration from: {env_file}")
        with open(env_file, 'r') as f:
            content = f.read().strip()
            
        # Try to parse as JSON (the .env file is in JSON format)
        try:
            env_vars = json.loads(content)
            for key, value in env_vars.items():
                os.environ[key] = str(value)
            print(f"Loaded {len(env_vars)} environment variables:")
            print("-" * 50)
            for key, value in env_vars.items():
                # Mask sensitive values (API keys, secrets)
                if any(sensitive in key.upper() for sensitive in ['KEY', 'SECRET', 'PASSWORD', 'TOKEN']):
                    masked_value = value[:4] + '***' + value[-4:] if len(value) > 8 else '***'
                else:
                    masked_value = value
                print(f"  {key}: {masked_value}")
            print("-" * 50)
        except json.JSONDecodeError:
            # Fallback: parse as standard KEY=VALUE format
            for line in content.splitlines():
                line = line.strip()
                if line and not line.startswith('#') and '=' in line:
                    key, value = line.split('=', 1)
                    os.environ[key.strip()] = value.strip().strip('"\'')
    else:
        print(f"Warning: .env file not found at {env_file}")


# Load environment variables before importing modules that need them
load_env_from_root()

from sequential import SequentialWorkflowDemo
from concurrent_workflow import ConcurrentWorkflowDemo
from human_in_the_loop import HumanInTheLoopWorkflowDemo


def print_header():
    """Print the application header and validate configuration."""
    print("=====================================================================")
    print("                        WORKFLOW LAB                                 ")
    print("              Python AI Workflow Patterns                            ")
    print("=====================================================================")
    print()
    print("This lab demonstrates three workflow patterns using a")
    print("Customer Support Ticket System as the example scenario.")
    print()

    # Validate and display configuration
    endpoint = os.environ.get("AZURE_OPENAI_ENDPOINT", "")
    deployment = os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME") or \
                 os.environ.get("AZURE_AI_MODEL_DEPLOYMENT_NAME") or "gpt-4o-mini"
    api_key = os.environ.get("AZURE_OPENAI_API_KEY", "")
    tenant_id = os.environ.get("AZURE_TENANT_ID", "")
    client_id = os.environ.get("AZURE_CLIENT_ID", "")
    client_secret = os.environ.get("AZURE_CLIENT_SECRET", "")

    def mask(val):
        if not val:
            return None
        return val[:4] + "****" + val[-4:] if len(val) > 8 else "****"

    print("Configuration (from .env):")
    print(f"  AZURE_OPENAI_ENDPOINT:         {'❌ NOT SET' if not endpoint else f'✅ {endpoint}'}")
    print(f"  AZURE_OPENAI_DEPLOYMENT_NAME:  ✅ {deployment}")
    print(f"  AZURE_OPENAI_API_KEY:          {'⚠️  not set' if not api_key else f'✅ {mask(api_key)}'}")
    print(f"  AZURE_TENANT_ID:               {'⚠️  not set' if not tenant_id else f'✅ {tenant_id}'}")
    print(f"  AZURE_CLIENT_ID:               {'⚠️  not set' if not client_id else f'✅ {client_id}'}")
    print(f"  AZURE_CLIENT_SECRET:           {'⚠️  not set' if not client_secret else '✅ ********'}")
    print()

    if not endpoint:
        print("ERROR: AZURE_OPENAI_ENDPOINT is required but not configured.")
        print("Please set it in the .env file under the python folder.")
        sys.exit(1)

    if not api_key and not (tenant_id and client_id and client_secret):
        print("WARNING: No API Key or Service Principal configured. Will fall back to DefaultAzureCredential.")
    print()
    print("=====================================================================")
    print()


def print_menu():
    """Print the menu options."""
    print("Select a workflow demo to run:")
    print()
    print("  [1] Sequential Workflow")
    print("      Process tickets through a linear AI pipeline")
    print("      (Intake -> Categorization -> Response)")
    print()
    print("  [2] Concurrent Workflow")
    print("      Fan-out questions to multiple specialist agents")
    print("      (Question -> [Billing + Technical Experts] -> Combined)")
    print()
    print("  [3] Human-in-the-Loop Workflow")
    print("      AI-assisted responses with human supervisor review")
    print("      (Ticket -> AI Draft -> Human Review -> Final Response)")
    print()
    print("  [Q] Exit")
    print()


async def run_demo(choice: str) -> bool:
    """
    Run the selected demo.
    
    Returns:
        True if should continue, False if should exit.
    """
    try:
        if choice == "1":
            await SequentialWorkflowDemo.run_async()
        elif choice == "2":
            await ConcurrentWorkflowDemo.run_async()
        elif choice == "3":
            await HumanInTheLoopWorkflowDemo.run_async()
        elif choice.upper() == "Q":
            print("Thank you for using Workflow Lab. Goodbye!")
            return False
        else:
            print("Invalid choice. Please enter 1, 2, 3, or Q.")
    except Exception as e:
        print(f"\nError running demo: {e}")
        print("Please check your Azure OpenAI configuration and try again.")
    
    return True


async def main():
    """Main entry point."""
    print_header()
    print_menu()
    
    while True:
        choice = input("Enter your choice (1-3 or Q): ").strip()
        print()
        
        should_continue = await run_demo(choice)
        
        if not should_continue:
            break
        
        print()
        print("=" * 69)
        print()
        print_menu()


if __name__ == "__main__":
    asyncio.run(main())
