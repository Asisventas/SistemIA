@echo off
echo ============================================================
echo REGISTRO DEL DLL SIFEN INTERCEPTOR
echo Ejecutar como ADMINISTRADOR
echo ============================================================
echo.

set DLLPATH="%~dp0bin\Release\net472\Sifen.dll"

echo Registrando DLL para 64 bits...
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe %DLLPATH% /codebase /tlb
if %errorlevel%==0 (
    echo [OK] Registro 64 bits EXITOSO
) else (
    echo [ERROR] Fallo registro 64 bits
)

echo.
echo Registrando DLL para 32 bits...
C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm.exe %DLLPATH% /codebase /tlb
if %errorlevel%==0 (
    echo [OK] Registro 32 bits EXITOSO
) else (
    echo [ERROR] Fallo registro 32 bits
)

echo.
echo ============================================================
echo Proceso completado
echo ============================================================
echo.
echo Archivos de intercepcion se guardaran en: C:\nextsys - GLP\
echo - interceptor_log.txt       : Log general
echo - interceptor_entrada.json  : Parametros recibidos
echo - interceptor_xml_input.txt : XML de entrada completo
echo - interceptor_xml_firmado.txt : XML despues de firmar
echo - interceptor_soap_completo.txt : SOAP enviado a SIFEN
echo - interceptor_respuesta.txt : Respuesta de SIFEN
echo - interceptor_retorno.json  : JSON retornado a PowerBuilder
echo.
pause
