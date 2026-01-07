#!/usr/bin/env bash

SP_NAME="my-agent-app"
ROLE="Cognitive Services User"
SUB_ID="9ba7af2d-0913-4012-a72f-29edf9ee02d0"
RG="rg-agentlab"
RESOURCE_NAME="proj-agentlab-resource"

# Create a service principal
echo "Creating service principal '$SP_NAME'..."
SP_OUTPUT=$(az ad sp create-for-rbac --name "$SP_NAME" \
  --role "$ROLE" \
  --scopes "/subscriptions/$SUB_ID/resourceGroups/$RG/providers/Microsoft.CognitiveServices/accounts/$RESOURCE_NAME" \
  --output json)

# Extract values from the JSON output
AZURE_CLIENT_ID=$(echo "$SP_OUTPUT" | jq -r '.appId')
AZURE_CLIENT_SECRET=$(echo "$SP_OUTPUT" | jq -r '.password')
AZURE_TENANT_ID=$(echo "$SP_OUTPUT" | jq -r '.tenant')

echo ""
echo "Service principal '$SP_NAME' created successfully!"
echo ""
echo "Add these to your .env file:"
echo "AZURE_TENANT_ID=$AZURE_TENANT_ID"
echo "AZURE_CLIENT_ID=$AZURE_CLIENT_ID"
echo "AZURE_CLIENT_SECRET=$AZURE_CLIENT_SECRET"



