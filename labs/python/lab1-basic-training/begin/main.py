"""
Hello World Agent Framework Application (Python version)

Multi-model comparison tool that runs the same prompt against all available
chat models and displays a comparison table with metrics.

For authentication, run `az login` command in terminal or set up service principal
credentials via environment variables:
    AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET
"""

import asyncio
import sys
import time
from dataclasses import dataclass

from agent_framework.azure import AzureAIClient
from foundry_client_factory import get_configuration, get_chat_completion_models
from configure_lab_keys import ConfigureLabKeys


@dataclass
class ModelResult:
    """Results from running a prompt against a single model."""
    model_name: str
    underlying_model: str
    prompt_tokens: int = 0
    completion_tokens: int = 0
    reasoning_tokens: int = 0
    total_tokens: int = 0
    response_time_seconds: float = 0.0
    response_text: str = ""
    success: bool = True
    error_message: str = ""


async def run_model(
    config,
    deployment_name: str,
    underlying_model: str,
    prompt: str,
) -> ModelResult:
    """
    Run a prompt against a single model and collect metrics.

    Uses async context manager for automatic agent cleanup.
    """
    result = ModelResult(
        model_name=deployment_name,
        underlying_model=underlying_model,
    )

    try:
        async with (
            AzureAIClient(
                project_endpoint=config.endpoint,
                model_deployment_name=deployment_name,
                credential=config.credential,
            ).create_agent(
                name="HelloWorldAgent",
                instructions="You are a friendly assistant that gives concise responses.",
            ) as agent,
        ):
            start_time = time.perf_counter()
            response = await agent.run(prompt)
            result.response_time_seconds = time.perf_counter() - start_time

            result.response_text = response.text

            # Extract token usage
            if hasattr(response, 'usage_details') and response.usage_details:
                usage = response.usage_details
                result.prompt_tokens = getattr(usage, 'input_token_count', 0) or 0
                result.completion_tokens = getattr(usage, 'output_token_count', 0) or 0
                result.total_tokens = getattr(usage, 'total_token_count', 0) or 0

                # Reasoning tokens are in additional_counts
                if hasattr(usage, 'additional_counts') and usage.additional_counts:
                    result.reasoning_tokens = usage.additional_counts.get('openai.reasoning_tokens', 0)

    except Exception as e:
        result.success = False
        result.error_message = str(e)

    return result


def print_table_header() -> None:
    """Print the table header row."""
    print()
    print(f"{'MODEL':<20} {'IN':>6} {'OUT':>6} {'(REAS)':>6} {'TOTAL':>7} {'SECS':>7}  RESPONSE")
    print("=" * 100)


def print_table_row(r: ModelResult) -> None:
    """Print a single result row in the comparison table."""
    if r.success:
        # Truncate response to fit nicely
        response_preview = r.response_text.replace('\n', ' ').strip()
        print(
            f"{r.model_name:<20} "
            f"{r.prompt_tokens:>6} "
            f"{r.completion_tokens:>6} "
            f"{r.reasoning_tokens:>6} "
            f"{r.total_tokens:>7} "
            f"{r.response_time_seconds:>7.2f}  "
            f"{response_preview}"
        )
    else:
        print(
            f"{r.model_name:<20} "
            f"{'--':>6} "
            f"{'--':>6} "
            f"{'--':>6} "
            f"{'--':>7} "
            f"{'--':>7}  "
            f"ERROR: {r.error_message[:40]}"
        )


def print_table_footer() -> None:
    """Print the table footer with legend."""
    print()
    print("Legend: IN=Input Tokens, OUT=Output Tokens, (REAS)=Reasoning Tokens, TOTAL=Total Tokens, SECS=Runtime")


async def main() -> None:
    # Parse command-line arguments
    verbose = "--verbose" in sys.argv or "-v" in sys.argv
    force = "--force" in sys.argv or "--overwrite" in sys.argv

    # --------- FIRST STEP ----------
    # ASK LAB INSTRUCTOR FOR THE PASSWORD
    # password = "\U0001d49c\U0001d4ae\U0001d4a6 \U0001d4b4\U0001d4aa\U0001d4b0\u211b \u2112\U0001d49c\u212c \u2110\U0001d4a9\U0001d4ae\U0001d4af\u211b\U0001d4b0\U0001d49e\U0001d4af\U0001d4aa\u211b \u2131\U0001d4aa\u211b \U0001d4af\u210b\u2130 \U0001d4ab\U0001d49c\U0001d4ae\U0001d4ae\U0001d4b2\U0001d4aa\u211b\U0001d49f"
    password = "𝒜𝒮𝒦 𝒴𝒪𝒰ℛ ℒ𝒜ℬ ℐ𝒩𝒮𝒯ℛ𝒰𝒞𝒯𝒪ℛ ℱ𝒪ℛ 𝒯ℋℰ 𝒫𝒜𝒮𝒮𝒲𝒪ℛ𝒟"

    # LAB STEP 1: CHANGE THE PASSWORD
    # password = "replace this with the real password given by your lab instructor"

    # One-time configuration: decrypt settings and create .env
    configger = ConfigureLabKeys(password, verbose)
    configger.randomize_decrypt_distribute(overwrite_existing=force)

    # Load configuration from .env up above
    config = get_configuration()

    print("Welcome to Agent Framework Dev Day!")
    print("=" * 40)
    print(f"Endpoint: {config.endpoint}")
    print(f"Auth: {'Service Principal' if config.is_service_principal else 'Default Azure Credential'}")
    print()

    # Get all chat-capable models
    chat_models = await get_chat_completion_models(config)

    if not chat_models:
        print("No chat-capable models found in the Azure AI Foundry project.")
        return

    # The prompt to run against all models
    prompt = "Summarize the vibe of programming in Python in 2026 in no more than 3 words"
    print(f'Prompt: "{prompt}"')
    print()
    print(f"Found {len(chat_models)} chat-capable model(s). Running comparison...")

    # Print table header
    print_table_header()

    # Run the prompt against each model and print results as we go
    for i, model in enumerate(chat_models, 1):
        deployment_name = model["name"]
        underlying_model = model["model"]

        if verbose:
            print(f"[{i}/{len(chat_models)}] Testing {deployment_name}...", end="", flush=True)

        result = await run_model(config, deployment_name, underlying_model, prompt)

        if verbose:
            print(f" done ({result.response_time_seconds:.2f}s)")

        # Print the row immediately
        print_table_row(result)

    # Print legend
    print_table_footer()

    # Close the credential to prevent unclosed session warnings
    await config.credential.close()


if __name__ == "__main__":
    asyncio.run(main())
