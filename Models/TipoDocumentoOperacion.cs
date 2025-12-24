using System.ComponentModel.DataAnnotations;

namespace SistemIA.Models
{
    public class TipoDocumentoOperacion
    {
        [Key]
        public int IdTipoDocumentoOperacion { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty; // FACTURA, NOTA DE CRÉDITO, etc.

        public bool Activo { get; set; } = true;

        public int Orden { get; set; } = 0;
    }
}
