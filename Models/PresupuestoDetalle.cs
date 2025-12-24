using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    public class PresupuestoDetalle
    {
        [Key]
        public int IdPresupuestoDetalle { get; set; }

        public int IdPresupuesto { get; set; }
        public Presupuesto? Presupuesto { get; set; }

        public int IdProducto { get; set; }
        public Producto? Producto { get; set; }

        [Column(TypeName = "decimal(18,4)")] public decimal Cantidad { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal PrecioUnitario { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Importe { get; set; }

        // IVA por línea
        [Column(TypeName = "decimal(18,4)")] public decimal IVA10 { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal IVA5 { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Exenta { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Grabado10 { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Grabado5 { get; set; }

        // Cambio aplicado
        [Column(TypeName = "decimal(18,4)")] public decimal? CambioDelDia { get; set; }

        public int? IdTipoIva { get; set; }
    }
}
