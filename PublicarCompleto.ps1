# ============================================
# Script de Publicación - SistemIA + Actualizador
# ============================================
# Publica ambos proyectos self-contained para distribución
# Autor: SistemIA
# ============================================

param(
    [string]$OutputPath = "c:\asis\SistemIA\publish_complete",
    [switch]$IncluirActualizador = $true
)

$ErrorActionPreference = "Stop"
$host.UI.RawUI.ForegroundColor = "Cyan"

Write-Host ""
Write-Host "============================================"
Write-Host "  PUBLICACION SISTEMAI + ACTUALIZADOR"
Write-Host "============================================"
Write-Host ""

# Limpiar carpeta de salida
if (Test-Path $OutputPath) {
    Write-Host "[1/6] Limpiando carpeta de salida..." -ForegroundColor Yellow
    Remove-Item -Path $OutputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# Publicar SistemIA principal
Write-Host "[2/6] Publicando SistemIA (puerto 5095)..." -ForegroundColor Yellow
$sistemiaCsproj = "c:\asis\SistemIA\SistemIA.csproj"
dotnet publish $sistemiaCsproj -c Release -o "$OutputPath" --self-contained true -r win-x64 /p:PublishSingleFile=false

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Fallo al publicar SistemIA" -ForegroundColor Red
    exit 1
}
Write-Host "  OK - SistemIA publicado" -ForegroundColor Green

# Publicar Actualizador
if ($IncluirActualizador) {
    Write-Host "[3/6] Publicando SistemIA.Actualizador (puerto 5096)..." -ForegroundColor Yellow
    $actualizadorCsproj = "c:\asis\SistemIA.Actualizador\SistemIA.Actualizador.csproj"
    $actualizadorOutput = "$OutputPath\Actualizador"
    
    if (Test-Path $actualizadorCsproj) {
        dotnet publish $actualizadorCsproj -c Release -o $actualizadorOutput --self-contained true -r win-x64 /p:PublishSingleFile=false
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  WARN: Fallo al publicar Actualizador" -ForegroundColor Yellow
        } else {
            Write-Host "  OK - Actualizador publicado" -ForegroundColor Green
        }
    } else {
        Write-Host "  SKIP - Proyecto Actualizador no encontrado" -ForegroundColor Yellow
    }
}

# Crear carpeta Releases
Write-Host "[4/6] Creando estructura de carpetas..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path "$OutputPath\Releases" -Force | Out-Null
New-Item -ItemType Directory -Path "$OutputPath\Backups" -Force | Out-Null
Write-Host "  OK - Carpetas creadas" -ForegroundColor Green

# Crear script de inicio del Actualizador
Write-Host "[5/6] Creando scripts de ayuda..." -ForegroundColor Yellow
$iniciarActualizador = @'
@echo off
echo ============================================
echo   Iniciando SistemIA Actualizador
echo   Puerto: 5096
echo ============================================
cd /d "%~dp0Actualizador"
start "" "SistemIA.Actualizador.exe"
echo.
echo Abriendo navegador en http://localhost:5096
timeout /t 3 > nul
start http://localhost:5096
'@
$iniciarActualizador | Out-File -FilePath "$OutputPath\IniciarActualizador.bat" -Encoding ASCII

$iniciarSistemIA = @'
@echo off
echo ============================================
echo   Iniciando SistemIA
echo   Puerto: 5095
echo ============================================
cd /d "%~dp0"
start "" "SistemIA.exe"
echo.
echo Abriendo navegador en http://localhost:5095
timeout /t 5 > nul
start http://localhost:5095
'@
$iniciarSistemIA | Out-File -FilePath "$OutputPath\IniciarSistemIA.bat" -Encoding ASCII

Write-Host "  OK - Scripts creados" -ForegroundColor Green

# Resumen
Write-Host "[6/6] Generando resumen..." -ForegroundColor Yellow
$archivos = Get-ChildItem $OutputPath -Recurse -File | Measure-Object -Property Length -Sum
$tamanoMB = [math]::Round($archivos.Sum / 1MB, 2)

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  PUBLICACION COMPLETADA" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Ubicacion: $OutputPath" -ForegroundColor White
Write-Host "Archivos:  $($archivos.Count)" -ForegroundColor White
Write-Host "Tamano:    $tamanoMB MB" -ForegroundColor White
Write-Host ""
Write-Host "Contenido:" -ForegroundColor Cyan
Write-Host "  - SistemIA.exe         (puerto 5095)"
Write-Host "  - Actualizador\        (puerto 5096)"
Write-Host "  - IniciarSistemIA.bat"
Write-Host "  - IniciarActualizador.bat"
Write-Host "  - Releases\            (para paquetes ZIP)"
Write-Host "  - Backups\             (backups automaticos)"
Write-Host ""
Write-Host "Para instalar en cliente, copiar toda la carpeta a C:\SistemIA" -ForegroundColor Yellow
