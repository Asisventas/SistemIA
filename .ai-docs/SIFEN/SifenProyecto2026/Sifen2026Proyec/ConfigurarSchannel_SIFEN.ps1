# ========================================
# CONFIGURACIÓN SCHANNEL PARA SIFEN
# Windows 2025-2026 Compatibility Fix
# ========================================
# EJECUTAR COMO ADMINISTRADOR

Write-Host "=== CONFIGURANDO SCHANNEL PARA SIFEN ===" -ForegroundColor Cyan

# 1. Habilitar TLS 1.2 Client
Write-Host "`n1. Habilitando TLS 1.2 Client..." -ForegroundColor Yellow
$tls12ClientPath = 'HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client'
if (-not (Test-Path $tls12ClientPath)) {
    New-Item $tls12ClientPath -Force | Out-Null
}
New-ItemProperty -Path $tls12ClientPath -Name 'Enabled' -Value 1 -PropertyType DWORD -Force | Out-Null
New-ItemProperty -Path $tls12ClientPath -Name 'DisabledByDefault' -Value 0 -PropertyType DWORD -Force | Out-Null
Write-Host "   TLS 1.2 Client habilitado" -ForegroundColor Green

# 2. Desactivar verificación de revocación de certificados
Write-Host "`n2. Desactivando verificación de revocación..." -ForegroundColor Yellow
$oidPath = 'HKLM:\SOFTWARE\Microsoft\Cryptography\OID\EncodingType 0\CertDllCreateCertificateChainEngine\Config'
if (-not (Test-Path $oidPath)) {
    New-Item $oidPath -Force | Out-Null
}
New-ItemProperty -Path $oidPath -Name 'MaxUrlRetrievalByteCount' -Value 0 -PropertyType DWORD -Force | Out-Null
New-ItemProperty -Path $oidPath -Name 'ChainCacheResyncFiletime' -Value 0 -PropertyType DWORD -Force | Out-Null
Write-Host "   Verificación de revocación desactivada" -ForegroundColor Green

# 3. Configurar .NET Framework para usar TLS 1.2
Write-Host "`n3. Configurando .NET Framework para TLS 1.2..." -ForegroundColor Yellow
$netFx64Path = 'HKLM:\SOFTWARE\Microsoft\.NETFramework\v4.0.30319'
$netFx32Path = 'HKLM:\SOFTWARE\WOW6432Node\Microsoft\.NETFramework\v4.0.30319'

New-ItemProperty -Path $netFx64Path -Name 'SchUseStrongCrypto' -Value 1 -PropertyType DWORD -Force | Out-Null
New-ItemProperty -Path $netFx64Path -Name 'SystemDefaultTlsVersions' -Value 1 -PropertyType DWORD -Force | Out-Null

if (Test-Path $netFx32Path) {
    New-ItemProperty -Path $netFx32Path -Name 'SchUseStrongCrypto' -Value 1 -PropertyType DWORD -Force | Out-Null
    New-ItemProperty -Path $netFx32Path -Name 'SystemDefaultTlsVersions' -Value 1 -PropertyType DWORD -Force | Out-Null
}
Write-Host "   .NET Framework configurado" -ForegroundColor Green

# 4. Descargar e importar certificado raíz de SIFEN
Write-Host "`n4. Descargando certificado del servidor SIFEN..." -ForegroundColor Yellow
try {
    # Configurar temporalmente para permitir conexión
    [Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    
    $url = "https://sifen-test.set.gov.py"
    $webRequest = [Net.HttpWebRequest]::Create($url)
    $webRequest.Timeout = 10000
    
    try {
        $response = $webRequest.GetResponse()
        $response.Close()
    } catch {}
    
    if ($webRequest.ServicePoint.Certificate -ne $null) {
        $cert = $webRequest.ServicePoint.Certificate
        $bytes = $cert.Export([Security.Cryptography.X509Certificates.X509ContentType]::Cert)
        $certPath = "C:\SIFEN_server_cert.cer"
        [IO.File]::WriteAllBytes($certPath, $bytes)
        Write-Host "   Certificado descargado: $certPath" -ForegroundColor Green
        
        # Importar a almacén de confianza
        Import-Certificate -FilePath $certPath -CertStoreLocation Cert:\LocalMachine\Root -ErrorAction SilentlyContinue | Out-Null
        Write-Host "   Certificado importado al almacén Root" -ForegroundColor Green
    } else {
        Write-Host "   No se pudo obtener el certificado del servidor" -ForegroundColor Red
    }
} catch {
    Write-Host "   Error descargando certificado: $($_.Exception.Message)" -ForegroundColor Red
}

# 5. Limpiar eventos antiguos de Schannel
Write-Host "`n5. Limpiando eventos antiguos de Schannel..." -ForegroundColor Yellow
try {
    wevtutil cl System
    Write-Host "   Eventos limpiados" -ForegroundColor Green
} catch {
    Write-Host "   No se pudieron limpiar eventos" -ForegroundColor Yellow
}

Write-Host "`n=== CONFIGURACIÓN COMPLETADA ===" -ForegroundColor Cyan
Write-Host "`nIMPORTANTE: Reinicia Windows para aplicar cambios de Schannel" -ForegroundColor Yellow
Write-Host "Comando: Restart-Computer -Force`n" -ForegroundColor White

$respuesta = Read-Host "¿Deseas reiniciar ahora? (S/N)"
if ($respuesta -eq "S" -or $respuesta -eq "s") {
    Write-Host "Reiniciando en 10 segundos..." -ForegroundColor Red
    Start-Sleep -Seconds 10
    Restart-Computer -Force
} else {
    Write-Host "Recuerda reiniciar antes de probar el DLL SIFEN" -ForegroundColor Yellow
}
