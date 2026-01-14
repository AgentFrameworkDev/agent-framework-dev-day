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

from workflow_lab.sequential import SequentialWorkflowDemo
from workflow_lab.concurrent import ConcurrentWorkflowDemo
from workflow_lab.human_in_the_loop import HumanInTheLoopWorkflowDemo


def print_header():
    """Print the application header."""
    print("=====================================================================")
    print("                        WORKFLOW LAB                                 ")
    print("              Python AI Workflow Patterns                            ")
    print("=====================================================================")
    print()
    print("This lab demonstrates three workflow patterns using a")
    print("Customer Support Ticket System as the example scenario.")
    print()
    print("Environment Variables Required:")
    print("  - AZURE_OPENAI_ENDPOINT (required)")
    print("  - AZURE_OPENAI_DEPLOYMENT_NAME (optional, default: gpt-4o-mini)")
    print("  - Authentication (one of the following):")
    print("    - AZURE_OPENAI_API_KEY (API Key auth)")
    print("    - AZURE_TENANT_ID + AZURE_CLIENT_ID + AZURE_CLIENT_SECRET (Service Principal)")
    print("    - None (uses DefaultAzureCredential/Managed Identity)")
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
