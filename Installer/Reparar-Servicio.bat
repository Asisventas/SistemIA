@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo.
echo ======================================================================
echo        REPARACION DE SERVICIO SISTEMIA
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

set "INSTALL_PATH=C:\SistemIA"
set "SERVICE_NAME=Sistema de Gestión Empresarial SistemIA"
set "SERVICE_NAME_SHORT=SistemIA"
set "EXE_PATH=%INSTALL_PATH%\SistemIA.exe"

:: Verificar que existe el ejecutable
if not exist "%EXE_PATH%" (
    echo [ERROR] No se encuentra el ejecutable: %EXE_PATH%
    pause
    exit /b 1
)

echo [INFO] Ejecutable: %EXE_PATH%
echo.

:: ============================================================
:: PASO 1: Detener y eliminar servicios existentes
:: ============================================================
echo [1/3] Eliminando servicios existentes...

:: Detener procesos
taskkill /F /IM SistemIA.exe >nul 2>&1
timeout /t 2 /nobreak >nul

:: Cerrar el administrador de servicios si está abierto (puede bloquear la eliminación)
taskkill /F /IM mmc.exe >nul 2>&1
taskkill /F /IM services.msc >nul 2>&1

:: Eliminar servicio con nombre largo
sc query "%SERVICE_NAME%" >nul 2>&1
if %errorlevel% equ 0 (
    echo       Deteniendo servicio "%SERVICE_NAME%"...
    sc stop "%SERVICE_NAME%" >nul 2>&1
    timeout /t 5 /nobreak >nul
    sc delete "%SERVICE_NAME%" >nul 2>&1
    echo       Servicio marcado para eliminacion
)

:: Eliminar servicio con nombre corto
sc query "%SERVICE_NAME_SHORT%" >nul 2>&1
if %errorlevel% equ 0 (
    echo       Deteniendo servicio "%SERVICE_NAME_SHORT%"...
    sc stop "%SERVICE_NAME_SHORT%" >nul 2>&1
    timeout /t 5 /nobreak >nul
    sc delete "%SERVICE_NAME_SHORT%" >nul 2>&1
    echo       Servicio marcado para eliminacion
)

:: Esperar a que el servicio se elimine completamente
echo       Esperando eliminacion completa del servicio...
set /a DELETE_WAIT=0
:WAIT_DELETE
timeout /t 3 /nobreak >nul
sc query "%SERVICE_NAME_SHORT%" >nul 2>&1
if %errorlevel% neq 0 (
    echo       Servicio eliminado correctamente
    goto :CREATE_SERVICE
)
set /a DELETE_WAIT+=1
echo       Esperando... (%DELETE_WAIT%/10)
if %DELETE_WAIT% lss 10 goto :WAIT_DELETE

echo.
echo [ADVERTENCIA] El servicio aun esta pendiente de eliminacion.
echo              Esto puede ocurrir si el Administrador de Servicios esta abierto.
echo.
echo Opciones:
echo   1. Cierre el Administrador de Servicios (services.msc)
echo   2. Reinicie el equipo
echo   3. Vuelva a ejecutar este script
echo.
pause
exit /b 1

:CREATE_SERVICE

:: ============================================================
:: PASO 2: Crear nuevo servicio
:: ============================================================
:CREATE_SERVICE
echo.
echo [2/3] Creando nuevo servicio...

:: Crear el servicio correctamente
sc create "%SERVICE_NAME_SHORT%" binPath= "\"%EXE_PATH%\"" start= auto DisplayName= "%SERVICE_NAME%"

if %errorlevel% neq 0 (
    echo [ERROR] No se pudo crear el servicio
    pause
    exit /b 1
)

:: Configurar descripción
sc description "%SERVICE_NAME_SHORT%" "Servidor web SistemIA - Sistema de Gestión Empresarial"

:: Configurar recuperación (reiniciar en caso de fallo)
sc failure "%SERVICE_NAME_SHORT%" reset= 86400 actions= restart/5000/restart/10000/restart/30000

echo       Servicio creado correctamente

:: ============================================================
:: PASO 3: Iniciar servicio
:: ============================================================
echo.
echo [3/3] Iniciando servicio...

sc start "%SERVICE_NAME_SHORT%" >nul 2>&1

:: Esperar inicio
set /a WAIT_COUNT=0
:WAIT_START
timeout /t 3 /nobreak >nul
sc query "%SERVICE_NAME_SHORT%" | find "RUNNING" >nul 2>&1
if %errorlevel% equ 0 (
    echo       Servicio iniciado correctamente
    goto :DONE
)

sc query "%SERVICE_NAME_SHORT%" | find "STOPPED" >nul 2>&1
if %errorlevel% equ 0 (
    echo       [ERROR] El servicio no pudo iniciar
    echo.
    echo       Ejecute manualmente para ver el error:
    echo       cd %INSTALL_PATH%
    echo       SistemIA.exe
    goto :DONE
)

set /a WAIT_COUNT+=1
echo       Esperando... (%WAIT_COUNT%/20)
if %WAIT_COUNT% lss 20 goto :WAIT_START

echo       [ADVERTENCIA] Timeout esperando inicio

:DONE
echo.
echo ======================================================================
echo        REPARACION COMPLETADA
echo ======================================================================
echo.
echo Nombre del servicio: %SERVICE_NAME_SHORT%
echo Nombre visible: %SERVICE_NAME%
echo.
echo Para verificar el estado:
echo   sc query %SERVICE_NAME_SHORT%
echo.
echo Para ver logs de la aplicacion:
echo   %INSTALL_PATH%\Logs
echo.

pause
