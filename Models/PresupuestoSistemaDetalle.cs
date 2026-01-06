using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Items/líneas de detalle para presupuestos comerciales del sistema
    /// Permite agregar productos, servicios y conceptos personalizados
    /// </summary>
    public class PresupuestoSistemaDetalle
    {
        [Key]
        public int IdDetalle { get; set; }

        // ========== RELACIÓN CON PRESUPUESTO ==========
        public int IdPresupuesto { get; set; }
        [ForeignKey("IdPresupuesto")]
        public PresupuestoSistema? Presupuesto { get; set; }

        // ========== DESCRIPCIÓN DEL ITEM ==========
        public int NumeroLinea { get; set; } = 1;

        [Required, MaxLength(300)]
        public string Descripcion { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? DescripcionAdicional { get; set; }

        [MaxLength(50)]
        public string? Codigo { get; set; }

        // ========== CANTIDADES Y PRECIOS ==========
        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; } = 1;

        [MaxLength(20)]
        public string Unidad { get; set; } = "Unidad";

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitarioUsd { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal PrecioUnitarioGs { get; set; }

        // ========== DESCUENTO ==========
        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeDescuento { get; set; } = 0;

        // ========== TOTALES CALCULADOS ==========
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubtotalUsd { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal SubtotalGs { get; set; }

        // ========== TIPO DE ITEM ==========
        [MaxLength(30)]
        public string TipoItem { get; set; } = "Producto"; // Producto, Servicio, Hardware, Otro

        // ========== OPCIONES ==========
        public bool EsOpcional { get; set; } = false;
        public bool Incluido { get; set; } = true;

        // ========== MÉTODOS DE CÁLCULO ==========
        public void CalcularSubtotales()
        {
            var subtotalSinDescuento = Cantidad * PrecioUnitarioUsd;
            var descuento = subtotalSinDescuento * (PorcentajeDescuento / 100m);
            SubtotalUsd = subtotalSinDescuento - descuento;
            
            var subtotalGsSinDescuento = Cantidad * PrecioUnitarioGs;
            var descuentoGs = subtotalGsSinDescuento * (PorcentajeDescuento / 100m);
            SubtotalGs = subtotalGsSinDescuento - descuentoGs;
        }

        public void CalcularPrecioGs(decimal tipoCambio)
        {
            PrecioUnitarioGs = Math.Round(PrecioUnitarioUsd * tipoCambio, 0);
            CalcularSubtotales();
        }
    }
}
