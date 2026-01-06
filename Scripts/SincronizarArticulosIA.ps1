# =====================================================
# Sincronizador de Artículos de Conocimiento
# Detecta artículos nuevos en BD que no están en el script
# =====================================================

param(
    [string]$Server = "SERVERSIS\SQL2022",
    [string]$Database = "asiswebapp",
    [string]$User = "sa",
    [switch]$GenerateSQL
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Sincronizador de Artículos IA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Leer títulos existentes en InicializarDatos.sql
$scriptPath = Join-Path $PSScriptRoot "..\Installer\InicializarDatos.sql"
if (-not (Test-Path $scriptPath)) {
    Write-Host "No se encontró: $scriptPath" -ForegroundColor Red
    exit 1
}

$scriptContent = Get-Content $scriptPath -Raw
$titulosEnScript = [regex]::Matches($scriptContent, "WHERE Titulo = '([^']+)'") | 
    ForEach-Object { $_.Groups[1].Value }

Write-Host "Artículos en InicializarDatos.sql: $($titulosEnScript.Count)" -ForegroundColor Yellow

# Solicitar contraseña
$Password = $env:DB_PASSWORD
if (-not $Password) {
    $securePassword = Read-Host "Contraseña de BD" -AsSecureString
    $Password = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    )
}

# Obtener artículos de BD
$query = "SELECT IdArticulo, Titulo, Categoria FROM ArticulosConocimiento WHERE Activo = 1 ORDER BY Categoria, Titulo"
$resultRaw = sqlcmd -S $Server -d $Database -U $User -P $Password -Q $query -W -h -1 -s "|" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error de conexión: $resultRaw" -ForegroundColor Red
    exit 1
}

# Parsear resultados
$articulosBD = @()
$resultRaw -split "`n" | Where-Object { $_ -match '\|' } | ForEach-Object {
    $parts = $_ -split '\|'
    if ($parts.Count -ge 3) {
        $articulosBD += @{
            Id = $parts[0].Trim()
            Titulo = $parts[1].Trim()
            Categoria = $parts[2].Trim()
        }
    }
}

Write-Host "Artículos en BD: $($articulosBD.Count)" -ForegroundColor Yellow
Write-Host ""

# Detectar nuevos
$nuevos = $articulosBD | Where-Object { $_.Titulo -notin $titulosEnScript }

if ($nuevos.Count -eq 0) {
    Write-Host "✓ Todos los artículos están sincronizados" -ForegroundColor Green
    Write-Host ""
    exit 0
}

Write-Host "⚠ Artículos NUEVOS (no en script): $($nuevos.Count)" -ForegroundColor Yellow
Write-Host "-------------------------------------------" -ForegroundColor Gray
foreach ($art in $nuevos) {
    Write-Host "  [$($art.Categoria)] $($art.Titulo)" -ForegroundColor White
}
Write-Host ""

if ($GenerateSQL) {
    Write-Host "Generando SQL para artículos nuevos..." -ForegroundColor Cyan
    
    $ids = ($nuevos | ForEach-Object { $_.Id }) -join ","
    $queryExport = @"
SELECT 
    'IF NOT EXISTS (SELECT 1 FROM ArticulosConocimiento WHERE Titulo = ''' + REPLACE(Titulo, '''', '''''') + ''')
INSERT INTO ArticulosConocimiento (Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES (''' + ISNULL(Categoria, '') + ''', ' + 
    CASE WHEN Subcategoria IS NULL THEN 'NULL' ELSE '''' + Subcategoria + '''' END + ', ''' + 
    REPLACE(Titulo, '''', '''''') + ''',
''' + REPLACE(REPLACE(Contenido, '''', ''''''), CHAR(10), CHAR(10)) + ''',
''' + ISNULL(PalabrasClave, '') + ''', ' + 
    CASE WHEN RutaNavegacion IS NULL THEN 'NULL' ELSE '''' + RutaNavegacion + '''' END + ', ''' + 
    ISNULL(Icono, 'bi-question-circle') + ''', ' + 
    CAST(Prioridad AS VARCHAR) + ', GETDATE(), GETDATE(), 1, 0);
'
FROM ArticulosConocimiento
WHERE IdArticulo IN ($ids)
"@
    
    $outputFile = Join-Path $PSScriptRoot "ArticulosNuevos_$(Get-Date -Format 'yyyyMMdd_HHmm').sql"
    
    $sqlResult = sqlcmd -S $Server -d $Database -U $User -P $Password -Q $queryExport -W -h -1 2>&1
    
    $header = @"
-- =====================================================
-- Artículos NUEVOS para agregar a InicializarDatos.sql
-- Generado: $(Get-Date -Format "yyyy-MM-dd HH:mm")
-- Cantidad: $($nuevos.Count) artículo(s)
-- =====================================================

"@
    
    $header + $sqlResult | Out-File -FilePath $outputFile -Encoding UTF8
    
    Write-Host ""
    Write-Host "✓ SQL generado: $outputFile" -ForegroundColor Green
    Write-Host ""
    Write-Host "Copiar el contenido a: Installer\InicializarDatos.sql" -ForegroundColor Yellow
}
else {
    Write-Host "Para generar SQL de estos artículos, ejecutar con -GenerateSQL" -ForegroundColor Gray
}
