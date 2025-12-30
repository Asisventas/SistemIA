# =============================================================================
# SistemIA - Instalador de Certificado HTTPS para Servidor
# =============================================================================
param(
    [string]$InstallPath = "C:\SistemIA",
    [string]$ServerIP = "",
    [string]$ServerName = ""
)

$ErrorActionPreference = "Continue"

# === FUNCIONES ===
function Write-Title { param($text) Write-Host "`n=== $text ===" -ForegroundColor Cyan }
function Write-Success { param($text) Write-Host "[OK] $text" -ForegroundColor Green }
function Write-Info { param($text) Write-Host "[i] $text" -ForegroundColor Yellow }
function Write-Err { param($text) Write-Host "[ERROR] $text" -ForegroundColor Red }

# === BUSCAR MKCERT ===
function Find-Mkcert {
    $locations = @(
        (Join-Path $PSScriptRoot "mkcert.exe"),
        (Join-Path (Get-Location).Path "mkcert.exe"),
        "C:\SistemIA\SistemIA_Certificados_HTTPS\mkcert.exe",
        "$env:USERPROFILE\Desktop\SistemIA_Certificados_HTTPS\mkcert.exe",
        "C:\Users\Admin\Desktop\SistemIA_Certificados_HTTPS\mkcert.exe",
        "C:\SistemIA\mkcert.exe"
    )
    
    foreach ($loc in $locations) {
        if (-not [string]::IsNullOrWhiteSpace($loc) -and (Test-Path $loc -ErrorAction SilentlyContinue)) {
            return $loc
        }
    }
    return $null
}

# === INICIO ===
Clear-Host
Write-Host ""
Write-Host "======================================================" -ForegroundColor Cyan
Write-Host "  SistemIA - Instalador de Certificado HTTPS (Servidor)" -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Err "Este script debe ejecutarse como Administrador"
    Write-Info "Clic derecho -> Ejecutar como administrador"
    pause
    exit 1
}

# Buscar mkcert
Write-Info "Buscando mkcert.exe..."
$MKCERT = Find-Mkcert

if ([string]::IsNullOrWhiteSpace($MKCERT)) {
    Write-Err "No se encontro mkcert.exe en ninguna ubicacion conocida"
    Write-Err "Asegurese de que mkcert.exe este en la misma carpeta que este script"
    pause
    exit 1
}

Write-Success "mkcert encontrado: $MKCERT"

Write-Title "CONFIGURACION"

# Obtener IP
if ([string]::IsNullOrWhiteSpace($ServerIP)) {
    $ipAddresses = @(Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notlike "127.*" -and $_.PrefixOrigin -ne "WellKnown" } | Select-Object -ExpandProperty IPAddress)
    
    Write-Info "IPs detectadas en este equipo:"
    for ($i = 0; $i -lt $ipAddresses.Count; $i++) {
        Write-Host "  $($i+1). $($ipAddresses[$i])"
    }
    
    $ServerIP = Read-Host "`nIngrese la IP del servidor (Enter para usar $($ipAddresses[0]))"
    if ([string]::IsNullOrWhiteSpace($ServerIP)) {
        $ServerIP = $ipAddresses[0]
    }
}

# Obtener nombre del servidor
if ([string]::IsNullOrWhiteSpace($ServerName)) {
    $defaultName = $env:COMPUTERNAME
    Write-Host ""
    Write-Host "  IMPORTANTE: Ingrese solo el nombre de la MAQUINA, NO la instancia de SQL" -ForegroundColor Yellow
    Write-Host "  Ejemplo: Si su SQL es 'GASPARINI\SQL2022', ingrese solo 'GASPARINI'" -ForegroundColor Yellow
    Write-Host ""
    $ServerName = Read-Host "Nombre del equipo (Enter para '$defaultName')"
    if ([string]::IsNullOrWhiteSpace($ServerName)) {
        $ServerName = $defaultName
    }
    
    # Limpiar si el usuario ingresó la instancia de SQL por error
    if ($ServerName -match '\\') {
        $ServerName = $ServerName.Split('\')[0]
        Write-Info "Se detecto instancia de SQL, usando solo el nombre de maquina: $ServerName"
    }
}

Write-Info "IP del servidor: $ServerIP"
Write-Info "Nombre del servidor: $ServerName"
Write-Info "Ruta de instalacion: $InstallPath"

# Crear carpetas
if (-not (Test-Path $InstallPath)) {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
    Write-Success "Carpeta creada: $InstallPath"
}

$caRoot = Join-Path $InstallPath "CA"
if (-not (Test-Path $caRoot)) {
    New-Item -ItemType Directory -Path $caRoot -Force | Out-Null
}

Write-Title "INSTALANDO CA RAIZ LOCAL"

# Configurar CAROOT
$env:CAROOT = $caRoot
Write-Info "CAROOT configurado en: $env:CAROOT"
Write-Info "Ejecutando: $MKCERT -install"

# Ejecutar mkcert -install
try {
    $process = Start-Process -FilePath $MKCERT -ArgumentList "-install" -Wait -PassThru -NoNewWindow
    if ($process.ExitCode -eq 0) {
        Write-Success "CA raiz instalada correctamente"
    } else {
        Write-Err "Error al instalar CA raiz (codigo: $($process.ExitCode))"
    }
} catch {
    Write-Err "Excepcion: $($_.Exception.Message)"
}

Write-Title "GENERANDO CERTIFICADO SSL"

$certPath = Join-Path $InstallPath "certificate"
Write-Info "Generando certificado para: localhost, $ServerIP, $ServerName"

# Ejecutar mkcert para generar certificado
try {
    $args = "-pkcs12", "-p12-file", "$certPath.p12", "localhost", $ServerIP, $ServerName, "$ServerName.local", "127.0.0.1"
    Write-Info "Ejecutando: $MKCERT $($args -join ' ')"
    
    $process = Start-Process -FilePath $MKCERT -ArgumentList $args -Wait -PassThru -NoNewWindow -WorkingDirectory $InstallPath
    
    if (Test-Path "$certPath.p12") {
        Write-Success "Certificado generado: $certPath.p12"
        Write-Info "Password del certificado: changeit"
    } else {
        Write-Err "Error al generar certificado"
    }
} catch {
    Write-Err "Excepcion: $($_.Exception.Message)"
}

Write-Title "CONFIGURANDO APPSETTINGS"

$appSettingsPath = Join-Path $InstallPath "appsettings.json"
$httpsPort = 7060

if (Test-Path $appSettingsPath) {
    try {
        $json = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
        
        if (-not $json.Kestrel) {
            $json | Add-Member -NotePropertyName "Kestrel" -NotePropertyValue @{} -Force
        }
        
        $json.Kestrel = @{
            Endpoints = @{
                Http = @{
                    Url = "http://0.0.0.0:5095"
                }
                Https = @{
                    Url = "https://0.0.0.0:$httpsPort"
                    Certificate = @{
                        Path = "$certPath.p12"
                        Password = "changeit"
                    }
                }
            }
        }
        
        $json | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
        Write-Success "appsettings.json actualizado con configuracion HTTPS"
    } catch {
        Write-Err "Error al actualizar appsettings.json: $($_.Exception.Message)"
    }
} else {
    Write-Info "No se encontro appsettings.json en $InstallPath"
    Write-Info "Debera configurar manualmente el certificado"
}

Write-Title "EXPORTANDO CA PARA CLIENTES"

$caFile = Join-Path $caRoot "rootCA.pem"
$exportPath = Join-Path $InstallPath "rootCA-para-clientes.crt"

if (Test-Path $caFile) {
    Copy-Item $caFile $exportPath -Force
    Write-Success "CA exportada para clientes: $exportPath"
    Write-Info "Copie este archivo a los equipos cliente e instalelo como CA de confianza"
} else {
    Write-Err "No se encontro el archivo CA raiz"
}

Write-Title "INSTALACION COMPLETADA"

Write-Host ""
Write-Success "Certificado SSL instalado correctamente"
Write-Host ""
Write-Info "Resumen:"
Write-Host "  - Certificado: $certPath.p12" -ForegroundColor White
Write-Host "  - Password: changeit" -ForegroundColor White
Write-Host "  - Puerto HTTPS: $httpsPort" -ForegroundColor White
Write-Host "  - CA para clientes: $exportPath" -ForegroundColor White
Write-Host ""
Write-Info "Proximos pasos:"
Write-Host "  1. Reinicie SistemIA" -ForegroundColor White
Write-Host "  2. Acceda via https://$($ServerIP):$httpsPort" -ForegroundColor White
Write-Host "  3. Instale rootCA-para-clientes.crt en equipos cliente" -ForegroundColor White
Write-Host ""

pause
