#!/bin/bash

# Generate .vscode/launch.json from all .csproj files in labs/dotnet

mkdir -p .vscode

cat > .vscode/launch.json << 'HEADER'
{
  "version": "0.2.0",
  "configurations": [
HEADER

first=true

# Find all .csproj files, sorted
find ./ -name "*.csproj" | sort | while read -r csproj; do
    # Extract lab number (e.g., "lab0" -> "0")
    labnum=$(echo "$csproj" | grep -oE 'lab[0-9]+' | head -1 | sed 's/lab//')
    
    # Determine if it's a solution or lab
    if [[ "$csproj" == *"/solution/"* ]]; then
        name="Lab $labnum (Solution)"
    else
        name="Lab $labnum"
    fi
    
    # Convert to relative path with ${workspaceFolder}
    relpath=$(echo "$csproj" | sed 's|^\./||')
    
    # Add comma before all but first entry
    if [ "$first" = true ]; then
        first=false
    else
        echo "," >> .vscode/launch.json
    fi
    
    # Write configuration
    cat >> .vscode/launch.json << EOF
    {
      "name": "$name",
      "type": "coreclr",
      "request": "launch",
      "projectPath": "\${workspaceFolder}/$relpath",
      "console": "integratedTerminal"
    }
EOF

done

# Close the JSON
cat >> .vscode/launch.json << 'FOOTER'

  ]
}
FOOTER

echo "Generated .vscode/launch.json"

