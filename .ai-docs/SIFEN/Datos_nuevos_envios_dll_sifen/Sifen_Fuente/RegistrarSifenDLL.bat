@echo off
:: ============================================
:: Script para registrar Sifen.dll para COM
:: Se auto-eleva a Administrador
:: ============================================

:: Verificar si ya somos administrador
net session >nul 2>&1
if %errorLevel% == 0 (
    goto :run
) else (
    goto :elevate
)

:elevate
echo Solicitando permisos de administrador...
powershell -Command "Start-Process '%~f0' -Verb RunAs"
exit /b

:run
echo ============================================
echo  Registro de Sifen.dll para COM Interop
echo ============================================
echo.

set "DLL_PATH=C:\nextsys - GLP\Sifen.dll"

if not exist "%DLL_PATH%" (
    echo ERROR: No se encontro el archivo: %DLL_PATH%
    pause
    exit /b 1
)

echo DLL encontrado: %DLL_PATH%
echo.

:: Registrar para 64 bits
echo Registrando para 64 bits...
"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe" "%DLL_PATH%" /codebase
if %errorLevel% == 0 (
    echo Registro 64 bits: EXITOSO
) else (
    echo Registro 64 bits: ERROR
)
echo.

:: Registrar para 32 bits
echo Registrando para 32 bits...
"C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe" "%DLL_PATH%" /codebase
if %errorLevel% == 0 (
    echo Registro 32 bits: EXITOSO
) else (
    echo Registro 32 bits: ERROR
)
echo.

echo ============================================
echo  Proceso completado
echo ============================================
echo.
echo Ahora puede abrir PowerBuilder y probar el DLL
echo.
pause
