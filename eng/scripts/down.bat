@echo off
REM ==============================================================================
REM down.bat - Stop Endpoint Engineering Services
REM ==============================================================================
REM Services:
REM   - endpoint-engineering (Angular) - Port 4200
REM   - Endpoint.Engineering.ALaCarte.Api (.NET) - Port 5076
REM   - Endpoint.Engineering.ApiGateway (.NET) - Port 5079
REM ==============================================================================

echo Stopping Endpoint Engineering Services...
echo.

REM Stop Angular Frontend (port 4200)
echo Stopping Angular Frontend (port 4200)...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr :4200 ^| findstr LISTENING') do (
    taskkill /PID %%a /F >nul 2>&1
)

REM Stop ALaCarte API (port 5076)
echo Stopping ALaCarte API (port 5076)...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5076 ^| findstr LISTENING') do (
    taskkill /PID %%a /F >nul 2>&1
)

REM Stop API Gateway (port 5079)
echo Stopping API Gateway (port 5079)...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5079 ^| findstr LISTENING') do (
    taskkill /PID %%a /F >nul 2>&1
)

echo.
echo All services stopped.
