using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Cabecera de deuda a proveedores (similar a CuentaPorCobrar pero inversa)
    /// </summary>
    [Table("CuentasPorPagar")]
    public class CuentaPorPagar
    {
        [Key]
        public int IdCuentaPorPagar { get; set; }

        public int IdCompra { get; set; }
        public int IdProveedor { get; set; }
        public int IdSucursal { get; set; }
        public int? IdMoneda { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoTotal { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal SaldoPendiente { get; set; }

        public DateTime FechaCredito { get; set; }
        public DateTime? FechaVencimiento { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Abierta"; // Abierta, Pagada, Anulada

        public int NumeroCuotas { get; set; } = 1;
        public int PlazoDias { get; set; } = 0;

        [StringLength(280)]
        public string? Observaciones { get; set; }

        public int? IdUsuarioAutorizo { get; set; }

        // Navegación
        [ForeignKey(nameof(IdCompra))]
        public Compra? Compra { get; set; }

        [ForeignKey(nameof(IdProveedor))]
        public ProveedorSifenMejorado? Proveedor { get; set; }

        [ForeignKey(nameof(IdMoneda))]
        public Moneda? Moneda { get; set; }

        [ForeignKey(nameof(IdUsuarioAutorizo))]
        public Usuario? UsuarioAutorizo { get; set; }

        public ICollection<CuentaPorPagarCuota> Cuotas { get; set; } = new List<CuentaPorPagarCuota>();
    }
}
