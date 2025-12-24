using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SistemIA.Models
{
    // Tabla: departamento
    public class DepartamentoCatalogo
    {
        // número int NOT NULL PRIMARY KEY
        [Key]
        public int Numero { get; set; }

        // nombre varchar(100) NOT NULL
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        // Navegación
        public ICollection<DistritoCatalogo> Distritos { get; set; } = new List<DistritoCatalogo>();
        public ICollection<CiudadCatalogo> Ciudades { get; set; } = new List<CiudadCatalogo>();
    }
}
