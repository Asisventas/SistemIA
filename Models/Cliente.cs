
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace SistemIA.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; } // Sin [StringLength]

        [StringLength(50)]
        public string? CodigoCliente { get; set; }

        [Required, StringLength(250)]
        public string RazonSocial { get; set; } = "";

        [Required]
        [StringLength(50, ErrorMessage = "El RUC debe tener como máximo 50 caracteres.")]
        public string RUC { get; set; } = "";

        [Required, StringLength(2)]
        public string TipoDocumento { get; set; } = "";

        [StringLength(50)]
        public string? NumeroDocumento { get; set; }

        public TiposDocumentosIdentidad? TipoDocumentoIdentidad { get; set; }
        public TiposContribuyentes? TipoContribuyente { get; set; }

        [Required]
        public int DV { get; set; }

        [StringLength(150)]
        public string? Direccion { get; set; }

        [StringLength(10)]
        public string? NumeroCasa { get; set; }

        [StringLength(100)]
        public string? CompDireccion1 { get; set; }

        [StringLength(100)]
        public string? CompDireccion2 { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(20)]
        public string? Celular { get; set; }

        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? CodigoEstablecimiento { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public DateTime? FechaAlta { get; set; }

        [StringLength(100)]
        public string? Contacto { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? LimiteCredito { get; set; }

        /// <summary>
        /// Habilita/deshabilita la venta a crédito y emisión de remisiones para este cliente
        /// </summary>
        public bool PermiteCredito { get; set; }

        [Required]
        public bool Estado { get; set; }

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
       
    // Se quita navegación antigua a 'Ciudades'; ahora IdCiudad apunta a CiudadCatalogo.Numero

        public bool EsExtranjero { get; set; }

        /// <summary>
        /// Tipo de operación SIFEN
        /// 1 = B2B (Business to Business) - Empresa a Empresa
        /// 2 = B2C (Business to Consumer) - Empresa a Consumidor Final
        /// 3 = B2G (Business to Government) - Empresa a Gobierno  
        /// 4 = B2F (Business to Foreigner) - Empresa a Extranjero
        /// </summary>
        [StringLength(3)]
        public string? TipoOperacion { get; set; }

        [ForeignKey(nameof(TipoOperacion))]
        public TipoOperacion? TipoOperacionNavigation { get; set; }

        // Campos específicos para SIFEN - Agregados para cumplir con Manual Técnico v150
        
        /// <summary>
        /// iNatRec - Naturaleza del receptor SIFEN
        /// 1 = Contribuyente (Persona con RUC activo)
        /// 2 = No contribuyente (Persona sin RUC - consumidor final)
        /// </summary>
        [Required]
        public int NaturalezaReceptor { get; set; } = 1;

        /// <summary>
        /// cDepRec - Código del departamento según catálogo SIFEN
        /// Ejemplo: "01"=Capital, "02"=San Pedro, etc.
        /// </summary>
        [StringLength(2)]
        public string? CodigoDepartamento { get; set; }

        /// <summary>
        /// dDesDepRec - Descripción del departamento SIFEN
        /// Ejemplo: "CAPITAL", "SAN PEDRO"
        /// </summary>
        [StringLength(16)]
        public string? DescripcionDepartamento { get; set; }

        /// <summary>
        /// cDisRec - Código del distrito según catálogo SIFEN
        /// Ejemplo: "0001"=Asunción, "0002"=San Lorenzo
        /// </summary>
        [StringLength(4)]
        public string? CodigoDistrito { get; set; }

        /// <summary>
        /// dDesDisRec - Descripción del distrito SIFEN
        /// Ejemplo: "ASUNCION", "SAN LORENZO"
        /// </summary>
        [StringLength(30)]
        public string? DescripcionDistrito { get; set; }

        /// <summary>
        /// dNomFanRec - Nombre de fantasía del receptor
        /// Nombre comercial con el que se conoce la empresa
        /// </summary>
        [StringLength(255)]
        public string? NombreFantasia { get; set; }

        /// <summary>
        /// iTipIDRec - Tipo de documento para no contribuyentes
        /// 1=CI Paraguaya, 2=Pasaporte, 3=CI Extranjera, 4=Otro
        /// </summary>
        public int? TipoDocumentoIdentidadSifen { get; set; }

        /// <summary>
        /// dNumIDRec - Número de documento para no contribuyentes
        /// </summary>
        [StringLength(20)]
        public string? NumeroDocumentoIdentidad { get; set; }

       
    }
}
