#!/bin/bash
# MCP Workflow Lab - Launch Application
# This script starts the workflow demo application

echo "============================================================"
echo "         MCP Workflow Lab - Starting Application"
echo "============================================================"
echo ""

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Activate virtual environment
VENV_ACTIVATE="$SCRIPT_DIR/../../../../.venv/bin/activate"
if [ -f "$VENV_ACTIVATE" ]; then
    source "$VENV_ACTIVATE"
else
    echo "Warning: Virtual environment not found at $VENV_ACTIVATE"
fi

echo "Starting Workflow Demo Application..."
cd "$SCRIPT_DIR"
python program.py
