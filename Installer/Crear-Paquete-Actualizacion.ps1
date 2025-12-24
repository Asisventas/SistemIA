<#
.SYNOPSIS
    Genera un paquete de actualización self-contained de SistemIA
.DESCRIPTION
    - Verifica que las migraciones estén listas
    - Compila como self-contained (incluye .NET runtime)
    - Incluye Actualizar.bat para instalación automática
    - Crea un ZIP listo para distribuir (sin appsettings.json)
.PARAMETER Version
    Versión del paquete (ej: 1.2.0)
.PARAMETER OutputPath
    Carpeta donde se guardará el paquete
.EXAMPLE
    .\Crear-Paquete-Actualizacion.ps1 -Version "1.2.0"
#>

param(
    [string]$Version = "1.0.0",
    [string]$OutputPath = ".\Releases"
)

$ErrorActionPreference = "Stop"
$projectPath = Split-Path -Parent $PSScriptRoot
$publishPath = Join-Path $projectPath "publish_update_temp"
$installerPath = $PSScriptRoot

function Write-Header {
    param([string]$Text)
    Write-Host ""
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host "  $Text" -ForegroundColor Cyan
    Write-Host "================================================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Step {
    param([int]$Number, [int]$Total, [string]$Text)
    Write-Host "[$Number/$Total] $Text" -ForegroundColor Yellow
}

function Write-Success {
    param([string]$Text)
    Write-Host "  + $Text" -ForegroundColor Green
}

function Write-ErrorMsg {
    param([string]$Text)
    Write-Host "  x $Text" -ForegroundColor Red
}

function Write-Info {
    param([string]$Text)
    Write-Host "  > $Text" -ForegroundColor Cyan
}

# ============================================================
# INICIO
# ============================================================

Write-Header "GENERADOR DE PAQUETE DE ACTUALIZACION SistemIA v$Version"

# Crear carpeta de salida
$releaseFolder = Join-Path $projectPath $OutputPath
if (-not (Test-Path $releaseFolder)) {
    New-Item -ItemType Directory -Path $releaseFolder -Force | Out-Null
}

# ============================================================
# PASO 1: Verificar migraciones pendientes
# ============================================================
Write-Step 1 6 "Verificando migraciones..."

Push-Location $projectPath
try {
    # Compilar primero
    $buildOutput = dotnet build --verbosity quiet 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorMsg "Error en la compilacion"
        Write-Host $buildOutput
        exit 1
    }
    
    # Listar migraciones
    $migrationsOutput = dotnet ef migrations list --no-build 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorMsg "Error al verificar migraciones"
        Write-Host $migrationsOutput
        exit 1
    }
    
    # Contar migraciones
    $allMigrations = $migrationsOutput | Where-Object { $_ -match "^\d{14}_" }
    $pendingMigrations = $migrationsOutput | Where-Object { $_ -match "\(Pending\)" }
    
    Write-Success "Migraciones totales: $($allMigrations.Count)"
    
    if ($pendingMigrations.Count -gt 0) {
        Write-Info "Migraciones pendientes que se aplicaran:"
        foreach ($mig in $pendingMigrations) {
            Write-Host "    * $mig" -ForegroundColor Magenta
        }
    } else {
        Write-Info "No hay migraciones pendientes (todas aplicadas en este entorno)"
    }
    
    Write-Host ""
    Write-Host "  Las migraciones se aplicaran automaticamente en el servidor destino." -ForegroundColor White
    $confirm = Read-Host "  Continuar con la generacion del paquete? (S/N)"
    if ($confirm -notmatch "^[SsYy]") {
        Write-Host "Operacion cancelada" -ForegroundColor Yellow
        exit 0
    }
    
} finally {
    Pop-Location
}

# ============================================================
# PASO 2: Limpiar y publicar
# ============================================================
Write-Step 2 6 "Compilando aplicacion self-contained..."

if (Test-Path $publishPath) {
    Remove-Item -Path $publishPath -Recurse -Force
}

Push-Location $projectPath
try {
    $publishOutput = dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o $publishPath 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorMsg "Error en la publicacion"
        Write-Host $publishOutput
        exit 1
    }
    
    Write-Success "Publicacion completada"
} finally {
    Pop-Location
}

# ============================================================
# PASO 3: Eliminar archivos de configuración del paquete
# ============================================================
Write-Step 3 6 "Preparando archivos (excluyendo configuracion)..."

$configFiles = @(
    "appsettings.json",
    "appsettings.Development.json",
    "appsettings.Production.json"
)

foreach ($configFile in $configFiles) {
    $configPath = Join-Path $publishPath $configFile
    if (Test-Path $configPath) {
        Remove-Item $configPath -Force
        Write-Info "Excluido: $configFile"
    }
}

# ============================================================
# PASO 4: Crear archivo de versión
# ============================================================
Write-Step 4 6 "Creando metadatos de version..."

$versionInfo = @{
    Version = $Version
    BuildDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    DotNetVersion = "8.0"
    Platform = "win-x64"
    SelfContained = $true
    MigrationsIncluded = $true
}

$versionInfo | ConvertTo-Json | Set-Content -Path (Join-Path $publishPath "version.json") -Encoding UTF8
Write-Success "Archivo version.json creado"

# ============================================================
# PASO 5: Copiar scripts de actualización
# ============================================================
Write-Step 5 6 "Copiando scripts de actualizacion..."

$batSource = Join-Path $installerPath "Actualizar.bat"
$batDest = Join-Path $publishPath "Actualizar.bat"

if (Test-Path $batSource) {
    Copy-Item -Path $batSource -Destination $batDest -Force
    Write-Success "Script Actualizar.bat incluido"
} else {
    Write-ErrorMsg "No se encontro Actualizar.bat en $installerPath"
    exit 1
}

# Copiar script de reparación de servicio
$repairSource = Join-Path $installerPath "Reparar-Servicio.bat"
$repairDest = Join-Path $publishPath "Reparar-Servicio.bat"

if (Test-Path $repairSource) {
    Copy-Item -Path $repairSource -Destination $repairDest -Force
    Write-Success "Script Reparar-Servicio.bat incluido"
} else {
    Write-Info "Script Reparar-Servicio.bat no encontrado (opcional)"
}

# Copiar script de diagnóstico
$diagSource = Join-Path $installerPath "Diagnostico-SistemIA.bat"
$diagDest = Join-Path $publishPath "Diagnostico-SistemIA.bat"

if (Test-Path $diagSource) {
    Copy-Item -Path $diagSource -Destination $diagDest -Force
    Write-Success "Script Diagnostico-SistemIA.bat incluido"
} else {
    Write-Info "Script Diagnostico-SistemIA.bat no encontrado (opcional)"
}

# ============================================================
# PASO 6: Crear ZIP
# ============================================================
Write-Step 6 6 "Creando paquete ZIP..."

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$zipFileName = "SistemIA_Update_v${Version}_${timestamp}.zip"
$zipPath = Join-Path $releaseFolder $zipFileName

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive -Path "$publishPath\*" -DestinationPath $zipPath -CompressionLevel Optimal

# Limpiar carpeta temporal
Remove-Item -Path $publishPath -Recurse -Force

$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)

# ============================================================
# RESUMEN
# ============================================================
Write-Header "PAQUETE DE ACTUALIZACION CREADO EXITOSAMENTE"

Write-Host "  Archivo:     $zipFileName" -ForegroundColor White
Write-Host "  Tamano:      $zipSize MB" -ForegroundColor White
Write-Host "  Ubicacion:   $zipPath" -ForegroundColor White
Write-Host ""
Write-Host "  Contenido:" -ForegroundColor Yellow
Write-Host "    - Aplicacion self-contained (incluye .NET 8.0 runtime)" -ForegroundColor White
Write-Host "    - Script Actualizar.bat para instalacion automatica" -ForegroundColor White
Write-Host "    - Script Diagnostico-SistemIA.bat (menu de diagnostico)" -ForegroundColor White
Write-Host "    - Script Reparar-Servicio.bat (si el servicio no inicia)" -ForegroundColor White
Write-Host "    - Migraciones de BD incluidas (se aplican al iniciar)" -ForegroundColor White
Write-Host "    - NO incluye appsettings.json (preserva configuracion)" -ForegroundColor White
Write-Host ""
Write-Host "  Instrucciones de uso:" -ForegroundColor Yellow
Write-Host "    1. Copie el ZIP al servidor destino" -ForegroundColor White
Write-Host "    2. Extraiga el contenido en una carpeta temporal" -ForegroundColor White
Write-Host "    3. Ejecute Actualizar.bat como Administrador" -ForegroundColor White
Write-Host "    4. Si hay problemas, ejecute Diagnostico-SistemIA.bat" -ForegroundColor White
Write-Host ""
