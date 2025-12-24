using System.ComponentModel.DataAnnotations;

namespace SistemIA.Models
{
    public class TipoPago
    {
        [Key]
        public int IdTipoPago { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty; // Contado, Crédito, Remisión, etc.

        public bool EsCredito { get; set; } = false; // Si requiere saldo/plazo

        public bool Activo { get; set; } = true;

        public int Orden { get; set; } = 0;
    }
}
