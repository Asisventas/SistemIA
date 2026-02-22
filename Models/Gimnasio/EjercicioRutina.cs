using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.Gimnasio
{
    /// <summary>
    /// Ejercicio individual dentro de una rutina de gimnasio.
    /// Define series, repeticiones, peso y descanso para cada ejercicio.
    /// </summary>
    [Table("EjerciciosRutina")]
    public class EjercicioRutina
    {
        [Key]
        public int IdEjercicioRutina { get; set; }

        // ========== RELACIÓN CON RUTINA ==========
        
        /// <summary>
        /// Rutina a la que pertenece este ejercicio
        /// </summary>
        [Required]
        public int IdRutina { get; set; }
        [ForeignKey("IdRutina")]
        public RutinaGimnasio? Rutina { get; set; }

        // ========== DATOS DEL EJERCICIO ==========
        
        /// <summary>
        /// Nombre del ejercicio (ej: "Press de banca", "Sentadilla")
        /// </summary>
        [Required]
        [StringLength(100)]
        public string NombreEjercicio { get; set; } = string.Empty;

        /// <summary>
        /// Grupo muscular objetivo (ej: "Pecho", "Piernas", "Espalda")
        /// </summary>
        [Required]
        [StringLength(50)]
        public string GrupoMuscular { get; set; } = string.Empty;

        /// <summary>
        /// Descripción o instrucciones del ejercicio
        /// </summary>
        [StringLength(500)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// URL de video demostrativo (opcional)
        /// </summary>
        [StringLength(300)]
        public string? UrlVideo { get; set; }

        /// <summary>
        /// Imagen demostrativa (opcional)
        /// </summary>
        public byte[]? ImagenDemostrativa { get; set; }

        // ========== CONFIGURACIÓN DEL EJERCICIO ==========
        
        /// <summary>
        /// Orden del ejercicio dentro de la rutina
        /// </summary>
        public int Orden { get; set; } = 1;

        /// <summary>
        /// Número de series
        /// </summary>
        public int Series { get; set; } = 3;

        /// <summary>
        /// Número de repeticiones por serie
        /// </summary>
        public int Repeticiones { get; set; } = 12;

        /// <summary>
        /// Peso recomendado en kg (0 si es peso corporal)
        /// </summary>
        [Column(TypeName = "decimal(8,2)")]
        public decimal PesoRecomendadoKg { get; set; } = 0;

        /// <summary>
        /// Tiempo de descanso entre series en segundos
        /// </summary>
        public int DescansoSegundos { get; set; } = 60;

        /// <summary>
        /// Duración en segundos (para ejercicios de tiempo, ej: plancha)
        /// </summary>
        public int? DuracionSegundos { get; set; }

        // ========== ESTADO ==========
        
        /// <summary>
        /// Notas adicionales del entrenador
        /// </summary>
        [StringLength(300)]
        public string? Notas { get; set; }

        /// <summary>
        /// Activo (visible en la rutina)
        /// </summary>
        public bool Activo { get; set; } = true;

        // ========== PROPIEDADES CALCULADAS ==========
        
        [NotMapped]
        public string ResumenEjercicio => 
            DuracionSegundos.HasValue 
                ? $"{Series} x {DuracionSegundos}s" 
                : $"{Series} x {Repeticiones}";

        [NotMapped]
        public string PesoDescripcion => 
            PesoRecomendadoKg > 0 
                ? $"{PesoRecomendadoKg:N1} kg" 
                : "Peso corporal";
    }
}
