"""
List Available Models in Azure AI Foundry Project

Lists all model deployments available in the configured Azure AI project.
"""

import asyncio
import os
from pathlib import Path

from dotenv import load_dotenv
from azure.identity import DefaultAzureCredential
from azure.mgmt.cognitiveservices import CognitiveServicesManagementClient


def load_root_dotenv(filename: str = ".env") -> Path:
    """Walk up the directory tree to find and load a .env file."""
    here = Path(__file__).resolve()
    for d in [here.parent, *here.parents]:
        candidate = d / filename
        if candidate.is_file():
            load_dotenv(candidate, override=False)
            return candidate
    raise FileNotFoundError(f"Could not find {filename} by walking up from {here}")


def main() -> None:
    # Load environment variables from .env file
    env_path = load_root_dotenv()
    print(f"Loaded environment from: {env_path}")

    # Configuration
    subscription_id = "9ba7af2d-0913-4012-a72f-29edf9ee02d0"
    resource_group = "rg-agentlab"
    account_name = "proj-agentlab-resource"

    print("List Available Model Deployments")
    print("========================================")
    print(f"Subscription: {subscription_id}")
    print(f"Resource Group: {resource_group}")
    print(f"Account: {account_name}")
    print()

    credential = DefaultAzureCredential()
    client = CognitiveServicesManagementClient(credential, subscription_id)

    print("Available Model Deployments:")
    print("-" * 60)
    print(f"{'Deployment Name':<30} {'Model':<30}")
    print("-" * 60)

    deployments = client.deployments.list(resource_group, account_name)
    
    deployment_list = []
    for deployment in deployments:
        model_name = deployment.properties.model.name if deployment.properties.model else "N/A"
        model_version = deployment.properties.model.version if deployment.properties.model else ""
        deployment_list.append({
            "name": deployment.name,
            "model": f"{model_name} ({model_version})" if model_version else model_name
        })
    
    # Sort by name
    for d in sorted(deployment_list, key=lambda x: x["name"]):
        print(f"  {d['name']:<28} {d['model']:<30}")

    print()
    print(f"Total: {len(deployment_list)} deployment(s) available")


if __name__ == "__main__":
    main()
