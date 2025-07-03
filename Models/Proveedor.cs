using System.ComponentModel.DataAnnotations;

namespace SistemIA.Models
{
    public class Proveedor
    {
        [Key]
        public int Id_Proveedor { get; set; }

        [Required]
        public string CodigoProveedor { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Rubro { get; set; }
        public string RUC { get; set; } = null!;
        public int? DV { get; set; }
        public string? Correo { get; set; }
        public string? Contacto { get; set; }
        public byte[]? FotoLogo { get; set; }
        public string? EstadoProveedor { get; set; }
        public DateTime? VencimientoTimbrado { get; set; }
        public string? Timbrado { get; set; }
        public int? IdTipoContribuyente { get; set; }
        public int? CodigoContribuyente { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string? UsuarioCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        public TiposContribuyentes? TipoContribuyente { get; set; }
    }
}
