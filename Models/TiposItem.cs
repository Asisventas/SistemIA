using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("TiposItem")]
    public class TiposItem
    {
        [Key]
        public int IdTipoItem { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        // Marca si este tipo representa gastos/egresos administrativos
        public bool EsGasto { get; set; } = false;

        // Marca si este tipo es un servicio (no requiere control de stock)
        public bool EsServicio { get; set; } = false;

        // Auditoría básica opcional
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        [StringLength(50)]
        public string? UsuarioModificacion { get; set; }
    }
}
