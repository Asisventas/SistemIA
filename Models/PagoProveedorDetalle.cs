using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Detalle de pago a proveedor (medio de pago utilizado)
    /// </summary>
    [Table("PagosProveedoresDetalles")]
    public class PagoProveedorDetalle
    {
        [Key]
        public int IdPagoProveedorDetalle { get; set; }

        public int IdPagoProveedor { get; set; }
        public int? IdCuota { get; set; }

        [StringLength(20)]
        public string MedioPago { get; set; } = "EFECTIVO"; // EFECTIVO, CHEQUE, TRANSFERENCIA, TARJETA

        [Column(TypeName = "decimal(18,4)")]
        public decimal Monto { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? CambioDelDia { get; set; }

        // Datos de medio de pago
        [StringLength(100)]
        public string? BancoCheque { get; set; }

        [StringLength(30)]
        public string? NumeroCheque { get; set; }

        public DateTime? FechaCobroCheque { get; set; }

        [StringLength(100)]
        public string? BancoTransferencia { get; set; }

        [StringLength(100)]
        public string? NumeroTransferencia { get; set; }

        [StringLength(100)]
        public string? BancoTarjeta { get; set; }

        [StringLength(50)]
        public string? MarcaTarjeta { get; set; }

        [StringLength(4)]
        public string? Ultimos4Tarjeta { get; set; }

        [StringLength(50)]
        public string? NumeroAutorizacion { get; set; }

        [StringLength(280)]
        public string? Observaciones { get; set; }

        // Navegación
        [ForeignKey(nameof(IdPagoProveedor))]
        public PagoProveedor? PagoProveedor { get; set; }

        [ForeignKey(nameof(IdCuota))]
        public CuentaPorPagarCuota? Cuota { get; set; }
    }
}
