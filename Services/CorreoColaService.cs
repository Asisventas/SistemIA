using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para encolar correos cuando no hay conexión y procesarlos posteriormente.
    /// Evita que el sistema se cuelgue o genere loops cuando falla el envío de correos.
    /// </summary>
    public interface ICorreoColaService
    {
        /// <summary>
        /// Encola un correo para envío posterior
        /// </summary>
        Task<int> EncolarCorreoAsync(string destinatario, string asunto, string cuerpoHtml, 
            int sucursalId, string tipoCorreo = "General", int? idReferencia = null,
            List<(string nombre, byte[] contenido, string mimeType)>? adjuntos = null);

        /// <summary>
        /// Procesa los correos pendientes en la cola
        /// </summary>
        Task<(int procesados, int exitosos, int fallidos)> ProcesarColaPendienteAsync(int maxCorreos = 10);

        /// <summary>
        /// Obtiene la cantidad de correos pendientes
        /// </summary>
        Task<int> ObtenerCantidadPendientesAsync();

        /// <summary>
        /// Cancela un correo pendiente
        /// </summary>
        Task<bool> CancelarCorreoAsync(int idCorreoPendiente);
    }

    public class CorreoColaService : ICorreoColaService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ICorreoService _correoService;
        private readonly ILogger<CorreoColaService> _logger;

        // Delays para backoff exponencial (en minutos)
        private readonly int[] _delaysMinutos = { 1, 2, 5, 10, 30 };

        public CorreoColaService(
            IDbContextFactory<AppDbContext> dbFactory,
            ICorreoService correoService,
            ILogger<CorreoColaService> logger)
        {
            _dbFactory = dbFactory;
            _correoService = correoService;
            _logger = logger;
        }

        public async Task<int> EncolarCorreoAsync(string destinatario, string asunto, string cuerpoHtml,
            int sucursalId, string tipoCorreo = "General", int? idReferencia = null,
            List<(string nombre, byte[] contenido, string mimeType)>? adjuntos = null)
        {
            try
            {
                await using var ctx = await _dbFactory.CreateDbContextAsync();

                var correoPendiente = new CorreoPendiente
                {
                    Destinatario = destinatario,
                    Asunto = asunto,
                    CuerpoHtml = cuerpoHtml,
                    IdSucursal = sucursalId,
                    TipoCorreo = tipoCorreo,
                    IdReferencia = idReferencia,
                    Estado = "Pendiente",
                    Intentos = 0,
                    FechaCreacion = DateTime.Now,
                    ProximoIntento = DateTime.Now // Intentar inmediatamente
                };

                // Serializar adjuntos si hay
                if (adjuntos?.Any() == true)
                {
                    var adjuntosSerializables = adjuntos.Select(a => new
                    {
                        nombre = a.nombre,
                        contenidoBase64 = Convert.ToBase64String(a.contenido),
                        mimeType = a.mimeType
                    }).ToList();
                    correoPendiente.AdjuntosJson = JsonSerializer.Serialize(adjuntosSerializables);
                }

                ctx.CorreosPendientes.Add(correoPendiente);
                await ctx.SaveChangesAsync();

                _logger.LogInformation("[CorreoCola] Correo encolado #{Id}: {Tipo} a {Dest}", 
                    correoPendiente.IdCorreoPendiente, tipoCorreo, destinatario);

                return correoPendiente.IdCorreoPendiente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CorreoCola] Error al encolar correo para {Dest}", destinatario);
                return 0;
            }
        }

        public async Task<(int procesados, int exitosos, int fallidos)> ProcesarColaPendienteAsync(int maxCorreos = 10)
        {
            int procesados = 0, exitosos = 0, fallidos = 0;

            try
            {
                await using var ctx = await _dbFactory.CreateDbContextAsync();

                // Obtener correos que deben reintentarse
                var correosPendientes = await ctx.CorreosPendientes
                    .Where(c => c.Estado == "Pendiente" 
                             && c.Intentos < c.MaxIntentos
                             && (c.ProximoIntento == null || c.ProximoIntento <= DateTime.Now))
                    .OrderBy(c => c.FechaCreacion)
                    .Take(maxCorreos)
                    .ToListAsync();

                if (!correosPendientes.Any())
                {
                    return (0, 0, 0);
                }

                _logger.LogInformation("[CorreoCola] Procesando {Count} correos pendientes", correosPendientes.Count);

                foreach (var correo in correosPendientes)
                {
                    procesados++;
                    correo.Intentos++;
                    correo.FechaUltimoIntento = DateTime.Now;

                    try
                    {
                        // Deserializar adjuntos si hay
                        List<(string nombre, byte[] contenido, string mimeType)>? adjuntos = null;
                        if (!string.IsNullOrEmpty(correo.AdjuntosJson))
                        {
                            var adjuntosData = JsonSerializer.Deserialize<List<AdjuntoDto>>(correo.AdjuntosJson);
                            if (adjuntosData?.Any() == true)
                            {
                                adjuntos = adjuntosData.Select(a => (
                                    a.nombre,
                                    Convert.FromBase64String(a.contenidoBase64),
                                    a.mimeType
                                )).ToList();
                            }
                        }

                        // Obtener configuración de correo
                        var config = await ctx.ConfiguracionesCorreo
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Activo && c.IdSucursal == correo.IdSucursal);

                        if (config == null)
                        {
                            correo.UltimoError = "No hay configuración de correo activa";
                            correo.Estado = "Fallido";
                            fallidos++;
                            continue;
                        }

                        // Intentar enviar
                        var (exito, mensaje) = await _correoService.EnviarCorreoAsync(
                            config, correo.Destinatario, correo.Asunto, correo.CuerpoHtml, adjuntos);

                        if (exito)
                        {
                            correo.Estado = "Enviado";
                            correo.FechaEnvio = DateTime.Now;
                            correo.UltimoError = null;
                            exitosos++;
                            _logger.LogInformation("[CorreoCola] ✓ Correo #{Id} enviado a {Dest}", 
                                correo.IdCorreoPendiente, correo.Destinatario);
                        }
                        else
                        {
                            correo.UltimoError = mensaje;
                            
                            // Calcular próximo intento con backoff exponencial
                            if (correo.Intentos < correo.MaxIntentos)
                            {
                                var delayIndex = Math.Min(correo.Intentos - 1, _delaysMinutos.Length - 1);
                                var delayMinutos = _delaysMinutos[delayIndex];
                                correo.ProximoIntento = DateTime.Now.AddMinutes(delayMinutos);
                                _logger.LogWarning("[CorreoCola] Correo #{Id} falló (intento {Int}/{Max}). Próximo intento en {Min} min: {Error}",
                                    correo.IdCorreoPendiente, correo.Intentos, correo.MaxIntentos, delayMinutos, mensaje);
                            }
                            else
                            {
                                correo.Estado = "Fallido";
                                fallidos++;
                                _logger.LogError("[CorreoCola] Correo #{Id} marcado como fallido después de {Int} intentos",
                                    correo.IdCorreoPendiente, correo.Intentos);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        correo.UltimoError = ex.Message;
                        
                        // Verificar si es error de conexión (sin internet)
                        if (EsErrorDeConexion(ex))
                        {
                            // Backoff más largo para errores de conexión
                            var delayMinutos = 5;
                            correo.ProximoIntento = DateTime.Now.AddMinutes(delayMinutos);
                            _logger.LogWarning("[CorreoCola] Sin conexión. Correo #{Id} reintentará en {Min} min",
                                correo.IdCorreoPendiente, delayMinutos);
                        }
                        else if (correo.Intentos >= correo.MaxIntentos)
                        {
                            correo.Estado = "Fallido";
                            fallidos++;
                        }

                        _logger.LogError(ex, "[CorreoCola] Error procesando correo #{Id}", correo.IdCorreoPendiente);
                    }
                }

                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CorreoCola] Error general procesando cola");
            }

            return (procesados, exitosos, fallidos);
        }

        public async Task<int> ObtenerCantidadPendientesAsync()
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            return await ctx.CorreosPendientes
                .CountAsync(c => c.Estado == "Pendiente" && c.Intentos < c.MaxIntentos);
        }

        public async Task<bool> CancelarCorreoAsync(int idCorreoPendiente)
        {
            try
            {
                await using var ctx = await _dbFactory.CreateDbContextAsync();
                var correo = await ctx.CorreosPendientes.FindAsync(idCorreoPendiente);
                
                if (correo == null || correo.Estado != "Pendiente")
                    return false;

                correo.Estado = "Cancelado";
                await ctx.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Detecta si el error es por falta de conexión a internet
        /// </summary>
        private bool EsErrorDeConexion(Exception ex)
        {
            var mensaje = ex.Message.ToLower();
            var innerMensaje = ex.InnerException?.Message?.ToLower() ?? "";

            return mensaje.Contains("unable to connect") ||
                   mensaje.Contains("no such host") ||
                   mensaje.Contains("network") ||
                   mensaje.Contains("timeout") ||
                   mensaje.Contains("connection") ||
                   innerMensaje.Contains("unable to connect") ||
                   innerMensaje.Contains("network") ||
                   innerMensaje.Contains("socket");
        }

        private class AdjuntoDto
        {
            public string nombre { get; set; } = "";
            public string contenidoBase64 { get; set; } = "";
            public string mimeType { get; set; } = "";
        }
    }
}
