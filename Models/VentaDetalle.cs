using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    public class VentaDetalle
    {
        [Key]
        public int IdVentaDetalle { get; set; }

        public int IdVenta { get; set; }
        public Venta? Venta { get; set; }

        public int IdProducto { get; set; }
        public Producto? Producto { get; set; }

        [Column(TypeName = "decimal(18,4)")] public decimal Cantidad { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal PrecioUnitario { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Importe { get; set; }

        // IVA por línea (compatibles con esquema existente en compras)
        [Column(TypeName = "decimal(18,4)")] public decimal IVA10 { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal IVA5 { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Exenta { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Grabado10 { get; set; }
        [Column(TypeName = "decimal(18,4)")] public decimal Grabado5 { get; set; }

        // Cambio por detalle (si hubo conversión)
        [Column(TypeName = "decimal(18,4)")] public decimal? CambioDelDia { get; set; }

        // Extra SIFEN item
        public int? IdTipoIva { get; set; } // catálogo TiposIva

        // ========== COSTO AL MOMENTO DE LA VENTA ==========
        /// <summary>Costo unitario del producto al momento de la venta (para cálculo de utilidad)</summary>
        [Column(TypeName = "decimal(18,4)")] public decimal? CostoUnitario { get; set; }

        // ========== FARMACIA - PRECIO MINISTERIO ==========
        /// <summary>Precio Ministerio (máximo regulado) al momento de la venta</summary>
        [Column(TypeName = "decimal(18,4)")] public decimal? PrecioMinisterio { get; set; }
        
        /// <summary>Porcentaje de descuento aplicado (calculado como (PrecioMinisterio - PrecioUnitario) / PrecioMinisterio * 100)</summary>
        [Column(TypeName = "decimal(18,4)")] public decimal? PorcentajeDescuento { get; set; }
    }
}
