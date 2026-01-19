using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Detalle de Nota de Crédito de Compra
    /// </summary>
    public class NotaCreditoCompraDetalle
    {
        [Key]
        public int IdNotaCreditoCompraDetalle { get; set; }

        public int IdNotaCreditoCompra { get; set; }
        public NotaCreditoCompra? NotaCreditoCompra { get; set; }

        public int IdProducto { get; set; }
        public Producto? Producto { get; set; }
        
        /// <summary>
        /// Indica si el producto permite cantidades decimales (no persistido)
        /// </summary>
        [NotMapped]
        public bool PermiteDecimal { get; set; }
        
        // ========== DATOS DEL PRODUCTO (HISTÓRICO) ==========
        [MaxLength(50)]
        public string? CodigoProducto { get; set; }
        
        [MaxLength(250)]
        public string? NombreProducto { get; set; }

        // ========== CANTIDAD Y PRECIOS ==========
        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal PrecioUnitario { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal PorcentajeDescuento { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoDescuento { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal Importe { get; set; }

        // ========== IVA DESGLOSADO ==========
        /// <summary>
        /// Tasa de IVA aplicada: 10, 5 o 0
        /// </summary>
        public int TasaIVA { get; set; }
        
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

        // ========== DEPÓSITO (para devolución de stock por item) ==========
        public int? IdDepositoItem { get; set; }
        
        [ForeignKey(nameof(IdDepositoItem))]
        public Deposito? DepositoItem { get; set; }

        // ========== CAMPOS PERSISTENTES - MODO DE INGRESO PAQUETE/UNIDAD ==========
        
        /// <summary>Modo de ingreso al momento de la NC: "paquete" o "unidad"</summary>
        [MaxLength(20)] 
        public string? ModoIngresoPersistido { get; set; }
        
        /// <summary>Cantidad por paquete al momento de la NC</summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? CantidadPorPaqueteMomento { get; set; }
        
        // ========== CAMPOS NO MAPEADOS PARA UI ==========
        
        /// <summary>Cantidad por paquete (cargado desde Producto)</summary>
        [NotMapped]
        public decimal? CantidadPorPaquete { get; set; }
        
        /// <summary>Modo de ingreso actual: "paquete" o "unidad"</summary>
        [NotMapped]
        public string ModoIngreso { get; set; } = "unidad";
        
        /// <summary>Cantidad ingresada en el modo seleccionado</summary>
        [NotMapped]
        public decimal CantidadIngresada { get; set; } = 1;
        
        // ========== AUDITORÍA ==========
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        [MaxLength(100)]
        public string? UsuarioCreacion { get; set; }
        
        public DateTime? FechaModificacion { get; set; }
        
        [MaxLength(100)]
        public string? UsuarioModificacion { get; set; }
    }
}
