using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    public class AjusteStock
    {
        [Key]
        public int IdAjusteStock { get; set; }

        [Column("suc")]
        public int IdSucursal { get; set; }
        public int? IdCaja { get; set; }
        public int? Turno { get; set; }

        public DateTime FechaAjuste { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string Usuario { get; set; } = "Sistema";

        [StringLength(280)]
        public string? Comentario { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal TotalMonto { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        [StringLength(50)] public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        [StringLength(50)] public string? UsuarioModificacion { get; set; }

        // Navegación
        public ICollection<AjusteStockDetalle> Detalles { get; set; } = new List<AjusteStockDetalle>();
    }
}
