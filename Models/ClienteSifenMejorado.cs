using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    [Table("Clientes")]
    public class ClienteSifenMejorado
    {
        [Key]
        public int IdCliente { get; set; }

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
       
    // IdCiudad referencia ahora a CiudadCatalogo.Numero

        public bool EsExtranjero { get; set; }
        
        /// <summary>
        /// Tipo de operación para SIFEN
        /// 1 = B2B (Business to Business) - Empresa a Empresa
        /// 2 = B2C (Business to Consumer) - Empresa a Consumidor Final
        /// 3 = B2G (Business to Government) - Empresa a Gobierno
        /// 4 = B2F (Business to Foreigner) - Empresa a Extranjero
        /// </summary>
        [StringLength(1)]
        public string? TipoOperacion { get; set; }

        [ForeignKey(nameof(TipoOperacion))]
        public TipoOperacion? TipoOperacionNavigation { get; set; }

        // ===== CAMPOS ADICIONALES PARA SIFEN =====
        
        /// <summary>
        /// Naturaleza del receptor SIFEN
        /// Campo: iNatRec
        /// 1 = Contribuyente (Persona con RUC activo)
        /// 2 = No contribuyente (Persona sin RUC - consumidor final)
        /// </summary>
        [Required]
        [Range(1, 2, ErrorMessage = "Naturaleza debe ser 1 (Contribuyente) o 2 (No contribuyente)")]
        public int NaturalezaReceptor { get; set; } = 1;

        /// <summary>
        /// Código del departamento según catálogo SIFEN
        /// Campo: cDepRec
        /// Ejemplo: "01"=Capital, "02"=San Pedro, "03"=Cordillera, etc.
        /// Consultar catálogo oficial de departamentos en Manual SIFEN
        /// </summary>
        [StringLength(2)]
        public string? CodigoDepartamento { get; set; }

        /// <summary>
        /// Descripción del departamento
        /// Campo: dDesDepRec
        /// Ejemplo: "CAPITAL", "SAN PEDRO", "CORDILLERA"
        /// Máximo 16 caracteres según XSD SIFEN
        /// </summary>
        [StringLength(16)]
        public string? DescripcionDepartamento { get; set; }

        /// <summary>
        /// Código del distrito según catálogo SIFEN
        /// Campo: cDisRec
        /// Ejemplo: "0001"=Asunción, "0002"=San Lorenzo, etc.
        /// Consultar catálogo oficial de distritos en Manual SIFEN
        /// </summary>
        [StringLength(4)]
        public string? CodigoDistrito { get; set; }

        /// <summary>
        /// Descripción del distrito
        /// Campo: dDesDisRec
        /// Ejemplo: "ASUNCION", "SAN LORENZO", "LUQUE"
        /// Máximo 30 caracteres según XSD SIFEN
        /// </summary>
        [StringLength(30)]
        public string? DescripcionDistrito { get; set; }

        /// <summary>
        /// Nombre de fantasía del receptor
        /// Campo: dNomFanRec
        /// Nombre comercial o de fantasía con el que se conoce la empresa
        /// Ejemplo: "McDonald's" para "Arcos Dorados S.A."
        /// Opcional - máximo 255 caracteres
        /// </summary>
        [StringLength(255)]
        public string? NombreFantasia { get; set; }

        /// <summary>
        /// Tipo de documento de identidad para no contribuyentes
        /// Campo: iTipIDRec (SIFEN) - Solo usar si NaturalezaReceptor = 2
        /// 1 = Cédula de Identidad Paraguaya
        /// 2 = Pasaporte
        /// 3 = Cédula de Identidad Extranjera
        /// 4 = Otro documento de identidad
        /// </summary>
        public int? TipoDocumentoIdentidadSifen { get; set; }

        /// <summary>
        /// Número de documento de identidad para no contribuyentes
        /// Campo: dNumIDRec (SIFEN) - Solo usar si NaturalezaReceptor = 2
        /// Número del documento según el tipo especificado arriba
        /// Máximo 20 caracteres
        /// </summary>
        [StringLength(20)]
        public string? NumeroDocumentoIdentidad { get; set; }

        // ===== PROPIEDADES CALCULADAS PARA SIFEN =====

        /// <summary>
        /// Obtiene el valor para iNatRec según el tipo de contribuyente
        /// Lógica: Si tiene RUC válido = Contribuyente (1), sino = No contribuyente (2)
        /// </summary>
        /// <returns>1 = Contribuyente con RUC, 2 = No contribuyente sin RUC</returns>
        public int ObtenerNaturalezaSifen()
        {
            // Si tiene RUC válido, es contribuyente (1), sino no contribuyente (2)
            return (!string.IsNullOrEmpty(RUC) && RUC != "0") ? 1 : 2;
        }

        /// <summary>
        /// Obtiene el valor numérico para iTiOpe (tipo de operación SIFEN)
        /// Convierte códigos de texto a números requeridos por SIFEN
        /// </summary>
        /// <returns>
        /// 1 = B2B (Business to Business) - Empresa a Empresa/Extranjero (RUC >= 50M)
        /// 2 = B2C (Business to Client) - Empresa a Cliente (RUC < 50M)  
        /// 3 = B2G (Business to Government) - Empresa a Gobierno
        /// 4 = B2F (Business to Foreigner) - Empresa a Extranjero
        /// </returns>
        public int ObtenerTipoOperacionSifen()
        {
            return TipoOperacion switch
            {
                "1" => 1, // B2B - Business to Business
                "2" => 2, // B2C - Business to Client
                "3" => 3, // B2G - Business to Government
                "4" => 4, // B2F - Business to Foreigner
                _ => 1   // Por defecto B2B
            };
        }

        /// <summary>
        /// Obtiene descripción legible del tipo de operación
        /// </summary>
        /// <returns>Descripción completa del tipo de operación</returns>
        public string ObtenerDescripcionTipoOperacion()
        {
            return TipoOperacion switch
            {
                "1" => "B2B - Empresa a Empresa/Extranjero",
                "2" => "B2C - Empresa a Cliente",
                "3" => "B2G - Empresa a Gobierno",
                "4" => "B2F - Empresa a Extranjero",
                _ => "B2B - Empresa a Empresa (Por defecto)"
            };
        }

        /// <summary>
        /// Obtiene descripción legible de la naturaleza del receptor
        /// </summary>
        /// <returns>Descripción completa de la naturaleza</returns>
        public string ObtenerDescripcionNaturaleza()
        {
            return NaturalezaReceptor switch
            {
                1 => "Contribuyente - Persona con RUC activo",
                2 => "No contribuyente - Consumidor final sin RUC",
                _ => "No especificado"
            };
        }

        /// <summary>
        /// Obtiene descripción del tipo de documento para no contribuyentes
        /// </summary>
        /// <returns>Descripción del tipo de documento</returns>
        public string ObtenerDescripcionTipoDocumento()
        {
            if (TipoDocumentoIdentidadSifen == null) return "No especificado";
            
            // Catálogo SIFEN v150 - Tipos de documento de identidad receptor (iTipIDRec)
            // FIX 21-Ene-2026: Actualizado según Manual Técnico SIFEN v150
            return TipoDocumentoIdentidadSifen switch
            {
                1 => "Cédula paraguaya",
                3 => "Pasaporte",
                4 => "Carnet de residencia",
                5 => "Innominado",      // Consumidor Final
                9 => "Sin documento",
                _ => "Tipo de documento no válido"
            };
        }

        /// <summary>
        /// Valida si el cliente cumple con los requisitos mínimos de SIFEN
        /// Verifica todos los campos obligatorios según la naturaleza del receptor
        /// </summary>
        /// <returns>
        /// Tupla con:
        /// - bool EsValido: true si pasa todas las validaciones
        /// - List&lt;string&gt; Errores: lista de errores encontrados con descripciones detalladas
        /// </returns>
        public (bool EsValido, List<string> Errores) ValidarParaSifen()
        {
            var errores = new List<string>();

            // Validaciones generales para todos los tipos
            if (string.IsNullOrEmpty(RazonSocial))
                errores.Add("• Razón Social: Campo obligatorio para facturación electrónica SIFEN");

            if (string.IsNullOrEmpty(CodigoPais))
                errores.Add("• Código de País: Campo obligatorio - debe ser 'PRY' para Paraguay");

            // Validaciones específicas para CONTRIBUYENTES (NaturalezaReceptor = 1)
            if (NaturalezaReceptor == 1) 
            {
                if (string.IsNullOrEmpty(RUC))
                    errores.Add("• RUC: Campo obligatorio para contribuyentes - formato: ########-#");
                
                // FIX 23-Ene-2026: DV=0 es un valor válido para algunos RUCs según SIFEN
                // Ejemplo: RUC 4637249-0 tiene DV=0 y es válido en la BD del SET
                // No validamos DV > 0 porque 0 es un dígito verificador válido

                if (IdTipoContribuyente <= 0)
                    errores.Add("• Tipo de Contribuyente: Debe seleccionar un tipo válido del catálogo SIFEN");
            }

            // Validaciones específicas para NO CONTRIBUYENTES (NaturalezaReceptor = 2)
            if (NaturalezaReceptor == 2) 
            {
                if (TipoDocumentoIdentidadSifen == null || TipoDocumentoIdentidadSifen <= 0)
                    errores.Add("• Tipo de Documento: Obligatorio para no contribuyentes (1=CI, 2=Pasaporte, 3=CI Extranjera, 4=Otro)");
                
                if (string.IsNullOrEmpty(NumeroDocumentoIdentidad))
                    errores.Add("• Número de Documento: Campo obligatorio para no contribuyentes");
            }

            // Validaciones opcionales pero recomendadas
            if (string.IsNullOrEmpty(TipoOperacion))
                errores.Add("• Tipo de Operación: Recomendado especificar (1=B2B, 2=B2C, 3=B2G, 4=B2F)");

            // Validación de coherencia
            if (NaturalezaReceptor == 1 && (string.IsNullOrEmpty(RUC) || RUC == "0"))
                errores.Add("• Inconsistencia: Si es contribuyente debe tener RUC válido");

            if (NaturalezaReceptor == 2 && !string.IsNullOrEmpty(RUC) && RUC != "0")
                errores.Add("• Inconsistencia: Si es no contribuyente no debería tener RUC");

            return (errores.Count == 0, errores);
        }

        /// <summary>
        /// Genera un resumen completo del cliente para SIFEN con toda la información relevante
        /// </summary>
        /// <returns>String con resumen formateado para mostrar al usuario</returns>
        public string GenerarResumenSifen()
        {
            var resumen = new System.Text.StringBuilder();
            resumen.AppendLine("=== RESUMEN CLIENTE SIFEN ===");
            resumen.AppendLine($"Cliente: {RazonSocial}");
            resumen.AppendLine($"Naturaleza: {ObtenerDescripcionNaturaleza()}");
            
            if (NaturalezaReceptor == 1)
            {
                resumen.AppendLine($"RUC: {RUC}-{DV}");
            }
            else
            {
                resumen.AppendLine($"Documento: {ObtenerDescripcionTipoDocumento()} - {NumeroDocumentoIdentidad}");
            }

            resumen.AppendLine($"Tipo de Operación: {ObtenerDescripcionTipoOperacion()}");
            resumen.AppendLine($"País: {CodigoPais}");
            
            if (!string.IsNullOrEmpty(NombreFantasia))
                resumen.AppendLine($"Nombre de Fantasía: {NombreFantasia}");

            if (!string.IsNullOrEmpty(CodigoDepartamento))
                resumen.AppendLine($"Ubicación: {DescripcionDepartamento} - {DescripcionDistrito}");

            var (esValido, errores) = ValidarParaSifen();
            resumen.AppendLine($"Estado SIFEN: {(esValido ? "✅ VÁLIDO" : "❌ REQUIERE CORRECCIONES")}");
            
            if (!esValido)
            {
                resumen.AppendLine("\n--- ERRORES A CORREGIR ---");
                foreach (var error in errores)
                {
                    resumen.AppendLine(error);
                }
            }

            return resumen.ToString();
        }
    }
}
