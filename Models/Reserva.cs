using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Reserva de mesa o cancha.
    /// Permite programar ocupación futura con recordatorios.
    /// </summary>
    [Table("Reservas")]
    public class Reserva
    {
        [Key]
        public int IdReserva { get; set; }

        // ========== SUCURSAL (igual que otros modelos) ==========
        
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        // ========== MESA/ESPACIO ==========
        
        public int IdMesa { get; set; }
        public Mesa? Mesa { get; set; }

        // ========== IDENTIFICACIÓN ==========
        
        /// <summary>
        /// Número de reserva (auto-generado)
        /// </summary>
        public int NumeroReserva { get; set; }

        // ========== CLIENTE ==========
        
        /// <summary>
        /// Nombre del cliente que reserva
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string NombreCliente { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono de contacto
        /// </summary>
        [MaxLength(30)]
        public string? Telefono { get; set; }

        /// <summary>
        /// Email de contacto
        /// </summary>
        [MaxLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Cliente registrado en el sistema (opcional)
        /// </summary>
        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        // ========== FECHA Y HORA ==========
        
        /// <summary>
        /// Fecha de la reserva
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime FechaReserva { get; set; }

        /// <summary>
        /// Hora de inicio de la reserva
        /// </summary>
        public TimeSpan HoraInicio { get; set; }

        /// <summary>
        /// Hora de fin estimada
        /// </summary>
        public TimeSpan? HoraFin { get; set; }

        /// <summary>
        /// Duración en minutos (calculada o especificada)
        /// </summary>
        public int? DuracionMinutos { get; set; }

        /// <summary>
        /// Hora real en que el cliente llegó (para calcular tiempo de uso)
        /// </summary>
        public TimeSpan? HoraInicioReal { get; set; }

        /// <summary>
        /// Hora real en que el cliente se fue (para calcular tiempo de uso)
        /// </summary>
        public TimeSpan? HoraFinReal { get; set; }

        /// <summary>
        /// Si el administrador permitió extender el tiempo a pesar de llegada tardía
        /// (Solo aplica si no hay reserva siguiente que se vea afectada)
        /// </summary>
        public bool ExtensionPermitida { get; set; } = false;

        // ========== DETALLES ==========
        
        /// <summary>
        /// Número de personas esperadas
        /// </summary>
        public int CantidadPersonas { get; set; } = 1;

        /// <summary>
        /// Motivo u ocasión especial
        /// </summary>
        [MaxLength(200)]
        public string? Motivo { get; set; }

        /// <summary>
        /// Observaciones o requerimientos especiales
        /// </summary>
        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // ========== ESTADO ==========
        
        /// <summary>
        /// Estado: Pendiente, Confirmada, EnCurso, Completada, NoShow, Cancelada
        /// </summary>
        [MaxLength(20)]
        public string Estado { get; set; } = "Pendiente";

        /// <summary>
        /// Si se envió recordatorio
        /// </summary>
        public bool RecordatorioEnviado { get; set; } = false;

        /// <summary>
        /// Fecha/hora de envío del recordatorio
        /// </summary>
        public DateTime? FechaRecordatorio { get; set; }

        // ========== SEÑA/ANTICIPO ==========
        
        /// <summary>
        /// Si requiere seña/anticipo
        /// </summary>
        public bool RequiereSena { get; set; } = false;

        /// <summary>
        /// Monto de la seña/anticipo requerido
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? MontoSena { get; set; }

        /// <summary>
        /// Si la seña fue pagada (se actualiza al registrar SenaReserva)
        /// </summary>
        public bool SenaPagada { get; set; } = false;

        /// <summary>
        /// Fecha de pago de la seña
        /// </summary>
        public DateTime? FechaPagoSena { get; set; }

        /// <summary>
        /// Referencia al registro de pago de la seña (con todos los datos de caja)
        /// </summary>
        public SenaReserva? SenaReservaPago { get; set; }

        // ========== TARIFAS (para canchas) ==========
        
        /// <summary>
        /// Tarifa por hora aplicada
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? TarifaPorHora { get; set; }

        /// <summary>
        /// Total estimado de la reserva
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? TotalEstimado { get; set; }

        // ========== RELACIÓN CON PEDIDO ==========
        
        /// <summary>
        /// Pedido generado al iniciar la reserva
        /// </summary>
        public int? IdPedido { get; set; }
        public Pedido? Pedido { get; set; }

        // ========== AUDITORÍA ==========
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        public int? IdUsuarioCreacion { get; set; }
        public int? IdUsuarioModificacion { get; set; }

        /// <summary>
        /// Motivo de cancelación si aplica
        /// </summary>
        [MaxLength(500)]
        public string? MotivoCancelacion { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        /// <summary>
        /// Fecha y hora completa de inicio
        /// </summary>
        [NotMapped]
        public DateTime FechaHoraInicio => FechaReserva.Date + HoraInicio;

        /// <summary>
        /// Fecha y hora completa de fin
        /// </summary>
        [NotMapped]
        public DateTime? FechaHoraFin => HoraFin.HasValue 
            ? FechaReserva.Date + HoraFin.Value 
            : (DuracionMinutos.HasValue ? FechaHoraInicio.AddMinutes(DuracionMinutos.Value) : null);

        /// <summary>
        /// Indica si la reserva es para hoy
        /// </summary>
        [NotMapped]
        public bool EsHoy => FechaReserva.Date == DateTime.Today;

        /// <summary>
        /// Indica si la reserva está próxima (en las próximas 2 horas)
        /// </summary>
        [NotMapped]
        public bool EsProxima => EsHoy && FechaHoraInicio > DateTime.Now && 
                                 FechaHoraInicio <= DateTime.Now.AddHours(2);

        /// <summary>
        /// Tiempo restante hasta la reserva
        /// </summary>
        [NotMapped]
        public TimeSpan? TiempoRestante => FechaHoraInicio > DateTime.Now 
            ? FechaHoraInicio - DateTime.Now 
            : null;

        // ========== PROPIEDADES DE LLEGADA (TARDÍA/ADELANTADA) ==========

        /// <summary>
        /// Indica si el cliente llegó tarde (HoraInicioReal > HoraInicio)
        /// </summary>
        [NotMapped]
        public bool LlegoTarde => HoraInicioReal.HasValue && HoraInicioReal.Value > HoraInicio;

        /// <summary>
        /// Indica si el cliente llegó antes de tiempo (HoraInicioReal < HoraInicio)
        /// </summary>
        [NotMapped]
        public bool LlegoAdelantado => HoraInicioReal.HasValue && HoraInicioReal.Value < HoraInicio;

        /// <summary>
        /// Minutos de retraso (positivo = tarde, negativo = adelantado)
        /// </summary>
        [NotMapped]
        public int MinutosDesfase => HoraInicioReal.HasValue 
            ? (int)(HoraInicioReal.Value - HoraInicio).TotalMinutes 
            : 0;

        /// <summary>
        /// Hora de fin efectiva considerando llegada tardía
        /// Si llegó tarde y NO tiene extensión permitida, el fin es el original (HoraFin)
        /// Si llegó tarde y tiene extensión permitida, el fin se ajusta a la duración completa
        /// Si llegó adelantado, el inicio es cuando llegó pero el fin sigue siendo el original
        /// </summary>
        [NotMapped]
        public TimeSpan? HoraFinEfectiva
        {
            get
            {
                if (!HoraFin.HasValue) return null;
                
                // Si llegó tarde y NO tiene extensión permitida, termina a la hora original
                if (LlegoTarde && !ExtensionPermitida)
                    return HoraFin.Value;
                
                // Si llegó tarde CON extensión permitida, agregar el tiempo de retraso
                if (LlegoTarde && ExtensionPermitida)
                    return HoraFin.Value.Add(TimeSpan.FromMinutes(MinutosDesfase));
                
                // Si llegó adelantado, termina a la hora original (no gana tiempo extra)
                if (LlegoAdelantado)
                    return HoraFin.Value;
                
                // Normal: termina a la hora programada
                return HoraFin.Value;
            }
        }

        /// <summary>
        /// Duración efectiva de uso en minutos (considerando llegada tardía/adelantada)
        /// </summary>
        [NotMapped]
        public int? DuracionEfectivaMinutos
        {
            get
            {
                if (!HoraFinEfectiva.HasValue) return DuracionMinutos;
                
                var horaInicio = HoraInicioReal ?? HoraInicio;
                return (int)(HoraFinEfectiva.Value - horaInicio).TotalMinutes;
            }
        }

        /// <summary>
        /// Tiempo perdido por llegada tardía (si no tiene extensión)
        /// </summary>
        [NotMapped]
        public int MinutosPerdidos => LlegoTarde && !ExtensionPermitida ? MinutosDesfase : 0;

        /// <summary>
        /// Texto descriptivo del estado de llegada
        /// </summary>
        [NotMapped]
        public string? EstadoLlegadaTexto
        {
            get
            {
                if (!HoraInicioReal.HasValue) return null;
                if (LlegoTarde)
                    return ExtensionPermitida 
                        ? $"Llegó {MinutosDesfase} min tarde (extensión permitida)"
                        : $"Llegó {MinutosDesfase} min tarde (pierde tiempo)";
                if (LlegoAdelantado)
                    return $"Llegó {Math.Abs(MinutosDesfase)} min antes";
                return "Llegó a tiempo";
            }
        }

        [NotMapped]
        public string EstadoColor => Estado switch
        {
            "Pendiente" => "warning",
            "Confirmada" => "info",
            "EnCurso" => "success",
            "Completada" => "secondary",
            "NoShow" => "danger",
            "Cancelada" => "dark",
            _ => "light"
        };
    }
}
