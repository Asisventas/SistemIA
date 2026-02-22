using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.Gimnasio
{
    /// <summary>
    /// Horario programado de una clase grupal.
    /// Define días y horarios recurrentes o específicos.
    /// </summary>
    [Table("HorariosClases")]
    public class HorarioClase
    {
        [Key]
        public int IdHorario { get; set; }

        // ========== CLASE ==========
        
        /// <summary>
        /// Clase a la que pertenece este horario
        /// </summary>
        [Required]
        public int IdClase { get; set; }
        [ForeignKey("IdClase")]
        public ClaseGimnasio? Clase { get; set; }

        // ========== INSTRUCTOR ==========
        
        /// <summary>
        /// Instructor que dicta en este horario (puede ser diferente al principal de la clase)
        /// </summary>
        public int? IdInstructor { get; set; }
        [ForeignKey("IdInstructor")]
        public Instructor? Instructor { get; set; }

        // ========== TIPO DE HORARIO ==========
        
        /// <summary>
        /// Tipo: Recurrente (semanal), Único (fecha específica)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string TipoHorario { get; set; } = "Recurrente";

        // ========== PARA HORARIOS RECURRENTES ==========
        
        /// <summary>
        /// Día de la semana (0=Domingo, 1=Lunes, ..., 6=Sábado)
        /// </summary>
        public int? DiaSemana { get; set; }

        /// <summary>
        /// Fecha de inicio del período recurrente
        /// </summary>
        public DateTime? FechaInicioRecurrencia { get; set; }

        /// <summary>
        /// Fecha fin del período recurrente (null = indefinido)
        /// </summary>
        public DateTime? FechaFinRecurrencia { get; set; }

        // ========== PARA HORARIOS ÚNICOS ==========
        
        /// <summary>
        /// Fecha específica (para eventos únicos o clases especiales)
        /// </summary>
        public DateTime? FechaEspecifica { get; set; }

        // ========== HORARIO ==========
        
        /// <summary>
        /// Hora de inicio (formato HH:mm)
        /// </summary>
        [Required]
        public TimeSpan HoraInicio { get; set; }

        /// <summary>
        /// Hora de fin (formato HH:mm)
        /// </summary>
        [Required]
        public TimeSpan HoraFin { get; set; }

        /// <summary>
        /// Duración en minutos (calculado o personalizado)
        /// </summary>
        public int DuracionMinutos { get; set; } = 60;

        // ========== CAPACIDAD ==========
        
        /// <summary>
        /// Capacidad máxima para este horario (puede ser diferente a la clase)
        /// </summary>
        public int? CapacidadMaxima { get; set; }

        /// <summary>
        /// Sala específica para este horario (puede ser diferente a la clase)
        /// </summary>
        [StringLength(50)]
        public string? Sala { get; set; }

        // ========== ESTADO ==========
        
        /// <summary>
        /// Indica si este horario está activo
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Indica si está temporalmente cancelado (por vacaciones, etc.)
        /// </summary>
        public bool Cancelado { get; set; } = false;

        /// <summary>
        /// Motivo de cancelación temporal
        /// </summary>
        [StringLength(200)]
        public string? MotivoCancelacion { get; set; }

        // ========== AUDITORÍA ==========
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        // ========== NAVEGACIÓN ==========
        
        /// <summary>
        /// Reservas de clientes para este horario
        /// </summary>
        public ICollection<ReservaClase>? Reservas { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========

        /// <summary>
        /// Obtiene el nombre del día de la semana
        /// </summary>
        [NotMapped]
        public string NombreDia => DiaSemana switch
        {
            0 => "Domingo",
            1 => "Lunes",
            2 => "Martes",
            3 => "Miércoles",
            4 => "Jueves",
            5 => "Viernes",
            6 => "Sábado",
            _ => ""
        };

        /// <summary>
        /// Obtiene la capacidad efectiva (del horario o de la clase)
        /// </summary>
        [NotMapped]
        public int CapacidadEfectiva => CapacidadMaxima ?? Clase?.CapacidadMaxima ?? 20;

        /// <summary>
        /// Obtiene el horario formateado (ej: "08:00 - 09:00")
        /// </summary>
        [NotMapped]
        public string HorarioFormateado => $"{HoraInicio:hh\\:mm} - {HoraFin:hh\\:mm}";
    }
}
