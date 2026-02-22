using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.Gimnasio
{
    /// <summary>
    /// Sesión de entrenamiento completada por el cliente.
    /// Registra qué ejercicios realizó y con qué peso/repeticiones.
    /// </summary>
    [Table("SesionesEntrenamiento")]
    public class SesionEntrenamiento
    {
        [Key]
        public int IdSesion { get; set; }

        // ========== RELACIONES ==========
        
        /// <summary>
        /// Cliente que realizó la sesión
        /// </summary>
        [Required]
        public int IdCliente { get; set; }
        [ForeignKey("IdCliente")]
        public Cliente? Cliente { get; set; }

        /// <summary>
        /// Rutina seguida (opcional si fue libre)
        /// </summary>
        public int? IdRutina { get; set; }
        [ForeignKey("IdRutina")]
        public RutinaGimnasio? Rutina { get; set; }

        /// <summary>
        /// Sucursal donde entrenó
        /// </summary>
        public int? IdSucursal { get; set; }
        [ForeignKey("IdSucursal")]
        public Sucursal? Sucursal { get; set; }

        // ========== DATOS DE LA SESIÓN ==========
        
        /// <summary>
        /// Fecha y hora de inicio
        /// </summary>
        [Required]
        public DateTime FechaHoraInicio { get; set; } = DateTime.Now;

        /// <summary>
        /// Fecha y hora de fin
        /// </summary>
        public DateTime? FechaHoraFin { get; set; }

        /// <summary>
        /// Duración en minutos
        /// </summary>
        public int DuracionMinutos { get; set; } = 0;

        /// <summary>
        /// Tipo de sesión: Rutina, Libre, Clase, Personal
        /// </summary>
        [Required]
        [StringLength(30)]
        public string TipoSesion { get; set; } = "Rutina";

        /// <summary>
        /// Estado: EnProgreso, Completada, Cancelada
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "EnProgreso";

        // ========== MÉTRICAS ==========
        
        /// <summary>
        /// Calorías estimadas quemadas
        /// </summary>
        public int? CaloriasQuemadas { get; set; }

        /// <summary>
        /// Frecuencia cardíaca promedio (si usa smartwatch)
        /// </summary>
        public int? FrecuenciaCardiacaPromedio { get; set; }

        /// <summary>
        /// Puntuación de esfuerzo percibido (1-10)
        /// </summary>
        public int? EsfuerzoPercibido { get; set; }

        // ========== OBSERVACIONES ==========
        
        /// <summary>
        /// Notas del cliente sobre la sesión
        /// </summary>
        [StringLength(500)]
        public string? Notas { get; set; }

        /// <summary>
        /// Cómo se sintió: Excelente, Bien, Regular, Mal
        /// </summary>
        [StringLength(20)]
        public string? EstadoAnimo { get; set; }

        // ========== NAVEGACIÓN ==========
        
        /// <summary>
        /// Detalle de ejercicios realizados
        /// </summary>
        public ICollection<DetalleSesionEjercicio>? Ejercicios { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        [NotMapped]
        public bool EstaEnProgreso => Estado == "EnProgreso";

        [NotMapped]
        public string DuracionFormateada
        {
            get
            {
                if (DuracionMinutos <= 0) return "-";
                var horas = DuracionMinutos / 60;
                var mins = DuracionMinutos % 60;
                return horas > 0 ? $"{horas}h {mins}min" : $"{mins} min";
            }
        }

        [NotMapped]
        public int CantidadEjercicios => Ejercicios?.Count ?? 0;
    }

    /// <summary>
    /// Detalle de un ejercicio realizado dentro de una sesión de entrenamiento.
    /// </summary>
    [Table("DetallesSesionEjercicio")]
    public class DetalleSesionEjercicio
    {
        [Key]
        public int IdDetalle { get; set; }

        // ========== RELACIONES ==========
        
        /// <summary>
        /// Sesión a la que pertenece
        /// </summary>
        [Required]
        public int IdSesion { get; set; }
        [ForeignKey("IdSesion")]
        public SesionEntrenamiento? Sesion { get; set; }

        /// <summary>
        /// Ejercicio de la rutina (opcional si fue libre)
        /// </summary>
        public int? IdEjercicioRutina { get; set; }
        [ForeignKey("IdEjercicioRutina")]
        public EjercicioRutina? EjercicioRutina { get; set; }

        // ========== DATOS DEL EJERCICIO ==========
        
        /// <summary>
        /// Nombre del ejercicio (copiado para historial)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string NombreEjercicio { get; set; } = string.Empty;

        /// <summary>
        /// Grupo muscular
        /// </summary>
        [StringLength(50)]
        public string? GrupoMuscular { get; set; }

        /// <summary>
        /// Orden en la sesión
        /// </summary>
        public int Orden { get; set; } = 1;

        // ========== SERIES REALIZADAS ==========
        
        /// <summary>
        /// Series completadas
        /// </summary>
        public int SeriesCompletadas { get; set; } = 0;

        /// <summary>
        /// Series planificadas (de la rutina)
        /// </summary>
        public int SeriesPlanificadas { get; set; } = 0;

        /// <summary>
        /// Repeticiones promedio por serie
        /// </summary>
        public int RepeticionesPromedio { get; set; } = 0;

        /// <summary>
        /// Peso utilizado en kg
        /// </summary>
        [Column(TypeName = "decimal(8,2)")]
        public decimal PesoUtilizadoKg { get; set; } = 0;

        /// <summary>
        /// Peso máximo levantado
        /// </summary>
        [Column(TypeName = "decimal(8,2)")]
        public decimal PesoMaximoKg { get; set; } = 0;

        /// <summary>
        /// Fue completado exitosamente
        /// </summary>
        public bool Completado { get; set; } = false;

        /// <summary>
        /// Notas sobre el ejercicio
        /// </summary>
        [StringLength(200)]
        public string? Notas { get; set; }

        // ========== PROPIEDADES CALCULADAS ==========
        
        [NotMapped]
        public decimal PorcentajeCompletado => 
            SeriesPlanificadas > 0 
                ? Math.Round((decimal)SeriesCompletadas / SeriesPlanificadas * 100, 0) 
                : 0;
    }
}
