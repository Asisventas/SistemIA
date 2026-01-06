using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemIA.Models;

namespace SistemIA.Models
{
    public class Asistencia
    {
        [Key]
        public int Id_Asistencia { get; set; }

        [Required]
        [Column("Id_Usuario")]
        public int Id_Usuario { get; set; }
        [ForeignKey("Id_Usuario")]
        public Usuario? Usuario { get; set; }

        [Required]
        // Esta propiedad es la clave foránea que apunta a 'Id' en la tabla 'Sucursal'
        public int Sucursal { get; set; } // ¡Ahora es int, compatible con Id de Sucursal!
        [ForeignKey("Sucursal")] // Se relaciona con la propiedad 'Id' de Sucursal
        public Sucursal? SucursalNavigation { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoRegistro { get; set; } = string.Empty;
        
        // ========== TURNO Y CAJA ==========
        
        /// <summary>
        /// Turno en el que se registró la asistencia (1, 2, 3, etc.)
        /// </summary>
        public int? Turno { get; set; }
        
        /// <summary>
        /// ID de la caja donde se registró (opcional, para relacionar con el turno de caja)
        /// </summary>
        public int? IdCaja { get; set; }
        [ForeignKey("IdCaja")]
        public Caja? CajaNavigation { get; set; }

        [StringLength(255)]
        public string? Notas { get; set; }

        [StringLength(50)]
        public string? Ubicacion { get; set; }

        public DateTime? FechaInicioAusencia { get; set; }
        public DateTime? FechaFinAusencia { get; set; }

        [StringLength(100)]
        public string? MotivoAusencia { get; set; }

        public bool AprobadaPorGerencia { get; set; } = false;

        [Column("AprobadoPorId_Usuario")]
        public int? AprobadoPorId_Usuario { get; set; }
        [ForeignKey("AprobadoPorId_Usuario")]
        public Usuario? AprobadoPorUsuario { get; set; }

        public DateTime? FechaAprobacion { get; set; }

        public byte[]? ImagenRegistro { get; set; }

        // === CAMPOS DE CONTROL DE TIEMPO Y ASISTENCIA PROFESIONAL ===
        
        /// <summary>
        /// Hora programada/esperada según el horario asignado al empleado
        /// </summary>
        public DateTime? HoraProgramada { get; set; }

        /// <summary>
        /// Diferencia en minutos entre la hora programada y la real
        /// Positivo = Tardanza, Negativo = Adelanto
        /// </summary>
        public int? DiferenciaMinutos { get; set; }

        /// <summary>
        /// Estado de puntualidad: Puntual, Tardanza, Adelanto, Ausencia
        /// </summary>
        [StringLength(20)]
        public string? EstadoPuntualidad { get; set; }

        /// <summary>
        /// Minutos de tolerancia aplicados (configurables por empresa)
        /// </summary>
        public int? MinutosToleranciAplicada { get; set; }

        /// <summary>
        /// ID del horario que se usó para calcular el control
        /// </summary>
        public int? Id_HorarioAplicado { get; set; }
        [ForeignKey("Id_HorarioAplicado")]
        public HorarioTrabajo? HorarioAplicado { get; set; }

        /// <summary>
        /// Día de la semana (1=Lunes, 7=Domingo) para facilitar reportes
        /// </summary>
        public int? DiaSemana { get; set; }

        /// <summary>
        /// Indica si es un día laborable según el horario
        /// </summary>
        public bool? EsDiaLaborable { get; set; }

        /// <summary>
        /// Tiempo total trabajado en minutos (calculado automáticamente)
        /// </summary>
        public int? TiempoTrabajadoMinutos { get; set; }

        /// <summary>
        /// Horas extra trabajadas en minutos
        /// </summary>
        public int? HorasExtraMinutos { get; set; }

        /// <summary>
        /// Indica si requiere justificación (tardanzas, ausencias)
        /// </summary>
        public bool RequiereJustificacion { get; set; } = false;

        /// <summary>
        /// Estado de la justificación: Pendiente, Aprobada, Rechazada
        /// </summary>
        [StringLength(20)]
        public string? EstadoJustificacion { get; set; }

        /// <summary>
        /// Justificación del empleado o supervisor
        /// </summary>
        [StringLength(500)]
        public string? TextoJustificacion { get; set; }

        /// <summary>
        /// Indica si el registro fue generado automáticamente
        /// </summary>
        public bool EsRegistroAutomatico { get; set; } = false;

        /// <summary>
        /// Método de registro: Facial, Manual, Tarjeta, QR, etc.
        /// </summary>
        [StringLength(30)]
        public string? MetodoRegistro { get; set; }

        /// <summary>
        /// IP o ubicación desde donde se registró
        /// </summary>
        [StringLength(50)]
        public string? UbicacionRegistro { get; set; }

        /// <summary>
        /// Temperatura corporal (para controles de salud)
        /// </summary>
        [Column(TypeName = "decimal(4,1)")]
        public decimal? Temperatura { get; set; }

        /// <summary>
        /// Observaciones adicionales del sistema o supervisor
        /// </summary>
        [StringLength(500)]
        public string? ObservacionesSistema { get; set; }
    }
}