param(
    [string]$ConfigPath = "c:\asis\SistemIA\appsettings.json",
    [string]$SqlPath = "c:\asis\SistemIA\Scripts\fix_catalogo_geografico.sql",
    [switch]$DryRun
)
$ErrorActionPreference = 'Stop'
if (!(Test-Path $ConfigPath)) { throw "No existe el archivo de configuración: $ConfigPath" }
if (!(Test-Path $SqlPath)) { throw "No existe el script SQL: $SqlPath" }

# Leer cadena de conexión
$cfg = Get-Content -Raw -Path $ConfigPath | ConvertFrom-Json
$connStr = $cfg.ConnectionStrings.DefaultConnection
if (-not $connStr) { throw "No se encontró ConnectionStrings.DefaultConnection en $ConfigPath" }

# Leer SQL
$sql = Get-Content -Raw -Path $SqlPath

# Si DryRun, forzar @DoCommit = 0 si existe la variable en el script
if ($DryRun) {
    $sql = $sql -replace 'DECLARE\s+@DoCommit\s+bit\s*=\s*1', 'DECLARE @DoCommit bit = 0'
}

# Ejecutar usando System.Data.SqlClient
Add-Type -AssemblyName System.Data
$cn = New-Object System.Data.SqlClient.SqlConnection($connStr)
# Capturar mensajes PRINT/informativos del servidor
$cn.FireInfoMessageEventOnUserErrors = $true
$null = $cn.add_InfoMessage({ param($sender,$e) Write-Host $e.Message })
$cn.Open()
try {
    $cmd = $cn.CreateCommand()
    $cmd.CommandTimeout = 600
    $cmd.CommandText = $sql
    [void]$cmd.ExecuteNonQuery()
    if ($DryRun) {
        Write-Host "Dry-run ejecutado (no se aplicaron cambios). Revise el resumen anterior."
    } else {
        Write-Host "Correcciones aplicadas correctamente en la BD."
    }
}
finally {
    $cn.Close()
}
