"""
Hello World Agent Framework Application (Python version)

For authentication, run `az login` command in terminal or set up service principal
credentials via environment variables:
    AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET
"""

import asyncio
import os
from pathlib import Path

from dotenv import load_dotenv
from azure.identity.aio import DefaultAzureCredential
from agent_framework.azure import AzureAIClient


def load_root_dotenv(filename: str = ".env") -> Path:
    """Walk up the directory tree to find and load a .env file."""
    here = Path(__file__).resolve()
    for d in [here.parent, *here.parents]:
        candidate = d / filename
        if candidate.is_file():
            load_dotenv(candidate, override=False)
            return candidate
    raise FileNotFoundError(f"Could not find {filename} by walking up from {here}")


async def main() -> None:
    # Load environment variables from .env file
    env_path = load_root_dotenv()
    print(f"Loaded environment from: {env_path}")

    # Get configuration from environment variables
    # Uses official Agent Framework env var naming convention
    endpoint = os.environ.get("AZURE_AI_PROJECT_ENDPOINT")
    if not endpoint:
        raise ValueError("AZURE_AI_PROJECT_ENDPOINT environment variable is not set")

    deployment = os.environ.get("AZURE_AI_MODEL_DEPLOYMENT_NAME")
    if not deployment:
        raise ValueError("AZURE_AI_MODEL_DEPLOYMENT_NAME environment variable is not set")

    print("Hello World Agent Framework Application")
    print("========================================")
    print(f"Endpoint: {endpoint}")
    print(f"Deployment: {deployment}")
    print()

    # DefaultAzureCredential tries multiple authentication methods in order:
    # 1. EnvironmentCredential (service principal via AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET)
    # 2. WorkloadIdentityCredential (for Azure workloads)
    # 3. ManagedIdentityCredential (for Azure-hosted apps)
    # 4. AzureCliCredential (az login)
    # 5. AzureDeveloperCliCredential (azd login)
    # 6. AzurePowerShellCredential
    # 7. InteractiveBrowserCredential (as fallback)
    async with (
        DefaultAzureCredential() as credential,
        AzureAIClient(credential=credential).create_agent(
            name="HelloWorldAgent",
            instructions="You are a friendly assistant that gives concise responses.",
        ) as agent,
    ):
        print(f"Agent 'HelloWorldAgent' created successfully!")
        print()

        # Send a simple prompt and get response
        prompt = "Tell me one a joke or fact about .NET or Python!"
        print(f"User: {prompt}")
        print()

        # Run the agent (non-streaming to get token usage)
        response = await agent.run(prompt)

        print(f"Agent: {response.text}")
        print()

        # Display token usage information
        print("========================================")
        print("Token Usage:")
        if hasattr(response, 'usage_details') and response.usage_details:
            print(f"  Input Tokens:  {response.usage_details.input_token_count}")
            print(f"  Output Tokens: {response.usage_details.output_token_count}")
            print(f"  Total Tokens:  {response.usage_details.total_token_count}")
        else:
            print("  Token usage information not available")

    # Agent cleanup is handled automatically by the async context manager
    print()
    print("Agent cleaned up successfully!")


if __name__ == "__main__":
    asyncio.run(main())
