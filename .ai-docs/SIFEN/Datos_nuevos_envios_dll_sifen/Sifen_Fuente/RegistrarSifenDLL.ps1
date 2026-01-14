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
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Ruta del DLL - MODIFICAR SI ES NECESARIO
$dllPath = "C:\nextsys - GLP\Sifen.dll"

# Verificar que el DLL existe
if (-not (Test-Path $dllPath)) {
    Write-Host "ERROR: No se encontro el archivo: $dllPath" -ForegroundColor Red
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

# Rutas de RegAsm
$regAsm64 = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"
$regAsm32 = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe"

# Registrar para 64 bits
Write-Host "Registrando para 64 bits..." -ForegroundColor Yellow
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

# Registrar para 32 bits
Write-Host "Registrando para 32 bits..." -ForegroundColor Yellow
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
Write-Host " Proceso completado" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Ahora puede abrir PowerBuilder y probar el DLL" -ForegroundColor Green
Write-Host ""
Read-Host "Presione Enter para salir"
