using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    public class AjusteStockDetalle
    {
        [Key]
        public int IdAjusteStockDetalle { get; set; }

        public int IdAjusteStock { get; set; }
        public int IdProducto { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal StockAjuste { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal StockSistema { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Diferencia { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal Monto { get; set; }

        public DateTime FechaAjuste { get; set; } = DateTime.Now;

        [Column("suc")]
        public int IdSucursal { get; set; }
        public int? IdCaja { get; set; }
        public int? Turno { get; set; }

        [StringLength(50)]
        public string Usuario { get; set; } = "Sistema";

        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        [StringLength(50)] public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        [StringLength(50)] public string? UsuarioModificacion { get; set; }

        // Navegación
        public AjusteStock? Ajuste { get; set; }
        public Producto? Producto { get; set; }
    }
}
