using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Cryptography;
using System.Text;

namespace SistemIA.Services;

/// <summary>
/// Servicio de autenticación SA (Super Administrador) con memoria de sesión.
/// Una vez autenticado, recuerda la sesión por 30 minutos de inactividad o hasta cierre de sesión.
/// </summary>
public interface ISaAuthenticationService
{
    /// <summary>
    /// Verifica si el usuario está autenticado como SA
    /// </summary>
    Task<bool> EstaAutenticadoAsync();
    
    /// <summary>
    /// Intenta autenticar con la contraseña proporcionada
    /// </summary>
    /// <returns>True si la contraseña es correcta</returns>
    Task<bool> AutenticarAsync(string password);
    
    /// <summary>
    /// Cierra la sesión SA
    /// </summary>
    Task CerrarSesionAsync();
    
    /// <summary>
    /// Actualiza el timestamp de última actividad (para extender los 30 minutos)
    /// </summary>
    Task ActualizarActividadAsync();
    
    /// <summary>
    /// Evento que se dispara cuando cambia el estado de autenticación
    /// </summary>
    event Action? OnAuthStateChanged;
}

public class SaAuthenticationService : ISaAuthenticationService
{
    private const string PASSWORD_SA = "%L4V1CT0R14";
    private const string SESSION_KEY = "sa_auth_session";
    private const int TIMEOUT_MINUTOS = 30;
    
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ILogger<SaAuthenticationService> _logger;
    
    // Cache en memoria para evitar lecturas frecuentes al storage
    private bool? _cachedAuthState;
    private DateTime? _lastCheck;
    
    public event Action? OnAuthStateChanged;

    public SaAuthenticationService(
        ProtectedSessionStorage sessionStorage,
        ILogger<SaAuthenticationService> logger)
    {
        _sessionStorage = sessionStorage;
        _logger = logger;
    }

    public async Task<bool> EstaAutenticadoAsync()
    {
        try
        {
            // Usar cache si fue verificado recientemente (últimos 5 segundos)
            if (_cachedAuthState.HasValue && _lastCheck.HasValue && 
                (DateTime.UtcNow - _lastCheck.Value).TotalSeconds < 5)
            {
                return _cachedAuthState.Value;
            }
            
            var result = await _sessionStorage.GetAsync<SaSession>(SESSION_KEY);
            
            if (!result.Success || result.Value == null)
            {
                _cachedAuthState = false;
                _lastCheck = DateTime.UtcNow;
                return false;
            }
            
            var session = result.Value;
            
            // Verificar si no ha expirado (30 minutos de inactividad)
            if (DateTime.UtcNow - session.UltimaActividad > TimeSpan.FromMinutes(TIMEOUT_MINUTOS))
            {
                _logger.LogInformation("Sesión SA expirada por inactividad");
                await CerrarSesionAsync();
                _cachedAuthState = false;
                _lastCheck = DateTime.UtcNow;
                return false;
            }
            
            _cachedAuthState = true;
            _lastCheck = DateTime.UtcNow;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al verificar sesión SA");
            _cachedAuthState = false;
            _lastCheck = DateTime.UtcNow;
            return false;
        }
    }

    public async Task<bool> AutenticarAsync(string password)
    {
        if (password != PASSWORD_SA)
        {
            _logger.LogWarning("Intento de autenticación SA fallido");
            return false;
        }
        
        try
        {
            var session = new SaSession
            {
                FechaAutenticacion = DateTime.UtcNow,
                UltimaActividad = DateTime.UtcNow,
                Token = GenerarToken()
            };
            
            await _sessionStorage.SetAsync(SESSION_KEY, session);
            _cachedAuthState = true;
            _lastCheck = DateTime.UtcNow;
            
            _logger.LogInformation("Usuario autenticado como SA exitosamente");
            OnAuthStateChanged?.Invoke();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar sesión SA");
            return false;
        }
    }

    public async Task CerrarSesionAsync()
    {
        try
        {
            await _sessionStorage.DeleteAsync(SESSION_KEY);
            _cachedAuthState = false;
            _lastCheck = DateTime.UtcNow;
            
            _logger.LogInformation("Sesión SA cerrada");
            OnAuthStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al cerrar sesión SA");
        }
    }

    public async Task ActualizarActividadAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<SaSession>(SESSION_KEY);
            
            if (result.Success && result.Value != null)
            {
                result.Value.UltimaActividad = DateTime.UtcNow;
                await _sessionStorage.SetAsync(SESSION_KEY, result.Value);
                _lastCheck = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al actualizar actividad SA");
        }
    }
    
    private static string GenerarToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
    
    private class SaSession
    {
        public DateTime FechaAutenticacion { get; set; }
        public DateTime UltimaActividad { get; set; }
        public string Token { get; set; } = "";
    }
}
