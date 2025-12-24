# ============================================================
# Script de Publicacion SistemIA - Self-Contained con .NET 8
# ============================================================

param(
    [string]$OutputPath = ".\Publicacion",
    [switch]$NoComprimir,
    [switch]$NoAbrir
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " PUBLICACION SISTEMITA - SELF-CONTAINED" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host "Destino: $OutputPath" -ForegroundColor Gray
Write-Host ""

# Configuracion
$ProjectPath = ".\SistemIA.csproj"
$PublishFolder = Join-Path $OutputPath "SistemIA"
$Version = Get-Date -Format "yyyy.MM.dd.HHmm"
$ZipName = "SistemIA_$Version.zip"

# Verificar que estamos en la carpeta correcta
if (-not (Test-Path $ProjectPath)) {
    Write-Host "[ERROR] No se encontro $ProjectPath" -ForegroundColor Red
    exit 1
}

# Limpiar carpeta de publicacion anterior
Write-Host "[*] Limpiando carpeta anterior..." -ForegroundColor Yellow
if (Test-Path $PublishFolder) {
    Remove-Item $PublishFolder -Recurse -Force
}
if (Test-Path (Join-Path $OutputPath $ZipName)) {
    Remove-Item (Join-Path $OutputPath $ZipName) -Force
}
New-Item -ItemType Directory -Path $PublishFolder -Force | Out-Null
Write-Host "[OK] Carpeta limpiada" -ForegroundColor Green

# Restaurar paquetes
Write-Host "[*] Restaurando paquetes NuGet..." -ForegroundColor Yellow
dotnet restore $ProjectPath --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Error al restaurar paquetes" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Paquetes restaurados" -ForegroundColor Green

# Publicar como self-contained para Windows x64
Write-Host "[*] Publicando aplicacion (self-contained)..." -ForegroundColor Yellow
Write-Host "    Runtime: win-x64" -ForegroundColor Gray
Write-Host "    Modo: Release" -ForegroundColor Gray
Write-Host "    Self-Contained: Si (incluye .NET 8)" -ForegroundColor Gray

dotnet publish $ProjectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $PublishFolder `
    /p:PublishSingleFile=false `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Error durante la publicacion" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Publicacion completada" -ForegroundColor Green

# Copiar archivos adicionales
Write-Host "[*] Copiando archivos adicionales..." -ForegroundColor Yellow

if (Test-Path ".\Installer") {
    Copy-Item ".\Installer" -Destination "$PublishFolder\Installer" -Recurse -Force
    Write-Host "    - Installer copiado" -ForegroundColor Gray
}

if (Test-Path ".\certificados") {
    Copy-Item ".\certificados" -Destination "$PublishFolder\certificados" -Recurse -Force
    Write-Host "    - Certificados copiados" -ForegroundColor Gray
}

if (Test-Path ".\face_recognition_models") {
    Copy-Item ".\face_recognition_models" -Destination "$PublishFolder\face_recognition_models" -Recurse -Force
    Write-Host "    - Modelos facial copiados" -ForegroundColor Gray
}

Write-Host "[OK] Archivos adicionales copiados" -ForegroundColor Green

# Crear script de inicio
Write-Host "[*] Creando scripts de utilidad..." -ForegroundColor Yellow

$iniciar = "@echo off`r`necho Iniciando SistemIA...`r`ncd /d `"%~dp0`"`r`nSistemIA.exe`r`npause"
[System.IO.File]::WriteAllText("$PublishFolder\Iniciar.bat", $iniciar, [System.Text.Encoding]::ASCII)

$instalar = "@echo off`r`necho Instalando servicio SistemIA...`r`nset RUTA=%~dp0`r`nsc create SistemIA binPath= `"%RUTA%SistemIA.exe`" start= auto DisplayName= `"SistemIA`"`r`nnet start SistemIA`r`necho Servicio instalado!`r`npause"
[System.IO.File]::WriteAllText("$PublishFolder\InstalarServicio.bat", $instalar, [System.Text.Encoding]::ASCII)

$desinstalar = "@echo off`r`necho Desinstalando servicio SistemIA...`r`nnet stop SistemIA`r`nsc delete SistemIA`r`necho Servicio eliminado!`r`npause"
[System.IO.File]::WriteAllText("$PublishFolder\DesinstalarServicio.bat", $desinstalar, [System.Text.Encoding]::ASCII)

Write-Host "[OK] Scripts creados" -ForegroundColor Green

# Calcular tamano
$size = (Get-ChildItem $PublishFolder -Recurse | Measure-Object -Property Length -Sum).Sum
$sizeMB = [math]::Round($size / 1MB, 2)
Write-Host ""
Write-Host "Tamano de la publicacion: $sizeMB MB" -ForegroundColor Cyan

# Comprimir
if (-not $NoComprimir) {
    Write-Host "[*] Comprimiendo paquete..." -ForegroundColor Yellow
    $zipPath = Join-Path $OutputPath $ZipName
    
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    
    Compress-Archive -Path "$PublishFolder\*" -DestinationPath $zipPath -CompressionLevel Optimal
    
    $zipSize = (Get-Item $zipPath).Length
    $zipSizeMB = [math]::Round($zipSize / 1MB, 2)
    Write-Host "[OK] ZIP creado: $ZipName ($zipSizeMB MB)" -ForegroundColor Green
}

# Resumen
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " PUBLICACION COMPLETADA" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Carpeta: $PublishFolder" -ForegroundColor White
Write-Host "  ZIP:     $ZipName" -ForegroundColor White
Write-Host "  Version: $Version" -ForegroundColor White
Write-Host ""

# Abrir carpeta
if (-not $NoAbrir) {
    Start-Process explorer.exe -ArgumentList $OutputPath
}

Write-Host "Listo para distribuir!" -ForegroundColor Green
Write-Host ""
