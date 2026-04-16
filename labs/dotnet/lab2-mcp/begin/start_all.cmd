@echo off
REM MCP Workshop - Launch All .NET Services
REM This script starts all MCP components in separate windows

echo ============================================================
echo           MCP Workshop (.NET) - Starting All Services
echo ============================================================
echo.

REM Get the directory where this script is located
set SCRIPT_DIR=%~dp0

echo Starting Remote Server (REST API on port 5060)...
start "Remote Server (5060)" cmd /k "cd /d %SCRIPT_DIR%RemoteServer && dotnet run"

REM Wait a moment for the REST API to start
timeout /t 3 /nobreak > nul

echo Starting MCP Bridge Server (HTTP on port 5070)...
start "MCP Bridge (5070)" cmd /k "cd /d %SCRIPT_DIR%McpBridge && dotnet run"

REM Wait a moment for the bridge to start
timeout /t 3 /nobreak > nul

echo Starting MCP Agent Client...
start "MCP Agent Client" cmd /k "cd /d %SCRIPT_DIR%McpAgentClient && dotnet run"

echo.
echo ============================================================
echo All services started in separate windows:
echo   - Remote Server (REST API): http://localhost:5060
echo   - MCP Bridge (MCP/HTTP):    http://localhost:5070
echo   - MCP Agent Client:         Interactive menu
echo.
echo Note: MCP Local Server runs via STDIO when selected in the
echo       Agent Client menu (option 1).
echo ============================================================
