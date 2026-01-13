"""
List Available Models in Azure AI Foundry Project

Lists all model deployments available in the configured Azure AI project.
Uses the data plane API (like C# version) for proper authorization.
"""

import asyncio

from foundry_client_factory import get_configuration, list_available_deployments


async def main() -> None:
    # Load configuration
    config = get_configuration()

    print("List Available Model Deployments")
    print("========================================")
    print(f"Endpoint: {config.endpoint}")
    print(f"Default Deployment: {config.deployment_name}")
    print(f"Auth: {'Service Principal' if config.is_service_principal else 'Default Azure Credential'}")
    print()

    print("Available Model Deployments:")
    print("-" * 60)
    print(f"{'Deployment Name':<30} {'Model':<30}")
    print("-" * 60)

    deployments = await list_available_deployments(config)

    # Sort by name
    for d in sorted(deployments, key=lambda x: x["name"]):
        print(f"  {d['name']:<28} {d['model']:<30}")

    print()
    print(f"Total: {len(deployments)} deployment(s) available")


if __name__ == "__main__":
    asyncio.run(main())
