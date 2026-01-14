# Script para verificar el contenido del ZIP del SOAP lote
$ErrorActionPreference = 'Stop'

Write-Host "=== Obteniendo SOAP lote de venta 230 ===" -ForegroundColor Cyan

try {
    $resp = Invoke-RestMethod -Uri 'http://localhost:5095/debug/ventas/230/soap-lote' -Method GET -TimeoutSec 30
    
    $payload = $resp.payload
    Write-Host "Payload obtenido correctamente" -ForegroundColor Green
    
    # Extraer el Base64 del xDE
    if ($payload -match 'xDE[^>]*>([A-Za-z0-9+/=]+)<') {
        $b64 = $Matches[1].Trim()
        Write-Host "Base64 encontrado, longitud: $($b64.Length)" -ForegroundColor Green
        
        # Decodificar y guardar como ZIP
        $bytes = [Convert]::FromBase64String($b64)
        $tempZip = "C:\asis\debug_zip_230_test.zip"
        [IO.File]::WriteAllBytes($tempZip, $bytes)
        Write-Host "ZIP guardado en: $tempZip" -ForegroundColor Green
        
        # Verificar firma ZIP (PK)
        $header = [BitConverter]::ToString($bytes[0..3])
        Write-Host "Header ZIP: $header (debe ser 50-4B-03-04)" -ForegroundColor Yellow
        
        # Abrir y leer contenido
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [IO.Compression.ZipFile]::OpenRead($tempZip)
        
        foreach ($entry in $zip.Entries) {
            Write-Host "`n=== Entrada ZIP: $($entry.Name) ===" -ForegroundColor Cyan
            
            $sr = New-Object IO.StreamReader($entry.Open())
            $content = $sr.ReadToEnd()
            $sr.Close()
            
            Write-Host "Longitud contenido: $($content.Length) caracteres" -ForegroundColor Yellow
            
            # Verificar si tiene rLoteDE
            $tieneRLoteDE = $content -match '<rLoteDE'
            Write-Host "Tiene <rLoteDE>: $tieneRLoteDE" -ForegroundColor $(if ($tieneRLoteDE) { 'Green' } else { 'Red' })
            
            # Verificar si tiene rDE
            $tieneRDE = $content -match '<rDE'
            Write-Host "Tiene <rDE>: $tieneRDE" -ForegroundColor $(if ($tieneRDE) { 'Green' } else { 'Red' })
            
            # Mostrar primeros 600 caracteres
            $preview = $content.Substring(0, [Math]::Min(600, $content.Length))
            Write-Host "`n--- Primeros 600 caracteres ---" -ForegroundColor Cyan
            Write-Host $preview
        }
        
        $zip.Dispose()
    }
    else {
        Write-Host "ERROR: No se encontro xDE en payload" -ForegroundColor Red
        Write-Host "Payload (primeros 500 chars): $($payload.Substring(0, [Math]::Min(500, $payload.Length)))"
    }
}
catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
