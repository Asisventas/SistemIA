using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using SistemIA.Models; // Asegúrate de tener este using para Sucursal

namespace SistemIA.Models
{
    public class HorarioTrabajo
    {
        [Key]
        public int Id_Horario { get; set; }

        // Propiedad de clave foránea para la Sucursal
        [Column("Id_Sucursal")] // <-- Asegúrate de que el nombre de la columna en BD sea este
        public int Id_Sucursal { get; set; } // <-- ¡Propiedad corregida aquí!
        [ForeignKey("Id_Sucursal")]
        public Sucursal? SucursalNavigation { get; set; } // Propiedad de navegación a la Sucursal

        [Required]
        [StringLength(100)]
        public string NombreHorario { get; set; } = string.Empty; // Inicializado para evitar nulabilidad

        [Required]
        public TimeSpan HoraEntrada { get; set; }

        [Required]
        public TimeSpan HoraSalida { get; set; }

        public TimeSpan? InicioBreak { get; set; }
        public TimeSpan? FinBreak { get; set; }

        [Required]
        public bool Lunes { get; set; } = false;
        [Required]
        public bool Martes { get; set; } = false;
        [Required]
        public bool Miercoles { get; set; } = false;
        [Required]
        public bool Jueves { get; set; } = false;
        [Required]
        public bool Viernes { get; set; } = false;
        [Required]
        public bool Sabado { get; set; } = false;
        [Required]
        public bool Domingo { get; set; } = false;

        public bool EsActivo { get; set; } = true;

        [StringLength(255)]
        public string? Descripcion { get; set; }

        // --- SE REEMPLAZA LA COLECCIÓN DE USUARIOS POR ASIGNACIONES ---
        // public ICollection<Usuario>? Usuarios { get; set; }
        public ICollection<AsignacionHorario>? Asignaciones { get; set; }
        // ----------------------------------------------------------
    }
}