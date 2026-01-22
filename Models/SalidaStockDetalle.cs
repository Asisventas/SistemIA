using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Detalle de una salida de stock con información del producto y lote
    /// </summary>
    [Table("SalidasStockDetalle")]
    public class SalidaStockDetalle
    {
        [Key]
        public int IdSalidaStockDetalle { get; set; }

        [Required]
        public int IdSalidaStock { get; set; }

        [Required]
        public int IdProducto { get; set; }

        /// <summary>ID del lote específico (si aplica control de lotes)</summary>
        public int? IdProductoLote { get; set; }

        /// <summary>Número de lote al momento de la salida (para referencia histórica)</summary>
        [StringLength(50)]
        public string? NumeroLote { get; set; }

        /// <summary>Fecha de vencimiento del lote al momento de la salida</summary>
        public DateTime? FechaVencimientoLote { get; set; }

        /// <summary>Cantidad que sale del stock</summary>
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; }

        /// <summary>Costo unitario al momento de la salida</summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? CostoUnitario { get; set; }

        /// <summary>Observación específica de este item</summary>
        [StringLength(500)]
        public string? Observacion { get; set; }

        // ========== NAVEGACIÓN ==========
        [ForeignKey(nameof(IdSalidaStock))]
        public virtual SalidaStock SalidaStock { get; set; } = null!;

        [ForeignKey(nameof(IdProducto))]
        public virtual Producto? Producto { get; set; }

        [ForeignKey(nameof(IdProductoLote))]
        public virtual ProductoLote? ProductoLote { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        [NotMapped]
        public decimal CostoTotal => Cantidad * (CostoUnitario ?? 0);
    }
}
