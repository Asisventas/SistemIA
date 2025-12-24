using System;
using System.ComponentModel.DataAnnotations;

namespace SistemIA.Models
{
    public class Sociedad
    {
        [Key]
        public int IdSociedad { get; set; }

        // Identificación de la empresa (Emisor SIFEN)
        [Required, StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string RUC { get; set; } = string.Empty;

        public int? DV { get; set; }

        [Required, StringLength(255)]
        public string Direccion { get; set; } = string.Empty;

        [StringLength(10)]
        public string? NumeroCasa { get; set; }

        // Ubicación
        public int? Departamento { get; set; }
        public int? Ciudad { get; set; }
        public int? Distrito { get; set; }

        // Contacto
        [StringLength(50)]
        public string? Telefono { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        // Tipo Contribuyente (catálogo DNIT/SIFEN)
        public int? TipoContribuyente { get; set; }

        // CSC (Control de seguridad del contribuyente) para QR
        [StringLength(50)]
        public string? IdCsc { get; set; }

        [StringLength(50)]
        public string? Csc { get; set; }

        // Rutas/credenciales de certificados y archivos
        [StringLength(400)] public string? PathCertificadoP12 { get; set; }
        [StringLength(400)] public string? PasswordCertificadoP12 { get; set; }
        [StringLength(400)] public string? PathCertificadoPem { get; set; }
        [StringLength(400)] public string? PathCertificadoCrt { get; set; }
        [StringLength(400)] public string? PathArchivoSinFirma { get; set; }
        [StringLength(400)] public string? PathArchivoFirmado { get; set; }

        // URLs de servicios SIFEN
        [StringLength(400)] public string? DeUrlQr { get; set; }
        [StringLength(400)] public string? DeUrlEnvioDocumento { get; set; }
        [StringLength(400)] public string? DeUrlEnvioEvento { get; set; }
        [StringLength(400)] public string? DeUrlEnvioDocumentoLote { get; set; }
        [StringLength(400)] public string? DeUrlConsultaDocumento { get; set; }
        [StringLength(400)] public string? DeUrlConsultaDocumentoLote { get; set; }
        [StringLength(400)] public string? DeUrlConsultaRuc { get; set; }

        [StringLength(50)] public string? ServidorSifen { get; set; } // prod/test
        public int? DeConexion { get; set; }

        // Auditoría simple
        public DateTime? FechaAuditoria { get; set; }
        [StringLength(10)] public string? Usuario { get; set; }
    }
}
