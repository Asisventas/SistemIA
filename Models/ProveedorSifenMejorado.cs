using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Modelo mejorado de Proveedor compatible con SIFEN Paraguay v150
    /// Cumple con todos los estándares para ser emisor de documentos electrónicos
    /// Basado en gEmis (Grupo Emisor) del XML SIFEN
    /// </summary>
    [Table("ProveedoresSifen")]
    public class ProveedorSifenMejorado
    {
        [Key]
        public int IdProveedor { get; set; }

        // ===== CAMPOS BÁSICOS EMPRESARIALES =====
        
        [StringLength(50)]
        public string? CodigoProveedor { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "La razón social debe tener como máximo 200 caracteres.")]
        public string RazonSocial { get; set; } = "";

        [StringLength(200)]
        public string? NombreFantasia { get; set; }

        // ===== CAMPOS TRIBUTARIOS SIFEN (OBLIGATORIOS PARA EMISOR) =====
        
        /// <summary>
        /// RUC del emisor (dRucEm) - Campo OBLIGATORIO para emisor SIFEN
        /// Sin guión, solo números. Ejemplo: "80000001"
        /// </summary>
        [Required]
        [StringLength(8, ErrorMessage = "El RUC debe tener exactamente 8 dígitos.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El RUC debe contener solo 8 dígitos numéricos.")]
        public string RUC { get; set; } = "";

        /// <summary>
        /// Dígito verificador del RUC (dDVEmi) - Campo OBLIGATORIO
        /// Calculado automáticamente según algoritmo oficial SET
        /// </summary>
        [Required]
        [Range(0, 11, ErrorMessage = "El dígito verificador debe estar entre 0 y 11.")]
        public int DV { get; set; }

        /// <summary>
        /// Tipo de contribuyente SIFEN (iTipCont) - Campo opcional
        /// 1 = Persona Física
        /// 2 = Persona Jurídica  
        /// </summary>
       
        [Range(1, 2, ErrorMessage = "Tipo de contribuyente debe ser 1 (Persona Física) o 2 (Persona Jurídica).")]
        public int? TipoContribuyente { get; set; }

       
        public int? IdTipoContribuyenteCatalogo { get; set; }

        [ForeignKey(nameof(IdTipoContribuyenteCatalogo))]
        public TiposContribuyentes? TipoContribuyenteCatalogo { get; set; }

        /// <summary>
        /// Tipo de régimen SIFEN (cTipReg) - Campo opcional
        /// 1 = Régimen General
        /// 2 = Régimen Simplificado  
        /// 3 = Régimen de Pequeño Contribuyente
        /// </summary>
    
        [Range(1, 3, ErrorMessage = "Tipo de régimen debe ser 1 (General), 2 (Simplificado) o 3 (Pequeño Contribuyente).")]
        public int? TipoRegimen { get; set; }

        /// <summary>
        /// Timbrado fiscal - Número de autorización SET para emitir facturas
        /// Campo OBLIGATORIO para emisores de documentos fiscales
        /// </summary>
     
        [StringLength(8, ErrorMessage = "El timbrado debe tener exactamente 8 dígitos.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El timbrado debe contener solo 8 dígitos numéricos.")]
        public string Timbrado { get; set; } = "";

        /// <summary>
        /// Fecha de vencimiento del timbrado - Campo OBLIGATORIO
        /// Debe renovarse antes del vencimiento para seguir emitiendo facturas
        /// </summary>
   
        public DateTime VencimientoTimbrado { get; set; }

        /// <summary>
        /// Establecimiento fiscal (dEst) - Número de 3 dígitos
        /// Campo opcional
        /// </summary>
        
        [StringLength(3, ErrorMessage = "El establecimiento debe tener como máximo 3 dígitos.")]
        [RegularExpression(@"^(\d{3})?$", ErrorMessage = "El establecimiento debe contener 3 dígitos numéricos.")]
        public string? Establecimiento { get; set; }

        /// <summary>
        /// Punto de expedición (dPunExp) - Número de 3 dígitos  
        /// Campo opcional
        /// </summary>
       
        [StringLength(3, ErrorMessage = "El punto de expedición debe tener como máximo 3 dígitos.")]
        [RegularExpression(@"^(\d{3})?$", ErrorMessage = "El punto de expedición debe contener 3 dígitos numéricos.")]
        public string? PuntoExpedicion { get; set; }

        // ===== UBICACIÓN GEOGRÁFICA SIFEN =====
        
        /// <summary>
        /// Dirección completa del emisor (dDirEmi) - Campo opcional
        /// Dirección física donde se encuentra el establecimiento
        /// </summary>
        [StringLength(255, ErrorMessage = "La dirección debe tener como máximo 255 caracteres.")]
        public string? Direccion { get; set; }

        /// <summary>
        /// Número de casa (dNumCas) - Campo opcional pero recomendado
        /// </summary>
        [StringLength(10)]
        public string? NumeroCasa { get; set; }

        /// <summary>
        /// Código del departamento SIFEN (cDepEmi) - Campo opcional
        /// Según catálogo oficial SET: 01=Capital, 02=San Pedro, etc.
        /// </summary>
        [StringLength(2, ErrorMessage = "El código de departamento debe tener 1 o 2 dígitos.")]
        public string? CodigoDepartamento { get; set; }

        /// <summary>
        /// Descripción del departamento (dDesDepEmi) - Campo opcional
        /// Ejemplo: "CAPITAL", "SAN PEDRO", "CORDILLERA"
        /// </summary>
        [StringLength(16, ErrorMessage = "La descripción del departamento debe tener como máximo 16 caracteres.")]
        public string? DescripcionDepartamento { get; set; }

        /// <summary>
        /// Código de la ciudad SIFEN (cCiuEmi) - Campo opcional
        /// Según catálogo oficial SET
        /// </summary>
        public int? CodigoCiudad { get; set; }

        /// <summary>
        /// Descripción de la ciudad (dDesCiuEmi) - Campo opcional
        /// Ejemplo: "ASUNCION (DISTRITO)", "SAN LORENZO"
        /// </summary>
        [StringLength(30, ErrorMessage = "La descripción de la ciudad debe tener como máximo 30 caracteres.")]
        public string? DescripcionCiudad { get; set; }

        // ===== DATOS DE CONTACTO =====
        
        /// <summary>
        /// Teléfono del emisor (dTelEmi)
        /// Formato: sin espacios ni guiones, solo números
        /// </summary>
        [StringLength(15, ErrorMessage = "El teléfono debe tener como máximo 15 caracteres.")]
        [RegularExpression(@"^(\d{7,15})?$", ErrorMessage = "El teléfono debe contener solo números (7-15 dígitos).")]
        public string? Telefono { get; set; }

        /// <summary>
        /// Email del emisor (dEmailE)
        /// Debe ser un email válido para recibir notificaciones de la SET
        /// </summary>
        [StringLength(80, ErrorMessage = "El email debe tener como máximo 80 caracteres.")]
        [EmailAddress(ErrorMessage = "Debe ser un email válido.")]
        public string? Email { get; set; }

        [StringLength(15)]
        public string? Celular { get; set; }

        [StringLength(100)]
        public string? PersonaContacto { get; set; }

        // ===== ACTIVIDAD ECONÓMICA SIFEN =====
        
        /// <summary>
        /// Código de actividad económica (cActEco)
        /// Según clasificación CIIU de la SET Paraguay
        /// Ejemplo: "46510" = Comercio al por mayor de equipos informáticos
        /// </summary>
        [StringLength(5, ErrorMessage = "El código de actividad económica debe tener como máximo 5 dígitos.")]
        [RegularExpression(@"^(\d{5})?$", ErrorMessage = "El código de actividad económica debe contener solo 5 dígitos numéricos.")]
        public string? CodigoActividadEconomica { get; set; }

        /// <summary>
        /// Descripción de la actividad económica (dDesActEco)
        /// Descripción oficial según clasificación CIIU
        /// </summary>
        [StringLength(300, ErrorMessage = "La descripción de actividad económica debe tener como máximo 300 caracteres.")]
        public string? DescripcionActividadEconomica { get; set; }

        // ===== CAMPOS COMERCIALES Y FINANCIEROS =====
        
        [StringLength(100)]
        public string? Rubro { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? LimiteCredito { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal SaldoPendiente { get; set; } = 0;

        public int? PlazoPagoDias { get; set; }

        [StringLength(3)]
        public string CodigoPais { get; set; } = "PRY";

        // ===== CERTIFICADOS SIFEN =====
        
        /// <summary>
        /// Ruta del archivo del certificado digital .p12
        /// Campo OBLIGATORIO para emisores SIFEN
        /// </summary>
        [StringLength(500)]
        public string? CertificadoRuta { get; set; }

        /// <summary>
        /// Contraseña del certificado digital
        /// Campo OBLIGATORIO para emisores SIFEN
        /// </summary>
        [StringLength(100)]
        public string? CertificadoPassword { get; set; }

        /// <summary>
        /// Ambiente de operación SIFEN
        /// "test" = Ambiente de pruebas
        /// "prod" = Ambiente de producción
        /// </summary>
        [StringLength(10)]
        public string Ambiente { get; set; } = "test";

        // ===== CONTROL DE DOCUMENTOS =====
        
        /// <summary>
        /// Último número de documento emitido
        /// Para control de secuencia de facturas
        /// </summary>
        public long UltimoNumeroDocumento { get; set; } = 0;

        /// <summary>
        /// Última serie utilizada (si aplica)
        /// </summary>
        [StringLength(2)]
        public string? UltimaSerie { get; set; }

        // ===== CAMPOS DE AUDITORÍA Y CONTROL =====
        
    
        public bool Estado { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? UsuarioCreacion { get; set; }

        public DateTime? FechaModificacion { get; set; }

        [StringLength(100)]
        public string? UsuarioModificacion { get; set; }

        public DateTime? FechaUltimaFacturacion { get; set; }

        // ===== PROPIEDADES CALCULADAS Y MÉTODOS DE UTILIDAD =====

        /// <summary>
        /// Obtiene el RUC completo con dígito verificador
        /// Formato: 12345678-9
        /// </summary>
        public string RUCCompleto => $"{RUC}-{DV}";

        /// <summary>
        /// Verifica si el timbrado está próximo a vencer (30 días)
        /// </summary>
        public bool TimbradoProximoVencer => VencimientoTimbrado <= DateTime.Now.AddDays(30);

        /// <summary>
        /// Verifica si el timbrado está vencido
        /// </summary>
        public bool TimbradoVencido => VencimientoTimbrado < DateTime.Now;

        /// <summary>
        /// Estado del timbrado para mostrar en la UI
        /// </summary>
        public string EstadoTimbrado
        {
            get
            {
                if (TimbradoVencido) return "❌ VENCIDO";
                if (TimbradoProximoVencer) return "⚠️ PRÓXIMO A VENCER";
                return "✅ VIGENTE";
            }
        }

        /// <summary>
        /// Descripción del tipo de contribuyente
        /// </summary>
        public string DescripcionTipoContribuyente
        {
            get
            {
                return TipoContribuyente switch
                {
                    1 => "Persona Física",
                    2 => "Persona Jurídica",
                    _ => "No especificado"
                };
            }
        }

        /// <summary>
        /// Descripción del tipo de régimen  
        /// </summary>
        public string DescripcionTipoRegimen
        {
            get
            {
                return TipoRegimen switch
                {
                    1 => "Régimen General",
                    2 => "Régimen Simplificado", 
                    3 => "Pequeño Contribuyente",
                    _ => "No especificado"
                };
            }
        }

        /// <summary>
        /// Validaciones completas para SIFEN
        /// Verifica todos los campos obligatorios para emisión de documentos electrónicos
        /// </summary>
        /// <returns>
        /// Tupla con:
        /// - bool EsValido: true si cumple todos los requisitos SIFEN
        /// - List&lt;string&gt; Errores: lista detallada de errores encontrados
        /// </returns>
        public (bool EsValido, List<string> Errores) ValidarParaSifen()
        {
            var errores = new List<string>();

            // Validaciones básicas obligatorias
            if (string.IsNullOrEmpty(RazonSocial))
                errores.Add("• Razón Social: Campo obligatorio para emisores SIFEN");

            if (string.IsNullOrEmpty(RUC) || RUC.Length != 8)
                errores.Add("• RUC: Debe tener exactamente 8 dígitos numéricos");

            if (DV < 0 || DV > 11)
                errores.Add("• Dígito Verificador: Debe estar entre 0 y 11");

            if (TipoContribuyente < 1 || TipoContribuyente > 2)
                errores.Add("• Tipo de Contribuyente: Debe ser 1 (Física) o 2 (Jurídica)");

            if (TipoRegimen < 1 || TipoRegimen > 3)
                errores.Add("• Tipo de Régimen: Debe ser 1 (General), 2 (Simplificado) o 3 (Pequeño Contrib.)");

            // Validaciones de timbrado
            if (string.IsNullOrEmpty(Timbrado) || Timbrado.Length != 8)
                errores.Add("• Timbrado: Debe tener exactamente 8 dígitos numéricos");

            if (VencimientoTimbrado == default)
                errores.Add("• Vencimiento de Timbrado: Debe especificar una fecha válida");

            // Nota: Se permite timbrado vencido para pruebas, solo advertencia

            if (string.IsNullOrEmpty(Establecimiento) || Establecimiento.Length != 3)
                errores.Add("• Establecimiento: Debe tener exactamente 3 dígitos");

            if (string.IsNullOrEmpty(PuntoExpedicion) || PuntoExpedicion.Length != 3)
                errores.Add("• Punto de Expedición: Debe tener exactamente 3 dígitos");

            // Validaciones de ubicación - removidas para permitir flexibilidad

            // Validaciones de contacto (opcionales)
            // Email se valida solo si se proporciona (EmailAddress data annotation lo hace automáticamente)

            // Validaciones de actividad económica (opcionales)
            if (!string.IsNullOrEmpty(CodigoActividadEconomica) && CodigoActividadEconomica.Length != 5)
                errores.Add("• Código de Actividad Económica: Debe tener exactamente 5 dígitos según CIIU");

            // Validaciones de certificados (para producción)
            if (Ambiente == "prod")
            {
                if (string.IsNullOrEmpty(CertificadoRuta))
                    errores.Add("• Certificado Digital: Ruta del archivo .p12 obligatoria en producción");

                if (string.IsNullOrEmpty(CertificadoPassword))
                    errores.Add("• Contraseña del Certificado: Obligatoria en producción");
            }

            return (errores.Count == 0, errores);
        }

        /// <summary>
        /// Genera el siguiente número de documento para una nueva factura
        /// </summary>
        /// <returns>Número de documento a utilizar</returns>
        public long GenerarSiguienteNumeroDocumento()
        {
            UltimoNumeroDocumento++;
            FechaUltimaFacturacion = DateTime.Now;
            return UltimoNumeroDocumento;
        }

        /// <summary>
        /// Genera un resumen completo del proveedor para verificación SIFEN
        /// </summary>
        /// <returns>String con información detallada del emisor</returns>
        public string GenerarResumenSifen()
        {
            var resumen = new System.Text.StringBuilder();
            
            resumen.AppendLine("=== RESUMEN PROVEEDOR EMISOR SIFEN ===");
            resumen.AppendLine($"Razón Social: {RazonSocial}");
            resumen.AppendLine($"RUC: {RUCCompleto}");
            resumen.AppendLine($"Tipo: {DescripcionTipoContribuyente} - {DescripcionTipoRegimen}");
            resumen.AppendLine($"Timbrado: {Timbrado} ({EstadoTimbrado})");
            resumen.AppendLine($"Establecimiento: {Establecimiento} - Punto Expedición: {PuntoExpedicion}");
            resumen.AppendLine($"Actividad: [{CodigoActividadEconomica}] {DescripcionActividadEconomica}");
            resumen.AppendLine($"Ubicación: {DescripcionCiudad}, {DescripcionDepartamento}");
            resumen.AppendLine($"Contacto: {Telefono} - {Email}");
            resumen.AppendLine($"Ambiente: {Ambiente.ToUpper()}");
            resumen.AppendLine($"Último Documento: {UltimoNumeroDocumento}");

            var (esValido, errores) = ValidarParaSifen();
            resumen.AppendLine($"Estado SIFEN: {(esValido ? "✅ APTO PARA EMITIR" : "❌ REQUIERE CORRECCIONES")}");

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
