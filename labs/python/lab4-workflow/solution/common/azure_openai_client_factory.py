"""
Azure OpenAI Client Factory

Factory class for creating Azure OpenAI chat clients with multiple authentication options.
Supports configuration from environment variables.
"""

import os
from openai import AzureOpenAI
from azure.identity import AzureCliCredential, ClientSecretCredential, DefaultAzureCredential


def create_chat_client() -> AzureOpenAI:
    """
    Creates an Azure OpenAI chat client with support for multiple authentication methods:
    1. API Key authentication (AZURE_OPENAI_API_KEY)
    2. Service Principal authentication (AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET)
    3. Azure CLI credential
    4. Managed Identity / DefaultAzureCredential (fallback)
    
    Returns:
        AzureOpenAI: Configured Azure OpenAI client
        
    Raises:
        ValueError: If required configuration is missing
    """
    # Get endpoint (required)
    endpoint = os.environ.get("AZURE_OPENAI_ENDPOINT")
    if not endpoint:
        raise ValueError(
            "Azure OpenAI endpoint is not configured. "
            "Set 'AZURE_OPENAI_ENDPOINT' environment variable."
        )
    
    # Get deployment name (optional, default: gpt-4o-mini)
    deployment = os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o-mini")
    api_version = "2024-02-15-preview"
    
    # Option 1: API Key authentication
    api_key = os.environ.get("AZURE_OPENAI_API_KEY")
    if api_key:
        print("Using API Key authentication")
        return AzureOpenAI(
            azure_endpoint=endpoint,
            api_key=api_key,
            api_version=api_version
        )
    
    # Option 2: Service Principal authentication
    tenant_id = os.environ.get("AZURE_TENANT_ID")
    client_id = os.environ.get("AZURE_CLIENT_ID")
    client_secret = os.environ.get("AZURE_CLIENT_SECRET")
    
    if tenant_id and client_id and client_secret:
        print("Using Service Principal authentication")
        credential = ClientSecretCredential(
            tenant_id=tenant_id,
            client_id=client_id,
            client_secret=client_secret
        )
        token = credential.get_token("https://cognitiveservices.azure.com/.default")
        return AzureOpenAI(
            azure_endpoint=endpoint,
            api_key=token.token,
            api_version=api_version
        )
    
    # Option 3: Try Azure CLI credential first
    try:
        print("Trying Azure CLI authentication...")
        credential = AzureCliCredential()
        token = credential.get_token("https://cognitiveservices.azure.com/.default")
        print("Using Azure CLI authentication")
        return AzureOpenAI(
            azure_endpoint=endpoint,
            api_key=token.token,
            api_version=api_version
        )
    except Exception:
        pass
    
    # Option 4: Fallback to DefaultAzureCredential (Managed Identity, etc.)
    print("Using Managed Identity / DefaultAzureCredential authentication")
    credential = DefaultAzureCredential()
    token = credential.get_token("https://cognitiveservices.azure.com/.default")
    return AzureOpenAI(
        azure_endpoint=endpoint,
        api_key=token.token,
        api_version=api_version
    )


def get_deployment_name() -> str:
    """Get the deployment name from environment or default."""
    return os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o-mini")
