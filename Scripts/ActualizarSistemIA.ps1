# ============================================
# Script de Actualización Automática - SistemIA
# ============================================
# Este script facilita la actualización del sistema en el servidor del cliente
#
# Uso:
#   .\ActualizarSistemIA.ps1 -ArchivoZip "C:\Temp\SistemIA_Update.zip"
#
# Opciones:
#   -ArchivoZip        : Ruta al archivo ZIP de actualización (obligatorio)
#   -NoPararServicio   : No detener el servicio (usar solo si actualizas con app corriendo)
#   -NoBackup          : Omitir creación de backups (NO RECOMENDADO)
#   -NoMigraciones     : No aplicar migraciones de base de datos
# ============================================

param(
    [Parameter(Mandatory=$true, HelpMessage="Ruta al archivo ZIP de actualización")]
    [string]$ArchivoZip,
    
    [Parameter(Mandatory=$false)]
    [switch]$NoPararServicio = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$NoBackup = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$NoMigraciones = $false,
    
    [Parameter(Mandatory=$false)]
    [string]$AppPath = "C:\Apps\SistemIA",
    
    [Parameter(Mandatory=$false)]
    [string]$BackupPath = "C:\Backups\SistemIA",
    
    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "SistemIA"
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "Continue"

# Colores para mensajes
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Write-Step {
    param([string]$Message)
    Write-ColorOutput "`n[$(Get-Date -Format 'HH:mm:ss')] $Message" "Cyan"
}

function Write-Success {
    param([string]$Message)
    Write-ColorOutput "✓ $Message" "Green"
}

function Write-Error-Custom {
    param([string]$Message)
    Write-ColorOutput "✗ $Message" "Red"
}

function Write-Warning-Custom {
    param([string]$Message)
    Write-ColorOutput "⚠ $Message" "Yellow"
}

# Verificar ejecución como administrador
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Banner
Clear-Host
Write-ColorOutput "============================================" "Magenta"
Write-ColorOutput "   ACTUALIZACIÓN DE SistemIA" "Magenta"
Write-ColorOutput "============================================`n" "Magenta"

# Validaciones previas
Write-Step "Validando requisitos..."

if (-not (Test-Administrator)) {
    Write-Error-Custom "Este script debe ejecutarse como Administrador."
    Write-Host "`nEjecute PowerShell como Administrador e intente nuevamente.`n"
    exit 1
}
Write-Success "Ejecutando como Administrador"

if (-not (Test-Path $ArchivoZip)) {
    Write-Error-Custom "Archivo ZIP no encontrado: $ArchivoZip"
    exit 1
}
Write-Success "Archivo ZIP válido: $(Split-Path -Leaf $ArchivoZip)"

if (-not (Test-Path $AppPath)) {
    Write-Error-Custom "Directorio de aplicación no encontrado: $AppPath"
    exit 1
}
Write-Success "Directorio de aplicación: $AppPath"

# Mostrar resumen
Write-ColorOutput "`n────────────────────────────────────────────" "Yellow"
Write-ColorOutput "RESUMEN DE ACTUALIZACIÓN" "Yellow"
Write-ColorOutput "────────────────────────────────────────────" "Yellow"
Write-Host "Archivo:           $ArchivoZip"
Write-Host "Tamaño:            $([math]::Round((Get-Item $ArchivoZip).Length / 1MB, 2)) MB"
Write-Host "Aplicación:        $AppPath"
Write-Host "Backups:           $BackupPath"
Write-Host "Servicio:          $ServiceName"
Write-Host "Detener servicio:  $(-not $NoPararServicio)"
Write-Host "Crear backup:      $(-not $NoBackup)"
Write-Host "Migraciones BD:    $(-not $NoMigraciones)"
Write-ColorOutput "────────────────────────────────────────────`n" "Yellow"

# Confirmación
$confirmacion = Read-Host "¿Desea continuar con la actualización? (S/N)"
if ($confirmacion -notmatch '^[Ss]$') {
    Write-ColorOutput "`nActualización cancelada por el usuario.`n" "Yellow"
    exit 0
}

try {
    $tiempoInicio = Get-Date

    # ========================================
    # 1. DETENER SERVICIO
    # ========================================
    if (-not $NoPararServicio) {
        Write-Step "Deteniendo servicio $ServiceName..."
        
        $servicio = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        if ($servicio) {
            if ($servicio.Status -eq "Running") {
                Stop-Service -Name $ServiceName -Force
                Start-Sleep -Seconds 3
                
                # Verificar que se detuvo
                $servicio = Get-Service -Name $ServiceName
                if ($servicio.Status -ne "Stopped") {
                    throw "No se pudo detener el servicio"
                }
                Write-Success "Servicio detenido correctamente"
            } else {
                Write-Success "El servicio ya estaba detenido"
            }
        } else {
            Write-Warning-Custom "Servicio '$ServiceName' no encontrado (puede estar corriendo manualmente)"
        }
        
        # Forzar cierre de procesos
        $procesos = Get-Process | Where-Object { $_.ProcessName -like "*SistemIA*" }
        if ($procesos) {
            Write-Warning-Custom "Forzando cierre de procesos de SistemIA..."
            $procesos | Stop-Process -Force
            Start-Sleep -Seconds 2
        }
    }

    # ========================================
    # 2. CREAR BACKUP
    # ========================================
    if (-not $NoBackup) {
        Write-Step "Creando backup de la aplicación..."
        
        $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
        $backupDir = $BackupPath
        if (-not (Test-Path $backupDir)) {
            New-Item -Path $backupDir -ItemType Directory -Force | Out-Null
        }
        
        $backupZip = Join-Path $backupDir "SistemIA_Backup_$timestamp.zip"
        
        Write-Host "   Comprimiendo: $AppPath"
        Write-Host "   Destino: $backupZip"
        
        # Excluir logs y temporales
        $excludePaths = @("logs", "temp", "Temp", ".vs")
        $filesToBackup = Get-ChildItem -Path $AppPath -Recurse -File | 
                         Where-Object { 
                             $exclude = $false
                             foreach ($path in $excludePaths) {
                                 if ($_.FullName -like "*\$path\*") {
                                     $exclude = $true
                                     break
                                 }
                             }
                             -not $exclude -and -not $_.Name.EndsWith(".log")
                         }
        
        if ($filesToBackup) {
            Compress-Archive -Path $filesToBackup.FullName -DestinationPath $backupZip -CompressionLevel Fastest
            Write-Success "Backup creado: $(Split-Path -Leaf $backupZip)"
            Write-Host "   Tamaño: $([math]::Round((Get-Item $backupZip).Length / 1MB, 2)) MB"
        } else {
            Write-Warning-Custom "No se encontraron archivos para respaldar"
        }
    }

    # ========================================
    # 3. GUARDAR CONFIGURACIÓN
    # ========================================
    Write-Step "Preservando archivos de configuración..."
    
    $configFiles = @("appsettings.json", "appsettings.Production.json", "appsettings.Development.json")
    $configBackup = @{}
    
    foreach ($configFile in $configFiles) {
        $configPath = Join-Path $AppPath $configFile
        if (Test-Path $configPath) {
            $configBackup[$configFile] = Get-Content -Path $configPath -Raw
            Write-Success "Guardado: $configFile"
        }
    }

    # ========================================
    # 4. EXTRAER ACTUALIZACIÓN
    # ========================================
    Write-Step "Extrayendo actualización..."
    
    $tempExtract = Join-Path $env:TEMP "SistemIA_Update_$timestamp"
    if (Test-Path $tempExtract) {
        Remove-Item -Path $tempExtract -Recurse -Force
    }
    New-Item -Path $tempExtract -ItemType Directory -Force | Out-Null
    
    Expand-Archive -Path $ArchivoZip -DestinationPath $tempExtract -Force
    Write-Success "Archivos extraídos a: $tempExtract"
    
    $fileCount = (Get-ChildItem -Path $tempExtract -Recurse -File).Count
    Write-Host "   Archivos en actualización: $fileCount"

    # ========================================
    # 5. COPIAR ARCHIVOS NUEVOS
    # ========================================
    Write-Step "Actualizando archivos de la aplicación..."
    
    $archivos = Get-ChildItem -Path $tempExtract -Recurse -File
    $copiedCount = 0
    
    foreach ($archivo in $archivos) {
        $relativePath = $archivo.FullName.Substring($tempExtract.Length + 1)
        $destPath = Join-Path $AppPath $relativePath
        
        # Saltar archivos de configuración
        if ($configFiles -contains $archivo.Name) {
            Write-Host "   Omitiendo: $relativePath (configuración)"
            continue
        }
        
        # Crear directorio si no existe
        $destDir = Split-Path -Parent $destPath
        if (-not (Test-Path $destDir)) {
            New-Item -Path $destDir -ItemType Directory -Force | Out-Null
        }
        
        # Copiar archivo
        Copy-Item -Path $archivo.FullName -Destination $destPath -Force
        $copiedCount++
    }
    
    Write-Success "Archivos actualizados: $copiedCount"

    # ========================================
    # 6. RESTAURAR CONFIGURACIÓN
    # ========================================
    Write-Step "Restaurando configuración..."
    
    foreach ($config in $configBackup.GetEnumerator()) {
        $configPath = Join-Path $AppPath $config.Key
        Set-Content -Path $configPath -Value $config.Value -Force
        Write-Success "Restaurado: $($config.Key)"
    }

    # ========================================
    # 7. APLICAR MIGRACIONES (OPCIONAL)
    # ========================================
    if (-not $NoMigraciones) {
        Write-Step "Aplicando migraciones de base de datos..."
        
        $efToolsInstalled = & dotnet tool list --global | Select-String "dotnet-ef"
        if (-not $efToolsInstalled) {
            Write-Warning-Custom "dotnet-ef no está instalado. Instalando..."
            & dotnet tool install --global dotnet-ef
        }
        
        Push-Location $AppPath
        try {
            $migrationOutput = & dotnet ef database update --no-build 2>&1
            $exitCode = $LASTEXITCODE
            
            if ($exitCode -eq 0) {
                Write-Success "Migraciones aplicadas correctamente"
                Write-Host ($migrationOutput | Out-String)
            } else {
                Write-Warning-Custom "Error al aplicar migraciones:"
                Write-Host ($migrationOutput | Out-String)
                Write-Warning-Custom "Puede aplicar migraciones manualmente más tarde."
            }
        }
        finally {
            Pop-Location
        }
    }

    # ========================================
    # 8. LIMPIAR TEMPORALES
    # ========================================
    Write-Step "Limpiando archivos temporales..."
    
    if (Test-Path $tempExtract) {
        Remove-Item -Path $tempExtract -Recurse -Force
    }
    Write-Success "Limpieza completada"

    # ========================================
    # 9. INICIAR SERVICIO
    # ========================================
    if (-not $NoPararServicio) {
        Write-Step "Iniciando servicio $ServiceName..."
        
        $servicio = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        if ($servicio) {
            Start-Service -Name $ServiceName
            Start-Sleep -Seconds 5
            
            $servicio = Get-Service -Name $ServiceName
            if ($servicio.Status -eq "Running") {
                Write-Success "Servicio iniciado correctamente"
            } else {
                Write-Warning-Custom "El servicio no inició automáticamente. Estado: $($servicio.Status)"
                Write-Warning-Custom "Inicie el servicio manualmente: Start-Service $ServiceName"
            }
        } else {
            Write-Warning-Custom "Servicio no encontrado. Inicie la aplicación manualmente."
        }
    }

    # ========================================
    # RESUMEN FINAL
    # ========================================
    $tiempoFin = Get-Date
    $tiempoTotal = ($tiempoFin - $tiempoInicio).TotalSeconds

    Write-ColorOutput "`n════════════════════════════════════════════" "Green"
    Write-ColorOutput "   ✓ ACTUALIZACIÓN COMPLETADA EXITOSAMENTE" "Green"
    Write-ColorOutput "════════════════════════════════════════════" "Green"
    Write-Host "Tiempo total: $([math]::Round($tiempoTotal, 1)) segundos"
    Write-Host "Archivos copiados: $copiedCount"
    if (-not $NoBackup) {
        Write-Host "Backup: $backupZip"
    }
    Write-ColorOutput "`n⚠ IMPORTANTE: Verifique que la aplicación funcione correctamente.`n" "Yellow"

    # Limpiar backups antiguos (mantener últimos 5)
    if (-not $NoBackup -and (Test-Path $BackupPath)) {
        Write-Step "Limpiando backups antiguos..."
        $backupsAntiguos = Get-ChildItem -Path $BackupPath -Filter "SistemIA_Backup_*.zip" | 
                           Sort-Object CreationTime -Descending | 
                           Select-Object -Skip 5
        
        if ($backupsAntiguos) {
            foreach ($backup in $backupsAntiguos) {
                Remove-Item -Path $backup.FullName -Force
            }
            Write-Success "Eliminados $($backupsAntiguos.Count) backup(s) antiguos"
        }
    }

    exit 0

} catch {
    Write-ColorOutput "`n════════════════════════════════════════════" "Red"
    Write-ColorOutput "   ✗ ERROR EN LA ACTUALIZACIÓN" "Red"
    Write-ColorOutput "════════════════════════════════════════════" "Red"
    Write-Host "Error: $($_.Exception.Message)"
    Write-Host "Línea: $($_.InvocationInfo.ScriptLineNumber)"
    
    Write-ColorOutput "`n⚠ ROLLBACK RECOMENDADO:" "Yellow"
    Write-Host "Si se creó backup, puede restaurarlo manualmente desde:"
    Write-Host "   $BackupPath"
    
    if (-not $NoPararServicio) {
        Write-Host "`nIntentando reiniciar el servicio..."
        try {
            Start-Service -Name $ServiceName -ErrorAction SilentlyContinue
        } catch {
            Write-Warning-Custom "No se pudo reiniciar el servicio automáticamente."
        }
    }
    
    Write-Host ""
    exit 1
}
