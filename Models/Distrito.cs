using System.ComponentModel.DataAnnotations;

namespace SistemIA.Models
{
    public class Distrito
    {
        [Key]
        public int IdDistrito { get; set; }

        [Required]
        [StringLength(120)]
        public string Nombre { get; set; } = string.Empty;

    // Relación con Ciudad
    public int? IdCiudad { get; set; }
    }
}
