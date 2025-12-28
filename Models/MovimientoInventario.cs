using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("MovimientosInventario")]
    public class MovimientoInventario
    {
        [Key]
        public int IdMovimiento { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Required]
        public int IdDeposito { get; set; }

        // 1=Entrada, 2=Salida
        [Required]
        public int Tipo { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; }

        // Campos de trazabilidad para auditoría completa
        [Column(TypeName = "decimal(18,4)")]
        public decimal? CantidadAnterior { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? SaldoPosterior { get; set; }

        [StringLength(250)]
        public string? Motivo { get; set; }

        // Fecha del equipo/sistema cuando se registró
        public DateTime Fecha { get; set; } = DateTime.Now;

        // Fecha de caja (fecha contable)
        public DateTime? FechaCaja { get; set; }

        // Turno de caja
        public int? Turno { get; set; }

        // Sucursal donde se realizó el movimiento
        public int? IdSucursal { get; set; }

        // Caja donde se realizó el movimiento (si aplica)
        public int? IdCaja { get; set; }

        [StringLength(50)]
        public string? Usuario { get; set; }

        // Datos de valorización al momento del movimiento
        [Column(TypeName = "decimal(18,4)")]
        public decimal? PrecioCosto { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? PrecioVenta { get; set; }

        public int? IdMoneda { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal? TipoCambio { get; set; }

        // Valores convertidos a Guaraníes para sumatoria
        [Column(TypeName = "decimal(18,4)")]
        public decimal? PrecioCostoGs { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? PrecioVentaGs { get; set; }

        // Navegación
        [ForeignKey(nameof(IdProducto))]
        public virtual Producto Producto { get; set; } = null!;

        [ForeignKey(nameof(IdDeposito))]
        public virtual Deposito Deposito { get; set; } = null!;

        [ForeignKey(nameof(IdSucursal))]
        public virtual Sucursal? Sucursal { get; set; }

        [ForeignKey(nameof(IdCaja))]
        public virtual Caja? Caja { get; set; }

        [ForeignKey(nameof(IdMoneda))]
        public virtual Moneda? Moneda { get; set; }
    }
}
