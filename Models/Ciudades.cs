using System.ComponentModel.DataAnnotations;

namespace SistemIA.Models
{
    public class Ciudades
    {
        [Key]
        public int IdCiudad { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = "";

        [StringLength(100)]
        public string? Departamento { get; set; }

        public ICollection<Cliente>? Clientes { get; set; }

        // Debe existir esta propiedad de navegación
        public ICollection<Sucursal> Sucursales { get; set; } = new List<Sucursal>();
    }
}