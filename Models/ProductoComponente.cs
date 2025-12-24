using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("ProductosComponentes")]
    public class ProductoComponente
    {
        [Key]
        public int IdProductoComponente { get; set; }

        // Producto combo padre
        public int IdProducto { get; set; }
        [ForeignKey(nameof(IdProducto))]
        public Producto Producto { get; set; } = null!;

        // Producto componente
        public int IdComponente { get; set; }
        [ForeignKey(nameof(IdComponente))]
        public Producto Componente { get; set; } = null!;

        // Cantidad a descontar por unidad de combo vendida
        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; }

        // Unidad mostrada (informativa), se toma del componente real
        [StringLength(20)]
        public string? UnidadMedida { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        [StringLength(50)] public string? UsuarioCreacion { get; set; }
        [StringLength(50)] public string? UsuarioModificacion { get; set; }
    }
}
