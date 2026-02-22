using Microsoft.EntityFrameworkCore;
using SistemIA.Data;
using SistemIA.Models;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio de fondo que gestiona la conexiÃ³n VPN automÃ¡ticamente.
    /// - Conecta al iniciar el servicio si estÃ¡ configurado
    /// - Verifica la conexiÃ³n periÃ³dicamente (cada 15 min por defecto)
    /// - Reconecta automÃ¡ticamente si se pierde la conexiÃ³n
    /// </summary>
    public class VpnConnectionService : BackgroundService
    {
        private readonly ILogger<VpnConnectionService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        
        // Intervalo de verificaciÃ³n por defecto: 15 minutos
        private TimeSpan _intervaloVerificacion = TimeSpan.FromMinutes(15);
        private bool _vpnHabilitada = false;
        private string _nombreConexion = "SistemIA VPN";
        private string _servidorVpn = "";
        private string _usuarioVpn = "";
        private string _contrasenaVpn = "";
        private int _intentosReconexion = 3;
        private int _segundosEntreIntentos = 30;
        
        public VpnConnectionService(
            ILogger<VpnConnectionService> logger,
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("VpnConnectionService iniciando...");
            
            // Esperar un poco para que la aplicaciÃ³n termine de iniciar
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            
            // Cargar configuraciÃ³n inicial
            await CargarConfiguracionAsync();
            
            if (!_vpnHabilitada)
            {
                _logger.LogInformation("VPN no estÃ¡ habilitada para conexiÃ³n automÃ¡tica. Servicio en espera.");
            }
            else
            {
                _logger.LogInformation("VPN configurada para conexiÃ³n automÃ¡tica. Intentando conectar...");
                await IntentarConexionAsync();
            }
            
            // Bucle principal de verificaciÃ³n
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Esperar el intervalo de verificaciÃ³n
                    await Task.Delay(_intervaloVerificacion, stoppingToken);
                    
                    // Recargar configuraciÃ³n por si cambiÃ³
                    await CargarConfiguracionAsync();
                    
                    if (_vpnHabilitada)
                    {
                        // Verificar estado de la conexiÃ³n
                        var conectada = await VerificarConexionVpnAsync();
                        
                        if (!conectada)
                        {
                            _logger.LogWarning("VPN desconectada. Intentando reconectar...");
                            await IntentarConexionAsync();
                        }
                        else
                        {
                            _logger.LogDebug("VPN conectada correctamente.");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // CancelaciÃ³n normal, salir del bucle
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el bucle de verificaciÃ³n VPN");
                    // Esperar un poco antes de reintentar
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            
            _logger.LogInformation("VpnConnectionService detenido.");
        }
        
        /// <summary>
        /// Carga la configuraciÃ³n VPN desde la base de datos
        /// </summary>
        private async Task CargarConfiguracionAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                var config = await context.ConfiguracionesVPN
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
                
                if (config != null && config.Activo)
                {
                    _vpnHabilitada = config.ConectarAlIniciar;
                    _nombreConexion = config.NombreConexionWindows ?? "SistemIA VPN";
                    _servidorVpn = config.ServidorVPN ?? "";
                    _usuarioVpn = config.UsuarioVPN ?? "";
                    _contrasenaVpn = config.ContrasenaVPN ?? "";
                    _intentosReconexion = config.IntentosReconexion > 0 ? config.IntentosReconexion : 3;
                    _segundosEntreIntentos = config.SegundosEntreIntentos > 0 ? config.SegundosEntreIntentos : 30;
                    
                    // Intervalo de verificaciÃ³n: usar valor de config o 15 minutos por defecto
                    var minutos = config.MinutosVerificacion > 0 ? config.MinutosVerificacion : 15;
                    _intervaloVerificacion = TimeSpan.FromMinutes(minutos);
                    
                    _logger.LogDebug("ConfiguraciÃ³n VPN cargada: ConectarAlIniciar={Enabled}, Intervalo={Intervalo}min", 
                        _vpnHabilitada, minutos);
                }
                else
                {
                    _vpnHabilitada = false;
                    _logger.LogDebug("ConfiguraciÃ³n VPN no encontrada o deshabilitada");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuraciÃ³n VPN");
                _vpnHabilitada = false;
            }
        }
        
        /// <summary>
        /// Intenta conectar a la VPN con reintentos
        /// </summary>
        private async Task IntentarConexionAsync()
        {
            if (string.IsNullOrEmpty(_servidorVpn) || string.IsNullOrEmpty(_usuarioVpn))
            {
                _logger.LogWarning("ConfiguraciÃ³n VPN incompleta: falta servidor o usuario");
                return;
            }
            
            for (int intento = 1; intento <= _intentosReconexion; intento++)
            {
                _logger.LogInformation("Intento de conexiÃ³n VPN {Intento}/{Total}", intento, _intentosReconexion);
                
                try
                {
                    // Primero verificar si la conexiÃ³n VPN existe en Windows
                    var existeConexion = await VerificarConexionExisteAsync(_nombreConexion);
                    if (!existeConexion)
                    {
                        _logger.LogInformation("Creando conexiÃ³n VPN '{Nombre}' en Windows...", _nombreConexion);
                        await CrearConexionWindowsAsync();
                        await Task.Delay(2000); // Esperar a que Windows registre la conexiÃ³n
                    }
                    
                    // Conectar usando rasdial
                    var resultado = await EjecutarRasdialAsync(_nombreConexion, _usuarioVpn, _contrasenaVpn);
                    
                    if (resultado.Success)
                    {
                        _logger.LogInformation("VPN conectada exitosamente en intento {Intento}", intento);
                        await RegistrarConexionExitosaAsync();
                        return;
                    }
                    else
                    {
                        _logger.LogWarning("Fallo en intento {Intento}: {Error}", intento, resultado.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en intento {Intento} de conexiÃ³n VPN", intento);
                }
                
                // Esperar antes del siguiente intento (excepto en el Ãºltimo)
                if (intento < _intentosReconexion)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_segundosEntreIntentos));
                }
            }
            
            _logger.LogError("No se pudo conectar a la VPN despuÃ©s de {Intentos} intentos", _intentosReconexion);
        }
        
        /// <summary>
        /// Verifica si la VPN estÃ¡ conectada
        /// </summary>
        private async Task<bool> VerificarConexionVpnAsync()
        {
            try
            {
                // MÃ©todo 1: Verificar con rasdial
                var psi = new ProcessStartInfo
                {
                    FileName = "rasdial",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(psi);
                if (process != null)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    
                    // Si la salida contiene el nombre de nuestra conexiÃ³n, estÃ¡ conectada
                    if (output.Contains(_nombreConexion, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                
                // MÃ©todo 2: Verificar interfaces de red PPP activas
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var iface in interfaces)
                {
                    if (iface.NetworkInterfaceType == NetworkInterfaceType.Ppp &&
                        iface.OperationalStatus == OperationalStatus.Up &&
                        iface.Name.Contains(_nombreConexion, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar estado de conexiÃ³n VPN");
                return false;
            }
        }
        
        /// <summary>
        /// Verifica si la conexiÃ³n VPN existe en Windows
        /// </summary>
        private async Task<bool> VerificarConexionExisteAsync(string nombreConexion)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -Command \"Get-VpnConnection -Name '{nombreConexion}' -ErrorAction SilentlyContinue\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(psi);
                if (process != null)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    return !string.IsNullOrWhiteSpace(output);
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Crea la conexiÃ³n VPN en Windows usando PowerShell
        /// </summary>
        private async Task CrearConexionWindowsAsync()
        {
            try
            {
                // Primero eliminar si existe una conexiÃ³n con el mismo nombre
                var removeCmd = $"Remove-VpnConnection -Name '{_nombreConexion}' -Force -ErrorAction SilentlyContinue";
                await EjecutarPowerShellAsync(removeCmd);
                
                // Crear nueva conexiÃ³n PPTP
                var createCmd = $@"Add-VpnConnection -Name '{_nombreConexion}' -ServerAddress '{_servidorVpn}' -TunnelType Pptp -EncryptionLevel Optional -AuthenticationMethod MSChapv2 -RememberCredential -Force";
                
                var resultado = await EjecutarPowerShellAsync(createCmd);
                
                if (resultado.Success)
                {
                    _logger.LogInformation("ConexiÃ³n VPN '{Nombre}' creada exitosamente", _nombreConexion);
                }
                else
                {
                    _logger.LogWarning("Error al crear conexiÃ³n VPN: {Error}", resultado.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear conexiÃ³n VPN en Windows");
            }
        }
        
        /// <summary>
        /// Ejecuta rasdial para conectar
        /// </summary>
        private async Task<(bool Success, string ErrorMessage)> EjecutarRasdialAsync(string nombre, string usuario, string contrasena)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "rasdial",
                    Arguments = $"\"{nombre}\" \"{usuario}\" \"{contrasena}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(psi);
                if (process != null)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    
                    if (process.ExitCode == 0)
                    {
                        return (true, "");
                    }
                    else
                    {
                        return (false, string.IsNullOrEmpty(error) ? output : error);
                    }
                }
                
                return (false, "No se pudo iniciar rasdial");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        
        /// <summary>
        /// Ejecuta un comando PowerShell
        /// </summary>
        private async Task<(bool Success, string ErrorMessage)> EjecutarPowerShellAsync(string comando)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{comando}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(psi);
                if (process != null)
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    
                    if (process.ExitCode == 0)
                    {
                        return (true, "");
                    }
                    else
                    {
                        return (false, error);
                    }
                }
                
                return (false, "No se pudo iniciar PowerShell");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        
        /// <summary>
        /// Registra la conexiÃ³n exitosa en la base de datos
        /// </summary>
        private async Task RegistrarConexionExitosaAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                var config = await context.ConfiguracionesVPN.FirstOrDefaultAsync();
                if (config != null)
                {
                    config.UltimaConexionExitosa = DateTime.Now;
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar conexiÃ³n exitosa");
            }
        }
        
        /// <summary>
        /// MÃ©todo pÃºblico para forzar reconexiÃ³n desde la UI
        /// </summary>
        public async Task ForzarReconexionAsync()
        {
            _logger.LogInformation("ReconexiÃ³n VPN forzada desde la UI");
            await CargarConfiguracionAsync();
            await IntentarConexionAsync();
        }
        
        /// <summary>
        /// MÃ©todo pÃºblico para desconectar la VPN
        /// </summary>
        public async Task DesconectarAsync()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "rasdial",
                    Arguments = $"\"{_nombreConexion}\" /disconnect",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(psi);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    _logger.LogInformation("VPN desconectada manualmente");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desconectar VPN");
            }
        }
    }
}
