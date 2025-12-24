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

        // Navegación
        [ForeignKey(nameof(IdTransferencia))]
        public virtual TransferenciaDeposito Transferencia { get; set; } = null!;

        [ForeignKey(nameof(IdProducto))]
        public virtual Producto? Producto { get; set; }
    }
}
