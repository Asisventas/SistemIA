using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Detalle de Nota de Crédito de Venta
    /// </summary>
    public class NotaCreditoVentaDetalle
    {
        [Key]
        public int IdNotaCreditoDetalle { get; set; }
        
        // Alias para compatibilidad
        [NotMapped]
        public int IdNotaCreditoVentaDetalle => IdNotaCreditoDetalle;

        public int IdNotaCredito { get; set; }
        
        // Alias para compatibilidad
        [NotMapped]
        public int IdNotaCreditoVenta
        {
            get => IdNotaCredito;
            set => IdNotaCredito = value;
        }
        
        public NotaCreditoVenta? NotaCredito { get; set; }

        public int IdProducto { get; set; }
        public Producto? Producto { get; set; }
        
        // Código y nombre de producto (para histórico)
        [MaxLength(50)]
        public string? CodigoProducto { get; set; }
        
        [MaxLength(250)]
        public string? NombreProducto { get; set; }

        // Cantidad y precios
        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal PrecioUnitario { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal PorcentajeDescuento { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoDescuento { get; set; }
        
        // Alias para Descuento
        [NotMapped]
        public decimal Descuento
        {
            get => MontoDescuento;
            set => MontoDescuento = value;
        }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal Importe { get; set; }

        // Tasa de IVA (10, 5, 0)
        public int TasaIVA { get; set; }
        
        // IVA por línea
        [Column(TypeName = "decimal(18,4)")]
        public decimal IVA10 { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal IVA5 { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal Exenta { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal Grabado10 { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal Grabado5 { get; set; }

        // Cambio por detalle
        [Column(TypeName = "decimal(18,4)")]
        public decimal? CambioDelDia { get; set; }

        // Tipo IVA (catálogo)
        public int? IdTipoIva { get; set; }

        // Depósito origen (si aplica)
        public int? IdDeposito { get; set; }
        public Deposito? Deposito { get; set; }

        // Lote (si aplica)
        [MaxLength(50)]
        public string? Lote { get; set; }
    }
}
