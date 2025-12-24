using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    public class AsignacionHorario
    {
        [Key]
        public int Id_Asignacion { get; set; }

        [Required]
        public int Id_Usuario { get; set; }
        [ForeignKey("Id_Usuario")]
        public Usuario? Usuario { get; set; }

        [Required]
        public int Id_Horario { get; set; }
        [ForeignKey("Id_Horario")]
        public HorarioTrabajo? HorarioTrabajo { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        [Required]
        public bool Estado { get; set; } = true;

        public DateTime FechaAsignacion { get; set; } = DateTime.Now;

        public int? AsignadoPorId_Usuario { get; set; }

        public string? Observaciones { get; set; }
    }
}