using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using SistemIA.Data;
using SistemIA.Models;

namespace SistemIA.Pages;

public partial class VPNConfiguracion
{
    private bool _autenticado = false;
    private string _contrasenaSA = "";
    private bool _validando = false;
    private bool _cargando = false;
    private bool _guardando = false;
    private bool _conectando = false;
    private bool _mostrarContrasenaVPN = false;
    
    private string? _mensajeError;
    private string? _mensajeExito;
    private string? _mensajeConexion;
    private bool _exitoConexion = false;
    
    private string _estadoConexion = "Desconectado";
    private ConfiguracionVPN? _config;
    private List<Cliente> _clientesVPN = new();
    
    private string GetBadgeClass()
    {
        return _estadoConexion switch
        {
            "Conectado" => "bg-success",
            "Conectando" => "bg-warning text-dark",
            _ => "bg-secondary"
        };
    }
    
    private string GetBadgeIcon()
    {
        return _estadoConexion switch
        {
            "Conectado" => "bi-wifi",
            "Conectando" => "bi-hourglass-split",
            _ => "bi-wifi-off"
        };
    }
    
    private string GetConexionIcon()
    {
        return _estadoConexion switch
        {
            "Conectado" => "bi-wifi text-success",
            "Conectando" => "bi-hourglass-split text-warning",
            _ => "bi-wifi-off text-muted"
        };
    }
    
    private void ToggleContrasenaVPN()
    {
        _mostrarContrasenaVPN = !_mostrarContrasenaVPN;
    }
    
    private async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrEmpty(_contrasenaSA))
        {
            await ValidarSA();
        }
    }
    
    private async Task ValidarSA()
    {
        _validando = true;
        _mensajeError = null;
        StateHasChanged();
        
        try
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var builder = new SqlConnectionStringBuilder(connectionString);
            var testConnectionString = $"Server={builder.DataSource};Database={builder.InitialCatalog};User Id=sa;Password={_contrasenaSA};TrustServerCertificate=True;Connect Timeout=5;";
            
            using var testConnection = new SqlConnection(testConnectionString);
            await testConnection.OpenAsync();
            
            _autenticado = true;
            await CargarConfiguracion();
        }
        catch (SqlException ex)
        {
            _mensajeError = ex.Number == 18456 ? "Contrasena incorrecta." : $"Error: {ex.Message}";
        }
        catch (Exception ex)
        {
            _mensajeError = $"Error: {ex.Message}";
        }
        finally
        {
            _validando = false;
            StateHasChanged();
        }
    }
    
    private async Task CargarConfiguracion()
    {
        _cargando = true;
        StateHasChanged();
        
        try
        {
            await using var ctx = await DbFactory.CreateDbContextAsync();
            
            _config = await ctx.ConfiguracionesVPN.FirstOrDefaultAsync();
            if (_config == null)
            {
                _config = new ConfiguracionVPN();
                ctx.ConfiguracionesVPN.Add(_config);
                await ctx.SaveChangesAsync();
            }
            
            _clientesVPN = await ctx.Clientes
                .Where(c => c.HabilitadoSoporteRemoto && !string.IsNullOrEmpty(c.IpVpnAsignada))
                .OrderBy(c => c.RazonSocial)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _mensajeError = $"Error al cargar: {ex.Message}";
        }
        finally
        {
            _cargando = false;
            StateHasChanged();
        }
    }
    
    private async Task Guardar()
    {
        if (_config == null) return;
        
        _guardando = true;
        _mensajeError = null;
        _mensajeExito = null;
        StateHasChanged();
        
        try
        {
            await using var ctx = await DbFactory.CreateDbContextAsync();
            var configDb = await ctx.ConfiguracionesVPN.FindAsync(_config.IdConfiguracionVPN);
            
            if (configDb != null)
            {
                configDb.ServidorVPN = _config.ServidorVPN?.Trim();
                configDb.PuertoPPTP = _config.PuertoPPTP;
                configDb.UsuarioVPN = _config.UsuarioVPN?.Trim();
                configDb.ContrasenaVPN = _config.ContrasenaVPN;
                configDb.NombreConexionWindows = _config.NombreConexionWindows?.Trim() ?? "SistemIA VPN";
                configDb.RangoRedVPN = _config.RangoRedVPN?.Trim();
                configDb.IpLocalVPN = _config.IpLocalVPN?.Trim();
                configDb.ConectarAlIniciar = _config.ConectarAlIniciar;
                configDb.IntentosReconexion = _config.IntentosReconexion;
                configDb.SegundosEntreIntentos = _config.SegundosEntreIntentos;
                configDb.Activo = _config.Activo;
                configDb.FechaModificacion = DateTime.Now;
                
                await ctx.SaveChangesAsync();
                _mensajeExito = "Configuracion guardada correctamente.";
            }
        }
        catch (Exception ex)
        {
            _mensajeError = $"Error al guardar: {ex.Message}";
        }
        finally
        {
            _guardando = false;
            StateHasChanged();
        }
    }
    
    private async Task ConectarVPN()
    {
        if (_config == null || string.IsNullOrEmpty(_config.ServidorVPN)) return;
        
        _conectando = true;
        _estadoConexion = "Conectando";
        _mensajeConexion = null;
        StateHasChanged();
        
        try
        {
            var nombreConexion = _config.NombreConexionWindows ?? "SistemIA VPN";
            var usuario = _config.UsuarioVPN ?? "";
            var contrasena = _config.ContrasenaVPN ?? "";
            
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "rasdial",
                Arguments = $"\"{nombreConexion}\" \"{usuario}\" \"{contrasena}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            using var process = System.Diagnostics.Process.Start(psi);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0)
                {
                    _estadoConexion = "Conectado";
                    _exitoConexion = true;
                    _mensajeConexion = "Conectado exitosamente.";
                    
                    await using var ctx = await DbFactory.CreateDbContextAsync();
                    var cfg = await ctx.ConfiguracionesVPN.FindAsync(_config.IdConfiguracionVPN);
                    if (cfg != null)
                    {
                        cfg.UltimaConexionExitosa = DateTime.Now;
                        await ctx.SaveChangesAsync();
                        _config.UltimaConexionExitosa = cfg.UltimaConexionExitosa;
                    }
                }
                else
                {
                    _estadoConexion = "Desconectado";
                    _exitoConexion = false;
                    _mensajeConexion = $"Error al conectar: {(string.IsNullOrEmpty(error) ? output : error)}";
                }
            }
        }
        catch (Exception ex)
        {
            _estadoConexion = "Desconectado";
            _exitoConexion = false;
            _mensajeConexion = $"Error: {ex.Message}";
        }
        finally
        {
            _conectando = false;
            StateHasChanged();
        }
    }
    
    private async Task DesconectarVPN()
    {
        if (_config == null) return;
        
        _conectando = true;
        StateHasChanged();
        
        try
        {
            var nombreConexion = _config.NombreConexionWindows ?? "SistemIA VPN";
            
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "rasdial",
                Arguments = $"\"{nombreConexion}\" /disconnect",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using var process = System.Diagnostics.Process.Start(psi);
            if (process != null)
            {
                await process.WaitForExitAsync();
            }
            
            _estadoConexion = "Desconectado";
            _mensajeConexion = "Desconectado.";
            _exitoConexion = true;
        }
        catch (Exception ex)
        {
            _mensajeConexion = $"Error: {ex.Message}";
            _exitoConexion = false;
        }
        finally
        {
            _conectando = false;
            StateHasChanged();
        }
    }
    
    private async Task CrearConexionWindows()
    {
        if (_config == null || string.IsNullOrEmpty(_config.ServidorVPN)) return;
        
        _mensajeConexion = null;
        StateHasChanged();
        
        try
        {
            var nombreConexion = _config.NombreConexionWindows ?? "SistemIA VPN";
            var servidor = _config.ServidorVPN;
            
            var script = $@"
                try {{
                    Remove-VpnConnection -Name '{nombreConexion}' -Force -ErrorAction SilentlyContinue
                    Add-VpnConnection -Name '{nombreConexion}' -ServerAddress '{servidor}' -TunnelType Pptp -AuthenticationMethod Pap -RememberCredential -Force
                    Write-Output 'OK'
                }} catch {{
                    Write-Output $_.Exception.Message
                }}
            ";
            
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script.Replace("\"", "\\\"")}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            using var process = System.Diagnostics.Process.Start(psi);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                if (output.Contains("OK"))
                {
                    _mensajeConexion = $"Conexion '{nombreConexion}' creada. Ahora puede conectar.";
                    _exitoConexion = true;
                }
                else
                {
                    _mensajeConexion = $"Resultado: {output}";
                    _exitoConexion = false;
                }
            }
        }
        catch (Exception ex)
        {
            _mensajeConexion = $"Error: {ex.Message}";
            _exitoConexion = false;
        }
        
        StateHasChanged();
    }
    
    private async Task ProbarCliente(Cliente cli)
    {
        if (string.IsNullOrEmpty(cli.IpVpnAsignada)) return;
        
        try
        {
            var url = $"http://{cli.IpVpnAsignada}:{cli.PuertoSistema ?? 5095}/";
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("alert", $"Conexion exitosa a {cli.RazonSocial}");
            }
            else
            {
                await JS.InvokeVoidAsync("alert", $"Sistema respondio con codigo {(int)response.StatusCode}");
            }
        }
        catch (TaskCanceledException)
        {
            await JS.InvokeVoidAsync("alert", $"Timeout: {cli.RazonSocial} no responde (5s)");
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("alert", $"Error: {ex.Message}");
        }
    }
    
    private async Task AbrirSistemaCliente(Cliente cli)
    {
        if (string.IsNullOrEmpty(cli.IpVpnAsignada)) return;
        var url = $"http://{cli.IpVpnAsignada}:{cli.PuertoSistema ?? 5095}/";
        await JS.InvokeVoidAsync("open", url, "_blank");
    }
    
    private void EditarCliente(Cliente cli)
    {
        Navigation.NavigateTo($"/clientes/editar/{cli.IdCliente}");
    }
    
    private void Volver()
    {
        Navigation.NavigateTo("/configuracion");
    }
    
    // ========== DIAGNOSTICO FIREWALL ==========
    private bool _verificandoFirewall = false;
    private List<DiagnosticoItem>? _diagnosticoFirewall;
    
    public class DiagnosticoItem
    {
        public string Descripcion { get; set; } = "";
        public bool Ok { get; set; }
    }
    
    private async Task VerificarFirewall()
    {
        _verificandoFirewall = true;
        _diagnosticoFirewall = new List<DiagnosticoItem>();
        StateHasChanged();
        
        try
        {
            // 1. Verificar si el servicio de firewall esta activo
            var firewallActivo = await VerificarServicioFirewall();
            _diagnosticoFirewall.Add(new DiagnosticoItem
            {
                Descripcion = "Servicio Windows Firewall activo",
                Ok = firewallActivo
            });
            
            // 2. Verificar regla para PPTP (puerto 1723 TCP)
            var reglaPPTP = await VerificarReglaFirewall("PPTP", 1723, "TCP");
            _diagnosticoFirewall.Add(new DiagnosticoItem
            {
                Descripcion = "Regla Firewall: Puerto PPTP 1723/TCP",
                Ok = reglaPPTP
            });
            
            // 3. Verificar regla GRE (protocolo 47)
            var reglaGRE = await VerificarProtocoloGRE();
            _diagnosticoFirewall.Add(new DiagnosticoItem
            {
                Descripcion = "Regla Firewall: Protocolo GRE (47)",
                Ok = reglaGRE
            });
            
            // 4. Verificar si existe la conexion VPN en Windows
            var conexionExiste = await VerificarConexionVPNWindows();
            _diagnosticoFirewall.Add(new DiagnosticoItem
            {
                Descripcion = $"Conexion VPN '{_config?.NombreConexionWindows ?? "SistemIA VPN"}' existe",
                Ok = conexionExiste
            });
            
            // 5. Verificar conectividad al servidor VPN (ping)
            if (!string.IsNullOrEmpty(_config?.ServidorVPN))
            {
                var pingOk = await VerificarPing(_config.ServidorVPN);
                _diagnosticoFirewall.Add(new DiagnosticoItem
                {
                    Descripcion = $"Ping a servidor VPN ({_config.ServidorVPN})",
                    Ok = pingOk
                });
            }
            
            // 6. Verificar puerto 1723 abierto en servidor
            if (!string.IsNullOrEmpty(_config?.ServidorVPN))
            {
                var puerto = _config.PuertoPPTP > 0 ? _config.PuertoPPTP : 1723;
                var puertoAbierto = await VerificarPuertoTCP(_config.ServidorVPN, puerto);
                _diagnosticoFirewall.Add(new DiagnosticoItem
                {
                    Descripcion = $"Puerto {puerto}/TCP accesible en servidor",
                    Ok = puertoAbierto
                });
            }
        }
        catch (Exception ex)
        {
            _diagnosticoFirewall.Add(new DiagnosticoItem
            {
                Descripcion = $"Error: {ex.Message}",
                Ok = false
            });
        }
        finally
        {
            _verificandoFirewall = false;
            StateHasChanged();
        }
    }
    
    private async Task<bool> VerificarServicioFirewall()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "-NoProfile -Command \"(Get-Service -Name 'MpsSvc').Status\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using var process = System.Diagnostics.Process.Start(psi);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                return output.Trim().Contains("Running");
            }
        }
        catch { }
        return false;
    }
    
    private async Task<bool> VerificarReglaFirewall(string nombre, int puerto, string protocolo)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -Command \"Get-NetFirewallRule -Enabled True | Where-Object {{ $_.DisplayName -like '*{nombre}*' -or $_.DisplayName -like '*VPN*' }} | Measure-Object | Select-Object -ExpandProperty Count\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using var process = System.Diagnostics.Process.Start(psi);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                if (int.TryParse(output.Trim(), out var count) && count > 0)
                    return true;
            }
            
            // Verificar por puerto especifico
            psi.Arguments = $"-NoProfile -Command \"Get-NetFirewallPortFilter | Where-Object {{ $_.LocalPort -eq '{puerto}' -and $_.Protocol -eq '{protocolo}' }} | Measure-Object | Select-Object -ExpandProperty Count\"";
            using var process2 = System.Diagnostics.Process.Start(psi);
            if (process2 != null)
            {
                var output2 = await process2.StandardOutput.ReadToEndAsync();
                await process2.WaitForExitAsync();
                if (int.TryParse(output2.Trim(), out var count2) && count2 > 0)
                    return true;
            }
        }
        catch { }
        return false;
    }
    
    private async Task<bool> VerificarProtocoloGRE()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "-NoProfile -Command \"Get-NetFirewallRule -Enabled True | Where-Object { $_.DisplayName -like '*GRE*' -or $_.DisplayName -like '*PPTP*' } | Measure-Object | Select-Object -ExpandProperty Count\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using var process = System.Diagnostics.Process.Start(psi);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                return int.TryParse(output.Trim(), out var count) && count > 0;
            }
        }
        catch { }
        return false;
    }
    
    private async Task<bool> VerificarConexionVPNWindows()
    {
        try
        {
            var nombreConexion = _config?.NombreConexionWindows ?? "SistemIA VPN";
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -Command \"Get-VpnConnection -Name '{nombreConexion}' -ErrorAction SilentlyContinue | Measure-Object | Select-Object -ExpandProperty Count\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using var process = System.Diagnostics.Process.Start(psi);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                return int.TryParse(output.Trim(), out var count) && count > 0;
            }
        }
        catch { }
        return false;
    }
    
    private async Task<bool> VerificarPing(string host)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "ping",
                Arguments = $"-n 1 -w 3000 {host}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using var process = System.Diagnostics.Process.Start(psi);
            if (process != null)
            {
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
        }
        catch { }
        return false;
    }
    
    private async Task<bool> VerificarPuertoTCP(string host, int puerto)
    {
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            var connectTask = client.ConnectAsync(host, puerto);
            var timeoutTask = Task.Delay(5000);
            
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            if (completedTask == connectTask && client.Connected)
            {
                return true;
            }
        }
        catch { }
        return false;
    }
}
