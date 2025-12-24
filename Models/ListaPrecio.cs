using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("ListasPrecios")]
    public class ListaPrecio
    {
        [Key]
        public int IdListaPrecio { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required]
        [Display(Name = "Moneda")]
        public int IdMoneda { get; set; }

        [ForeignKey("IdMoneda")]
        public virtual Moneda Moneda { get; set; } = null!;

        [Display(Name = "Es Lista Predeterminada")]
        public bool EsPredeterminada { get; set; } = false;

        [Display(Name = "Activo")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Aplicar Descuento Global")]
        public bool AplicarDescuentoGlobal { get; set; } = false;

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Porcentaje Descuento")]
        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100%")]
        public decimal PorcentajeDescuento { get; set; } = 0;

        [Display(Name = "Fecha Vigencia Desde")]
        public DateTime? FechaVigenciaDesde { get; set; }

        [Display(Name = "Fecha Vigencia Hasta")]
        public DateTime? FechaVigenciaHasta { get; set; }

        [Display(Name = "Orden")]
        public int Orden { get; set; } = 0;

        // Propiedades de auditoría
        [Display(Name = "Fecha Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "Usuario Creación")]
        public string UsuarioCreacion { get; set; } = string.Empty;

        [Display(Name = "Fecha Modificación")]
        public DateTime? FechaModificacion { get; set; }

        [StringLength(50)]
        [Display(Name = "Usuario Modificación")]
        public string? UsuarioModificacion { get; set; }

        // Navegación
        public virtual ICollection<ListaPrecioDetalle> Detalles { get; set; } = new List<ListaPrecioDetalle>();

        // Propiedades calculadas
        [NotMapped]
        public string NombreCompleto => $"{Nombre} ({Moneda?.CodigoISO})";

        [NotMapped]
        public string EstadoDescripcion => Estado ? "Activa" : "Inactiva";

        [NotMapped]
        public bool EsVigente
        {
            get
            {
                var hoy = DateTime.Today;
                bool desdeCumple = !FechaVigenciaDesde.HasValue || FechaVigenciaDesde.Value.Date <= hoy;
                bool hastaCumple = !FechaVigenciaHasta.HasValue || FechaVigenciaHasta.Value.Date >= hoy;
                return Estado && desdeCumple && hastaCumple;
            }
        }

        [NotMapped]
        public string VigenciaDescripcion
        {
            get
            {
                if (!FechaVigenciaDesde.HasValue && !FechaVigenciaHasta.HasValue)
                    return "Sin límite de vigencia";
                
                if (FechaVigenciaDesde.HasValue && !FechaVigenciaHasta.HasValue)
                    return $"Desde {FechaVigenciaDesde.Value:dd/MM/yyyy}";
                
                if (!FechaVigenciaDesde.HasValue && FechaVigenciaHasta.HasValue)
                    return $"Hasta {FechaVigenciaHasta.Value:dd/MM/yyyy}";
                
                return $"{FechaVigenciaDesde.Value:dd/MM/yyyy} - {FechaVigenciaHasta.Value:dd/MM/yyyy}";
            }
        }
    }
}
