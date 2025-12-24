@echo off
chcp 65001 > nul
title SistemIA - Crear Paquete de Instalación

echo.
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║         SISTEMÍA - CREAR PAQUETE DE INSTALACIÓN              ║
echo ╚═══════════════════════════════════════════════════════════════╝
echo.

set /p VERSION="Ingrese la versión (ej: 1.0.0): "

powershell -ExecutionPolicy Bypass -File "%~dp0Crear-Paquete.ps1" -Version "%VERSION%"

echo.
pause
