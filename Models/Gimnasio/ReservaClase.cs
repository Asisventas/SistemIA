using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.Gimnasio
{
    /// <summary>
    /// Reserva de un cliente para una clase grupal.
    /// Incluye seguimiento de asistencia.
    /// </summary>
    [Table("ReservasClases")]
    public class ReservaClase
    {
        [Key]
        public int IdReserva { get; set; }

        // ========== SUCURSAL / CAJA / TURNO ==========
        
        /// <summary>
        /// Sucursal donde se reserva
        /// </summary>
        [Required]
        public int IdSucursal { get; set; }
        [ForeignKey("IdSucursal")]
        public Sucursal? Sucursal { get; set; }

        /// <summary>
        /// Caja donde se registró (si aplica)
        /// </summary>
        public int? IdCaja { get; set; }
        [ForeignKey("IdCaja")]
        public Caja? Caja { get; set; }

        /// <summary>
        /// Turno en que se registró (si aplica)
        /// </summary>
        public int? Turno { get; set; }

        // ========== HORARIO Y FECHA ==========
        
        /// <summary>
        /// Horario de clase reservado
        /// </summary>
        [Required]
        public int IdHorario { get; set; }
        [ForeignKey("IdHorario")]
        public HorarioClase? Horario { get; set; }

        /// <summary>
        /// Fecha específica de la clase reservada
        /// </summary>
        [Required]
        public DateTime FechaClase { get; set; }

        // ========== CLIENTE ==========
        
        /// <summary>
        /// Cliente que reserva
        /// </summary>
        [Required]
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }

        /// <summary>
        /// Membresía activa del cliente al momento de reservar
        /// </summary>
        public int? IdMembresia { get; set; }
        [ForeignKey("IdMembresia")]
        public MembresiaCliente? Membresia { get; set; }

        // ========== ESTADO DE LA RESERVA ==========
        
        /// <summary>
        /// Estado: Reservada, Confirmada, Asistió, NoAsistió, Cancelada, ListaEspera
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Reservada";

        /// <summary>
        /// Posición en lista de espera (0 = no está en lista de espera)
        /// </summary>
        public int PosicionListaEspera { get; set; } = 0;

        /// <summary>
        /// Fecha/hora en que se realizó la reserva
        /// </summary>
        public DateTime FechaReserva { get; set; } = DateTime.Now;

        /// <summary>
        /// Fecha/hora de confirmación (si aplica)
        /// </summary>
        public DateTime? FechaConfirmacion { get; set; }

        /// <summary>
        /// Fecha/hora de cancelación (si aplica)
        /// </summary>
        public DateTime? FechaCancelacion { get; set; }

        /// <summary>
        /// Motivo de cancelación
        /// </summary>
        [StringLength(200)]
        public string? MotivoCancelacion { get; set; }

        // ========== ASISTENCIA ==========
        
        /// <summary>
        /// Hora real de llegada (check-in)
        /// </summary>
        public DateTime? HoraCheckIn { get; set; }

        /// <summary>
        /// Indica si llegó tarde
        /// </summary>
        public bool LlegoTarde { get; set; } = false;

        /// <summary>
        /// Usuario que registró el check-in
        /// </summary>
        public int? IdUsuarioCheckIn { get; set; }

        /// <summary>
        /// Método de check-in: Manual, QR, Facial, Lista
        /// </summary>
        [StringLength(20)]
        public string? MetodoCheckIn { get; set; }

        // ========== PAGO (si la clase tiene costo adicional) ==========
        
        /// <summary>
        /// Indica si ya pagó (para clases con costo adicional)
        /// </summary>
        public bool Pagado { get; set; } = false;

        /// <summary>
        /// Monto pagado
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? MontoPagado { get; set; }

        /// <summary>
        /// Venta asociada al pago
        /// </summary>
        public int? IdVenta { get; set; }
        [ForeignKey("IdVenta")]
        public Venta? Venta { get; set; }

        // ========== NOTAS ==========
        
        /// <summary>
        /// Notas adicionales
        /// </summary>
        [StringLength(300)]
        public string? Notas { get; set; }

        // ========== NOTIFICACIONES ==========
        
        /// <summary>
        /// Se envió recordatorio de la clase
        /// </summary>
        public bool RecordatorioEnviado { get; set; } = false;

        /// <summary>
        /// Fecha/hora del recordatorio enviado
        /// </summary>
        public DateTime? FechaRecordatorio { get; set; }

        // ========== AUDITORÍA ==========
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========

        /// <summary>
        /// Indica si la reserva puede cancelarse (según configuración de la clase)
        /// </summary>
        [NotMapped]
        public bool PuedeCancelarse => 
            Estado == "Reservada" && 
            FechaClase > DateTime.Now.AddHours(Horario?.Clase?.HorasLimiteCancelacion ?? 2);

        /// <summary>
        /// Indica si ya pasó la hora de la clase
        /// </summary>
        [NotMapped]
        public bool YaPaso => 
            FechaClase.Date < DateTime.Today || 
            (FechaClase.Date == DateTime.Today && Horario?.HoraFin < DateTime.Now.TimeOfDay);

        /// <summary>
        /// Indica si puede hacer check-in (dentro de ventana de tiempo)
        /// </summary>
        [NotMapped]
        public bool PuedeHacerCheckIn =>
            Estado == "Reservada" &&
            FechaClase.Date == DateTime.Today &&
            DateTime.Now.TimeOfDay >= (Horario?.HoraInicio ?? TimeSpan.Zero).Add(TimeSpan.FromMinutes(-30)) &&
            DateTime.Now.TimeOfDay <= (Horario?.HoraFin ?? TimeSpan.MaxValue);
    }
}
