"""
Hello World Agent Framework Application (Python version)

For authentication, run `az login` command in terminal or set up service principal
credentials via environment variables:
    AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET
"""

import asyncio
import time

from agent_framework.azure import AzureAIClient
from foundry_client_factory import get_configuration, get_random_deployment


async def main() -> None:
    # Load configuration from .env up above
    config = get_configuration()

    print("Hello World Agent Framework Application")
    print("========================================")
    print(f"Endpoint: {config.endpoint}")
    print(f"Deployment: {config.deployment_name}")
    print(f"Auth: {'Service Principal' if config.is_service_principal else 'Default Azure Credential'}")
    print()

    # Randomly select a model from available deployments (like C# version)
    # Note: This creates its own credential internally
    selected_model, underlying_model_name = await get_random_deployment()
    print(f"Randomly selected model: {selected_model} ({underlying_model_name})")
    print()

    # Get a fresh credential for the main client (previous one was closed)
    fresh_config = get_configuration()

    # AzureAIClient uses the Agent Framework pattern:
    # - Uses the credential we provide (service principal or DefaultAzureCredential)
    # - Uses the randomly selected model deployment
    async with (
        fresh_config.credential,
        AzureAIClient(
            project_endpoint=fresh_config.endpoint,
            model_deployment_name=selected_model,  # Use randomly selected model
            credential=fresh_config.credential,
        ).create_agent(
            name="HelloWorldAgent",
            instructions="You are a friendly assistant that gives concise responses.",
        ) as agent,
    ):
        print(f"Agent 'HelloWorldAgent' created successfully! using model '{selected_model}'")
        print()

        # Send a simple prompt and get response
        prompt = "Tell me one a joke OR interesting fact about .NET or Python!"
        prompt = "In just one or two words, capture the vibe of Python developers..."
        print(f"User: {prompt}")
        print()

        # Run the agent (non-streaming to get token usage)
        start_time = time.perf_counter()
        response = await agent.run(prompt)
        elapsed_seconds = time.perf_counter() - start_time

        print(f"Agent: {response.text}")
        print()

        # Display token usage information (matching C# format)
        print("========================================")
        print("Token Usage:")
        if hasattr(response, 'usage_details') and response.usage_details:
            usage = response.usage_details
            input_tokens = getattr(usage, 'input_token_count', 0) or 0
            output_tokens = getattr(usage, 'output_token_count', 0) or 0
            total_tokens = getattr(usage, 'total_token_count', 0) or 0

            # Reasoning tokens are in additional_counts with key 'openai.reasoning_tokens'
            reasoning_tokens = 0
            if hasattr(usage, 'additional_counts') and usage.additional_counts:
                reasoning_tokens = usage.additional_counts.get('openai.reasoning_tokens', 0)

            print(f"  {input_tokens:6} Input Tokens")
            print(f"+ {output_tokens:6} Output Tokens (including {reasoning_tokens} Reasoning Tokens)")
            print(f"= {total_tokens:6} Total Tokens")
        else:
            print("  Token usage information not available")

        print()
        print("Models Used:")
        print("========================================")
        print(f"The model used by the agent was: {selected_model} ({underlying_model_name}) and took {elapsed_seconds:.2f} seconds.")

    # Agent cleanup is handled automatically by the async context manager
    print()
    print("Agent cleaned up successfully!")


if __name__ == "__main__":
    asyncio.run(main())
