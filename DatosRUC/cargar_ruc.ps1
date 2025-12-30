# Script PowerShell para cargar RUC DNIT en SQL Server
# Formato: RUC|NOMBRE|DV|CODIGO|ESTADO|

$connectionString = "Server=SERVERSIS\SQL2022;Database=asiswebapp;User Id=sa;Password=%L4V1CT0R14;TrustServerCertificate=True;"
$carpeta = "c:\asis\SistemIA\DatosRUC"

# Función para insertar batch
function Insert-RucBatch {
    param(
        [string]$connectionString,
        [System.Collections.Generic.List[PSObject]]$batch
    )
    
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    $transaction = $connection.BeginTransaction()
    
    try {
        foreach ($item in $batch) {
            $cmd = $connection.CreateCommand()
            $cmd.Transaction = $transaction
            $cmd.CommandText = @"
INSERT INTO RucDnit (RUC, RazonSocial, DV, Estado, FechaActualizacion)
VALUES (@RUC, @RazonSocial, @DV, @Estado, GETDATE())
"@
            $cmd.Parameters.AddWithValue("@RUC", $item.RUC) | Out-Null
            $cmd.Parameters.AddWithValue("@RazonSocial", $item.RazonSocial) | Out-Null
            $cmd.Parameters.AddWithValue("@DV", $item.DV) | Out-Null
            $cmd.Parameters.AddWithValue("@Estado", $item.Estado) | Out-Null
            $cmd.ExecuteNonQuery() | Out-Null
        }
        $transaction.Commit()
        return $true
    }
    catch {
        $transaction.Rollback()
        Write-Host "Error en batch: $($_.Exception.Message)"
        return $false
    }
    finally {
        $connection.Close()
    }
}

# Limpiar tabla primero
Write-Host "Limpiando tabla RucDnit..."
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()
$cmdTruncate = $conn.CreateCommand()
$cmdTruncate.CommandText = "TRUNCATE TABLE RucDnit"
$cmdTruncate.ExecuteNonQuery() | Out-Null
$conn.Close()
Write-Host "Tabla limpiada."

# Procesar cada archivo
$archivos = Get-ChildItem "$carpeta\*.txt"
$totalInsertados = 0
$batchSize = 1000

foreach ($archivo in $archivos) {
    Write-Host "Procesando $($archivo.Name)..."
    $lineas = Get-Content $archivo.FullName -Encoding UTF8
    $batch = New-Object System.Collections.Generic.List[PSObject]
    $contador = 0
    
    foreach ($linea in $lineas) {
        if ([string]::IsNullOrWhiteSpace($linea)) { continue }
        
        $partes = $linea.Split('|')
        if ($partes.Count -ge 5) {
            $ruc = $partes[0].Trim()
            $nombre = $partes[1].Trim()
            $dv = 0
            [int]::TryParse($partes[2].Trim(), [ref]$dv) | Out-Null
            $estado = $partes[4].Trim()
            
            if ($ruc -and $nombre) {
                $obj = [PSCustomObject]@{
                    RUC = $ruc.Substring(0, [Math]::Min(20, $ruc.Length))
                    RazonSocial = $nombre.Substring(0, [Math]::Min(300, $nombre.Length))
                    DV = $dv
                    Estado = if ($estado) { $estado.Substring(0, [Math]::Min(20, $estado.Length)) } else { $null }
                }
                $batch.Add($obj)
                $contador++
                
                if ($batch.Count -ge $batchSize) {
                    Insert-RucBatch -connectionString $connectionString -batch $batch
                    $totalInsertados += $batch.Count
                    $batch.Clear()
                    Write-Host "  Procesados: $contador"
                }
            }
        }
    }
    
    # Insertar batch restante
    if ($batch.Count -gt 0) {
        Insert-RucBatch -connectionString $connectionString -batch $batch
        $totalInsertados += $batch.Count
    }
    
    Write-Host "  $($archivo.Name) completado: $contador registros"
}

Write-Host ""
Write-Host "=========================================="
Write-Host "Total registros insertados: $totalInsertados"
Write-Host "=========================================="
