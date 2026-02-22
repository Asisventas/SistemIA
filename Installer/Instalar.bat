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

:: ═══════════════════════════════════════════════════════════════════
:: CONFIGURACIÓN DEL NOMBRE DEL SERVICIO
:: ═══════════════════════════════════════════════════════════════════
echo.
echo ┌─────────────────────────────────────────────────────────────────┐
echo │         CONFIGURACIÓN DEL SERVICIO DE WINDOWS                  │
echo └─────────────────────────────────────────────────────────────────┘
echo.
echo El sistema se instalará como un servicio de Windows.
echo Puede personalizar el nombre del servicio para identificarlo.
echo.
echo Ejemplos: SistemIA, SistemIA_Empresa1, MiNegocio, GestionERP
echo.
set /p SERVICE_NAME="Nombre del servicio [SistemIA]: "

:: Si está vacío, usar valor por defecto
if "%SERVICE_NAME%"=="" set SERVICE_NAME=SistemIA

:: Limpiar espacios y caracteres especiales del nombre
set SERVICE_NAME=%SERVICE_NAME: =_%

echo.
echo ─────────────────────────────────────────────────────────────────
echo   Nombre del servicio: %SERVICE_NAME%
echo ─────────────────────────────────────────────────────────────────
echo.

:: Ejecutar script de instalación con el nombre de servicio
echo Iniciando instalador de SistemIA...
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0Install-SistemIA.ps1" -ConfigFile "%~dp0config.json" -ServiceName "%SERVICE_NAME%"

echo.
pause
