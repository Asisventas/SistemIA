using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Registra todos los movimientos de stock por lote para trazabilidad completa.
    /// </summary>
    [Table("MovimientosLotes")]
    public class MovimientoLote
    {
        [Key]
        public int IdMovimientoLote { get; set; }

        [Required]
        public int IdProductoLote { get; set; }

        /// <summary>Tipo de movimiento: Entrada, Salida, Ajuste, Transferencia, Devolucion</summary>
        [Required]
        [StringLength(30)]
        [Display(Name = "Tipo")]
        public string TipoMovimiento { get; set; } = string.Empty;

        /// <summary>Cantidad del movimiento (positivo para entradas, negativo para salidas)</summary>
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Cantidad")]
        public decimal Cantidad { get; set; }

        /// <summary>Stock del lote ANTES del movimiento</summary>
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Stock Anterior")]
        public decimal StockAnterior { get; set; }

        /// <summary>Stock del lote DESPUÉS del movimiento</summary>
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Stock Posterior")]
        public decimal StockPosterior { get; set; }

        /// <summary>Tipo de documento origen: Venta, Compra, AjusteStock, Transferencia, NotaCredito, etc.</summary>
        [StringLength(50)]
        [Display(Name = "Tipo Documento")]
        public string? TipoDocumento { get; set; }

        /// <summary>ID del documento origen (IdVenta, IdCompra, etc.)</summary>
        public int? IdDocumento { get; set; }

        /// <summary>ID del detalle del documento (si aplica)</summary>
        public int? IdDocumentoDetalle { get; set; }

        /// <summary>Referencia legible del documento (ej: "001-001-0000123")</summary>
        [StringLength(100)]
        [Display(Name = "Referencia")]
        public string? ReferenciaDocumento { get; set; }

        /// <summary>Motivo o descripción del movimiento</summary>
        [StringLength(500)]
        [Display(Name = "Motivo")]
        public string? Motivo { get; set; }

        /// <summary>Fecha y hora del movimiento</summary>
        [Display(Name = "Fecha")]
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        /// <summary>Usuario que realizó el movimiento</summary>
        [StringLength(50)]
        [Display(Name = "Usuario")]
        public string? Usuario { get; set; }

        /// <summary>ID del lote destino (para transferencias entre lotes)</summary>
        public int? IdProductoLoteDestino { get; set; }

        // ========== NAVEGACIÓN ==========
        [ForeignKey(nameof(IdProductoLote))]
        public virtual ProductoLote ProductoLote { get; set; } = null!;

        [ForeignKey(nameof(IdProductoLoteDestino))]
        public virtual ProductoLote? ProductoLoteDestino { get; set; }
    }

    /// <summary>
    /// Tipos de movimiento de lote
    /// </summary>
    public static class TipoMovimientoLote
    {
        public const string Entrada = "Entrada";
        public const string Salida = "Salida";
        public const string Ajuste = "Ajuste";
        public const string Transferencia = "Transferencia";
        public const string Devolucion = "Devolucion";
        public const string Inicial = "Inicial";
        public const string Vencimiento = "Vencimiento";
    }
}
