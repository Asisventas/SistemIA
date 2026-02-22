using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.Gimnasio
{
    /// <summary>
    /// Membresía activa de un cliente en el gimnasio.
    /// Registra el período de suscripción, estado de pago y vínculo con ventas.
    /// </summary>
    [Table("MembresiasClientes")]
    public class MembresiaCliente
    {
        [Key]
        public int IdMembresia { get; set; }

        // ========== SUCURSAL / CAJA / TURNO ==========
        
        /// <summary>
        /// Sucursal donde se registró la membresía
        /// </summary>
        [Required]
        public int IdSucursal { get; set; }
        [ForeignKey("IdSucursal")]
        public Sucursal? Sucursal { get; set; }

        /// <summary>
        /// Caja donde se realizó el cobro de la membresía
        /// </summary>
        public int? IdCaja { get; set; }
        [ForeignKey("IdCaja")]
        public Caja? Caja { get; set; }

        /// <summary>
        /// Turno en que se registró la membresía
        /// </summary>
        public int? Turno { get; set; }

        /// <summary>
        /// Fecha de caja del registro
        /// </summary>
        public DateTime? FechaCaja { get; set; }

        // ========== CLIENTE Y MEMBRESÍA ==========
        
        /// <summary>
        /// Cliente al que pertenece la membresía
        /// </summary>
        [Required]
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }

        /// <summary>
        /// Producto de membresía contratado (Producto con EsMembresia=true)
        /// </summary>
        [Required]
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public Producto? Producto { get; set; }

        // ========== PERÍODO DE VIGENCIA ==========
        
        /// <summary>
        /// Fecha de inicio de la membresía
        /// </summary>
        [Required]
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fecha de vencimiento de la membresía
        /// </summary>
        [Required]
        public DateTime FechaVencimiento { get; set; }

        /// <summary>
        /// Fecha real de fin (puede ser diferente a vencimiento si se cancela antes)
        /// </summary>
        public DateTime? FechaFin { get; set; }

        // ========== CONGELAMIENTO ==========
        
        /// <summary>
        /// Indica si la membresía está actualmente congelada
        /// </summary>
        public bool EstaCongelada { get; set; } = false;

        /// <summary>
        /// Fecha en que se congeló (si aplica)
        /// </summary>
        public DateTime? FechaCongelamiento { get; set; }

        /// <summary>
        /// Días totales que ha sido congelada la membresía
        /// </summary>
        public int DiasCongeladosTotales { get; set; } = 0;

        /// <summary>
        /// Motivo del congelamiento
        /// </summary>
        [StringLength(200)]
        public string? MotivoCongelamiento { get; set; }

        // ========== ESTADO Y PAGO ==========
        
        /// <summary>
        /// Estado de la membresía: Activa, Vencida, Congelada, Cancelada, PendientePago
        /// </summary>
        [Required]
        [StringLength(30)]
        public string Estado { get; set; } = "Activa";

        /// <summary>
        /// Estado del pago: Pagado, Pendiente, Parcial
        /// </summary>
        [Required]
        [StringLength(20)]
        public string EstadoPago { get; set; } = "Pagado";

        /// <summary>
        /// Monto total a pagar
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoTotal { get; set; }

        /// <summary>
        /// Monto pagado hasta el momento
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoPagado { get; set; }

        /// <summary>
        /// Saldo pendiente de pago
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal SaldoPendiente { get; set; }

        // ========== VÍNCULO CON VENTAS ==========
        
        /// <summary>
        /// Venta asociada a la inscripción/renovación
        /// </summary>
        public int? IdVenta { get; set; }
        [ForeignKey("IdVenta")]
        public Venta? Venta { get; set; }

        /// <summary>
        /// Número de comprobante de la venta
        /// </summary>
        [StringLength(50)]
        public string? NumeroComprobante { get; set; }

        // ========== RENOVACIÓN ==========
        
        /// <summary>
        /// Indica si es una renovación de membresía anterior
        /// </summary>
        public bool EsRenovacion { get; set; } = false;

        /// <summary>
        /// ID de la membresía anterior (si es renovación)
        /// </summary>
        public int? IdMembresiaAnterior { get; set; }

        /// <summary>
        /// Indica si aplica renovación automática
        /// </summary>
        public bool RenovacionAutomatica { get; set; } = false;

        /// <summary>
        /// Indica si ya se envió recordatorio de vencimiento
        /// </summary>
        public bool RecordatorioEnviado { get; set; } = false;

        /// <summary>
        /// Fecha del último recordatorio enviado
        /// </summary>
        public DateTime? FechaRecordatorio { get; set; }

        // ========== USO Y ESTADÍSTICAS ==========
        
        /// <summary>
        /// Cantidad de accesos/visitas durante esta membresía
        /// </summary>
        public int CantidadVisitas { get; set; } = 0;

        /// <summary>
        /// Fecha de la última visita al gimnasio
        /// </summary>
        public DateTime? FechaUltimaVisita { get; set; }

        /// <summary>
        /// Cantidad de clases grupales utilizadas
        /// </summary>
        public int ClasesUtilizadas { get; set; } = 0;

        // ========== OBSERVACIONES ==========
        
        [StringLength(500)]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Motivo de cancelación (si aplica)
        /// </summary>
        [StringLength(200)]
        public string? MotivoCancelacion { get; set; }

        // ========== AUDITORÍA ==========
        
        public int? IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        [StringLength(50)]
        public string? UsuarioModificacion { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        /// <summary>
        /// Indica si la membresía está vigente (activa y no vencida)
        /// </summary>
        [NotMapped]
        public bool EstaVigente => Estado == "Activa" && DateTime.Now.Date <= FechaVencimiento.Date && !EstaCongelada;

        /// <summary>
        /// Días restantes de la membresía
        /// </summary>
        [NotMapped]
        public int DiasRestantes
        {
            get
            {
                if (Estado != "Activa" || EstaCongelada) return 0;
                var dias = (FechaVencimiento.Date - DateTime.Now.Date).Days;
                return Math.Max(0, dias);
            }
        }

        /// <summary>
        /// Indica si la membresía está por vencer (menos de 7 días)
        /// </summary>
        [NotMapped]
        public bool EstaPorVencer => DiasRestantes > 0 && DiasRestantes <= 7;

        /// <summary>
        /// Porcentaje de días transcurridos de la membresía
        /// </summary>
        [NotMapped]
        public int PorcentajeTranscurrido
        {
            get
            {
                if (Producto == null || !Producto.DuracionDiasMembresia.HasValue) return 0;
                var diasTotales = Producto.DuracionDiasMembresia.Value;
                var diasTranscurridos = (DateTime.Now.Date - FechaInicio.Date).Days;
                if (diasTranscurridos <= 0) return 0;
                if (diasTranscurridos >= diasTotales) return 100;
                return (int)((diasTranscurridos * 100.0) / diasTotales);
            }
        }
    }
}
