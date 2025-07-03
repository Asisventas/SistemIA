using System.ComponentModel.DataAnnotations;

namespace SistemIA.Models
{
    public class TipoOperacion
    {
        [Key]
        [StringLength(3)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Comentario { get; set; }

        public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    }
}
