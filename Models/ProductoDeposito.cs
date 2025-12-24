using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("ProductosDepositos")]
    public class ProductoDeposito
    {
        [Key]
        public int IdProductoDeposito { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Required]
        public int IdDeposito { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Stock { get; set; } = 0;

        [Column(TypeName = "decimal(18,4)")]
        public decimal StockMinimo { get; set; } = 0;

        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        [StringLength(50)]
        public string? UsuarioModificacion { get; set; }

        // Navegación
        [ForeignKey(nameof(IdProducto))]
        public virtual Producto Producto { get; set; } = null!;
        [ForeignKey(nameof(IdDeposito))]
        public virtual Deposito Deposito { get; set; } = null!;
    }
}
