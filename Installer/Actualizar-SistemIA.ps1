# ============================================================
# Script de Actualización de SistemIA
# Uso: .\Actualizar-SistemIA.ps1 -ZipPath "C:\ruta\al\paquete.zip"
# ============================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ZipPath,
    
    [string]$InstallPath = "C:\SistemIA",
    [string]$ServiceName = "SistemIA",
    [switch]$NoBackup
)

$ErrorActionPreference = "Stop"

function Write-Log {
    param([string]$Message, [string]$Color = "White")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] $Message" -ForegroundColor $Color
}

function Stop-SistemIAService {
    Write-Log "Deteniendo servicio $ServiceName..." "Yellow"
    
    $wasRunning = $false
    
    # Intentar detener como servicio Windows
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($service) {
        if ($service.Status -eq "Running") {
            Stop-Service -Name $ServiceName -Force
            Start-Sleep -Seconds 3
            Write-Log "Servicio detenido" "Green"
            $wasRunning = $true
        } else {
            Write-Log "El servicio ya estaba detenido" "Cyan"
        }
    }
    
    # Intentar matar proceso SistemIA (por si quedó huérfano)
    $process = Get-Process -Name "SistemIA" -ErrorAction SilentlyContinue
    if ($process) {
        Write-Log "Deteniendo proceso SistemIA huérfano..." "Yellow"
        Stop-Process -Name "SistemIA" -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
        Write-Log "Proceso detenido" "Green"
        $wasRunning = $true
    }
    
    # Matar cualquier proceso que use el puerto 5095
    Stop-ProcessOnPort -Port 5095
    
    # Esperar a que el puerto esté libre
    Wait-PortFree -Port 5095 -TimeoutSeconds 30
    
    if (-not $wasRunning -and -not $service) {
        Write-Log "No se encontró servicio ni proceso en ejecución" "Cyan"
    }
    
    return $wasRunning
}

function Stop-ProcessOnPort {
    param([int]$Port)
    
    Write-Log "Verificando procesos en puerto $Port..." "Cyan"
    
    $connections = netstat -ano | Select-String ":$Port\s" | ForEach-Object {
        if ($_ -match '\s(\d+)$') {
            [int]$matches[1]
        }
    } | Sort-Object -Unique
    
    foreach ($pid in $connections) {
        if ($pid -gt 0) {
            $proc = Get-Process -Id $pid -ErrorAction SilentlyContinue
            if ($proc) {
                Write-Log "  Terminando proceso $($proc.Name) (PID: $pid) en puerto $Port" "Yellow"
                Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
            }
        }
    }
}

function Wait-PortFree {
    param(
        [int]$Port,
        [int]$TimeoutSeconds = 30
    )
    
    Write-Log "Esperando que el puerto $Port se libere..." "Cyan"
    
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    
    while ($stopwatch.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
        $inUse = netstat -ano | Select-String ":$Port\s.*LISTENING"
        
        if (-not $inUse) {
            Write-Log "Puerto $Port liberado" "Green"
            return $true
        }
        
        Start-Sleep -Seconds 1
    }
    
    Write-Log "ADVERTENCIA: Timeout esperando que se libere el puerto $Port" "Yellow"
    return $false
}

function Start-SistemIAService {
    Write-Log "Iniciando servicio $ServiceName..." "Yellow"
    
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($service) {
        # Verificar una vez más que el puerto esté libre antes de iniciar
        $portInUse = netstat -ano | Select-String ":5095\s.*LISTENING"
        if ($portInUse) {
            Write-Log "ADVERTENCIA: Puerto 5095 aún en uso. Intentando liberar..." "Yellow"
            Stop-ProcessOnPort -Port 5095
            Start-Sleep -Seconds 3
        }
        
        Start-Service -Name $ServiceName
        
        # Esperar y verificar que el servicio inició correctamente
        $maxWait = 30
        $waited = 0
        
        while ($waited -lt $maxWait) {
            Start-Sleep -Seconds 2
            $waited += 2
            
            $service = Get-Service -Name $ServiceName
            if ($service.Status -eq "Running") {
                # Verificar que el puerto esté escuchando
                Start-Sleep -Seconds 3
                $portListening = netstat -ano | Select-String ":5095\s.*LISTENING"
                
                if ($portListening) {
                    Write-Log "Servicio iniciado correctamente (puerto 5095 activo)" "Green"
                    return $true
                }
            } elseif ($service.Status -eq "Stopped") {
                Write-Log "ERROR: El servicio se detuvo inesperadamente" "Red"
                Write-Log "Revise los logs en el Visor de Eventos de Windows" "Yellow"
                return $false
            }
        }
        
        Write-Log "ADVERTENCIA: Timeout esperando inicio del servicio" "Yellow"
        return $false
    } else {
        Write-Log "No hay servicio Windows configurado. Inicie la aplicación manualmente." "Yellow"
        return $false
    }
}

# ============================================================
# INICIO DEL SCRIPT
# ============================================================

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "       ACTUALIZACION DE SISTEMIA" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Validar archivo ZIP
if (-not (Test-Path $ZipPath)) {
    Write-Log "ERROR: No se encuentra el archivo ZIP: $ZipPath" "Red"
    exit 1
}

# Validar carpeta de instalación
if (-not (Test-Path $InstallPath)) {
    Write-Log "ERROR: No se encuentra la carpeta de instalación: $InstallPath" "Red"
    exit 1
}

Write-Log "Archivo ZIP: $ZipPath"
Write-Log "Carpeta instalación: $InstallPath"

# Crear backup si no se especificó -NoBackup
if (-not $NoBackup) {
    $backupPath = "C:\backup\SistemIA_Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').zip"
    Write-Log "Creando backup en: $backupPath" "Yellow"
    
    if (-not (Test-Path "C:\backup")) {
        New-Item -ItemType Directory -Path "C:\backup" -Force | Out-Null
    }
    
    try {
        Compress-Archive -Path "$InstallPath\*" -DestinationPath $backupPath -Force
        Write-Log "Backup creado correctamente" "Green"
    } catch {
        Write-Log "ADVERTENCIA: No se pudo crear backup: $_" "Yellow"
    }
}

# Detener aplicación
$wasRunning = Stop-SistemIAService

# Esperar a que se liberen los archivos y el puerto
Write-Log "Esperando liberación de archivos..." "Cyan"
Start-Sleep -Seconds 3

# Extraer actualización
Write-Log "Extrayendo archivos de actualización..." "Yellow"
$tempPath = "$env:TEMP\SistemIA_Update_$(Get-Date -Format 'yyyyMMddHHmmss')"

try {
    Expand-Archive -Path $ZipPath -DestinationPath $tempPath -Force
    Write-Log "Archivos extraídos a: $tempPath" "Green"
} catch {
    Write-Log "ERROR al extraer ZIP: $_" "Red"
    if ($wasRunning) { Start-SistemIAService }
    exit 1
}

# Copiar archivos (excluyendo configuración)
Write-Log "Copiando archivos actualizados..." "Yellow"

$excludeFiles = @(
    "appsettings.json",
    "appsettings.Production.json",
    "appsettings.Development.json"
)

$copiedCount = 0
$errorCount = 0

Get-ChildItem -Path $tempPath -Recurse -File | ForEach-Object {
    $relativePath = $_.FullName.Substring($tempPath.Length + 1)
    $destPath = Join-Path $InstallPath $relativePath
    
    # Excluir archivos de configuración
    if ($excludeFiles -contains $_.Name) {
        Write-Log "  Omitiendo configuración: $($_.Name)" "Cyan"
        return
    }
    
    # Crear directorio si no existe
    $destDir = Split-Path $destPath -Parent
    if (-not (Test-Path $destDir)) {
        New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    }
    
    try {
        Copy-Item -Path $_.FullName -Destination $destPath -Force
        $copiedCount++
    } catch {
        Write-Log "  ERROR copiando: $relativePath - $_" "Red"
        $errorCount++
    }
}

Write-Log "Archivos copiados: $copiedCount" "Green"
if ($errorCount -gt 0) {
    Write-Log "Archivos con error: $errorCount" "Red"
}

# Limpiar temporal
Remove-Item -Path $tempPath -Recurse -Force -ErrorAction SilentlyContinue

# Reiniciar aplicación si estaba corriendo
if ($wasRunning) {
    Start-SistemIAService
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host "       ACTUALIZACION COMPLETADA" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host ""

if ($errorCount -eq 0) {
    Write-Log "La actualización se completó exitosamente" "Green"
} else {
    Write-Log "La actualización se completó con $errorCount errores" "Yellow"
}
