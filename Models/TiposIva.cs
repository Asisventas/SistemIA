using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("TiposIva")]
    public class TiposIva
    {
        [Key]
        public int IdTipoIva { get; set; }

        [Required]
        [Display(Name = "Código SIFEN")]
        public int CodigoSifen { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Porcentaje")]
        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
        public decimal Porcentaje { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Tasa SIFEN")]
        [Range(0, 100, ErrorMessage = "La tasa SIFEN debe estar entre 0 y 100")]
        public decimal TasaSifen { get; set; }

        [Required]
        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Es Predeterminado")]
        public bool EsPredeterminado { get; set; } = false;

        [StringLength(10)]
        [Display(Name = "Código Abreviado")]
        public string? CodigoAbreviado { get; set; }

        [StringLength(255)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Required]
        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "Usuario de Creación")]
        public string? UsuarioCreacion { get; set; }

        [Display(Name = "Fecha de Modificación")]
        public DateTime? FechaModificacion { get; set; }

        [StringLength(50)]
        [Display(Name = "Usuario de Modificación")]
        public string? UsuarioModificacion { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public string DescripcionCompleta => $"{Descripcion} ({Porcentaje:0.##}%)";

        // Métodos de cálculo
        public decimal CalcularMontoIva(decimal valorBase)
        {
            return Math.Round(valorBase * (Porcentaje / 100), 2);
        }

        public decimal CalcularValorConIva(decimal valorBase)
        {
            return Math.Round(valorBase + CalcularMontoIva(valorBase), 2);
        }

        public decimal CalcularValorBase(decimal valorConIva)
        {
            if (Porcentaje == 0) return valorConIva;
            return Math.Round(valorConIva / (1 + (Porcentaje / 100)), 2);
        }

        // Validaciones de negocio
        public bool EsValidoParaSifen()
        {
            // Validar códigos SIFEN válidos para Paraguay
            var codigosValidos = new[] { 1, 2, 3, 4, 5 };
            if (!codigosValidos.Contains(CodigoSifen))
                return false;

            // Validar coherencia entre código y porcentaje
            switch (CodigoSifen)
            {
                case 1: // IVA
                    return Porcentaje > 0;
                case 4: // No tributado
                case 5: // Exonerado
                    return Porcentaje == 0;
                default:
                    return true;
            }
        }

        public override string ToString()
        {
            return DescripcionCompleta;
        }
    }
}
