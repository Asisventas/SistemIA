using System;
using System.ComponentModel.DataAnnotations;

namespace SistemIA.Models
{
    /// <summary>
    /// Timbrado vigente por sucursal/caja y documento
    /// </summary>
    public class Timbrado
    {
        [Key]
        public int IdTimbrado { get; set; }

        [Required, StringLength(8)]
        public string NumeroTimbrado { get; set; } = string.Empty; // 8 dígitos

        [Required]
        public DateTime FechaInicioVigencia { get; set; }

        [Required]
        public DateTime FechaFinVigencia { get; set; }

        // Numeración
        [Required, StringLength(3)] public string Establecimiento { get; set; } = string.Empty;
        [Required, StringLength(3)] public string PuntoExpedicion { get; set; } = string.Empty;
        [StringLength(12)] public string? TipoDocumento { get; set; } // Factura, NotaCredito, etc.

        // Alcance
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }
        public int? IdCaja { get; set; }
        public Caja? Caja { get; set; }

        // Estado
        public bool Activo { get; set; } = true;
    }
}
