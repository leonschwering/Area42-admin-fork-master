# Area42 Startup Script (PowerShell)
# This script starts both services for you

Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host " Area42 Reservation System - Startup" -ForegroundColor Cyan
Write-Host "============================================`n" -ForegroundColor Cyan

# Check if running from correct directory
if (-not (Test-Path "Area42-1.Web\Area42-1.Web.csproj")) {
    Write-Host "ERROR: Run this script from the solution root directory" -ForegroundColor Red
    Write-Host "Expected: C:\Users\LAURE\source\repos\Area42-1\" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "Setting up services..." -ForegroundColor Yellow
Write-Host ""

# Start API Service
Write-Host "[1/2] Starting API Service on https://localhost:7001" -ForegroundColor Green
Write-Host "      (Keep this window open)" -ForegroundColor Gray
Start-Process powershell -ArgumentList {
    cd "Area42-1.ApiService"
    Write-Host "`nStarting API Service..." -ForegroundColor Green
    Write-Host "Press CTRL+C to stop`n" -ForegroundColor Yellow
    dotnet run
} -NoNewWindow

# Wait for API to start
Start-Sleep -Seconds 3

# Start Web App
Write-Host "[2/2] Starting Web App on https://localhost:7000" -ForegroundColor Green
Write-Host "      (Keep this window open)" -ForegroundColor Gray
Start-Process powershell -ArgumentList {
    cd "Area42-1.Web"
    Write-Host "`nStarting Web App..." -ForegroundColor Green
    Write-Host "Press CTRL+C to stop`n" -ForegroundColor Yellow
    dotnet run
} -NoNewWindow

# Wait for services to start
Start-Sleep -Seconds 3

Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host " ✓ Services Starting" -ForegroundColor Green
Write-Host "============================================`n" -ForegroundColor Cyan

Write-Host "Web App:    https://localhost:7000" -ForegroundColor Cyan
Write-Host "API:        https://localhost:7001" -ForegroundColor Cyan
Write-Host "Admin:      https://localhost:7000/admin" -ForegroundColor Cyan
Write-Host ""

Write-Host "Opening web app in browser..." -ForegroundColor Yellow
Start-Process "https://localhost:7000"

Write-Host "`nServices are running. Keep PowerShell windows open." -ForegroundColor Gray
Write-Host "To stop services, close the PowerShell windows or press CTRL+C." -ForegroundColor Gray
Write-Host ""
Read-Host "Press Enter to exit this window"
