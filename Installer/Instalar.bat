@echo off
chcp 65001 > nul
title SistemIA - Instalador

echo.
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║                      SISTEMÍA INSTALLER                       ║
echo ║            Sistema de Gestión Empresarial v1.0                ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

:: Verificar permisos de administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] Este instalador requiere permisos de administrador.
    echo.
    echo Por favor, haga clic derecho en este archivo y seleccione
    echo "Ejecutar como administrador"
    echo.
    pause
    exit /b 1
)

:: Verificar PowerShell
where powershell >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] PowerShell no está instalado o no está en el PATH.
    pause
    exit /b 1
)

:: Cambiar al directorio del instalador
cd /d "%~dp0"

:: Ejecutar script de instalación
echo Iniciando instalador de SistemIA...
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0Install-SistemIA.ps1" -ConfigFile "%~dp0config.json"

echo.
pause
