using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SistemIA.Models;

namespace SistemIA.Services;

/// <summary>
/// Interfaz para el servicio de Hub IA Central
/// </summary>
public interface IHubIACentralService
{
    /// <summary>
    /// Verifica si el Hub está habilitado
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Realiza una consulta al Hub Central de IA
    /// </summary>
    Task<HubConsultaResponse?> ConsultarAsync(string pregunta, string? contexto = null, string? paginaActual = null);

    /// <summary>
    /// Verifica el estado de conexión con el Hub
    /// </summary>
    Task<HubEstadoConexion> VerificarConexionAsync();

    /// <summary>
    /// Sincroniza los conocimientos locales con el Hub
    /// </summary>
    Task<HubSincronizarResponse?> SincronizarConocimientosAsync(List<HubConocimientoItem> conocimientos);

    /// <summary>
    /// Obtiene el historial de consultas del usuario
    /// </summary>
    Task<List<HubConsultaResponse>?> ObtenerHistorialAsync(int limite = 20);
}

/// <summary>
/// Servicio para comunicación con el Hub Central de IA
/// </summary>
public class HubIACentralService : IHubIACentralService
{
    private readonly HttpClient _httpClient;
    private readonly HubIACentralSettings _settings;
    private readonly ILogger<HubIACentralService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HubIACentralService(
        HttpClient httpClient,
        IOptions<HubIACentralSettings> settings,
        ILogger<HubIACentralService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Configurar timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        // Configurar headers por defecto
        ConfigurarHeaders();
    }

    public bool IsEnabled => _settings.Enabled && !string.IsNullOrWhiteSpace(_settings.BaseUrl);

    private void ConfigurarHeaders()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        if (!string.IsNullOrWhiteSpace(_settings.SistemaId))
            _httpClient.DefaultRequestHeaders.Add("X-Sistema-Id", _settings.SistemaId);
        
        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _settings.ApiKey);
        
        if (!string.IsNullOrWhiteSpace(_settings.ApiSecret))
            _httpClient.DefaultRequestHeaders.Add("X-API-Secret", _settings.ApiSecret);
    }

    public async Task<HubConsultaResponse?> ConsultarAsync(string pregunta, string? contexto = null, string? paginaActual = null)
    {
        if (!IsEnabled)
        {
            _logger.LogWarning("Hub IA Central no está habilitado");
            return null;
        }

        try
        {
            var request = new HubConsultaRequest
            {
                Pregunta = pregunta,
                Contexto = contexto,
                IncluirCodigo = _settings.IncluirCodigoFuente,
                PaginaActual = paginaActual
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Enviando consulta al Hub IA Central: {Pregunta}", 
                pregunta.Length > 100 ? pregunta.Substring(0, 100) + "..." : pregunta);

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/consultas", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<HubConsultaResponse>(responseJson, _jsonOptions);
                
                _logger.LogInformation("Respuesta recibida del Hub en {TiempoMs}ms, tokens: {Entrada}/{Salida}",
                    resultado?.TiempoMs ?? 0,
                    resultado?.Tokens?.Entrada ?? 0,
                    resultado?.Tokens?.Salida ?? 0);

                return resultado;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error del Hub IA Central: {StatusCode} - {Error}", 
                    response.StatusCode, errorContent);

                return new HubConsultaResponse
                {
                    Success = false,
                    Error = $"Error {(int)response.StatusCode}: {errorContent}"
                };
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Timeout al consultar Hub IA Central después de {Timeout}s", _settings.TimeoutSeconds);
            return new HubConsultaResponse
            {
                Success = false,
                Error = $"Timeout: La consulta tardó más de {_settings.TimeoutSeconds} segundos"
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión con Hub IA Central: {Url}", _settings.BaseUrl);
            return new HubConsultaResponse
            {
                Success = false,
                Error = $"Error de conexión: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al consultar Hub IA Central");
            return new HubConsultaResponse
            {
                Success = false,
                Error = $"Error inesperado: {ex.Message}"
            };
        }
    }

    public async Task<HubEstadoConexion> VerificarConexionAsync()
    {
        if (!IsEnabled)
        {
            return new HubEstadoConexion
            {
                Conectado = false,
                Habilitado = false,
                Mensaje = "Hub IA Central no está habilitado en la configuración"
            };
        }

        try
        {
            var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/health");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                
                // Intentar parsear información adicional
                try
                {
                    var healthInfo = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
                    return new HubEstadoConexion
                    {
                        Conectado = true,
                        Habilitado = _settings.Enabled,
                        UltimaConexion = DateTime.Now,
                        Version = healthInfo?.GetValueOrDefault("version").GetString() ?? "desconocida",
                        Mensaje = "Conexión exitosa",
                        ConsultasHoy = healthInfo?.GetValueOrDefault("consultas_hoy").TryGetInt32(out var c) == true ? c : null,
                        LimiteConsultas = healthInfo?.GetValueOrDefault("limite").TryGetInt32(out var l) == true ? l : null
                    };
                }
                catch
                {
                    return new HubEstadoConexion
                    {
                        Conectado = true,
                        Habilitado = _settings.Enabled,
                        UltimaConexion = DateTime.Now,
                        Mensaje = "Conexión exitosa"
                    };
                }
            }
            else
            {
                return new HubEstadoConexion
                {
                    Conectado = false,
                    Habilitado = _settings.Enabled,
                    Mensaje = $"Error HTTP {(int)response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            return new HubEstadoConexion
            {
                Conectado = false,
                Habilitado = _settings.Enabled,
                Mensaje = $"Error de conexión: {ex.Message}"
            };
        }
    }

    public async Task<HubSincronizarResponse?> SincronizarConocimientosAsync(List<HubConocimientoItem> conocimientos)
    {
        if (!IsEnabled)
        {
            _logger.LogWarning("Hub IA Central no está habilitado para sincronización");
            return null;
        }

        try
        {
            var request = new HubSincronizarConocimientosRequest
            {
                SistemaId = _settings.SistemaId,
                Conocimientos = conocimientos
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sincronizando {Count} conocimientos con Hub IA Central", conocimientos.Count);

            var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/conocimientos/importar", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<HubSincronizarResponse>(responseJson, _jsonOptions);

                _logger.LogInformation("Sincronización completada: {Insertados} insertados, {Actualizados} actualizados",
                    resultado?.Insertados ?? 0, resultado?.Actualizados ?? 0);

                return resultado;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error al sincronizar con Hub: {StatusCode} - {Error}",
                    response.StatusCode, errorContent);

                return new HubSincronizarResponse
                {
                    Success = false,
                    Errores = new List<string> { $"Error {(int)response.StatusCode}: {errorContent}" }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al sincronizar conocimientos con Hub");
            return new HubSincronizarResponse
            {
                Success = false,
                Errores = new List<string> { ex.Message }
            };
        }
    }

    public async Task<List<HubConsultaResponse>?> ObtenerHistorialAsync(int limite = 20)
    {
        if (!IsEnabled)
        {
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/consultas/historial?limite={limite}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<HistorialWrapper>(content, _jsonOptions);
                return resultado?.Consultas;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial del Hub");
            return null;
        }
    }

    private class HistorialWrapper
    {
        public List<HubConsultaResponse>? Consultas { get; set; }
        public int Total { get; set; }
    }
}

/// <summary>
/// Servicio stub para cuando el Hub no está configurado
/// </summary>
public class HubIACentralServiceStub : IHubIACentralService
{
    public bool IsEnabled => false;

    public Task<HubConsultaResponse?> ConsultarAsync(string pregunta, string? contexto = null, string? paginaActual = null)
        => Task.FromResult<HubConsultaResponse?>(null);

    public Task<HubEstadoConexion> VerificarConexionAsync()
        => Task.FromResult(new HubEstadoConexion 
        { 
            Conectado = false, 
            Habilitado = false,
            Mensaje = "Hub IA Central no configurado" 
        });

    public Task<HubSincronizarResponse?> SincronizarConocimientosAsync(List<HubConocimientoItem> conocimientos)
        => Task.FromResult<HubSincronizarResponse?>(null);

    public Task<List<HubConsultaResponse>?> ObtenerHistorialAsync(int limite = 20)
        => Task.FromResult<List<HubConsultaResponse>?>(null);
}
