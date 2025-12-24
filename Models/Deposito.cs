using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("Depositos")]
    public class Deposito
    {
        [Key]
        public int IdDeposito { get; set; }

        [Required]
        [StringLength(120)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descripcion { get; set; }

        // Relación con Sucursal
        [Column("suc")]
        [Display(Name = "Sucursal")]
        public int IdSucursal { get; set; }

        [ForeignKey(nameof(IdSucursal))]
        public virtual Sucursal Sucursal { get; set; } = null!;

        public bool Activo { get; set; } = true;

        // Auditoría
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        [StringLength(50)]
        public string? UsuarioModificacion { get; set; }

        // Navegación
        public ICollection<ProductoDeposito>? Productos { get; set; }
    }
}
