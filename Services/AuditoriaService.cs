using Microsoft.EntityFrameworkCore;
using SistemIA.Models;
using System.Text.Json;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para registro de auditoría de acciones de usuarios
    /// </summary>
    public class AuditoriaService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public AuditoriaService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        /// <summary>
        /// Registra una acción de usuario en la auditoría
        /// </summary>
        public async Task<int> RegistrarAccionAsync(
            int? idUsuario,
            string? nombreUsuario,
            string? rolUsuario,
            string accion,
            string? tipoAccion = null,
            string? modulo = null,
            string? entidad = null,
            int? idRegistroAfectado = null,
            string? descripcion = null,
            object? datosAntes = null,
            object? datosDespues = null,
            string? direccionIP = null,
            string? navegador = null,
            bool exitosa = true,
            string? mensajeError = null,
            string severidad = "INFO",
            // Nuevos parámetros de contexto
            DateTime? fechaHoraEquipo = null,
            DateTime? fechaCaja = null,
            int? turno = null,
            int? idSucursal = null,
            string? nombreSucursal = null,
            int? idCaja = null,
            string? nombreCaja = null)
        {
            try
            {
                await using var ctx = await _dbFactory.CreateDbContextAsync();

                var auditoria = new AuditoriaAccion
                {
                    IdUsuario = idUsuario,
                    NombreUsuario = nombreUsuario,
                    RolUsuario = rolUsuario,
                    FechaHora = DateTime.Now,
                    Modulo = modulo,
                    Accion = accion,
                    TipoAccion = tipoAccion,
                    Entidad = entidad,
                    IdRegistroAfectado = idRegistroAfectado,
                    Descripcion = descripcion,
                    DatosAntes = datosAntes != null ? JsonSerializer.Serialize(datosAntes) : null,
                    DatosDespues = datosDespues != null ? JsonSerializer.Serialize(datosDespues) : null,
                    DireccionIP = direccionIP,
                    Navegador = navegador,
                    Exitosa = exitosa,
                    MensajeError = mensajeError,
                    Severidad = severidad,
                    // Contexto de operación
                    FechaHoraEquipo = fechaHoraEquipo,
                    FechaCaja = fechaCaja,
                    Turno = turno,
                    IdSucursal = idSucursal,
                    NombreSucursal = nombreSucursal,
                    IdCaja = idCaja,
                    NombreCaja = nombreCaja
                };

                ctx.AuditoriasAcciones.Add(auditoria);
                await ctx.SaveChangesAsync();

                return auditoria.IdAuditoria;
            }
            catch (Exception ex)
            {
                // Log error pero no fallar la operación principal
                Console.WriteLine($"Error al registrar auditoría: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Obtiene el historial de auditoría con filtros
        /// </summary>
        public async Task<List<AuditoriaAccion>> ObtenerHistorialAsync(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idUsuario = null,
            string? modulo = null,
            string? tipoAccion = null,
            int limit = 100)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            var query = ctx.AuditoriasAcciones
                .Include(a => a.Usuario)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(a => a.FechaHora >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(a => a.FechaHora <= fechaHasta.Value);

            if (idUsuario.HasValue)
                query = query.Where(a => a.IdUsuario == idUsuario.Value);

            if (!string.IsNullOrEmpty(modulo))
                query = query.Where(a => a.Modulo == modulo);

            if (!string.IsNullOrEmpty(tipoAccion))
                query = query.Where(a => a.TipoAccion == tipoAccion);

            return await query
                .OrderByDescending(a => a.FechaHora)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene estadísticas de auditoría
        /// </summary>
        public async Task<Dictionary<string, int>> ObtenerEstadisticasAsync(
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null,
            int? idUsuario = null,
            string? modulo = null)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            var query = ctx.AuditoriasAcciones.AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(a => a.FechaHora >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(a => a.FechaHora <= fechaHasta.Value);

            if (idUsuario.HasValue)
                query = query.Where(a => a.IdUsuario == idUsuario.Value);

            if (!string.IsNullOrEmpty(modulo))
                query = query.Where(a => a.Modulo == modulo);

            var total = await query.CountAsync();

            var porTipo = await query
                .GroupBy(a => a.TipoAccion ?? "OTROS")
                .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                .ToDictionaryAsync(x => x.Tipo, x => x.Cantidad);

            porTipo.Add("TOTAL", total);

            return porTipo;
        }
    }
}
