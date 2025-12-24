<#
.SYNOPSIS
    Instalador de servicio alternativo usando NSSM
.DESCRIPTION
    Instala SistemIA como servicio de Windows usando NSSM para mejor compatibilidad
#>

param(
    [string]$ExePath = "C:\SistemIA\SistemIA.exe",
    [string]$ServiceName = "SistemIA",
    [switch]$Uninstall
)

$nssmUrl = "https://nssm.cc/release/nssm-2.24.zip"
$nssmPath = "$env:TEMP\nssm.zip"
$nssmDir = "$env:TEMP\nssm-2.24"
$nssm = "$nssmDir\win64\nssm.exe"

function Download-NSSM {
    Write-Host "Descargando NSSM..." -ForegroundColor Yellow
    
    if (Test-Path $nssm) {
        Write-Host "NSSM ya está descargado" -ForegroundColor Green
        return $true
    }
    
    try {
        Invoke-WebRequest -Uri $nssmUrl -OutFile $nssmPath
        Expand-Archive -Path $nssmPath -DestinationPath $env:TEMP -Force
        Write-Host "NSSM descargado correctamente" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "Error al descargar NSSM: $_" -ForegroundColor Red
        return $false
    }
}

function Install-ServiceWithNSSM {
    param(
        [string]$Name,
        [string]$ExePath
    )
    
    if (-not (Test-Path $ExePath)) {
        Write-Host "No se encontró el ejecutable: $ExePath" -ForegroundColor Red
        return $false
    }
    
    $exeDir = Split-Path -Parent $ExePath
    
    # Detener y eliminar servicio existente
    & $nssm stop $Name 2>$null
    & $nssm remove $Name confirm 2>$null
    
    Write-Host "Instalando servicio $Name..." -ForegroundColor Yellow
    
    # Instalar servicio
    & $nssm install $Name $ExePath
    
    # Configurar directorio de trabajo
    & $nssm set $Name AppDirectory $exeDir
    
    # Configurar descripción
    & $nssm set $Name Description "Servidor web SistemIA - Sistema de Gestión Empresarial"
    & $nssm set $Name DisplayName "SistemIA Web Server"
    
    # Configurar inicio automático
    & $nssm set $Name Start SERVICE_AUTO_START
    
    # Configurar reinicio automático en caso de fallo
    & $nssm set $Name AppExit Default Restart
    & $nssm set $Name AppRestartDelay 5000
    
    # Configurar logs
    $logPath = Join-Path $exeDir "logs"
    if (-not (Test-Path $logPath)) {
        New-Item -ItemType Directory -Path $logPath -Force | Out-Null
    }
    
    & $nssm set $Name AppStdout "$logPath\service-stdout.log"
    & $nssm set $Name AppStderr "$logPath\service-stderr.log"
    & $nssm set $Name AppStdoutCreationDisposition 4
    & $nssm set $Name AppStderrCreationDisposition 4
    & $nssm set $Name AppRotateFiles 1
    & $nssm set $Name AppRotateBytes 1048576
    
    Write-Host "Servicio instalado correctamente" -ForegroundColor Green
    
    # Iniciar servicio
    Write-Host "Iniciando servicio..." -ForegroundColor Yellow
    & $nssm start $Name
    
    Start-Sleep -Seconds 3
    
    $service = Get-Service -Name $Name -ErrorAction SilentlyContinue
    if ($service.Status -eq 'Running') {
        Write-Host "Servicio iniciado correctamente" -ForegroundColor Green
        return $true
    }
    else {
        Write-Host "El servicio no se inició. Estado: $($service.Status)" -ForegroundColor Yellow
        return $false
    }
}

function Uninstall-ServiceWithNSSM {
    param([string]$Name)
    
    Write-Host "Desinstalando servicio $Name..." -ForegroundColor Yellow
    
    & $nssm stop $Name 2>$null
    & $nssm remove $Name confirm
    
    Write-Host "Servicio desinstalado" -ForegroundColor Green
}

# Main
if (-not (Download-NSSM)) {
    exit 1
}

if ($Uninstall) {
    Uninstall-ServiceWithNSSM -Name $ServiceName
}
else {
    Install-ServiceWithNSSM -Name $ServiceName -ExePath $ExePath
}
