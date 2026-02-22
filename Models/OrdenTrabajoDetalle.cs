using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Detalle de una Orden de Trabajo.
    /// Puede ser: Repuesto (producto), Mano de Obra, o Servicio externo.
    /// </summary>
    [Table("OrdenTrabajoDetalles")]
    public class OrdenTrabajoDetalle
    {
        [Key]
        public int IdOrdenTrabajoDetalle { get; set; }

        // ========== ORDEN DE TRABAJO ==========
        
        public int IdOrdenTrabajo { get; set; }
        public OrdenTrabajo? OrdenTrabajo { get; set; }

        // ========== TIPO DE LÍNEA ==========
        
        /// <summary>
        /// Tipo: Repuesto, ManoObra, Servicio
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string TipoLinea { get; set; } = "Repuesto";

        // ========== PRODUCTO (si TipoLinea = Repuesto) ==========
        
        public int? IdProducto { get; set; }
        public Producto? Producto { get; set; }

        /// <summary>
        /// Código del producto
        /// </summary>
        [MaxLength(50)]
        public string? CodigoProducto { get; set; }

        // ========== DESCRIPCIÓN ==========
        
        /// <summary>
        /// Descripción del item (repuesto, servicio, mano de obra)
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Observaciones específicas de esta línea
        /// </summary>
        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // ========== CANTIDADES Y PRECIOS ==========
        
        /// <summary>
        /// Cantidad (horas para ManoObra, unidades para Repuesto)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; } = 1;

        /// <summary>
        /// Precio unitario
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal PrecioUnitario { get; set; }

        /// <summary>
        /// Costo unitario (para cálculo de margen)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? CostoUnitario { get; set; }

        /// <summary>
        /// Porcentaje de descuento
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? PorcentajeDescuento { get; set; }

        /// <summary>
        /// Monto de descuento
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoDescuento { get; set; }

        /// <summary>
        /// Total de la línea (Cantidad * PrecioUnitario - MontoDescuento)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Total { get; set; }

        // ========== IVA ==========
        
        /// <summary>
        /// Tipo de IVA: 10, 5, 0 (Exenta)
        /// </summary>
        public int TipoIva { get; set; } = 10;

        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoIva10 { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoIva5 { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoExenta { get; set; }

        // ========== MANO DE OBRA (si TipoLinea = ManoObra) ==========
        
        /// <summary>
        /// Horas de mano de obra
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? HorasTrabajo { get; set; }

        /// <summary>
        /// Mecánico que realizó el trabajo
        /// </summary>
        public int? IdMecanico { get; set; }

        /// <summary>
        /// Nombre del mecánico
        /// </summary>
        [MaxLength(200)]
        public string? NombreMecanico { get; set; }

        // ========== ESTADO ==========
        
        /// <summary>
        /// Estado del item:
        /// - Pendiente: Aún no se realizó
        /// - EnProceso: En ejecución
        /// - Completado: Realizado
        /// - Cancelado: No se realizó
        /// </summary>
        [MaxLength(20)]
        public string Estado { get; set; } = "Pendiente";

        /// <summary>
        /// Fecha/hora de inicio del trabajo (para mano de obra)
        /// </summary>
        public DateTime? FechaInicio { get; set; }

        /// <summary>
        /// Fecha/hora de finalización
        /// </summary>
        public DateTime? FechaFin { get; set; }

        // ========== ORDEN DE VISUALIZACIÓN ==========
        
        public int Orden { get; set; } = 0;

        // ========== AUDITORÍA ==========
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        [NotMapped]
        public string IconoTipo => TipoLinea switch
        {
            "Repuesto" => "bi-box-seam",
            "ManoObra" => "bi-wrench",
            "Servicio" => "bi-gear",
            _ => "bi-circle"
        };

        [NotMapped]
        public string ColorTipo => TipoLinea switch
        {
            "Repuesto" => "primary",
            "ManoObra" => "warning",
            "Servicio" => "info",
            _ => "secondary"
        };

        [NotMapped]
        public string EstadoIcono => Estado switch
        {
            "Pendiente" => "⏳",
            "EnProceso" => "🔧",
            "Completado" => "✅",
            "Cancelado" => "❌",
            _ => "○"
        };
    }
}
