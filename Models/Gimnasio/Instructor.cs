using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models.Gimnasio
{
    /// <summary>
    /// Instructor/Entrenador del gimnasio.
    /// Puede dictar clases grupales y sesiones de entrenamiento personal.
    /// </summary>
    [Table("Instructores")]
    public class Instructor
    {
        [Key]
        public int IdInstructor { get; set; }

        // ========== SUCURSAL ==========
        
        /// <summary>
        /// Sucursal donde trabaja (null = todas las sucursales)
        /// </summary>
        public int? IdSucursal { get; set; }
        [ForeignKey("IdSucursal")]
        public Sucursal? Sucursal { get; set; }

        // ========== DATOS PERSONALES ==========
        
        /// <summary>
        /// Nombre completo del instructor
        /// </summary>
        [Required]
        [StringLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Cédula de identidad
        /// </summary>
        [StringLength(20)]
        public string? Cedula { get; set; }

        /// <summary>
        /// Teléfono de contacto
        /// </summary>
        [StringLength(30)]
        public string? Telefono { get; set; }

        /// <summary>
        /// Email de contacto
        /// </summary>
        [StringLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Foto del instructor (opcional)
        /// </summary>
        public byte[]? Foto { get; set; }

        // ========== ESPECIALIZACIÓN ==========
        
        /// <summary>
        /// Especialidades del instructor (ej: "Spinning, Funcional, Yoga")
        /// </summary>
        [StringLength(300)]
        public string? Especialidades { get; set; }

        /// <summary>
        /// Certificaciones/títulos del instructor
        /// </summary>
        [StringLength(500)]
        public string? Certificaciones { get; set; }

        /// <summary>
        /// Es Personal Trainer (atiende clientes individuales)
        /// </summary>
        public bool EsPersonalTrainer { get; set; } = false;

        /// <summary>
        /// Dicta clases grupales
        /// </summary>
        public bool DictaClasesGrupales { get; set; } = true;

        // ========== CONTRATACIÓN ==========
        
        /// <summary>
        /// Fecha de ingreso al gimnasio
        /// </summary>
        public DateTime? FechaIngreso { get; set; }

        /// <summary>
        /// Tipo de contrato: Fijo, Por Hora, Freelance
        /// </summary>
        [StringLength(30)]
        public string? TipoContrato { get; set; } = "Fijo";

        /// <summary>
        /// Salario o tarifa por hora (según tipo contrato)
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal? TarifaHora { get; set; }

        // ========== HORARIOS ==========
        
        /// <summary>
        /// Días disponibles para trabajar (ej: "L-V" o "L,M,X,J,V,S")
        /// </summary>
        [StringLength(30)]
        public string? DiasDisponibles { get; set; }

        /// <summary>
        /// Horario de disponibilidad (ej: "06:00-14:00")
        /// </summary>
        [StringLength(50)]
        public string? HorarioDisponible { get; set; }

        // ========== ESTADO ==========
        
        /// <summary>
        /// Indica si el instructor está activo
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Color identificador (para calendario)
        /// </summary>
        [StringLength(7)]
        public string? ColorHex { get; set; } = "#17a2b8";

        // ========== AUDITORÍA ==========
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        // ========== NAVEGACIÓN ==========
        
        /// <summary>
        /// Clases que dicta este instructor
        /// </summary>
        public ICollection<ClaseGimnasio>? Clases { get; set; }
    }
}
