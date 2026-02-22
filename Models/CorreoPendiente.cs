using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Cola de correos pendientes de envío.
    /// Se utiliza cuando no hay conexión a internet para encolar y reintentar posteriormente.
    /// </summary>
    [Table("CorreosPendientes")]
    public class CorreoPendiente
    {
        [Key]
        public int IdCorreoPendiente { get; set; }

        // ========== DATOS DEL CORREO ==========
        
        /// <summary>
        /// Dirección de correo del destinatario
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Destinatario { get; set; } = "";

        /// <summary>
        /// Asunto del correo
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Asunto { get; set; } = "";

        /// <summary>
        /// Cuerpo HTML del correo (guardado comprimido para ahorrar espacio)
        /// </summary>
        [Required]
        public string CuerpoHtml { get; set; } = "";

        // ========== CONFIGURACIÓN ==========
        
        /// <summary>
        /// ID de la sucursal para obtener configuración SMTP
        /// </summary>
        public int IdSucursal { get; set; }

        /// <summary>
        /// Tipo de correo: Reserva, Factura, CierreCaja, Cortesia, Alerta, etc.
        /// </summary>
        [MaxLength(50)]
        public string TipoCorreo { get; set; } = "General";

        /// <summary>
        /// ID de referencia (IdReserva, IdVenta, etc.) para trazabilidad
        /// </summary>
        public int? IdReferencia { get; set; }

        // ========== ESTADO Y REINTENTOS ==========
        
        /// <summary>
        /// Estado: Pendiente, Enviado, Fallido, Cancelado
        /// </summary>
        [MaxLength(20)]
        public string Estado { get; set; } = "Pendiente";

        /// <summary>
        /// Número de intentos de envío realizados
        /// </summary>
        public int Intentos { get; set; } = 0;

        /// <summary>
        /// Máximo de intentos antes de marcar como Fallido
        /// </summary>
        public int MaxIntentos { get; set; } = 5;

        /// <summary>
        /// Último mensaje de error si falló
        /// </summary>
        [MaxLength(1000)]
        public string? UltimoError { get; set; }

        // ========== FECHAS ==========
        
        /// <summary>
        /// Fecha en que se encoló el correo
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        /// <summary>
        /// Fecha del último intento de envío
        /// </summary>
        public DateTime? FechaUltimoIntento { get; set; }

        /// <summary>
        /// Fecha en que se envió exitosamente
        /// </summary>
        public DateTime? FechaEnvio { get; set; }

        /// <summary>
        /// Próximo intento programado (para backoff exponencial)
        /// </summary>
        public DateTime? ProximoIntento { get; set; }

        // ========== ADJUNTOS (opcional, como JSON) ==========
        
        /// <summary>
        /// Adjuntos serializados como JSON: [{nombre, contenidoBase64, mimeType}]
        /// </summary>
        public string? AdjuntosJson { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        [NotMapped]
        public bool PuedeReintentar => Estado == "Pendiente" && Intentos < MaxIntentos;

        [NotMapped]
        public bool DebeReintentar => PuedeReintentar && (ProximoIntento == null || ProximoIntento <= DateTime.Now);
    }
}
