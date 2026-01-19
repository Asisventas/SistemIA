@echo off
echo ============================================================
echo COMPILACION DE SIFEN INTERCEPTOR
echo ============================================================
echo.

cd /d %~dp0

REM Usar MSBuild de Visual Studio si esta disponible
set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
if not exist %MSBUILD% set MSBUILD="C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
if not exist %MSBUILD% set MSBUILD="C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

REM Intentar con dotnet primero
where dotnet >nul 2>&1
if %errorlevel%==0 (
    echo Usando dotnet build...
    dotnet build -c Release
    if %errorlevel%==0 (
        echo.
        echo ============================================================
        echo COMPILACION EXITOSA
        echo ============================================================
        echo.
        echo El DLL se encuentra en: bin\Release\net472\Sifen.dll
        echo.
        echo Para instalar:
        echo 1. Respalda el DLL original en C:\nextsys - GLP\
        echo 2. Copia el nuevo Sifen.dll a esa carpeta
        echo 3. Ejecuta REGISTRAR_DLL.bat como administrador
        echo 4. Realiza una factura desde PowerBuilder
        echo 5. Revisa los archivos interceptor_*.txt en C:\nextsys - GLP\
        goto end
    )
)

echo Usando MSBuild nativo...
%MSBUILD% SifenInterceptor.csproj /p:Configuration=Release /t:Build

if %errorlevel%==0 (
    echo.
    echo ============================================================
    echo COMPILACION EXITOSA
    echo ============================================================
) else (
    echo.
    echo ERROR: No se pudo compilar
)

:end
pause
