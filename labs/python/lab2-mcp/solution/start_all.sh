#!/bin/bash
# MCP Workshop - Launch All Servers
# This script starts all MCP components in separate terminal windows

echo "============================================================"
echo "             MCP Workshop - Starting All Services"
echo "============================================================"
echo ""

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Activate virtual environment
VENV_ACTIVATE="$SCRIPT_DIR/../../../.venv/bin/activate"
if [ -f "$VENV_ACTIVATE" ]; then
    source "$VENV_ACTIVATE"
else
    echo "Warning: Virtual environment not found at $VENV_ACTIVATE"
fi

# Function to open a new terminal (cross-platform)
open_terminal() {
    local title="$1"
    local cmd="$2"
    if command -v gnome-terminal &> /dev/null; then
        gnome-terminal --title="$title" -- bash -c "cd '$SCRIPT_DIR' && source '$VENV_ACTIVATE' 2>/dev/null; $cmd; exec bash"
    elif command -v xterm &> /dev/null; then
        xterm -title "$title" -e "cd '$SCRIPT_DIR' && source '$VENV_ACTIVATE' 2>/dev/null && $cmd" &
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        osascript -e "tell app \"Terminal\" to do script \"cd '$SCRIPT_DIR' && source '$VENV_ACTIVATE' 2>/dev/null && $cmd\""
    else
        echo "Cannot open terminal. Run manually: $cmd"
    fi
}

echo "Starting MCP Remote Server (REST API on port 5060)..."
open_terminal "MCP Remote Server (5060)" "python -m mcp_remote_server.main"

# Wait a moment for the REST API to start
sleep 2

echo "Starting MCP Bridge Server (Streamable HTTP on port 5070)..."
open_terminal "MCP Bridge (5070)" "python -m mcp_bridge.main"

# Wait a moment for the bridge to start
sleep 2

echo "Starting MCP Agent Client..."
open_terminal "MCP Agent Client" "python -m mcp_agent_client.main"

echo ""
echo "============================================================"
echo "All services started in separate windows:"
echo "  - MCP Remote Server (REST API): http://localhost:5060"
echo "  - MCP Bridge (MCP/HTTP):        http://localhost:5070"
echo "  - MCP Agent Client:             Interactive menu"
echo ""
echo "Note: MCP Local Server runs via STDIO when selected in the"
echo "      Agent Client menu (option 1)."
echo "============================================================"
