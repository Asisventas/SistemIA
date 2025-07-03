using System.ComponentModel.DataAnnotations;

namespace SistemIA.Models
{
    public class Paises
    {
        [Key]
        [StringLength(3)]
        public string CodigoPais { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = "";

        public ICollection<Cliente>? Clientes { get; set; }
    }
}