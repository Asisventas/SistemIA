using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    public class SociedadActividadEconomica
    {
        [Key]
        public int Numero { get; set; } // PK identidad

        [Required]
        public int IdSociedad { get; set; } // FK a Sociedad

        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string CodigoActividad { get; set; } = string.Empty; // cActEco

        [StringLength(300)]
        [Column(TypeName = "varchar(300)")]
        public string? NombreActividad { get; set; } // dDesActEco

        [StringLength(1)]
        [Column(TypeName = "char(1)")]
        public string? ActividadPrincipal { get; set; } // 'S' / 'N'

        // Navegación opcional
        public Sociedad? Sociedad { get; set; }
    }
}
