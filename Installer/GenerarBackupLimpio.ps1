<#
.SYNOPSIS
    Genera un backup limpio de la base de datos de desarrollo para distribucion
.DESCRIPTION
    Este script:
    1. Crea un backup de la BD de desarrollo
    2. Restaura en una BD temporal
    3. Limpia datos transaccionales (ventas, compras, etc.)
    4. Mantiene catalogos y configuraciones base
    5. Crea un backup limpio para el instalador
.NOTES
    Ejecutar desde el servidor de desarrollo
#>

param(
    [string]$ServidorSQL = "SERVERSIS\SQL2022",
    [string]$BaseDatosOrigen = "asiswebapp",
    [string]$BaseDatosTemp = "SistemIA_Temp_Clean",
    [string]$RutaSalida = ".\SistemIA_Base.bak",
    [string]$Usuario = "sa",
    [string]$Password = "%L4V1CT0R14"
)

# Colores
function Write-Title { param($msg) Write-Host "`n====== $msg ======" -ForegroundColor Cyan }
function Write-Success { param($msg) Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-ErrorMsg { param($msg) Write-Host "[ERROR] $msg" -ForegroundColor Red }
function Write-Info { param($msg) Write-Host "[INFO] $msg" -ForegroundColor White }

Write-Host ""
Write-Host "=========================================================" -ForegroundColor Cyan
Write-Host "       GENERADOR DE BACKUP LIMPIO PARA INSTALADOR        " -ForegroundColor Cyan
Write-Host "=========================================================" -ForegroundColor Cyan

Write-Title "CONFIGURACION"
Write-Info "Servidor SQL: $ServidorSQL"
Write-Info "BD Origen: $BaseDatosOrigen"
Write-Info "BD Temporal: $BaseDatosTemp"
Write-Info "Ruta Salida: $RutaSalida"

$connectionString = "Server=$ServidorSQL;Database=master;User Id=$Usuario;Password=$Password;TrustServerCertificate=True;"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    $cmd = $connection.CreateCommand()
    $cmd.CommandTimeout = 600

    # PASO 1: Backup de la BD de desarrollo
    Write-Title "PASO 1: Backup de BD de desarrollo"
    
    # Obtener la carpeta de backup de SQL Server (tiene permisos)
    $cmd.CommandText = "SELECT SERVERPROPERTY('InstanceDefaultBackupPath') AS BackupPath"
    $sqlBackupPath = $cmd.ExecuteScalar()
    if ([string]::IsNullOrEmpty($sqlBackupPath)) {
        # Fallback a la ruta estandar de SQL Server 2022
        $sqlBackupPath = "C:\Program Files\Microsoft SQL Server\MSSQL16.SQL2022\MSSQL\Backup"
    }
    
    $backupTemp = Join-Path $sqlBackupPath "SistemIA_Dev_Temp.bak"
    Write-Info "Creando backup temporal en: $backupTemp"
    
    $cmd.CommandText = "BACKUP DATABASE [$BaseDatosOrigen] TO DISK = '$backupTemp' WITH INIT, COMPRESSION"
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Success "Backup de desarrollo creado"
    
    # PASO 2: Eliminar BD temporal si existe
    Write-Title "PASO 2: Preparar BD temporal"
    
    $sqlDropTemp = "IF EXISTS (SELECT 1 FROM sys.databases WHERE name = '$BaseDatosTemp') BEGIN ALTER DATABASE [$BaseDatosTemp] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$BaseDatosTemp]; END"
    $cmd.CommandText = $sqlDropTemp
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Info "BD temporal limpiada"
    
    # PASO 3: Obtener nombres logicos del backup
    Write-Title "PASO 3: Analizar backup"
    
    $cmd.CommandText = "RESTORE FILELISTONLY FROM DISK = '$backupTemp'"
    $reader = $cmd.ExecuteReader()
    $logicalDataName = ""
    $logicalLogName = ""
    while ($reader.Read()) {
        if ($reader["Type"].ToString() -eq "D") { $logicalDataName = $reader["LogicalName"].ToString() }
        elseif ($reader["Type"].ToString() -eq "L") { $logicalLogName = $reader["LogicalName"].ToString() }
    }
    $reader.Close()
    Write-Info "Archivo datos: $logicalDataName"
    Write-Info "Archivo log: $logicalLogName"
    
    # PASO 4: Restaurar en BD temporal
    Write-Title "PASO 4: Restaurar en BD temporal"
    
    $sqlDataPath = "C:\Program Files\Microsoft SQL Server\MSSQL16.SQL2022\MSSQL\DATA"
    if (-not (Test-Path $sqlDataPath)) {
        $cmd.CommandText = "SELECT SERVERPROPERTY('InstanceDefaultDataPath') AS DataPath"
        $sqlDataPath = $cmd.ExecuteScalar()
        if ([string]::IsNullOrEmpty($sqlDataPath)) {
            $sqlDataPath = "C:\SQLData"
        }
    }
    
    $mdfPath = Join-Path $sqlDataPath "$BaseDatosTemp.mdf"
    $ldfPath = Join-Path $sqlDataPath "${BaseDatosTemp}_log.ldf"
    
    $sqlRestore = "RESTORE DATABASE [$BaseDatosTemp] FROM DISK = '$backupTemp' WITH MOVE '$logicalDataName' TO '$mdfPath', MOVE '$logicalLogName' TO '$ldfPath', REPLACE"
    $cmd.CommandText = $sqlRestore
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Success "BD temporal restaurada"
    
    # PASO 5: Limpiar datos transaccionales
    Write-Title "PASO 5: Limpiar datos transaccionales"
    
    # Conectar a la BD temporal
    $connection.Close()
    $connection.ConnectionString = "Server=$ServidorSQL;Database=$BaseDatosTemp;User Id=$Usuario;Password=$Password;TrustServerCertificate=True;"
    $connection.Open()
    $cmd = $connection.CreateCommand()
    $cmd.CommandTimeout = 300
    
    # Script de limpieza en partes
    $limpiezaScripts = @(
        "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'",
        "IF OBJECT_ID('VentaDetalles', 'U') IS NOT NULL DELETE FROM VentaDetalles",
        "IF OBJECT_ID('Ventas', 'U') IS NOT NULL DELETE FROM Ventas",
        "IF OBJECT_ID('VentasPagos', 'U') IS NOT NULL DELETE FROM VentasPagos",
        "IF OBJECT_ID('CompraDetalles', 'U') IS NOT NULL DELETE FROM CompraDetalles",
        "IF OBJECT_ID('Compras', 'U') IS NOT NULL DELETE FROM Compras",
        "IF OBJECT_ID('NotasCreditoVentasDetalles', 'U') IS NOT NULL DELETE FROM NotasCreditoVentasDetalles",
        "IF OBJECT_ID('NotasCreditoVentas', 'U') IS NOT NULL DELETE FROM NotasCreditoVentas",
        "IF OBJECT_ID('NotasCreditoComprasDetalles', 'U') IS NOT NULL DELETE FROM NotasCreditoComprasDetalles",
        "IF OBJECT_ID('NotasCreditoCompras', 'U') IS NOT NULL DELETE FROM NotasCreditoCompras",
        "IF OBJECT_ID('MovimientosInventario', 'U') IS NOT NULL DELETE FROM MovimientosInventario",
        "IF OBJECT_ID('AjustesInventario', 'U') IS NOT NULL DELETE FROM AjustesInventario",
        "IF OBJECT_ID('TransferenciasDepositoDetalle', 'U') IS NOT NULL DELETE FROM TransferenciasDepositoDetalle",
        "IF OBJECT_ID('TransferenciasDeposito', 'U') IS NOT NULL DELETE FROM TransferenciasDeposito",
        "IF OBJECT_ID('ProductoDeposito', 'U') IS NOT NULL DELETE FROM ProductoDeposito",
        "IF OBJECT_ID('CobrosCuotas', 'U') IS NOT NULL DELETE FROM CobrosCuotas",
        "IF OBJECT_ID('CuotasClientes', 'U') IS NOT NULL DELETE FROM CuotasClientes",
        "IF OBJECT_ID('PagosProveedores', 'U') IS NOT NULL DELETE FROM PagosProveedores",
        "IF OBJECT_ID('CuotasProveedores', 'U') IS NOT NULL DELETE FROM CuotasProveedores",
        "IF OBJECT_ID('CierresCaja', 'U') IS NOT NULL DELETE FROM CierresCaja",
        "IF OBJECT_ID('MovimientosCaja', 'U') IS NOT NULL DELETE FROM MovimientosCaja",
        "IF OBJECT_ID('PresupuestosDetalles', 'U') IS NOT NULL DELETE FROM PresupuestosDetalles",
        "IF OBJECT_ID('Presupuestos', 'U') IS NOT NULL DELETE FROM Presupuestos",
        "IF OBJECT_ID('PedidosMesaDetalle', 'U') IS NOT NULL DELETE FROM PedidosMesaDetalle",
        "IF OBJECT_ID('PedidosMesa', 'U') IS NOT NULL DELETE FROM PedidosMesa",
        "IF OBJECT_ID('Asistencias', 'U') IS NOT NULL DELETE FROM Asistencias",
        "IF OBJECT_ID('HistorialCambiosSistema', 'U') IS NOT NULL DELETE FROM HistorialCambiosSistema",
        "IF OBJECT_ID('ConversacionesIAHistorial', 'U') IS NOT NULL DELETE FROM ConversacionesIAHistorial",
        "IF OBJECT_ID('ConversacionesAsistente', 'U') IS NOT NULL DELETE FROM ConversacionesAsistente",
        "IF OBJECT_ID('SolicitudesSoporteAsistente', 'U') IS NOT NULL DELETE FROM SolicitudesSoporteAsistente",
        "IF OBJECT_ID('PreciosProductosClientes', 'U') IS NOT NULL DELETE FROM PreciosProductosClientes",
        "IF OBJECT_ID('ListasPreciosDetalles', 'U') IS NOT NULL DELETE FROM ListasPreciosDetalles",
        "IF OBJECT_ID('ListasPrecios', 'U') IS NOT NULL DELETE FROM ListasPrecios",
        "IF OBJECT_ID('Clientes', 'U') IS NOT NULL DELETE FROM Clientes WHERE IdCliente > 1",
        "IF OBJECT_ID('ProveedoresSifen', 'U') IS NOT NULL DELETE FROM ProveedoresSifen WHERE IdProveedor > 1",
        "IF OBJECT_ID('Empleados', 'U') IS NOT NULL DELETE FROM Empleados",
        "IF OBJECT_ID('Cajas', 'U') IS NOT NULL UPDATE Cajas SET Serie = 1, SerieR = 1, SerieNC = 1, SerieRecibo = 1, TurnoActual = 1, CajaActual = NULL, FechaActualCaja = NULL",
        "IF OBJECT_ID('Clientes', 'U') IS NOT NULL UPDATE Clientes SET Saldo = 0",
        "IF OBJECT_ID('ProveedoresSifen', 'U') IS NOT NULL UPDATE ProveedoresSifen SET SaldoPendiente = 0",
        "IF OBJECT_ID('Usuarios', 'U') IS NOT NULL DELETE FROM Usuarios WHERE IdUsuario > 1",
        "IF OBJECT_ID('Usuarios', 'U') IS NOT NULL UPDATE Usuarios SET PasswordHash = '`$2a`$11`$rKLWc5a5BmVVW1lHKIYqpOG3LDqBMPOCCl.HJ.KDHh9x5RLfxQYdq' WHERE IdUsuario = 1",
        "EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'"
    )
    
    $i = 0
    foreach ($sql in $limpiezaScripts) {
        $i++
        try {
            $cmd.CommandText = $sql
            $cmd.ExecuteNonQuery() | Out-Null
        } catch {
            # Ignorar errores de tablas que no existen
        }
        if ($i % 10 -eq 0) {
            Write-Host "." -NoNewline
        }
    }
    Write-Host ""
    Write-Success "Datos transaccionales eliminados"
    
    # PASO 6: Shrink de la BD
    Write-Title "PASO 6: Compactar BD"
    
    try {
        $cmd.CommandText = "DBCC SHRINKDATABASE ([$BaseDatosTemp], 10)"
        $cmd.ExecuteNonQuery() | Out-Null
    } catch { }
    Write-Success "BD compactada"
    
    # PASO 7: Crear backup final limpio
    Write-Title "PASO 7: Crear backup limpio final"
    
    $connection.Close()
    $connection.ConnectionString = "Server=$ServidorSQL;Database=master;User Id=$Usuario;Password=$Password;TrustServerCertificate=True;"
    $connection.Open()
    $cmd = $connection.CreateCommand()
    $cmd.CommandTimeout = 300
    
    # Ruta absoluta del backup final
    $rutaAbsoluta = $null
    try {
        $rutaAbsoluta = (Resolve-Path -Path (Split-Path $RutaSalida -Parent) -ErrorAction Stop).Path
    } catch {
        $rutaAbsoluta = (Get-Location).Path
    }
    $nombreArchivo = Split-Path $RutaSalida -Leaf
    $backupFinal = Join-Path $rutaAbsoluta $nombreArchivo
    
    Write-Info "Generando backup en: $backupFinal"
    
    $cmd.CommandText = "BACKUP DATABASE [$BaseDatosTemp] TO DISK = '$backupFinal' WITH INIT, COMPRESSION"
    $cmd.ExecuteNonQuery() | Out-Null
    
    Write-Success "Backup limpio creado: $backupFinal"
    
    # PASO 8: Eliminar BD temporal
    Write-Title "PASO 8: Limpieza final"
    
    $cmd.CommandText = "IF EXISTS (SELECT 1 FROM sys.databases WHERE name = '$BaseDatosTemp') BEGIN ALTER DATABASE [$BaseDatosTemp] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$BaseDatosTemp]; END"
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Info "BD temporal eliminada"
    
    # Eliminar backup temporal
    if (Test-Path $backupTemp) {
        Remove-Item $backupTemp -Force -ErrorAction SilentlyContinue
        Write-Info "Backup temporal eliminado"
    }
    
    $connection.Close()
    
    # RESULTADO FINAL
    Write-Host ""
    Write-Host "=========================================================" -ForegroundColor Green
    Write-Host "          BACKUP LIMPIO GENERADO EXITOSAMENTE            " -ForegroundColor Green
    Write-Host "=========================================================" -ForegroundColor Green
    Write-Host ""
    Write-Success "Archivo: $backupFinal"
    $size = (Get-Item $backupFinal).Length / 1MB
    Write-Success "Tamano: $([math]::Round($size, 2)) MB"
    Write-Host ""
    Write-Info "Este archivo contiene:"
    Write-Host "  + Estructura completa de tablas" -ForegroundColor White
    Write-Host "  + Catalogos (paises, ciudades, tipos IVA, etc.)" -ForegroundColor White
    Write-Host "  + Usuario admin (password: admin)" -ForegroundColor White
    Write-Host "  + Configuraciones base" -ForegroundColor White
    Write-Host "  - SIN datos transaccionales" -ForegroundColor Yellow
    Write-Host ""
    Write-Info "Copie este archivo a la carpeta Installer del paquete."
    
}
catch {
    Write-ErrorMsg "Error: $_"
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    # Limpieza en caso de error
    if ($connection -and $connection.State -eq 'Open') { $connection.Close() }
}
