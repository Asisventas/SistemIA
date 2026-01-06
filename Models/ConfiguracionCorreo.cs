using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Configuración de servidor de correo electrónico.
    /// Soporta SMTP tradicional, Gmail con OAuth2 y otros proveedores modernos.
    /// Solo debe existir un registro activo por sucursal/sociedad (singleton).
    /// </summary>
    [Table("ConfiguracionCorreo")]
    public class ConfiguracionCorreo
    {
        [Key]
        public int IdConfiguracionCorreo { get; set; }

        // ========== INFORMACIÓN BÁSICA ==========

        /// <summary>
        /// Nombre descriptivo de la configuración (ej: "Correo Principal", "Notificaciones")
        /// </summary>
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        [Display(Name = "Nombre de Configuración")]
        public string Nombre { get; set; } = "Correo Principal";

        /// <summary>
        /// Indica si esta configuración está activa
        /// </summary>
        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // ========== TIPO DE PROVEEDOR ==========

        /// <summary>
        /// Tipo de proveedor de correo: SMTP, Gmail, Outlook, SendGrid, etc.
        /// </summary>
        [Required]
        [StringLength(30)]
        [Display(Name = "Proveedor")]
        public string TipoProveedor { get; set; } = "SMTP";

        // ========== CONFIGURACIÓN SMTP TRADICIONAL ==========

        /// <summary>
        /// Servidor SMTP (ej: smtp.gmail.com, mail.tudominio.com)
        /// </summary>
        [StringLength(200)]
        [Display(Name = "Servidor SMTP")]
        public string? ServidorSmtp { get; set; }

        /// <summary>
        /// Puerto SMTP (25, 465, 587, 2525 son comunes)
        /// </summary>
        [Display(Name = "Puerto")]
        [Range(1, 65535, ErrorMessage = "Puerto debe estar entre 1 y 65535")]
        public int Puerto { get; set; } = 587;

        /// <summary>
        /// Tipo de conexión segura: None, SSL, TLS, STARTTLS
        /// </summary>
        [StringLength(20)]
        [Display(Name = "Tipo de Seguridad")]
        public string TipoSeguridad { get; set; } = "TLS";

        /// <summary>
        /// Usuario para autenticación SMTP (generalmente el email)
        /// </summary>
        [StringLength(200)]
        [Display(Name = "Usuario SMTP")]
        public string? UsuarioSmtp { get; set; }

        /// <summary>
        /// Contraseña SMTP (para Gmail usar "Contraseña de Aplicación")
        /// IMPORTANTE: En producción considerar encriptación
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string? ContrasenaSmtp { get; set; }

        // ========== CONFIGURACIÓN OAUTH2 (Gmail, Outlook, etc.) ==========

        /// <summary>
        /// Indica si usar OAuth2 en lugar de usuario/contraseña
        /// </summary>
        [Display(Name = "Usar OAuth2")]
        public bool UsarOAuth2 { get; set; } = false;

        /// <summary>
        /// Client ID de la aplicación OAuth2 (Google Cloud Console, Azure AD, etc.)
        /// </summary>
        [StringLength(300)]
        [Display(Name = "Client ID (OAuth2)")]
        public string? OAuth2ClientId { get; set; }

        /// <summary>
        /// Client Secret de la aplicación OAuth2
        /// IMPORTANTE: En producción considerar encriptación
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Client Secret (OAuth2)")]
        [DataType(DataType.Password)]
        public string? OAuth2ClientSecret { get; set; }

        /// <summary>
        /// Refresh Token para renovar el Access Token automáticamente
        /// </summary>
        [StringLength(2000)]
        [Display(Name = "Refresh Token")]
        public string? OAuth2RefreshToken { get; set; }

        /// <summary>
        /// Access Token actual (se renueva automáticamente con el Refresh Token)
        /// </summary>
        [StringLength(2000)]
        [Display(Name = "Access Token")]
        public string? OAuth2AccessToken { get; set; }

        /// <summary>
        /// Fecha de expiración del Access Token
        /// </summary>
        [Display(Name = "Expira Token")]
        public DateTime? OAuth2TokenExpira { get; set; }

        /// <summary>
        /// Scopes requeridos (ej: https://mail.google.com/)
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Scopes OAuth2")]
        public string? OAuth2Scopes { get; set; }

        // ========== CONFIGURACIÓN DEL REMITENTE ==========

        /// <summary>
        /// Dirección de correo del remitente (From).
        /// Si está vacío, se usará el correo de la sucursal.
        /// </summary>
        [StringLength(200)]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [Display(Name = "Correo Remitente")]
        public string? CorreoRemitente { get; set; }

        /// <summary>
        /// Nombre que aparece como remitente (ej: "Mi Empresa").
        /// Si está vacío, se usará automáticamente el NombreEmpresa de la Sucursal.
        /// </summary>
        [StringLength(200)]
        [Display(Name = "Nombre Remitente")]
        public string? NombreRemitente { get; set; }

        /// <summary>
        /// Si true, usa automáticamente los datos de la Sucursal para el remitente.
        /// NombreRemitente = Sucursal.NombreEmpresa, CorreoRemitente = Sucursal.Correo
        /// </summary>
        [Display(Name = "Usar datos de empresa como remitente")]
        public bool UsarDatosEmpresaComoRemitente { get; set; } = true;

        /// <summary>
        /// Obtiene el nombre del remitente efectivo (de la config o de la sucursal)
        /// </summary>
        [NotMapped]
        public string NombreRemitenteEfectivo => 
            !string.IsNullOrEmpty(NombreRemitente) ? NombreRemitente 
            : (Sucursal?.NombreEmpresa ?? "SistemIA");

        /// <summary>
        /// Obtiene el correo del remitente efectivo (de la config o de la sucursal)
        /// </summary>
        [NotMapped]
        public string CorreoRemitenteEfectivo => 
            !string.IsNullOrEmpty(CorreoRemitente) ? CorreoRemitente 
            : (Sucursal?.Correo ?? UsuarioSmtp ?? string.Empty);

        /// <summary>
        /// Correo para respuestas (Reply-To), si es diferente al remitente
        /// </summary>
        [StringLength(200)]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [Display(Name = "Correo para Respuestas (Reply-To)")]
        public string? CorreoReplyTo { get; set; }

        /// <summary>
        /// Correo con copia oculta para todos los envíos (auditoría)
        /// </summary>
        [StringLength(200)]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [Display(Name = "Copia Oculta (BCC)")]
        public string? CorreoBccAuditoria { get; set; }

        // ========== OPCIONES AVANZADAS ==========

        /// <summary>
        /// Timeout en segundos para conexión y envío
        /// </summary>
        [Display(Name = "Timeout (segundos)")]
        [Range(5, 300, ErrorMessage = "Timeout debe estar entre 5 y 300 segundos")]
        public int TimeoutSegundos { get; set; } = 30;

        /// <summary>
        /// Número máximo de reintentos si falla el envío
        /// </summary>
        [Display(Name = "Reintentos")]
        [Range(0, 10, ErrorMessage = "Reintentos debe estar entre 0 y 10")]
        public int MaxReintentos { get; set; } = 3;

        /// <summary>
        /// Habilitar logging detallado de comunicación SMTP
        /// </summary>
        [Display(Name = "Habilitar Log Detallado")]
        public bool HabilitarLogDetallado { get; set; } = false;

        /// <summary>
        /// Certificado SSL personalizado (para servidores con certificados autofirmados)
        /// </summary>
        [Display(Name = "Ignorar Errores de Certificado SSL")]
        public bool IgnorarErroresCertificado { get; set; } = false;

        // ========== CONFIGURACIÓN ESPECÍFICA POR PROVEEDOR ==========

        /// <summary>
        /// Para SendGrid, Mailgun, etc.: API Key
        /// </summary>
        [StringLength(500)]
        [Display(Name = "API Key")]
        [DataType(DataType.Password)]
        public string? ApiKey { get; set; }

        /// <summary>
        /// Dominio personalizado (para Mailgun y similares)
        /// </summary>
        [StringLength(200)]
        [Display(Name = "Dominio")]
        public string? DominioApi { get; set; }

        // ========== PLANTILLAS Y FORMATO ==========

        /// <summary>
        /// Firma HTML predeterminada para los correos
        /// </summary>
        [Display(Name = "Firma HTML")]
        public string? FirmaHtml { get; set; }

        /// <summary>
        /// Usar formato HTML por defecto en los correos
        /// </summary>
        [Display(Name = "Usar HTML por Defecto")]
        public bool UsarHtmlPorDefecto { get; set; } = true;

        // ========== OPCIONES DE ENVÍO AUTOMÁTICO ==========

        /// <summary>
        /// Enviar informes automáticamente al cerrar el sistema
        /// </summary>
        [Display(Name = "Enviar al Cierre del Sistema")]
        public bool EnviarAlCierreSistema { get; set; } = false;

        /// <summary>
        /// Hora programada para envío automático de informes diarios (formato HH:mm)
        /// </summary>
        [StringLength(5)]
        [Display(Name = "Hora Envío Diario")]
        public string? HoraEnvioDiario { get; set; } = "22:00";

        /// <summary>
        /// Día de la semana para envío de resumen semanal (1=Lunes, 7=Domingo)
        /// </summary>
        [Display(Name = "Día Envío Semanal")]
        [Range(1, 7)]
        public int? DiaEnvioSemanal { get; set; } = 1; // Lunes

        /// <summary>
        /// Día del mes para envío de resumen mensual (1-28)
        /// </summary>
        [Display(Name = "Día Envío Mensual")]
        [Range(1, 28)]
        public int? DiaEnvioMensual { get; set; } = 1;

        // ========== RELACIONES ==========

        /// <summary>
        /// ID de la sociedad a la que pertenece esta configuración
        /// </summary>
        [Display(Name = "Sociedad")]
        public int? IdSociedad { get; set; }

        [ForeignKey("IdSociedad")]
        public virtual Sociedad? Sociedad { get; set; }

        /// <summary>
        /// ID de la sucursal a la que pertenece esta configuración.
        /// Permite configuraciones de correo diferentes por sucursal.
        /// </summary>
        [Display(Name = "Sucursal")]
        public int? IdSucursal { get; set; }

        [ForeignKey("IdSucursal")]
        public virtual Sucursal? Sucursal { get; set; }

        // ========== AUDITORÍA ==========

        [Display(Name = "Fecha Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Fecha Modificación")]
        public DateTime? FechaModificacion { get; set; }

        [StringLength(50)]
        [Display(Name = "Usuario Creación")]
        public string? UsuarioCreacion { get; set; }

        [StringLength(50)]
        [Display(Name = "Usuario Modificación")]
        public string? UsuarioModificacion { get; set; }

        /// <summary>
        /// Último resultado de prueba de conexión
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Resultado Última Prueba")]
        public string? UltimaPruebaResultado { get; set; }

        [Display(Name = "Fecha Última Prueba")]
        public DateTime? UltimaPruebaFecha { get; set; }

        // ========== PROPIEDADES DE SOLO LECTURA ==========

        /// <summary>
        /// Indica si la configuración está completa para enviar correos
        /// </summary>
        [NotMapped]
        public bool ConfiguracionCompleta
        {
            get
            {
                // El correo puede venir de la config o de la sucursal
                var correoEfectivo = CorreoRemitenteEfectivo;
                if (string.IsNullOrEmpty(correoEfectivo))
                    return false;

                if (TipoProveedor == "SMTP")
                {
                    return !string.IsNullOrEmpty(ServidorSmtp) && Puerto > 0;
                }
                else if (TipoProveedor == "Gmail" || TipoProveedor == "Outlook")
                {
                    if (UsarOAuth2)
                        return !string.IsNullOrEmpty(OAuth2ClientId) && !string.IsNullOrEmpty(OAuth2RefreshToken);
                    else
                        return !string.IsNullOrEmpty(UsuarioSmtp) && !string.IsNullOrEmpty(ContrasenaSmtp);
                }
                else if (TipoProveedor == "SendGrid" || TipoProveedor == "Mailgun")
                {
                    return !string.IsNullOrEmpty(ApiKey);
                }

                return false;
            }
        }

        /// <summary>
        /// Descripción resumida de la configuración
        /// </summary>
        [NotMapped]
        public string Descripcion
        {
            get
            {
                if (TipoProveedor == "SMTP")
                    return $"SMTP: {ServidorSmtp}:{Puerto}";
                else if (TipoProveedor == "Gmail")
                    return UsarOAuth2 ? "Gmail (OAuth2)" : "Gmail (Contraseña App)";
                else if (TipoProveedor == "Outlook")
                    return UsarOAuth2 ? "Outlook (OAuth2)" : "Outlook (SMTP)";
                else
                    return $"{TipoProveedor}";
            }
        }
    }

    /// <summary>
    /// Tipos de proveedores de correo soportados
    /// </summary>
    public static class TiposProveedorCorreo
    {
        public const string SMTP = "SMTP";
        public const string Gmail = "Gmail";
        public const string Outlook = "Outlook";
        public const string SendGrid = "SendGrid";
        public const string Mailgun = "Mailgun";
        public const string AmazonSES = "AmazonSES";

        public static List<(string Valor, string Nombre, string Descripcion)> Listado => new()
        {
            (SMTP, "SMTP Genérico", "Servidor SMTP tradicional (cualquier proveedor)"),
            (Gmail, "Gmail / Google Workspace", "Gmail con OAuth2 o Contraseña de Aplicación"),
            (Outlook, "Outlook / Microsoft 365", "Outlook.com, Hotmail o Microsoft 365"),
            (SendGrid, "SendGrid", "Servicio de envío masivo de Twilio"),
            (Mailgun, "Mailgun", "Servicio de envío de Mailgun"),
            (AmazonSES, "Amazon SES", "Simple Email Service de AWS")
        };
    }

    /// <summary>
    /// Tipos de seguridad de conexión
    /// </summary>
    public static class TiposSeguridadCorreo
    {
        public const string Ninguno = "None";
        public const string SSL = "SSL";
        public const string TLS = "TLS";
        public const string STARTTLS = "STARTTLS";

        public static List<(string Valor, string Nombre, int PuertoSugerido)> Listado => new()
        {
            (Ninguno, "Sin encriptación", 25),
            (SSL, "SSL/TLS implícito", 465),
            (TLS, "TLS explícito", 587),
            (STARTTLS, "STARTTLS", 587)
        };
    }
}
