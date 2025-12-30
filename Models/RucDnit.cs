using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Tabla de RUC oficiales de la DNIT (Dirección Nacional de Ingresos Tributarios)
    /// Fuente: https://www.dnit.gov.py/web/portal-institucional/listado-de-ruc-con-sus-equivalencias
    /// </summary>
    [Table("RucDnit")]
    public class RucDnit
    {
        [Key]
        [StringLength(20)]
        public string RUC { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string RazonSocial { get; set; } = string.Empty;

        public int DV { get; set; }

        [StringLength(20)]
        public string? Estado { get; set; }

        /// <summary>
        /// Fecha de última actualización de los datos
        /// </summary>
        public DateTime? FechaActualizacion { get; set; }
    }
}
