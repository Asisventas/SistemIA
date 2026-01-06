using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SistemIA.Models;
using SistemIA.Models.AsistenteIA;

namespace SistemIA.Services
{
    /// <summary>
    /// Interfaz para el servicio de tracking de acciones del usuario
    /// </summary>
    public interface ITrackingService
    {
        /// <summary>
        /// Registra una acción del usuario
        /// </summary>
        Task RegistrarAccionAsync(AccionUsuario accion);

        /// <summary>
        /// Registra una navegación
        /// </summary>
        Task RegistrarNavegacionAsync(string ruta, int? idUsuario = null, string? nombreUsuario = null, string? sessionId = null);

        /// <summary>
        /// Registra una operación (crear, editar, eliminar) con contexto de caja/sucursal
        /// </summary>
        Task RegistrarOperacionAsync(string tipoAccion, string categoria, string descripcion, 
            int? entidadId = null, string? entidadTipo = null, object? datos = null,
            int? idUsuario = null, string? nombreUsuario = null, string? sessionId = null,
            int? idSucursal = null, string? nombreSucursal = null,
            int? idCaja = null, string? nombreCaja = null,
            int? turno = null, DateTime? fechaCaja = null, DateTime? fechaEquipo = null);

        /// <summary>
        /// Registra un error
        /// </summary>
        Task RegistrarErrorAsync(string descripcion, string? mensajeError, string? stackTrace = null,
            string? ruta = null, string? componente = null,
            int? idUsuario = null, string? nombreUsuario = null, string? sessionId = null);

        /// <summary>
        /// Registra una consulta al asistente IA
        /// </summary>
        Task RegistrarConsultaIAAsync(string consulta, string? respuestaTipo, bool exitosa,
            int? idUsuario = null, string? nombreUsuario = null, string? sessionId = null);

        /// <summary>
        /// Obtiene las últimas N acciones de un usuario/sesión
        /// </summary>
        List<AccionUsuario> ObtenerAccionesRecientes(string? sessionId = null, int? idUsuario = null, int cantidad = 20);

        /// <summary>
        /// Obtiene los últimos errores
        /// </summary>
        List<AccionUsuario> ObtenerErroresRecientes(string? sessionId = null, int? idUsuario = null, int cantidad = 10);

        /// <summary>
        /// Genera un resumen de contexto para soporte (últimas acciones + errores)
        /// </summary>
        string GenerarContextoSoporte(string? sessionId = null, int? idUsuario = null);

        /// <summary>
        /// Obtiene estadísticas de uso por categoría
        /// </summary>
        Dictionary<string, int> ObtenerEstadisticasUso(DateTime? desde = null, DateTime? hasta = null);

        /// <summary>
        /// Limpia acciones antiguas de la memoria
        /// </summary>
        void LimpiarAccionesAntiguas(TimeSpan antiguedad);
    }

    /// <summary>
    /// Servicio de tracking de acciones del usuario
    /// Mantiene un historial en memoria para acceso rápido y persiste a BD periódicamente
    /// </summary>
    public class TrackingService : ITrackingService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TrackingService> _logger;
        
        // Buffer en memoria para acceso rápido (máx 1000 acciones)
        private readonly ConcurrentQueue<AccionUsuario> _bufferAcciones = new();
        private const int MAX_BUFFER_SIZE = 1000;
        
        // Buffer de errores separado para acceso rápido
        private readonly ConcurrentQueue<AccionUsuario> _bufferErrores = new();
        private const int MAX_ERRORES_BUFFER = 100;

        // Contador para persistir periódicamente
        private int _contadorAcciones = 0;
        private const int PERSISTIR_CADA_N = 50;

        public TrackingService(IServiceScopeFactory scopeFactory, ILogger<TrackingService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task RegistrarAccionAsync(AccionUsuario accion)
        {
            try
            {
                accion.FechaHora = DateTime.Now;
                
                // Agregar al buffer en memoria
                _bufferAcciones.Enqueue(accion);
                
                // Mantener tamaño del buffer
                while (_bufferAcciones.Count > MAX_BUFFER_SIZE)
                {
                    _bufferAcciones.TryDequeue(out _);
                }

                // Si es error, agregar también al buffer de errores
                if (accion.EsError)
                {
                    _bufferErrores.Enqueue(accion);
                    while (_bufferErrores.Count > MAX_ERRORES_BUFFER)
                    {
                        _bufferErrores.TryDequeue(out _);
                    }
                }

                // Persistir periódicamente (solo acciones importantes)
                _contadorAcciones++;
                if (_contadorAcciones >= PERSISTIR_CADA_N && DebePeristir(accion))
                {
                    _contadorAcciones = 0;
                    await PersistirAccionesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar acción de tracking");
            }
        }

        private bool DebePeristir(AccionUsuario accion)
        {
            // Solo persistir acciones importantes
            return accion.EsError ||
                   accion.TipoAccion == TipoAccionTracking.Login ||
                   accion.TipoAccion == TipoAccionTracking.Logout ||
                   accion.TipoAccion == TipoAccionTracking.Creacion ||
                   accion.TipoAccion == TipoAccionTracking.Edicion ||
                   accion.TipoAccion == TipoAccionTracking.Eliminacion;
        }

        private async Task PersistirAccionesAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Obtener acciones importantes del buffer
                var accionesAPersistir = _bufferAcciones
                    .Where(DebePeristir)
                    .OrderByDescending(a => a.FechaHora)
                    .Take(50)
                    .ToList();

                if (accionesAPersistir.Any())
                {
                    // Evitar duplicados
                    var ultimaFecha = await context.AccionesUsuario
                        .OrderByDescending(a => a.FechaHora)
                        .Select(a => a.FechaHora)
                        .FirstOrDefaultAsync();

                    var nuevas = accionesAPersistir
                        .Where(a => a.FechaHora > ultimaFecha)
                        .ToList();

                    if (nuevas.Any())
                    {
                        context.AccionesUsuario.AddRange(nuevas);
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al persistir acciones de tracking");
            }
        }

        public Task RegistrarNavegacionAsync(string ruta, int? idUsuario = null, string? nombreUsuario = null, string? sessionId = null)
        {
            var categoria = DetectarCategoria(ruta);
            var accion = new AccionUsuario
            {
                TipoAccion = TipoAccionTracking.Navegacion,
                Categoria = categoria,
                Descripcion = $"Navegó a {ruta}",
                Ruta = ruta,
                IdUsuario = idUsuario,
                NombreUsuario = nombreUsuario,
                SessionId = sessionId
            };
            return RegistrarAccionAsync(accion);
        }

        public Task RegistrarOperacionAsync(string tipoAccion, string categoria, string descripcion,
            int? entidadId = null, string? entidadTipo = null, object? datos = null,
            int? idUsuario = null, string? nombreUsuario = null, string? sessionId = null,
            int? idSucursal = null, string? nombreSucursal = null,
            int? idCaja = null, string? nombreCaja = null,
            int? turno = null, DateTime? fechaCaja = null, DateTime? fechaEquipo = null)
        {
            var accion = new AccionUsuario
            {
                TipoAccion = tipoAccion,
                Categoria = categoria,
                Descripcion = descripcion,
                EntidadId = entidadId,
                EntidadTipo = entidadTipo,
                DatosJson = datos != null ? JsonSerializer.Serialize(datos) : null,
                IdUsuario = idUsuario,
                NombreUsuario = nombreUsuario,
                SessionId = sessionId,
                // Contexto de caja/sucursal
                IdSucursal = idSucursal,
                NombreSucursal = nombreSucursal,
                IdCaja = idCaja,
                NombreCaja = nombreCaja,
                Turno = turno,
                FechaCaja = fechaCaja,
                FechaEquipo = fechaEquipo ?? DateTime.Now
            };
            
            // Persistir inmediatamente para operaciones CRUD
            return RegistrarYPersistirAsync(accion);
        }

        /// <summary>
        /// Registra y persiste inmediatamente una acción en la BD
        /// </summary>
        private async Task RegistrarYPersistirAsync(AccionUsuario accion)
        {
            try
            {
                accion.FechaHora = DateTime.Now;
                
                // Agregar al buffer
                _bufferAcciones.Enqueue(accion);
                while (_bufferAcciones.Count > MAX_BUFFER_SIZE)
                    _bufferAcciones.TryDequeue(out _);

                if (accion.EsError)
                {
                    _bufferErrores.Enqueue(accion);
                    while (_bufferErrores.Count > MAX_ERRORES_BUFFER)
                        _bufferErrores.TryDequeue(out _);
                }

                // Persistir inmediatamente a BD
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.AccionesUsuario.Add(accion);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar y persistir acción de tracking");
            }
        }

        public Task RegistrarErrorAsync(string descripcion, string? mensajeError, string? stackTrace = null,
            string? ruta = null, string? componente = null,
            int? idUsuario = null, string? nombreUsuario = null, string? sessionId = null)
        {
            var accion = new AccionUsuario
            {
                TipoAccion = TipoAccionTracking.Error,
                Categoria = CategoriaAccion.Sistema,
                Descripcion = descripcion,
                MensajeError = mensajeError?.Length > 1000 ? mensajeError.Substring(0, 1000) : mensajeError,
                StackTrace = stackTrace,
                Ruta = ruta,
                Componente = componente,
                EsError = true,
                IdUsuario = idUsuario,
                NombreUsuario = nombreUsuario,
                SessionId = sessionId
            };
            // Persistir inmediatamente errores
            return RegistrarYPersistirAsync(accion);
        }

        public Task RegistrarConsultaIAAsync(string consulta, string? respuestaTipo, bool exitosa,
            int? idUsuario = null, string? nombreUsuario = null, string? sessionId = null)
        {
            var accion = new AccionUsuario
            {
                TipoAccion = TipoAccionTracking.ConsultaIA,
                Categoria = CategoriaAccion.AsistenteIA,
                Descripcion = consulta.Length > 200 ? consulta.Substring(0, 200) : consulta,
                DatosJson = JsonSerializer.Serialize(new { respuestaTipo, exitosa }),
                IdUsuario = idUsuario,
                NombreUsuario = nombreUsuario,
                SessionId = sessionId
            };
            return RegistrarAccionAsync(accion);
        }

        public List<AccionUsuario> ObtenerAccionesRecientes(string? sessionId = null, int? idUsuario = null, int cantidad = 20)
        {
            var query = _bufferAcciones.AsEnumerable();

            if (!string.IsNullOrEmpty(sessionId))
                query = query.Where(a => a.SessionId == sessionId);
            
            if (idUsuario.HasValue)
                query = query.Where(a => a.IdUsuario == idUsuario);

            return query
                .OrderByDescending(a => a.FechaHora)
                .Take(cantidad)
                .ToList();
        }

        public List<AccionUsuario> ObtenerErroresRecientes(string? sessionId = null, int? idUsuario = null, int cantidad = 10)
        {
            var query = _bufferErrores.AsEnumerable();

            if (!string.IsNullOrEmpty(sessionId))
                query = query.Where(a => a.SessionId == sessionId);
            
            if (idUsuario.HasValue)
                query = query.Where(a => a.IdUsuario == idUsuario);

            return query
                .OrderByDescending(a => a.FechaHora)
                .Take(cantidad)
                .ToList();
        }

        public string GenerarContextoSoporte(string? sessionId = null, int? idUsuario = null)
        {
            var sb = new System.Text.StringBuilder();
            
            // Obtener últimas acciones
            var acciones = ObtenerAccionesRecientes(sessionId, idUsuario, 15);
            var errores = ObtenerErroresRecientes(sessionId, idUsuario, 5);

            sb.AppendLine("=== CONTEXTO DE SESIÓN ===");
            sb.AppendLine($"Fecha/Hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine();

            // Errores recientes
            if (errores.Any())
            {
                sb.AppendLine("🔴 ERRORES RECIENTES:");
                foreach (var error in errores)
                {
                    sb.AppendLine($"  [{error.FechaHora:HH:mm:ss}] {error.Descripcion}");
                    if (!string.IsNullOrEmpty(error.MensajeError))
                        sb.AppendLine($"     → {error.MensajeError}");
                    if (!string.IsNullOrEmpty(error.Ruta))
                        sb.AppendLine($"     Ruta: {error.Ruta}");
                }
                sb.AppendLine();
            }

            // Acciones recientes
            if (acciones.Any())
            {
                sb.AppendLine("📋 ÚLTIMAS ACCIONES:");
                foreach (var accion in acciones.Take(10))
                {
                    var emoji = accion.TipoAccion switch
                    {
                        TipoAccionTracking.Navegacion => "🧭",
                        TipoAccionTracking.Creacion => "➕",
                        TipoAccionTracking.Edicion => "✏️",
                        TipoAccionTracking.Eliminacion => "🗑️",
                        TipoAccionTracking.Error => "❌",
                        TipoAccionTracking.ConsultaIA => "🤖",
                        TipoAccionTracking.Busqueda => "🔍",
                        _ => "•"
                    };
                    sb.AppendLine($"  {emoji} [{accion.FechaHora:HH:mm:ss}] {accion.Descripcion}");
                }
                sb.AppendLine();
            }

            // Estadísticas rápidas
            var estadisticas = acciones
                .GroupBy(a => a.Categoria)
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .ToDictionary(g => g.Key!, g => g.Count());

            if (estadisticas.Any())
            {
                sb.AppendLine("📊 MÓDULOS UTILIZADOS:");
                foreach (var stat in estadisticas.OrderByDescending(s => s.Value))
                {
                    sb.AppendLine($"  • {stat.Key}: {stat.Value} acciones");
                }
            }

            return sb.ToString();
        }

        public Dictionary<string, int> ObtenerEstadisticasUso(DateTime? desde = null, DateTime? hasta = null)
        {
            var query = _bufferAcciones.AsEnumerable();

            if (desde.HasValue)
                query = query.Where(a => a.FechaHora >= desde.Value);
            
            if (hasta.HasValue)
                query = query.Where(a => a.FechaHora <= hasta.Value);

            return query
                .GroupBy(a => a.Categoria ?? "Sin categoría")
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public void LimpiarAccionesAntiguas(TimeSpan antiguedad)
        {
            var limite = DateTime.Now - antiguedad;
            var accionesAMantener = _bufferAcciones
                .Where(a => a.FechaHora >= limite)
                .ToList();

            // Limpiar y re-agregar
            while (_bufferAcciones.TryDequeue(out _)) { }
            foreach (var accion in accionesAMantener)
            {
                _bufferAcciones.Enqueue(accion);
            }
        }

        private string DetectarCategoria(string ruta)
        {
            ruta = ruta.ToLower();
            
            if (ruta.Contains("/venta")) return CategoriaAccion.Ventas;
            if (ruta.Contains("/compra")) return CategoriaAccion.Compras;
            if (ruta.Contains("/producto")) return CategoriaAccion.Productos;
            if (ruta.Contains("/cliente")) return CategoriaAccion.Clientes;
            if (ruta.Contains("/proveedor")) return CategoriaAccion.Proveedores;
            if (ruta.Contains("/caja") || ruta.Contains("/cierre")) return CategoriaAccion.Caja;
            if (ruta.Contains("/stock") || ruta.Contains("/inventario") || ruta.Contains("/deposito")) return CategoriaAccion.Inventario;
            if (ruta.Contains("/informe") || ruta.Contains("/reporte")) return CategoriaAccion.Reportes;
            if (ruta.Contains("/config") || ruta.Contains("/parametro")) return CategoriaAccion.Configuracion;
            if (ruta.Contains("/usuario") || ruta.Contains("/permiso")) return CategoriaAccion.Usuarios;
            if (ruta.Contains("/sifen")) return CategoriaAccion.SIFEN;
            
            return CategoriaAccion.Sistema;
        }
    }
}
