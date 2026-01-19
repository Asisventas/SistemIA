@echo off
REM ============================================
REM Registrar Sifen.dll para COM
REM Ejecutar como Administrador
REM ============================================

echo ============================================
echo  Registro de Sifen.dll - Version Corregida
echo  Compatible con Windows 10/11
echo ============================================
echo.

REM Verificar si se ejecuta como administrador
net session >nul 2>&1
if %errorLevel% NEQ 0 (
    echo ERROR: Este script debe ejecutarse como Administrador
    echo Haga clic derecho en el archivo y seleccione "Ejecutar como administrador"
    echo.
    pause
    exit /b 1
)

REM Obtener la ruta del script
set "SCRIPT_DIR=%~dp0"
set "DLL_PATH=%SCRIPT_DIR%Sifen.dll"

REM Verificar que el DLL existe
if not exist "%DLL_PATH%" (
    echo ERROR: No se encontro Sifen.dll en: %DLL_PATH%
    echo.
    echo Asegurese de que Sifen.dll este en la misma carpeta que este script
    echo.
    pause
    exit /b 1
)

echo DLL encontrado: %DLL_PATH%
echo.

REM Registrar para 64 bits
echo Registrando para 64 bits...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe "%DLL_PATH%" /codebase /tlb
if %errorLevel% EQU 0 (
    echo [OK] Registro 64 bits EXITOSO
) else (
    echo [ERROR] Fallo el registro 64 bits
)
echo.

REM Registrar para 32 bits
echo Registrando para 32 bits...
C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe "%DLL_PATH%" /codebase /tlb
if %errorLevel% EQU 0 (
    echo [OK] Registro 32 bits EXITOSO
) else (
    echo [ERROR] Fallo el registro 32 bits
)
echo.

echo ============================================
echo  Proceso completado
echo ============================================
echo.
echo El DLL esta listo para usarse desde:
echo  - PowerBuilder
echo  - VB6/VBA
echo  - Blazor Server C#
echo  - Cualquier aplicacion COM
echo.
echo IMPORTANTE: Esta version usa RSA moderna
echo Compatible con equipos nuevos sin error de proveedor
echo.
pause
