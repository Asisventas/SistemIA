
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

        [NotMapped]
        public TiposDocumentosIdentidad? TipoDocumentoIdentidad { get; set; }
        [NotMapped]
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

        /// <summary>
        /// Indica si se debe enviar automáticamente la factura por correo electrónico al confirmar la venta
        /// Requiere que el cliente tenga un correo electrónico válido configurado
        /// </summary>
        [Display(Name = "Enviar Factura por Correo")]
        public bool EnviarFacturaPorCorreo { get; set; } = false;

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

        // ========== CONFIGURACIÓN SOPORTE REMOTO (VPN) ==========
        
        /// <summary>
        /// Indica si este cliente tiene habilitado el acceso remoto para soporte
        /// </summary>
        public bool HabilitadoSoporteRemoto { get; set; } = false;

        /// <summary>
        /// IP asignada en la VPN PPTP (ej: 192.168.89.10)
        /// Cada cliente tiene una IP fija para identificarlo
        /// </summary>
        [StringLength(15)]
        public string? IpVpnAsignada { get; set; }

        /// <summary>
        /// Puerto donde corre SistemIA en el cliente (default: 5095)
        /// </summary>
        public int? PuertoSistema { get; set; } = 5095;

        /// <summary>
        /// Ruta de instalación de SistemIA en el cliente
        /// (ej: C:\SistemIA o C:\Program Files\SistemIA)
        /// </summary>
        [StringLength(255)]
        public string? RutaCarpetaSistema { get; set; }

        /// <summary>
        /// Nombre del servicio Windows si está instalado como servicio
        /// (ej: SistemIA.Service)
        /// </summary>
        [StringLength(100)]
        public string? NombreServicioWindows { get; set; }

        /// <summary>
        /// Cadena de conexión a la base de datos del cliente (encriptada/ofuscada)
        /// Solo para consultas de soporte
        /// </summary>
        [StringLength(500)]
        public string? CadenaConexionBD { get; set; }

        /// <summary>
        /// Notas adicionales para acceso remoto
        /// </summary>
        [StringLength(1000)]
        public string? NotasAccesoRemoto { get; set; }

        /// <summary>
        /// Última fecha de conexión remota exitosa
        /// </summary>
        public DateTime? UltimaConexionRemota { get; set; }

        // ========== GIMNASIO ==========

        /// <summary>
        /// Indica si el cliente es miembro del gimnasio
        /// </summary>
        public bool EsMiembroGimnasio { get; set; } = false;

        /// <summary>
        /// Código de cliente para gimnasio (opcional, para uso manual)
        /// </summary>
        [StringLength(20)]
        public string? CodigoGimnasio { get; set; }

        /// <summary>
        /// Embedding facial para reconocimiento (128 floats = 512 bytes)
        /// </summary>
        public byte[]? EmbeddingFacialGimnasio { get; set; }

        /// <summary>
        /// Fecha de captura del embedding facial
        /// </summary>
        public DateTime? FechaCaptuaFacial { get; set; }

        /// <summary>
        /// Foto del cliente para verificación en gimnasio
        /// </summary>
        public byte[]? FotoGimnasio { get; set; }

        /// <summary>
        /// Fecha de alta como miembro del gimnasio
        /// </summary>
        public DateTime? FechaAltaGimnasio { get; set; }

        /// <summary>
        /// Objetivo fitness del cliente (ej: Pérdida de peso, Ganancia muscular, Mantenimiento)
        /// </summary>
        [StringLength(100)]
        public string? ObjetivoFitness { get; set; }

        /// <summary>
        /// Condiciones médicas a considerar
        /// </summary>
        [StringLength(500)]
        public string? CondicionesMedicas { get; set; }

        /// <summary>
        /// Contacto de emergencia
        /// </summary>
        [StringLength(100)]
        public string? ContactoEmergencia { get; set; }

        /// <summary>
        /// Teléfono de contacto de emergencia
        /// </summary>
        [StringLength(20)]
        public string? TelefonoEmergencia { get; set; }

        /// <summary>
        /// Nota/mensaje personalizado de bienvenida para este cliente
        /// </summary>
        [StringLength(200)]
        public string? MensajeBienvenidaPersonalizado { get; set; }

        /// <summary>
        /// Total de visitas históricas al gimnasio
        /// </summary>
        public int TotalVisitasGimnasio { get; set; } = 0;

        /// <summary>
        /// Fecha de última visita al gimnasio
        /// </summary>
        public DateTime? FechaUltimaVisitaGimnasio { get; set; }

        // ========== UBICACIÓN GPS (GOOGLE MAPS) ==========

        /// <summary>
        /// Latitud de la ubicación del cliente (Google Maps)
        /// </summary>
        [Column(TypeName = "decimal(10,7)")]
        public decimal? Latitud { get; set; }

        /// <summary>
        /// Longitud de la ubicación del cliente (Google Maps)
        /// </summary>
        [Column(TypeName = "decimal(10,7)")]
        public decimal? Longitud { get; set; }

        /// <summary>
        /// Dirección completa formateada para GPS
        /// </summary>
        [StringLength(500)]
        public string? DireccionCompleta { get; set; }

        /// <summary>
        /// Color asignado al cliente en calendario/agenda (hexadecimal)
        /// </summary>
        [StringLength(20)]
        public string? ColorAgenda { get; set; }

        /// <summary>
        /// Genera la URL de Google Maps basada en las coordenadas o dirección
        /// </summary>
        [NotMapped]
        public string UrlGoogleMaps
        {
            get
            {
                if (Latitud.HasValue && Longitud.HasValue)
                {
                    return $"https://www.google.com/maps?q={Latitud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)},{Longitud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                }
                else if (!string.IsNullOrEmpty(DireccionCompleta))
                {
                    return $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(DireccionCompleta)}";
                }
                else if (!string.IsNullOrEmpty(Direccion))
                {
                    return $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(Direccion)}";
                }
                return string.Empty;
            }
        }

        // ========== PROPIEDADES DE NAVEGACIÓN GIMNASIO ==========

        /// <summary>
        /// Historial de membresías del cliente
        /// </summary>
        public virtual ICollection<Gimnasio.MembresiaCliente>? Membresias { get; set; }

        /// <summary>
        /// Historial de accesos al gimnasio
        /// </summary>
        public virtual ICollection<Gimnasio.AccesoGimnasio>? AccesosGimnasio { get; set; }

        /// <summary>
        /// Citas asociadas a este cliente
        /// </summary>
        public virtual ICollection<CitaAgenda>? CitasAgenda { get; set; }
    }
}
