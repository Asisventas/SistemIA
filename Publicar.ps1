# ============================================================
# Script de Publicación SistemIA - Self-Contained con .NET 8
# ============================================================
# Genera un paquete completo con el runtime de .NET incluido
# No requiere instalación previa de .NET en el servidor destino
# ============================================================

param(
    [string]$OutputPath = ".\Publicacion",
    [switch]$Comprimir = $true,
    [switch]$AbrirCarpeta = $true
)

$ErrorActionPreference = "Stop"
$Host.UI.RawUI.WindowTitle = "Publicando SistemIA..."

# Colores para la consola
function Write-Header { param($msg) Write-Host "`n========================================" -ForegroundColor Cyan; Write-Host " $msg" -ForegroundColor Cyan; Write-Host "========================================" -ForegroundColor Cyan }
function Write-Step { param($msg) Write-Host "[*] $msg" -ForegroundColor Yellow }
function Write-Success { param($msg) Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Error { param($msg) Write-Host "[ERROR] $msg" -ForegroundColor Red }

Write-Header "PUBLICACIÓN SISTEMITA - SELF-CONTAINED"
Write-Host "Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host "Destino: $OutputPath" -ForegroundColor Gray

# Configuración
$ProjectPath = ".\SistemIA.csproj"
$PublishFolder = Join-Path $OutputPath "SistemIA"
$Version = Get-Date -Format "yyyy.MM.dd"
$ZipName = "SistemIA_$Version.zip"

# Verificar que estamos en la carpeta correcta
if (-not (Test-Path $ProjectPath)) {
    Write-Error "No se encontró $ProjectPath. Ejecute este script desde la carpeta del proyecto."
    exit 1
}

# Limpiar carpeta de publicación anterior
Write-Step "Limpiando carpeta de publicación anterior..."
if (Test-Path $PublishFolder) {
    Remove-Item $PublishFolder -Recurse -Force
}
if (Test-Path (Join-Path $OutputPath $ZipName)) {
    Remove-Item (Join-Path $OutputPath $ZipName) -Force
}
New-Item -ItemType Directory -Path $PublishFolder -Force | Out-Null

# Restaurar paquetes
Write-Step "Restaurando paquetes NuGet..."
dotnet restore $ProjectPath --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error al restaurar paquetes"
    exit 1
}
Write-Success "Paquetes restaurados"

# Publicar como self-contained para Windows x64
Write-Step "Publicando aplicación (self-contained con .NET 8)..."
Write-Host "  Runtime: win-x64" -ForegroundColor Gray
Write-Host "  Modo: Release" -ForegroundColor Gray
Write-Host "  Self-Contained: Sí (incluye .NET 8)" -ForegroundColor Gray

dotnet publish $ProjectPath `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $PublishFolder `
    /p:PublishSingleFile=false `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:EnableCompressionInSingleFile=true `
    --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Error "Error durante la publicación"
    exit 1
}
Write-Success "Publicación completada"

# Copiar archivos adicionales necesarios
Write-Step "Copiando archivos adicionales..."

# Copiar carpeta Installer si existe
if (Test-Path ".\Installer") {
    Copy-Item ".\Installer" -Destination "$PublishFolder\Installer" -Recurse -Force
    Write-Success "Carpeta Installer copiada"
}

# Copiar certificados si existen
if (Test-Path ".\certificados") {
    Copy-Item ".\certificados" -Destination "$PublishFolder\certificados" -Recurse -Force
    Write-Success "Carpeta certificados copiada"
}

# Copiar modelos de reconocimiento facial si existen
if (Test-Path ".\face_recognition_models") {
    Copy-Item ".\face_recognition_models" -Destination "$PublishFolder\face_recognition_models" -Recurse -Force
    Write-Success "Modelos de reconocimiento facial copiados"
}

# Crear script de inicio rápido
$inicioRapido = @"
@echo off
echo ============================================
echo        INICIANDO SISTEMITA
echo ============================================
echo.
echo Presione Ctrl+C para detener el servidor
echo.
cd /d "%~dp0"
SistemIA.exe
pause
"@
$inicioRapido | Out-File -FilePath "$PublishFolder\Iniciar.bat" -Encoding ASCII

# Crear script de instalación como servicio
$instalarServicio = @"
@echo off
echo ============================================
echo   INSTALAR SISTEMITA COMO SERVICIO
echo ============================================
echo.
echo IMPORTANTE: Ejecute este script como Administrador
echo.

set RUTA=%~dp0
set NOMBRE=SistemIA
set EXE=%RUTA%SistemIA.exe

echo Deteniendo servicio existente (si existe)...
net stop %NOMBRE% 2>nul

echo Eliminando servicio existente (si existe)...
sc delete %NOMBRE% 2>nul

echo Creando nuevo servicio...
sc create %NOMBRE% binPath= "%EXE%" start= auto DisplayName= "SistemIA - Sistema de Gestión"
sc description %NOMBRE% "Sistema de Gestión Empresarial SistemIA - Blazor Server"

echo Iniciando servicio...
net start %NOMBRE%

echo.
echo ============================================
echo   SERVICIO INSTALADO CORRECTAMENTE
echo ============================================
echo.
echo El sistema está disponible en:
echo   http://localhost:5095
echo   https://localhost:7060
echo.
pause
"@
$instalarServicio | Out-File -FilePath "$PublishFolder\InstalarServicio.bat" -Encoding ASCII

# Crear script de desinstalación
$desinstalarServicio = @"
@echo off
echo ============================================
echo   DESINSTALAR SERVICIO SISTEMITA
echo ============================================
echo.
echo IMPORTANTE: Ejecute este script como Administrador
echo.

set NOMBRE=SistemIA

echo Deteniendo servicio...
net stop %NOMBRE%

echo Eliminando servicio...
sc delete %NOMBRE%

echo.
echo ============================================
echo   SERVICIO ELIMINADO CORRECTAMENTE
echo ============================================
echo.
pause
"@
$desinstalarServicio | Out-File -FilePath "$PublishFolder\DesinstalarServicio.bat" -Encoding ASCII

Write-Success "Scripts de utilidad creados"

# Crear archivo README
$readme = @"
# SistemIA - Paquete de Instalación
## Versión: $Version
## Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')

---

## REQUISITOS
- Windows 10/11 o Windows Server 2016+
- NO requiere instalación de .NET (incluido en el paquete)
- SQL Server (Express o superior)

---

## INSTALACIÓN RÁPIDA

### Opción 1: Ejecutar directamente
1. Descomprima el archivo en C:\SistemIA (o la ubicación deseada)
2. Ejecute ``Iniciar.bat``
3. Abra el navegador en http://localhost:5095

### Opción 2: Instalar como servicio de Windows
1. Descomprima el archivo en C:\SistemIA
2. Ejecute ``InstalarServicio.bat`` como Administrador
3. El servicio se iniciará automáticamente con Windows

---

## ARCHIVOS INCLUIDOS

| Archivo | Descripción |
|---------|-------------|
| Iniciar.bat | Inicia la aplicación manualmente |
| InstalarServicio.bat | Instala como servicio de Windows |
| DesinstalarServicio.bat | Elimina el servicio de Windows |
| appsettings.json | Configuración de la aplicación |

---

## CONFIGURACIÓN

Edite ``appsettings.json`` para configurar:
- Cadena de conexión a SQL Server
- Puertos HTTP/HTTPS
- Otras opciones

---

## URLS POR DEFECTO

- HTTP:  http://localhost:5095
- HTTPS: https://localhost:7060
- LAN:   http://[IP-LOCAL]:5095

---

## SOPORTE

Para soporte técnico contacte al administrador del sistema.
"@
$readme | Out-File -FilePath "$PublishFolder\README.md" -Encoding UTF8

Write-Success "README.md creado"

# Calcular tamaño
$size = (Get-ChildItem $PublishFolder -Recurse | Measure-Object -Property Length -Sum).Sum
$sizeMB = [math]::Round($size / 1MB, 2)
Write-Host "`nTamaño de la publicación: $sizeMB MB" -ForegroundColor Cyan

# Comprimir si se solicitó
if ($Comprimir) {
    Write-Step "Comprimiendo paquete..."
    $zipPath = Join-Path $OutputPath $ZipName
    
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    
    Compress-Archive -Path "$PublishFolder\*" -DestinationPath $zipPath -CompressionLevel Optimal
    
    $zipSize = (Get-Item $zipPath).Length
    $zipSizeMB = [math]::Round($zipSize / 1MB, 2)
    Write-Success "Paquete comprimido: $ZipName ($zipSizeMB MB)"
}

# Resumen final
Write-Header "PUBLICACIÓN COMPLETADA"
Write-Host ""
Write-Host "  Carpeta: $PublishFolder" -ForegroundColor White
if ($Comprimir) {
    Write-Host "  ZIP:     $(Join-Path $OutputPath $ZipName)" -ForegroundColor White
}
Write-Host "  Tamaño:  $sizeMB MB (sin comprimir)" -ForegroundColor White
if ($Comprimir) {
    Write-Host "  Tamaño:  $zipSizeMB MB (comprimido)" -ForegroundColor White
}
Write-Host ""
Write-Host "  Este paquete incluye:" -ForegroundColor Gray
Write-Host "    - Runtime .NET 8 completo" -ForegroundColor Gray
Write-Host "    - Todas las dependencias" -ForegroundColor Gray
Write-Host "    - Scripts de instalación" -ForegroundColor Gray
Write-Host ""

# Abrir carpeta si se solicitó
if ($AbrirCarpeta) {
    Start-Process explorer.exe -ArgumentList $OutputPath
}

Write-Host "Listo para distribuir!" -ForegroundColor Green
Write-Host ""
