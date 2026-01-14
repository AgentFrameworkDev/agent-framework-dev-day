"""
Azure OpenAI Client Factory

============================================================================
EXERCISE 1: Configure Azure OpenAI Client
============================================================================
Factory class for creating Azure OpenAI chat clients with multiple authentication options.
Complete the TODO sections to enable AI functionality.
============================================================================
"""

import os
from openai import AzureOpenAI
from azure.identity import AzureCliCredential, ClientSecretCredential, DefaultAzureCredential


def create_chat_client() -> AzureOpenAI:
    """
    Creates an Azure OpenAI chat client with support for multiple authentication methods.
    
    Returns:
        AzureOpenAI: Configured Azure OpenAI client
        
    Raises:
        ValueError: If required configuration is missing
    """
    # ============================================================================
    # STEP 1.1: Get endpoint from environment
    # Uncomment the lines below
    # ============================================================================
    # endpoint = os.environ.get("AZURE_OPENAI_ENDPOINT")
    # if not endpoint:
    #     raise ValueError(
    #         "Azure OpenAI endpoint is not configured. "
    #         "Set 'AZURE_OPENAI_ENDPOINT' environment variable."
    #     )
    
    # Placeholder - REMOVE after uncommenting above
    endpoint = "https://YOUR-RESOURCE.openai.azure.com/"
    
    # ============================================================================
    # STEP 1.2: Get deployment name from environment
    # Uncomment the line below
    # ============================================================================
    # deployment = os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o-mini")
    
    # Placeholder - REMOVE after uncommenting above
    deployment = "gpt-4o-mini"
    
    api_version = "2024-02-15-preview"
    
    # ============================================================================
    # STEP 1.3: Enable authentication
    # Uncomment ONE of the authentication options below based on your setup
    # ============================================================================
    
    # Option 1: API Key authentication
    # api_key = os.environ.get("AZURE_OPENAI_API_KEY")
    # if api_key:
    #     print("Using API Key authentication")
    #     return AzureOpenAI(
    #         azure_endpoint=endpoint,
    #         api_key=api_key,
    #         api_version=api_version
    #     )
    
    # Option 2: Service Principal authentication
    # tenant_id = os.environ.get("AZURE_TENANT_ID")
    # client_id = os.environ.get("AZURE_CLIENT_ID")
    # client_secret = os.environ.get("AZURE_CLIENT_SECRET")
    # if tenant_id and client_id and client_secret:
    #     print("Using Service Principal authentication")
    #     credential = ClientSecretCredential(
    #         tenant_id=tenant_id,
    #         client_id=client_id,
    #         client_secret=client_secret
    #     )
    #     token = credential.get_token("https://cognitiveservices.azure.com/.default")
    #     return AzureOpenAI(
    #         azure_endpoint=endpoint,
    #         api_key=token.token,
    #         api_version=api_version
    #     )
    
    # Option 3: Azure CLI credential
    # try:
    #     print("Trying Azure CLI authentication...")
    #     credential = AzureCliCredential()
    #     token = credential.get_token("https://cognitiveservices.azure.com/.default")
    #     print("Using Azure CLI authentication")
    #     return AzureOpenAI(
    #         azure_endpoint=endpoint,
    #         api_key=token.token,
    #         api_version=api_version
    #     )
    # except Exception:
    #     pass
    
    # Option 4: Fallback to DefaultAzureCredential
    # print("Using Managed Identity / DefaultAzureCredential authentication")
    # credential = DefaultAzureCredential()
    # token = credential.get_token("https://cognitiveservices.azure.com/.default")
    # return AzureOpenAI(
    #     azure_endpoint=endpoint,
    #     api_key=token.token,
    #     api_version=api_version
    # )
    
    # Placeholder - REMOVE after uncommenting above
    raise NotImplementedError("Exercise 1 not completed. Please uncomment the authentication code above.")


def get_deployment_name() -> str:
    """Get the deployment name from environment or default."""
    return os.environ.get("AZURE_OPENAI_DEPLOYMENT_NAME", "gpt-4o-mini")
