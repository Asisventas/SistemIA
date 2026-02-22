using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Registro de pagos parciales o totales de un pedido.
    /// Permite dividir la cuenta entre varios clientes/pagos.
    /// Similar a VentaPago pero adaptado para pedidos de mesa.
    /// </summary>
    [Table("PedidoPagos")]
    public class PedidoPago
    {
        [Key]
        public int IdPedidoPago { get; set; }

        // ========== PEDIDO ==========
        
        public int IdPedido { get; set; }
        public Pedido? Pedido { get; set; }

        // ========== MONTO Y FORMA DE PAGO (mismo patrón que Venta) ==========
        
        /// <summary>
        /// Monto del pago
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Monto { get; set; }

        /// <summary>
        /// Forma de pago: EFECTIVO, TARJETA, TRANSFERENCIA, QR, etc.
        /// </summary>
        [MaxLength(20)]
        public string FormaPago { get; set; } = "EFECTIVO";

        /// <summary>
        /// Referencia del pago (número de tarjeta, referencia de transferencia, etc.)
        /// </summary>
        [MaxLength(100)]
        public string? Referencia { get; set; }

        // ========== VUELTO ==========
        
        /// <summary>
        /// Monto recibido (para calcular vuelto en efectivo)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? MontoRecibido { get; set; }

        /// <summary>
        /// Vuelto entregado
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? Vuelto { get; set; }

        // ========== PROPINA ==========
        
        /// <summary>
        /// Monto de propina incluido en este pago
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Propina { get; set; }

        // ========== DIVISIÓN DE CUENTA ==========
        
        /// <summary>
        /// Identificador del comensal que realiza este pago
        /// </summary>
        public int? NumeroComensal { get; set; }

        /// <summary>
        /// Nombre/descripción del pagador (ej: "Juan", "Cuenta 1")
        /// </summary>
        [MaxLength(100)]
        public string? NombrePagador { get; set; }

        /// <summary>
        /// IDs de detalles incluidos en este pago (separados por coma)
        /// Útil para división de cuenta por items específicos
        /// </summary>
        [MaxLength(500)]
        public string? DetallesIncluidos { get; set; }

        // ========== FACTURACIÓN ==========
        
        /// <summary>
        /// Si este pago generó una factura
        /// </summary>
        public bool Facturado { get; set; } = false;

        /// <summary>
        /// ID de la venta/factura generada
        /// </summary>
        public int? IdVenta { get; set; }
        public Venta? Venta { get; set; }

        /// <summary>
        /// Cliente al que se facturó
        /// </summary>
        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        // ========== ESTADO ==========
        
        /// <summary>
        /// Estado: Pendiente, Completado, Anulado
        /// </summary>
        [MaxLength(20)]
        public string Estado { get; set; } = "Completado";

        // ========== AUDITORÍA ==========
        
        public DateTime FechaPago { get; set; } = DateTime.Now;
        
        public int? IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }
    }
}
