"""
Foundry Client Factory for Python

Factory for creating Microsoft Foundry clients with automatic configuration discovery.
Searches parent directories for .env files and supports environment variable configuration.

Environment Variables:
    Required:
        AZURE_AI_PROJECT_ENDPOINT - Azure AI Foundry project endpoint

    Optional (Service Principal auth):
        AZURE_TENANT_ID - Azure tenant ID
        AZURE_CLIENT_ID - Service principal client ID
        AZURE_CLIENT_SECRET - Service principal client secret
"""

from dataclasses import dataclass, field
from pathlib import Path
from typing import TYPE_CHECKING
import os

from dotenv import load_dotenv

if TYPE_CHECKING:
    from azure.core.credentials_async import AsyncTokenCredential


# =============================================================================
# Configuration Data Classes
# =============================================================================


@dataclass
class FoundryConfig:
    """
    Configuration details for creating Microsoft Foundry clients.
    """

    endpoint: str
    credential: "AsyncTokenCredential"
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


def find_env_file(filename: str = _DEFAULT_ENV_FILENAME) -> Path | None:
    """
    Search current directory and parent directories for a .env file.

    Args:
        filename: The name of the config file to search for. Defaults to ".env".

    Returns:
        Path to the .env file, or None if not found.
    """
    directory = Path.cwd()

    while directory != directory.parent:
        env_path = directory / filename
        if env_path.is_file():
            return env_path
        directory = directory.parent

    # Check root directory as well
    env_path = directory / filename
    if env_path.is_file():
        return env_path

    return None


# =============================================================================
# Credential Factory
# =============================================================================


def get_azure_credential(
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


def load_config(env_filename: str = _DEFAULT_ENV_FILENAME) -> FoundryConfig:
    """
    Load configuration from .env (searching parent directories) and environment variables.

    Args:
        env_filename: The configuration file name to search for. Defaults to ".env".

    Returns:
        Configuration containing endpoint and credential.

    Raises:
        FileNotFoundError: When no .env file is found.
        ValueError: When required configuration values are missing.
    """
    # Find and load the .env file
    env_path = find_env_file(env_filename)
    if env_path is None:
        raise FileNotFoundError(
            f"Could not find {env_filename} in current directory or any parent directory. "
            f"Current directory: {Path.cwd()}"
        )

    load_dotenv(env_path, override=False)

    # Get required configuration
    endpoint = os.environ.get("AZURE_AI_PROJECT_ENDPOINT")
    if not endpoint:
        raise ValueError(
            "Azure AI endpoint not configured. "
            "Set AZURE_AI_PROJECT_ENDPOINT in .env or environment."
        )

    # Get optional service principal credentials
    tenant_id = os.environ.get("AZURE_TENANT_ID")
    client_id = os.environ.get("AZURE_CLIENT_ID")
    client_secret = os.environ.get("AZURE_CLIENT_SECRET")

    # Create credential
    credential = get_azure_credential(tenant_id, client_id, client_secret)

    return FoundryConfig(
        endpoint=endpoint,
        credential=credential,
        _tenant_id=tenant_id,
        _client_id=client_id,
        _client_secret=client_secret,
    )
