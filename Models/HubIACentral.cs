using System.Text.Json.Serialization;

namespace SistemIA.Models;

/// <summary>
/// Configuración para conectar al Hub Central de IA
/// </summary>
public class HubIACentralSettings
{
    /// <summary>
    /// Habilita/deshabilita la integración con el Hub Central
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// URL base del Hub Central (ej: https://192.168.100.160:3000/api)
    /// </summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>
    /// Identificador único de este sistema en el Hub
    /// </summary>
    public string SistemaId { get; set; } = "sistemia";

    /// <summary>
    /// API Key proporcionada por el Hub Central
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    /// API Secret para autenticación
    /// </summary>
    public string ApiSecret { get; set; } = "";

    /// <summary>
    /// Timeout en segundos para las consultas
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Si debe incluir búsqueda en código fuente
    /// </summary>
    public bool IncluirCodigoFuente { get; set; } = true;

    /// <summary>
    /// Usar como fallback cuando no hay respuesta local
    /// </summary>
    public bool UsarComoFallback { get; set; } = true;
}

/// <summary>
/// Request para consulta al Hub Central
/// </summary>
public class HubConsultaRequest
{
    [JsonPropertyName("pregunta")]
    public string Pregunta { get; set; } = "";

    [JsonPropertyName("contexto")]
    public string? Contexto { get; set; }

    [JsonPropertyName("incluir_codigo")]
    public bool IncluirCodigo { get; set; } = true;

    [JsonPropertyName("pagina_actual")]
    public string? PaginaActual { get; set; }
}

/// <summary>
/// Response de consulta del Hub Central
/// </summary>
public class HubConsultaResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("respuesta")]
    public string Respuesta { get; set; } = "";

    [JsonPropertyName("fuentes")]
    public List<HubFuenteInfo> Fuentes { get; set; } = new();

    [JsonPropertyName("tokens")]
    public HubTokensInfo? Tokens { get; set; }

    [JsonPropertyName("tiempo_ms")]
    public int TiempoMs { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
/// Información de fuente usada en la respuesta
/// </summary>
public class HubFuenteInfo
{
    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = ""; // "conocimiento" | "codigo"

    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("titulo")]
    public string? Titulo { get; set; }

    [JsonPropertyName("archivo")]
    public string? Archivo { get; set; }

    [JsonPropertyName("lineas")]
    public string? Lineas { get; set; }
}

/// <summary>
/// Información de tokens usados
/// </summary>
public class HubTokensInfo
{
    [JsonPropertyName("entrada")]
    public int Entrada { get; set; }

    [JsonPropertyName("salida")]
    public int Salida { get; set; }
}

/// <summary>
/// Request para sincronizar conocimientos con el Hub
/// </summary>
public class HubSincronizarConocimientosRequest
{
    [JsonPropertyName("sistema_id")]
    public string SistemaId { get; set; } = "";

    [JsonPropertyName("conocimientos")]
    public List<HubConocimientoItem> Conocimientos { get; set; } = new();
}

/// <summary>
/// Item de conocimiento para sincronización
/// </summary>
public class HubConocimientoItem
{
    [JsonPropertyName("categoria")]
    public string Categoria { get; set; } = "";

    [JsonPropertyName("subcategoria")]
    public string? Subcategoria { get; set; }

    [JsonPropertyName("titulo")]
    public string Titulo { get; set; } = "";

    [JsonPropertyName("contenido")]
    public string Contenido { get; set; } = "";

    [JsonPropertyName("palabras_clave")]
    public List<string> PalabrasClave { get; set; } = new();

    [JsonPropertyName("prioridad")]
    public int Prioridad { get; set; } = 5;
}

/// <summary>
/// Response de sincronización
/// </summary>
public class HubSincronizarResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("insertados")]
    public int Insertados { get; set; }

    [JsonPropertyName("actualizados")]
    public int Actualizados { get; set; }

    [JsonPropertyName("errores")]
    public List<string> Errores { get; set; } = new();
}

/// <summary>
/// Estado de conexión con el Hub
/// </summary>
public class HubEstadoConexion
{
    public bool Conectado { get; set; }
    public bool Habilitado { get; set; }
    public DateTime? UltimaConexion { get; set; }
    public string? Version { get; set; }
    public string? Mensaje { get; set; }
    public int? ConsultasHoy { get; set; }
    public int? LimiteConsultas { get; set; }
}
