# ============================================================
# Script de Instalación - SistemIA Actualizador
# Instala como SERVICIO DE WINDOWS para inicio automático
# EJECUTAR COMO ADMINISTRADOR
# ============================================================

param(
    [switch]$Desinstalar
)

$ErrorActionPreference = "Stop"

# Configuración
$NombreServicio = "SistemIA.Actualizador"
$NombreMostrar = "SistemIA Actualizador"
$Descripcion = "Servicio de actualización automática de SistemIA - Puerto 5096"
$RutaActualizador = Join-Path $PSScriptRoot "SistemIA.Actualizador.exe"

# Verificar si se ejecuta como administrador
$esAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $esAdmin) {
    Write-Host "X Este script debe ejecutarse como ADMINISTRADOR" -ForegroundColor Red
    Write-Host "   Haga clic derecho -> 'Ejecutar como administrador'" -ForegroundColor Yellow
    Read-Host "Presione Enter para salir"
    exit 1
}

# ============================================================
# DESINSTALACIÓN
# ============================================================
if ($Desinstalar) {
    Write-Host "============================================================" -ForegroundColor Yellow
    Write-Host "   DESINSTALANDO SISTEMÍA ACTUALIZADOR" -ForegroundColor Yellow
    Write-Host "============================================================" -ForegroundColor Yellow
    Write-Host ""
    
    # Verificar si el servicio existe
    $servicio = Get-Service -Name $NombreServicio -ErrorAction SilentlyContinue
    
    if ($servicio) {
        # Detener el servicio si está corriendo
        if ($servicio.Status -eq 'Running') {
            Write-Host "Deteniendo servicio..." -ForegroundColor Gray
            Stop-Service -Name $NombreServicio -Force
            Start-Sleep -Seconds 2
        }
        
        # Eliminar el servicio
        Write-Host "Eliminando servicio de Windows..." -ForegroundColor Gray
        sc.exe delete $NombreServicio | Out-Null
        Start-Sleep -Seconds 1
        
        Write-Host "[OK] Servicio eliminado correctamente" -ForegroundColor Green
    } else {
        Write-Host "   El servicio no estaba instalado" -ForegroundColor Gray
    }
    
    # También detener el proceso si está corriendo como app normal
    $proceso = Get-Process -Name "SistemIA.Actualizador" -ErrorAction SilentlyContinue
    if ($proceso) {
        Stop-Process -Name "SistemIA.Actualizador" -Force -ErrorAction SilentlyContinue
        Write-Host "[OK] Proceso detenido" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "[OK] SistemIA Actualizador desinstalado correctamente" -ForegroundColor Green
    Write-Host ""
    Read-Host "Presione Enter para salir"
    exit 0
}

# ============================================================
# INSTALACIÓN
# ============================================================

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   INSTALACION DE SISTEMÍA ACTUALIZADOR" -ForegroundColor Cyan
Write-Host "   (Como Servicio de Windows)" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que existe el ejecutable
if (-not (Test-Path $RutaActualizador)) {
    Write-Host "X No se encontro el ejecutable: $RutaActualizador" -ForegroundColor Red
    Read-Host "Presione Enter para salir"
    exit 1
}

Write-Host "Ruta del Actualizador: $RutaActualizador" -ForegroundColor White
Write-Host ""

# Verificar si el servicio ya existe
$servicioExistente = Get-Service -Name $NombreServicio -ErrorAction SilentlyContinue

if ($servicioExistente) {
    Write-Host "!! Ya existe un servicio con este nombre" -ForegroundColor Yellow
    $respuesta = Read-Host "Desea reinstalar? (S/N)"
    if ($respuesta -ne "S" -and $respuesta -ne "s") {
        exit 0
    }
    
    # Detener y eliminar el servicio existente
    if ($servicioExistente.Status -eq 'Running') {
        Write-Host "   Deteniendo servicio existente..." -ForegroundColor Gray
        Stop-Service -Name $NombreServicio -Force
        Start-Sleep -Seconds 2
    }
    
    Write-Host "   Eliminando servicio existente..." -ForegroundColor Gray
    sc.exe delete $NombreServicio | Out-Null
    Start-Sleep -Seconds 2
}

# Detener proceso si está corriendo como aplicación normal
$proceso = Get-Process -Name "SistemIA.Actualizador" -ErrorAction SilentlyContinue
if ($proceso) {
    Write-Host "   Deteniendo proceso existente..." -ForegroundColor Gray
    Stop-Process -Name "SistemIA.Actualizador" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
}

# Crear el servicio de Windows
Write-Host "Creando servicio de Windows..." -ForegroundColor White

try {
    # Usar sc.exe para crear el servicio
    $resultado = sc.exe create $NombreServicio binPath= "`"$RutaActualizador`"" start= auto DisplayName= "$NombreMostrar"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Servicio creado exitosamente" -ForegroundColor Green
    } else {
        throw "Error al crear servicio: $resultado"
    }
    
    # Configurar descripción del servicio
    sc.exe description $NombreServicio "$Descripcion" | Out-Null
    
    # Configurar recuperación automática (reiniciar si falla)
    sc.exe failure $NombreServicio reset= 86400 actions= restart/5000/restart/10000/restart/30000 | Out-Null
    Write-Host "[OK] Configurada recuperacion automatica" -ForegroundColor Green
    
} catch {
    Write-Host "X Error al crear el servicio: $_" -ForegroundColor Red
    Read-Host "Presione Enter para salir"
    exit 1
}

# Iniciar el servicio
Write-Host ""
Write-Host "Iniciando el servicio..." -ForegroundColor White

try {
    Start-Service -Name $NombreServicio
    Start-Sleep -Seconds 3
    
    $servicio = Get-Service -Name $NombreServicio
    if ($servicio.Status -eq 'Running') {
        Write-Host "[OK] Servicio iniciado correctamente" -ForegroundColor Green
    } else {
        Write-Host "!! El servicio no inicio. Estado: $($servicio.Status)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "X Error al iniciar el servicio: $_" -ForegroundColor Red
}

# Verificar puerto
Write-Host ""
Write-Host "Verificando puerto 5096..." -ForegroundColor White
Start-Sleep -Seconds 2

$puerto = netstat -ano | Select-String ":5096.*LISTENING"
if ($puerto) {
    Write-Host "[OK] Puerto 5096 activo - Actualizador funcionando" -ForegroundColor Green
} else {
    Write-Host "!! Puerto 5096 no detectado aun (puede tardar unos segundos)" -ForegroundColor Yellow
}

# Resumen
Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   INSTALACION COMPLETADA" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "El Actualizador se ejecuta como Servicio de Windows" -ForegroundColor White
Write-Host "Se inicia automaticamente con el sistema" -ForegroundColor White
Write-Host "Acceda desde: http://localhost:5096" -ForegroundColor White
Write-Host ""
Write-Host "Comandos utiles:" -ForegroundColor Gray
Write-Host "  - Ver estado:   sc.exe query $NombreServicio" -ForegroundColor Gray
Write-Host "  - Detener:      sc.exe stop $NombreServicio" -ForegroundColor Gray
Write-Host "  - Iniciar:      sc.exe start $NombreServicio" -ForegroundColor Gray
Write-Host "  - Desinstalar:  .\InstalarServicioActualizador.ps1 -Desinstalar" -ForegroundColor Gray
Write-Host ""
Read-Host "Presione Enter para salir"
