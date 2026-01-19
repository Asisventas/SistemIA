@echo off
echo ============================================================
echo INSTALACION DE SIFEN INTERCEPTOR
echo ============================================================
echo.
echo Este script:
echo 1. Respalda el DLL original
echo 2. Copia el DLL interceptor
echo 3. Registra el DLL con regasm
echo.
echo IMPORTANTE: Ejecutar como ADMINISTRADOR
echo.
pause

REM Verificar si es Administrador
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo.
    echo ERROR: Debe ejecutar como Administrador
    echo Haga click derecho -> Ejecutar como Administrador
    pause
    exit /b 1
)

set NEXTSYS_PATH=C:\nextsys - GLP
set INTERCEPTOR_PATH=%~dp0bin\Release\net472\Sifen.dll
set REGASM_PATH=C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe

echo.
echo Verificando rutas...
if not exist "%NEXTSYS_PATH%\Sifen.dll" (
    echo ERROR: No se encontro %NEXTSYS_PATH%\Sifen.dll
    pause
    exit /b 1
)

if not exist "%INTERCEPTOR_PATH%" (
    echo ERROR: No se encontro el interceptor compilado
    echo Ejecute primero: dotnet build -c Release
    pause
    exit /b 1
)

echo.
echo [PASO 1] Respaldando DLL original...
if not exist "%NEXTSYS_PATH%\Sifen_ORIGINAL.dll" (
    copy "%NEXTSYS_PATH%\Sifen.dll" "%NEXTSYS_PATH%\Sifen_ORIGINAL.dll"
    echo   -> Respaldo creado: Sifen_ORIGINAL.dll
) else (
    echo   -> Respaldo ya existe: Sifen_ORIGINAL.dll
)

echo.
echo [PASO 2] Desregistrando DLL actual...
"%REGASM_PATH%" "%NEXTSYS_PATH%\Sifen.dll" /u 2>nul
echo   -> Desregistrado

echo.
echo [PASO 3] Copiando DLL interceptor...
copy /Y "%INTERCEPTOR_PATH%" "%NEXTSYS_PATH%\Sifen.dll"
echo   -> Copiado

echo.
echo [PASO 4] Registrando DLL interceptor...
"%REGASM_PATH%" "%NEXTSYS_PATH%\Sifen.dll" /codebase /tlb
if %errorlevel% neq 0 (
    echo ERROR: Fallo al registrar el DLL
    pause
    exit /b 1
)
echo   -> Registrado OK

echo.
echo ============================================================
echo INSTALACION COMPLETADA
echo ============================================================
echo.
echo Ahora:
echo 1. Abra el sistema nextsys
echo 2. Genere una factura electronica de prueba
echo 3. Los archivos de captura estaran en: %NEXTSYS_PATH%\
echo    - interceptor_log.txt
echo    - interceptor_entrada.json
echo    - interceptor_xml_input.txt
echo    - interceptor_xml_firmado.txt
echo    - interceptor_soap_completo.txt
echo    - interceptor_respuesta.txt
echo.
echo Para restaurar el DLL original, ejecute: RESTAURAR_ORIGINAL.bat
echo.
pause
