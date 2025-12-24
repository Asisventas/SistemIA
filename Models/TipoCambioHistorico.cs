using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Modelo para almacenar el historial de tipos de cambio
    /// </summary>
    [Table("TiposCambioHistorico")]
    public class TipoCambioHistorico
    {
        [Key]
        public int IdHistorial { get; set; }

        [Required]
        public int IdMonedaOrigen { get; set; }

        [Required]
        public int IdMonedaDestino { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal TasaCambio { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? TasaCompra { get; set; }

        [Required]
        public DateTime FechaRegistro { get; set; }

        [StringLength(100)]
        public string? Fuente { get; set; }

        [StringLength(100)]
        public string? UsuarioCreacion { get; set; }

        // Navigation properties
        [ForeignKey("IdMonedaOrigen")]
        public virtual Moneda? MonedaOrigen { get; set; }

        [ForeignKey("IdMonedaDestino")]
        public virtual Moneda? MonedaDestino { get; set; }
    }
}
