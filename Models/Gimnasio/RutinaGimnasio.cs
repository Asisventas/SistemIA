using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.Gimnasio
{
    /// <summary>
    /// Rutina de ejercicios asignada a un cliente del gimnasio.
    /// Puede ser creada por un personal trainer o el propio cliente.
    /// </summary>
    [Table("RutinasGimnasio")]
    public class RutinaGimnasio
    {
        [Key]
        public int IdRutina { get; set; }

        // ========== RELACIONES ==========
        
        /// <summary>
        /// Cliente al que pertenece la rutina
        /// </summary>
        [Required]
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }

        /// <summary>
        /// Personal Trainer que creó/supervisó la rutina (opcional)
        /// </summary>
        public int? IdInstructor { get; set; }
        [ForeignKey("IdInstructor")]
        public Instructor? Instructor { get; set; }

        /// <summary>
        /// Sucursal donde se creó la rutina
        /// </summary>
        public int? IdSucursal { get; set; }
        [ForeignKey("IdSucursal")]
        public Sucursal? Sucursal { get; set; }

        // ========== DATOS DE LA RUTINA ==========
        
        /// <summary>
        /// Nombre descriptivo de la rutina
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción u objetivo de la rutina
        /// </summary>
        [StringLength(500)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Tipo de rutina: Fuerza, Cardio, Funcional, Mixto, Flexibilidad
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = "Mixto";

        /// <summary>
        /// Nivel de dificultad: Principiante, Intermedio, Avanzado
        /// </summary>
        [Required]
        [StringLength(30)]
        public string NivelDificultad { get; set; } = "Principiante";

        /// <summary>
        /// Duración estimada en minutos
        /// </summary>
        public int DuracionMinutos { get; set; } = 60;

        /// <summary>
        /// Días de la semana recomendados (ej: "Lunes, Miércoles, Viernes")
        /// </summary>
        [StringLength(100)]
        public string? DiasRecomendados { get; set; }

        // ========== ESTADO Y FECHAS ==========
        
        /// <summary>
        /// Estado: Activa, Pausada, Completada, Cancelada
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Activa";

        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        /// <summary>
        /// Fecha de inicio de la rutina
        /// </summary>
        public DateTime? FechaInicio { get; set; }

        /// <summary>
        /// Fecha de fin planificada
        /// </summary>
        public DateTime? FechaFin { get; set; }

        // ========== NAVEGACIÓN ==========
        
        /// <summary>
        /// Ejercicios que componen la rutina
        /// </summary>
        public ICollection<EjercicioRutina>? Ejercicios { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        [NotMapped]
        public int CantidadEjercicios => Ejercicios?.Count ?? 0;

        [NotMapped]
        public bool EstaActiva => Estado == "Activa";
    }
}
