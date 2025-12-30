# Script PowerShell para cargar RUC DNIT usando SqlBulkCopy (RÁPIDO)
# Formato: RUC|NOMBRE|DV|CODIGO|ESTADO|

$connectionString = "Server=SERVERSIS\SQL2022;Database=asiswebapp;User Id=sa;Password=%L4V1CT0R14;TrustServerCertificate=True;"
$carpeta = "c:\asis\SistemIA\DatosRUC"

Write-Host "=== Carga de RUC DNIT ===" -ForegroundColor Cyan
Write-Host "Conectando a SQL Server..."

# Crear DataTable
$dataTable = New-Object System.Data.DataTable
$dataTable.Columns.Add("RUC", [string]) | Out-Null
$dataTable.Columns.Add("RazonSocial", [string]) | Out-Null
$dataTable.Columns.Add("DV", [int]) | Out-Null
$dataTable.Columns.Add("Estado", [string]) | Out-Null
$dataTable.Columns.Add("FechaActualizacion", [datetime]) | Out-Null

$fechaActual = Get-Date

# Limpiar tabla
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "TRUNCATE TABLE RucDnit"
$cmd.ExecuteNonQuery() | Out-Null
Write-Host "Tabla RucDnit limpiada."
$conn.Close()

# Procesar archivos
$archivos = Get-ChildItem "$carpeta\*.txt"
$totalProcesados = 0
$totalInsertados = 0

foreach ($archivo in $archivos) {
    Write-Host "Procesando $($archivo.Name)..." -ForegroundColor Yellow
    $dataTable.Clear()
    
    $lineas = [System.IO.File]::ReadAllLines($archivo.FullName, [System.Text.Encoding]::UTF8)
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
            
            if ($ruc -and $nombre -and $ruc.Length -le 20) {
                $row = $dataTable.NewRow()
                $row["RUC"] = $ruc
                $row["RazonSocial"] = if ($nombre.Length -gt 300) { $nombre.Substring(0, 300) } else { $nombre }
                $row["DV"] = $dv
                $row["Estado"] = if ($estado -and $estado.Length -le 20) { $estado } else { $null }
                $row["FechaActualizacion"] = $fechaActual
                $dataTable.Rows.Add($row)
                $contador++
            }
        }
    }
    
    # Bulk insert
    if ($dataTable.Rows.Count -gt 0) {
        $conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $conn.Open()
        
        $bulkCopy = New-Object System.Data.SqlClient.SqlBulkCopy($conn)
        $bulkCopy.DestinationTableName = "RucDnit"
        $bulkCopy.BatchSize = 10000
        $bulkCopy.BulkCopyTimeout = 600
        
        # Mapear columnas
        $bulkCopy.ColumnMappings.Add("RUC", "RUC") | Out-Null
        $bulkCopy.ColumnMappings.Add("RazonSocial", "RazonSocial") | Out-Null
        $bulkCopy.ColumnMappings.Add("DV", "DV") | Out-Null
        $bulkCopy.ColumnMappings.Add("Estado", "Estado") | Out-Null
        $bulkCopy.ColumnMappings.Add("FechaActualizacion", "FechaActualizacion") | Out-Null
        
        try {
            $bulkCopy.WriteToServer($dataTable)
            $totalInsertados += $dataTable.Rows.Count
            Write-Host "  Insertados: $($dataTable.Rows.Count) registros" -ForegroundColor Green
        }
        catch {
            Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        }
        finally {
            $bulkCopy.Close()
            $conn.Close()
        }
    }
    
    $totalProcesados += $contador
}

Write-Host ""
Write-Host "=========================================="
Write-Host "Total procesados: $totalProcesados" -ForegroundColor Cyan
Write-Host "Total insertados: $totalInsertados" -ForegroundColor Green
Write-Host "=========================================="

# Verificar
Write-Host ""
Write-Host "Verificando primeros 10 registros..."
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TOP 10 RUC, RazonSocial, DV, Estado FROM RucDnit ORDER BY RUC"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Host "$($reader['RUC'])-$($reader['DV']) | $($reader['RazonSocial']) | $($reader['Estado'])"
}
$reader.Close()
$conn.Close()
