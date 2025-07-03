using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    public class Sucursal
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(7)]
        public string NumSucursal { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string NombreSucursal { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string NombreEmpresa { get; set; } = string.Empty;

        [StringLength(150)]
        public string? RubroEmpresa { get; set; }

        [Required, StringLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        public int? IdCiudad { get; set; }  // Clave foránea
        public Ciudades? Ciudad { get; set; }  // Navegación

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(100), EmailAddress]
        public string? Correo { get; set; }

        [Required, StringLength(8)]
        public string RUC { get; set; } = string.Empty;

        [Required]
        public int? DV { get; set; } // ✅ ahora compatible con InputNumber

        [StringLength(250)]
        public string? CertificadoRuta { get; set; }

        [StringLength(250)]
        public string? CertificadoPassword { get; set; }

        [StringLength(10)]
        public string? PuntoExpedicion { get; set; }

        public bool SistemaPlaya { get; set; } = false;

        public bool Automatizado { get; set; } = false;

        [StringLength(15)]
        public string? IpConsola { get; set; }

        [StringLength(10)]
        public string? PuertoConsola { get; set; }

        public byte[]? Logo { get; set; }

        [StringLength(200)]
        public string? Conexion { get; set; }
    }
}
