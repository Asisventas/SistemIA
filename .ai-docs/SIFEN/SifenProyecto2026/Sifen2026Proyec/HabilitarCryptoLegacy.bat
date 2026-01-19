@echo off
:: ============================================
:: Script para habilitar algoritmos criptográficos legacy
:: Necesario para Windows con actualizaciones 2025-2026
:: Ejecutar como Administrador
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
echo  Habilitando algoritmos criptograficos legacy
echo ============================================
echo.

:: Habilitar TLS 1.0
echo Habilitando TLS 1.0...
reg add "HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.0\Client" /v "Enabled" /t REG_DWORD /d 1 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.0\Client" /v "DisabledByDefault" /t REG_DWORD /d 0 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.0\Server" /v "Enabled" /t REG_DWORD /d 1 /f

:: Habilitar TLS 1.1
echo Habilitando TLS 1.1...
reg add "HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.1\Client" /v "Enabled" /t REG_DWORD /d 1 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.1\Client" /v "DisabledByDefault" /t REG_DWORD /d 0 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.1\Server" /v "Enabled" /t REG_DWORD /d 1 /f

:: Asegurar TLS 1.2 habilitado
echo Asegurando TLS 1.2...
reg add "HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client" /v "Enabled" /t REG_DWORD /d 1 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client" /v "DisabledByDefault" /t REG_DWORD /d 0 /f

:: Habilitar SHA1 para firmas (puede estar deshabilitado en Windows actualizado)
echo Habilitando SHA1 para firmas...
reg add "HKLM\SOFTWARE\Microsoft\Cryptography\OID\EncodingType 0\CertDllCreateCertificateChainEngine\Config" /v "WeakSignatureHashAlgorithms" /t REG_MULTI_SZ /d "SHA1" /f 2>nul

:: Habilitar algoritmos legacy en .NET
echo Configurando .NET Framework...
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework\v4.0.30319" /v "SchUseStrongCrypto" /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319" /v "SchUseStrongCrypto" /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Microsoft\.NETFramework\v4.0.30319" /v "SystemDefaultTlsVersions" /t REG_DWORD /d 1 /f
reg add "HKLM\SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.0.30319" /v "SystemDefaultTlsVersions" /t REG_DWORD /d 1 /f

:: Habilitar 3DES (Triple DES) - puede estar deshabilitado
echo Habilitando 3DES...
reg add "HKLM\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Ciphers\Triple DES 168" /v "Enabled" /t REG_DWORD /d 4294967295 /f 2>nul

:: Habilitar RSA con claves legacy
echo Configurando RSA...
reg add "HKLM\SOFTWARE\Microsoft\Cryptography\Defaults\Provider\Microsoft Enhanced RSA and AES Cryptographic Provider" /v "Image Path" /t REG_SZ /d "rsaenh.dll" /f 2>nul

:: Variable de entorno para legacy CAS policy
echo Configurando variable de entorno...
setx COMPlus_UseLegacyCasPolicyBehavior 1 /M >nul 2>&1

echo.
echo ============================================
echo  Configuracion completada
echo ============================================
echo.
echo IMPORTANTE: Es necesario REINICIAR el equipo
echo para que los cambios surtan efecto.
echo.
pause
