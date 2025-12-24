using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("ClientesPrecios")]
    public class ClientePrecio
    {
        [Key]
        public int IdClientePrecio { get; set; }

        [Required]
        public int IdCliente { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? PrecioFijoGs { get; set; }

        // 0 a 100 (porcentaje)
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PorcentajeDescuento { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string? UsuarioCreacion { get; set; }

        // Navegación (opcionales)
        public Cliente? Cliente { get; set; }
        public Producto? Producto { get; set; }
    }
}
