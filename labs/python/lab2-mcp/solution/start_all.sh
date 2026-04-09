#!/bin/bash
# MCP Workshop - Launch All Servers (macOS/Linux)
# This script starts all MCP components in separate terminal windows/tabs

echo "============================================================"
echo "              MCP Workshop - Starting All Services"
echo "============================================================"
echo ""

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Get the python folder (two levels up from solution)
PYTHON_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Virtual environment activation
ACTIVATE="source $PYTHON_DIR/.venv/bin/activate"

# Function to open a new terminal window/tab based on the OS
open_terminal() {
    local title="$1"
    local command="$2"
    
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS - use osascript to open Terminal.app
        osascript -e "tell application \"Terminal\" to do script \"echo -n -e '\\033]0;$title\\007'; cd '$SCRIPT_DIR' && $ACTIVATE && $command\""
    elif command -v gnome-terminal &> /dev/null; then
        # Linux with GNOME Terminal
        gnome-terminal --title="$title" -- bash -c "cd '$SCRIPT_DIR' && $ACTIVATE && $command; exec bash"
    elif command -v konsole &> /dev/null; then
        # Linux with KDE Konsole
        konsole --new-tab -p tabtitle="$title" -e bash -c "cd '$SCRIPT_DIR' && $ACTIVATE && $command; exec bash" &
    elif command -v xterm &> /dev/null; then
        # Fallback to xterm
        xterm -T "$title" -e "cd '$SCRIPT_DIR' && $ACTIVATE && $command; exec bash" &
    else
        echo "No supported terminal emulator found. Please install gnome-terminal, konsole, or xterm."
        exit 1
    fi
}

echo "Starting MCP Remote Server (REST API on port 5060)..."
open_terminal "MCP Remote Server (5060)" "python -m mcp_remote_server.main"

# Wait a moment for the REST API to start
sleep 2

echo "Starting MCP Bridge Server (HTTP/SSE on port 5070)..."
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
