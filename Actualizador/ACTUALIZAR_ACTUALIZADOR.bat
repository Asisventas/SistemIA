@echo off
chcp 65001 >nul
echo ============================================
echo  Actualizar SistemIA.Actualizador
echo ============================================
echo.

REM Verificar si se ejecuta como Administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Este script debe ejecutarse como Administrador
    echo.
    echo Haga clic derecho en el archivo y seleccione "Ejecutar como administrador"
    pause
    exit /b 1
)

echo [1/4] Deteniendo servicio SistemIA.Actualizador...
sc stop "SistemIA.Actualizador" >nul 2>&1
timeout /t 3 /nobreak >nul

echo [2/4] Copiando nuevos archivos...
if exist "c:\asis\SistemIA.Actualizador\publish_temp" (
    xcopy "c:\asis\SistemIA.Actualizador\publish_temp\*.*" "c:\asis\SistemIA\Actualizador\" /E /Y /Q
    echo    Archivos copiados exitosamente
) else (
    echo ERROR: No se encontró la carpeta publish_temp
    echo Primero compile el proyecto Actualizador
    pause
    exit /b 1
)

echo [3/4] Iniciando servicio...
sc start "SistemIA.Actualizador" >nul 2>&1
timeout /t 2 /nobreak >nul

echo [4/4] Verificando estado...
sc query "SistemIA.Actualizador" | findstr "RUNNING" >nul
if %errorLevel% equ 0 (
    echo.
    echo ============================================
    echo  Actualizacion completada exitosamente!
    echo  El servicio esta corriendo.
    echo ============================================
) else (
    echo.
    echo ADVERTENCIA: El servicio no esta corriendo.
    echo Verifique manualmente con: sc query "SistemIA.Actualizador"
)

echo.
pause
