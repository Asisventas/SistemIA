
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace SistemIA.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; } // Sin [StringLength]

        [Required, StringLength(250)]
        public string RazonSocial { get; set; } = "";


        [Required]
        [StringLength(8, ErrorMessage = "El RUC debe tener como máximo 8 caracteres.")]
        public string RUC { get; set; } = "";

        [Required, StringLength(2)]
        public string TipoDocumento { get; set; } = "";

        public TiposDocumentosIdentidad? TipoDocumentoIdentidad { get; set; }
        public TiposContribuyentes? TipoContribuyente { get; set; }

        [Required]
        public int DV { get; set; }

        [StringLength(150)]
        public string? Direccion { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(150)]
        public string? Email { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [StringLength(100)]
        public string? Contacto { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? LimiteCredito { get; set; }

        [Required]
        [StringLength(20)]
        public string? Estado { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Saldo { get; set; }

        [Required]
        public int IdTipoContribuyente { get; set; }

        [StringLength(8)]
        public string? Timbrado { get; set; }

        public DateTime? VencimientoTimbrado { get; set; }

        public bool PrecioDiferenciado { get; set; }

        public int? PlazoDiasCredito { get; set; }

        [Required]
        [StringLength(3)]
        public string CodigoPais { get; set; } = "";
       
        public Paises? Pais { get; set; }
      
        public int IdCiudad { get; set; }
       
        public Ciudades? Ciudad { get; set; }

        public bool EsExtranjero { get; set; }
        [Required]

        [StringLength(3)]
        public string? TipoOperacion { get; set; }

        [ForeignKey(nameof(TipoOperacion))]
        public TipoOperacion? TipoOperacionNavigation { get; set; }

       
    }
}
