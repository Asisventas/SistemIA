# =====================================================
# Script para exportar artículos de conocimiento nuevos
# Genera SQL para incluir en InicializarDatos.sql
# =====================================================

param(
    [string]$Server = "SERVERSIS\SQL2022",
    [string]$Database = "asiswebapp",
    [string]$User = "sa",
    [string]$OutputFile = ".\ArticulosNuevos.sql"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Exportador de Artículos de Conocimiento" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Solicitar contraseña si no está en variable de entorno
$Password = $env:DB_PASSWORD
if (-not $Password) {
    $securePassword = Read-Host "Contraseña de BD" -AsSecureString
    $Password = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    )
}

# Query para obtener artículos
$query = @"
SELECT 
    'IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = ''' + REPLACE(Titulo, '''', '''''') + ''')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES (''' + ISNULL(Categoria, '') + ''', ' + 
    CASE WHEN Subcategoria IS NULL THEN 'NULL' ELSE '''' + Subcategoria + '''' END + ', ''' + 
    REPLACE(Titulo, '''', '''''') + ''',
''' + REPLACE(REPLACE(Contenido, '''', ''''''), CHAR(10), '
') + ''',
''' + ISNULL(PalabrasClave, '') + ''', ' + 
    CASE WHEN RutaNavegacion IS NULL THEN 'NULL' ELSE '''' + RutaNavegacion + '''' END + ', ''' + 
    ISNULL(Icono, 'bi-question-circle') + ''', ' + 
    CAST(Prioridad AS VARCHAR) + ', GETDATE(), GETDATE(), 1, 0);
' AS SqlInsert
FROM ArticulosConocimiento
WHERE Activo = 1
ORDER BY Categoria, Prioridad DESC, Titulo
"@

Write-Host "Conectando a $Server..." -ForegroundColor Yellow

try {
    # Ejecutar query
    $result = sqlcmd -S $Server -d $Database -U $User -P $Password -Q $query -W -h -1 -s "|" 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error al conectar: $result" -ForegroundColor Red
        exit 1
    }
    
    # Guardar resultado
    $header = @"
-- =====================================================
-- Artículos de Conocimiento - Generado automáticamente
-- Fecha: $(Get-Date -Format "yyyy-MM-dd HH:mm")
-- =====================================================

"@
    
    $header + $result | Out-File -FilePath $OutputFile -Encoding UTF8
    
    Write-Host ""
    Write-Host "✓ Exportado a: $OutputFile" -ForegroundColor Green
    Write-Host ""
    Write-Host "Próximo paso: Copiar contenido relevante a Installer\InicializarDatos.sql" -ForegroundColor Yellow
    
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
