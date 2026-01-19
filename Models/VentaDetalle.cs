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
        
        // ========== CAMPOS PERSISTENTES - MODO DE INGRESO PAQUETE/UNIDAD ==========
        
        /// <summary>Modo de ingreso al momento de la venta: "paquete" o "unidad" (persistido para visualización posterior)</summary>
        [MaxLength(20)] public string? ModoIngresoPersistido { get; set; }
        
        /// <summary>Cantidad por paquete al momento de la venta (para mostrar correctamente cuántas cajas se vendieron)</summary>
        [Column(TypeName = "decimal(18,4)")] public decimal? CantidadPorPaqueteMomento { get; set; }
        
        /// <summary>Precio por paquete al momento de la venta (para mostrar en reportes/impresiones)</summary>
        [Column(TypeName = "decimal(18,4)")] public decimal? PrecioPaqueteMomento { get; set; }
        
        /// <summary>Precio Ministerio por paquete al momento de la venta (para farmacias)</summary>
        [Column(TypeName = "decimal(18,4)")] public decimal? PrecioMinisterioPaqueteMomento { get; set; }
        
        // ========== CONTROL DE LOTES (FARMACIA - FEFO) ==========
        
        /// <summary>ID del lote del cual se descontó el stock (para trazabilidad FEFO)</summary>
        public int? IdProductoLote { get; set; }
        
        /// <summary>Número de lote al momento de la venta (persistido para impresión/trazabilidad)</summary>
        [MaxLength(50)]
        public string? NumeroLoteMomento { get; set; }
        
        /// <summary>Fecha de vencimiento del lote al momento de la venta</summary>
        public DateTime? FechaVencimientoLoteMomento { get; set; }
        
        // Navegación al lote
        [ForeignKey(nameof(IdProductoLote))]
        public virtual ProductoLote? ProductoLote { get; set; }
        
        // ========== CAMPOS AUXILIARES (NO MAPEADOS) ==========
        
        // Para productos vendidos por paquete/caja (cargado desde Producto al editar)
        [NotMapped]
        public decimal? CantidadPorPaquete { get; set; }
        
        [NotMapped]
        public bool PermiteVentaPorUnidad { get; set; } = true;
        
        // Modo de ingreso actual: "paquete" o "unidad" (para UI en tiempo real)
        [NotMapped]
        public string ModoIngreso { get; set; } = "unidad";
        
        // Cantidad ingresada en el modo seleccionado (antes de convertir a unidades)
        [NotMapped]
        public decimal CantidadIngresada { get; set; } = 1;
        
        // Si el producto permite cantidad decimal
        [NotMapped]
        public bool PermiteDecimal { get; set; }
    }
}
