using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.AsistenteIA
{
    /// <summary>
    /// Configuración global del Asistente IA
    /// </summary>
    public class ConfiguracionAsistenteIA
    {
        [Key]
        public int IdConfiguracion { get; set; } = 1;

        // ========== CORREO DE SOPORTE ==========
        /// <summary>
        /// Correo electrónico al que se envían los reportes de problemas
        /// </summary>
        [MaxLength(150)]
        public string? CorreoSoporte { get; set; }

        /// <summary>
        /// Nombre del soporte para mostrar en correos
        /// </summary>
        [MaxLength(100)]
        public string NombreSoporte { get; set; } = "Soporte Técnico SistemIA";

        /// <summary>
        /// Si está habilitado el envío de reportes a soporte
        /// </summary>
        public bool HabilitarEnvioSoporte { get; set; } = true;

        // ========== CONFIGURACIÓN GENERAL ==========
        /// <summary>
        /// Mensaje de bienvenida personalizado (puede incluir {nombre})
        /// </summary>
        [MaxLength(500)]
        public string? MensajeBienvenida { get; set; }

        /// <summary>
        /// Mensaje cuando no se encuentra respuesta
        /// </summary>
        [MaxLength(500)]
        public string MensajeSinRespuesta { get; set; } = "No encontré información sobre tu consulta. ¿Deseas enviar un reporte a soporte técnico?";

        /// <summary>
        /// Habilitar voz de entrada (reconocimiento)
        /// </summary>
        public bool HabilitarVozEntrada { get; set; } = true;

        /// <summary>
        /// Habilitar voz de salida (texto a voz)
        /// </summary>
        public bool HabilitarVozSalida { get; set; } = true;

        /// <summary>
        /// Habilitar captura de pantalla en reportes
        /// </summary>
        public bool HabilitarCapturaPantalla { get; set; } = true;

        /// <summary>
        /// Habilitar grabación de video corto (máx 30 seg)
        /// </summary>
        public bool HabilitarGrabacionVideo { get; set; } = true;

        /// <summary>
        /// Segundos máximos de grabación de video
        /// </summary>
        public int MaxSegundosVideo { get; set; } = 30;

        // ========== SERVIDOR CENTRAL DE CONOCIMIENTO ==========
        /// <summary>
        /// Habilitar consulta a servidor central cuando no hay respuesta local
        /// </summary>
        public bool HabilitarServidorCentral { get; set; } = false;

        /// <summary>
        /// Servidor SQL Server central (ej: 192.168.1.100 o servidor.empresa.com\SQLSERVER)
        /// </summary>
        [MaxLength(200)]
        public string? ServidorCentral { get; set; }

        /// <summary>
        /// Puerto del servidor (default 1433)
        /// </summary>
        public int PuertoCentral { get; set; } = 1433;

        /// <summary>
        /// Nombre de la base de datos central de conocimiento IA
        /// </summary>
        [MaxLength(100)]
        public string? BaseDatosCentral { get; set; }

        /// <summary>
        /// Usuario para autenticación SQL (si es vacío, usa Windows Auth)
        /// </summary>
        [MaxLength(100)]
        public string? UsuarioCentral { get; set; }

        /// <summary>
        /// Contraseña para autenticación SQL (encriptada)
        /// </summary>
        [MaxLength(200)]
        public string? ContrasenaCentral { get; set; }

        /// <summary>
        /// Usar autenticación de Windows en lugar de SQL
        /// </summary>
        public bool UsarWindowsAuthCentral { get; set; } = false;

        /// <summary>
        /// Timeout en segundos para conexión al servidor central
        /// </summary>
        public int TimeoutConexionCentral { get; set; } = 10;

        /// <summary>
        /// Última vez que se verificó la conexión al servidor central
        /// </summary>
        public DateTime? UltimaVerificacionCentral { get; set; }

        /// <summary>
        /// Estado de la última conexión al servidor central
        /// </summary>
        public bool? ConexionCentralExitosa { get; set; }

        // ========== MODO DE CONEXIÓN (SQL vs API REST) ==========
        /// <summary>
        /// Modo de conexión: "SQL" = conexión directa a SQL Server, "API" = API REST (más segura)
        /// </summary>
        [MaxLength(10)]
        public string ModoConexionCentral { get; set; } = "API";

        /// <summary>
        /// URL base de la API REST central (ej: https://api.empresa.com/asistente-ia)
        /// Solo se usa cuando ModoConexionCentral = "API"
        /// </summary>
        [MaxLength(500)]
        public string? UrlApiCentral { get; set; }

        /// <summary>
        /// API Key para autenticación con el servidor central
        /// Se encripta antes de guardar
        /// </summary>
        [MaxLength(200)]
        public string? ApiKeyCentral { get; set; }

        /// <summary>
        /// Secret adicional para firmar peticiones (opcional, para mayor seguridad)
        /// Se encripta antes de guardar
        /// </summary>
        [MaxLength(200)]
        public string? ApiSecretCentral { get; set; }

        // ========== AUDITORÍA ==========
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Solicitud de soporte enviada desde el asistente
    /// </summary>
    public class SolicitudSoporteIA
    {
        [Key]
        public int IdSolicitud { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int? IdUsuario { get; set; }

        [MaxLength(100)]
        public string? NombreUsuario { get; set; }

        [MaxLength(200)]
        public string? PaginaOrigen { get; set; }

        /// <summary>
        /// Pregunta original del usuario
        /// </summary>
        public string? PreguntaOriginal { get; set; }

        /// <summary>
        /// Descripción adicional del problema
        /// </summary>
        public string? DescripcionProblema { get; set; }

        /// <summary>
        /// Screenshot en Base64 (si se capturó)
        /// </summary>
        public string? ScreenshotBase64 { get; set; }

        /// <summary>
        /// Video en Base64 (si se grabó)
        /// </summary>
        public string? VideoBase64 { get; set; }

        /// <summary>
        /// Información del navegador/sistema
        /// </summary>
        [MaxLength(500)]
        public string? InfoNavegador { get; set; }

        /// <summary>
        /// Contexto de acciones del usuario (historial reciente + errores)
        /// Generado por TrackingService para facilitar diagnóstico
        /// </summary>
        public string? ContextoAcciones { get; set; }

        /// <summary>
        /// Estado: Pendiente, EnProceso, Resuelto
        /// </summary>
        [MaxLength(30)]
        public string Estado { get; set; } = "Pendiente";

        /// <summary>
        /// Si se envió el correo correctamente
        /// </summary>
        public bool CorreoEnviado { get; set; } = false;

        /// <summary>
        /// Mensaje de error si falló el envío
        /// </summary>
        [MaxLength(500)]
        public string? ErrorEnvio { get; set; }

        /// <summary>
        /// Notas del soporte técnico
        /// </summary>
        public string? NotasSoporte { get; set; }

        public DateTime? FechaResolucion { get; set; }
    }

    /// <summary>
    /// Representa un artículo de conocimiento del asistente IA (para JSON)
    /// </summary>
    public class ArticuloConocimiento
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public string Categoria { get; set; } = string.Empty;
        public string Subcategoria { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public List<string> PalabrasClave { get; set; } = new();
        public List<string> Sinonimos { get; set; } = new();
        public string? RutaNavegacion { get; set; }
        public string? Icono { get; set; }
        public int Prioridad { get; set; } = 5; // 1-10, mayor = más relevante
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Artículo de conocimiento almacenado en BD (editable por admin)
    /// </summary>
    public class ArticuloConocimientoDB
    {
        [Key]
        public int IdArticulo { get; set; }

        [Required]
        [MaxLength(50)]
        public string Categoria { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Subcategoria { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Contenido { get; set; } = string.Empty;

        /// <summary>
        /// Palabras clave separadas por coma
        /// </summary>
        [MaxLength(500)]
        public string? PalabrasClave { get; set; }

        /// <summary>
        /// Sinónimos separados por coma
        /// </summary>
        [MaxLength(500)]
        public string? Sinonimos { get; set; }

        [MaxLength(200)]
        public string? RutaNavegacion { get; set; }

        [MaxLength(50)]
        public string? Icono { get; set; }

        /// <summary>
        /// Prioridad 1-10 (mayor = más relevante)
        /// </summary>
        public int Prioridad { get; set; } = 5;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;

        /// <summary>
        /// Usuario que creó el artículo
        /// </summary>
        public int? IdUsuarioCreador { get; set; }

        /// <summary>
        /// Cantidad de veces que este artículo fue utilizado como respuesta
        /// </summary>
        public int VecesUtilizado { get; set; } = 0;

        // Métodos de conversión
        public List<string> ObtenerPalabrasClave() =>
            string.IsNullOrEmpty(PalabrasClave) 
                ? new List<string>() 
                : PalabrasClave.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        public List<string> ObtenerSinonimos() =>
            string.IsNullOrEmpty(Sinonimos) 
                ? new List<string>() 
                : Sinonimos.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        public ArticuloConocimiento ToArticuloConocimiento() => new()
        {
            Id = $"db_{IdArticulo}",
            Categoria = Categoria,
            Subcategoria = Subcategoria ?? string.Empty,
            Titulo = Titulo,
            Contenido = Contenido,
            PalabrasClave = ObtenerPalabrasClave(),
            Sinonimos = ObtenerSinonimos(),
            RutaNavegacion = RutaNavegacion,
            Icono = Icono,
            Prioridad = Prioridad,
            FechaActualizacion = FechaActualizacion
        };
    }

    /// <summary>
    /// Pregunta sin respuesta para que el admin pueda crear conocimiento
    /// </summary>
    public class PreguntaSinRespuesta
    {
        [Key]
        public int IdPregunta { get; set; }

        [Required]
        public string Pregunta { get; set; } = string.Empty;

        public int CantidadVeces { get; set; } = 1;

        public DateTime PrimeraVez { get; set; } = DateTime.Now;

        public DateTime UltimaVez { get; set; } = DateTime.Now;

        /// <summary>
        /// Si ya se creó un artículo para responder esta pregunta
        /// </summary>
        public bool Resuelta { get; set; } = false;

        /// <summary>
        /// ID del artículo creado para responder esta pregunta (si existe)
        /// </summary>
        public int? IdArticuloRespuesta { get; set; }
    }

    /// <summary>
    /// Categorías disponibles para artículos
    /// </summary>
    public class CategoriaConocimiento
    {
        [Key]
        public int IdCategoria { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        [MaxLength(50)]
        public string? Icono { get; set; }

        public int Orden { get; set; } = 0;

        public bool Activa { get; set; } = true;
    }

    /// <summary>
    /// Representa una intención del usuario
    /// </summary>
    public class IntencionUsuario
    {
        public string Nombre { get; set; } = string.Empty;
        public List<string> Patrones { get; set; } = new();
        public string TipoAccion { get; set; } = "informacion"; // informacion, navegacion, informe, consejo
        public string? AccionParametro { get; set; }
        public List<string> RespuestasPosibles { get; set; } = new();
    }

    /// <summary>
    /// Representa un consejo contextual
    /// </summary>
    public class ConsejoContextual
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public string Modulo { get; set; } = string.Empty;
        public string Contexto { get; set; } = string.Empty; // crear, editar, listar, error
        public string Mensaje { get; set; } = string.Empty;
        public string? Condicion { get; set; } // Condición opcional para mostrar
        public int Frecuencia { get; set; } = 1; // Cada cuántas veces mostrar
        public bool Activo { get; set; } = true;
    }

    /// <summary>
    /// Registro de errores para análisis
    /// </summary>
    public class RegistroError
    {
        [Key]
        public int IdRegistroError { get; set; }
        
        public DateTime Fecha { get; set; } = DateTime.Now;
        
        [MaxLength(100)]
        public string? Modulo { get; set; }
        
        [MaxLength(200)]
        public string? Pagina { get; set; }
        
        public string? MensajeError { get; set; }
        
        public string? StackTrace { get; set; }
        
        public int? IdUsuario { get; set; }
        
        [MaxLength(100)]
        public string? NombreUsuario { get; set; }
        
        [MaxLength(50)]
        public string? TipoError { get; set; }
        
        public bool Analizado { get; set; } = false;
        
        [MaxLength(500)]
        public string? NotasAnalisis { get; set; }
    }

    /// <summary>
    /// Historial de conversaciones con el asistente
    /// </summary>
    public class ConversacionAsistente
    {
        [Key]
        public int IdConversacion { get; set; }
        
        public DateTime Fecha { get; set; } = DateTime.Now;
        
        public int? IdUsuario { get; set; }
        
        [MaxLength(100)]
        public string? NombreUsuario { get; set; }
        
        public string Pregunta { get; set; } = string.Empty;
        
        public string Respuesta { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? TipoIntencion { get; set; }
        
        public bool Util { get; set; } = true; // Feedback del usuario
        
        [MaxLength(200)]
        public string? PaginaOrigen { get; set; }
    }

    /// <summary>
    /// Base de conocimiento completa serializable
    /// </summary>
    public class BaseConocimiento
    {
        public string Version { get; set; } = "1.0.0";
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
        public List<ArticuloConocimiento> Articulos { get; set; } = new();
        public List<IntencionUsuario> Intenciones { get; set; } = new();
        public List<ConsejoContextual> Consejos { get; set; } = new();
        public Dictionary<string, List<string>> Sinonimos { get; set; } = new();
        public Dictionary<string, string> RutasModulos { get; set; } = new();
    }

    /// <summary>
    /// Resultado de búsqueda del asistente
    /// </summary>
    public class ResultadoBusqueda
    {
        public ArticuloConocimiento? Articulo { get; set; }
        public double Puntuacion { get; set; }
        public string? RazonCoincidencia { get; set; }
    }

    /// <summary>
    /// Respuesta del asistente
    /// </summary>
    public class RespuestaAsistente
    {
        public string Mensaje { get; set; } = string.Empty;
        public string TipoRespuesta { get; set; } = "texto"; // texto, navegacion, informe, consejo, error
        public string? RutaNavegacion { get; set; }
        public string? Icono { get; set; }
        public List<string> Sugerencias { get; set; } = new();
        public List<ArticuloConocimiento> ArticulosRelacionados { get; set; } = new();
        public bool Exito { get; set; } = true;
    }
}
