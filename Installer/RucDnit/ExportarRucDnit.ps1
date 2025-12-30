# ExportarRucDnit.ps1
# Exporta la tabla RucDnit a archivo para distribución

param(
    [string]$Server = "SERVERSIS\SQL2022",
    [string]$Database = "asiswebapp",
    [string]$User = "sa",
    [string]$Password = "%L4V1CT0R14",
    [string]$OutputDir = ".\Output"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Exportador de RucDnit - SistemIA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Crear directorio de salida
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

$dataFile = Join-Path $OutputDir "RucDnit.dat"
$formatFile = Join-Path $OutputDir "RucDnit.fmt"

Write-Host "[1/4] Generando archivo de formato..." -ForegroundColor Yellow

# Generar archivo de formato
$bcpFormat = "bcp asiswebapp.dbo.RucDnit format nul -S `"$Server`" -U `"$User`" -P `"$Password`" -n -f `"$formatFile`""
Invoke-Expression $bcpFormat

if (-not (Test-Path $formatFile)) {
    Write-Host "ERROR: No se pudo generar archivo de formato" -ForegroundColor Red
    exit 1
}
Write-Host "   Archivo de formato creado: $formatFile" -ForegroundColor Green

Write-Host "[2/4] Exportando datos (esto puede tardar unos minutos)..." -ForegroundColor Yellow

# Exportar datos
$bcpExport = "bcp asiswebapp.dbo.RucDnit out `"$dataFile`" -S `"$Server`" -U `"$User`" -P `"$Password`" -n"
$result = Invoke-Expression $bcpExport

if (-not (Test-Path $dataFile)) {
    Write-Host "ERROR: No se pudo exportar datos" -ForegroundColor Red
    exit 1
}

$fileSize = (Get-Item $dataFile).Length / 1MB
Write-Host "   Datos exportados: $dataFile ($([math]::Round($fileSize, 2)) MB)" -ForegroundColor Green

Write-Host "[3/4] Comprimiendo archivos..." -ForegroundColor Yellow

$fecha = Get-Date -Format "yyyy.MM.dd"
$zipFile = Join-Path $OutputDir "RucDnit_$fecha.zip"

# Copiar el script de importación
$importScript = @'
# ImportarRucDnit.ps1
# Importa los datos de RucDnit a la base de datos local

param(
    [string]$Server = "",
    [string]$Database = "", 
    [string]$User = "",
    [string]$Password = "",
    [switch]$TrustedConnection = $false
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Importador de RucDnit - SistemIA" -ForegroundColor Cyan
Write-Host "  1,550,608 registros de DNIT" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$dataFile = Join-Path $scriptDir "RucDnit.dat"
$formatFile = Join-Path $scriptDir "RucDnit.fmt"

if (-not (Test-Path $dataFile)) {
    Write-Host "ERROR: No se encontro archivo de datos: $dataFile" -ForegroundColor Red
    Read-Host "Presione Enter para salir"
    exit 1
}

Write-Host "Configuracion de conexion:" -ForegroundColor Yellow
Write-Host ""

# Siempre solicitar servidor si no viene por parametro
if ([string]::IsNullOrEmpty($Server)) {
    Write-Host "Ejemplos de servidor:" -ForegroundColor Gray
    Write-Host "  - .\SQLEXPRESS" -ForegroundColor Gray
    Write-Host "  - localhost\SQL2022" -ForegroundColor Gray
    Write-Host "  - SERVIDOR\INSTANCIA" -ForegroundColor Gray
    Write-Host ""
    $Server = Read-Host "Nombre del servidor SQL"
    if ([string]::IsNullOrEmpty($Server)) {
        Write-Host "ERROR: Debe ingresar un servidor" -ForegroundColor Red
        Read-Host "Presione Enter para salir"
        exit 1
    }
}

# Siempre solicitar base de datos si no viene por parametro
if ([string]::IsNullOrEmpty($Database)) {
    $Database = Read-Host "Nombre de la base de datos (default: asiswebapp)"
    if ([string]::IsNullOrEmpty($Database)) {
        $Database = "asiswebapp"
    }
}

# Solicitar autenticacion
if ([string]::IsNullOrEmpty($User) -and -not $TrustedConnection) {
    Write-Host ""
    $opcion = Read-Host "Usar autenticacion Windows? (S/N, default: N)"
    if ($opcion -eq "S" -or $opcion -eq "s") {
        $TrustedConnection = $true
    } else {
        $User = Read-Host "Usuario SQL (default: sa)"
        if ([string]::IsNullOrEmpty($User)) {
            $User = "sa"
        }
        $SecurePassword = Read-Host "Password" -AsSecureString
        $Password = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecurePassword))
    }
}

Write-Host ""
Write-Host "Configuracion:" -ForegroundColor Yellow
Write-Host "  Servidor: $Server"
Write-Host "  Base de datos: $Database"
Write-Host "  Autenticacion: $(if($TrustedConnection){'Windows'}else{'SQL Server'})"
Write-Host ""

# Construir cadena de conexión para BCP
if ($TrustedConnection) {
    $authParam = "-T"
} else {
    $authParam = "-U `"$User`" -P `"$Password`""
}

Write-Host "[1/3] Verificando conexion..." -ForegroundColor Yellow
$testCmd = "sqlcmd -S `"$Server`" -d `"$Database`" $authParam -Q `"SELECT 1`" -h-1"
try {
    $null = Invoke-Expression $testCmd 2>&1
    Write-Host "   Conexion exitosa" -ForegroundColor Green
} catch {
    Write-Host "ERROR: No se pudo conectar a la base de datos" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host "[2/3] Limpiando tabla existente..." -ForegroundColor Yellow
$truncateCmd = "sqlcmd -S `"$Server`" -d `"$Database`" $authParam -Q `"TRUNCATE TABLE RucDnit`""
Invoke-Expression $truncateCmd
Write-Host "   Tabla limpiada" -ForegroundColor Green

Write-Host "[3/3] Importando datos (esto puede tardar varios minutos)..." -ForegroundColor Yellow
Write-Host "   Archivo: $dataFile" -ForegroundColor Gray

$startTime = Get-Date

$bcpImport = "bcp $Database.dbo.RucDnit in `"$dataFile`" -S `"$Server`" $authParam -n -b 10000"
$result = Invoke-Expression $bcpImport

$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  IMPORTACION COMPLETADA" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Tiempo: $([math]::Round($duration.TotalMinutes, 2)) minutos"
Write-Host ""

# Verificar conteo
$countCmd = "sqlcmd -S `"$Server`" -d `"$Database`" $authParam -Q `"SELECT COUNT(*) FROM RucDnit`" -h-1 -W"
$count = Invoke-Expression $countCmd
Write-Host "  Registros importados: $count" -ForegroundColor Cyan
Write-Host ""

Read-Host "Presione Enter para salir"
'@

$importScriptPath = Join-Path $OutputDir "ImportarRucDnit.ps1"
$importScript | Out-File -FilePath $importScriptPath -Encoding UTF8

# Crear archivo BAT para facilitar ejecución
$batContent = @'
@echo off
echo ========================================
echo   Importador de RucDnit - SistemIA
echo ========================================
echo.
echo Este script importara los datos de RucDnit a su base de datos.
echo.
powershell -ExecutionPolicy Bypass -File "%~dp0ImportarRucDnit.ps1"
pause
'@

$batPath = Join-Path $OutputDir "Importar.bat"
$batContent | Out-File -FilePath $batPath -Encoding ASCII

# Crear README
$readme = @"
# Paquete de Datos RucDnit - SistemIA

## Contenido
- RucDnit.dat: Datos exportados (formato nativo SQL Server)
- RucDnit.fmt: Archivo de formato BCP
- ImportarRucDnit.ps1: Script de importación PowerShell
- Importar.bat: Ejecutar para importar (doble clic)

## Requisitos
- SQL Server instalado (el mismo donde está SistemIA)
- BCP y SQLCMD disponibles (vienen con SQL Server)

## Instrucciones

### Opción 1: Doble clic en Importar.bat
1. Extraiga todos los archivos a una carpeta
2. Doble clic en "Importar.bat"
3. Ingrese los datos de conexión cuando se soliciten

### Opción 2: PowerShell con parámetros
```powershell
.\ImportarRucDnit.ps1 -Server "SERVIDOR\INSTANCIA" -Database "asiswebapp" -User "sa" -Password "supassword"
```

### Opción 3: Autenticación Windows
```powershell
.\ImportarRucDnit.ps1 -Server "SERVIDOR\INSTANCIA" -TrustedConnection
```

## Registros
Este paquete contiene aproximadamente 1,550,608 registros de RUC del DNIT.

## Fecha de generación
$(Get-Date -Format "dd/MM/yyyy HH:mm")
"@

$readmePath = Join-Path $OutputDir "README.md"
$readme | Out-File -FilePath $readmePath -Encoding UTF8

# Comprimir todo
$filesToZip = @($dataFile, $formatFile, $importScriptPath, $batPath, $readmePath)
Compress-Archive -Path $filesToZip -DestinationPath $zipFile -Force

$zipSize = (Get-Item $zipFile).Length / 1MB
Write-Host "   Paquete creado: $zipFile ($([math]::Round($zipSize, 2)) MB)" -ForegroundColor Green

Write-Host ""
Write-Host "[4/4] Limpiando archivos temporales..." -ForegroundColor Yellow
# Mantener los archivos individuales por si se necesitan
Write-Host "   Archivos mantenidos en: $OutputDir" -ForegroundColor Gray

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  EXPORTACION COMPLETADA" -ForegroundColor Green  
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Paquete listo para distribuir:" -ForegroundColor Cyan
Write-Host "  $zipFile" -ForegroundColor White
Write-Host ""
Write-Host "Tamanio: $([math]::Round($zipSize, 2)) MB" -ForegroundColor Cyan
Write-Host ""
