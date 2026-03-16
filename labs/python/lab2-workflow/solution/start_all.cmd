@echo off
REM MCP Workflow Lab - Launch Application
REM This script starts the workflow demo application

echo ============================================================
echo          MCP Workflow Lab - Starting Application
echo ============================================================
echo.

REM Get the directory where this script is located
set SCRIPT_DIR=%~dp0

REM Activate virtual environment command
set ACTIVATE=%SCRIPT_DIR%..\..\venv\Scripts\activate

echo Starting Workflow Demo Application...
cmd /k "cd /d %SCRIPT_DIR% && call %ACTIVATE% && python program.py"
