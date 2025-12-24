using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Cabecera de pago a proveedores (similar a CobroCuota pero para compras)
    /// </summary>
    [Table("PagosProveedores")]
    public class PagoProveedor
    {
        [Key]
        public int IdPagoProveedor { get; set; }

        // Relaciones principales
        public int IdCompra { get; set; }
        public int IdProveedor { get; set; }
        public int IdSucursal { get; set; }
        public int? IdMoneda { get; set; }
        public int? IdCaja { get; set; }
        public int? IdUsuario { get; set; }
        
        // Turno de caja al momento del pago
        public int? Turno { get; set; }

        // Datos de pago
        public DateTime FechaPago { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoTotal { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? CambioDelDia { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Pagado"; // Pagado, Pendiente, Anulado

        [StringLength(30)]
        public string? NumeroRecibo { get; set; }

        [StringLength(280)]
        public string? Observaciones { get; set; }

        // Navegación
        [ForeignKey(nameof(IdCompra))]
        public Compra? Compra { get; set; }

        [ForeignKey(nameof(IdProveedor))]
        public ProveedorSifenMejorado? Proveedor { get; set; }

        [ForeignKey(nameof(IdMoneda))]
        public Moneda? Moneda { get; set; }

        [ForeignKey(nameof(IdCaja))]
        public Caja? Caja { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario? Usuario { get; set; }

        public ICollection<PagoProveedorDetalle> Detalles { get; set; } = new List<PagoProveedorDetalle>();
    }
}
