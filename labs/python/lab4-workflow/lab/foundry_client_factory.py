"""
Foundry Client Factory for Python

Factory for creating Microsoft Foundry clients with automatic configuration discovery.
Searches parent directories for .env files and supports environment variable configuration.

This is the Python equivalent of FoundryClientFactory.cs and provides:
- Directory tree walk to find .env (like C# finds appsettings.Local.json)
- Service principal authentication with fallback to DefaultAzureCredential
- Factory methods for AIProjectClient, AzureOpenAI clients, and embeddings
- Azure Search configuration support for RAG scenarios
- Both sync and async credential support

Environment Variables:
    Required:
        AZURE_AI_PROJECT_ENDPOINT - Azure AI Foundry project endpoint
        AZURE_AI_MODEL_DEPLOYMENT_NAME - Default chat model deployment name

    Optional (Service Principal auth):
        AZURE_TENANT_ID - Azure tenant ID
        AZURE_CLIENT_ID - Service principal client ID
        AZURE_CLIENT_SECRET - Service principal client secret

    Optional (Embeddings):
        AZURE_AI_EMBEDDING_DEPLOYMENT_NAME - Embedding model deployment name

    Optional (Azure Search for RAG):
        AZURE_SEARCH_ENDPOINT - Azure AI Search endpoint
        AZURE_SEARCH_API_KEY - Azure AI Search API key
        AZURE_SEARCH_INDEX_NAME - Azure AI Search index name
"""

from dataclasses import dataclass, field
from pathlib import Path
from typing import TYPE_CHECKING
import os

from dotenv import load_dotenv

if TYPE_CHECKING:
    from azure.core.credentials import TokenCredential
    from azure.core.credentials_async import AsyncTokenCredential
    from azure.ai.projects import AIProjectClient
    from azure.ai.projects.aio import AIProjectClient as AsyncAIProjectClient
    from openai import AzureOpenAI, AsyncAzureOpenAI


# =============================================================================
# Configuration Data Classes
# =============================================================================


@dataclass
class FoundryClientConfiguration:
    """
    Configuration details for creating Microsoft Foundry clients and Azure Search.
    
    This mirrors the C# FoundryClientConfiguration record, holding all the details
    needed to create various Azure AI clients.
    """

    endpoint: str
    deployment_name: str
    credential: "TokenCredential | AsyncTokenCredential"
    embedding_deployment_name: str | None = None
    search_endpoint: str | None = None
    search_api_key: str | None = None
    search_index_name: str | None = None
    # Internal tracking for auth type
    _tenant_id: str | None = field(default=None, repr=False)
    _client_id: str | None = field(default=None, repr=False)
    _client_secret: str | None = field(default=None, repr=False)

    @property
    def is_service_principal(self) -> bool:
        """Check if using service principal authentication."""
        return all([self._tenant_id, self._client_id, self._client_secret])

    @property
    def auth_description(self) -> str:
        """Human-readable description of authentication method."""
        return "Service Principal" if self.is_service_principal else "DefaultAzureCredential"


# =============================================================================
# Configuration Discovery
# =============================================================================

_DEFAULT_ENV_FILENAME = ".env"


def find_config_directory(filename: str = _DEFAULT_ENV_FILENAME) -> Path | None:
    """
    Search current directory and parent directories for a configuration file.

    Args:
        filename: The name of the config file to search for. Defaults to ".env".

    Returns:
        Path to the directory containing the config file, or None if not found.
    """
    directory = Path.cwd()
    
    while directory != directory.parent:  # Stop at filesystem root
        if (directory / filename).is_file():
            return directory
        directory = directory.parent
    
    # Check root directory as well
    if (directory / filename).is_file():
        return directory
    
    return None


def find_and_load_env(filename: str = _DEFAULT_ENV_FILENAME) -> Path:
    """
    Walk up the directory tree to find and load a .env file.

    Args:
        filename: The name of the env file to search for. Defaults to ".env".

    Returns:
        Path to the found .env file.

    Raises:
        FileNotFoundError: If no .env file is found in the directory tree.
    """
    config_dir = find_config_directory(filename)
    if config_dir is None:
        raise FileNotFoundError(
            f"Could not find {filename} in current directory or any parent directory. "
            f"Current directory: {Path.cwd()}"
        )
    
    env_path = config_dir / filename
    load_dotenv(env_path, override=False)
    return env_path


# =============================================================================
# Credential Factory
# =============================================================================


def _create_credential_sync(
    tenant_id: str | None,
    client_id: str | None,
    client_secret: str | None,
) -> "TokenCredential":
    """Create a synchronous TokenCredential based on available configuration."""
    from azure.identity import ClientSecretCredential, DefaultAzureCredential

    if tenant_id and client_id and client_secret:
        return ClientSecretCredential(
            tenant_id=tenant_id,
            client_id=client_id,
            client_secret=client_secret,
        )
    return DefaultAzureCredential()


def _create_credential_async(
    tenant_id: str | None,
    client_id: str | None,
    client_secret: str | None,
) -> "AsyncTokenCredential":
    """Create an asynchronous TokenCredential based on available configuration."""
    from azure.identity.aio import ClientSecretCredential, DefaultAzureCredential

    if tenant_id and client_id and client_secret:
        return ClientSecretCredential(
            tenant_id=tenant_id,
            client_id=client_id,
            client_secret=client_secret,
        )
    return DefaultAzureCredential()


# =============================================================================
# Configuration Loading
# =============================================================================


def get_configuration(
    env_filename: str = _DEFAULT_ENV_FILENAME,
    *,
    use_async: bool = True,
) -> FoundryClientConfiguration:
    """
    Load configuration from .env (searching parent directories) and environment variables.
    Returns details needed to create any Foundry client.

    Args:
        env_filename: The configuration file name to search for. Defaults to ".env".
        use_async: If True, creates async credential; if False, creates sync credential.
                   Defaults to True for Agent Framework compatibility.

    Returns:
        Configuration containing endpoint, deployment name, credential, and optional
        embedding/search settings.

    Raises:
        ValueError: When required configuration values are missing.
    """
    # Find and load the .env file
    find_and_load_env(env_filename)

    # Get required configuration
    endpoint = os.environ.get("AZURE_AI_PROJECT_ENDPOINT")
    if not endpoint:
        raise ValueError(
            "Azure AI endpoint not configured. "
            "Set AZURE_AI_PROJECT_ENDPOINT in .env or environment."
        )

    deployment_name = os.environ.get("AZURE_AI_MODEL_DEPLOYMENT_NAME")
    if not deployment_name:
        raise ValueError(
            "Azure AI deployment not configured. "
            "Set AZURE_AI_MODEL_DEPLOYMENT_NAME in .env or environment."
        )

    # Get optional service principal credentials
    tenant_id = os.environ.get("AZURE_TENANT_ID")
    client_id = os.environ.get("AZURE_CLIENT_ID")
    client_secret = os.environ.get("AZURE_CLIENT_SECRET")

    # Create appropriate credential
    if use_async:
        credential = _create_credential_async(tenant_id, client_id, client_secret)
    else:
        credential = _create_credential_sync(tenant_id, client_id, client_secret)

    # Get optional embedding deployment
    embedding_deployment = os.environ.get("AZURE_AI_EMBEDDING_DEPLOYMENT_NAME")

    # Get optional Azure Search configuration (support both flat and nested keys)
    search_endpoint = (
        os.environ.get("AZURE_SEARCH_ENDPOINT") or
        os.environ.get("AZURE_AI_SEARCH_ENDPOINT")
    )
    search_api_key = (
        os.environ.get("AZURE_SEARCH_API_KEY") or
        os.environ.get("AZURE_AI_SEARCH_API_KEY")
    )
    search_index_name = (
        os.environ.get("AZURE_SEARCH_INDEX_NAME") or
        os.environ.get("AZURE_AI_SEARCH_INDEX_NAME")
    )

    return FoundryClientConfiguration(
        endpoint=endpoint,
        deployment_name=deployment_name,
        credential=credential,
        embedding_deployment_name=embedding_deployment,
        search_endpoint=search_endpoint,
        search_api_key=search_api_key,
        search_index_name=search_index_name,
        _tenant_id=tenant_id,
        _client_id=client_id,
        _client_secret=client_secret,
    )


# =============================================================================
# Client Factory Methods - Async (default for Agent Framework)
# =============================================================================


def create_project_client(
    config: FoundryClientConfiguration | None = None,
) -> "AsyncAIProjectClient":
    """
    Create an async AIProjectClient using the provided configuration or by loading
    configuration automatically.

    Args:
        config: Optional configuration. If None, calls get_configuration() to load automatically.

    Returns:
        A configured async AIProjectClient ready for use.
    """
    from azure.ai.projects.aio import AIProjectClient

    if config is None:
        config = get_configuration(use_async=True)

    return AIProjectClient(
        endpoint=config.endpoint,
        credential=config.credential,
    )


async def get_openai_endpoint(
    config: FoundryClientConfiguration | None = None,
) -> str:
    """
    Retrieve the Azure OpenAI endpoint from the Foundry Project's connections.

    Args:
        config: Optional configuration. If None, calls get_configuration() to load automatically.

    Returns:
        The Azure OpenAI endpoint URL.

    Raises:
        ValueError: When no Azure OpenAI connection is found.
    """
    from azure.ai.projects.aio import AIProjectClient

    if config is None:
        config = get_configuration(use_async=True)

    async with AIProjectClient(
        endpoint=config.endpoint,
        credential=config.credential,
    ) as client:
        # Get the Azure OpenAI connection
        connection = await client.connections.get_default(
            connection_type="AzureOpenAI",
            include_credentials=False,
        )
        
        if not connection or not connection.endpoint_url:
            raise ValueError(
                "No Azure OpenAI connection found in the Foundry Project. "
                "Ensure an Azure OpenAI connection is configured in your Azure AI Foundry project."
            )
        
        return connection.endpoint_url


async def create_openai_client_async(
    config: FoundryClientConfiguration | None = None,
) -> "AsyncAzureOpenAI":
    """
    Create an async AzureOpenAI client using Foundry credentials.
    Automatically discovers the Azure OpenAI endpoint from the project's connections.

    Args:
        config: Optional configuration. If None, calls get_configuration() to load automatically.

    Returns:
        A configured AsyncAzureOpenAI client ready for use.
    """
    from openai import AsyncAzureOpenAI

    if config is None:
        config = get_configuration(use_async=True)

    openai_endpoint = await get_openai_endpoint(config)
    
    # Get a token from the credential
    token = await config.credential.get_token("https://cognitiveservices.azure.com/.default")
    
    return AsyncAzureOpenAI(
        azure_endpoint=openai_endpoint,
        api_key=token.token,
        api_version="2024-06-01",
    )


async def create_chat_client_async(
    config: FoundryClientConfiguration | None = None,
    deployment_name: str | None = None,
):
    """
    Create an async chat completion interface for the configured deployment.

    Args:
        config: Optional configuration. If None, calls get_configuration() to load automatically.
        deployment_name: Override for deployment name. If None, uses config.deployment_name.

    Returns:
        The AsyncAzureOpenAI client and deployment name tuple for chat completions.
    """
    if config is None:
        config = get_configuration(use_async=True)
    
    client = await create_openai_client_async(config)
    model = deployment_name or config.deployment_name
    
    return client, model


# =============================================================================
# Client Factory Methods - Sync (for scripts and non-async contexts)
# =============================================================================


def create_project_client_sync(
    config: FoundryClientConfiguration | None = None,
) -> "AIProjectClient":
    """
    Create a sync AIProjectClient using the provided configuration or by loading
    configuration automatically.

    Args:
        config: Optional configuration. If None, calls get_configuration() to load automatically.

    Returns:
        A configured sync AIProjectClient ready for use.
    """
    from azure.ai.projects import AIProjectClient

    if config is None:
        config = get_configuration(use_async=False)

    return AIProjectClient(
        endpoint=config.endpoint,
        credential=config.credential,
    )


def get_openai_endpoint_sync(
    config: FoundryClientConfiguration | None = None,
) -> str:
    """
    Retrieve the Azure OpenAI endpoint from the Foundry Project's connections (sync version).

    Args:
        config: Optional configuration. If None, calls get_configuration() to load automatically.

    Returns:
        The Azure OpenAI endpoint URL.

    Raises:
        ValueError: When no Azure OpenAI connection is found.
    """
    from azure.ai.projects import AIProjectClient

    if config is None:
        config = get_configuration(use_async=False)

    with AIProjectClient(
        endpoint=config.endpoint,
        credential=config.credential,
    ) as client:
        # Get the Azure OpenAI connection
        connection = client.connections.get_default(
            connection_type="AzureOpenAI",
            include_credentials=False,
        )
        
        if not connection or not connection.endpoint_url:
            raise ValueError(
                "No Azure OpenAI connection found in the Foundry Project. "
                "Ensure an Azure OpenAI connection is configured in your Azure AI Foundry project."
            )
        
        return connection.endpoint_url


def create_openai_client_sync(
    config: FoundryClientConfiguration | None = None,
) -> "AzureOpenAI":
    """
    Create a sync AzureOpenAI client using Foundry credentials.
    Automatically discovers the Azure OpenAI endpoint from the project's connections.

    Args:
        config: Optional configuration. If None, calls get_configuration() to load automatically.

    Returns:
        A configured AzureOpenAI client ready for use.
    """
    from openai import AzureOpenAI

    if config is None:
        config = get_configuration(use_async=False)

    openai_endpoint = get_openai_endpoint_sync(config)
    
    # Get a token from the credential
    token = config.credential.get_token("https://cognitiveservices.azure.com/.default")
    
    return AzureOpenAI(
        azure_endpoint=openai_endpoint,
        api_key=token.token,
        api_version="2024-06-01",
    )


def create_chat_client_sync(
    config: FoundryClientConfiguration | None = None,
    deployment_name: str | None = None,
):
    """
    Create a sync chat completion interface for the configured deployment.

    Args:
        config: Optional configuration. If None, calls get_configuration() to load automatically.
        deployment_name: Override for deployment name. If None, uses config.deployment_name.

    Returns:
        The AzureOpenAI client and deployment name tuple for chat completions.
    """
    if config is None:
        config = get_configuration(use_async=False)
    
    client = create_openai_client_sync(config)
    model = deployment_name or config.deployment_name
    
    return client, model


# =============================================================================
# Utility Methods
# =============================================================================


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
        config = get_configuration(use_async=True)

    client = create_project_client(config)
    try:
        deployments = []
        async for deployment in client.deployments.list():
            deployments.append({
                "name": deployment.name,
                "model": getattr(deployment, "model_name", "N/A"),
            })
        return deployments
    finally:
        await client.close()


async def get_random_deployment(
    config: FoundryClientConfiguration | None = None,
    chat_models_only: bool = True,
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


def validate_search_configuration(config: FoundryClientConfiguration) -> None:
    """
    Validate that Azure Search configuration is present.

    Args:
        config: The configuration to validate.

    Raises:
        ValueError: When required Azure Search values are missing.
    """
    if not config.search_endpoint:
        raise ValueError("AZURE_SEARCH_ENDPOINT configuration is required")
    if not config.search_api_key:
        raise ValueError("AZURE_SEARCH_API_KEY configuration is required")
    if not config.search_index_name:
        raise ValueError("AZURE_SEARCH_INDEX_NAME configuration is required")
