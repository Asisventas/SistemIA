using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("Monedas")]
    public class Moneda
    {
        [Key]
        public int IdMoneda { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Código ISO")]
        public string CodigoISO { get; set; } = string.Empty; // PYG, ARS, BRL, USD

        [Required]
        [StringLength(50)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty; // Guaraní Paraguayo, Peso Argentino, Real Brasileño

        [Required]
        [StringLength(10)]
        [Display(Name = "Símbolo")]
        public string Simbolo { get; set; } = string.Empty; // ₲, $, R$

        [Display(Name = "Es Moneda Base")]
        public bool EsMonedaBase { get; set; } = false; // Guaraní será la base

        [Display(Name = "Activo")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Orden")]
        public int Orden { get; set; } = 0;

        // Propiedades de auditoría
        [Display(Name = "Fecha Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "Usuario Creación")]
        public string UsuarioCreacion { get; set; } = string.Empty;

        [Display(Name = "Fecha Modificación")]
        public DateTime? FechaModificacion { get; set; }

        [StringLength(50)]
        [Display(Name = "Usuario Modificación")]
        public string? UsuarioModificacion { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public string NombreCompleto => $"{Nombre} ({CodigoISO})";

        [NotMapped]
        public string DisplayName => $"{Simbolo} {Nombre}";
    }
}
