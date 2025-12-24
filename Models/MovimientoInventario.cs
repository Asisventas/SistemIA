using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("MovimientosInventario")]
    public class MovimientoInventario
    {
        [Key]
        public int IdMovimiento { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Required]
        public int IdDeposito { get; set; }

        // 1=Entrada, 2=Salida
        [Required]
        public int Tipo { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; }

        [StringLength(250)]
        public string? Motivo { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string? Usuario { get; set; }

        // Navegación
        [ForeignKey(nameof(IdProducto))]
        public virtual Producto Producto { get; set; } = null!;
        [ForeignKey(nameof(IdDeposito))]
        public virtual Deposito Deposito { get; set; } = null!;
    }
}
