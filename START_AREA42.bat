@echo off
REM Area42 Startup Script
REM This script helps you start both services at once

TITLE Area42 - Starting Services

echo.
echo ============================================
echo  Area42 Reservation System - Startup
echo ============================================
echo.

REM Check if running from correct directory
if not exist "Area42-1.Web\Area42-1.Web.csproj" (
    echo ERROR: Run this script from the solution root directory
    echo Expected: C:\Users\LAURE\source\repos\Area42-1\
    pause
    exit /b 1
)

echo Setting up services...
echo.

REM Start API Service in new window
echo [1/2] Starting API Service on https://localhost:7001
echo        Press CTRL+C to stop
start "Area42 API Service" cmd /k "cd Area42-1.ApiService && dotnet run"

REM Wait a moment for API to start
timeout /t 3 /nobreak

REM Start Web App in new window
echo [2/2] Starting Web App on https://localhost:7000
echo        Press CTRL+C to stop
start "Area42 Web App" cmd /k "cd Area42-1.Web && dotnet run"

REM Wait for services to fully start
timeout /t 3 /nobreak

echo.
echo ============================================
echo  ✓ Services Starting
echo ============================================
echo.
echo Web App:    https://localhost:7000
echo API:        https://localhost:7001
echo.
echo Press any key to open the web app...
pause

REM Open web app in default browser
start https://localhost:7000

echo.
echo Services are running. Keep this window open.
echo To stop services, close the other windows or press CTRL+C.
echo.
pause

