@echo off
REM MCP Workshop - Launch All Servers
REM This script starts all MCP components in separate windows

echo ============================================================
echo              MCP Workshop - Starting All Services
echo ============================================================
echo.

REM Get the directory where this script is located
set SCRIPT_DIR=%~dp0

REM Get the python folder (two levels up from solution)
set PYTHON_DIR=%SCRIPT_DIR%..\..\

REM Activate virtual environment command
set ACTIVATE=%PYTHON_DIR%.venv\Scripts\activate

echo Starting MCP Remote Server (REST API on port 5060)...
start "MCP Remote Server (5060)" cmd /k "cd /d %SCRIPT_DIR% && call %ACTIVATE% && python -m mcp_remote_server.main"

REM Wait a moment for the REST API to start
timeout /t 2 /nobreak > nul

echo Starting MCP Bridge Server (HTTP/SSE on port 5070)...
start "MCP Bridge (5070)" cmd /k "cd /d %SCRIPT_DIR% && call %ACTIVATE% && python -m mcp_bridge.main"

REM Wait a moment for the bridge to start
timeout /t 2 /nobreak > nul

echo Starting MCP Agent Client...
start "MCP Agent Client" cmd /k "cd /d %SCRIPT_DIR% && call %ACTIVATE% && python -m mcp_agent_client.main"

echo.
echo ============================================================
echo All services started in separate windows:
echo   - MCP Remote Server (REST API): http://localhost:5060
echo   - MCP Bridge (MCP/HTTP):        http://localhost:5070
echo   - MCP Agent Client:             Interactive menu
echo.
echo Note: MCP Local Server runs via STDIO when selected in the
echo       Agent Client menu (option 1).
echo ============================================================
