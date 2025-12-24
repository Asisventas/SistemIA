using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Detalle de medios de pago utilizados en un cobro (efectivo, tarjeta, cheque, etc.)
    /// </summary>
    public class CobroDetalle
    {
        [Key]
        public int IdCobroDetalle { get; set; }

        // Cobro al que pertenece
        [Required]
        public int IdCobro { get; set; }
        public CobroCuota? CobroCuota { get; set; }

        // Cuota específica que se está pagando (puede ser pago parcial)
        public int? IdCuota { get; set; }
        public CuentaPorCobrarCuota? Cuota { get; set; }

        // Medio de pago
        [Required]
        [MaxLength(20)]
        public string MedioPago { get; set; } = "EFECTIVO"; // EFECTIVO, TARJETA, CHEQUE, TRANSFERENCIA, QR

        [Column(TypeName = "decimal(18,4)")]
        public decimal Monto { get; set; }

        // Moneda del pago
        public int? IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal? CambioDelDia { get; set; } // Tipo de cambio usado si el pago es en moneda diferente
        // Datos específicos según medio de pago
        [MaxLength(50)]
        public string? BancoTarjeta { get; set; }

        [MaxLength(4)]
        public string? Ultimos4Tarjeta { get; set; }

        [MaxLength(50)]
        public string? NumeroAutorizacion { get; set; }

        [MaxLength(50)]
        public string? NumeroCheque { get; set; }

        [MaxLength(50)]
        public string? BancoCheque { get; set; }

        [MaxLength(50)]
        public string? NumeroTransferencia { get; set; }

        [MaxLength(280)]
        public string? Observaciones { get; set; }
    }
}
