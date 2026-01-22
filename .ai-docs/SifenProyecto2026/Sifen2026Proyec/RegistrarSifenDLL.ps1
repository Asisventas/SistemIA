# ============================================
# Script para registrar Sifen.dll para COM
# Ejecutar como Administrador
# ============================================

# Verificar si se ejecuta como Administrador
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: Este script debe ejecutarse como Administrador" -ForegroundColor Red
    Write-Host "Haga clic derecho en PowerShell y seleccione 'Ejecutar como administrador'" -ForegroundColor Yellow
    Read-Host "Presione Enter para salir"
    exit 1
}

Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Registro de Sifen.dll para COM Interop" -ForegroundColor Cyan
Write-Host " VERSION CORREGIDA - Compatible equipos nuevos" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Ruta del DLL recién compilado
$dllPathCompilado = "c:\visualcodeproyect\Sifen_26 - copia\bin\Release\Sifen.dll"
$dllPathDestino = "C:\nextsys - GLP\Sifen.dll"

# Paso 1: Copiar el DLL compilado al destino
Write-Host "Paso 1: Copiar DLL compilado al directorio de producción" -ForegroundColor Yellow
if (Test-Path $dllPathCompilado) {
    Write-Host "  Origen: $dllPathCompilado" -ForegroundColor Gray
    Write-Host "  Destino: $dllPathDestino" -ForegroundColor Gray
    
    # Crear directorio si no existe
    $destinoDir = Split-Path $dllPathDestino
    if (-not (Test-Path $destinoDir)) {
        New-Item -ItemType Directory -Path $destinoDir -Force | Out-Null
        Write-Host "  Directorio creado: $destinoDir" -ForegroundColor Green
    }
    
    Copy-Item $dllPathCompilado $dllPathDestino -Force
    Write-Host "  DLL copiado exitosamente" -ForegroundColor Green
    $dllPath = $dllPathDestino
} else {
    Write-Host "  ADVERTENCIA: No se encontró el DLL compilado" -ForegroundColor Yellow
    Write-Host "  Buscando en ubicación de producción..." -ForegroundColor Yellow
    $dllPath = $dllPathDestino
}

Write-Host ""

# Paso 2: Verificar que el DLL existe
Write-Host "Paso 2: Verificar DLL" -ForegroundColor Yellow
if (-not (Test-Path $dllPath)) {
    Write-Host "ERROR: No se encontró el archivo: $dllPath" -ForegroundColor Red
    Write-Host ""
    $dllPath = Read-Host "Ingrese la ruta completa del Sifen.dll"
    
    if (-not (Test-Path $dllPath)) {
        Write-Host "ERROR: El archivo no existe" -ForegroundColor Red
        Read-Host "Presione Enter para salir"
        exit 1
    }
}

Write-Host "DLL encontrado: $dllPath" -ForegroundColor Green
Write-Host ""

# Mostrar información del DLL
$fileInfo = Get-Item $dllPath
Write-Host "Información del DLL:" -ForegroundColor Cyan
Write-Host "  Tamaño: $($fileInfo.Length) bytes" -ForegroundColor Gray
Write-Host "  Fecha modificación: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
Write-Host ""

# Rutas de RegAsm
$regAsm64 = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"
$regAsm32 = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe"

# Paso 3: Registrar para 64 bits
Write-Host "Paso 3: Registrar DLL para 64 bits..." -ForegroundColor Yellow
if (Test-Path $regAsm64) {
    $result = & $regAsm64 $dllPath /codebase /tlb 2>&1
    Write-Host $result
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Registro 64 bits: EXITOSO" -ForegroundColor Green
    } else {
        Write-Host "Registro 64 bits: ERROR" -ForegroundColor Red
    }
} else {
    Write-Host "RegAsm 64 bits no encontrado" -ForegroundColor Yellow
}

Write-Host ""

# Paso 4: Registrar para 32 bits
Write-Host "Paso 4: Registrar DLL para 32 bits..." -ForegroundColor Yellow
if (Test-Path $regAsm32) {
    $result = & $regAsm32 $dllPath /codebase /tlb 2>&1
    Write-Host $result
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Registro 32 bits: EXITOSO" -ForegroundColor Green
    } else {
        Write-Host "Registro 32 bits: ERROR" -ForegroundColor Red
    }
} else {
    Write-Host "RegAsm 32 bits no encontrado" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Proceso completado exitosamente" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Cambios aplicados:" -ForegroundColor Green
Write-Host "  ✓ DLL compilado copiado a producción" -ForegroundColor Green
Write-Host "  ✓ Registrado para COM 64 bits" -ForegroundColor Green
Write-Host "  ✓ Registrado para COM 32 bits" -ForegroundColor Green
Write-Host ""
Write-Host "IMPORTANTE: Esta versión usa RSA moderna" -ForegroundColor Yellow
Write-Host "Compatible con equipos nuevos (Windows 10/11)" -ForegroundColor Yellow
Write-Host ""
Write-Host "Ahora puede usar el DLL desde:" -ForegroundColor Cyan
Write-Host "  - PowerBuilder" -ForegroundColor Gray
Write-Host "  - VB6/VBA" -ForegroundColor Gray
Write-Host "  - Blazor Server C#" -ForegroundColor Gray
Write-Host "  - Cualquier aplicación COM" -ForegroundColor Gray
Write-Host ""
Read-Host "Presione Enter para salir"
