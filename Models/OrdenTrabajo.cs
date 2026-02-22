using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Orden de Trabajo para el modo Taller.
    /// Es similar a Pedido pero con campos específicos para talleres mecánicos.
    /// Se vincula a una Bahía (Mesa con Tipo="Bahía") y un Vehículo.
    /// </summary>
    [Table("OrdenesTrabajo")]
    public class OrdenTrabajo
    {
        [Key]
        public int IdOrdenTrabajo { get; set; }

        // ========== SUCURSAL ==========
        
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        // ========== CAJA Y TURNO ==========
        
        public int? IdCaja { get; set; }
        public Caja? Caja { get; set; }

        public int? Turno { get; set; }
        public DateTime? FechaCaja { get; set; }

        // ========== BAHÍA (equivalente a Mesa) ==========
        
        /// <summary>
        /// Bahía donde se trabaja el vehículo
        /// </summary>
        public int? IdMesa { get; set; }
        public Mesa? Bahia { get; set; }

        // ========== VEHÍCULO ==========
        
        public int IdVehiculo { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        // ========== CLIENTE ==========
        
        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        // ========== IDENTIFICACIÓN ==========
        
        /// <summary>
        /// Número de orden de trabajo (correlativo por año o día)
        /// </summary>
        public int NumeroOrden { get; set; }

        /// <summary>
        /// Año de la orden (para correlativo anual)
        /// </summary>
        public int AnioOrden { get; set; }

        /// <summary>
        /// Código único de la orden (ej: "OT-2026-0001")
        /// </summary>
        [MaxLength(30)]
        public string? CodigoOrden { get; set; }

        // ========== DATOS DEL VEHÍCULO AL MOMENTO DE INGRESO ==========
        
        /// <summary>
        /// Kilometraje del vehículo al ingresar
        /// </summary>
        public int? KilometrajeIngreso { get; set; }

        /// <summary>
        /// Nivel de combustible al ingresar (0-100%)
        /// </summary>
        public int? NivelCombustible { get; set; }

        /// <summary>
        /// Observaciones del estado del vehículo al ingresar
        /// </summary>
        [MaxLength(1000)]
        public string? EstadoIngreso { get; set; }

        /// <summary>
        /// Fotos del vehículo al ingresar (JSON con URLs)
        /// </summary>
        public string? FotosIngreso { get; set; }

        // ========== DESCRIPCIÓN DEL TRABAJO ==========
        
        /// <summary>
        /// Motivo de consulta del cliente (síntomas, problemas)
        /// </summary>
        [MaxLength(1000)]
        public string? MotivoConsulta { get; set; }

        /// <summary>
        /// Diagnóstico del mecánico
        /// </summary>
        [MaxLength(2000)]
        public string? Diagnostico { get; set; }

        /// <summary>
        /// Trabajos a realizar (descripción general)
        /// </summary>
        [MaxLength(2000)]
        public string? TrabajosARealizar { get; set; }

        /// <summary>
        /// Trabajos efectivamente realizados
        /// </summary>
        [MaxLength(2000)]
        public string? TrabajosRealizados { get; set; }

        /// <summary>
        /// Recomendaciones para el cliente
        /// </summary>
        [MaxLength(1000)]
        public string? Recomendaciones { get; set; }

        // ========== TIEMPOS ==========
        
        /// <summary>
        /// Fecha/hora de creación de la orden
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        /// <summary>
        /// Fecha/hora de ingreso del vehículo al taller
        /// </summary>
        public DateTime? FechaIngreso { get; set; }

        /// <summary>
        /// Fecha/hora de inicio de trabajo
        /// </summary>
        public DateTime? FechaInicioTrabajo { get; set; }

        /// <summary>
        /// Fecha/hora de finalización del trabajo
        /// </summary>
        public DateTime? FechaFinTrabajo { get; set; }

        /// <summary>
        /// Fecha/hora de entrega al cliente
        /// </summary>
        public DateTime? FechaEntrega { get; set; }

        /// <summary>
        /// Fecha prometida de entrega
        /// </summary>
        public DateTime? FechaPrometida { get; set; }

        // ========== PERSONAL ASIGNADO ==========
        
        /// <summary>
        /// Mecánico principal asignado
        /// </summary>
        public int? IdMecanico { get; set; }
        public Usuario? Mecanico { get; set; }

        /// <summary>
        /// Nombre del mecánico (para mostrar)
        /// </summary>
        [MaxLength(200)]
        public string? NombreMecanico { get; set; }

        /// <summary>
        /// IDs de mecánicos adicionales (JSON)
        /// </summary>
        public string? MecanicosAdicionales { get; set; }

        /// <summary>
        /// Usuario que creó la orden
        /// </summary>
        public int? IdUsuarioCreacion { get; set; }

        /// <summary>
        /// Usuario que cerró/facturó la orden
        /// </summary>
        public int? IdUsuarioCierre { get; set; }

        // ========== ESTADO ==========
        
        /// <summary>
        /// Estado de la orden:
        /// - Recepcion: Vehículo ingresado, esperando diagnóstico
        /// - Diagnostico: En proceso de diagnóstico
        /// - Presupuestado: Presupuesto enviado al cliente, esperando aprobación
        /// - Aprobado: Cliente aprobó, esperando inicio
        /// - EnProceso: Trabajo en curso
        /// - Pausado: Trabajo pausado (falta repuestos, etc.)
        /// - Terminado: Trabajo completado, esperando pago/entrega
        /// - Entregado: Vehículo entregado al cliente
        /// - Cancelado: Orden cancelada
        /// </summary>
        [MaxLength(30)]
        public string Estado { get; set; } = "Recepcion";

        /// <summary>
        /// Prioridad: Normal, Alta, Urgente
        /// </summary>
        [MaxLength(20)]
        public string Prioridad { get; set; } = "Normal";

        /// <summary>
        /// Tipo de servicio: Mantenimiento, Reparación, Diagnóstico, Inspección
        /// </summary>
        [MaxLength(30)]
        public string? TipoServicio { get; set; }

        // ========== TOTALES ==========
        
        /// <summary>
        /// Total de mano de obra
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalManoObra { get; set; }

        /// <summary>
        /// Total de repuestos/materiales
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalRepuestos { get; set; }

        /// <summary>
        /// Subtotal antes de descuentos
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Total de descuentos aplicados
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalDescuento { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalIva10 { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalIva5 { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalExenta { get; set; }

        /// <summary>
        /// Total final de la orden
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Total { get; set; }

        /// <summary>
        /// Monto pagado (anticipo o pago total)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal MontoPagado { get; set; }

        // ========== GARANTÍA ==========
        
        /// <summary>
        /// Días de garantía del trabajo
        /// </summary>
        public int? DiasGarantia { get; set; }

        /// <summary>
        /// Fecha de vencimiento de garantía
        /// </summary>
        public DateTime? FechaVencimientoGarantia { get; set; }

        /// <summary>
        /// Condiciones de garantía
        /// </summary>
        [MaxLength(500)]
        public string? CondicionesGarantia { get; set; }

        // ========== RELACIÓN CON PEDIDO (para facturación) ==========
        
        /// <summary>
        /// Pedido generado para facturación
        /// </summary>
        public int? IdPedido { get; set; }
        public Pedido? Pedido { get; set; }

        // ========== AUDITORÍA ==========
        
        public DateTime? FechaModificacion { get; set; }

        // ========== NAVEGACIÓN ==========
        
        /// <summary>
        /// Detalles de la orden (repuestos, servicios, mano de obra)
        /// </summary>
        public ICollection<OrdenTrabajoDetalle>? Detalles { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        [NotMapped]
        public decimal SaldoPendiente => Total - MontoPagado;

        [NotMapped]
        public bool EstaPagado => MontoPagado >= Total && Total > 0;

        [NotMapped]
        public TimeSpan? TiempoEnTaller => FechaEntrega.HasValue && FechaIngreso.HasValue 
            ? FechaEntrega.Value - FechaIngreso.Value 
            : (FechaIngreso.HasValue ? DateTime.Now - FechaIngreso.Value : null);

        [NotMapped]
        public string TiempoEnTallerTexto
        {
            get
            {
                var tiempo = TiempoEnTaller;
                if (tiempo == null) return "-";
                if (tiempo.Value.TotalDays >= 1)
                    return $"{(int)tiempo.Value.TotalDays}d {tiempo.Value.Hours}h";
                if (tiempo.Value.TotalHours >= 1)
                    return $"{(int)tiempo.Value.TotalHours}h {tiempo.Value.Minutes}m";
                return $"{tiempo.Value.Minutes}m";
            }
        }

        /// <summary>
        /// Texto descriptivo del estado
        /// </summary>
        [NotMapped]
        public string EstadoTexto => Estado switch
        {
            "Recepcion" => "📥 Recepción",
            "Diagnostico" => "🔍 Diagnóstico",
            "Presupuestado" => "📋 Presupuestado",
            "Aprobado" => "✅ Aprobado",
            "EnProceso" => "🔧 En Proceso",
            "Pausado" => "⏸️ Pausado",
            "Terminado" => "✔️ Terminado",
            "Entregado" => "🚗 Entregado",
            "Cancelado" => "❌ Cancelado",
            _ => Estado
        };

        /// <summary>
        /// Color CSS para el estado
        /// </summary>
        [NotMapped]
        public string ColorEstado => Estado switch
        {
            "Recepcion" => "info",
            "Diagnostico" => "secondary",
            "Presupuestado" => "warning",
            "Aprobado" => "primary",
            "EnProceso" => "danger",
            "Pausado" => "secondary",
            "Terminado" => "success",
            "Entregado" => "success",
            "Cancelado" => "dark",
            _ => "secondary"
        };
    }
}
