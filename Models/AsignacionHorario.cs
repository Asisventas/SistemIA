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

        [Required]
        public DateTime FechaFin { get; set; }
    }
}