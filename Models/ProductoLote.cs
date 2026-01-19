using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Representa un lote de producto con su vencimiento y stock.
    /// Permite el control FEFO (First Expired, First Out) para farmacias.
    /// </summary>
    [Table("ProductosLotes")]
    public class ProductoLote
    {
        [Key]
        public int IdProductoLote { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Required]
        public int IdDeposito { get; set; }

        /// <summary>Número o código del lote (ej: "L2026001", "ABC123")</summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "Número de Lote")]
        public string NumeroLote { get; set; } = string.Empty;

        /// <summary>Fecha de vencimiento del lote</summary>
        [Display(Name = "Fecha de Vencimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaVencimiento { get; set; }

        /// <summary>Fecha de fabricación (opcional)</summary>
        [Display(Name = "Fecha de Fabricación")]
        [DataType(DataType.Date)]
        public DateTime? FechaFabricacion { get; set; }

        /// <summary>Stock actual de este lote específico</summary>
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Stock")]
        public decimal Stock { get; set; } = 0;

        /// <summary>Stock inicial cuando se creó/ingresó el lote</summary>
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Stock Inicial")]
        public decimal StockInicial { get; set; } = 0;

        /// <summary>Costo unitario de adquisición del lote</summary>
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Costo Unitario")]
        public decimal? CostoUnitario { get; set; }

        /// <summary>ID de la compra que originó este lote (si aplica)</summary>
        public int? IdCompra { get; set; }

        /// <summary>ID del detalle de compra específico</summary>
        public int? IdCompraDetalle { get; set; }

        /// <summary>Fecha en que el lote ingresó al sistema</summary>
        [Display(Name = "Fecha de Ingreso")]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        /// <summary>Estado del lote: Activo, Agotado, Vencido, Bloqueado</summary>
        [Required]
        [StringLength(20)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Activo";

        /// <summary>Observaciones o notas sobre el lote</summary>
        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observacion { get; set; }

        /// <summary>Indica si es un lote creado por migración de stock existente</summary>
        public bool EsLoteInicial { get; set; } = false;

        // ========== AUDITORÍA ==========
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        [StringLength(50)]
        public string? UsuarioModificacion { get; set; }

        // ========== NAVEGACIÓN ==========
        [ForeignKey(nameof(IdProducto))]
        public virtual Producto Producto { get; set; } = null!;

        [ForeignKey(nameof(IdDeposito))]
        public virtual Deposito Deposito { get; set; } = null!;

        [ForeignKey(nameof(IdCompra))]
        public virtual Compra? Compra { get; set; }

        public virtual ICollection<MovimientoLote>? Movimientos { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========

        /// <summary>Indica si el lote está vencido</summary>
        [NotMapped]
        public bool EstaVencido => FechaVencimiento.HasValue && FechaVencimiento.Value.Date < DateTime.Today;

        /// <summary>Indica si el lote está próximo a vencer (30 días por defecto)</summary>
        [NotMapped]
        public bool EstaProximoAVencer => FechaVencimiento.HasValue 
            && FechaVencimiento.Value.Date >= DateTime.Today 
            && FechaVencimiento.Value.Date <= DateTime.Today.AddDays(30);

        /// <summary>Días para el vencimiento (negativo si ya venció)</summary>
        [NotMapped]
        public int? DiasParaVencimiento => FechaVencimiento.HasValue 
            ? (int)(FechaVencimiento.Value.Date - DateTime.Today).TotalDays 
            : null;

        /// <summary>Indica si hay stock disponible para venta</summary>
        [NotMapped]
        public bool TieneStockDisponible => Stock > 0 && Estado == "Activo" && !EstaVencido;
    }
}
