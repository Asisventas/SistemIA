@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

:MENU
cls
echo.
echo ╔═══════════════════════════════════════════════════════════════╗
echo ║           SISTEMIA - HERRAMIENTA DE DIAGNOSTICO              ║
echo ╚═══════════════════════════════════════════════════════════════╝
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
set "SERVICE_NAME_LONG=Sistema de Gestión Empresarial SistemIA"
set "SERVICE_NAME_SHORT=SistemIA"

echo ┌──────────────────────────────────────────────────────────────┐
echo │                    MENU DE DIAGNOSTICO                       │
echo ├──────────────────────────────────────────────────────────────┤
echo │  1. Diagnostico completo (recomendado)                       │
echo │  2. Verificar archivos de la aplicacion                      │
echo │  3. Verificar servicio Windows                               │
echo │  4. Verificar conexion a base de datos                       │
echo │  5. Ver logs de la aplicacion                                │
echo │  6. Ver eventos de Windows                                   │
echo │  7. Probar ejecucion manual                                  │
echo │  8. Reparar servicio Windows                                 │
echo │  9. Ver configuracion actual (appsettings.json)              │
echo │ 10. Verificar puertos en uso                                 │
echo │  0. Salir                                                    │
echo └──────────────────────────────────────────────────────────────┘
echo.

set /p opcion="Seleccione una opcion: "

if "%opcion%"=="1" goto :DIAG_COMPLETO
if "%opcion%"=="2" goto :VERIFICAR_ARCHIVOS
if "%opcion%"=="3" goto :VERIFICAR_SERVICIO
if "%opcion%"=="4" goto :VERIFICAR_BD
if "%opcion%"=="5" goto :VER_LOGS
if "%opcion%"=="6" goto :VER_EVENTOS
if "%opcion%"=="7" goto :EJECUCION_MANUAL
if "%opcion%"=="8" goto :REPARAR_SERVICIO
if "%opcion%"=="9" goto :VER_CONFIG
if "%opcion%"=="10" goto :VERIFICAR_PUERTOS
if "%opcion%"=="0" exit /b 0

echo Opcion no valida
timeout /t 2 >nul
goto :MENU

:: ============================================================
:: DIAGNOSTICO COMPLETO
:: ============================================================
:DIAG_COMPLETO
cls
echo.
echo ══════════════════════════════════════════════════════════════
echo                  DIAGNOSTICO COMPLETO DE SISTEMIA
echo ══════════════════════════════════════════════════════════════
echo.

echo [1/6] VERIFICANDO ARCHIVOS...
echo ────────────────────────────────────────────────────────────────
if exist "%INSTALL_PATH%\SistemIA.exe" (
    echo   [OK] Ejecutable encontrado
    for %%A in ("%INSTALL_PATH%\SistemIA.exe") do echo        Tamano: %%~zA bytes
) else (
    echo   [ERROR] Ejecutable NO encontrado: %INSTALL_PATH%\SistemIA.exe
)

if exist "%INSTALL_PATH%\appsettings.json" (
    echo   [OK] appsettings.json encontrado
) else (
    echo   [ERROR] appsettings.json NO encontrado
)

if exist "%INSTALL_PATH%\wwwroot" (
    echo   [OK] Carpeta wwwroot encontrada
) else (
    echo   [AVISO] Carpeta wwwroot NO encontrada
)

echo.
echo [2/6] VERIFICANDO SERVICIO WINDOWS...
echo ────────────────────────────────────────────────────────────────
set "FOUND_SVC="
sc query "%SERVICE_NAME_SHORT%" >nul 2>&1
if %errorlevel% equ 0 (
    set "FOUND_SVC=%SERVICE_NAME_SHORT%"
) else (
    sc query "%SERVICE_NAME_LONG%" >nul 2>&1
    if !errorlevel! equ 0 set "FOUND_SVC=%SERVICE_NAME_LONG%"
)

if defined FOUND_SVC (
    echo   [OK] Servicio encontrado: !FOUND_SVC!
    for /f "tokens=3" %%a in ('sc query "!FOUND_SVC!" ^| findstr "STATE"') do (
        echo        Estado: %%a
        if "%%a"=="4" echo        [RUNNING - En ejecucion]
        if "%%a"=="1" echo        [STOPPED - Detenido]
    )
    sc qc "!FOUND_SVC!" 2>nul | findstr "BINARY_PATH_NAME"
) else (
    echo   [AVISO] Servicio NO encontrado
)

echo.
echo [3/6] VERIFICANDO PROCESO...
echo ────────────────────────────────────────────────────────────────
tasklist /FI "IMAGENAME eq SistemIA.exe" 2>nul | findstr "SistemIA" >nul
if %errorlevel% equ 0 (
    echo   [OK] Proceso SistemIA.exe en ejecucion
    for /f "tokens=2" %%a in ('tasklist /FI "IMAGENAME eq SistemIA.exe" ^| findstr "SistemIA"') do echo        PID: %%a
) else (
    echo   [INFO] Proceso SistemIA.exe NO esta corriendo
)

echo.
echo [4/6] VERIFICANDO CONEXION A SQL SERVER...
echo ────────────────────────────────────────────────────────────────
powershell -NoProfile -Command "$cs = (Get-Content '%INSTALL_PATH%\appsettings.json' -Raw | ConvertFrom-Json).ConnectionStrings.DefaultConnection; if ($cs) { $cs -match 'Server=([^;]+)' | Out-Null; $server = $matches[1]; $cs -match 'Database=([^;]+)' | Out-Null; $db = $matches[1]; Write-Host \"  Servidor: $server\"; Write-Host \"  Base de datos: $db\"; try { $conn = New-Object System.Data.SqlClient.SqlConnection($cs); $conn.Open(); Write-Host '  [OK] Conexion exitosa' -ForegroundColor Green; $conn.Close() } catch { Write-Host \"  [ERROR] No se pudo conectar: $($_.Exception.Message)\" -ForegroundColor Red } } else { Write-Host '  [ERROR] No se encontro cadena de conexion' -ForegroundColor Red }"

echo.
echo [5/6] VERIFICANDO PUERTOS...
echo ────────────────────────────────────────────────────────────────
powershell -NoProfile -Command "$settings = Get-Content '%INSTALL_PATH%\appsettings.json' -Raw | ConvertFrom-Json; $urls = $settings.Kestrel.Endpoints.Http.Url, $settings.Kestrel.Endpoints.Https.Url; foreach ($url in $urls) { if ($url) { $url -match ':(\d+)' | Out-Null; $port = $matches[1]; $inUse = netstat -an | Select-String \":$port\s\" | Select-String 'LISTENING'; if ($inUse) { Write-Host \"  Puerto $port : EN USO\" } else { Write-Host \"  Puerto $port : Disponible\" } } }"

echo.
echo [6/6] VERIFICANDO LOGS RECIENTES...
echo ────────────────────────────────────────────────────────────────
if exist "%INSTALL_PATH%\Logs" (
    echo   Carpeta de logs encontrada
    for /f "delims=" %%F in ('dir /b /o-d "%INSTALL_PATH%\Logs\*.log" 2^>nul') do (
        echo   Ultimo log: %%F
        goto :LOGS_FOUND
    )
    echo   [INFO] No hay archivos de log
    :LOGS_FOUND
) else (
    echo   [INFO] Carpeta de Logs no existe aun
)

echo.
echo ══════════════════════════════════════════════════════════════
echo                    DIAGNOSTICO FINALIZADO
echo ══════════════════════════════════════════════════════════════
echo.
pause
goto :MENU

:: ============================================================
:: VERIFICAR ARCHIVOS
:: ============================================================
:VERIFICAR_ARCHIVOS
cls
echo.
echo ══════════════════════════════════════════════════════════════
echo                VERIFICACION DE ARCHIVOS
echo ══════════════════════════════════════════════════════════════
echo.
echo Carpeta de instalacion: %INSTALL_PATH%
echo.

if not exist "%INSTALL_PATH%" (
    echo [ERROR] La carpeta de instalacion no existe
    pause
    goto :MENU
)

echo Archivos principales:
echo ────────────────────────────────────────────────────────────────
for %%F in (SistemIA.exe appsettings.json web.config SistemIA.dll SistemIA.runtimeconfig.json) do (
    if exist "%INSTALL_PATH%\%%F" (
        for %%A in ("%INSTALL_PATH%\%%F") do echo   [OK] %%F (%%~zA bytes)
    ) else (
        echo   [--] %%F (no encontrado)
    )
)

echo.
echo Carpetas importantes:
echo ────────────────────────────────────────────────────────────────
for %%D in (wwwroot Logs certificados) do (
    if exist "%INSTALL_PATH%\%%D" (
        echo   [OK] %%D\
    ) else (
        echo   [--] %%D\ (no encontrada)
    )
)

echo.
echo DLLs de Entity Framework:
echo ────────────────────────────────────────────────────────────────
dir /b "%INSTALL_PATH%\Microsoft.EntityFrameworkCore*.dll" 2>nul
if %errorlevel% neq 0 echo   (ninguna encontrada)

echo.
pause
goto :MENU

:: ============================================================
:: VERIFICAR SERVICIO
:: ============================================================
:VERIFICAR_SERVICIO
cls
echo.
echo ══════════════════════════════════════════════════════════════
echo                VERIFICACION DE SERVICIO WINDOWS
echo ══════════════════════════════════════════════════════════════
echo.

echo Buscando servicios SistemIA...
echo.

sc query "%SERVICE_NAME_SHORT%" >nul 2>&1
if %errorlevel% equ 0 (
    echo [ENCONTRADO] Servicio: %SERVICE_NAME_SHORT%
    echo.
    sc query "%SERVICE_NAME_SHORT%"
    echo.
    echo Configuracion del servicio:
    sc qc "%SERVICE_NAME_SHORT%"
) else (
    sc query "%SERVICE_NAME_LONG%" >nul 2>&1
    if !errorlevel! equ 0 (
        echo [ENCONTRADO] Servicio: %SERVICE_NAME_LONG%
        echo.
        sc query "%SERVICE_NAME_LONG%"
    ) else (
        echo [NO ENCONTRADO] No hay servicio de SistemIA instalado
        echo.
        echo Para instalar el servicio, use el script Reparar-Servicio.bat
    )
)

echo.
pause
goto :MENU

:: ============================================================
:: VERIFICAR BD
:: ============================================================
:VERIFICAR_BD
cls
echo.
echo ══════════════════════════════════════════════════════════════
echo             VERIFICACION DE CONEXION A BASE DE DATOS
echo ══════════════════════════════════════════════════════════════
echo.

if not exist "%INSTALL_PATH%\appsettings.json" (
    echo [ERROR] No se encontro appsettings.json
    pause
    goto :MENU
)

powershell -NoProfile -Command ^
    "$json = Get-Content '%INSTALL_PATH%\appsettings.json' -Raw | ConvertFrom-Json; " ^
    "$cs = $json.ConnectionStrings.DefaultConnection; " ^
    "Write-Host 'Cadena de conexion:' -ForegroundColor Cyan; " ^
    "Write-Host $cs; " ^
    "Write-Host ''; " ^
    "Write-Host 'Probando conexion...' -ForegroundColor Yellow; " ^
    "try { " ^
    "    $conn = New-Object System.Data.SqlClient.SqlConnection($cs); " ^
    "    $conn.Open(); " ^
    "    Write-Host '[OK] Conexion exitosa!' -ForegroundColor Green; " ^
    "    $cmd = $conn.CreateCommand(); " ^
    "    $cmd.CommandText = 'SELECT @@VERSION'; " ^
    "    $version = $cmd.ExecuteScalar(); " ^
    "    Write-Host ''; " ^
    "    Write-Host 'Version SQL Server:' -ForegroundColor Cyan; " ^
    "    Write-Host $version.Substring(0,80); " ^
    "    $conn.Close(); " ^
    "} catch { " ^
    "    Write-Host \"[ERROR] $($_.Exception.Message)\" -ForegroundColor Red; " ^
    "}"

echo.
pause
goto :MENU

:: ============================================================
:: VER LOGS
:: ============================================================
:VER_LOGS
cls
echo.
echo ══════════════════════════════════════════════════════════════
echo                    LOGS DE LA APLICACION
echo ══════════════════════════════════════════════════════════════
echo.

set "LOG_PATH=%INSTALL_PATH%\Logs"

if not exist "%LOG_PATH%" (
    echo [INFO] La carpeta de Logs no existe: %LOG_PATH%
    echo        Se creara automaticamente cuando la aplicacion inicie.
    pause
    goto :MENU
)

echo Archivos de log encontrados:
echo ────────────────────────────────────────────────────────────────
dir /b /o-d "%LOG_PATH%\*.log" 2>nul
if %errorlevel% neq 0 (
    echo   (ninguno)
    pause
    goto :MENU
)

echo.
set /p "ver_log=Desea ver el ultimo log? (S/N): "
if /I "%ver_log%"=="S" (
    for /f "delims=" %%F in ('dir /b /o-d "%LOG_PATH%\*.log" 2^>nul') do (
        echo.
        echo Contenido de %%F (ultimas 50 lineas):
        echo ────────────────────────────────────────────────────────────────
        powershell -Command "Get-Content '%LOG_PATH%\%%F' -Tail 50"
        goto :LOG_SHOWN
    )
    :LOG_SHOWN
)

echo.
pause
goto :MENU

:: ============================================================
:: VER EVENTOS WINDOWS
:: ============================================================
:VER_EVENTOS
cls
echo.
echo ══════════════════════════════════════════════════════════════
echo              EVENTOS DE WINDOWS (ultimas 24 horas)
echo ══════════════════════════════════════════════════════════════
echo.

echo Buscando eventos relacionados con SistemIA y .NET...
echo.

powershell -NoProfile -Command ^
    "$after = (Get-Date).AddDays(-1); " ^
    "Write-Host 'Errores de Aplicacion:' -ForegroundColor Cyan; " ^
    "Get-WinEvent -FilterHashtable @{LogName='Application'; Level=2; StartTime=$after} -MaxEvents 10 -ErrorAction SilentlyContinue | " ^
    "Where-Object { $_.Message -match 'SistemIA|\.NET|ASP\.NET|Kestrel' } | " ^
    "ForEach-Object { Write-Host \"[$($_.TimeCreated)] $($_.Message.Substring(0,[Math]::Min(200,$_.Message.Length)))...\" -ForegroundColor Yellow }; " ^
    "Write-Host ''; " ^
    "Write-Host 'Eventos del Sistema:' -ForegroundColor Cyan; " ^
    "Get-WinEvent -FilterHashtable @{LogName='System'; Level=2,3; StartTime=$after} -MaxEvents 5 -ErrorAction SilentlyContinue | " ^
    "Where-Object { $_.Message -match 'SistemIA|servicio' } | " ^
    "ForEach-Object { Write-Host \"[$($_.TimeCreated)] $($_.Message.Substring(0,[Math]::Min(200,$_.Message.Length)))...\" -ForegroundColor Yellow }"

echo.
pause
goto :MENU

:: ============================================================
:: EJECUCION MANUAL
:: ============================================================
:EJECUCION_MANUAL
cls
echo.
echo ══════════════════════════════════════════════════════════════
echo                   EJECUCION MANUAL
echo ══════════════════════════════════════════════════════════════
echo.
echo Esta opcion ejecutara SistemIA.exe directamente en la consola.
echo Esto permite ver errores de inicio en tiempo real.
echo.
echo IMPORTANTE: Si el servicio esta corriendo, detengalo primero.
echo.

set /p "continuar=Desea continuar? (S/N): "
if /I not "%continuar%"=="S" goto :MENU

:: Detener servicio si existe
sc query "%SERVICE_NAME_SHORT%" >nul 2>&1
if %errorlevel% equ 0 (
    echo Deteniendo servicio...
    sc stop "%SERVICE_NAME_SHORT%" >nul 2>&1
    timeout /t 3 /nobreak >nul
)

echo.
echo Iniciando SistemIA.exe...
echo (Presione Ctrl+C para detener)
echo ────────────────────────────────────────────────────────────────
echo.

cd /d "%INSTALL_PATH%"
"%INSTALL_PATH%\SistemIA.exe"

echo.
echo La aplicacion se detuvo.
pause
goto :MENU

:: ============================================================
:: REPARAR SERVICIO
:: ============================================================
:REPARAR_SERVICIO
cls
echo.
echo Ejecutando script de reparacion de servicio...
echo.

if exist "%~dp0Reparar-Servicio.bat" (
    call "%~dp0Reparar-Servicio.bat"
) else (
    echo [ERROR] No se encontro Reparar-Servicio.bat
    echo         Copie el archivo al mismo directorio de este script.
)

pause
goto :MENU

:: ============================================================
:: VER CONFIGURACION
:: ============================================================
:VER_CONFIG
cls
echo.
echo ══════════════════════════════════════════════════════════════
echo                 CONFIGURACION ACTUAL
echo ══════════════════════════════════════════════════════════════
echo.

if not exist "%INSTALL_PATH%\appsettings.json" (
    echo [ERROR] No se encontro appsettings.json en %INSTALL_PATH%
    pause
    goto :MENU
)

echo Contenido de appsettings.json:
echo ────────────────────────────────────────────────────────────────
type "%INSTALL_PATH%\appsettings.json"
echo.
echo ────────────────────────────────────────────────────────────────
echo.
pause
goto :MENU

:: ============================================================
:: VERIFICAR PUERTOS
:: ============================================================
:VERIFICAR_PUERTOS
cls
echo.
echo ══════════════════════════════════════════════════════════════
echo                  PUERTOS EN USO
echo ══════════════════════════════════════════════════════════════
echo.

echo Puertos comunes de SistemIA (5095, 7060):
echo ────────────────────────────────────────────────────────────────
netstat -ano | findstr ":5095 :7060" | findstr "LISTENING"
if %errorlevel% neq 0 (
    echo   Puertos 5095 y 7060 estan disponibles
)

echo.
echo Todos los puertos en LISTENING:
echo ────────────────────────────────────────────────────────────────
netstat -ano | findstr "LISTENING" | findstr "TCP"

echo.
pause
goto :MENU
