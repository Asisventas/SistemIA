using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Representa una acción o permiso específico en el sistema
    /// </summary>
    [Table("Permisos")]
    public class Permiso
    {
        [Key]
        public int IdPermiso { get; set; }

        /// <summary>
        /// Nombre del permiso (ej: "Ver", "Crear", "Editar", "Eliminar", "Exportar", "Imprimir")
        /// </summary>
        [Required, StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Código único del permiso (ej: "VIEW", "CREATE", "EDIT", "DELETE", "EXPORT", "PRINT")
        /// </summary>
        [Required, StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del permiso
        /// </summary>
        [StringLength(500)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Orden de visualización
        /// </summary>
        public int? Orden { get; set; }

        /// <summary>
        /// Si el permiso está activo
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Relación con roles y módulos
        /// </summary>
        public ICollection<RolModuloPermiso> PermisosRol { get; set; } = new List<RolModuloPermiso>();
    }
}
