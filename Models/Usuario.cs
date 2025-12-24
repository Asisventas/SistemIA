using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models // ¡Este es el namespace que debe envolver tu clase!
{
    public class Usuario
    {
        [Key]
        public int Id_Usu { get; set; } // Tu clave primaria tal como la tienes

        // --- SE AÑADE LA NUEVA RELACIÓN A MUCHAS ASIGNACIONES ---
        public ICollection<AsignacionHorario>? Asignaciones { get; set; }
        // ----------------------------------------------------------

        [Required, MaxLength(100)]
        public string Nombres { get; set; } = string.Empty; // Inicializado

        [Required, MaxLength(100)]
        public string Apellidos { get; set; } = string.Empty; // Inicializado

        [MaxLength(200)]
        public string? Direccion { get; set; }

        [MaxLength(100)]
        public string? Ciudad { get; set; }

        [MaxLength(15)]
        public string? CI { get; set; }

        [MaxLength(150)]
        public string? Correo { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Required]
        public DateTime Fecha_Ingreso { get; set; } = DateTime.Now;

        [Required]
        public int Id_Rol { get; set; }

        [Required]
        public bool Estado_Usu { get; set; } = true;

        [Required, MaxLength(50)]
        public string UsuarioNombre { get; set; } = string.Empty; // Inicializado

        [Required] // Asegúrate de que ContrasenaHash sea requerido y tenga un valor inicial no nulo
        public byte[] ContrasenaHash { get; set; } = Array.Empty<byte>();

        public byte[]? Foto { get; set; }

        public byte[]? EmbeddingFacial { get; set; }

        public DateTime? Fecha_Nacimiento { get; set; }

        public byte[]? HuellaDigital { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Salario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? IPS { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Comision { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Descuento { get; set; }

        [NotMapped]
        public decimal? Salario_Neto =>
            (Salario ?? 0) + (Comision ?? 0) - (Descuento ?? 0) - (IPS ?? 0);

        // Propiedad de navegación. Asegúrate de que Rol también esté en SistemIA.Models
        public Rol? Rol { get; set; }

        // Propiedad de navegación para colecciones (si las tienes)
        // Por ejemplo, si un usuario puede tener muchas asistencias:
        // public ICollection<Asistencia>? Asistencias { get; set; }
    }
}