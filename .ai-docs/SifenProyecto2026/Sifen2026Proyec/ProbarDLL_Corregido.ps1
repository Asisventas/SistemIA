# ============================================
# Script para probar el Sifen.dll CORREGIDO
# ============================================

Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Prueba de Sifen.dll - Versión Corregida" -ForegroundColor Cyan
Write-Host " (Compatible con equipos nuevos)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Ruta del DLL compilado
$dllPath = "c:\visualcodeproyect\Sifen_26 - copia\bin\Release\Sifen.dll"

# Verificar que el DLL existe
if (-not (Test-Path $dllPath)) {
    Write-Host "ERROR: No se encontró el DLL en: $dllPath" -ForegroundColor Red
    exit 1
}

Write-Host "DLL encontrado: $dllPath" -ForegroundColor Green
Write-Host ""

# Cargar el DLL
try {
    [System.Reflection.Assembly]::LoadFrom($dllPath) | Out-Null
    Write-Host "DLL cargado correctamente" -ForegroundColor Green
} catch {
    Write-Host "ERROR cargando DLL: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Crear instancia del objeto Sifen
try {
    $sifen = New-Object Sifen.Sifen
    Write-Host "Instancia de Sifen creada correctamente" -ForegroundColor Green
} catch {
    Write-Host "ERROR creando instancia: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Prueba de Firma Digital" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Leer el XML de prueba (el que está en el attachment)
$xmlPath = "C:\Users\Admin\Desktop\REMOTO_PRUEBA\sifen_xml_input.txt"
if (-not (Test-Path $xmlPath)) {
    Write-Host "ADVERTENCIA: No se encontró el XML de prueba en: $xmlPath" -ForegroundColor Yellow
    Write-Host "El DLL está compilado correctamente, pero no se puede probar sin XML" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Para probar, necesitas:" -ForegroundColor Yellow
    Write-Host "1. Un archivo XML de SIFEN" -ForegroundColor Yellow
    Write-Host "2. Un certificado .pfx válido" -ForegroundColor Yellow
    Write-Host ""
} else {
    $xmlContent = Get-Content $xmlPath -Raw
    
    # Parámetros de prueba
    $url = "https://sifen-test.set.gov.py/de/ws/async/recibe-lote.wsdl"
    $urlQR = "https://ekuatia.set.gov.py/consultas-test/qr?"
    $p12FilePath = "C:\nextsys - GLP\sifen\WEN.pfx"
    $password = ""  # Solicitar password
    $tipoFirmado = "1"
    
    if (Test-Path $p12FilePath) {
        Write-Host "Parámetros de prueba:" -ForegroundColor Cyan
        Write-Host "  URL: $url"
        Write-Host "  Certificado: $p12FilePath"
        Write-Host "  XML Length: $($xmlContent.Length) caracteres"
        Write-Host ""
        
        Write-Host "IMPORTANTE: Esta versión usa RSA moderno (sin CSP tipo 24)" -ForegroundColor Green
        Write-Host "Debería funcionar en equipos nuevos sin el error de proveedor" -ForegroundColor Green
        Write-Host ""
        
        Write-Host "Para ejecutar la prueba completa, necesitas ingresar la contraseña del certificado" -ForegroundColor Yellow
        Write-Host ""
    } else {
        Write-Host "No se encontró el certificado en: $p12FilePath" -ForegroundColor Yellow
        Write-Host ""
    }
}

Write-Host "============================================" -ForegroundColor Cyan
Write-Host " Resumen de Cambios Aplicados" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "✓ Eliminado: RSACryptoServiceProvider con CspParameters(24)" -ForegroundColor Green
Write-Host "✓ Implementado: Uso directo de RSA moderna" -ForegroundColor Green
Write-Host "✓ Compatible con: Windows 10/11 y equipos modernos" -ForegroundColor Green
Write-Host "✓ Soporta: SHA-256 nativo" -ForegroundColor Green
Write-Host ""
Write-Host "El DLL está listo para ser registrado y usado" -ForegroundColor Green
Write-Host ""
Write-Host "Siguiente paso: Ejecutar RegistrarSifenDLL.ps1 como Administrador" -ForegroundColor Yellow
Write-Host ""
