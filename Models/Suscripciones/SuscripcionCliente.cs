using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.Suscripciones
{
    /// <summary>
    /// Configuración de suscripción para un cliente.
    /// Permite definir un producto y monto a facturar automáticamente en una fecha específica.
    /// </summary>
    public class SuscripcionCliente
    {
        [Key]
        public int IdSuscripcion { get; set; }

        // ========== RELACIONES ==========
        public int IdSucursal { get; set; }
        [ForeignKey("IdSucursal")]
        public Sucursal? Sucursal { get; set; }

        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }

        public int? IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public Producto? Producto { get; set; }

        /// <summary>
        /// Caja donde se generarán las facturas automáticas
        /// </summary>
        public int? IdCaja { get; set; }
        [ForeignKey("IdCaja")]
        public Caja? Caja { get; set; }

        /// <summary>
        /// Venta de referencia/plantilla para generar las facturas recurrentes.
        /// Contiene todos los productos, condiciones de pago, descuentos, etc.
        /// </summary>
        public int? IdVentaReferencia { get; set; }
        [ForeignKey("IdVentaReferencia")]
        public Venta? VentaReferencia { get; set; }

        /// <summary>
        /// Condición de pago: Contado o Crédito
        /// </summary>
        [MaxLength(20)]
        public string CondicionPago { get; set; } = "Contado";

        /// <summary>
        /// Cantidad de cuotas si es a crédito
        /// </summary>
        public int? CantidadCuotas { get; set; }

        // ========== CONFIGURACIÓN DE FACTURACIÓN ==========
        /// <summary>
        /// Monto a facturar cada período (si es diferente al precio del producto)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoFacturar { get; set; }

        /// <summary>
        /// Cantidad del producto a facturar (por defecto 1)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Cantidad { get; set; } = 1m;

        /// <summary>
        /// Día del mes para generar la factura (1-28)
        /// </summary>
        [Range(1, 28)]
        public int DiaFacturacion { get; set; } = 1;

        /// <summary>
        /// Hora del día para generar la factura (ej: 08:00)
        /// </summary>
        public TimeSpan? HoraFacturacion { get; set; }

        /// <summary>
        /// Hora del día para enviar correo de factura (ej: 09:00)
        /// </summary>
        public TimeSpan? HoraEnvioCorreo { get; set; }

        /// <summary>
        /// Tipo de período: Mensual, Bimestral, Trimestral, Semestral, Anual
        /// </summary>
        [MaxLength(20)]
        public string TipoPeriodo { get; set; } = "Mensual";

        // ========== FECHAS ==========
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public DateTime? FechaProximaFactura { get; set; }
        public DateTime? FechaUltimaFactura { get; set; }

        // ========== ESTADO ==========
        /// <summary>
        /// Estado: Activa, Pausada, Cancelada, Vencida
        /// </summary>
        [MaxLength(20)]
        public string Estado { get; set; } = "Activa";

        /// <summary>
        /// Si está activa para facturación automática
        /// </summary>
        public bool FacturacionActiva { get; set; } = true;

        /// <summary>
        /// Enviar factura por correo automáticamente
        /// </summary>
        public bool EnviarCorreoFactura { get; set; } = true;

        /// <summary>
        /// Enviar recibo por correo al cobrar
        /// </summary>
        public bool EnviarCorreoRecibo { get; set; } = true;

        // ========== DESCRIPCIÓN PERSONALIZADA ==========
        /// <summary>
        /// Descripción personalizada que aparecerá en la factura (opcional)
        /// </summary>
        [MaxLength(500)]
        public string? DescripcionFactura { get; set; }

        /// <summary>
        /// Observaciones internas
        /// </summary>
        [MaxLength(1000)]
        public string? Observaciones { get; set; }

        // ========== AUDITORÍA ==========
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        public int? IdUsuarioCreacion { get; set; }
        public int? IdUsuarioModificacion { get; set; }

        // ========== CONTADORES ==========
        /// <summary>
        /// Total de facturas generadas para esta suscripción
        /// </summary>
        public int TotalFacturasGeneradas { get; set; } = 0;

        /// <summary>
        /// Total de facturas cobradas
        /// </summary>
        public int TotalFacturasCobradas { get; set; } = 0;

        // ========== MÉTODOS AUXILIARES ==========
        [NotMapped]
        public bool EstaActiva => Estado == "Activa" && FacturacionActiva && 
                                  (FechaFin == null || FechaFin > DateTime.Now);

        [NotMapped]
        public int DiasHastaProximaFactura => FechaProximaFactura.HasValue 
            ? (int)(FechaProximaFactura.Value - DateTime.Today).TotalDays 
            : -1;

        /// <summary>
        /// Obtiene los meses según el tipo de período
        /// </summary>
        public int ObtenerMesesPeriodo()
        {
            return TipoPeriodo switch
            {
                "Mensual" => 1,
                "Bimestral" => 2,
                "Trimestral" => 3,
                "Semestral" => 6,
                "Anual" => 12,
                _ => 1
            };
        }

        /// <summary>
        /// Calcula la próxima fecha de facturación
        /// </summary>
        public DateTime CalcularProximaFechaFactura(DateTime? desdeFactura = null)
        {
            var fechaBase = desdeFactura ?? FechaUltimaFactura ?? FechaInicio;
            var meses = ObtenerMesesPeriodo();
            var proximaFecha = fechaBase.AddMonths(meses);

            // Ajustar al día de facturación
            var diasEnMes = DateTime.DaysInMonth(proximaFecha.Year, proximaFecha.Month);
            var dia = Math.Min(DiaFacturacion, diasEnMes);

            return new DateTime(proximaFecha.Year, proximaFecha.Month, dia);
        }
    }
}
