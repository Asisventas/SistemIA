@echo off
title SistemIA - Instalador de Certificado HTTPS (Cliente)
cd /d "%~dp0"
echo.
echo ============================================================
echo   SistemIA - Instalador de Certificado HTTPS (Cliente)
echo ============================================================
echo.
echo Este instalador requiere permisos de Administrador.
echo Directorio: %~dp0
echo.

:: Verificar si es administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Solicitando permisos de administrador...
    powershell -Command "Start-Process -FilePath 'cmd.exe' -ArgumentList '/c cd /d \"%~dp0\" && \"%~f0\"' -Verb RunAs"
    exit /b
)

:: Cambiar al directorio del script
cd /d "%~dp0"

:: Ejecutar script PowerShell desde el directorio correcto
powershell -NoProfile -ExecutionPolicy Bypass -Command "& {Set-Location '%~dp0'; & '%~dp0Instalar-Certificado-Cliente.ps1'}"

pause
