using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Descuentos automáticos por Marca o Clasificación.
    /// Se aplican automáticamente en ventas según la configuración.
    /// </summary>
    [Table("DescuentosCategorias")]
    public class DescuentoCategoria
    {
        [Key]
        public int IdDescuentoCategoria { get; set; }

        /// <summary>
        /// Tipo de categoría: "MARCA", "CLASIFICACION", "TODOS"
        /// </summary>
        [Required]
        [StringLength(20)]
        public string TipoCategoria { get; set; } = "TODOS";

        /// <summary>
        /// ID de la marca (si TipoCategoria = "MARCA")
        /// </summary>
        public int? IdMarca { get; set; }

        [ForeignKey(nameof(IdMarca))]
        public virtual Marca? Marca { get; set; }

        /// <summary>
        /// ID de la clasificación (si TipoCategoria = "CLASIFICACION")
        /// </summary>
        public int? IdClasificacion { get; set; }

        [ForeignKey(nameof(IdClasificacion))]
        public virtual Clasificacion? Clasificacion { get; set; }

        /// <summary>
        /// Porcentaje de descuento a aplicar (0-100)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        [Display(Name = "Descuento (%)")]
        public decimal PorcentajeDescuento { get; set; }

        /// <summary>
        /// Si está activo, se aplica el descuento automáticamente
        /// </summary>
        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Prioridad para resolver conflictos (mayor número = mayor prioridad)
        /// </summary>
        [Display(Name = "Prioridad")]
        public int Prioridad { get; set; } = 0;

        /// <summary>
        /// Porcentaje adicional que el cajero puede modificar sobre el descuento base.
        /// Ejemplo: Si descuento base es 10% y margen adicional es 5%, el cajero puede aplicar entre 10% y 15%.
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        [Display(Name = "Margen adicional cajero (%)")]
        public decimal MargenAdicionalCajero { get; set; } = 0;

        // ========== AUDITORÍA ==========
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        public DateTime? FechaModificacion { get; set; }
        
        [StringLength(50)]
        public string? UsuarioCreacion { get; set; }
        
        [StringLength(50)]
        public string? UsuarioModificacion { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        [NotMapped]
        public string NombreCategoria => TipoCategoria switch
        {
            "MARCA" => Marca?.NombreMarca ?? "(Sin marca)",
            "CLASIFICACION" => Clasificacion?.Nombre ?? "(Sin clasificación)",
            "TODOS" => "Todos los productos",
            _ => TipoCategoria
        };
    }
}
