using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemIA.Models
{
    /// <summary>
    /// Configuración global de la VPN PPTP para soporte remoto.
    /// Solo debe existir UN registro en esta tabla (singleton).
    /// Protegida con contraseña SA para acceso.
    /// </summary>
    public class ConfiguracionVPN
    {
        [Key]
        public int IdConfiguracionVPN { get; set; }
        
        // ========== SERVIDOR VPN (MIKROTIK) ==========
        
        /// <summary>
        /// IP pública o dominio del servidor VPN (Mikrotik)
        /// Ejemplo: "vpn.miempresa.com" o "200.123.45.67"
        /// </summary>
        [MaxLength(100)]
        public string? ServidorVPN { get; set; }
        
        /// <summary>
        /// Puerto del servicio PPTP (por defecto 1723)
        /// </summary>
        public int PuertoPPTP { get; set; } = 1723;
        
        /// <summary>
        /// Usuario de la conexión VPN PPTP
        /// </summary>
        [MaxLength(100)]
        public string? UsuarioVPN { get; set; }
        
        /// <summary>
        /// Contraseña de la conexión VPN (encriptada)
        /// </summary>
        [MaxLength(256)]
        public string? ContrasenaVPN { get; set; }
        
        /// <summary>
        /// Nombre de la conexión VPN en Windows
        /// Ejemplo: "SistemIA VPN"
        /// </summary>
        [MaxLength(100)]
        public string NombreConexionWindows { get; set; } = "SistemIA VPN";
        
        // ========== CONFIGURACIÓN DE RED VPN ==========
        
        /// <summary>
        /// Rango de red VPN (para validar IPs)
        /// Ejemplo: "192.168.89"
        /// </summary>
        [MaxLength(50)]
        public string? RangoRedVPN { get; set; } = "192.168.89";
        
        /// <summary>
        /// IP asignada a ESTA PC (de desarrollo/soporte)
        /// Ejemplo: "192.168.89.1"
        /// </summary>
        [MaxLength(20)]
        public string? IpLocalVPN { get; set; }
        
        // ========== COMPORTAMIENTO ==========
        
        /// <summary>
        /// Si es true, el sistema intenta conectarse a la VPN al iniciar
        /// </summary>
        public bool ConectarAlIniciar { get; set; } = false;
        
        /// <summary>
        /// Intentos de reconexión automática si se pierde la VPN
        /// </summary>
        public int IntentosReconexion { get; set; } = 3;
        
        /// <summary>
        /// Segundos entre intentos de reconexión
        /// </summary>
        public int SegundosEntreIntentos { get; set; } = 30;
        
        /// <summary>
        /// Si es true, la configuración VPN está activa
        /// </summary>
        public bool Activo { get; set; } = true;
        
        /// <summary>
        /// Minutos entre verificaciones de conexión VPN (default: 15 minutos)
        /// </summary>
        public int MinutosVerificacion { get; set; } = 15;
        
        // ========== AUDITORÍA ==========
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        
        [MaxLength(100)]
        public string? UsuarioModificacion { get; set; }
        
        /// <summary>
        /// Última vez que se conectó exitosamente a la VPN
        /// </summary>
        public DateTime? UltimaConexionExitosa { get; set; }
        
        /// <summary>
        /// Estado actual de la conexión (para mostrar en UI)
        /// Valores: "Desconectado", "Conectando", "Conectado", "Error"
        /// </summary>
        [NotMapped]
        public string EstadoConexion { get; set; } = "Desconectado";
        
        /// <summary>
        /// Mensaje de último error si hubo problema
        /// </summary>
        [NotMapped]
        public string? UltimoError { get; set; }
    }
}
