using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Representa un módulo o sección del sistema
    /// </summary>
    [Table("Modulos")]
    public class Modulo
    {
        [Key]
        public int IdModulo { get; set; }

        /// <summary>
        /// Nombre del módulo (ej: "Ventas", "Compras", "Inventario")
        /// </summary>
        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del módulo
        /// </summary>
        [StringLength(500)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Icono Bootstrap Icons (ej: "bi-cart", "bi-box")
        /// </summary>
        [StringLength(50)]
        public string? Icono { get; set; }

        /// <summary>
        /// Orden de visualización en el menú
        /// </summary>
        public int? Orden { get; set; }

        /// <summary>
        /// Módulo padre (para jerarquía de menús)
        /// </summary>
        public int? IdModuloPadre { get; set; }

        [ForeignKey(nameof(IdModuloPadre))]
        public Modulo? ModuloPadre { get; set; }

        /// <summary>
        /// Módulos hijos
        /// </summary>
        public ICollection<Modulo> ModulosHijos { get; set; } = new List<Modulo>();

        /// <summary>
        /// Ruta de la página Blazor (ej: "/ventas", "/productos/listado")
        /// </summary>
        [StringLength(200)]
        public string? RutaPagina { get; set; }

        /// <summary>
        /// Si el módulo está activo en el sistema
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        /// <summary>
        /// Permisos asociados a este módulo
        /// </summary>
        public ICollection<RolModuloPermiso> PermisosRol { get; set; } = new List<RolModuloPermiso>();
    }
}
