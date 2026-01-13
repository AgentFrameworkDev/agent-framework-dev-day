"""
Foundry Client Factory for Python

Factory for creating Microsoft Foundry clients with automatic configuration discovery.
Searches parent directories for .env files and supports environment variable configuration.

This is the Python equivalent of FoundryClientFactory.cs but uses:
- .env files instead of appsettings.Local.json
- Agent Framework's idiomatic patterns
"""

from dataclasses import dataclass
from pathlib import Path
from typing import TYPE_CHECKING

from dotenv import load_dotenv
import os

if TYPE_CHECKING:
    from azure.core.credentials_async import AsyncTokenCredential
    from azure.ai.projects.aio import AIProjectClient


@dataclass
class FoundryClientConfiguration:
    """Configuration details for creating Microsoft Foundry clients."""

    endpoint: str
    deployment_name: str
    credential: "AsyncTokenCredential"
    tenant_id: str | None = None
    client_id: str | None = None
    client_secret: str | None = None

    @property
    def is_service_principal(self) -> bool:
        """Check if using service principal authentication."""
        return all([self.tenant_id, self.client_id, self.client_secret])


def find_and_load_env(filename: str = ".env") -> Path:
    """
    Walk up the directory tree to find and load a .env file.

    Args:
        filename: The name of the env file to search for. Defaults to ".env".

    Returns:
        Path to the found .env file.

    Raises:
        FileNotFoundError: If no .env file is found in the directory tree.
    """
    here = Path(__file__).resolve()
    for directory in [here.parent, *here.parents]:
        candidate = directory / filename
        if candidate.is_file():
            load_dotenv(candidate, override=False)
            return candidate
    raise FileNotFoundError(f"Could not find {filename} by walking up from {here}")


def get_configuration(env_filename: str = ".env") -> FoundryClientConfiguration:
    """
    Load configuration from .env (searching parent directories) and environment variables.
    Returns details needed to create any Foundry client.

    Args:
        env_filename: The configuration file name to search for. Defaults to ".env".

    Returns:
        Configuration containing endpoint, deployment name, and credential.

    Raises:
        ValueError: When required configuration values are missing.
    """
    from azure.identity.aio import ClientSecretCredential, DefaultAzureCredential

    # Find and load the .env file
    find_and_load_env(env_filename)

    # Get required configuration
    endpoint = os.environ.get("AZURE_AI_PROJECT_ENDPOINT")
    if not endpoint:
        raise ValueError(
            "Azure AI endpoint not configured. Set AZURE_AI_PROJECT_ENDPOINT in .env or environment."
        )

    deployment_name = os.environ.get("AZURE_AI_MODEL_DEPLOYMENT_NAME")
    if not deployment_name:
        raise ValueError(
            "Azure AI deployment not configured. Set AZURE_AI_MODEL_DEPLOYMENT_NAME in .env or environment."
        )

    # Get optional service principal credentials
    tenant_id = os.environ.get("AZURE_TENANT_ID")
    client_id = os.environ.get("AZURE_CLIENT_ID")
    client_secret = os.environ.get("AZURE_CLIENT_SECRET")

    # Create appropriate credential
    if tenant_id and client_id and client_secret:
        credential = ClientSecretCredential(
            tenant_id=tenant_id,
            client_id=client_id,
            client_secret=client_secret,
        )
    else:
        credential = DefaultAzureCredential()

    return FoundryClientConfiguration(
        endpoint=endpoint,
        deployment_name=deployment_name,
        credential=credential,
        tenant_id=tenant_id,
        client_id=client_id,
        client_secret=client_secret,
    )


def create_project_client(
    config: FoundryClientConfiguration | None = None,
) -> "AIProjectClient":
    """
    Create an AIProjectClient using the provided configuration or by loading configuration automatically.

    Args:
        config: Optional configuration. If None, calls get_configuration() to load automatically.

    Returns:
        A configured AIProjectClient ready for use.
    """
    from azure.ai.projects.aio import AIProjectClient

    if config is None:
        config = get_configuration()

    return AIProjectClient(
        endpoint=config.endpoint,
        credential=config.credential,
    )


async def list_available_deployments(
    config: FoundryClientConfiguration | None = None,
) -> list[dict[str, str]]:
    """
    List available model deployments in the Azure AI Foundry project.
    Uses the data plane API (like C# version), not management plane.

    Args:
        config: Optional configuration. If None, calls get_configuration() to load automatically.

    Returns:
        List of deployment info dictionaries with 'name' and 'model' keys.
    """
    if config is None:
        config = get_configuration()

    client = create_project_client(config)
    try:
        deployments = []
        async for deployment in client.deployments.list():
            # ModelDeployment has name and model_name attributes
            deployments.append({
                "name": deployment.name,
                "model": getattr(deployment, "model_name", "N/A"),
            })
        return deployments
    finally:
        await client.close()
        # Also close the credential if it supports it
        if hasattr(config.credential, "close"):
            await config.credential.close()


async def get_random_deployment(
    config: FoundryClientConfiguration | None = None,
    chat_models_only: bool = False,
) -> tuple[str, str]:
    """
    Randomly select and return one deployment name and corresponding model name.

    Args:
        config: Optional configuration. If None, calls get_configuration() to load automatically.
        chat_models_only: If True, filter out embedding models. Defaults to True.

    Returns:
        Tuple of (deployment_name, model_name).

    Raises:
        ValueError: If no deployments are available.
    """
    import random

    deployments = await list_available_deployments(config)

    if chat_models_only:
        # Filter out embedding models (they can't be used for chat)
        deployments = [
            d for d in deployments
            if not d["model"].lower().startswith("text-embedding")
        ]

    if not deployments:
        raise ValueError("No model deployments available in the Azure AI Foundry project.")

    selected = random.choice(deployments)
    return selected["name"], selected["model"]
