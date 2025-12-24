@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo.
echo ======================================================================
echo              ACTUALIZACION DE SISTEMIA
echo ======================================================================
echo.

:: Verificar permisos de administrador
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Este script requiere permisos de Administrador
    echo         Clic derecho - Ejecutar como administrador
    pause
    exit /b 1
)

:: Configuracion
set "SERVICE_NAME=Sistema de Gestión Empresarial SistemIA"
set "SERVICE_NAME_SHORT=SistemIA"
set "INSTALL_PATH=C:\SistemIA"
set "BACKUP_PATH=C:\backup"
set "SCRIPT_DIR=%~dp0"

:: Verificar que existe la carpeta de instalacion
if not exist "%INSTALL_PATH%" (
    echo [ERROR] No se encuentra la carpeta de instalacion: %INSTALL_PATH%
    pause
    exit /b 1
)

echo [INFO] Carpeta de instalacion: %INSTALL_PATH%
echo [INFO] Carpeta de origen: %SCRIPT_DIR%
echo.

:: ============================================================
:: PASO 1: Crear backup
:: ============================================================
echo [1/4] Creando backup...

if not exist "%BACKUP_PATH%" mkdir "%BACKUP_PATH%"

for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set datetime=%%I
set "BACKUP_FILE=%BACKUP_PATH%\SistemIA_Backup_%datetime:~0,8%_%datetime:~8,6%.zip"

powershell -Command "Compress-Archive -Path $env:INSTALL_PATH -DestinationPath $env:BACKUP_FILE -Force" 2>nul
if %errorlevel% equ 0 (
    echo       Backup creado: %BACKUP_FILE%
) else (
    echo       [ADVERTENCIA] No se pudo crear backup, continuando...
)

:: ============================================================
:: PASO 2: Detener servicio
:: ============================================================
echo.
echo [2/4] Deteniendo servicio...

:: Intentar con el nombre completo primero
set "FOUND_SERVICE="
sc query "%SERVICE_NAME%" >nul 2>&1
if %errorlevel% equ 0 (
    set "FOUND_SERVICE=%SERVICE_NAME%"
    echo       Encontrado servicio: %SERVICE_NAME%
) else (
    :: Intentar con nombre corto
    sc query "%SERVICE_NAME_SHORT%" >nul 2>&1
    if !errorlevel! equ 0 (
        set "FOUND_SERVICE=%SERVICE_NAME_SHORT%"
        echo       Encontrado servicio: %SERVICE_NAME_SHORT%
    )
)

if defined FOUND_SERVICE (
    echo       Deteniendo servicio Windows...
    sc stop "!FOUND_SERVICE!" >nul 2>&1
    
    :: Esperar hasta que el servicio se detenga completamente (max 30 seg)
    set /a WAIT_COUNT=0
    :WAIT_STOP
    timeout /t 2 /nobreak >nul
    sc query "!FOUND_SERVICE!" | find "STOPPED" >nul 2>&1
    if %errorlevel% equ 0 (
        echo       Servicio detenido correctamente
        goto :STOP_DONE
    )
    set /a WAIT_COUNT+=1
    if %WAIT_COUNT% lss 15 goto :WAIT_STOP
    
    echo       [ADVERTENCIA] Timeout esperando servicio, forzando cierre...
    taskkill /F /IM SistemIA.exe >nul 2>&1
    timeout /t 3 /nobreak >nul
) else (
    echo       Servicio no encontrado, deteniendo procesos...
    taskkill /F /IM SistemIA.exe >nul 2>&1
    timeout /t 2 /nobreak >nul
)
:STOP_DONE

:: Esperar que se liberen archivos
echo       Esperando liberacion de archivos...
timeout /t 5 /nobreak >nul

:: ============================================================
:: PASO 3: Copiar archivos (sin appsettings.json)
:: ============================================================
echo.
echo [3/4] Actualizando archivos...

:: Respaldar appsettings.json actual
if exist "%INSTALL_PATH%\appsettings.json" (
    copy /Y "%INSTALL_PATH%\appsettings.json" "%INSTALL_PATH%\appsettings.json.bak" >nul
    echo       Configuracion respaldada
)

:: Copiar todos los archivos excepto el script .bat y appsettings
for %%F in ("%SCRIPT_DIR%*.*") do (
    set "filename=%%~nxF"
    if /I not "!filename!"=="Actualizar.bat" (
        if /I not "!filename!"=="appsettings.json" (
            if /I not "!filename!"=="appsettings.Development.json" (
                if /I not "!filename!"=="appsettings.Production.json" (
                    copy /Y "%%F" "%INSTALL_PATH%\" >nul 2>&1
                )
            )
        )
    )
)

:: Copiar subcarpetas
for /D %%D in ("%SCRIPT_DIR%*") do (
    set "dirname=%%~nxD"
    xcopy /E /Y /I /Q "%%D" "%INSTALL_PATH%\!dirname!\" >nul 2>&1
)

echo       Archivos actualizados correctamente

:: ============================================================
:: PASO 4: Iniciar servicio
:: ============================================================
echo.
echo [4/4] Iniciando servicio...

if defined FOUND_SERVICE (
    echo       Iniciando servicio: !FOUND_SERVICE!
    sc start "!FOUND_SERVICE!" >nul 2>&1
    
    :: Esperar hasta que el servicio inicie (max 60 seg)
    set /a START_COUNT=0
    :WAIT_START
    timeout /t 3 /nobreak >nul
    sc query "!FOUND_SERVICE!" | find "RUNNING" >nul 2>&1
    if %errorlevel% equ 0 (
        echo       Servicio iniciado correctamente
        goto :START_DONE
    )
    
    :: Verificar si falló
    sc query "!FOUND_SERVICE!" | find "STOPPED" >nul 2>&1
    if %errorlevel% equ 0 (
        echo       [ERROR] El servicio no pudo iniciar
        echo.
        echo       Posibles causas:
        echo       - Verifique los logs en: %INSTALL_PATH%\Logs
        echo       - Ejecute manualmente: %INSTALL_PATH%\SistemIA.exe
        echo       - Revise el Visor de eventos de Windows
        set "SERVICE_FAILED=1"
        goto :START_DONE
    )
    
    set /a START_COUNT+=1
    echo       Esperando inicio del servicio... (%START_COUNT%/20)
    if %START_COUNT% lss 20 goto :WAIT_START
    
    echo       [ADVERTENCIA] Timeout esperando inicio del servicio
    echo       El servicio puede estar iniciando en segundo plano.
    echo       Verifique el estado con: sc query "!FOUND_SERVICE!"
    set "SERVICE_FAILED=1"
) else (
    echo       [INFO] No hay servicio configurado.
    echo       Inicie la aplicacion manualmente: %INSTALL_PATH%\SistemIA.exe
)
:START_DONE

:: ============================================================
:: FINALIZADO
:: ============================================================
echo.
echo ======================================================================
echo              ACTUALIZACION COMPLETADA
echo ======================================================================
echo.
echo Las migraciones de base de datos se aplicaran automaticamente
echo cuando el servicio inicie.
echo.
echo Backup guardado en: %BACKUP_FILE%
echo.

:: Si el servicio falló, mostrar información de diagnóstico
if defined SERVICE_FAILED (
    echo ======================================================================
    echo              DIAGNOSTICO - SERVICIO NO INICIO
    echo ======================================================================
    echo.
    echo El servicio no inicio correctamente. Posibles soluciones:
    echo.
    echo 1. Ejecutar la aplicacion manualmente para ver errores:
    echo    cd %INSTALL_PATH%
    echo    SistemIA.exe
    echo.
    echo 2. Verificar logs en:
    echo    %INSTALL_PATH%\Logs
    echo.
    echo 3. Revisar el Visor de eventos de Windows:
    echo    eventvwr.msc - Registros de Windows - Aplicacion
    echo.
    echo 4. Verificar que appsettings.json tiene la configuracion correcta
    echo.
    echo 5. Si usa NSSM, reinstalar el servicio:
    echo    nssm remove %SERVICE_NAME% confirm
    echo    nssm install %SERVICE_NAME% %INSTALL_PATH%\SistemIA.exe
    echo.
)

pause
