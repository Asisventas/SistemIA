using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Detalle de productos en una remisión interna
    /// </summary>
    public class RemisionInternaDetalle
    {
        [Key]
        public int IdRemisionDetalle { get; set; }

        // Remisión a la que pertenece
        [Required]
        public int IdRemision { get; set; }
        public RemisionInterna? RemisionInterna { get; set; }

        // Producto
        [Required]
        public int IdProducto { get; set; }
        public Producto? Producto { get; set; }

        // Datos del detalle
        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Subtotal { get; set; }

        // IVA (5%, 10%, Exento)
        [Column(TypeName = "decimal(18,4)")]
        public decimal Gravado5 { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Gravado10 { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Exenta { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal IVA5 { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal IVA10 { get; set; }

        [MaxLength(280)]
        public string? Observaciones { get; set; }
    }
}
