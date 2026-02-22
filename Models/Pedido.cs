using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Pedido de una mesa/cancha. Acumula consumos antes de facturar.
    /// Un pedido puede convertirse en una o más ventas (pagos parciales/división de cuenta).
    /// Sigue el mismo patrón de Sucursal/Caja/Turno/FechaCaja que Venta.
    /// </summary>
    [Table("Pedidos")]
    public class Pedido
    {
        [Key]
        public int IdPedido { get; set; }

        // ========== SUCURSAL (igual que Venta) ==========
        
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        // ========== CAJA Y TURNO (igual que Venta) ==========
        
        public int? IdCaja { get; set; }
        public Caja? Caja { get; set; }

        /// <summary>
        /// Turno de caja en el momento de apertura
        /// </summary>
        public int? Turno { get; set; }

        /// <summary>
        /// Fecha operativa de caja (FechaActualCaja de la caja al momento de apertura)
        /// </summary>
        public DateTime? FechaCaja { get; set; }

        // ========== MESA/ESPACIO ==========
        
        public int IdMesa { get; set; }
        public Mesa? Mesa { get; set; }

        // ========== IDENTIFICACIÓN ==========
        
        /// <summary>
        /// Número de pedido/comanda (auto-generado por día)
        /// </summary>
        public int NumeroPedido { get; set; }

        /// <summary>
        /// Número de comensales en la mesa
        /// </summary>
        public int Comensales { get; set; } = 1;

        /// <summary>
        /// Nombre del cliente o referencia (opcional)
        /// </summary>
        [MaxLength(200)]
        public string? NombreCliente { get; set; }

        // ========== CLIENTE (para facturación) ==========
        
        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        // ========== TIEMPOS ==========
        
        /// <summary>
        /// Fecha/hora de apertura del pedido (cuando se sentaron)
        /// </summary>
        public DateTime FechaApertura { get; set; } = DateTime.Now;

        /// <summary>
        /// Fecha/hora de cierre (cuando pagaron y se fueron)
        /// </summary>
        public DateTime? FechaCierre { get; set; }

        /// <summary>
        /// Hora inicio para canchas con reserva por tiempo
        /// </summary>
        public DateTime? HoraInicio { get; set; }

        /// <summary>
        /// Hora fin para canchas con reserva por tiempo
        /// </summary>
        public DateTime? HoraFin { get; set; }

        // ========== TOTALES (mismo patrón que Venta) ==========
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalDescuento { get; set; }

        /// <summary>
        /// Cargo por servicio (propina sugerida o cargo fijo)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal CargoServicio { get; set; }

        /// <summary>
        /// Porcentaje de cargo por servicio aplicado
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? PorcentajeServicio { get; set; }

        /// <summary>
        /// Cargo por tiempo (para canchas)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal CargoPorTiempo { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalIva10 { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalIva5 { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalExenta { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Total { get; set; }

        /// <summary>
        /// Monto ya pagado (para pagos parciales)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoPagado { get; set; }

        /// <summary>
        /// Saldo pendiente de pago
        /// </summary>
        [NotMapped]
        public decimal SaldoPendiente => Total - MontoPagado;

        // ========== ESTADO ==========
        
        /// <summary>
        /// Estado: Abierto, EnCurso, PendientePago, Pagado, Cancelado
        /// </summary>
        [MaxLength(20)]
        public string Estado { get; set; } = "Abierto";

        /// <summary>
        /// Si el pedido fue impreso como comanda
        /// </summary>
        public bool ComandaImpresa { get; set; } = false;

        /// <summary>
        /// Número de veces que se imprimió la comanda
        /// </summary>
        public int ImpresionesComanda { get; set; } = 0;

        // ========== OBSERVACIONES ==========
        
        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // ========== RESERVA ==========
        
        /// <summary>
        /// Referencia a la reserva (si el pedido viene de una reserva)
        /// </summary>
        public int? IdReserva { get; set; }
        public Reserva? Reserva { get; set; }

        // ========== USUARIO (igual que Venta) ==========
        
        /// <summary>
        /// Mesero/Mozo que atiende el pedido
        /// </summary>
        public int? IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        [MaxLength(250)]
        public string? NombreMesero { get; set; }

        /// <summary>
        /// Usuario que abrió el pedido
        /// </summary>
        public int? IdUsuarioApertura { get; set; }

        /// <summary>
        /// Usuario que cerró/cobró el pedido
        /// </summary>
        public int? IdUsuarioCierre { get; set; }

        // ========== AUDITORÍA ==========
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        // ========== NAVEGACIÓN ==========
        
        public ICollection<PedidoDetalle>? Detalles { get; set; }
        public ICollection<PedidoPago>? Pagos { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        /// <summary>
        /// Tiempo transcurrido desde la apertura
        /// </summary>
        [NotMapped]
        public TimeSpan TiempoTranscurrido => FechaCierre.HasValue 
            ? FechaCierre.Value - FechaApertura 
            : DateTime.Now - FechaApertura;

        [NotMapped]
        public string TiempoTranscurridoTexto
        {
            get
            {
                var tiempo = TiempoTranscurrido;
                if (tiempo.TotalHours >= 1)
                    return $"{(int)tiempo.TotalHours}h {tiempo.Minutes}m";
                return $"{tiempo.Minutes}m";
            }
        }

        /// <summary>
        /// Indica si el pedido está completamente pagado
        /// </summary>
        [NotMapped]
        public bool EstaPagado => MontoPagado >= Total && Total > 0;

        /// <summary>
        /// Cantidad total de items en el pedido
        /// </summary>
        [NotMapped]
        public int CantidadItems => Detalles?.Sum(d => (int)d.Cantidad) ?? 0;
    }
}
