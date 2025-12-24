@echo off
chcp 65001 >nul
echo.
echo ══════════════════════════════════════════════════════════════════
echo         GENERADOR DE PAQUETE DE ACTUALIZACION - SistemIA
echo ══════════════════════════════════════════════════════════════════
echo.

set /p VERSION="Ingrese la version del paquete (ej: 1.2.0): "

if "%VERSION%"=="" (
    echo [ERROR] Debe ingresar una version
    pause
    exit /b 1
)

echo.
echo Generando paquete version %VERSION%...
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0Crear-Paquete-Actualizacion.ps1" -Version "%VERSION%"

pause
