using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("TransferenciasDepositoDetalle")]
    public class TransferenciaDepositoDetalle
    {
        [Key]
        public int IdTransferenciaDetalle { get; set; }

        [Required]
        public int IdTransferencia { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostoUnitario { get; set; }
        
        // ========== SOPORTE PARA LOTES ==========
        /// <summary>
        /// Lote de origen que se transfiere (null = producto sin control de lotes)
        /// </summary>
        public int? IdProductoLoteOrigen { get; set; }
        
        /// <summary>
        /// Lote de destino donde se registra (puede ser el mismo, nuevo o null)
        /// </summary>
        public int? IdProductoLoteDestino { get; set; }
        
        /// <summary>
        /// Número de lote (para referencia rápida)
        /// </summary>
        [MaxLength(50)]
        public string? NumeroLote { get; set; }
        
        /// <summary>
        /// Fecha de vencimiento del lote (para referencia)
        /// </summary>
        public DateTime? FechaVencimientoLote { get; set; }

        // Navegación
        [ForeignKey(nameof(IdTransferencia))]
        public virtual TransferenciaDeposito Transferencia { get; set; } = null!;

        [ForeignKey(nameof(IdProducto))]
        public virtual Producto? Producto { get; set; }
        
        [ForeignKey(nameof(IdProductoLoteOrigen))]
        public virtual ProductoLote? LoteOrigen { get; set; }
        
        [ForeignKey(nameof(IdProductoLoteDestino))]
        public virtual ProductoLote? LoteDestino { get; set; }
    }
}
