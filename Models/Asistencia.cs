using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemIA.Models;

namespace SistemIA.Models
{
    public class Asistencia
    {
        [Key]
        public int Id_Asistencia { get; set; }

        [Required]
        [Column("Id_Usuario")]
        public int Id_Usuario { get; set; }
        [ForeignKey("Id_Usuario")]
        public Usuario? Usuario { get; set; }

        [Required]
        // Esta propiedad es la clave foránea que apunta a 'Id' en la tabla 'Sucursal'
        public int Sucursal { get; set; } // ¡Ahora es int, compatible con Id de Sucursal!
        [ForeignKey("Sucursal")] // Se relaciona con la propiedad 'Id' de Sucursal
        public Sucursal? SucursalNavigation { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoRegistro { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Notas { get; set; }

        [StringLength(50)]
        public string? Ubicacion { get; set; }

        public DateTime? FechaInicioAusencia { get; set; }
        public DateTime? FechaFinAusencia { get; set; }

        [StringLength(100)]
        public string? MotivoAusencia { get; set; }

        public bool AprobadaPorGerencia { get; set; } = false;

        [Column("AprobadoPorId_Usuario")]
        public int? AprobadoPorId_Usuario { get; set; }
        [ForeignKey("AprobadoPorId_Usuario")]
        public Usuario? AprobadoPorUsuario { get; set; }

        public DateTime? FechaAprobacion { get; set; }
    }
}