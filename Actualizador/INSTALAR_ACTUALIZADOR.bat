@echo off
:: ============================================================
:: Instalador de SistemIA Actualizador
:: Este archivo ejecuta el script de instalación como administrador
:: ============================================================

echo.
echo ============================================================
echo    INSTALADOR DE SISTEMÍA ACTUALIZADOR
echo ============================================================
echo.

:: Verificar si es administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Solicitando permisos de administrador...
    echo.
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

:: Ejecutar el script de PowerShell
cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -File "InstalarServicioActualizador.ps1"

pause
