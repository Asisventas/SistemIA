# Módulo de Actualización de SistemIA

## 📋 Descripción

Este módulo proporciona una solución completa y segura para actualizar tanto la aplicación como la base de datos de SistemIA, incluyendo:

- ✅ Interfaz web amigable para actualizar desde el navegador
- ✅ Script PowerShell automatizado para actualización desde el servidor
- ✅ Backups automáticos antes de cada actualización
- ✅ Rollback automático en caso de error
- ✅ Aplicación automática de migraciones de base de datos
- ✅ Validaciones exhaustivas en cada paso
- ✅ Logs detallados del proceso
- ✅ Gestión de backups (listado y limpieza)

## 🌐 Interfaz Web

### Acceso

Navega a: **Configuración → Actualización del Sistema**

O directamente: `https://tuservidor:7060/actualizacion-sistema`

### Características

1. **Visualización de Versión Actual**
   - Número de versión
   - Fecha de compilación
   - Entorno (Production/Development)

2. **Proceso de Actualización**
   - Subida de archivo ZIP
   - Barra de progreso en tiempo real
   - Logs detallados del proceso
   - Resumen de resultados

3. **Gestión de Backups**
   - Lista completa de backups disponibles
   - Información de tamaño y fecha
   - Limpieza de backups antiguos

### Cómo Usar

1. Prepara el archivo ZIP con la actualización (ver sección "Preparar Actualización")
2. Accede al módulo de actualización
3. Selecciona el archivo ZIP
4. Marca la opción "Aplicar migraciones de BD" si hay cambios en el esquema
5. Haz clic en "Iniciar Actualización"
6. Espera a que complete (NO CIERRES EL NAVEGADOR)
7. Si es exitoso, reinicia la aplicación cuando se indique

**⚠️ IMPORTANTE:** El proceso puede tomar varios minutos. NO interrumpas.

## 💻 Script PowerShell

### Ubicación

```
C:\Apps\SistemIA\Scripts\ActualizarSistemIA.ps1
```

### Uso Básico

```powershell
.\ActualizarSistemIA.ps1 -ArchivoZip "C:\Temp\SistemIA_Update_20251215.zip"
```

### Parámetros

| Parámetro | Tipo | Descripción | Obligatorio |
|-----------|------|-------------|-------------|
| `-ArchivoZip` | String | Ruta completa al archivo ZIP de actualización | ✅ Sí |
| `-NoPararServicio` | Switch | No detener el servicio antes de actualizar | ❌ No |
| `-NoBackup` | Switch | Omitir creación de backup (NO RECOMENDADO) | ❌ No |
| `-NoMigraciones` | Switch | No aplicar migraciones de base de datos | ❌ No |
| `-AppPath` | String | Ruta de instalación (default: C:\Apps\SistemIA) | ❌ No |
| `-BackupPath` | String | Ruta de backups (default: C:\Backups\SistemIA) | ❌ No |
| `-ServiceName` | String | Nombre del servicio (default: SistemIA) | ❌ No |

### Ejemplos

**Actualización completa con todos los pasos:**
```powershell
.\ActualizarSistemIA.ps1 -ArchivoZip "C:\Temp\Update.zip"
```

**Actualización sin migraciones (solo archivos):**
```powershell
.\ActualizarSistemIA.ps1 -ArchivoZip "C:\Temp\Update.zip" -NoMigraciones
```

**Actualización en instalación personalizada:**
```powershell
.\ActualizarSistemIA.ps1 `
    -ArchivoZip "D:\Updates\SistemIA_v2.0.zip" `
    -AppPath "D:\Aplicaciones\SistemIA" `
    -BackupPath "D:\Backups" `
    -ServiceName "SistemIA_Prod"
```

**Solo actualizar archivos (sin parar servicio - útil para desarrollo):**
```powershell
.\ActualizarSistemIA.ps1 `
    -ArchivoZip "C:\Temp\Update.zip" `
    -NoPararServicio `
    -NoBackup `
    -NoMigraciones
```

### Requisitos

- ✅ Ejecutar PowerShell como **Administrador**
- ✅ Tener `.NET 8.0 SDK` o `Runtime` instalado
- ✅ Tener `dotnet-ef` instalado (se instala automáticamente si falta)
- ✅ Conexión a SQL Server

## 📦 Preparar Actualización

### En tu Servidor de Desarrollo

#### 1. Compilar en modo Release

```powershell
cd C:\asis\SistemIA
dotnet publish -c Release -o .\publish
```

#### 2. Crear archivo ZIP

```powershell
$fecha = Get-Date -Format "yyyyMMdd_HHmm"
$zipName = "SistemIA_Update_$fecha.zip"
Compress-Archive -Path .\publish\* -DestinationPath $zipName
```

#### 3. Transferir al servidor del cliente

Usa cualquier método seguro:
- USB / Disco externo
- FTP / SFTP
- Red compartida
- Servicio en la nube (OneDrive, Dropbox)

**⚠️ NO envíes por email si el archivo es muy grande**

## 🔄 Proceso de Actualización Completo

### Flujo del Script/Interfaz

```
┌─────────────────────────────────────────┐
│ 1. Validaciones Previas                │
│    ✓ Permisos de administrador         │
│    ✓ Archivo ZIP válido                │
│    ✓ Directorios existentes            │
└─────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────┐
│ 2. Detener Servicio/Aplicación         │
│    ✓ Stop-Service SistemIA             │
│    ✓ Forzar cierre de procesos         │
└─────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────┐
│ 3. Crear Backups                        │
│    ✓ Backup de aplicación (ZIP)        │
│    ✓ Backup de BD (SQL .bak)           │
│    ✓ Timestamp único                   │
└─────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────┐
│ 4. Preservar Configuración              │
│    ✓ Guardar appsettings.json          │
│    ✓ Guardar archivos de configuración │
└─────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────┐
│ 5. Extraer y Copiar Actualización       │
│    ✓ Descomprimir ZIP                  │
│    ✓ Copiar archivos nuevos            │
│    ✓ Omitir configuración              │
└─────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────┐
│ 6. Restaurar Configuración              │
│    ✓ Recuperar appsettings.json        │
│    ✓ Mantener cadenas de conexión      │
└─────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────┐
│ 7. Aplicar Migraciones (opcional)       │
│    ✓ dotnet ef database update         │
│    ✓ Actualizar esquema de BD          │
└─────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────┐
│ 8. Limpiar Temporales                   │
│    ✓ Eliminar archivos extraídos       │
└─────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────┐
│ 9. Iniciar Servicio                     │
│    ✓ Start-Service SistemIA            │
│    ✓ Verificar estado                  │
└─────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────┐
│ 10. Verificación Final                  │
│     ✓ Comprobar funcionamiento         │
│     ✓ Revisar logs                     │
└─────────────────────────────────────────┘
```

## 🔐 Backups

### Ubicación por Defecto

```
C:\Backups\SistemIA\
├── SistemIA_Backup_20251215_143000.zip    (Aplicación)
├── SistemIA_Backup_20251215_153000.zip    (Aplicación)
├── SistemIA_backup_20251215_143000.bak    (Base de Datos)
└── SistemIA_backup_20251215_153000.bak    (Base de Datos)
```

### Política de Retención

- **Automática:** Se mantienen los últimos 5 backups de cada tipo
- **Manual:** Puedes limpiar backups antiguos desde la interfaz web

### Restaurar Backup Manualmente

#### Restaurar Aplicación

```powershell
# Detener servicio
Stop-Service SistemIA

# Extraer backup
Expand-Archive -Path "C:\Backups\SistemIA\SistemIA_Backup_20251215_143000.zip" `
               -DestinationPath "C:\Apps\SistemIA" `
               -Force

# Iniciar servicio
Start-Service SistemIA
```

#### Restaurar Base de Datos

```sql
-- En SQL Server Management Studio o sqlcmd

USE master;
GO

-- Poner BD en modo single user (desconectar usuarios)
ALTER DATABASE SistemIA SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Restaurar desde backup
RESTORE DATABASE SistemIA
FROM DISK = 'C:\Backups\SistemIA\SistemIA_backup_20251215_143000.bak'
WITH REPLACE;
GO

-- Volver a modo multi user
ALTER DATABASE SistemIA SET MULTI_USER;
GO
```

## 🛡️ Seguridad y Validaciones

### Validaciones del Script

1. ✅ Ejecución como Administrador
2. ✅ Existencia del archivo ZIP
3. ✅ Tamaño del archivo (máx 500 MB en interfaz web)
4. ✅ Estructura válida del ZIP
5. ✅ Existencia de directorios de instalación
6. ✅ Confirmación del usuario antes de proceder

### Manejo de Errores

Si ocurre un error durante la actualización:

1. **El script intenta rollback automático**
   - Restaura archivos desde el backup
   - Reinicia el servicio
   - Muestra instrucciones para restaurar BD

2. **Si el rollback falla**
   - Se muestran rutas de los backups
   - Se proporcionan comandos para restauración manual

3. **Logs completos**
   - Todos los pasos se registran
   - Fácil diagnóstico de problemas

## 🚨 Solución de Problemas

### Error: "El servicio no se detuvo"

**Solución:**
```powershell
# Forzar cierre de procesos
Get-Process | Where-Object {$_.ProcessName -like "*SistemIA*"} | Stop-Process -Force

# Luego reintentar actualización
```

### Error: "No se puede escribir en el directorio"

**Causa:** Falta de permisos

**Solución:**
```powershell
# Verificar permisos
icacls "C:\Apps\SistemIA"

# Dar permisos al usuario actual
icacls "C:\Apps\SistemIA" /grant "$env:USERNAME:(OI)(CI)F" /T
```

### Error: "Migraciones fallaron"

**Solución:**
```powershell
# Aplicar manualmente
cd C:\Apps\SistemIA
dotnet ef database update --verbose

# Ver migraciones pendientes
dotnet ef migrations list
```

### Error: "Archivo ZIP corrupto"

**Solución:**
1. Verificar integridad del archivo:
   ```powershell
   Test-Path "C:\Temp\Update.zip"
   Get-FileHash "C:\Temp\Update.zip" -Algorithm SHA256
   ```
2. Re-descargar o re-transferir el archivo
3. Verificar que no se interrumpió la transferencia

### El servicio no inicia después de actualizar

**Diagnóstico:**
```powershell
# Ver logs del servicio
Get-EventLog -LogName Application -Source SistemIA -Newest 20

# Ver logs de la aplicación
Get-Content "C:\Apps\SistemIA\logs\*.log" -Tail 50

# Intentar inicio manual para ver errores
cd C:\Apps\SistemIA
.\SistemIA.exe
```

**Causas comunes:**
- Cadena de conexión incorrecta (revisar appsettings.json)
- Puerto en uso (otro proceso usando 5095/7060)
- Migraciones pendientes
- Archivos DLL faltantes

## 📊 Checklist de Actualización

### Antes de Actualizar

- [ ] Leer notas de la nueva versión
- [ ] Verificar requisitos (versión .NET, SQL Server, etc.)
- [ ] Notificar a usuarios que el sistema estará temporalmente fuera de línea
- [ ] Tener acceso como Administrador
- [ ] Verificar espacio en disco (mínimo 2x el tamaño de la aplicación)
- [ ] Crear backup manual adicional (por seguridad)

### Durante la Actualización

- [ ] Cerrar todas las sesiones de usuarios
- [ ] Ejecutar script o usar interfaz web
- [ ] NO interrumpir el proceso
- [ ] Monitorear logs en tiempo real

### Después de Actualizar

- [ ] Verificar que el servicio/aplicación inició correctamente
- [ ] Probar login
- [ ] Verificar funcionalidades críticas:
  - [ ] Crear/editar productos
  - [ ] Realizar venta de prueba
  - [ ] Consultar reportes
  - [ ] Verificar conexión a base de datos
- [ ] Revisar logs de errores
- [ ] Notificar a usuarios que el sistema está disponible
- [ ] Documentar la actualización (versión, fecha, responsable)

## 📝 Registro de Actualizaciones

Mantén un registro de cada actualización:

```
┌─────────────┬──────────┬──────────────┬─────────────────┬──────────┐
│ Fecha       │ Versión  │ Responsable  │ Método          │ Estado   │
├─────────────┼──────────┼──────────────┼─────────────────┼──────────┤
│ 15/12/2025  │ 1.0.0    │ Admin        │ Script PS       │ Exitoso  │
│ 20/12/2025  │ 1.1.0    │ Admin        │ Interfaz Web    │ Exitoso  │
└─────────────┴──────────┴──────────────┴─────────────────┴──────────┘
```

## 🔗 Referencias

- [Plan de Implementación Completo](./PLAN_IMPLEMENTACION.md)
- Documentación de Entity Framework: https://docs.microsoft.com/ef/core/
- Documentación de .NET: https://docs.microsoft.com/dotnet/

## 💡 Consejos y Mejores Prácticas

1. **Siempre haz backup antes de actualizar** - Aunque el sistema lo hace automáticamente, un backup manual extra nunca está de más

2. **Programa actualizaciones en horarios de baja actividad** - Preferiblemente fuera del horario laboral

3. **Prueba en entorno de desarrollo primero** - Si es posible, aplica la actualización en un servidor de pruebas

4. **Mantén backups históricos** - No elimines todos los backups antiguos inmediatamente

5. **Documenta cambios importantes** - Especialmente cambios en configuración o base de datos

6. **Capacita a los usuarios** - Si hay cambios en la interfaz, informa a los usuarios con anticipación

7. **Ten un plan de rollback** - Conoce cómo revertir en caso de problemas graves

8. **Monitorea después de actualizar** - Los primeros 30 minutos son críticos para detectar problemas

## 📞 Soporte

Si encuentras problemas durante la actualización:

1. **Revisa los logs** - La mayoría de errores se explican en los logs
2. **Consulta esta documentación** - Sección "Solución de Problemas"
3. **Contacta soporte técnico** - Proporciona logs y detalles del error

---

**Última actualización:** 15 de diciembre de 2025  
**Versión del documento:** 1.0
