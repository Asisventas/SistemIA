@echo off
echo ============================================
echo    PUBLICAR SISTEMITA (Self-Contained)
echo ============================================
echo.
echo Este script genera un paquete completo con
echo .NET 8 incluido para distribucion.
echo.
echo Presione cualquier tecla para continuar...
pause >nul

cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -File ".\Publicar.ps1"

echo.
pause
