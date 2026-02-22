using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.Gimnasio
{
    /// <summary>
    /// Definición de una clase grupal del gimnasio.
    /// Ej: Spinning, Yoga, Funcional, Zumba, CrossFit, etc.
    /// </summary>
    [Table("ClasesGimnasio")]
    public class ClaseGimnasio
    {
        [Key]
        public int IdClase { get; set; }

        // ========== SUCURSAL ==========
        
        /// <summary>
        /// Sucursal donde se imparte la clase (null = todas)
        /// </summary>
        public int? IdSucursal { get; set; }
        [ForeignKey("IdSucursal")]
        public Sucursal? Sucursal { get; set; }

        // ========== DATOS DE LA CLASE ==========
        
        /// <summary>
        /// Nombre de la clase (ej: "Spinning Intenso", "Yoga Básico")
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada de la clase
        /// </summary>
        [StringLength(500)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Categoría de la clase: Cardio, Fuerza, Flexibilidad, Baile, Funcional, Artes Marciales
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Categoria { get; set; } = "Cardio";

        /// <summary>
        /// Nivel de dificultad: Principiante, Intermedio, Avanzado, Todos
        /// </summary>
        [StringLength(30)]
        public string? Nivel { get; set; } = "Todos";

        /// <summary>
        /// Icono para identificar visualmente (Bootstrap Icons)
        /// </summary>
        [StringLength(50)]
        public string? Icono { get; set; } = "bi-lightning-charge";

        /// <summary>
        /// Color identificador de la clase (para calendario)
        /// </summary>
        [StringLength(7)]
        public string? ColorHex { get; set; } = "#007bff";

        /// <summary>
        /// Imagen promocional de la clase
        /// </summary>
        public byte[]? Imagen { get; set; }

        // ========== CONFIGURACIÓN ==========
        
        /// <summary>
        /// Duración estándar en minutos
        /// </summary>
        public int DuracionMinutos { get; set; } = 60;

        /// <summary>
        /// Capacidad máxima de alumnos por sesión
        /// </summary>
        public int CapacidadMaxima { get; set; } = 20;

        /// <summary>
        /// Cantidad mínima de alumnos para dar la clase
        /// </summary>
        public int MinimoAlumnos { get; set; } = 3;

        /// <summary>
        /// Lugar/Sala donde se imparte (ej: "Salón A", "Terraza", "Piscina")
        /// </summary>
        [StringLength(50)]
        public string? Sala { get; set; }

        /// <summary>
        /// Beneficios calorías quemadas aproximadas
        /// </summary>
        public int? CaloriasAproximadas { get; set; }

        // ========== INSTRUCTOR ==========
        
        /// <summary>
        /// Instructor principal asignado a esta clase
        /// </summary>
        public int? IdInstructor { get; set; }
        [ForeignKey("IdInstructor")]
        public Instructor? Instructor { get; set; }

        // ========== PRECIO ==========
        
        /// <summary>
        /// Indica si la clase tiene costo adicional (fuera de membresía)
        /// </summary>
        public bool TieneCostoAdicional { get; set; } = false;

        /// <summary>
        /// Precio por sesión si tiene costo adicional
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? PrecioPorSesion { get; set; }

        /// <summary>
        /// Planes de membresía que incluyen esta clase (IDs separados por coma)
        /// Si está vacío, todos los miembros pueden acceder
        /// </summary>
        [StringLength(200)]
        public string? PlanesIncluidos { get; set; }

        // ========== RESERVAS ==========
        
        /// <summary>
        /// Permite reservas online
        /// </summary>
        public bool PermiteReservas { get; set; } = true;

        /// <summary>
        /// Horas antes que se puede reservar (ej: 24 = un día antes)
        /// </summary>
        public int HorasAnticipacionReserva { get; set; } = 2;

        /// <summary>
        /// Horas límite para cancelar sin penalización
        /// </summary>
        public int HorasLimiteCancelacion { get; set; } = 2;

        /// <summary>
        /// Máximo de inasistencias permitidas antes de bloquear reservas
        /// </summary>
        public int MaxInasistencias { get; set; } = 3;

        // ========== ESTADO ==========
        
        /// <summary>
        /// Indica si la clase está activa
        /// </summary>
        public bool Activa { get; set; } = true;

        /// <summary>
        /// Orden de visualización
        /// </summary>
        public int Orden { get; set; } = 0;

        // ========== AUDITORÍA ==========
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        // ========== NAVEGACIÓN ==========
        
        /// <summary>
        /// Horarios programados de esta clase
        /// </summary>
        public ICollection<HorarioClase>? Horarios { get; set; }
    }
}
