using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Detalle/línea de un pedido de mesa.
    /// Cada producto consumido es una línea.
    /// Sigue el mismo patrón que VentaDetalle.
    /// </summary>
    [Table("PedidoDetalles")]
    public class PedidoDetalle
    {
        [Key]
        public int IdPedidoDetalle { get; set; }

        // ========== PEDIDO PADRE ==========
        
        public int IdPedido { get; set; }
        public Pedido? Pedido { get; set; }

        // ========== PRODUCTO ==========
        
        public int IdProducto { get; set; }
        public Producto? Producto { get; set; }

        /// <summary>
        /// Código del producto (snapshot al momento del pedido)
        /// </summary>
        [MaxLength(50)]
        public string? CodigoProducto { get; set; }

        /// <summary>
        /// Descripción del producto (snapshot)
        /// </summary>
        [MaxLength(250)]
        public string Descripcion { get; set; } = string.Empty;

        // ========== CANTIDADES Y PRECIOS (mismo patrón que VentaDetalle) ==========
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; } = 1;

        /// <summary>
        /// Cantidad ya entregada/servida
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal CantidadEntregada { get; set; } = 0;

        [Column(TypeName = "decimal(18,4)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Importe { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeDescuento { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Descuento { get; set; }

        // ========== IVA (mismo patrón que VentaDetalle) ==========
        
        public int? IdTipoIva { get; set; }

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

        // ========== ESTADO Y COCINA ==========
        
        /// <summary>
        /// Estado: Pendiente, EnPreparacion, Listo, Entregado, Cancelado
        /// </summary>
        [MaxLength(20)]
        public string Estado { get; set; } = "Pendiente";

        /// <summary>
        /// Si ya se envió a cocina/barra
        /// </summary>
        public bool EnviadoCocina { get; set; } = false;

        /// <summary>
        /// Fecha/hora de envío a cocina
        /// </summary>
        public DateTime? FechaEnvioCocina { get; set; }

        /// <summary>
        /// Fecha/hora de entrega al cliente
        /// </summary>
        public DateTime? FechaEntrega { get; set; }

        /// <summary>
        /// Número de comanda/impresión (agrupa items impresos juntos)
        /// </summary>
        public int? NumeroComanda { get; set; }

        // ========== MODIFICADORES/NOTAS ==========
        
        /// <summary>
        /// Modificadores del plato (sin cebolla, extra queso, etc.)
        /// </summary>
        [MaxLength(500)]
        public string? Modificadores { get; set; }

        /// <summary>
        /// Notas especiales para cocina
        /// </summary>
        [MaxLength(500)]
        public string? NotasCocina { get; set; }

        /// <summary>
        /// Si el item es urgente (destacado en comanda)
        /// </summary>
        public bool EsUrgente { get; set; } = false;

        // ========== DIVISIÓN DE CUENTA ==========
        
        /// <summary>
        /// Identificador del comensal (para división de cuenta)
        /// </summary>
        public int? NumeroComensal { get; set; }

        /// <summary>
        /// Si este item ya fue incluido en un pago/factura
        /// </summary>
        public bool Facturado { get; set; } = false;

        /// <summary>
        /// ID de la venta donde se facturó (si aplica)
        /// </summary>
        public int? IdVenta { get; set; }

        // ========== AUDITORÍA ==========
        
        /// <summary>
        /// Usuario que agregó el item
        /// </summary>
        public int? IdUsuario { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        [NotMapped]
        public decimal CantidadPendiente => Cantidad - CantidadEntregada;

        [NotMapped]
        public bool EstaCompleto => CantidadEntregada >= Cantidad;

        [NotMapped]
        public string EstadoIcono => Estado switch
        {
            "Pendiente" => "bi-clock text-warning",
            "EnPreparacion" => "bi-fire text-danger",
            "Listo" => "bi-check-circle text-success",
            "Entregado" => "bi-check-all text-primary",
            "Cancelado" => "bi-x-circle text-muted",
            _ => "bi-question-circle"
        };
    }
}
