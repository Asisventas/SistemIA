using System.ComponentModel.DataAnnotations;

namespace SistemIA.Models
{
    // Tabla: ciudad
    public class CiudadCatalogo
    {
        [Key]
        public int Numero { get; set; }

        [Required]
    [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // FK a departamento y distrito
        [Required]
        public int Departamento { get; set; }

        [Required]
        public int Distrito { get; set; }

        // Navegación
        public DepartamentoCatalogo? DepartamentoNavigation { get; set; }
        public DistritoCatalogo? DistritoNavigation { get; set; }
    }
}
