<#
.SYNOPSIS
    Genera el paquete de instalación de SistemIA
.DESCRIPTION
    Compila la aplicación y crea un paquete ZIP listo para distribución
#>

param(
    [string]$OutputPath = ".\Releases",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"
$projectPath = Split-Path -Parent $PSScriptRoot
$publishPath = Join-Path $projectPath "bin\Release\net8.0\publish"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║        GENERADOR DE PAQUETE DE INSTALACIÓN                ║" -ForegroundColor Cyan
Write-Host "║                    SistemIA v$Version                       ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Crear carpeta de salida
$releaseFolder = Join-Path $projectPath $OutputPath
if (-not (Test-Path $releaseFolder)) {
    New-Item -ItemType Directory -Path $releaseFolder -Force | Out-Null
}

# Limpiar publicación anterior
Write-Host "[1/5] Limpiando publicación anterior..." -ForegroundColor Yellow
if (Test-Path $publishPath) {
    Remove-Item -Path $publishPath -Recurse -Force
}

# Compilar y publicar
Write-Host "[2/5] Compilando aplicación..." -ForegroundColor Yellow
Push-Location $projectPath

try {
    # Publicar para Windows x64 (self-contained)
    dotnet publish -c Release -r win-x64 --self-contained true `
        -p:PublishSingleFile=false `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -o $publishPath
    
    if ($LASTEXITCODE -ne 0) {
        throw "Error en la compilación"
    }
    
    Write-Host "  Compilación exitosa" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Copiar archivos del instalador
Write-Host "[3/5] Copiando archivos del instalador..." -ForegroundColor Yellow
$installerSource = Join-Path $projectPath "Installer"
$installerDest = Join-Path $publishPath "Installer"

New-Item -ItemType Directory -Path $installerDest -Force | Out-Null

Copy-Item -Path "$installerSource\*.ps1" -Destination $installerDest
Copy-Item -Path "$installerSource\*.bat" -Destination $installerDest
Copy-Item -Path "$installerSource\*.sql" -Destination $installerDest
Copy-Item -Path "$installerSource\*.json" -Destination $installerDest
Copy-Item -Path "$installerSource\README.md" -Destination $installerDest

# Copiar carpeta de Certificados HTTPS
$certSource = Join-Path $installerSource "Certificados"
if (Test-Path $certSource) {
    $certDest = Join-Path $installerDest "Certificados"
    Copy-Item -Path $certSource -Destination $certDest -Recurse -Force
    Write-Host "  Carpeta Certificados copiada" -ForegroundColor Green
}

# Copiar modelos de reconocimiento facial
$faceModelsSource = Join-Path $projectPath "face_recognition_models"
if (Test-Path $faceModelsSource) {
    $faceModelsDest = Join-Path $publishPath "face_recognition_models"
    Copy-Item -Path $faceModelsSource -Destination $faceModelsDest -Recurse -Force
    Write-Host "  Modelos de reconocimiento facial copiados" -ForegroundColor Green
} else {
    Write-Host "  [AVISO] Carpeta face_recognition_models no encontrada" -ForegroundColor Yellow
}

# Crear archivo de versión
Write-Host "[4/5] Creando archivo de versión..." -ForegroundColor Yellow
$versionInfo = @{
    Version = $Version
    BuildDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    DotNetVersion = "8.0"
    Platform = "win-x64"
}

$versionInfo | ConvertTo-Json | Set-Content -Path (Join-Path $publishPath "version.json") -Encoding UTF8

# Crear archivo ZIP
Write-Host "[5/5] Creando paquete ZIP..." -ForegroundColor Yellow
$zipFileName = "SistemIA_v$($Version)_$(Get-Date -Format 'yyyyMMdd').zip"
$zipPath = Join-Path $releaseFolder $zipFileName

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive -Path "$publishPath\*" -DestinationPath $zipPath -CompressionLevel Optimal

$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  PAQUETE CREADO EXITOSAMENTE" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "  Archivo: $zipFileName" -ForegroundColor White
Write-Host "  Tamaño:  $zipSize MB" -ForegroundColor White
Write-Host "  Ruta:    $zipPath" -ForegroundColor White
Write-Host ""
Write-Host "Para instalar:" -ForegroundColor Yellow
Write-Host "  1. Extraiga el ZIP en el servidor destino" -ForegroundColor White
Write-Host "  2. Ejecute Installer\Instalar.bat como Administrador" -ForegroundColor White
Write-Host ""
