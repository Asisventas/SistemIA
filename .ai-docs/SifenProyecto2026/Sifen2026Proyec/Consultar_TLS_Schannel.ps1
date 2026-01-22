# ========================================
# CONSULTAR CONFIGURACIÓN TLS/SCHANNEL
# Muestra estado actual de protocolos y configuraciones
# ========================================

Write-Host "`n=== ESTADO ACTUAL DE TLS/SCHANNEL ===" -ForegroundColor Cyan
Write-Host "Sistema: $env:COMPUTERNAME" -ForegroundColor Gray
Write-Host "Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n" -ForegroundColor Gray

# ========================================
# 1. PROTOCOLOS SCHANNEL
# ========================================
Write-Host "1. PROTOCOLOS SCHANNEL (Client)" -ForegroundColor Yellow
Write-Host "=" * 60

$protocols = @("SSL 2.0", "SSL 3.0", "TLS 1.0", "TLS 1.1", "TLS 1.2", "TLS 1.3")

foreach ($protocol in $protocols) {
    $clientPath = "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\$protocol\Client"
    
    if (Test-Path $clientPath) {
        $enabled = Get-ItemProperty -Path $clientPath -Name "Enabled" -ErrorAction SilentlyContinue
        $disabledByDefault = Get-ItemProperty -Path $clientPath -Name "DisabledByDefault" -ErrorAction SilentlyContinue
        
        $status = "CONFIGURADO"
        $color = "White"
        
        if ($enabled.Enabled -eq 1 -and $disabledByDefault.DisabledByDefault -eq 0) {
            $status = "HABILITADO"
            $color = "Green"
        } elseif ($enabled.Enabled -eq 0 -or $disabledByDefault.DisabledByDefault -eq 1) {
            $status = "DESHABILITADO"
            $color = "Red"
        }
        
        Write-Host "  $protocol : " -NoNewline
        Write-Host $status -ForegroundColor $color
        Write-Host "    Enabled=$($enabled.Enabled), DisabledByDefault=$($disabledByDefault.DisabledByDefault)" -ForegroundColor Gray
    } else {
        Write-Host "  $protocol : " -NoNewline
        Write-Host "NO CONFIGURADO (usa default Windows)" -ForegroundColor Yellow
    }
}

# ========================================
# 2. CONFIGURACIÓN .NET FRAMEWORK
# ========================================
Write-Host "`n2. CONFIGURACIÓN .NET FRAMEWORK" -ForegroundColor Yellow
Write-Host "=" * 60

$netFx64Path = 'HKLM:\SOFTWARE\Microsoft\.NETFramework\v4.0.30319'
$netFx32Path = 'HKLM:\SOFTWARE\WOW6432Node\Microsoft\.NETFramework\v4.0.30319'

Write-Host "  .NET Framework 4.0+ (64-bits):" -ForegroundColor White
if (Test-Path $netFx64Path) {
    $strongCrypto = Get-ItemProperty -Path $netFx64Path -Name "SchUseStrongCrypto" -ErrorAction SilentlyContinue
    $systemDefault = Get-ItemProperty -Path $netFx64Path -Name "SystemDefaultTlsVersions" -ErrorAction SilentlyContinue
    
    Write-Host "    SchUseStrongCrypto: " -NoNewline
    if ($strongCrypto.SchUseStrongCrypto -eq 1) {
        Write-Host "HABILITADO" -ForegroundColor Green
    } else {
        Write-Host "DESHABILITADO" -ForegroundColor Red
    }
    
    Write-Host "    SystemDefaultTlsVersions: " -NoNewline
    if ($systemDefault.SystemDefaultTlsVersions -eq 1) {
        Write-Host "HABILITADO" -ForegroundColor Green
    } else {
        Write-Host "DESHABILITADO" -ForegroundColor Red
    }
} else {
    Write-Host "    NO CONFIGURADO" -ForegroundColor Yellow
}

Write-Host "`n  .NET Framework 4.0+ (32-bits):" -ForegroundColor White
if (Test-Path $netFx32Path) {
    $strongCrypto32 = Get-ItemProperty -Path $netFx32Path -Name "SchUseStrongCrypto" -ErrorAction SilentlyContinue
    $systemDefault32 = Get-ItemProperty -Path $netFx32Path -Name "SystemDefaultTlsVersions" -ErrorAction SilentlyContinue
    
    Write-Host "    SchUseStrongCrypto: " -NoNewline
    if ($strongCrypto32.SchUseStrongCrypto -eq 1) {
        Write-Host "HABILITADO" -ForegroundColor Green
    } else {
        Write-Host "DESHABILITADO" -ForegroundColor Red
    }
    
    Write-Host "    SystemDefaultTlsVersions: " -NoNewline
    if ($systemDefault32.SystemDefaultTlsVersions -eq 1) {
        Write-Host "HABILITADO" -ForegroundColor Green
    } else {
        Write-Host "DESHABILITADO" -ForegroundColor Red
    }
} else {
    Write-Host "    NO APLICA (sistema 32-bit)" -ForegroundColor Gray
}

# ========================================
# 3. VERIFICACIÓN DE CERTIFICADOS
# ========================================
Write-Host "`n3. VERIFICACIÓN DE REVOCACIÓN DE CERTIFICADOS" -ForegroundColor Yellow
Write-Host "=" * 60

$oidPath = 'HKLM:\SOFTWARE\Microsoft\Cryptography\OID\EncodingType 0\CertDllCreateCertificateChainEngine\Config'

if (Test-Path $oidPath) {
    $maxUrl = Get-ItemProperty -Path $oidPath -Name "MaxUrlRetrievalByteCount" -ErrorAction SilentlyContinue
    
    Write-Host "  MaxUrlRetrievalByteCount: " -NoNewline
    if ($maxUrl.MaxUrlRetrievalByteCount -eq 0) {
        Write-Host "0 (Verificación DESHABILITADA)" -ForegroundColor Green
    } else {
        Write-Host "$($maxUrl.MaxUrlRetrievalByteCount) (Verificación HABILITADA)" -ForegroundColor Yellow
    }
} else {
    Write-Host "  NO CONFIGURADO (verificación por defecto HABILITADA)" -ForegroundColor Yellow
}

# ========================================
# 4. PROTOCOLOS SOPORTADOS POR .NET
# ========================================
Write-Host "`n4. PROTOCOLOS SOPORTADOS POR .NET (PowerShell)" -ForegroundColor Yellow
Write-Host "=" * 60

$currentProtocols = [Net.ServicePointManager]::SecurityProtocol
Write-Host "  Protocolos actuales: " -NoNewline
Write-Host $currentProtocols -ForegroundColor Cyan

Write-Host "`n  Desglose:" -ForegroundColor Gray
if ($currentProtocols -band [Net.SecurityProtocolType]::Ssl3) {
    Write-Host "    - SSL 3.0: HABILITADO" -ForegroundColor Red
}
if ($currentProtocols -band [Net.SecurityProtocolType]::Tls) {
    Write-Host "    - TLS 1.0: HABILITADO" -ForegroundColor Yellow
}
if ($currentProtocols -band [Net.SecurityProtocolType]::Tls11) {
    Write-Host "    - TLS 1.1: HABILITADO" -ForegroundColor Yellow
}
if ($currentProtocols -band [Net.SecurityProtocolType]::Tls12) {
    Write-Host "    - TLS 1.2: HABILITADO" -ForegroundColor Green
}
if ($currentProtocols -band 12288) { # TLS 1.3
    Write-Host "    - TLS 1.3: HABILITADO" -ForegroundColor Green
}

# ========================================
# 5. EVENTOS SCHANNEL RECIENTES
# ========================================
Write-Host "`n5. EVENTOS SCHANNEL RECIENTES (últimas 24 horas)" -ForegroundColor Yellow
Write-Host "=" * 60

try {
    $yesterday = (Get-Date).AddDays(-1)
    $schannelEvents = Get-WinEvent -LogName System -ErrorAction SilentlyContinue | 
        Where-Object {$_.ProviderName -eq "Schannel" -and $_.TimeCreated -gt $yesterday} | 
        Select-Object -First 10
    
    if ($schannelEvents) {
        foreach ($event in $schannelEvents) {
            $color = switch ($event.Level) {
                1 { "Red" }      # Critical
                2 { "Red" }      # Error
                3 { "Yellow" }   # Warning
                default { "White" }
            }
            
            Write-Host "  [$($event.TimeCreated.ToString('yyyy-MM-dd HH:mm:ss'))] " -NoNewline -ForegroundColor Gray
            Write-Host "ID:$($event.Id) - " -NoNewline -ForegroundColor $color
            Write-Host $event.Message.Split("`n")[0].Substring(0, [Math]::Min(60, $event.Message.Length)) -ForegroundColor $color
        }
    } else {
        Write-Host "  No hay eventos de Schannel recientes" -ForegroundColor Green
    }
} catch {
    Write-Host "  No se pudo acceder al log de eventos" -ForegroundColor Red
}

# ========================================
# 6. TEST DE CONEXIÓN A SIFEN
# ========================================
Write-Host "`n6. TEST DE CONEXIÓN A SIFEN" -ForegroundColor Yellow
Write-Host "=" * 60

try {
    Write-Host "  Probando conexión a sifen-test.set.gov.py:443..." -ForegroundColor Gray
    
    $testResult = Test-NetConnection -ComputerName "sifen-test.set.gov.py" -Port 443 -WarningAction SilentlyContinue
    
    if ($testResult.TcpTestSucceeded) {
        Write-Host "  Conexión TCP: " -NoNewline
        Write-Host "EXITOSA" -ForegroundColor Green
        Write-Host "  IP Remota: $($testResult.RemoteAddress)" -ForegroundColor Gray
    } else {
        Write-Host "  Conexión TCP: " -NoNewline
        Write-Host "FALLIDA" -ForegroundColor Red
    }
} catch {
    Write-Host "  Error en test de conexión: $($_.Exception.Message)" -ForegroundColor Red
}

# ========================================
# RESUMEN Y RECOMENDACIONES
# ========================================
Write-Host "`n" + ("=" * 60) -ForegroundColor Cyan
Write-Host "RESUMEN Y RECOMENDACIONES" -ForegroundColor Cyan
Write-Host ("=" * 60) -ForegroundColor Cyan

$issues = @()

# Verificar TLS 1.2
$tls12Path = "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.2\Client"
if (-not (Test-Path $tls12Path)) {
    $issues += "❌ TLS 1.2 NO está configurado en Schannel"
} else {
    $tls12Enabled = Get-ItemProperty -Path $tls12Path -Name "Enabled" -ErrorAction SilentlyContinue
    if ($tls12Enabled.Enabled -ne 1) {
        $issues += "❌ TLS 1.2 está DESHABILITADO en Schannel"
    }
}

# Verificar .NET
if (Test-Path $netFx64Path) {
    $netCrypto = Get-ItemProperty -Path $netFx64Path -Name "SchUseStrongCrypto" -ErrorAction SilentlyContinue
    if ($netCrypto.SchUseStrongCrypto -ne 1) {
        $issues += "❌ .NET Framework NO tiene SchUseStrongCrypto habilitado"
    }
}

if ($issues.Count -eq 0) {
    Write-Host "✅ Sistema configurado correctamente para SIFEN" -ForegroundColor Green
} else {
    Write-Host "`nPROBLEMAS DETECTADOS:" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "  $issue" -ForegroundColor Red
    }
    Write-Host "`nEjecuta: ConfigurarSchannel_SIFEN.ps1 (como Administrador)" -ForegroundColor Yellow
}

Write-Host "`n"
