# =============================================================================
# SistemIA - Instalador de Certificado HTTPS para Cliente
# =============================================================================
# Este script instala la CA raíz de SistemIA en el equipo cliente.
# Después de ejecutarlo, Chrome confiará en el certificado del servidor.
# =============================================================================

$ErrorActionPreference = "Continue"

# === FUNCIONES ===
function Write-Title { param($text) Write-Host "`n=== $text ===" -ForegroundColor Cyan }
function Write-Success { param($text) Write-Host "[OK] $text" -ForegroundColor Green }
function Write-Info { param($text) Write-Host "[i] $text" -ForegroundColor Yellow }
function Write-Err { param($text) Write-Host "[ERROR] $text" -ForegroundColor Red }

# === BUSCAR CERTIFICADO CA ===
function Find-CACert {
    $locations = @(
        (Join-Path $PSScriptRoot "SistemIA-CA.crt"),
        (Join-Path $PSScriptRoot "rootCA-para-clientes.crt"),
        (Join-Path $PSScriptRoot "rootCA.crt"),
        (Join-Path (Get-Location).Path "SistemIA-CA.crt"),
        (Join-Path (Get-Location).Path "rootCA-para-clientes.crt"),
        (Join-Path (Get-Location).Path "rootCA.crt")
    )
    
    foreach ($loc in $locations) {
        if (-not [string]::IsNullOrWhiteSpace($loc) -and (Test-Path $loc -ErrorAction SilentlyContinue)) {
            return $loc
        }
    }
    
    # Buscar cualquier .crt o .pem en las ubicaciones conocidas
    $searchPaths = @($PSScriptRoot, (Get-Location).Path)
    foreach ($path in $searchPaths) {
        if (-not [string]::IsNullOrWhiteSpace($path) -and (Test-Path $path)) {
            $crt = Get-ChildItem -Path $path -Filter "*.crt" -File -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($crt) { return $crt.FullName }
            
            $pem = Get-ChildItem -Path $path -Filter "*.pem" -File -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($pem) { return $pem.FullName }
        }
    }
    
    return $null
}

# === INICIO ===
Clear-Host
Write-Host ""
Write-Host "======================================================" -ForegroundColor Cyan
Write-Host "  SistemIA - Instalador de Certificado HTTPS (Cliente)" -ForegroundColor Cyan
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

# Buscar certificado CA
Write-Info "Buscando certificado CA..."
$caFile = Find-CACert

if ([string]::IsNullOrWhiteSpace($caFile)) {
    Write-Err "No se encontro el archivo de certificado CA"
    Write-Err "Asegurese de que 'SistemIA-CA.crt' este en la misma carpeta"
    pause
    exit 1
}

Write-Success "Certificado CA encontrado: $caFile"

Write-Title "INSTALANDO CERTIFICADO CA"

try {
    # Importar certificado en el almacén de certificados raíz de confianza
    $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($caFile)
    
    Write-Info "Certificado: $($cert.Subject)"
    Write-Info "Valido hasta: $($cert.NotAfter)"
    
    # Abrir almacén de certificados raíz
    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")
    $store.Open("ReadWrite")
    
    # Verificar si ya existe
    $existing = $store.Certificates | Where-Object { $_.Thumbprint -eq $cert.Thumbprint }
    if ($existing) {
        Write-Info "El certificado ya estaba instalado"
    } else {
        $store.Add($cert)
        Write-Success "Certificado instalado correctamente"
    }
    
    $store.Close()
    
} catch {
    Write-Err "Error al instalar certificado: $($_.Exception.Message)"
    
    Write-Info "Intentando metodo alternativo con certutil..."
    try {
        $result = & certutil -addstore -f "ROOT" "$caFile" 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Certificado instalado con certutil"
        } else {
            Write-Err "Error con certutil: $result"
        }
    } catch {
        Write-Err "Error: $($_.Exception.Message)"
    }
}

Write-Title "VERIFICACION"

# Verificar instalación
try {
    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")
    $store.Open("ReadOnly")
    
    $sistemiaCerts = $store.Certificates | Where-Object { $_.Subject -like "*mkcert*" -or $_.Subject -like "*SistemIA*" }
    
    if ($sistemiaCerts) {
        Write-Success "Certificados SistemIA instalados:"
        foreach ($c in $sistemiaCerts) {
            Write-Host "  - $($c.Subject)" -ForegroundColor White
            Write-Host "    Valido: $($c.NotBefore) - $($c.NotAfter)" -ForegroundColor Gray
        }
    } else {
        Write-Info "No se encontraron certificados con nombre SistemIA/mkcert"
        Write-Info "Esto puede ser normal si el certificado tiene otro nombre"
    }
    
    $store.Close()
} catch {
    Write-Err "Error al verificar: $($_.Exception.Message)"
}

Write-Title "INSTALACION COMPLETADA"

Write-Host ""
Write-Success "El certificado CA ha sido instalado"
Write-Host ""
Write-Info "Proximos pasos:"
Write-Host "  1. Cierre y vuelva a abrir Chrome" -ForegroundColor White
Write-Host "  2. Acceda a https://[IP-SERVIDOR]:7060" -ForegroundColor White
Write-Host "  3. Ya no deberia ver advertencias de seguridad" -ForegroundColor White
Write-Host ""

pause
