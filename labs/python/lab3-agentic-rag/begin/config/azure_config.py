"""
Configuration module for Azure AI Search and OpenAI settings.
"""
import os
from dataclasses import dataclass
from azure.identity import DefaultAzureCredential


@dataclass
class AzureConfig:
    """Configuration for Azure services."""
    
    # Azure AI Search
    search_endpoint: str
    search_api_key: str
    search_index_name: str
    
    # Azure OpenAI
    openai_endpoiint: str
    openai_api_version: str
    chat_model: str
    embedding_model: str
    
    # Credentials
    credential: DefaultAzureCredential
    
    @classmethod
    def from_env(cls) -> "AzureConfig":
        """Load configuration from environment variables."""
        return cls(
            search_endpoint=os.getenv("AZURE_SEARCH_ENDPOINT", ""),
            search_api_key=os.getenv("AZURE_SEARCH_API_KEY", ""),
            search_index_name=os.getenv("AZURE_SEARCH_INDEX_NAME", ""),
            openai_endpoiint=os.getenv("AZURE_OPENAI_ENDPOINT", ""),
            openai_api_version=os.getenv("AZURE_OPENAI_API_VERSION", ""),
            chat_model=os.getenv("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME", ""),
            embedding_model=os.getenv("AZURE_OPENAI_EMBEDDING_DEPLOYMENT", ""),
            credential=DefaultAzureCredential()
        )
    
    def validate(self) -> None:
        """Validate that all required configuration is present."""
        if not self.search_endpoint:
            raise ValueError("AZURE_SEARCH_ENDPOINT environment variable is required")
        if not self.search_api_key:
            raise ValueError("AZURE_SEARCH_API_KEY environment variable is required")
        if not self.search_index_name:
            raise ValueError("AZURE_SEARCH_INDEX_NAME environment variable is required")
        if not self.embedding_model:
            raise ValueError("AZURE_OPENAI_EMBEDDING_DEPLOYMENT environment variable is required")
        if not self.chat_model:
            raise ValueError("AZURE_OPENAI_CHAT_DEPLOYMENT_NAME environment variable is required")
        if not self.openai_endpoiint:
            raise ValueError("AZURE_OPENAI_ENDPOINT environment variable is required")
