Param()

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data

# Connection string desde appsettings.json
$cs = "Server=SERVERSIS\SQL2022;Database=asiswebapp;User Id=sa;Password=%L4V1CT0R14;TrustServerCertificate=True;"

$cn = New-Object System.Data.SqlClient.SqlConnection($cs)
$cn.Open()
try {
    $cmd = $cn.CreateCommand()
    $cmd.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Compras' AND COLUMN_NAME='MedioPago')
BEGIN
    ALTER TABLE [dbo].[Compras] ADD [MedioPago] NVARCHAR(13) NULL;
END
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Compras' AND COLUMN_NAME='TipoIngreso')
BEGIN
    ALTER TABLE [dbo].[Compras] ADD [TipoIngreso] NVARCHAR(13) NOT NULL CONSTRAINT DF_Compras_TipoIngreso DEFAULT('COMPRA');
END
UPDATE [dbo].[Compras] SET [MedioPago] = 'EFECTIVO' WHERE [MedioPago] IS NULL OR LTRIM(RTRIM([MedioPago]))='';
UPDATE [dbo].[Compras] SET [TipoIngreso] = 'COMPRA' WHERE [TipoIngreso] IS NULL OR LTRIM(RTRIM([TipoIngreso]))='';
"@
    [void]$cmd.ExecuteNonQuery()
    Write-Host "OK: Columnas MedioPago y TipoIngreso aseguradas en dbo.Compras."
}
finally {
    $cn.Close()
}
