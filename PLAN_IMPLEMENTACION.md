# Plan de Implementación - SistemIA

## 📋 Índice
1. [Requisitos del Cliente](#requisitos-del-cliente)
2. [Instalación Inicial](#instalación-inicial)
3. [Configuración del Servidor](#configuración-del-servidor)
4. [Inicio del Servidor](#inicio-del-servidor)
5. [Actualización de la Aplicación](#actualización-de-la-aplicación)
6. [Actualización de la Base de Datos](#actualización-de-la-base-de-datos)
7. [Scripts de Automatización](#scripts-de-automatización)
8. [Mantenimiento y Monitoreo](#mantenimiento-y-monitoreo)

---

## 🖥️ Requisitos del Cliente

### Software Necesario
- **Windows Server 2019/2022** o Windows 10/11 Pro
- **SQL Server 2019 Express o superior** (2022 recomendado)
- **.NET 8.0 Runtime** (ASP.NET Core)
- **IIS 10** (opcional, para producción)

### Hardware Recomendado
- **CPU**: 4 cores mínimo
- **RAM**: 8 GB mínimo (16 GB recomendado)
- **Disco**: 100 GB SSD
- **Red**: 100 Mbps mínimo

---

## 📦 Instalación Inicial

### 1. Instalar .NET 8.0 Runtime

```powershell
# Descargar e instalar desde:
# https://dotnet.microsoft.com/download/dotnet/8.0

# Verificar instalación
dotnet --version
# Debe mostrar: 8.0.x
```

### 2. Instalar SQL Server

```powershell
# SQL Server 2022 Express
# https://www.microsoft.com/sql-server/sql-server-downloads

# Configuración durante instalación:
# - Modo de autenticación: Mixto (Windows + SQL)
# - Usuario sa: [Configurar contraseña segura]
# - Habilitar TCP/IP en SQL Server Configuration Manager
```

### 3. Copiar Aplicación

```powershell
# Crear directorio de aplicación
New-Item -Path "C:\Apps\SistemIA" -ItemType Directory

# Copiar archivos de publicación
# (Los archivos vienen de tu servidor de desarrollo)
```

### 4. Crear Base de Datos Inicial

```sql
-- Ejecutar en SQL Server Management Studio
CREATE DATABASE SistemIA
GO

USE SistemIA
GO

-- Crear usuario para la aplicación
CREATE LOGIN sistemiauser WITH PASSWORD = '[ContraseñaSegura123!]'
GO

USE SistemIA
GO

CREATE USER sistemiauser FOR LOGIN sistemiauser
GO

-- Dar permisos
ALTER ROLE db_owner ADD MEMBER sistemiauser
GO
```

### 5. Configurar Connection String

Editar `C:\Apps\SistemIA\appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SistemIA;User Id=sistemiauser;Password=[ContraseñaSegura123!];TrustServerCertificate=True;MultipleActiveResultSets=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## ⚙️ Configuración del Servidor

### Opción 1: Ejecutar como Servicio de Windows (Recomendado)

#### Crear el Servicio

```powershell
# Instalar herramienta NSSM (Non-Sucking Service Manager)
# Descargar desde: https://nssm.cc/download

# Instalar servicio
nssm install SistemIA "C:\Apps\SistemIA\SistemIA.exe"

# Configurar parámetros
nssm set SistemIA AppDirectory "C:\Apps\SistemIA"
nssm set SistemIA AppEnvironmentExtra ASPNETCORE_ENVIRONMENT=Production
nssm set SistemIA AppEnvironmentExtra ASPNETCORE_URLS=http://0.0.0.0:5095;https://0.0.0.0:7060
nssm set SistemIA DisplayName "SistemIA - Sistema de Gestión"
nssm set SistemIA Description "Sistema integral de gestión empresarial"
nssm set SistemIA Start SERVICE_AUTO_START

# Iniciar servicio
nssm start SistemIA

# Verificar estado
nssm status SistemIA
```

#### Comandos de Gestión del Servicio

```powershell
# Iniciar
Start-Service SistemIA

# Detener
Stop-Service SistemIA

# Reiniciar
Restart-Service SistemIA

# Ver estado
Get-Service SistemIA

# Ver logs
Get-EventLog -LogName Application -Source SistemIA -Newest 50
```

### Opción 2: Ejecutar con IIS

#### Configurar IIS

```powershell
# Instalar IIS y ASP.NET Core Module
Install-WindowsFeature -Name Web-Server -IncludeManagementTools
Install-WindowsFeature -Name Web-Asp-Net45

# Instalar ASP.NET Core Hosting Bundle
# https://dotnet.microsoft.com/download/dotnet/8.0
```

#### Crear Sitio en IIS

```powershell
# Crear Application Pool
New-WebAppPool -Name "SistemIA"
Set-ItemProperty IIS:\AppPools\SistemIA -Name "managedRuntimeVersion" -Value ""

# Crear sitio web
New-Website -Name "SistemIA" `
    -PhysicalPath "C:\Apps\SistemIA" `
    -ApplicationPool "SistemIA" `
    -Port 5095

# Configurar permisos
icacls "C:\Apps\SistemIA" /grant "IIS AppPool\SistemIA:(OI)(CI)F" /T
```

---

## 🚀 Inicio del Servidor

### Método Manual (Desarrollo/Pruebas)

```powershell
# Navegar al directorio
cd C:\Apps\SistemIA

# Iniciar aplicación
.\SistemIA.exe
# O
dotnet SistemIA.dll

# Con URLs específicas
$env:ASPNETCORE_URLS="http://0.0.0.0:5095;https://0.0.0.0:7060"
.\SistemIA.exe
```

### Verificar que Está Corriendo

```powershell
# Verificar proceso
Get-Process | Where-Object {$_.ProcessName -like "*SistemIA*"}

# Verificar puertos
netstat -ano | findstr ":5095"
netstat -ano | findstr ":7060"

# Probar en navegador
Start-Process "http://localhost:5095"
```

---

## 🔄 Actualización de la Aplicación

### Proceso de Actualización Completo

#### 1. En Tu Servidor de Desarrollo

```powershell
# Compilar en modo Release
cd C:\asis\SistemIA
dotnet publish -c Release -o publish

# Crear paquete de actualización
$fecha = Get-Date -Format "yyyyMMdd_HHmm"
Compress-Archive -Path .\publish\* -DestinationPath "SistemIA_Update_$fecha.zip"
```

#### 2. Script de Actualización en Cliente

Crear `C:\Apps\SistemIA\Scripts\actualizar.ps1`:

```powershell
# Script de Actualización Automática
param(
    [Parameter(Mandatory=$true)]
    [string]$ArchivoZip
)

$ErrorActionPreference = "Stop"
$AppPath = "C:\Apps\SistemIA"
$BackupPath = "C:\Apps\SistemIA_Backups"
$TempPath = "C:\Temp\SistemIA_Update"

try {
    Write-Host "=== ACTUALIZACIÓN DE SistemIA ===" -ForegroundColor Cyan
    
    # 1. Verificar archivo existe
    if (-not (Test-Path $ArchivoZip)) {
        throw "Archivo no encontrado: $ArchivoZip"
    }
    
    # 2. Detener servicio
    Write-Host "`n[1/7] Deteniendo servicio..." -ForegroundColor Yellow
    Stop-Service SistemIA -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 3
    
    # Verificar que se detuvo
    $proceso = Get-Process -Name "SistemIA" -ErrorAction SilentlyContinue
    if ($proceso) {
        Write-Host "Forzando cierre de proceso..." -ForegroundColor Yellow
        Stop-Process -Name "SistemIA" -Force
        Start-Sleep -Seconds 2
    }
    
    # 3. Crear backup
    Write-Host "`n[2/7] Creando backup..." -ForegroundColor Yellow
    $fecha = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupDir = Join-Path $BackupPath "Backup_$fecha"
    New-Item -Path $backupDir -ItemType Directory -Force | Out-Null
    
    # Copiar archivos actuales (excepto logs y appsettings.json)
    Copy-Item -Path "$AppPath\*" -Destination $backupDir -Recurse -Force -Exclude @("logs", "*.log", "appsettings.json", "appsettings.Production.json")
    Write-Host "Backup creado en: $backupDir" -ForegroundColor Green
    
    # 4. Guardar configuración actual
    Write-Host "`n[3/7] Guardando configuración..." -ForegroundColor Yellow
    $configBackup = Join-Path $TempPath "config_backup"
    New-Item -Path $configBackup -ItemType Directory -Force | Out-Null
    Copy-Item -Path "$AppPath\appsettings.json" -Destination $configBackup -Force -ErrorAction SilentlyContinue
    Copy-Item -Path "$AppPath\appsettings.Production.json" -Destination $configBackup -Force -ErrorAction SilentlyContinue
    
    # 5. Extraer nueva versión
    Write-Host "`n[4/7] Extrayendo actualización..." -ForegroundColor Yellow
    New-Item -Path $TempPath -ItemType Directory -Force | Out-Null
    Expand-Archive -Path $ArchivoZip -DestinationPath $TempPath -Force
    
    # 6. Copiar archivos nuevos
    Write-Host "`n[5/7] Copiando archivos nuevos..." -ForegroundColor Yellow
    Copy-Item -Path "$TempPath\*" -Destination $AppPath -Recurse -Force
    
    # 7. Restaurar configuración
    Write-Host "`n[6/7] Restaurando configuración..." -ForegroundColor Yellow
    if (Test-Path "$configBackup\appsettings.json") {
        Copy-Item -Path "$configBackup\appsettings.json" -Destination $AppPath -Force
    }
    if (Test-Path "$configBackup\appsettings.Production.json") {
        Copy-Item -Path "$configBackup\appsettings.Production.json" -Destination $AppPath -Force
    }
    
    # 8. Limpiar temporales
    Remove-Item -Path $TempPath -Recurse -Force -ErrorAction SilentlyContinue
    
    # 9. Iniciar servicio
    Write-Host "`n[7/7] Iniciando servicio..." -ForegroundColor Yellow
    Start-Service SistemIA
    Start-Sleep -Seconds 5
    
    # Verificar que inició correctamente
    $servicio = Get-Service SistemIA
    if ($servicio.Status -eq "Running") {
        Write-Host "`n✅ ACTUALIZACIÓN COMPLETADA EXITOSAMENTE" -ForegroundColor Green
        Write-Host "La aplicación está funcionando correctamente.`n" -ForegroundColor Green
    } else {
        throw "El servicio no inició correctamente. Estado: $($servicio.Status)"
    }
    
} catch {
    Write-Host "`n❌ ERROR EN ACTUALIZACIÓN: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`nRestaurando desde backup..." -ForegroundColor Yellow
    
    # Intentar restaurar backup más reciente
    $ultimoBackup = Get-ChildItem -Path $BackupPath -Directory | Sort-Object Name -Descending | Select-Object -First 1
    if ($ultimoBackup) {
        Copy-Item -Path "$($ultimoBackup.FullName)\*" -Destination $AppPath -Recurse -Force
        Start-Service SistemIA -ErrorAction SilentlyContinue
        Write-Host "Backup restaurado. Verificar manualmente el servicio." -ForegroundColor Yellow
    }
    
    exit 1
}
```

#### 3. Ejecutar Actualización

```powershell
# Copiar archivo ZIP al servidor del cliente
# Ejemplo: SistemIA_Update_20251215_1430.zip

# Ejecutar script de actualización
cd C:\Apps\SistemIA\Scripts
.\actualizar.ps1 -ArchivoZip "C:\Temp\SistemIA_Update_20251215_1430.zip"
```

---

## 🗄️ Actualización de la Base de Datos

### Aplicar Migraciones de Entity Framework

#### Método 1: Automático (Recomendado para Producción)

La aplicación aplica migraciones automáticamente al iniciar (configurado en `Program.cs`):

```csharp
// Ya está implementado en Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // Aplica migraciones pendientes
}
```

**✅ Ventaja**: No requiere intervención manual  
**⚠️ Precaución**: Siempre hacer backup antes de actualizar

#### Método 2: Manual con EF Core Tools

```powershell
# Instalar herramientas de EF Core (una sola vez)
dotnet tool install --global dotnet-ef

# Verificar instalación
dotnet ef --version

# En el directorio de la aplicación
cd C:\Apps\SistemIA

# Ver migraciones pendientes
dotnet ef migrations list --no-build

# Aplicar migraciones
dotnet ef database update --no-build

# Ver script SQL que se ejecutará (sin aplicar)
dotnet ef migrations script --no-build -o migracion.sql
```

#### Método 3: Script SQL Manual

```powershell
# Generar script SQL desde tu servidor de desarrollo
cd C:\asis\SistemIA

# Generar script completo desde inicio
dotnet ef migrations script -o Scripts\migracion_completa.sql

# O generar script incremental desde una migración específica
dotnet ef migrations script NombreMigracionAnterior -o Scripts\migracion_incremental.sql

# Copiar el script al servidor del cliente y ejecutarlo en SSMS
```

### Script de Backup Antes de Migrar

Crear `C:\Apps\SistemIA\Scripts\backup_bd.ps1`:

```powershell
# Script de Backup de Base de Datos
param(
    [string]$ServerInstance = "localhost",
    [string]$Database = "SistemIA",
    [string]$BackupPath = "C:\Backups\SistemIA"
)

$ErrorActionPreference = "Stop"

try {
    # Crear directorio de backup
    if (-not (Test-Path $BackupPath)) {
        New-Item -Path $BackupPath -ItemType Directory -Force | Out-Null
    }
    
    # Nombre del archivo de backup
    $fecha = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFile = Join-Path $BackupPath "SistemIA_$fecha.bak"
    
    Write-Host "Creando backup de la base de datos..." -ForegroundColor Yellow
    
    # Comando SQL de backup
    $sql = @"
BACKUP DATABASE [$Database] 
TO DISK = N'$backupFile' 
WITH NOFORMAT, 
     NOINIT,  
     NAME = N'SistemIA-Full Database Backup', 
     SKIP, 
     NOREWIND, 
     NOUNLOAD,  
     COMPRESSION,
     STATS = 10
"@
    
    # Ejecutar backup
    Invoke-Sqlcmd -ServerInstance $ServerInstance -Query $sql -QueryTimeout 600
    
    Write-Host "✅ Backup creado exitosamente: $backupFile" -ForegroundColor Green
    
    # Limpiar backups antiguos (mantener últimos 7 días)
    Get-ChildItem -Path $BackupPath -Filter "*.bak" | 
        Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-7) } | 
        Remove-Item -Force
    
} catch {
    Write-Host "❌ Error al crear backup: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
```

### Proceso Completo de Actualización con BD

Crear `C:\Apps\SistemIA\Scripts\actualizar_completo.ps1`:

```powershell
# Actualización Completa: Aplicación + Base de Datos
param(
    [Parameter(Mandatory=$true)]
    [string]$ArchivoZip
)

$ErrorActionPreference = "Stop"

try {
    Write-Host "=== ACTUALIZACIÓN COMPLETA DE SistemIA ===" -ForegroundColor Cyan
    
    # 1. Backup de base de datos
    Write-Host "`n[1/3] Creando backup de base de datos..." -ForegroundColor Yellow
    .\backup_bd.ps1
    if ($LASTEXITCODE -ne 0) {
        throw "Error al crear backup de BD"
    }
    
    # 2. Actualizar aplicación
    Write-Host "`n[2/3] Actualizando aplicación..." -ForegroundColor Yellow
    .\actualizar.ps1 -ArchivoZip $ArchivoZip
    if ($LASTEXITCODE -ne 0) {
        throw "Error al actualizar aplicación"
    }
    
    # 3. Migraciones se aplican automáticamente al iniciar la app
    Write-Host "`n[3/3] Esperando aplicación de migraciones..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    
    # Verificar logs para confirmar migraciones
    $logFile = Get-ChildItem "C:\Apps\SistemIA\logs" -Filter "*.log" | 
               Sort-Object LastWriteTime -Descending | 
               Select-Object -First 1
    
    if ($logFile) {
        $contenido = Get-Content $logFile.FullName -Tail 20
        if ($contenido -match "Applied migration") {
            Write-Host "✅ Migraciones aplicadas correctamente" -ForegroundColor Green
        }
    }
    
    Write-Host "`n✅ ACTUALIZACIÓN COMPLETA FINALIZADA" -ForegroundColor Green
    Write-Host "Verificar funcionamiento en: http://localhost:5095`n" -ForegroundColor Cyan
    
} catch {
    Write-Host "`n❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Revisar logs en C:\Apps\SistemIA\logs`n" -ForegroundColor Yellow
    exit 1
}
```

---

## 🤖 Scripts de Automatización

### Script de Inicio Rápido

Crear `C:\Apps\SistemIA\inicio_rapido.bat`:

```batch
@echo off
echo ============================================
echo  SistemIA - Inicio Rapido
echo ============================================
echo.

:: Verificar si el servicio existe
sc query SistemIA >nul 2>&1
if %errorlevel% equ 0 (
    echo Iniciando servicio SistemIA...
    net start SistemIA
    if %errorlevel% equ 0 (
        echo.
        echo ✓ Servicio iniciado correctamente
        echo.
        echo Abriendo navegador...
        timeout /t 3 >nul
        start http://localhost:5095
    ) else (
        echo.
        echo × Error al iniciar el servicio
        pause
    )
) else (
    echo Servicio no encontrado. Iniciando aplicacion directamente...
    cd /d C:\Apps\SistemIA
    start SistemIA.exe
    echo.
    echo ✓ Aplicacion iniciada
    echo.
    echo Abriendo navegador en 5 segundos...
    timeout /t 5 >nul
    start http://localhost:5095
)

pause
```

### Script de Detención

Crear `C:\Apps\SistemIA\detener.bat`:

```batch
@echo off
echo Deteniendo SistemIA...
net stop SistemIA
echo.
echo Servicio detenido
pause
```

### Script de Reinicio

Crear `C:\Apps\SistemIA\reiniciar.bat`:

```batch
@echo off
echo ============================================
echo  SistemIA - Reinicio
echo ============================================
echo.
echo Deteniendo servicio...
net stop SistemIA
timeout /t 3 >nul
echo.
echo Iniciando servicio...
net start SistemIA
echo.
echo ✓ Servicio reiniciado
echo.
echo Abriendo navegador...
timeout /t 3 >nul
start http://localhost:5095
pause
```

---

## 🔍 Mantenimiento y Monitoreo

### Verificar Estado del Sistema

```powershell
# Script: verificar_estado.ps1
Write-Host "=== ESTADO DE SistemIA ===" -ForegroundColor Cyan

# 1. Estado del servicio
$servicio = Get-Service SistemIA -ErrorAction SilentlyContinue
if ($servicio) {
    Write-Host "`n[Servicio]" -ForegroundColor Yellow
    Write-Host "  Estado: $($servicio.Status)" -ForegroundColor $(if ($servicio.Status -eq "Running") {"Green"} else {"Red"})
    Write-Host "  Tipo inicio: $($servicio.StartType)"
} else {
    Write-Host "`n[Servicio] No instalado" -ForegroundColor Red
}

# 2. Procesos activos
$procesos = Get-Process -Name "SistemIA" -ErrorAction SilentlyContinue
if ($procesos) {
    Write-Host "`n[Procesos]" -ForegroundColor Yellow
    $procesos | Format-Table Id, ProcessName, CPU, WorkingSet -AutoSize
}

# 3. Puertos
Write-Host "`n[Puertos]" -ForegroundColor Yellow
netstat -ano | Select-String ":5095|:7060"

# 4. Uso de disco
Write-Host "`n[Espacio en Disco]" -ForegroundColor Yellow
$appSize = (Get-ChildItem -Path "C:\Apps\SistemIA" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "  Tamaño aplicación: $([math]::Round($appSize, 2)) MB"

$backupSize = (Get-ChildItem -Path "C:\Apps\SistemIA_Backups" -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "  Tamaño backups: $([math]::Round($backupSize, 2)) MB"

# 5. Último log
Write-Host "`n[Últimas líneas del log]" -ForegroundColor Yellow
$ultimoLog = Get-ChildItem "C:\Apps\SistemIA\logs" -Filter "*.log" -ErrorAction SilentlyContinue | 
             Sort-Object LastWriteTime -Descending | 
             Select-Object -First 1
if ($ultimoLog) {
    Get-Content $ultimoLog.FullName -Tail 10
}

# 6. Conectividad BD
Write-Host "`n[Base de Datos]" -ForegroundColor Yellow
try {
    $connString = (Get-Content "C:\Apps\SistemIA\appsettings.json" | ConvertFrom-Json).ConnectionStrings.DefaultConnection
    Write-Host "  Connection String configurado: ✓" -ForegroundColor Green
    
    # Probar conexión (requiere módulo SqlServer)
    if (Get-Module -ListAvailable -Name SqlServer) {
        Test-DbaConnection -SqlInstance localhost -Database SistemIA -ErrorAction Stop | Out-Null
        Write-Host "  Conexión a BD: ✓" -ForegroundColor Green
    }
} catch {
    Write-Host "  Error verificando BD: $($_.Exception.Message)" -ForegroundColor Red
}
```

### Limpieza de Logs

```powershell
# Script: limpiar_logs.ps1
param(
    [int]$DiasAMantener = 30
)

$logsPath = "C:\Apps\SistemIA\logs"
$fecha = (Get-Date).AddDays(-$DiasAMantener)

Write-Host "Limpiando logs anteriores a: $($fecha.ToString('yyyy-MM-dd'))" -ForegroundColor Yellow

$archivos = Get-ChildItem -Path $logsPath -Filter "*.log" | Where-Object { $_.LastWriteTime -lt $fecha }

if ($archivos) {
    $archivos | ForEach-Object {
        Write-Host "Eliminando: $($_.Name)" -ForegroundColor Gray
        Remove-Item $_.FullName -Force
    }
    Write-Host "`n✓ Limpieza completada. Eliminados: $($archivos.Count) archivos" -ForegroundColor Green
} else {
    Write-Host "No hay logs antiguos para eliminar" -ForegroundColor Green
}
```

### Tarea Programada de Backup

```powershell
# Crear tarea programada para backup diario
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-File C:\Apps\SistemIA\Scripts\backup_bd.ps1"
$trigger = New-ScheduledTaskTrigger -Daily -At 2:00AM
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -RunOnlyIfNetworkAvailable

Register-ScheduledTask -TaskName "SistemIA - Backup Diario" `
    -Action $action `
    -Trigger $trigger `
    -Principal $principal `
    -Settings $settings `
    -Description "Backup automático diario de la base de datos SistemIA"

Write-Host "✓ Tarea programada creada: Backup diario a las 2:00 AM" -ForegroundColor Green
```

---

## 📊 Checklist de Implementación

### Pre-instalación
- [ ] Verificar requisitos de hardware
- [ ] Instalar .NET 8.0 Runtime
- [ ] Instalar SQL Server
- [ ] Configurar firewall (puertos 5095, 7060, 1433)
- [ ] Crear usuario de Windows para el servicio (opcional)

### Instalación
- [ ] Copiar archivos de aplicación
- [ ] Crear base de datos
- [ ] Configurar connection string
- [ ] Configurar appsettings.json
- [ ] Aplicar migraciones iniciales
- [ ] Crear servicio de Windows
- [ ] Configurar inicio automático

### Post-instalación
- [ ] Verificar que el servicio inicia correctamente
- [ ] Probar acceso desde navegador
- [ ] Crear usuarios iniciales
- [ ] Configurar datos maestros (productos, clientes, etc.)
- [ ] Configurar backup automático
- [ ] Documentar credenciales y configuración
- [ ] Capacitar a usuarios finales

### Actualización
- [ ] Crear backup de BD
- [ ] Crear backup de aplicación
- [ ] Detener servicio
- [ ] Copiar nuevos archivos
- [ ] Restaurar configuración
- [ ] Iniciar servicio
- [ ] Verificar migraciones aplicadas
- [ ] Probar funcionalidad

---

## 🆘 Solución de Problemas

### El servicio no inicia

```powershell
# Ver logs del sistema
Get-EventLog -LogName Application -Source SistemIA -Newest 20

# Ver logs de la aplicación
Get-Content "C:\Apps\SistemIA\logs\*.log" -Tail 50

# Verificar permisos
icacls "C:\Apps\SistemIA"

# Probar inicio manual
cd C:\Apps\SistemIA
.\SistemIA.exe
```

### Error de conexión a base de datos

```powershell
# Verificar SQL Server corriendo
Get-Service MSSQLSERVER

# Probar conexión
sqlcmd -S localhost -U sistemiauser -P [Contraseña] -Q "SELECT @@VERSION"

# Verificar TCP/IP habilitado
# SQL Server Configuration Manager > SQL Server Network Configuration > Protocols
```

### Puerto ya en uso

```powershell
# Ver qué está usando el puerto
netstat -ano | findstr ":5095"

# Matar proceso
taskkill /PID [PID] /F

# Cambiar puerto en appsettings.json o variable de entorno
```

---

## 📞 Contacto y Soporte

Para soporte técnico o consultas sobre actualizaciones:
- **Email**: soporte@sistemiacorp.com
- **Teléfono**: +595 21 XXX-XXXX
- **Horario**: Lunes a Viernes, 8:00 - 18:00

---

**Última actualización**: 15 de diciembre de 2025  
**Versión del documento**: 1.0
