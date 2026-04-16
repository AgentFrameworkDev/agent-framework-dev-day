#!/bin/bash
# MCP Workshop - Launch All .NET Services (macOS/Linux)
# This script starts all MCP components in separate terminal windows/tabs

echo "============================================================"
echo "           MCP Workshop (.NET) - Starting All Services"
echo "============================================================"
echo ""

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Function to open a new terminal window/tab based on the OS
open_terminal() {
    local title="$1"
    local command="$2"
    
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS - use osascript to open Terminal.app
        osascript -e "tell application \"Terminal\" to do script \"echo -n -e '\\033]0;$title\\007'; cd '$SCRIPT_DIR' && $command\""
    elif command -v gnome-terminal &> /dev/null; then
        # Linux with GNOME Terminal
        gnome-terminal --title="$title" -- bash -c "cd '$SCRIPT_DIR' && $command; exec bash"
    elif command -v konsole &> /dev/null; then
        # Linux with KDE Konsole
        konsole --new-tab -p tabtitle="$title" -e bash -c "cd '$SCRIPT_DIR' && $command; exec bash" &
    elif command -v xterm &> /dev/null; then
        # Fallback to xterm
        xterm -T "$title" -e "cd '$SCRIPT_DIR' && $command; exec bash" &
    else
        echo "No supported terminal emulator found. Please install gnome-terminal, konsole, or xterm."
        exit 1
    fi
}

echo "Starting Remote Server (REST API on port 5060)..."
open_terminal "Remote Server (5060)" "cd RemoteServer && dotnet run"

# Wait a moment for the REST API to start
sleep 3

echo "Starting MCP Bridge Server (HTTP on port 5070)..."
open_terminal "MCP Bridge (5070)" "cd McpBridge && dotnet run"

# Wait a moment for the bridge to start
sleep 3

echo "Starting MCP Agent Client..."
open_terminal "MCP Agent Client" "cd McpAgentClient && dotnet run"

echo ""
echo "============================================================"
echo "All services started in separate windows:"
echo "  - Remote Server (REST API): http://localhost:5060"
echo "  - MCP Bridge (MCP/HTTP):    http://localhost:5070"
echo "  - MCP Agent Client:         Interactive menu"
echo ""
echo "Note: MCP Local Server runs via STDIO when selected in the"
echo "      Agent Client menu (option 1)."
echo "============================================================"
