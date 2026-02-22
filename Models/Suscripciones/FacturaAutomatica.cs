using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.Suscripciones
{
    /// <summary>
    /// Registro de cada factura generada automáticamente por el sistema de suscripciones.
    /// Permite hacer seguimiento del estado de cada factura.
    /// </summary>
    public class FacturaAutomatica
    {
        [Key]
        public int IdFacturaAutomatica { get; set; }

        // ========== RELACIONES ==========
        public int IdSuscripcion { get; set; }
        [ForeignKey("IdSuscripcion")]
        public SuscripcionCliente? Suscripcion { get; set; }

        public int IdSucursal { get; set; }
        [ForeignKey("IdSucursal")]
        public Sucursal? Sucursal { get; set; }

        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }

        /// <summary>
        /// Venta generada (factura)
        /// </summary>
        public int? IdVenta { get; set; }
        [ForeignKey("IdVenta")]
        public Venta? Venta { get; set; }

        // ========== DATOS DE FACTURACIÓN ==========
        /// <summary>
        /// Período facturado: "Enero 2026", "Febrero 2026", etc.
        /// </summary>
        [MaxLength(50)]
        public string PeriodoFacturado { get; set; } = "";

        /// <summary>
        /// Fecha de inicio del período
        /// </summary>
        public DateTime FechaInicioPeriodo { get; set; }

        /// <summary>
        /// Fecha de fin del período
        /// </summary>
        public DateTime FechaFinPeriodo { get; set; }

        /// <summary>
        /// Monto facturado
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoFacturado { get; set; }

        // ========== ESTADO DE LA FACTURA ==========
        /// <summary>
        /// Estado: Pendiente, Generada, ErrorGeneracion, Anulada
        /// </summary>
        [MaxLength(30)]
        public string EstadoFactura { get; set; } = "Pendiente";

        /// <summary>
        /// Mensaje de error si falló la generación
        /// </summary>
        [MaxLength(1000)]
        public string? MensajeError { get; set; }

        /// <summary>
        /// Intentos de generación
        /// </summary>
        public int IntentosGeneracion { get; set; } = 0;

        // ========== ESTADO DEL CORREO ==========
        /// <summary>
        /// Estado del envío de correo: NoEnviado, Enviado, Error, NoAplica
        /// </summary>
        [MaxLength(30)]
        public string EstadoCorreoFactura { get; set; } = "NoEnviado";

        public DateTime? FechaEnvioCorreoFactura { get; set; }

        [MaxLength(500)]
        public string? ErrorCorreoFactura { get; set; }

        public int IntentosEnvioCorreo { get; set; } = 0;

        // ========== ESTADO DE COBRO ==========
        /// <summary>
        /// Estado de cobro: Pendiente, Parcial, Cobrado
        /// </summary>
        [MaxLength(20)]
        public string EstadoCobro { get; set; } = "Pendiente";

        public DateTime? FechaCobro { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoCobrado { get; set; } = 0;

        // ========== RECIBO ==========
        /// <summary>
        /// Estado del envío de recibo: NoEnviado, Enviado, Error, NoAplica
        /// </summary>
        [MaxLength(30)]
        public string EstadoCorreoRecibo { get; set; } = "NoEnviado";

        public DateTime? FechaEnvioCorreoRecibo { get; set; }

        [MaxLength(500)]
        public string? ErrorCorreoRecibo { get; set; }

        // ========== FECHAS ==========
        /// <summary>
        /// Fecha programada para generar la factura
        /// </summary>
        public DateTime FechaProgramada { get; set; }

        /// <summary>
        /// Fecha real de generación
        /// </summary>
        public DateTime? FechaGeneracion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // ========== DATOS DE REFERENCIA ==========
        /// <summary>
        /// Número de factura generada (para búsqueda rápida)
        /// </summary>
        [MaxLength(20)]
        public string? NumeroFactura { get; set; }

        /// <summary>
        /// CDC de SIFEN si aplica
        /// </summary>
        [MaxLength(64)]
        public string? CDC { get; set; }

        /// <summary>
        /// Estado SIFEN: NoAplica, Pendiente, Enviado, Aceptado, Rechazado
        /// </summary>
        [MaxLength(30)]
        public string EstadoSifen { get; set; } = "NoAplica";

        // ========== MÉTODOS AUXILIARES ==========
        [NotMapped]
        public string EstadoGeneral
        {
            get
            {
                if (EstadoFactura == "ErrorGeneracion") return "Error";
                if (EstadoFactura == "Anulada") return "Anulada";
                if (EstadoCobro == "Cobrado") return "Cobrado";
                if (EstadoCobro == "Parcial") return "Parcial";
                if (EstadoFactura == "Generada") return "Facturado";
                return "Pendiente";
            }
        }

        [NotMapped]
        public string ColorEstado
        {
            get
            {
                return EstadoGeneral switch
                {
                    "Error" => "danger",
                    "Anulada" => "secondary",
                    "Cobrado" => "success",
                    "Parcial" => "warning",
                    "Facturado" => "info",
                    _ => "secondary"
                };
            }
        }

        [NotMapped]
        public string IconoEstado
        {
            get
            {
                return EstadoGeneral switch
                {
                    "Error" => "bi-exclamation-triangle",
                    "Anulada" => "bi-x-circle",
                    "Cobrado" => "bi-check-circle",
                    "Parcial" => "bi-clock-history",
                    "Facturado" => "bi-file-earmark-check",
                    _ => "bi-hourglass"
                };
            }
        }
    }
}
