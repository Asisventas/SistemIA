using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SistemIA.Models;

/// <summary>
/// Converter flexible para IDs que pueden venir como string o número
/// </summary>
public class FlexibleIdConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.GetInt64().ToString(),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unexpected token type: {reader.TokenType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value);
    }
}

/// <summary>
/// Converter flexible para Size que puede venir como string o número
/// </summary>
public class FlexibleSizeConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => long.TryParse(reader.GetString(), out var size) ? size : 0,
            JsonTokenType.Number => reader.GetInt64(),
            JsonTokenType.Null => 0,
            _ => throw new JsonException($"Unexpected token type for size: {reader.TokenType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

/// <summary>
/// Configuración de sincronización con CloudSync
/// </summary>
public class ConfiguracionCloudSync
{
    [Key]
    public int IdConfiguracion { get; set; }

    /// <summary>
    /// API Key proporcionada por CloudSync
    /// </summary>
    [MaxLength(100)]
    public string? ApiKey { get; set; }

    /// <summary>
    /// URL base de la API (ej: https://190.104.149.35:3000/api)
    /// </summary>
    [MaxLength(200)]
    public string UrlApi { get; set; } = "https://190.104.149.35:3000/api";

    /// <summary>
    /// Carpeta local de donde se obtienen los backups
    /// </summary>
    [MaxLength(500)]
    public string? CarpetaBackup { get; set; }

    /// <summary>
    /// Extensiones de archivo a incluir (ej: .bak,.zip)
    /// </summary>
    [MaxLength(200)]
    public string ExtensionesIncluir { get; set; } = ".bak,.zip,.sql";

    /// <summary>
    /// Si el backup automático está habilitado
    /// </summary>
    public bool BackupAutomaticoHabilitado { get; set; } = false;

    /// <summary>
    /// Tipo de programación: "Horario" (hora fija) o "Intervalo" (cada X horas)
    /// </summary>
    [MaxLength(20)]
    public string TipoProgramacion { get; set; } = "Horario";

    /// <summary>
    /// Hora del día para ejecutar backup (formato HH:mm) - Solo para TipoProgramacion="Horario"
    /// </summary>
    public TimeSpan? HoraBackup { get; set; }

    /// <summary>
    /// Intervalo en horas entre backups - Solo para TipoProgramacion="Intervalo"
    /// </summary>
    public int IntervaloHoras { get; set; } = 6;

    /// <summary>
    /// Días de la semana para backup (CSV: 1,2,3,4,5 = Lun-Vie)
    /// </summary>
    [MaxLength(20)]
    public string? DiasBackup { get; set; } = "1,2,3,4,5";

    /// <summary>
    /// Retener últimos N backups en la nube
    /// </summary>
    public int RetenerUltimosBackups { get; set; } = 30;

    /// <summary>
    /// Comprimir antes de subir
    /// </summary>
    public bool ComprimirAntesDeSubir { get; set; } = true;

    /// <summary>
    /// Última sincronización exitosa
    /// </summary>
    public DateTime? UltimaSincronizacion { get; set; }

    /// <summary>
    /// Último backup exitoso sincronizado
    /// </summary>
    public DateTime? UltimoBackupExitoso { get; set; }

    /// <summary>
    /// Estado de la última sincronización
    /// </summary>
    [MaxLength(50)]
    public string? EstadoUltimaSincronizacion { get; set; }

    /// <summary>
    /// Mensaje de la última sincronización
    /// </summary>
    public string? MensajeUltimaSincronizacion { get; set; }

    /// <summary>
    /// Próxima ejecución programada
    /// </summary>
    public DateTime? ProximaEjecucion { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// Historial de backups subidos a CloudSync
/// </summary>
public class HistorialBackupCloud
{
    [Key]
    public int IdHistorial { get; set; }

    /// <summary>
    /// Nombre del archivo subido
    /// </summary>
    [MaxLength(300)]
    public string NombreArchivo { get; set; } = "";

    /// <summary>
    /// Ruta local del archivo
    /// </summary>
    [MaxLength(500)]
    public string? RutaLocal { get; set; }

    /// <summary>
    /// Ruta remota en CloudSync
    /// </summary>
    [MaxLength(500)]
    public string? RutaRemota { get; set; }

    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long TamanoBytes { get; set; }

    /// <summary>
    /// Fecha y hora de inicio de subida
    /// </summary>
    public DateTime FechaInicio { get; set; }

    /// <summary>
    /// Fecha y hora de fin de subida
    /// </summary>
    public DateTime? FechaFin { get; set; }

    /// <summary>
    /// Duración en segundos
    /// </summary>
    public int? DuracionSegundos { get; set; }

    /// <summary>
    /// Estado: Pendiente, Subiendo, Completado, Error
    /// </summary>
    [MaxLength(30)]
    public string Estado { get; set; } = "Pendiente";

    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? MensajeError { get; set; }

    /// <summary>
    /// ID del archivo en CloudSync (si lo devuelve la API)
    /// </summary>
    [MaxLength(100)]
    public string? IdArchivoRemoto { get; set; }

    /// <summary>
    /// Hash MD5 del archivo para verificación
    /// </summary>
    [MaxLength(32)]
    public string? HashMD5 { get; set; }

    /// <summary>
    /// Si fue backup automático o manual
    /// </summary>
    public bool FueAutomatico { get; set; }

    /// <summary>
    /// Usuario que inició el backup (si fue manual)
    /// </summary>
    public int? IdUsuario { get; set; }

    [ForeignKey("IdUsuario")]
    public Usuario? Usuario { get; set; }

    // Propiedades calculadas
    [NotMapped]
    public string TamanoFormateado => FormatearTamano(TamanoBytes);

    [NotMapped]
    public string DuracionFormateada => DuracionSegundos.HasValue 
        ? TimeSpan.FromSeconds(DuracionSegundos.Value).ToString(@"mm\:ss") 
        : "-";

    private static string FormatearTamano(long bytes)
    {
        string[] sufijos = { "B", "KB", "MB", "GB", "TB" };
        int i = 0;
        double tamano = bytes;
        while (tamano >= 1024 && i < sufijos.Length - 1)
        {
            tamano /= 1024;
            i++;
        }
        return $"{tamano:N2} {sufijos[i]}";
    }
}

// ========== DTOs para la API CloudSync ==========

/// <summary>
/// Respuesta de subida de archivo
/// </summary>
public class CloudSyncUploadResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("file")]
    public CloudSyncFileInfo? File { get; set; }

    // Alternativa: algunos endpoints devuelven data.file
    [JsonPropertyName("data")]
    public CloudSyncUploadData? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    // Helper para obtener el archivo de cualquier estructura
    public CloudSyncFileInfo? GetFile() => File ?? Data?.File;
}

public class CloudSyncUploadData
{
    [JsonPropertyName("file")]
    public CloudSyncFileInfo? File { get; set; }
}

/// <summary>
/// Información de archivo en CloudSync
/// </summary>
public class CloudSyncFileInfo
{
    // El servidor devuelve id como número, usamos JsonConverter para aceptar ambos
    [JsonPropertyName("id")]
    [JsonConverter(typeof(FlexibleIdConverter))]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("size")]
    [JsonConverter(typeof(FlexibleSizeConverter))]
    public long Size { get; set; }

    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Respuesta de listado de archivos
/// </summary>
public class CloudSyncListResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("files")]
    public List<CloudSyncFileInfo> Files { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
/// Estado de conexión con CloudSync
/// </summary>
public class CloudSyncEstadoConexion
{
    public bool Conectado { get; set; }
    public string? Version { get; set; }
    public string? Mensaje { get; set; }
    public long? EspacioUsado { get; set; }
    public long? EspacioTotal { get; set; }
    public int? ArchivosEnNube { get; set; }
}

// ========== DTOs para subida por Chunks ==========

/// <summary>
/// Respuesta de inicialización de subida por chunks
/// </summary>
public class ChunkUploadInitResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public ChunkUploadInitData? Data { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public class ChunkUploadInitData
{
    [JsonPropertyName("uploadId")]
    public string? UploadId { get; set; }

    [JsonPropertyName("chunkSize")]
    public int ChunkSize { get; set; }
}

/// <summary>
/// Respuesta de subida de chunk individual
/// </summary>
public class ChunkUploadResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public ChunkUploadData? Data { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public class ChunkUploadData
{
    [JsonPropertyName("chunkIndex")]
    public int ChunkIndex { get; set; }

    [JsonPropertyName("received")]
    public bool Received { get; set; }
}

/// <summary>
/// Respuesta de completar subida por chunks
/// </summary>
public class ChunkUploadCompleteResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public ChunkUploadCompleteData? Data { get; set; }

    [JsonPropertyName("file")]
    public CloudSyncFileInfo? File { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public class ChunkUploadCompleteData
{
    [JsonPropertyName("file")]
    public CloudSyncFileInfo? File { get; set; }
}
