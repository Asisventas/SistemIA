using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Cuota de pago a proveedor (desglose de deuda)
    /// </summary>
    [Table("CuentasPorPagarCuotas")]
    public class CuentaPorPagarCuota
    {
        [Key]
        public int IdCuota { get; set; }

        public int IdCuentaPorPagar { get; set; }

        public int NumeroCuota { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoCuota { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal SaldoCuota { get; set; } // Saldo pendiente de esta cuota

        public DateTime FechaVencimiento { get; set; }

        public DateTime? FechaPago { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Pagada, Anulada

        [StringLength(280)]
        public string? Observaciones { get; set; }

        // Navegación
        [ForeignKey(nameof(IdCuentaPorPagar))]
        public CuentaPorPagar? CuentaPorPagar { get; set; }

        public ICollection<PagoProveedorDetalle> PagoDetalles { get; set; } = new List<PagoProveedorDetalle>();
    }
}
