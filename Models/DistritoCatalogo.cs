using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemIA.Models
{
    // Tabla: distrito
    public class DistritoCatalogo
    {
        [Key]
        public int Numero { get; set; }

        [Required]
        [StringLength(120)]
        public string Nombre { get; set; } = string.Empty;

        // FK al departamento (cDep)
        [Required]
        public int Departamento { get; set; }

        // Navegación
        public DepartamentoCatalogo? DepartamentoNavigation { get; set; }
        public ICollection<CiudadCatalogo> Ciudades { get; set; } = new List<CiudadCatalogo>();
    }
}
