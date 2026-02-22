# Sistema CloudSync - Documentación Técnica

## 📋 Descripción General

**CloudSync** es el módulo de sincronización de backups en la nube de SistemIA. Permite respaldar automáticamente la base de datos y archivos de backup a un servidor remoto.

---

## 🗃️ Modelos de Datos

### ConfiguracionCloudSync (`Models/CloudSync.cs`)

```csharp
public class ConfiguracionCloudSync
{
    public int IdConfiguracion { get; set; }
    
    // ========== CONEXIÓN API ==========
    public string? UrlApi { get; set; }           // URL del servidor CloudSync (ej: http://190.104.149.35:3000/api)
    public string? ApiKey { get; set; }           // API Key de autenticación
    
    // ========== CONFIGURACIÓN LOCAL ==========
    public string? CarpetaBackup { get; set; }    // Ruta local de backups (ej: C:\SistemIA\Backups)
    public string? ExtensionesIncluir { get; set; } // Extensiones a sincronizar (ej: ".bak,.zip,.sql")
    
    // ========== PROGRAMACIÓN ==========
    public bool BackupProgramado { get; set; }    // Si está habilitado el backup automático
    public string? HoraBackup { get; set; }       // Hora del backup (formato "HH:mm")
    public string? DiasBackup { get; set; }       // Días separados por coma (1=Lun...7=Dom)
    
    // ========== ESTADO ==========
    public bool Activo { get; set; }
    public DateTime? UltimaSincronizacion { get; set; }
    public string? EstadoUltimaSincronizacion { get; set; }  // "Exitoso" o "Error"
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
```

### HistorialBackupCloud (`Models/CloudSync.cs`)

```csharp
public class HistorialBackupCloud
{
    public int IdHistorial { get; set; }
    public string NombreArchivo { get; set; }
    public long TamanoBytes { get; set; }
    public string? HashMD5 { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Estado { get; set; }           // "Completado", "Error", "EnProgreso"
    public string? MensajeError { get; set; }
    public string? IdArchivoRemoto { get; set; } // ID del archivo en el servidor
    public bool EsAutomatico { get; set; }       // Si fue backup programado
    public int? IdUsuario { get; set; }          // Usuario que lo ejecutó (si manual)
    
    // Calculados
    public string TamanoFormateado { get; }      // "1.5 GB", "500 MB", etc.
    public TimeSpan? Duracion { get; }
}
```

### CloudSyncFileInfo (`Models/CloudSync.cs`)

Representa un archivo en el servidor remoto:

```csharp
public class CloudSyncFileInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("path")]
    public string? Path { get; set; }
    
    [JsonPropertyName("size")]
    public long Size { get; set; }
    
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}
```

### EstadoSincronizacionArchivo (`Services/CloudSyncService.cs`)

Estado de sincronización de un archivo local vs remoto:

```csharp
public class EstadoSincronizacionArchivo
{
    public string RutaLocal { get; set; }
    public string NombreArchivo { get; set; }
    public long TamanoLocal { get; set; }
    public DateTime FechaLocal { get; set; }
    
    // Estado de sincronización
    public bool ExisteEnNube { get; set; }
    public bool EstaSincronizado { get; set; }  // Mismo nombre Y tamaño
    public bool NecesitaActualizar { get; set; } // Mismo nombre pero diferente tamaño
    
    // Info del archivo remoto (si existe)
    public string? IdRemoto { get; set; }
    public long? TamanoRemoto { get; set; }
    public DateTime? FechaRemota { get; set; }
    
    // Propiedades calculadas para UI
    public string DescripcionEstado { get; }  // "Sincronizado ✓", "No sincronizado", etc.
    public string ClaseCss { get; }           // "text-success", "text-warning", etc.
    public string Icono { get; }              // "bi-cloud-check", "bi-cloud-slash", etc.
}
```

---

## 🔧 Servicios

### ICloudSyncService (`Services/CloudSyncService.cs`)

Interfaz principal del servicio de sincronización:

```csharp
public interface ICloudSyncService
{
    // Conexión
    Task<CloudSyncEstadoConexion> VerificarConexionAsync();
    
    // Subida de archivos
    Task<(bool exito, string mensaje, string? idRemoto)> SubirArchivoAsync(
        string rutaArchivo, 
        bool esAutomatico = false, 
        int? idUsuario = null,
        Action<CloudSyncUploadProgress>? onProgress = null);
    
    // Sincronización masiva
    Task<(int exitosos, int fallidos, List<string> errores)> SincronizarBackupsAsync(int? idUsuario = null);
    
    // Listado de archivos
    Task<List<CloudSyncFileInfo>> ListarArchivosRemotosAsync(string? carpeta = null);
    Task<List<(string ruta, long tamano, DateTime fechaModificacion)>> ObtenerArchivosPendientesAsync();
    
    // Estado de sincronización
    Task<List<EstadoSincronizacionArchivo>> ObtenerEstadoSincronizacionAsync(List<string> rutasArchivos);
    Task<(bool existeEnNube, bool esIdentico, CloudSyncFileInfo? archivoRemoto)> VerificarSincronizacionAsync(string rutaArchivoLocal);
    
    // Operaciones con archivos remotos
    Task<bool> EliminarArchivoRemotoAsync(string idArchivo);
    Task<(bool exito, string? rutaLocal)> DescargarArchivoAsync(string idArchivo, string carpetaDestino);
    
    // Configuración
    Task<ConfiguracionCloudSync?> ObtenerConfiguracionAsync();
    Task<bool> GuardarConfiguracionAsync(ConfiguracionCloudSync config);
    
    // Historial
    Task<List<HistorialBackupCloud>> ObtenerHistorialAsync(int cantidad = 50);
    Task<(bool exito, int registrosEliminados)> EliminarHistorialAsync();
    
    // Diagnóstico
    Task<CloudSyncDiagnostico> DiagnosticarApiAsync();
}
```

### Métodos Importantes

#### SubirArchivoAsync
Sube un archivo al servidor con soporte de progreso:
- Calcula MD5 del archivo
- Reporta progreso mediante callback
- Registra en historial
- Maneja reintentos automáticos

#### ObtenerEstadoSincronizacionAsync
Compara archivos locales con remotos:
1. Obtiene lista de archivos remotos
2. Para cada archivo local, busca coincidencia por nombre
3. Compara tamaños para determinar si está sincronizado
4. Retorna estado detallado de cada archivo

#### SincronizarBackupsAsync
Sincronización masiva:
1. Obtiene archivos pendientes (no subidos según hash MD5)
2. Sube cada archivo secuencialmente
3. Retorna conteo de éxitos/fallos

---

## 📡 API CloudSync (Servidor Remoto)

### URL Base
```
http://190.104.149.35:3000/api
```

### Endpoints

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/status` | Estado del servidor y versión |
| GET | `/files` | Lista archivos en la nube |
| POST | `/files/upload` | Sube un archivo (multipart/form-data) |
| GET | `/files/{id}/download` | Descarga un archivo |
| DELETE | `/files/{id}` | Elimina un archivo |

### Autenticación
Header: `X-API-Key: {ApiKey}`

### Respuestas

**Lista de archivos:**
```json
{
  "success": true,
  "files": [
    {
      "id": "123",
      "name": "backup_2026-01-26.bak",
      "path": "/backups/",
      "size": 1073741824,
      "mimeType": "application/octet-stream",
      "createdAt": "2026-01-26T10:30:00Z",
      "updatedAt": "2026-01-26T10:30:00Z"
    }
  ],
  "total": 1
}
```

**Subida exitosa:**
```json
{
  "success": true,
  "message": "Archivo subido exitosamente",
  "file": {
    "id": "456",
    "name": "backup_2026-01-26.bak",
    "size": 1073741824
  }
}
```

---

## 📄 Páginas

### CloudSync.razor (`Pages/CloudSync.razor`)

Página principal de gestión de backups en la nube.

#### Secciones:
1. **Estado de Conexión** - Muestra si está conectado al servidor
2. **Backup de Base de Datos** - Card principal para crear y sincronizar backup de BD
3. **Configuración** - URL API, API Key, carpeta local, programación
4. **Archivos de Backup Local** - Lista archivos locales con estado de sincronización
5. **Historial de Backups** - Registro de todos los backups realizados

#### Variables de Estado Importantes:
```csharp
private ConfiguracionCloudSync? _config;
private CloudSyncEstadoConexion? _estadoConexion;
private bool _conexionOk;
private List<HistorialBackupCloud> _historial = new();
private List<(string ruta, long tamano, DateTime fechaModificacion)> _archivosPendientes = new();
private Dictionary<string, EstadoSincronizacionArchivo> _estadosSincronizacion = new();
private CloudSyncFileInfo? _backupEnNube;  // Backup de BD en la nube
```

#### Métodos Principales:
- `VerificarConexion()` - Verifica conexión con el servidor
- `VerificarEstadoSincronizacion()` - Compara archivos locales vs remotos
- `CrearBackupYSincronizar()` - Crea backup de BD y lo sube
- `SubirArchivo(ruta)` - Sube un archivo individual
- `EliminarHistorial()` - Limpia el historial de backups
- `GuardarConfiguracion()` - Guarda la configuración

---

## 🔄 Flujo de Backup Automático

### BackupSchedulerService (`Services/BackupSchedulerService.cs`)

Servicio en background que ejecuta backups programados:

```csharp
public class BackupSchedulerService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            await VerificarYEjecutarBackupProgramado();
        }
    }
}
```

#### Lógica de Ejecución:
1. Cada minuto verifica si hay backup programado
2. Compara hora actual con `HoraBackup` configurada
3. Verifica si el día actual está en `DiasBackup`
4. Si corresponde, ejecuta `CrearBackupYSincronizar()`
5. Actualiza `UltimaSincronizacion` y `EstadoUltimaSincronizacion`

---

## 🗄️ Base de Datos

### Tablas

```sql
-- Configuración
CREATE TABLE ConfiguracionesCloudSync (
    IdConfiguracion INT IDENTITY PRIMARY KEY,
    UrlApi NVARCHAR(500),
    ApiKey NVARCHAR(500),
    CarpetaBackup NVARCHAR(500),
    ExtensionesIncluir NVARCHAR(200),
    BackupProgramado BIT DEFAULT 0,
    HoraBackup NVARCHAR(10),
    DiasBackup NVARCHAR(50),
    Activo BIT DEFAULT 1,
    UltimaSincronizacion DATETIME,
    EstadoUltimaSincronizacion NVARCHAR(50),
    FechaCreacion DATETIME DEFAULT GETDATE(),
    FechaModificacion DATETIME
);

-- Historial
CREATE TABLE HistorialBackupsCloud (
    IdHistorial INT IDENTITY PRIMARY KEY,
    NombreArchivo NVARCHAR(500) NOT NULL,
    TamanoBytes BIGINT,
    HashMD5 NVARCHAR(100),
    FechaInicio DATETIME NOT NULL,
    FechaFin DATETIME,
    Estado NVARCHAR(50) NOT NULL,
    MensajeError NVARCHAR(MAX),
    IdArchivoRemoto NVARCHAR(100),
    EsAutomatico BIT DEFAULT 0,
    IdUsuario INT
);
```

---

## 🎨 Indicadores de UI

### Estado de Sincronización en NavMenu

El menú principal muestra un indicador de estado del backup:

```razor
<!-- En NavMenu.razor -->
@if (_backupActualizado)
{
    <i class="bi bi-check-circle text-success"></i>  <!-- Verde: sincronizado -->
}
else if (_ultimoBackup.HasValue)
{
    <i class="bi bi-exclamation-triangle text-warning"></i>  <!-- Amarillo: desactualizado -->
}
else
{
    <i class="bi bi-x-circle text-danger"></i>  <!-- Rojo: sin backup -->
}
```

### Resumen de Sincronización en CloudSync

Barra que muestra conteo de archivos:
- **Verde** (`bg-success-subtle`): Todos sincronizados
- **Amarillo** (`bg-warning-subtle`): Algunos pendientes
- **Azul** (`bg-info-subtle`): Algunos para actualizar

---

## 📦 Registro en Program.cs

```csharp
// Servicios CloudSync
builder.Services.AddScoped<ICloudSyncService, CloudSyncService>();
builder.Services.AddHostedService<BackupSchedulerService>();

// HttpClient para CloudSync
builder.Services.AddHttpClient("CloudSync", client =>
{
    client.Timeout = TimeSpan.FromMinutes(30); // Timeout largo para archivos grandes
});
```

---

## 🔐 Seguridad

### API Key
- Se almacena en la tabla `ConfiguracionesCloudSync`
- Se envía en cada request como header `X-API-Key`
- Se muestra oculta en la UI (tipo password)

### Hash MD5
- Se calcula para cada archivo antes de subir
- Se usa para evitar subir duplicados
- Se almacena en el historial para referencia

---

## 🐛 Troubleshooting

### Problemas Comunes

| Problema | Causa | Solución |
|----------|-------|----------|
| "No se puede conectar" | URL incorrecta o servidor caído | Verificar URL y que el servidor esté activo |
| "API Key inválida" | Key incorrecta o expirada | Verificar ApiKey en configuración |
| "Archivo ya existe" | Duplicado detectado por hash | Verificar estado de sincronización |
| Timeout en subida | Archivo muy grande | Aumentar timeout o dividir archivo |

### Logs de Debug
```csharp
// El servicio usa ILogger<CloudSyncService>
_logger.LogInformation("Subiendo archivo {Nombre} ({Tamano} bytes)", nombre, tamano);
_logger.LogError(ex, "Error al subir archivo {Nombre}", nombre);
```

---

## 📝 Notas Importantes

1. **Backup de BD**: El archivo se llama siempre `SistemIA_Backup.bak` y se sobrescribe en cada backup
2. **Carpeta por defecto**: `C:\SistemIA\Backups`
3. **Extensiones por defecto**: `.bak`, `.zip`, `.sql`
4. **Días de backup**: Por defecto Lunes a Viernes (1,2,3,4,5)
5. **El historial no borra archivos**: Solo elimina registros de la tabla, no archivos remotos
