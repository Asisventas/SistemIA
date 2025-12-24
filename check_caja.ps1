$conn = New-Object System.Data.SqlClient.SqlConnection
$conn.ConnectionString = "Server=(localdb)\MSSQLLocalDB;Database=SistemIA;Integrated Security=true;"
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT id_caja, CajaActual, Imprimir_Factura FROM Cajas WHERE CajaActual = 1"
$reader = $cmd.ExecuteReader()
while($reader.Read()) {
    Write-Host "id_caja: $($reader['id_caja'])"
    Write-Host "CajaActual: $($reader['CajaActual'])"
    Write-Host "Imprimir_Factura: $($reader['Imprimir_Factura'])"
}
$reader.Close()
$conn.Close()
