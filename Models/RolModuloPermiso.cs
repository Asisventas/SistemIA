using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Tabla intermedia que relaciona Roles, Módulos y Permisos
    /// Define qué permisos tiene cada rol sobre cada módulo
    /// </summary>
    [Table("RolesModulosPermisos")]
    public class RolModuloPermiso
    {
        [Key]
        public int IdRolModuloPermiso { get; set; }

        /// <summary>
        /// ID del rol
        /// </summary>
        [Required]
        public int IdRol { get; set; }

        [ForeignKey(nameof(IdRol))]
        public Rol Rol { get; set; } = null!;

        /// <summary>
        /// ID del módulo
        /// </summary>
        [Required]
        public int IdModulo { get; set; }

        [ForeignKey(nameof(IdModulo))]
        public Modulo Modulo { get; set; } = null!;

        /// <summary>
        /// ID del permiso
        /// </summary>
        [Required]
        public int IdPermiso { get; set; }

        [ForeignKey(nameof(IdPermiso))]
        public Permiso Permiso { get; set; } = null!;

        /// <summary>
        /// Si el permiso está concedido (true) o denegado (false)
        /// </summary>
        public bool Concedido { get; set; } = true;

        /// <summary>
        /// Fecha de asignación del permiso
        /// </summary>
        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        /// <summary>
        /// Usuario que asignó el permiso
        /// </summary>
        [StringLength(100)]
        public string? UsuarioAsignacion { get; set; }
    }
}
