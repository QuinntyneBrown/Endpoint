@echo off
REM ==============================================================================
REM up.bat - Start Endpoint Engineering Services
REM ==============================================================================
REM Services:
REM   - endpoint-engineering (Angular) - Port 4200
REM   - Endpoint.Engineering.ALaCarte.Api (.NET) - Port 5076
REM   - Endpoint.Engineering.ApiGateway (.NET) - Port 5079
REM ==============================================================================

echo Starting Endpoint Engineering Services...
echo.

REM Get the script directory and navigate to repo root
set SCRIPT_DIR=%~dp0
set REPO_ROOT=%SCRIPT_DIR%..\..

REM Start Angular Frontend (endpoint-engineering)
echo Starting Angular Frontend (endpoint-engineering) on port 4200...
start "Endpoint-Angular" cmd /k "cd /d %REPO_ROOT%\src\Endpoint.Engineering.Workspace && npm start"

REM Start ALaCarte API
echo Starting ALaCarte API on port 5076...
start "Endpoint-ALaCarte-Api" cmd /k "cd /d %REPO_ROOT%\src\Endpoint.Engineering.ALaCarte.Api && dotnet run"

REM Start API Gateway
echo Starting API Gateway on port 5079...
start "Endpoint-ApiGateway" cmd /k "cd /d %REPO_ROOT%\src\Endpoint.Engineering.ApiGateway && dotnet run"

echo.
echo All services started in separate windows.
echo.
echo Service URLs:
echo   - Angular Frontend:  http://localhost:4200
echo   - ALaCarte API:      http://localhost:5076
echo   - API Gateway:       http://localhost:5079 (main entry point)
echo.
echo Use down.bat to stop all services.
