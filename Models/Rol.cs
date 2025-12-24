using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SistemIA.Models
{

    [Table("Rol")]
    public class Rol
    {
        [Key]
        public int Id_Rol { get; set; }

        [Required]
        public string NombreRol { get; set; } = null!;

        public string? Descripcion { get; set; }

        public bool Estado { get; set; } = true;

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        
        public ICollection<RolModuloPermiso> PermisosModulos { get; set; } = new List<RolModuloPermiso>();
    }
}
