<#
.SYNOPSIS
    Instalador de SistemIA - Sistema de Gestión Empresarial
.DESCRIPTION
    Este script instala SistemIA como servicio de Windows con configuración personalizada
.NOTES
    Ejecutar como Administrador
#>

param(
    [string]$ConfigFile,
    [string]$ServiceName,
    [switch]$Uninstall,
    [switch]$LimpiarDatos,
    [switch]$SoloConfigurar
)

# Determinar directorio del script de múltiples formas para máxima compatibilidad
$ScriptDir = $null

# Método 1: Desde MyInvocation (funciona cuando se ejecuta como archivo)
if ($MyInvocation.MyCommand.Path) {
    $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
}

# Método 2: Desde PSScriptRoot (funciona en PS 3.0+)
if (-not $ScriptDir -and $PSScriptRoot) {
    $ScriptDir = $PSScriptRoot
}

# Método 3: Desde la ubicación actual si todo lo demás falla
if (-not $ScriptDir) {
    $ScriptDir = (Get-Location).Path
}

# Validar que ScriptDir existe
if (-not (Test-Path $ScriptDir)) {
    $ScriptDir = (Get-Location).Path
}

Write-Host "[DEBUG] Directorio del instalador: $ScriptDir" -ForegroundColor DarkGray

# Si no se especificó ConfigFile, usar el del directorio del script
if (-not $ConfigFile) {
    $ConfigFile = Join-Path $ScriptDir "config.json"
}

# Colores para mensajes
function Write-Title { param($msg) Write-Host "`n====== $msg ======" -ForegroundColor Cyan }
function Write-Success { param($msg) Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Error { param($msg) Write-Host "[ERROR] $msg" -ForegroundColor Red }
function Write-Warning { param($msg) Write-Host "[AVISO] $msg" -ForegroundColor Yellow }
function Write-Info { param($msg) Write-Host "[INFO] $msg" -ForegroundColor White }

# Verificar permisos de administrador
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Banner
function Show-Banner {
    Clear-Host
    Write-Host @"
╔═══════════════════════════════════════════════════════════════╗
║                                                               ║
║   ███████╗██╗███████╗████████╗███████╗███╗   ███╗██╗ █████╗  ║
║   ██╔════╝██║██╔════╝╚══██╔══╝██╔════╝████╗ ████║██║██╔══██╗ ║
║   ███████╗██║███████╗   ██║   █████╗  ██╔████╔██║██║███████║ ║
║   ╚════██║██║╚════██║   ██║   ██╔══╝  ██║╚██╔╝██║██║██╔══██║ ║
║   ███████║██║███████║   ██║   ███████╗██║ ╚═╝ ██║██║██║  ██║ ║
║   ╚══════╝╚═╝╚══════╝   ╚═╝   ╚══════╝╚═╝     ╚═╝╚═╝╚═╝  ╚═╝ ║
║                                                               ║
║           Sistema de Gestión Empresarial v1.0                 ║
║                     INSTALADOR                                ║
╚═══════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Cyan
}

# Menú principal
function Show-Menu {
    Write-Host "`n┌───────────────────────────────────────────┐" -ForegroundColor White
    Write-Host "│            MENÚ DE INSTALACIÓN            │" -ForegroundColor White
    Write-Host "├───────────────────────────────────────────┤" -ForegroundColor White
    Write-Host "│  1. Instalación completa (Servidor)       │" -ForegroundColor White
    Write-Host "│  2. Instalación Cliente (acceso remoto)   │" -ForegroundColor Cyan
    Write-Host "│  3. Solo configurar servidor/BD           │" -ForegroundColor White
    Write-Host "│  4. Instalar servicio Windows             │" -ForegroundColor White
    Write-Host "│  5. Desinstalar servicio                  │" -ForegroundColor White
    Write-Host "│  6. Crear/Restaurar base de datos         │" -ForegroundColor White
    Write-Host "│  7. Limpiar datos del sistema             │" -ForegroundColor White
    Write-Host "│  8. Ver configuración actual              │" -ForegroundColor White
    Write-Host "│  9. Probar conexión a BD                  │" -ForegroundColor White
    Write-Host "│ 10. Diagnosticar servicio                 │" -ForegroundColor White
    Write-Host "├───────────────────────────────────────────┤" -ForegroundColor White
    Write-Host "│ 11. Instalar certificado HTTPS (mkcert)   │" -ForegroundColor Green
    Write-Host "│ 12. Configurar Firewall (red)             │" -ForegroundColor White
    Write-Host "│ 13. Ver acceso en red                     │" -ForegroundColor White
    Write-Host "│ 14. Crear acceso directo en escritorio    │" -ForegroundColor Cyan
    Write-Host "├───────────────────────────────────────────┤" -ForegroundColor White
    Write-Host "│  0. Salir                                 │" -ForegroundColor White
    Write-Host "└───────────────────────────────────────────┘" -ForegroundColor White
    Write-Host ""
}

# Cargar configuración
function Get-InstallerConfig {
    param([string]$Path)
    
    if (-not (Test-Path $Path)) {
        Write-Error "Archivo de configuración no encontrado: $Path"
        return $null
    }
    
    try {
        $config = Get-Content $Path -Raw | ConvertFrom-Json
        
        # === ASEGURAR PROPIEDADES POR DEFECTO ===
        
        # Instalacion
        if (-not $config.PSObject.Properties['Instalacion']) {
            $config | Add-Member -NotePropertyName 'Instalacion' -NotePropertyValue (New-Object PSObject) -Force
        }
        if (-not $config.Instalacion.PSObject.Properties['ModoInstalacion']) {
            $config.Instalacion | Add-Member -NotePropertyName 'ModoInstalacion' -NotePropertyValue 'Servidor' -Force
        }
        if (-not $config.Instalacion.PSObject.Properties['RutaInstalacion']) {
            $config.Instalacion | Add-Member -NotePropertyName 'RutaInstalacion' -NotePropertyValue 'C:\SistemIA' -Force
        }
        if (-not $config.Instalacion.PSObject.Properties['PuertoHttp']) {
            $config.Instalacion | Add-Member -NotePropertyName 'PuertoHttp' -NotePropertyValue 5095 -Force
        }
        if (-not $config.Instalacion.PSObject.Properties['PuertoHttps']) {
            $config.Instalacion | Add-Member -NotePropertyName 'PuertoHttps' -NotePropertyValue 7060 -Force
        }
        if (-not $config.Instalacion.PSObject.Properties['NombreServicio']) {
            $config.Instalacion | Add-Member -NotePropertyName 'NombreServicio' -NotePropertyValue 'SistemIA' -Force
        }
        # Override con parametro -ServiceName si fue proporcionado
        if ($ServiceName -and $ServiceName -ne '') {
            $config.Instalacion.NombreServicio = $ServiceName
            Write-Host "   Nombre de servicio personalizado: $ServiceName" -ForegroundColor Cyan
        }
        if (-not $config.Instalacion.PSObject.Properties['DescripcionServicio']) {
            $config.Instalacion | Add-Member -NotePropertyName 'DescripcionServicio' -NotePropertyValue 'SistemIA - Sistema de Gestión Empresarial' -Force
        }
        if (-not $config.Instalacion.PSObject.Properties['InicioAutomatico']) {
            $config.Instalacion | Add-Member -NotePropertyName 'InicioAutomatico' -NotePropertyValue $true -Force
        }
        if (-not $config.Instalacion.PSObject.Properties['UsarMkcert']) {
            $config.Instalacion | Add-Member -NotePropertyName 'UsarMkcert' -NotePropertyValue $true -Force
        }
        if (-not $config.Instalacion.PSObject.Properties['CrearAccesoDirecto']) {
            $config.Instalacion | Add-Member -NotePropertyName 'CrearAccesoDirecto' -NotePropertyValue $true -Force
        }
        
        # BaseDatos
        if (-not $config.PSObject.Properties['BaseDatos']) {
            $config | Add-Member -NotePropertyName 'BaseDatos' -NotePropertyValue (New-Object PSObject) -Force
        }
        if (-not $config.BaseDatos.PSObject.Properties['Servidor']) {
            $config.BaseDatos | Add-Member -NotePropertyName 'Servidor' -NotePropertyValue 'localhost\SQLEXPRESS' -Force
        }
        if (-not $config.BaseDatos.PSObject.Properties['BaseDatos']) {
            $dbName = if ($config.BaseDatos.NombreBD) { $config.BaseDatos.NombreBD } else { 'SistemIA' }
            $config.BaseDatos | Add-Member -NotePropertyName 'BaseDatos' -NotePropertyValue $dbName -Force
        }
        if (-not $config.BaseDatos.PSObject.Properties['Usuario']) {
            $config.BaseDatos | Add-Member -NotePropertyName 'Usuario' -NotePropertyValue 'sa' -Force
        }
        if (-not $config.BaseDatos.PSObject.Properties['Password']) {
            $pass = if ($config.BaseDatos.Contrasena) { $config.BaseDatos.Contrasena } else { '%L4V1CT0R14' }
            $config.BaseDatos | Add-Member -NotePropertyName 'Password' -NotePropertyValue $pass -Force
        }
        if (-not $config.BaseDatos.PSObject.Properties['AutenticacionWindows']) {
            $config.BaseDatos | Add-Member -NotePropertyName 'AutenticacionWindows' -NotePropertyValue $false -Force
        }
        if (-not $config.BaseDatos.PSObject.Properties['TrustServerCertificate']) {
            $config.BaseDatos | Add-Member -NotePropertyName 'TrustServerCertificate' -NotePropertyValue 'True' -Force
        }
        
        # Sociedad
        if (-not $config.PSObject.Properties['Sociedad']) {
            $config | Add-Member -NotePropertyName 'Sociedad' -NotePropertyValue (New-Object PSObject) -Force
        }
        if (-not $config.Sociedad.PSObject.Properties['Nombre']) {
            $config.Sociedad | Add-Member -NotePropertyName 'Nombre' -NotePropertyValue 'Mi Empresa' -Force
        }
        if (-not $config.Sociedad.PSObject.Properties['RUC']) {
            $config.Sociedad | Add-Member -NotePropertyName 'RUC' -NotePropertyValue '80000000-0' -Force
        }
        if (-not $config.Sociedad.PSObject.Properties['Direccion']) {
            $config.Sociedad | Add-Member -NotePropertyName 'Direccion' -NotePropertyValue '' -Force
        }
        
        # Cliente (para instalación remota)
        if (-not $config.PSObject.Properties['Cliente']) {
            $config | Add-Member -NotePropertyName 'Cliente' -NotePropertyValue (New-Object PSObject) -Force
        }
        if (-not $config.Cliente.PSObject.Properties['ServidorRemoto']) {
            $config.Cliente | Add-Member -NotePropertyName 'ServidorRemoto' -NotePropertyValue '192.168.1.100' -Force
        }
        if (-not $config.Cliente.PSObject.Properties['PuertoRemoto']) {
            $config.Cliente | Add-Member -NotePropertyName 'PuertoRemoto' -NotePropertyValue 5095 -Force
        }
        if (-not $config.Cliente.PSObject.Properties['UsarHttps']) {
            $config.Cliente | Add-Member -NotePropertyName 'UsarHttps' -NotePropertyValue $false -Force
        }
        
        return $config
    }
    catch {
        Write-Error "Error al leer configuración: $_"
        return $null
    }
}

# Guardar configuración
function Save-InstallerConfig {
    param($Config, [string]$Path)
    
    try {
        $Config | ConvertTo-Json -Depth 5 | Set-Content $Path -Encoding UTF8
        Write-Success "Configuración guardada"
    }
    catch {
        Write-Error "Error al guardar configuración: $_"
    }
}

# Configuración interactiva
function Set-InteractiveConfig {
    param($Config)
    
    Write-Title "TIPO DE INSTALACIÓN"
    Write-Host ""
    Write-Host "  1. SERVIDOR - Instalación completa con base de datos local" -ForegroundColor White
    Write-Host "  2. CLIENTE  - Conectar a un servidor SistemIA existente" -ForegroundColor White
    Write-Host ""
    
    $modoInstalacion = Read-Host "Seleccione el modo de instalación [1]"
    if ([string]::IsNullOrWhiteSpace($modoInstalacion) -or $modoInstalacion -eq "1") {
        $Config.Instalacion.ModoInstalacion = "Servidor"
    }
    else {
        $Config.Instalacion.ModoInstalacion = "Cliente"
        
        Write-Title "CONFIGURACIÓN DEL SERVIDOR REMOTO"
        
        $defaultRemoto = if ($Config.Cliente.ServidorRemoto) { $Config.Cliente.ServidorRemoto } else { "192.168.1.100" }
        $servidorRemoto = Read-Host "IP o nombre del servidor SistemIA [$defaultRemoto]"
        if ([string]::IsNullOrWhiteSpace($servidorRemoto)) { $servidorRemoto = $defaultRemoto }
        $Config.Cliente.ServidorRemoto = $servidorRemoto
        
        $defaultPuerto = if ($Config.Cliente.PuertoRemoto) { $Config.Cliente.PuertoRemoto } else { 5095 }
        $puertoRemoto = Read-Host "Puerto del servidor [$defaultPuerto]"
        if ([string]::IsNullOrWhiteSpace($puertoRemoto)) { $puertoRemoto = $defaultPuerto }
        $Config.Cliente.PuertoRemoto = [int]$puertoRemoto
        
        $usarHttps = Read-Host "¿Usar HTTPS? (S/N) [N]"
        $Config.Cliente.UsarHttps = ($usarHttps -eq 'S' -or $usarHttps -eq 's')
        
        # Preguntar por acceso directo
        $crearAcceso = Read-Host "¿Crear acceso directo en el escritorio? (S/N) [S]"
        $Config.Instalacion.CrearAccesoDirecto = ($crearAcceso -ne 'N' -and $crearAcceso -ne 'n')
        
        return $Config
    }
    
    Write-Title "CONFIGURACIÓN DEL SERVIDOR"
    
    # Usar valores predefinidos del config.json
    $defaultPath = if ($Config.Instalacion.RutaInstalacion) { $Config.Instalacion.RutaInstalacion } else { "C:\SistemIA" }
    $rutaInstalacion = Read-Host "Ruta de instalación [$defaultPath]"
    if ([string]::IsNullOrWhiteSpace($rutaInstalacion)) { $rutaInstalacion = $defaultPath }
    $Config.Instalacion.RutaInstalacion = $rutaInstalacion
    
    # Puertos - usar valores predefinidos sin preguntar
    if (-not $Config.Instalacion.PuertoHttp) { $Config.Instalacion.PuertoHttp = 5095 }
    if (-not $Config.Instalacion.PuertoHttps) { $Config.Instalacion.PuertoHttps = 7060 }
    Write-Info "Puertos configurados: HTTP=$($Config.Instalacion.PuertoHttp), HTTPS=$($Config.Instalacion.PuertoHttps)"
    
    Write-Title "CONFIGURACIÓN DE BASE DE DATOS"
    
    # Servidor SQL - preguntar solo si se quiere cambiar
    $defaultServer = if ($Config.BaseDatos.Servidor) { $Config.BaseDatos.Servidor } else { "localhost\SQLEXPRESS" }
    $servidor = Read-Host "Servidor SQL [$defaultServer]"
    if ([string]::IsNullOrWhiteSpace($servidor)) { $servidor = $defaultServer }
    $Config.BaseDatos.Servidor = $servidor
    
    # Nombre de BD - usar predefinido
    $defaultDb = if ($Config.BaseDatos.NombreBD) { $Config.BaseDatos.NombreBD } elseif ($Config.BaseDatos.BaseDatos) { $Config.BaseDatos.BaseDatos } else { "SistemIA" }
    Write-Info "Base de datos: $defaultDb"
    $Config.BaseDatos.BaseDatos = $defaultDb
    $Config.BaseDatos.NombreBD = $defaultDb
    
    # Autenticación - usar SQL Server con valores predefinidos
    if (-not $Config.BaseDatos.PSObject.Properties['AutenticacionWindows']) {
        $Config.BaseDatos | Add-Member -NotePropertyName 'AutenticacionWindows' -NotePropertyValue $false -Force
    }
    $Config.BaseDatos.AutenticacionWindows = $false
    
    # Usuario y contraseña - usar valores predefinidos del config.json
    $defaultUser = if ($Config.BaseDatos.Usuario) { $Config.BaseDatos.Usuario } else { "sa" }
    $defaultPass = if ($Config.BaseDatos.Password) { $Config.BaseDatos.Password } elseif ($Config.BaseDatos.Contrasena) { $Config.BaseDatos.Contrasena } else { "" }
    
    $Config.BaseDatos.Usuario = $defaultUser
    $Config.BaseDatos.Password = $defaultPass
    Write-Info "Usuario SQL: $defaultUser (contraseña predefinida)"
    
    # OMITIR configuración de empresa - se hace después en el sistema
    Write-Info "Los datos de la empresa se configurarán dentro del sistema"
    
    # Asegurar que existe la sección Sociedad
    if (-not $Config.PSObject.Properties['Sociedad']) {
        $Config | Add-Member -NotePropertyName 'Sociedad' -NotePropertyValue @{
            Nombre = "Mi Empresa"
            RUC = "80000000-0"
            Direccion = ""
        } -Force
    }
    
    Write-Title "OPCIONES ADICIONALES"
    
    # Usar certificado predefinido sin preguntar
    if (-not $Config.Instalacion.PSObject.Properties['UsarMkcert']) {
        $Config.Instalacion | Add-Member -NotePropertyName 'UsarMkcert' -NotePropertyValue $true -Force
    }
    Write-Info "Certificado HTTPS: $( if ($Config.Instalacion.UsarMkcert) { 'mkcert (confianza local)' } else { 'Autofirmado' } )"
    
    # Preguntar solo por acceso directo
    $crearAcceso = Read-Host "¿Crear acceso directo en el escritorio? (S/N) [S]"
    if (-not $Config.Instalacion.PSObject.Properties['CrearAccesoDirecto']) {
        $Config.Instalacion | Add-Member -NotePropertyName 'CrearAccesoDirecto' -NotePropertyValue $true -Force
    }
    $Config.Instalacion.CrearAccesoDirecto = ($crearAcceso -ne 'N' -and $crearAcceso -ne 'n')
    
    return $Config
}

# Construir cadena de conexión
function Build-ConnectionString {
    param($DbConfig)
    
    $dbName = if ($DbConfig.BaseDatos) { $DbConfig.BaseDatos } elseif ($DbConfig.NombreBD) { $DbConfig.NombreBD } else { "SistemIA" }
    $password = if ($DbConfig.Password) { $DbConfig.Password } elseif ($DbConfig.Contrasena) { $DbConfig.Contrasena } else { "" }
    
    if ($DbConfig.AutenticacionWindows) {
        return "Server=$($DbConfig.Servidor);Database=$dbName;Integrated Security=True;TrustServerCertificate=True;"
    }
    else {
        return "Server=$($DbConfig.Servidor);Database=$dbName;User Id=$($DbConfig.Usuario);Password=$password;TrustServerCertificate=True;"
    }
}

# Construir cadena de conexión al servidor (master)
function Build-MasterConnectionString {
    param($DbConfig)
    
    $trustCert = if ($DbConfig.TrustServerCertificate) { $DbConfig.TrustServerCertificate } else { "True" }
    $password = if ($DbConfig.Password) { $DbConfig.Password } elseif ($DbConfig.Contrasena) { $DbConfig.Contrasena } else { "" }
    
    if ($DbConfig.AutenticacionWindows) {
        return "Server=$($DbConfig.Servidor);Database=master;Integrated Security=True;TrustServerCertificate=$trustCert;"
    }
    else {
        return "Server=$($DbConfig.Servidor);Database=master;User Id=$($DbConfig.Usuario);Password=$password;TrustServerCertificate=$trustCert;"
    }
}

# ============ SISTEMA DE ROLLBACK ============

# Variable global para tracking de rollback
$script:RollbackState = @{
    BackupCreado = $false
    BackupPath = ""
    DbExistia = $false
    DbName = ""
    ArchivosCopiados = @()
    ServicioCreado = $false
    FirewallCreado = $false
}

# Crear backup previo para rollback
function New-PreInstallBackup {
    param($DbConfig)
    
    $dbName = if ($DbConfig.BaseDatos) { $DbConfig.BaseDatos } elseif ($DbConfig.NombreBD) { $DbConfig.NombreBD } else { "SistemIA" }
    $script:RollbackState.DbName = $dbName
    
    Write-Info "Verificando si existe base de datos previa..."
    
    try {
        $masterConn = Build-MasterConnectionString -DbConfig $DbConfig
        $connection = New-Object System.Data.SqlClient.SqlConnection
        $connection.ConnectionString = $masterConn
        $connection.Open()
        
        $cmd = $connection.CreateCommand()
        $cmd.CommandTimeout = 300
        
        # Verificar si la BD existe
        $cmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '$dbName'"
        $exists = $cmd.ExecuteScalar()
        
        if ($exists -gt 0) {
            $script:RollbackState.DbExistia = $true
            
            Write-Info "Base de datos existente encontrada. Creando backup de seguridad..."
            
            # Obtener ruta de backup
            $cmd.CommandText = "SELECT SERVERPROPERTY('InstanceDefaultBackupPath') AS BackupPath"
            $backupPath = $cmd.ExecuteScalar()
            if (-not $backupPath) {
                $backupPath = "C:\Program Files\Microsoft SQL Server\MSSQL16.SQL2022\MSSQL\Backup"
            }
            
            $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
            $backupFile = Join-Path $backupPath "SistemIA_PreInstall_$timestamp.bak"
            
            # Crear backup
            $cmd.CommandText = "BACKUP DATABASE [$dbName] TO DISK = '$backupFile' WITH INIT, COMPRESSION"
            $cmd.ExecuteNonQuery() | Out-Null
            
            $script:RollbackState.BackupCreado = $true
            $script:RollbackState.BackupPath = $backupFile
            
            Write-Success "Backup de seguridad creado: $backupFile"
        }
        else {
            Write-Info "No existe base de datos previa (instalación nueva)"
            $script:RollbackState.DbExistia = $false
        }
        
        $connection.Close()
        return $true
    }
    catch {
        Write-Warning "No se pudo crear backup previo: $_"
        return $false
    }
}

# Restaurar desde backup de rollback
function Invoke-Rollback {
    param($DbConfig, $Config)
    
    Write-Title "EJECUTANDO ROLLBACK - Revirtiendo cambios"
    Write-Warning "Ocurrió un error durante la instalación. Revirtiendo..."
    
    $erroresRollback = @()
    
    # 1. Detener y eliminar servicio si fue creado
    if ($script:RollbackState.ServicioCreado) {
        try {
            $serviceName = $Config.Instalacion.NombreServicio
            Write-Info "Eliminando servicio: $serviceName"
            Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
            sc.exe delete $serviceName | Out-Null
            Write-Success "Servicio eliminado"
        }
        catch {
            $erroresRollback += "Error eliminando servicio: $_"
        }
    }
    
    # 2. Restaurar base de datos si existía previamente
    if ($script:RollbackState.BackupCreado -and $script:RollbackState.BackupPath) {
        try {
            Write-Info "Restaurando base de datos desde backup..."
            
            $masterConn = Build-MasterConnectionString -DbConfig $DbConfig
            $connection = New-Object System.Data.SqlClient.SqlConnection
            $connection.ConnectionString = $masterConn
            $connection.Open()
            
            $cmd = $connection.CreateCommand()
            $cmd.CommandTimeout = 600
            
            $dbName = $script:RollbackState.DbName
            $backupFile = $script:RollbackState.BackupPath
            
            # Cerrar conexiones y restaurar
            try {
                $cmd.CommandText = "ALTER DATABASE [$dbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
                $cmd.ExecuteNonQuery() | Out-Null
            } catch { }
            
            # Obtener rutas
            $cmd.CommandText = "SELECT SERVERPROPERTY('InstanceDefaultDataPath') AS DataPath"
            $dataPath = $cmd.ExecuteScalar()
            if (-not $dataPath) { $dataPath = "C:\Program Files\Microsoft SQL Server\MSSQL16.SQL2022\MSSQL\DATA\" }
            
            $mdfPath = Join-Path $dataPath "$dbName.mdf"
            $ldfPath = Join-Path $dataPath "${dbName}_log.ldf"
            
            # Obtener nombres lógicos
            $cmd.CommandText = "RESTORE FILELISTONLY FROM DISK = '$backupFile'"
            $reader = $cmd.ExecuteReader()
            $logicalDataName = ""
            $logicalLogName = ""
            while ($reader.Read()) {
                if ($reader["Type"].ToString() -eq "D") { $logicalDataName = $reader["LogicalName"].ToString() }
                elseif ($reader["Type"].ToString() -eq "L") { $logicalLogName = $reader["LogicalName"].ToString() }
            }
            $reader.Close()
            
            # Restaurar
            $restoreQuery = "RESTORE DATABASE [$dbName] FROM DISK = '$backupFile' WITH MOVE '$logicalDataName' TO '$mdfPath', MOVE '$logicalLogName' TO '$ldfPath', REPLACE"
            $cmd.CommandText = $restoreQuery
            $cmd.ExecuteNonQuery() | Out-Null
            
            $connection.Close()
            Write-Success "Base de datos restaurada al estado anterior"
        }
        catch {
            $erroresRollback += "Error restaurando BD: $_"
        }
    }
    elseif (-not $script:RollbackState.DbExistia) {
        # Si la BD no existía antes, eliminarla
        try {
            Write-Info "Eliminando base de datos creada..."
            $masterConn = Build-MasterConnectionString -DbConfig $DbConfig
            $connection = New-Object System.Data.SqlClient.SqlConnection
            $connection.ConnectionString = $masterConn
            $connection.Open()
            
            $cmd = $connection.CreateCommand()
            $dbName = $script:RollbackState.DbName
            
            try {
                $cmd.CommandText = "ALTER DATABASE [$dbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
                $cmd.ExecuteNonQuery() | Out-Null
            } catch { }
            
            $cmd.CommandText = "DROP DATABASE [$dbName]"
            $cmd.ExecuteNonQuery() | Out-Null
            
            $connection.Close()
            Write-Success "Base de datos eliminada"
        }
        catch {
            $erroresRollback += "Error eliminando BD: $_"
        }
    }
    
    # 3. Eliminar archivos copiados
    if ($script:RollbackState.ArchivosCopiados.Count -gt 0) {
        try {
            Write-Info "Eliminando archivos copiados..."
            foreach ($archivo in $script:RollbackState.ArchivosCopiados) {
                if (Test-Path $archivo) {
                    Remove-Item $archivo -Force -Recurse -ErrorAction SilentlyContinue
                }
            }
            Write-Success "Archivos eliminados"
        }
        catch {
            $erroresRollback += "Error eliminando archivos: $_"
        }
    }
    
    # 4. Eliminar reglas de firewall
    if ($script:RollbackState.FirewallCreado) {
        try {
            Write-Info "Eliminando reglas de firewall..."
            netsh advfirewall firewall delete rule name="SistemIA HTTP" | Out-Null
            netsh advfirewall firewall delete rule name="SistemIA HTTPS" | Out-Null
            Write-Success "Reglas de firewall eliminadas"
        }
        catch {
            $erroresRollback += "Error eliminando reglas firewall: $_"
        }
    }
    
    if ($erroresRollback.Count -gt 0) {
        Write-Error "Se encontraron errores durante el rollback:"
        foreach ($err in $erroresRollback) {
            Write-Host "  - $err" -ForegroundColor Red
        }
    }
    else {
        Write-Success "Rollback completado exitosamente"
    }
    
    Write-Host ""
    Write-Info "El sistema ha sido revertido al estado anterior a la instalación"
}

# Limpiar backup de rollback (cuando instalación es exitosa)
function Remove-RollbackBackup {
    if ($script:RollbackState.BackupCreado -and $script:RollbackState.BackupPath) {
        try {
            if (Test-Path $script:RollbackState.BackupPath) {
                Remove-Item $script:RollbackState.BackupPath -Force -ErrorAction SilentlyContinue
                Write-Info "Backup de rollback eliminado"
            }
        }
        catch { }
    }
    
    # Resetear estado
    $script:RollbackState = @{
        BackupCreado = $false
        BackupPath = ""
        DbExistia = $false
        DbName = ""
        ArchivosCopiados = @()
        ServicioCreado = $false
        FirewallCreado = $false
    }
}

# ============ FIN SISTEMA DE ROLLBACK ============

# Probar conexión al servidor SQL (sin BD específica)
function Test-ServerConnection {
    param($DbConfig)
    
    Write-Info "Probando conexión al servidor SQL..."
    
    $connectionString = Build-MasterConnectionString -DbConfig $DbConfig
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection
        $connection.ConnectionString = $connectionString
        $connection.Open()
        
        Write-Success "Conexión exitosa al servidor: $($DbConfig.Servidor)"
        
        $connection.Close()
        return $true
    }
    catch {
        Write-Error "No se pudo conectar al servidor: $_"
        return $false
    }
}

# Probar conexión a BD
function Test-DatabaseConnection {
    param($DbConfig)
    
    Write-Info "Probando conexión a la base de datos..."
    
    $connectionString = Build-ConnectionString -DbConfig $DbConfig
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection
        $connection.ConnectionString = $connectionString
        $connection.Open()
        
        Write-Success "Conexión exitosa a: $($DbConfig.Servidor) / $($DbConfig.BaseDatos)"
        
        $connection.Close()
        return $true
    }
    catch {
        Write-Error "No se pudo conectar: $_"
        return $false
    }
}

# Crear base de datos si no existe
function Initialize-Database {
    param($DbConfig)
    
    Write-Info "Verificando base de datos..."
    
    $dbName = if ($DbConfig.BaseDatos) { $DbConfig.BaseDatos } elseif ($DbConfig.NombreBD) { $DbConfig.NombreBD } else { "SistemIA" }
    $trustCert = if ($DbConfig.TrustServerCertificate) { $DbConfig.TrustServerCertificate } else { "True" }
    $password = if ($DbConfig.Password) { $DbConfig.Password } elseif ($DbConfig.Contrasena) { $DbConfig.Contrasena } else { "" }
    
    # Primero conectar al master para verificar/crear la BD
    $masterConnString = if ($DbConfig.AutenticacionWindows) {
        "Server=$($DbConfig.Servidor);Database=master;Integrated Security=True;TrustServerCertificate=$trustCert;"
    }
    else {
        "Server=$($DbConfig.Servidor);Database=master;User Id=$($DbConfig.Usuario);Password=$password;TrustServerCertificate=$trustCert;"
    }
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection
        $connection.ConnectionString = $masterConnString
        $connection.Open()
        
        $cmd = $connection.CreateCommand()
        $cmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '$dbName'"
        $exists = $cmd.ExecuteScalar()
        
        if ($exists -eq 0) {
            Write-Info "Creando base de datos $dbName..."
            $cmd.CommandText = "CREATE DATABASE [$dbName]"
            $cmd.ExecuteNonQuery()
            Write-Success "Base de datos creada"
        }
        else {
            Write-Success "Base de datos ya existe"
        }
        
        $connection.Close()
        return $true
    }
    catch {
        Write-Error "Error al inicializar BD: $_"
        return $false
    }
}

# Ejecutar script SQL
function Invoke-SqlScript {
    param(
        $DbConfig,
        [string]$ScriptPath,
        [string]$Description
    )
    
    if (-not (Test-Path $ScriptPath)) {
        Write-Warning "Script no encontrado: $ScriptPath"
        return $false
    }
    
    Write-Info "Ejecutando $Description..."
    
    $connectionString = Build-ConnectionString -DbConfig $DbConfig
    
    try {
        $sqlScript = Get-Content $ScriptPath -Raw -Encoding UTF8
        
        # Dividir por GO para ejecutar por lotes
        $batches = $sqlScript -split '\r?\nGO\r?\n'
        
        $connection = New-Object System.Data.SqlClient.SqlConnection
        $connection.ConnectionString = $connectionString
        $connection.Open()
        
        $batchCount = 0
        foreach ($batch in $batches) {
            $batch = $batch.Trim()
            if ($batch.Length -gt 0) {
                try {
                    $cmd = $connection.CreateCommand()
                    $cmd.CommandText = $batch
                    $cmd.CommandTimeout = 300  # 5 minutos timeout
                    $cmd.ExecuteNonQuery() | Out-Null
                    $batchCount++
                }
                catch {
                    # Algunos errores son esperados (tablas que ya existen, etc.)
                    # Continuar con el siguiente batch
                }
            }
        }
        
        $connection.Close()
        Write-Success "$Description completado ($batchCount lotes ejecutados)"
        return $true
    }
    catch {
        Write-Error "Error ejecutando script: $_"
        return $false
    }
}

# Restaurar base de datos desde backup
function Restore-DatabaseFromBackup {
    param(
        $DbConfig,
        [string]$InstallerPath
    )
    
    Write-Title "RESTAURANDO BASE DE DATOS DESDE BACKUP"
    
    # Validar que InstallerPath no esté vacío
    if (-not $InstallerPath) {
        Write-Error "Ruta del instalador no especificada"
        Write-Info "Usando directorio actual..."
        $InstallerPath = (Get-Location).Path
    }
    
    $backupFile = Join-Path $InstallerPath "SistemIA_Base.bak"
    
    Write-Info "Buscando backup en: $backupFile"
    
    if (-not (Test-Path $backupFile)) {
        Write-Error "Archivo de backup no encontrado: $backupFile"
        Write-Info "Archivos disponibles en el directorio:"
        Get-ChildItem $InstallerPath -Filter "*.bak" | ForEach-Object { Write-Host "  - $($_.Name)" }
        return $false
    }
    
    $backupSize = (Get-Item $backupFile).Length / 1MB
    Write-Info "Tamaño del backup: $([math]::Round($backupSize, 2)) MB"
    
    $dbName = $DbConfig.BaseDatos
    $server = $DbConfig.Servidor
    
    Write-Info "Restaurando desde: $backupFile"
    Write-Info "Servidor destino: $server"
    Write-Info "Base de datos destino: $dbName"
    
    # Variable para el archivo temporal (se inicializa aquí para el cleanup)
    $targetBackupFile = $null
    
    try {
        # Usar ADO.NET para restaurar (más confiable que sqlcmd)
        $masterConn = Build-MasterConnectionString -DbConfig $DbConfig
        $connection = New-Object System.Data.SqlClient.SqlConnection
        $connection.ConnectionString = $masterConn
        $connection.Open()
        
        $cmd = $connection.CreateCommand()
        $cmd.CommandTimeout = 300  # 5 minutos timeout
        
        # 1. Obtener la ruta de datos predeterminada del servidor
        Write-Info "Obteniendo rutas de datos del servidor..."
        $cmd.CommandText = "SELECT SERVERPROPERTY('InstanceDefaultDataPath') AS DataPath, SERVERPROPERTY('InstanceDefaultLogPath') AS LogPath"
        $reader = $cmd.ExecuteReader()
        $dataPath = ""
        $logPath = ""
        if ($reader.Read()) {
            $dataPath = $reader["DataPath"]
            $logPath = $reader["LogPath"]
        }
        $reader.Close()
        
        if (-not $dataPath) {
            Write-Warning "No se pudo obtener ruta de datos, usando ruta por defecto"
            $dataPath = "C:\Program Files\Microsoft SQL Server\MSSQL16.SQL2022\MSSQL\DATA\"
            $logPath = $dataPath
        }
        
        Write-Info "Ruta de datos: $dataPath"
        
        # 2. Copiar backup a carpeta de SQL Server (para evitar problemas de permisos)
        $sqlBackupFolder = Split-Path $dataPath -Parent | Join-Path -ChildPath "Backup"
        if (-not (Test-Path $sqlBackupFolder)) {
            # Intentar crear o usar la carpeta DATA
            $sqlBackupFolder = $dataPath
        }
        
        $targetBackupFile = Join-Path $sqlBackupFolder "SistemIA_Restore_Temp.bak"
        
        Write-Info "Copiando backup a carpeta de SQL Server..."
        Write-Info "  Destino: $targetBackupFile"
        
        try {
            Copy-Item -Path $backupFile -Destination $targetBackupFile -Force
            Write-Success "Backup copiado correctamente"
            $backupFile = $targetBackupFile
        }
        catch {
            Write-Warning "No se pudo copiar a carpeta de SQL Server: $_"
            Write-Info "Intentando usar ubicación original (puede fallar por permisos)..."
        }
        
        $mdfPath = Join-Path $dataPath "$dbName.mdf"
        $ldfPath = Join-Path $logPath "${dbName}_log.ldf"
        
        # 2. Verificar si la BD existe y cerrar conexiones
        Write-Info "Preparando servidor..."
        $cmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '$dbName'"
        $exists = $cmd.ExecuteScalar()
        
        if ($exists -gt 0) {
            Write-Info "Cerrando conexiones existentes..."
            try {
                $cmd.CommandText = "ALTER DATABASE [$dbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
                $cmd.ExecuteNonQuery() | Out-Null
            } catch { }
            
            Write-Info "Eliminando BD existente..."
            $cmd.CommandText = "DROP DATABASE [$dbName]"
            $cmd.ExecuteNonQuery() | Out-Null
        }
        
        # 3. Obtener nombres lógicos del backup
        Write-Info "Analizando archivo de backup..."
        $cmd.CommandText = "RESTORE FILELISTONLY FROM DISK = '$backupFile'"
        $reader = $cmd.ExecuteReader()
        $logicalDataName = ""
        $logicalLogName = ""
        while ($reader.Read()) {
            $fileType = $reader["Type"].ToString()
            $logicalName = $reader["LogicalName"].ToString()
            if ($fileType -eq "D") {
                $logicalDataName = $logicalName
            }
            elseif ($fileType -eq "L") {
                $logicalLogName = $logicalName
            }
        }
        $reader.Close()
        
        Write-Info "Nombre lógico de datos: $logicalDataName"
        Write-Info "Nombre lógico de log: $logicalLogName"
        
        # 4. Restaurar la base de datos
        Write-Info "Restaurando base de datos (esto puede tardar)..."
        $restoreQuery = @"
RESTORE DATABASE [$dbName] 
FROM DISK = '$backupFile' 
WITH 
    MOVE '$logicalDataName' TO '$mdfPath',
    MOVE '$logicalLogName' TO '$ldfPath',
    REPLACE,
    STATS = 10
"@
        $cmd.CommandText = $restoreQuery
        $cmd.ExecuteNonQuery() | Out-Null
        
        Write-Success "Restauración completada"
        
        # 5. Verificar que la BD se restauró correctamente
        Write-Info "Verificando restauración..."
        $cmd.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '$dbName' AND state = 0"
        $dbOk = $cmd.ExecuteScalar()
        
        if ($dbOk -eq 0) {
            throw "La base de datos no está en estado ONLINE después de la restauración"
        }
        
        # 6. Contar tablas para verificar que tiene datos
        $connection.Close()
        
        # Conectar a la BD restaurada para verificar
        $dbConnString = Build-ConnectionString -DbConfig $DbConfig
        $dbConnection = New-Object System.Data.SqlClient.SqlConnection
        $dbConnection.ConnectionString = $dbConnString
        $dbConnection.Open()
        
        $dbCmd = $dbConnection.CreateCommand()
        $dbCmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
        $tableCount = $dbCmd.ExecuteScalar()
        
        $dbConnection.Close()
        
        Write-Success "Base de datos restaurada exitosamente"
        Write-Success "Tablas encontradas: $tableCount"
        
        # 7. Limpiar archivo temporal de backup si se creó
        if ($targetBackupFile -and (Test-Path $targetBackupFile)) {
            try {
                Remove-Item $targetBackupFile -Force -ErrorAction SilentlyContinue
                Write-Info "Archivo temporal de backup eliminado"
            } catch { }
        }
        
        return $true
    }
    catch {
        Write-Error "Error al restaurar base de datos: $_"
        Write-Info "Detalles del error:"
        Write-Host $_.Exception.Message -ForegroundColor Red
        
        # Limpiar archivo temporal en caso de error también
        if ($targetBackupFile -and (Test-Path $targetBackupFile)) {
            try {
                Remove-Item $targetBackupFile -Force -ErrorAction SilentlyContinue
            } catch { }
        }
        
        return $false
    }
}

# Crear estructura completa de BD - SOLO desde backup (método más confiable)
function Initialize-DatabaseStructure {
    param(
        $DbConfig,
        [string]$InstallerPath
    )
    
    Write-Title "INICIALIZANDO BASE DE DATOS"
    Write-Host ""
    Write-Info "La base de datos se creará restaurando desde un backup limpio."
    Write-Info "Este método garantiza la integridad de la estructura."
    Write-Host ""
    
    # Verificar que existe el archivo de backup
    $backupFile = Join-Path $InstallerPath "SistemIA_Base.bak"
    if (-not (Test-Path $backupFile)) {
        Write-Error "No se encontró el archivo de backup: $backupFile"
        Write-Error "El instalador requiere el archivo SistemIA_Base.bak para funcionar."
        Write-Info "Genere el backup ejecutando: .\GenerarBackupLimpio.ps1"
        return $false
    }
    
    $result = Restore-DatabaseFromBackup -DbConfig $DbConfig -InstallerPath $InstallerPath
    
    if (-not $result) {
        Write-Error "Error al restaurar la base de datos"
        Write-Info "Verifique los permisos de SQL Server y el archivo de backup"
        return $false
    }
    
    Write-Success "Base de datos inicializada correctamente"
    return $true
}

# Crear appsettings.json
function New-AppSettings {
    param($Config, [string]$DestPath)
    
    Write-Info "Generando appsettings.json..."
    
    $connectionString = Build-ConnectionString -DbConfig $Config.BaseDatos
    
    $appSettings = @{
        ConnectionStrings = @{
            DefaultConnection = $connectionString
        }
        Logging = @{
            LogLevel = @{
                Default = "Information"
                "Microsoft.AspNetCore" = "Warning"
            }
        }
        AllowedHosts = "*"
        Kestrel = @{
            Endpoints = @{
                Http = @{
                    Url = "http://0.0.0.0:$($Config.Instalacion.PuertoHttp)"
                }
                Https = @{
                    Url = "https://0.0.0.0:$($Config.Instalacion.PuertoHttps)"
                }
            }
        }
    }
    
    $appSettingsJson = $appSettings | ConvertTo-Json -Depth 5
    $appSettingsPath = Join-Path $DestPath "appsettings.json"
    
    Set-Content -Path $appSettingsPath -Value $appSettingsJson -Encoding UTF8
    Write-Success "appsettings.json creado en $appSettingsPath"
}

# Instalar servicio de Windows
function Install-SistemIAService {
    param($Config)
    
    $serviceName = $Config.Instalacion.NombreServicio
    $exePath = Join-Path $Config.Instalacion.RutaInstalacion "SistemIA.exe"
    
    Write-Info "Instalando servicio Windows: $serviceName"
    Write-Info "Ejecutable: $exePath"
    
    # Verificar que el ejecutable existe
    if (-not (Test-Path $exePath)) {
        Write-Error "No se encontró el ejecutable: $exePath"
        Write-Info "Verifique que los archivos se copiaron correctamente"
        return $false
    }
    
    # Verificar si ya existe
    $existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($existingService) {
        Write-Warning "El servicio ya existe. Deteniendo..."
        Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 1
        sc.exe delete $serviceName | Out-Null
        Start-Sleep -Seconds 2
    }
    
    # Crear el servicio
    $params = @{
        Name = $serviceName
        BinaryPathName = "`"$exePath`""
        DisplayName = $Config.Instalacion.DescripcionServicio
        StartupType = if ($Config.Instalacion.InicioAutomatico) { "Automatic" } else { "Manual" }
        Description = "Servidor web SistemIA - Sistema de Gestión Empresarial"
    }
    
    try {
        New-Service @params
        Write-Success "Servicio instalado correctamente"
        
        # Intentar iniciar el servicio
        Write-Info "Iniciando servicio..."
        try {
            Start-Service -Name $serviceName -ErrorAction Stop
            Start-Sleep -Seconds 2
            
            $svc = Get-Service -Name $serviceName
            if ($svc.Status -eq 'Running') {
                Write-Success "Servicio iniciado correctamente"
            }
            else {
                Write-Warning "El servicio no está corriendo. Estado: $($svc.Status)"
                Write-Info "Revise los logs del sistema para más detalles"
                Write-Info "Puede iniciar manualmente con: Start-Service $serviceName"
            }
        }
        catch {
            Write-Warning "No se pudo iniciar el servicio automáticamente: $_"
            Write-Info "El servicio está instalado pero debe iniciarse manualmente"
            Write-Info "Puede usar: net start $serviceName"
            Write-Info "O ejecutar directamente: $exePath"
        }
        
        return $true
    }
    catch {
        Write-Error "Error al instalar servicio: $_"
        return $false
    }
}

# Verificar estado del servicio y diagnosticar problemas
function Test-ServiceStatus {
    param($Config)
    
    $serviceName = $Config.Instalacion.NombreServicio
    $exePath = Join-Path $Config.Instalacion.RutaInstalacion "SistemIA.exe"
    
    Write-Title "DIAGNÓSTICO DEL SERVICIO"
    
    # Verificar ejecutable
    if (Test-Path $exePath) {
        Write-Success "Ejecutable encontrado: $exePath"
        $exeInfo = Get-Item $exePath
        Write-Info "  Tamaño: $([math]::Round($exeInfo.Length / 1MB, 2)) MB"
        Write-Info "  Fecha: $($exeInfo.LastWriteTime)"
    }
    else {
        Write-Error "Ejecutable NO encontrado: $exePath"
        return
    }
    
    # Verificar appsettings.json
    $appSettingsPath = Join-Path $Config.Instalacion.RutaInstalacion "appsettings.json"
    if (Test-Path $appSettingsPath) {
        Write-Success "appsettings.json encontrado"
    }
    else {
        Write-Error "appsettings.json NO encontrado"
    }
    
    # Verificar servicio
    $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($service) {
        Write-Info "Estado del servicio: $($service.Status)"
        
        # Obtener detalles del servicio
        $serviceInfo = Get-WmiObject Win32_Service -Filter "Name='$serviceName'" -ErrorAction SilentlyContinue
        if ($serviceInfo) {
            Write-Info "  Ruta: $($serviceInfo.PathName)"
            Write-Info "  Usuario: $($serviceInfo.StartName)"
            Write-Info "  Modo inicio: $($serviceInfo.StartMode)"
        }
        
        # Verificar eventos del servicio
        try {
            $events = Get-EventLog -LogName Application -Source ".NET Runtime" -Newest 5 -EntryType Error -ErrorAction SilentlyContinue
            if ($events) {
                Write-Warning "Últimos errores .NET en Application Log:"
                $events | ForEach-Object {
                    Write-Host "  [$($_.TimeGenerated)] $($_.Message.Substring(0, [Math]::Min(150, $_.Message.Length)))..." -ForegroundColor Yellow
                }
            }
        } catch { }
    }
    else {
        Write-Warning "Servicio NO instalado"
    }
    
    # Probar ejecución directa
    Write-Host ""
    $testDirect = Read-Host "¿Desea probar ejecutar la aplicación directamente? (S/N)"
    if ($testDirect -eq "S" -or $testDirect -eq "s") {
        Write-Info "Ejecutando $exePath directamente..."
        Write-Info "Presione Ctrl+C para detener"
        Write-Host ""
        Push-Location $Config.Instalacion.RutaInstalacion
        & $exePath
        Pop-Location
    }
}

# Desinstalar servicio
function Uninstall-SistemIAService {
    param($Config)
    
    $serviceName = $Config.Instalacion.NombreServicio
    
    Write-Info "Desinstalando servicio: $serviceName"
    
    $existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if (-not $existingService) {
        Write-Warning "El servicio no existe"
        return $true
    }
    
    try {
        Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
        sc.exe delete $serviceName | Out-Null
        Write-Success "Servicio desinstalado"
        return $true
    }
    catch {
        Write-Error "Error al desinstalar servicio: $_"
        return $false
    }
}

# Instalar .NET SDK si no está presente
function Install-DotNetSdk {
    Write-Title "INSTALANDO .NET SDK"
    
    # Verificar si ya está instalado
    $dotnetVersion = & dotnet --version 2>$null
    if ($dotnetVersion) {
        Write-Success ".NET SDK ya está instalado: $dotnetVersion"
        return $true
    }
    
    Write-Info "Descargando instalador de .NET 8 SDK..."
    
    try {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        
        # Usar el script oficial de instalación de Microsoft
        $installScript = Join-Path $env:TEMP "dotnet-install.ps1"
        
        Write-Info "Descargando script de instalación..."
        Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
        
        if (-not (Test-Path $installScript)) {
            throw "No se pudo descargar el script de instalación"
        }
        
        Write-Info "Instalando .NET 8 SDK (esto puede tardar varios minutos)..."
        
        # Ejecutar instalación del SDK
        & $installScript -Channel 8.0 -InstallDir "$env:ProgramFiles\dotnet" -NoPath
        
        # Agregar al PATH si no está
        $dotnetPath = "$env:ProgramFiles\dotnet"
        $currentPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
        if ($currentPath -notlike "*$dotnetPath*") {
            Write-Info "Agregando .NET al PATH del sistema..."
            [Environment]::SetEnvironmentVariable("Path", "$currentPath;$dotnetPath", "Machine")
        }
        
        # Actualizar PATH en la sesión actual
        $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")
        
        # Verificar instalación
        $newVersion = & "$dotnetPath\dotnet.exe" --version 2>$null
        if ($newVersion) {
            Write-Success ".NET SDK instalado correctamente"
            Write-Success "Versión: $newVersion"
            return $true
        }
        else {
            Write-Warning "SDK instalado pero requiere reiniciar la consola"
            Write-Info "Cierre esta ventana y vuelva a ejecutar el instalador"
            return $false
        }
    }
    catch {
        Write-Error "Error al instalar .NET SDK: $_"
        Write-Info ""
        Write-Info "Puede instalar manualmente desde:"
        Write-Info "  https://dotnet.microsoft.com/download/dotnet/8.0"
        return $false
    }
    finally {
        # Limpiar script
        if (Test-Path $installScript) {
            Remove-Item $installScript -Force -ErrorAction SilentlyContinue
        }
    }
}

# ============================================
# MKCERT - Certificados de Confianza Local
# ============================================

# Instalar mkcert si no está instalado
function Install-Mkcert {
    Write-Title "INSTALANDO MKCERT"
    
    try {
        # Verificar si ya está instalado
        $mkcertPath = (Get-Command mkcert -ErrorAction SilentlyContinue).Path
        if ($mkcertPath) {
            Write-Success "mkcert ya está instalado en: $mkcertPath"
            return $true
        }
        
        Write-Info "Descargando mkcert..."
        
        # Directorio de instalación
        $installDir = "C:\Program Files\mkcert"
        if (-not (Test-Path $installDir)) {
            New-Item -ItemType Directory -Path $installDir -Force | Out-Null
        }
        
        $mkcertExe = Join-Path $installDir "mkcert.exe"
        
        # Descargar última versión de mkcert
        $mkcertUrl = "https://dl.filippo.io/mkcert/latest?for=windows/amd64"
        
        # Usar WebClient para descargar
        $webClient = New-Object System.Net.WebClient
        $webClient.Headers.Add("User-Agent", "PowerShell")
        $webClient.DownloadFile($mkcertUrl, $mkcertExe)
        
        Write-Success "mkcert descargado a: $mkcertExe"
        
        # Agregar al PATH del sistema si no está
        $currentPath = [Environment]::GetEnvironmentVariable("Path", "Machine")
        if ($currentPath -notlike "*$installDir*") {
            [Environment]::SetEnvironmentVariable("Path", "$currentPath;$installDir", "Machine")
            $env:Path = "$env:Path;$installDir"
            Write-Info "mkcert agregado al PATH del sistema"
        }
        
        # Instalar CA raíz de mkcert
        Write-Info "Instalando Autoridad Certificadora local de mkcert..."
        Write-Info "(Esto hace que los certificados sean confiables para Windows y navegadores)"
        
        & $mkcertExe -install
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "CA de mkcert instalada correctamente"
            Write-Success "Los certificados generados serán confiables en Chrome, Edge y Firefox"
            return $true
        }
        else {
            Write-Warning "mkcert instalado pero la CA puede requerir configuración manual"
            return $true
        }
    }
    catch {
        Write-Error "Error al instalar mkcert: $_"
        Write-Info ""
        Write-Info "Puede instalar manualmente:"
        Write-Info "  winget install FiloSottile.mkcert"
        Write-Info "  o descargarlo de: https://github.com/FiloSottile/mkcert/releases"
        return $false
    }
}

# Generar certificado con mkcert
function New-MkcertCertificate {
    param(
        [string]$InstallPath = "C:\SistemIA",
        [string[]]$Domains = @("localhost", "sistemía.local")
    )
    
    Write-Title "GENERANDO CERTIFICADO CON MKCERT"
    
    try {
        # Verificar que mkcert está instalado
        $mkcertExe = (Get-Command mkcert -ErrorAction SilentlyContinue).Path
        if (-not $mkcertExe) {
            $mkcertExe = "C:\Program Files\mkcert\mkcert.exe"
            if (-not (Test-Path $mkcertExe)) {
                Write-Error "mkcert no está instalado. Ejecute primero Install-Mkcert"
                return $null
            }
        }
        
        # Obtener nombre del equipo e IPs locales
        $hostname = $env:COMPUTERNAME
        $localIps = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notlike "127.*" -and $_.IPAddress -notlike "169.*" }).IPAddress
        
        # Construir lista de dominios
        $allDomains = @("localhost", "127.0.0.1", $hostname, "$hostname.local") + $Domains + $localIps
        $allDomains = $allDomains | Select-Object -Unique
        
        Write-Info "Generando certificado para:"
        $allDomains | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }
        
        # Crear directorio de certificados si no existe
        $certDir = Join-Path $InstallPath "Certificados"
        if (-not (Test-Path $certDir)) {
            New-Item -ItemType Directory -Path $certDir -Force | Out-Null
        }
        
        # Generar certificado
        $certName = "sistemía"
        $certPath = Join-Path $certDir "$certName.pem"
        $keyPath = Join-Path $certDir "$certName-key.pem"
        
        Push-Location $certDir
        & $mkcertExe -cert-file "$certName.pem" -key-file "$certName-key.pem" $allDomains
        Pop-Location
        
        if ($LASTEXITCODE -ne 0 -or -not (Test-Path $certPath)) {
            Write-Error "Error al generar certificado con mkcert"
            return $null
        }
        
        Write-Success "Certificado generado:"
        Write-Info "  Certificado: $certPath"
        Write-Info "  Clave:       $keyPath"
        
        # Convertir PEM a PFX para Kestrel (opcional, Kestrel soporta PEM)
        $pfxPath = Join-Path $certDir "sistemía.pfx"
        $pfxPassword = "SistemIA2024!"
        
        # Usar OpenSSL si está disponible, o generar PFX con .NET
        Write-Info "Convirtiendo a formato PFX..."
        
        try {
            # Leer certificado y clave PEM
            $certPem = Get-Content $certPath -Raw
            $keyPem = Get-Content $keyPath -Raw
            
            # Usar .NET para crear PFX
            $certBytes = [System.Text.Encoding]::UTF8.GetBytes($certPem)
            $keyBytes = [System.Text.Encoding]::UTF8.GetBytes($keyPem)
            
            # Crear certificado X509 con clave privada
            $cert = [System.Security.Cryptography.X509Certificates.X509Certificate2]::CreateFromPemFile($certPath, $keyPath)
            
            # Exportar a PFX
            $pfxBytes = $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, $pfxPassword)
            [System.IO.File]::WriteAllBytes($pfxPath, $pfxBytes)
            
            Write-Success "PFX generado: $pfxPath"
            
            return @{
                CertPath = $certPath
                KeyPath = $keyPath
                PfxPath = $pfxPath
                PfxPassword = $pfxPassword
                Domains = $allDomains
            }
        }
        catch {
            Write-Warning "No se pudo crear PFX, usando certificado PEM directamente"
            return @{
                CertPath = $certPath
                KeyPath = $keyPath
                PfxPath = $null
                PfxPassword = $null
                Domains = $allDomains
            }
        }
    }
    catch {
        Write-Error "Error al generar certificado: $_"
        return $null
    }
}

# ============================================
# ACCESO DIRECTO EN ESCRITORIO
# ============================================

function New-DesktopShortcut {
    param(
        [string]$Name = "SistemIA",
        [string]$TargetUrl,
        [string]$IconPath,
        [string]$Description = "Sistema de Gestión Empresarial"
    )
    
    Write-Title "CREANDO ACCESO DIRECTO EN ESCRITORIO"
    
    try {
        # Obtener ruta del escritorio (para todos los usuarios o usuario actual)
        $desktopPath = [Environment]::GetFolderPath("Desktop")
        $publicDesktopPath = [Environment]::GetFolderPath("CommonDesktopDirectory")
        
        # Usar escritorio público si es posible
        $targetDesktop = if (Test-Path $publicDesktopPath) { $publicDesktopPath } else { $desktopPath }
        
        $shortcutPath = Join-Path $targetDesktop "$Name.url"
        
        # Crear acceso directo URL (más compatible que .lnk para URLs)
        $shortcutContent = @"
[InternetShortcut]
URL=$TargetUrl
IconIndex=0
"@
        
        # Si hay icono personalizado
        if ($IconPath -and (Test-Path $IconPath)) {
            $shortcutContent += "`r`nIconFile=$IconPath"
        }
        
        Set-Content -Path $shortcutPath -Value $shortcutContent -Encoding ASCII
        
        Write-Success "Acceso directo creado: $shortcutPath"
        Write-Info "URL: $TargetUrl"
        
        # También crear un acceso directo .lnk si es posible (para mejor integración con Windows)
        try {
            $lnkPath = Join-Path $targetDesktop "$Name.lnk"
            $WScriptShell = New-Object -ComObject WScript.Shell
            $shortcut = $WScriptShell.CreateShortcut($lnkPath)
            
            # Usar el navegador predeterminado
            $shortcut.TargetPath = $TargetUrl
            $shortcut.Description = $Description
            $shortcut.Save()
            
            # Eliminar el .url si el .lnk funcionó
            Remove-Item $shortcutPath -Force -ErrorAction SilentlyContinue
            Write-Success "Acceso directo mejorado creado: $lnkPath"
        }
        catch {
            # Si falla el .lnk, el .url sigue funcionando
            Write-Info "Usando acceso directo URL estándar"
        }
        
        return $true
    }
    catch {
        Write-Error "Error al crear acceso directo: $_"
        return $false
    }
}

# Crear acceso directo para modo cliente
function New-ClientShortcut {
    param($Config)
    
    $protocol = if ($Config.Cliente.UsarHttps) { "https" } else { "http" }
    $url = "$protocol`://$($Config.Cliente.ServidorRemoto):$($Config.Cliente.PuertoRemoto)"
    
    New-DesktopShortcut -Name "SistemIA" -TargetUrl $url -Description "SistemIA - Conectar a $($Config.Cliente.ServidorRemoto)"
}

# Crear acceso directo para modo servidor
function New-ServerShortcut {
    param($Config)
    
    $httpUrl = "http://localhost:$($Config.Instalacion.PuertoHttp)"
    $httpsUrl = "https://localhost:$($Config.Instalacion.PuertoHttps)"
    
    # Crear acceso HTTP (principal)
    New-DesktopShortcut -Name "SistemIA" -TargetUrl $httpUrl -Description "SistemIA - Sistema de Gestión Empresarial"
    
    # Crear acceso HTTPS (adicional)
    New-DesktopShortcut -Name "SistemIA (HTTPS)" -TargetUrl $httpsUrl -Description "SistemIA - Conexión Segura"
}

# Instalar certificado HTTPS de desarrollo
function Install-HttpsCertificate {
    param(
        [string]$InstallPath = "C:\SistemIA",
        [bool]$UsarMkcert = $true
    )
    
    Write-Title "CONFIGURANDO CERTIFICADO HTTPS"
    
    try {
        $certPath = Join-Path $InstallPath "certificate.pfx"
        $certPassword = "SistemIA2024!"
        
        # Verificar si ya existe un certificado configurado
        if (Test-Path $certPath) {
            Write-Info "Ya existe un certificado en: $certPath"
            $reemplazar = Read-Host "¿Desea generar uno nuevo? (S/N)"
            if ($reemplazar -notmatch '^[sS]$') {
                Write-Success "Usando certificado existente"
                return $true
            }
        }
        
        # Decidir método de generación
        if ($UsarMkcert) {
            Write-Info "Usando mkcert para generar certificado de confianza..."
            
            # Instalar mkcert si no está disponible
            $mkcertInstalled = Install-Mkcert
            if (-not $mkcertInstalled) {
                Write-Warning "mkcert no disponible. Usando certificado autofirmado como alternativa..."
                $UsarMkcert = $false
            }
            else {
                # Generar certificado con mkcert
                $mkcertResult = New-MkcertCertificate -InstallPath $InstallPath
                if ($mkcertResult -and $mkcertResult.PfxPath) {
                    $certPath = $mkcertResult.PfxPath
                    
                    # Actualizar appsettings.json
                    Update-AppSettingsCertificate -InstallPath $InstallPath -CertPath $certPath -CertPassword $certPassword
                    
                    Write-Host ""
                    Write-Success "Certificado HTTPS configurado con mkcert"
                    Write-Info "HTTP:  http://localhost:5095"
                    Write-Info "HTTPS: https://localhost:7060"
                    Write-Host ""
                    Write-Host "[INFO] Este certificado es de CONFIANZA para Chrome, Edge y Firefox" -ForegroundColor Green
                    Write-Host "       No verá advertencias de seguridad en este equipo." -ForegroundColor Green
                    Write-Host ""
                    Write-Host "[ANDROID] Para usar en dispositivos Android:" -ForegroundColor Yellow
                    Write-Host "  1. Copie el archivo rootCA.pem del directorio:" -ForegroundColor White
                    Write-Host "     $env:LOCALAPPDATA\mkcert" -ForegroundColor Gray
                    Write-Host "  2. Instálelo en Android: Configuración > Seguridad > Instalar certificado CA" -ForegroundColor White
                    
                    return $true
                }
            }
        }
        
        # Método tradicional: certificado autofirmado
        Write-Info "Generando certificado autofirmado para HTTPS..."
        
        # Obtener el nombre del equipo para el certificado
        $hostname = $env:COMPUTERNAME
        $localIps = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notlike "127.*" -and $_.IPAddress -notlike "169.*" }).IPAddress
        $dnsNames = @("localhost", $hostname, "$hostname.local", "127.0.0.1") + $localIps
        
        # Crear certificado autofirmado
        $cert = New-SelfSignedCertificate `
            -DnsName $dnsNames `
            -CertStoreLocation "Cert:\LocalMachine\My" `
            -NotAfter (Get-Date).AddYears(5) `
            -FriendlyName "SistemIA HTTPS Certificate" `
            -KeyAlgorithm RSA `
            -KeyLength 2048 `
            -KeyExportPolicy Exportable `
            -HashAlgorithm SHA256
        
        Write-Info "Certificado creado con thumbprint: $($cert.Thumbprint)"
        
        # Exportar a archivo PFX
        $securePassword = ConvertTo-SecureString -String $certPassword -Force -AsPlainText
        Export-PfxCertificate -Cert $cert -FilePath $certPath -Password $securePassword | Out-Null
        
        Write-Success "Certificado exportado a: $certPath"
        
        # Copiar al almacén de certificados raíz de confianza para que los navegadores lo acepten
        Write-Info "Instalando certificado en almacén de confianza..."
        $rootStore = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "LocalMachine")
        $rootStore.Open("ReadWrite")
        $rootStore.Add($cert)
        $rootStore.Close()
        
        Write-Success "Certificado instalado en almacén de confianza"
        
        # Actualizar appsettings.json
        Update-AppSettingsCertificate -InstallPath $InstallPath -CertPath $certPath -CertPassword $certPassword
        
        Write-Host ""
        Write-Success "Certificado HTTPS configurado correctamente"
        Write-Info "HTTP:  http://localhost:5095"
        Write-Info "HTTPS: https://localhost:7060"
        Write-Host ""
        Write-Host "[NOTA] Los navegadores mostrarán una advertencia de seguridad" -ForegroundColor Yellow
        Write-Host "       la primera vez. Esto es normal para certificados autofirmados." -ForegroundColor Yellow
        
        return $true
    }
    catch {
        Write-Error "Error al configurar certificado HTTPS: $_"
        return $false
    }
}

# Función auxiliar para actualizar appsettings.json con certificado
function Update-AppSettingsCertificate {
    param(
        [string]$InstallPath,
        [string]$CertPath,
        [string]$CertPassword
    )
    
    $appSettingsPath = Join-Path $InstallPath "appsettings.json"
    if (Test-Path $appSettingsPath) {
        Write-Info "Actualizando configuración de Kestrel..."
        $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
        
        # Agregar configuración de Kestrel si no existe
        if (-not $appSettings.Kestrel) {
            $appSettings | Add-Member -NotePropertyName "Kestrel" -NotePropertyValue @{} -Force
        }
        
        $appSettings.Kestrel = @{
            Endpoints = @{
                Http = @{
                    Url = "http://0.0.0.0:5095"
                }
                Https = @{
                    Url = "https://0.0.0.0:7060"
                    Certificate = @{
                        Path = $CertPath
                        Password = $CertPassword
                    }
                }
            }
        }
        
        $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath -Encoding UTF8
        Write-Success "Configuración de Kestrel actualizada"
    }
}

# Configurar reglas de Firewall para acceso en red
function Configure-Firewall {
    param(
        [int]$HttpPort = 5095,
        [int]$HttpsPort = 7060
    )
    
    Write-Title "CONFIGURANDO FIREWALL DE WINDOWS"
    
    try {
        # Eliminar reglas anteriores si existen
        Write-Info "Eliminando reglas anteriores de SistemIA..."
        Get-NetFirewallRule -DisplayName "SistemIA*" -ErrorAction SilentlyContinue | Remove-NetFirewallRule -ErrorAction SilentlyContinue
        
        # Crear regla para HTTP
        Write-Info "Creando regla para HTTP (puerto $HttpPort)..."
        New-NetFirewallRule `
            -DisplayName "SistemIA HTTP ($HttpPort)" `
            -Description "Permite acceso HTTP al sistema SistemIA" `
            -Direction Inbound `
            -Protocol TCP `
            -LocalPort $HttpPort `
            -Action Allow `
            -Profile Domain,Private,Public `
            -Enabled True | Out-Null
        Write-Success "Regla HTTP creada"
        
        # Crear regla para HTTPS
        Write-Info "Creando regla para HTTPS (puerto $HttpsPort)..."
        New-NetFirewallRule `
            -DisplayName "SistemIA HTTPS ($HttpsPort)" `
            -Description "Permite acceso HTTPS al sistema SistemIA" `
            -Direction Inbound `
            -Protocol TCP `
            -LocalPort $HttpsPort `
            -Action Allow `
            -Profile Domain,Private,Public `
            -Enabled True | Out-Null
        Write-Success "Regla HTTPS creada"
        
        # Mostrar resumen
        Write-Host ""
        Write-Success "Firewall configurado correctamente"
        Write-Host ""
        Write-Host "El sistema será accesible desde otros equipos en:" -ForegroundColor Cyan
        
        # Obtener IPs de la máquina
        $ips = Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -ne "127.0.0.1" -and $_.PrefixOrigin -ne "WellKnown" }
        foreach ($ip in $ips) {
            Write-Host "  HTTP:  http://$($ip.IPAddress):$HttpPort" -ForegroundColor Yellow
            Write-Host "  HTTPS: https://$($ip.IPAddress):$HttpsPort" -ForegroundColor Yellow
        }
        Write-Host ""
        
        return $true
    }
    catch {
        Write-Error "Error al configurar firewall: $_"
        return $false
    }
}

# Mostrar información de acceso en red
function Show-NetworkAccess {
    param(
        [int]$HttpPort = 5095,
        [int]$HttpsPort = 7060
    )
    
    Write-Title "INFORMACIÓN DE ACCESO EN RED"
    
    $hostname = $env:COMPUTERNAME
    
    Write-Host ""
    Write-Host "Acceso local:" -ForegroundColor Cyan
    Write-Host "  HTTP:  http://localhost:$HttpPort" -ForegroundColor White
    Write-Host "  HTTPS: https://localhost:$HttpsPort" -ForegroundColor White
    Write-Host ""
    Write-Host "Acceso por nombre de equipo:" -ForegroundColor Cyan
    Write-Host "  HTTP:  http://${hostname}:$HttpPort" -ForegroundColor White
    Write-Host "  HTTPS: https://${hostname}:$HttpsPort" -ForegroundColor White
    Write-Host ""
    Write-Host "Acceso por IP:" -ForegroundColor Cyan
    
    $ips = Get-NetIPAddress -AddressFamily IPv4 | Where-Object { 
        $_.IPAddress -ne "127.0.0.1" -and 
        $_.PrefixOrigin -ne "WellKnown" -and
        $_.AddressState -eq "Preferred"
    }
    
    foreach ($ip in $ips) {
        Write-Host "  HTTP:  http://$($ip.IPAddress):$HttpPort" -ForegroundColor Yellow
        Write-Host "  HTTPS: https://$($ip.IPAddress):$HttpsPort" -ForegroundColor Yellow
    }
    
    Write-Host ""
    
    # Verificar estado del firewall
    $httpRule = Get-NetFirewallRule -DisplayName "SistemIA HTTP*" -ErrorAction SilentlyContinue
    $httpsRule = Get-NetFirewallRule -DisplayName "SistemIA HTTPS*" -ErrorAction SilentlyContinue
    
    if ($httpRule -and $httpsRule) {
        Write-Success "Reglas de firewall configuradas correctamente"
    }
    else {
        Write-Warning "Las reglas de firewall no están configuradas"
        Write-Info "Use la opción 11 del menú para configurar el firewall"
    }
}

# Copiar archivos de la aplicación
function Copy-ApplicationFiles {
    param($Config, [string]$SourcePath, [string]$InstallerPath)
    
    $destPath = $Config.Instalacion.RutaInstalacion
    
    Write-Info "Copiando archivos a $destPath..."
    
    # Crear directorio si no existe
    if (-not (Test-Path $destPath)) {
        New-Item -ItemType Directory -Path $destPath -Force | Out-Null
    }
    
    # Primero buscar carpeta publish incluida en el instalador
    $publishPath = Join-Path $InstallerPath "publish"
    
    if (-not (Test-Path $publishPath)) {
        # Si no hay publish en instalador, buscar en el proyecto fuente
        $publishPath = Join-Path $SourcePath "bin\Release\net8.0\publish"
        if (-not (Test-Path $publishPath)) {
            $publishPath = Join-Path $SourcePath "bin\Debug\net8.0"
        }
    }
    
    if (-not (Test-Path $publishPath)) {
        Write-Error "No se encontró la carpeta de publicación."
        Write-Info "El paquete de instalación debe incluir la carpeta 'publish' con los binarios."
        return $false
    }
    
    try {
        Copy-Item -Path "$publishPath\*" -Destination $destPath -Recurse -Force
        Write-Success "Archivos copiados correctamente"
        
        # Copiar modelos de reconocimiento facial si existen
        $faceModelsSource = Join-Path $InstallerPath "face_recognition_models"
        if (-not (Test-Path $faceModelsSource)) {
            $faceModelsSource = Join-Path $publishPath "face_recognition_models"
        }
        if (Test-Path $faceModelsSource) {
            $faceModelsDest = Join-Path $destPath "face_recognition_models"
            if (-not (Test-Path $faceModelsDest)) {
                New-Item -ItemType Directory -Path $faceModelsDest -Force | Out-Null
            }
            Copy-Item -Path "$faceModelsSource\*" -Destination $faceModelsDest -Recurse -Force
            Write-Success "Modelos de reconocimiento facial copiados"
        }
        else {
            Write-Warning "No se encontraron modelos de reconocimiento facial (face_recognition_models)"
        }
        
        return $true
    }
    catch {
        Write-Error "Error al copiar archivos: $_"
        return $false
    }
}

# Publicar la aplicación (solo si hay código fuente)
function Publish-Application {
    param([string]$ProjectPath, [string]$InstallerPath)
    
    # Verificar si ya hay binarios publicados en el instalador
    $publishPath = Join-Path $InstallerPath "publish"
    if (Test-Path $publishPath) {
        Write-Info "Usando binarios pre-compilados del instalador..."
        return $true
    }
    
    Write-Info "Publicando aplicación..."
    
    $csprojPath = Join-Path $ProjectPath "SistemIA.csproj"
    
    if (-not (Test-Path $csprojPath)) {
        Write-Error "No se encontró el proyecto: $csprojPath"
        Write-Info "Si está instalando desde un paquete, asegúrese de que incluya la carpeta 'publish'"
        return $false
    }
    
    try {
        Push-Location $ProjectPath
        
        # Publicar para Windows
        dotnet publish -c Release -r win-x64 --self-contained true -o ".\bin\Release\net8.0\publish"
        
        Pop-Location
        
        Write-Success "Aplicación publicada"
        return $true
    }
    catch {
        Pop-Location
        Write-Error "Error al publicar: $_"
        return $false
    }
}

# Crear reglas de firewall
function Set-FirewallRules {
    param($Config)
    
    Write-Info "Configurando reglas de firewall para acceso en red..."
    
    $httpPort = $Config.Instalacion.PuertoHttp
    $httpsPort = $Config.Instalacion.PuertoHttps
    
    try {
        # Eliminar reglas anteriores de SistemIA
        Get-NetFirewallRule -DisplayName "SistemIA*" -ErrorAction SilentlyContinue | Remove-NetFirewallRule -ErrorAction SilentlyContinue
        
        # Crear regla para HTTP
        New-NetFirewallRule `
            -DisplayName "SistemIA HTTP ($httpPort)" `
            -Description "Permite acceso HTTP al sistema SistemIA desde la red local" `
            -Direction Inbound `
            -Protocol TCP `
            -LocalPort $httpPort `
            -Action Allow `
            -Profile Domain,Private,Public `
            -Enabled True | Out-Null
        
        # Crear regla para HTTPS
        New-NetFirewallRule `
            -DisplayName "SistemIA HTTPS ($httpsPort)" `
            -Description "Permite acceso HTTPS al sistema SistemIA desde la red local" `
            -Direction Inbound `
            -Protocol TCP `
            -LocalPort $httpsPort `
            -Action Allow `
            -Profile Domain,Private,Public `
            -Enabled True | Out-Null
        
        Write-Success "Reglas de firewall creadas para puertos $httpPort (HTTP) y $httpsPort (HTTPS)"
        
        # Mostrar IPs disponibles
        $hostname = $env:COMPUTERNAME
        Write-Host ""
        Write-Host "El sistema será accesible desde otros equipos en:" -ForegroundColor Cyan
        Write-Host "  Por nombre: http://${hostname}:$httpPort | https://${hostname}:$httpsPort" -ForegroundColor Yellow
        
        $ips = Get-NetIPAddress -AddressFamily IPv4 | Where-Object { 
            $_.IPAddress -ne "127.0.0.1" -and 
            $_.PrefixOrigin -ne "WellKnown" -and
            $_.AddressState -eq "Preferred"
        }
        foreach ($ip in $ips) {
            Write-Host "  Por IP:     http://$($ip.IPAddress):$httpPort | https://$($ip.IPAddress):$httpsPort" -ForegroundColor Yellow
        }
        
        return $true
    }
    catch {
        Write-Error "Error al configurar firewall: $_"
        return $false
    }
}

# Limpiar datos del sistema
function Clear-SystemData {
    param($DbConfig)
    
    Write-Title "LIMPIEZA DE DATOS DEL SISTEMA"
    
    # Mostrar la BD que se va a limpiar
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║  BASE DE DATOS A LIMPIAR:                                     ║" -ForegroundColor Red
    Write-Host "║  Servidor: $($DbConfig.Servidor.PadRight(48))║" -ForegroundColor Yellow
    Write-Host "║  Base de datos: $($DbConfig.BaseDatos.PadRight(43))║" -ForegroundColor Yellow
    Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    
    Write-Warning "Esta operación eliminará datos TRANSACCIONALES pero conservará:"
    Write-Host "  - Depósitos"
    Write-Host "  - Listas de precios (estructura)"
    Write-Host "  - Monedas y tipos de cambio"
    Write-Host "  - Tipos de IVA y tipos de pago"
    Write-Host "  - Clasificaciones y marcas"
    Write-Host "  - Proveedor ID 1 y Cliente 'Consumidor Final'"
    Write-Host "  - Usuario administrador"
    Write-Host "  - Certificados SIFEN en Sociedades"
    Write-Host ""
    
    $confirm = Read-Host "¿Está seguro de continuar? Escriba 'CONFIRMAR' para proceder"
    
    if ($confirm -ne "CONFIRMAR") {
        Write-Warning "Operación cancelada"
        return $false
    }
    
    # Buscar el script de limpieza
    $scriptPath = Join-Path $PSScriptRoot "LimpiarDatos.sql"
    if (-not (Test-Path $scriptPath)) {
        Write-Error "No se encontró el script LimpiarDatos.sql en $PSScriptRoot"
        return $false
    }
    
    $connectionString = Build-ConnectionString -DbConfig $DbConfig
    
    try {
        Write-Info "Ejecutando script de limpieza..."
        Write-Info "Base de datos: $($DbConfig.BaseDatos)"
        
        # Leer el script SQL
        $sqlScript = Get-Content $scriptPath -Raw -Encoding UTF8
        
        $connection = New-Object System.Data.SqlClient.SqlConnection
        $connection.ConnectionString = $connectionString
        $connection.Open()
        
        # Ejecutar el script completo usando sqlcmd para mejor manejo de GO
        # Primero intentar con SqlCommand
        $cmd = $connection.CreateCommand()
        $cmd.CommandTimeout = 600  # 10 minutos
        
        # El script tiene GO al final, separar y ejecutar
        $batches = $sqlScript -split '\r?\nGO\r?\n|\r?\nGO$'
        
        $errores = @()
        foreach ($batch in $batches) {
            $batch = $batch.Trim()
            if ($batch -and $batch -notmatch '^\s*$') {
                $cmd = $connection.CreateCommand()
                $cmd.CommandTimeout = 600
                $cmd.CommandText = $batch
                try {
                    # Capturar mensajes informativos de SQL Server
                    $handler = [System.Data.SqlClient.SqlInfoMessageEventHandler] {
                        param($sender, $event)
                        Write-Host $event.Message -ForegroundColor Gray
                    }
                    $connection.add_InfoMessage($handler)
                    $connection.FireInfoMessageEventOnUserErrors = $true
                    
                    $cmd.ExecuteNonQuery() | Out-Null
                    
                    $connection.remove_InfoMessage($handler)
                }
                catch {
                    $errores += $_.Exception.Message
                    Write-Warning "Error SQL: $($_.Exception.Message)"
                }
            }
        }
        
        $connection.Close()
        
        if ($errores.Count -gt 0) {
            Write-Error "Se encontraron errores durante la limpieza"
            foreach ($err in $errores) {
                Write-Host "  - $err" -ForegroundColor Red
            }
            return $false
        }
        
        Write-Success "Datos limpiados correctamente"
        Write-Host ""
        Write-Info "Datos ELIMINADOS:"
        Write-Host "  - Ventas, Compras, Presupuestos"
        Write-Host "  - Productos y movimientos de stock"
        Write-Host "  - Clientes (excepto ID 1) y Proveedores (excepto ID 1)"
        Write-Host "  - Timbrados, Asistencias, Auditoría"
        Write-Host ""
        Write-Info "Datos CONSERVADOS:"
        Write-Host "  - Depósitos, Monedas, Tipos IVA/Pago"
        Write-Host "  - Clasificaciones, Marcas, Listas de precios"
        Write-Host "  - Configuración de Sociedad con certificados SIFEN"
        Write-Host ""
        Write-Info "El sistema está listo para cargar nuevos datos"
        return $true
    }
    catch {
        Write-Error "Error al limpiar datos: $_"
        return $false
    }
}

# Mostrar configuración actual
function Show-CurrentConfig {
    param($Config)
    
    Write-Title "CONFIGURACIÓN ACTUAL"
    
    Write-Host "`n[INSTALACIÓN]" -ForegroundColor Yellow
    Write-Host "  Ruta: $($Config.Instalacion.RutaInstalacion)"
    Write-Host "  Puerto HTTP: $($Config.Instalacion.PuertoHttp)"
    Write-Host "  Puerto HTTPS: $($Config.Instalacion.PuertoHttps)"
    Write-Host "  Inicio automático: $($Config.Instalacion.InicioAutomatico)"
    
    Write-Host "`n[BASE DE DATOS]" -ForegroundColor Yellow
    Write-Host "  Servidor: $($Config.BaseDatos.Servidor)"
    Write-Host "  Base de datos: $($Config.BaseDatos.BaseDatos)"
    Write-Host "  Auth Windows: $($Config.BaseDatos.AutenticacionWindows)"
    if (-not $Config.BaseDatos.AutenticacionWindows) {
        Write-Host "  Usuario: $($Config.BaseDatos.Usuario)"
    }
    
    Write-Host "`n[SOCIEDAD]" -ForegroundColor Yellow
    Write-Host "  Nombre: $($Config.Sociedad.Nombre)"
    Write-Host "  RUC: $($Config.Sociedad.RUC)"
    Write-Host ""
}

# Instalación completa (con rollback automático)
function Install-Complete {
    param($Config, [string]$SourcePath)
    
    Write-Title "INSTALACIÓN COMPLETA DE SISTEMÍA"
    Write-Info "Esta instalación incluye rollback automático en caso de error"
    Write-Host ""
    
    # 1. Configuración interactiva
    $Config = Set-InteractiveConfig -Config $Config
    
    # 2. Guardar configuración
    Save-InstallerConfig -Config $Config -Path $ConfigFile
    
    # 3. Probar conexión al SERVIDOR (no a la BD específica)
    Write-Title "VERIFICANDO CONEXIÓN AL SERVIDOR SQL"
    
    # Primero verificar si podemos conectar al servidor
    $serverConnOk = Test-ServerConnection -DbConfig $Config.BaseDatos
    if (-not $serverConnOk) {
        Write-Error "No se puede conectar al servidor SQL"
        Write-Info "Verifique el servidor y las credenciales"
        return
    }
    
    # === CREAR BACKUP PARA ROLLBACK ===
    Write-Title "PREPARANDO SISTEMA DE ROLLBACK"
    New-PreInstallBackup -DbConfig $Config.BaseDatos
    
    try {
        # Preguntar cómo quiere inicializar la BD
        Write-Title "INICIALIZACIÓN DE BASE DE DATOS"
        Write-Host ""
        Write-Host "Seleccione cómo desea configurar la base de datos:" -ForegroundColor Yellow
        Write-Host "  1. Restaurar desde backup (RECOMENDADO - incluye estructura y datos)" -ForegroundColor White
        Write-Host "  2. Crear BD vacía y estructura desde scripts SQL" -ForegroundColor White
        Write-Host "  3. Usar BD existente (ya tiene estructura)" -ForegroundColor White
        Write-Host ""
        $opcionBD = Read-Host "Opción"
        
        switch ($opcionBD) {
            "1" {
                # Restaurar desde backup - esto crea la BD completa
                if (-not (Restore-DatabaseFromBackup -DbConfig $Config.BaseDatos -InstallerPath $ScriptDir)) {
                    throw "No se pudo restaurar la base de datos"
                }
            }
            "2" {
                # Crear BD vacía y luego estructura
                if (-not (Test-DatabaseConnection -DbConfig $Config.BaseDatos)) {
                    Initialize-Database -DbConfig $Config.BaseDatos
                }
                if (-not (Initialize-DatabaseStructure -DbConfig $Config.BaseDatos -InstallerPath $ScriptDir)) {
                    throw "No se pudo crear la estructura de la base de datos"
                }
            }
            "3" {
                # Verificar que la BD existe
                if (-not (Test-DatabaseConnection -DbConfig $Config.BaseDatos)) {
                    throw "La base de datos no existe"
                }
                Write-Success "Usando base de datos existente"
            }
            default {
                Write-Warning "Opción no válida, usando restauración desde backup"
                if (-not (Restore-DatabaseFromBackup -DbConfig $Config.BaseDatos -InstallerPath $ScriptDir)) {
                    throw "No se pudo restaurar la base de datos"
                }
            }
        }
        
        # 4. Publicar aplicación (o verificar binarios pre-compilados)
        Write-Title "PREPARANDO APLICACIÓN"
        if (-not (Publish-Application -ProjectPath $SourcePath -InstallerPath $ScriptDir)) {
            throw "No se pudo preparar la aplicación"
        }
        
        # 5. Copiar archivos
        Write-Title "COPIANDO ARCHIVOS"
        if (-not (Copy-ApplicationFiles -Config $Config -SourcePath $SourcePath -InstallerPath $ScriptDir)) {
            throw "No se pudieron copiar los archivos"
        }
        # Registrar para rollback
        $script:RollbackState.ArchivosCopiados += $Config.Instalacion.RutaInstalacion
        
        # 6. Generar appsettings
        New-AppSettings -Config $Config -DestPath $Config.Instalacion.RutaInstalacion
        
        # 7. Instalar certificado HTTPS
        Write-Title "CONFIGURANDO CERTIFICADO HTTPS"
        $usarMkcert = if ($Config.Instalacion.UsarMkcert -eq $false) { $false } else { $true }
        Install-HttpsCertificate -InstallPath $Config.Instalacion.RutaInstalacion -UsarMkcert $usarMkcert
        
        # 8. Configurar firewall para acceso en red
        Write-Title "CONFIGURANDO FIREWALL"
        if (Set-FirewallRules -Config $Config) {
            $script:RollbackState.FirewallCreado = $true
        }
        
        # 9. Instalar servicio
        Write-Title "INSTALANDO SERVICIO WINDOWS"
        if (Install-SistemIAService -Config $Config) {
            $script:RollbackState.ServicioCreado = $true
        }
        
        # 10. Crear acceso directo en escritorio (si está habilitado)
        if ($Config.Instalacion.CrearAccesoDirecto -ne $false) {
            Write-Title "CREANDO ACCESO DIRECTO"
            New-ServerShortcut -Config $Config
        }
        
        # === INSTALACIÓN EXITOSA - LIMPIAR BACKUP DE ROLLBACK ===
        Remove-RollbackBackup
        
        Write-Title "INSTALACIÓN COMPLETADA"
        Write-Success "SistemIA se ha instalado correctamente"
        Write-Host ""
        Write-Host "Acceda al sistema en:" -ForegroundColor White
        Write-Host "  HTTP:  http://localhost:$($Config.Instalacion.PuertoHttp)" -ForegroundColor Green
        Write-Host "  HTTPS: https://localhost:$($Config.Instalacion.PuertoHttps)" -ForegroundColor Green
        Write-Host ""
        if ($Config.Instalacion.CrearAccesoDirecto -ne $false) {
            Write-Host "Acceso directo creado en el escritorio" -ForegroundColor Cyan
            Write-Host ""
        }
        Write-Host "Credenciales iniciales:" -ForegroundColor Yellow
        Write-Host "  Usuario: admin" -ForegroundColor White
        Write-Host "  Contraseña: admin" -ForegroundColor White
        Write-Host ""
        Write-Warning "IMPORTANTE: Cambie la contraseña después del primer inicio de sesión"
        Write-Host ""
    }
    catch {
        # === ERROR DETECTADO - EJECUTAR ROLLBACK ===
        Write-Host ""
        Write-Error "ERROR DURANTE LA INSTALACIÓN: $_"
        Write-Host ""
        
        $ejecutarRollback = Read-Host "¿Desea revertir los cambios realizados? (S/N) [S]"
        if ($ejecutarRollback -ne 'N' -and $ejecutarRollback -ne 'n') {
            Invoke-Rollback -DbConfig $Config.BaseDatos -Config $Config
        }
        else {
            Write-Warning "Rollback cancelado. El sistema puede estar en un estado inconsistente."
            Write-Info "Backup de seguridad disponible en: $($script:RollbackState.BackupPath)"
        }
    }
}

# === MAIN ===
Show-Banner

if (-not (Test-Administrator)) {
    Write-Error "Este script debe ejecutarse como Administrador"
    Write-Host "Haga clic derecho en PowerShell y seleccione 'Ejecutar como administrador'"
    Read-Host "Presione Enter para salir"
    exit 1
}

# Usar $ScriptDir ya definido al inicio del script
$scriptPath = $ScriptDir
$projectPath = Split-Path -Parent $scriptPath

# Cargar configuración
$config = Get-InstallerConfig -Path $ConfigFile
if (-not $config) {
    Write-Error "No se pudo cargar la configuración"
    Write-Info "Ruta buscada: $ConfigFile"
    Read-Host "Presione Enter para salir"
    exit 1
}

# Menú principal
do {
    Show-Menu
    $opcion = Read-Host "Seleccione una opción"
    
    switch ($opcion) {
        "1" { Install-Complete -Config $config -SourcePath $projectPath }
        "2" { 
            # Instalación Cliente
            Write-Title "INSTALACIÓN MODO CLIENTE"
            $config.Instalacion.ModoInstalacion = "Cliente"
            
            Write-Host ""
            $defaultRemoto = if ($config.Cliente.ServidorRemoto) { $config.Cliente.ServidorRemoto } else { "192.168.1.100" }
            $servidorRemoto = Read-Host "IP o nombre del servidor SistemIA [$defaultRemoto]"
            if ([string]::IsNullOrWhiteSpace($servidorRemoto)) { $servidorRemoto = $defaultRemoto }
            $config.Cliente.ServidorRemoto = $servidorRemoto
            
            $defaultPuerto = if ($config.Cliente.PuertoRemoto) { $config.Cliente.PuertoRemoto } else { 5095 }
            $puertoRemoto = Read-Host "Puerto del servidor [$defaultPuerto]"
            if ([string]::IsNullOrWhiteSpace($puertoRemoto)) { $puertoRemoto = $defaultPuerto }
            $config.Cliente.PuertoRemoto = [int]$puertoRemoto
            
            $usarHttps = Read-Host "¿Usar HTTPS? (S/N) [N]"
            $config.Cliente.UsarHttps = ($usarHttps -eq 'S' -or $usarHttps -eq 's')
            
            # Guardar configuración
            Save-InstallerConfig -Config $config -Path $ConfigFile
            
            # Crear acceso directo
            New-ClientShortcut -Config $config
            
            Write-Title "INSTALACIÓN CLIENTE COMPLETADA"
            $protocol = if ($config.Cliente.UsarHttps) { "https" } else { "http" }
            $url = "$protocol`://$($config.Cliente.ServidorRemoto):$($config.Cliente.PuertoRemoto)"
            Write-Success "Acceso directo creado en el escritorio"
            Write-Info "URL del servidor: $url"
        }
        "3" { 
            $config = Set-InteractiveConfig -Config $config
            Save-InstallerConfig -Config $config -Path $ConfigFile
            New-AppSettings -Config $config -DestPath $projectPath
        }
        "4" { Install-SistemIAService -Config $config }
        "5" { Uninstall-SistemIAService -Config $config }
        "6" { 
            # Crear/Restaurar base de datos
            Write-Title "CREAR/RESTAURAR BASE DE DATOS"
            Write-Host ""
            Write-Host "Esta opción permite:" -ForegroundColor Yellow
            Write-Host "  - Restaurar la base de datos desde un backup limpio" -ForegroundColor White
            Write-Host "  - O crear la estructura desde scripts SQL" -ForegroundColor White
            Write-Host ""
            Write-Warning "ADVERTENCIA: Esto eliminará la base de datos actual si existe"
            Write-Host ""
            $confirmar = Read-Host "¿Desea continuar? (S/N)"
            if ($confirmar -match '^[sS]$') {
                # Crear BD vacía si no existe (para poder restaurar)
                $testConn = Test-DatabaseConnection -DbConfig $config.BaseDatos
                
                # Restaurar/Inicializar estructura
                Initialize-DatabaseStructure -DbConfig $config.BaseDatos -InstallerPath $scriptPath
            }
        }
        "7" { Clear-SystemData -DbConfig $config.BaseDatos }
        "8" { Show-CurrentConfig -Config $config }
        "9" { Test-DatabaseConnection -DbConfig $config.BaseDatos }
        "10" { Test-ServiceStatus -Config $config }
        "11" { 
            # Instalar certificado con mkcert
            $usarMkcert = $true
            if ($config.Instalacion.UsarMkcert -eq $false) {
                $usarMkcert = $false
            }
            Install-HttpsCertificate -InstallPath $config.Instalacion.RutaInstalacion -UsarMkcert $usarMkcert
        }
        "12" { Configure-Firewall -HttpPort $config.Instalacion.PuertoHttp -HttpsPort $config.Instalacion.PuertoHttps }
        "13" { Show-NetworkAccess -HttpPort $config.Instalacion.PuertoHttp -HttpsPort $config.Instalacion.PuertoHttps }
        "14" { 
            # Crear acceso directo manualmente
            if ($config.Instalacion.ModoInstalacion -eq "Cliente") {
                New-ClientShortcut -Config $config
            }
            else {
                New-ServerShortcut -Config $config
            }
        }
        "0" { Write-Host "`nSaliendo..." -ForegroundColor Yellow }
        default { Write-Warning "Opción no válida" }
    }
    
    if ($opcion -ne "0") {
        Write-Host ""
        Read-Host "Presione Enter para continuar"
    }
    
} while ($opcion -ne "0")

Write-Host "`nGracias por usar SistemIA" -ForegroundColor Cyan

