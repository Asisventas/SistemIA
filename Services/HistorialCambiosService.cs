using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services;

/// <summary>
/// Servicio para registrar cambios del sistema automáticamente.
/// La IA debe usar este servicio para documentar los trabajos realizados.
/// </summary>
public interface IHistorialCambiosService
{
    /// <summary>
    /// Registra un cambio en el historial del sistema.
    /// </summary>
    Task<HistorialCambioSistema> RegistrarCambioAsync(RegistroCambioDto cambio);

    /// <summary>
    /// Inicia una nueva sesión de conversación con IA.
    /// </summary>
    Task<ConversacionIAHistorial> IniciarConversacionAsync(string titulo, string modeloIA = "Claude Opus 4.5");

    /// <summary>
    /// Actualiza una sesión de conversación existente.
    /// </summary>
    Task<ConversacionIAHistorial?> ActualizarConversacionAsync(int idConversacion, ActualizarConversacionDto datos);

    /// <summary>
    /// Finaliza una sesión de conversación.
    /// </summary>
    Task FinalizarConversacionAsync(int idConversacion, string? resumenFinal = null, string? tareasPendientes = null);

    /// <summary>
    /// Obtiene el resumen de cambios recientes para contexto de la IA.
    /// </summary>
    Task<string> ObtenerResumenCambiosRecientesAsync(int dias = 30, string? tema = null);

    /// <summary>
    /// Obtiene los cambios más recientes.
    /// </summary>
    Task<List<HistorialCambioSistema>> ObtenerCambiosRecientesAsync(int cantidad = 50, string? tema = null);

    /// <summary>
    /// Busca cambios por múltiples criterios.
    /// </summary>
    Task<List<HistorialCambioSistema>> BuscarCambiosAsync(BusquedaCambiosDto filtros);
}

public class HistorialCambiosService : IHistorialCambiosService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ILogger<HistorialCambiosService> _logger;

    public HistorialCambiosService(
        IDbContextFactory<AppDbContext> dbFactory,
        ILogger<HistorialCambiosService> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    public async Task<HistorialCambioSistema> RegistrarCambioAsync(RegistroCambioDto cambio)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var historial = new HistorialCambioSistema
        {
            Version = cambio.Version,
            FechaCambio = DateTime.Now,
            TituloCambio = cambio.Titulo,
            Tema = cambio.Tema,
            TipoCambio = cambio.TipoCambio ?? "Mejora",
            ModuloAfectado = cambio.ModuloAfectado,
            Prioridad = cambio.Prioridad ?? "Media",
            DescripcionBreve = cambio.DescripcionBreve,
            DescripcionTecnica = cambio.DescripcionTecnica,
            ArchivosModificados = cambio.ArchivosModificados,
            Tags = cambio.Tags,
            Referencias = cambio.Referencias,
            Notas = cambio.Notas,
            ImplementadoPor = cambio.ImplementadoPor ?? "Claude Opus 4.5",
            ReferenciaTicket = cambio.ReferenciaTicket,
            IdConversacionIA = cambio.IdConversacionIA,
            Estado = "Implementado",
            RequiereMigracion = cambio.RequiereMigracion,
            NombreMigracion = cambio.NombreMigracion,
            FechaCreacion = DateTime.Now
        };

        ctx.HistorialCambiosSistema.Add(historial);
        await ctx.SaveChangesAsync();

        // Actualizar contador de cambios en la conversación si aplica
        if (cambio.IdConversacionIA.HasValue)
        {
            var conv = await ctx.ConversacionesIAHistorial.FindAsync(cambio.IdConversacionIA.Value);
            if (conv != null)
            {
                conv.CantidadCambios++;
                conv.FechaModificacion = DateTime.Now;
                await ctx.SaveChangesAsync();
            }
        }

        _logger.LogInformation("Cambio registrado: {Titulo} - Tema: {Tema}", cambio.Titulo, cambio.Tema);
        return historial;
    }

    public async Task<ConversacionIAHistorial> IniciarConversacionAsync(string titulo, string modeloIA = "Claude Opus 4.5")
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();

        var conversacion = new ConversacionIAHistorial
        {
            FechaInicio = DateTime.Now,
            ModeloIA = modeloIA,
            Titulo = titulo,
            ResumenEjecutivo = $"Sesión iniciada: {titulo}",
            Complejidad = "Moderado",
            FechaCreacion = DateTime.Now
        };

        ctx.ConversacionesIAHistorial.Add(conversacion);
        await ctx.SaveChangesAsync();

        _logger.LogInformation("Conversación IA iniciada: {Titulo} con {ModeloIA}", titulo, modeloIA);
        return conversacion;
    }

    public async Task<ConversacionIAHistorial?> ActualizarConversacionAsync(int idConversacion, ActualizarConversacionDto datos)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var conv = await ctx.ConversacionesIAHistorial.FindAsync(idConversacion);
        
        if (conv == null) return null;

        if (!string.IsNullOrEmpty(datos.ResumenEjecutivo))
            conv.ResumenEjecutivo = datos.ResumenEjecutivo;
        
        if (!string.IsNullOrEmpty(datos.ObjetivosSesion))
            conv.ObjetivosSesion = datos.ObjetivosSesion;
        
        if (!string.IsNullOrEmpty(datos.ResultadosObtenidos))
            conv.ResultadosObtenidos = datos.ResultadosObtenidos;
        
        if (!string.IsNullOrEmpty(datos.TareasPendientes))
            conv.TareasPendientes = datos.TareasPendientes;
        
        if (!string.IsNullOrEmpty(datos.ModulosTrabajados))
            conv.ModulosTrabajados = datos.ModulosTrabajados;
        
        if (!string.IsNullOrEmpty(datos.ArchivosCreados))
            conv.ArchivosCreados = datos.ArchivosCreados;
        
        if (!string.IsNullOrEmpty(datos.ArchivosModificados))
            conv.ArchivosModificados = datos.ArchivosModificados;
        
        if (!string.IsNullOrEmpty(datos.MigracionesGeneradas))
            conv.MigracionesGeneradas = datos.MigracionesGeneradas;
        
        if (!string.IsNullOrEmpty(datos.ProblemasResoluciones))
            conv.ProblemasResoluciones = datos.ProblemasResoluciones;
        
        if (!string.IsNullOrEmpty(datos.DecisionesTecnicas))
            conv.DecisionesTecnicas = datos.DecisionesTecnicas;
        
        if (!string.IsNullOrEmpty(datos.Etiquetas))
            conv.Etiquetas = datos.Etiquetas;
        
        if (!string.IsNullOrEmpty(datos.Complejidad))
            conv.Complejidad = datos.Complejidad;
        
        if (datos.DuracionMinutos.HasValue)
            conv.DuracionMinutos = datos.DuracionMinutos;

        conv.FechaModificacion = DateTime.Now;

        await ctx.SaveChangesAsync();
        return conv;
    }

    public async Task FinalizarConversacionAsync(int idConversacion, string? resumenFinal = null, string? tareasPendientes = null)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        var conv = await ctx.ConversacionesIAHistorial.FindAsync(idConversacion);
        
        if (conv == null) return;

        conv.FechaFin = DateTime.Now;
        if (conv.FechaInicio != default)
            conv.DuracionMinutos = (int)(DateTime.Now - conv.FechaInicio).TotalMinutes;
        
        if (!string.IsNullOrEmpty(resumenFinal))
            conv.ResumenEjecutivo = resumenFinal;
        
        if (!string.IsNullOrEmpty(tareasPendientes))
            conv.TareasPendientes = tareasPendientes;

        conv.FechaModificacion = DateTime.Now;
        await ctx.SaveChangesAsync();

        _logger.LogInformation("Conversación IA finalizada: {Id} - Duración: {Min} min", idConversacion, conv.DuracionMinutos);
    }

    public async Task<string> ObtenerResumenCambiosRecientesAsync(int dias = 30, string? tema = null)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        
        var fechaDesde = DateTime.Now.AddDays(-dias);
        var query = ctx.HistorialCambiosSistema
            .Where(h => h.FechaCambio >= fechaDesde)
            .OrderByDescending(h => h.FechaCambio)
            .AsQueryable();

        if (!string.IsNullOrEmpty(tema))
            query = query.Where(h => h.Tema == tema);

        var cambios = await query.Take(100).ToListAsync();

        if (!cambios.Any())
            return $"No hay cambios registrados en los últimos {dias} días.";

        var resumen = new System.Text.StringBuilder();
        resumen.AppendLine($"=== RESUMEN DE CAMBIOS - Últimos {dias} días ===\n");

        // Agrupar por tema
        var porTema = cambios.GroupBy(c => c.Tema ?? "Sin Tema");
        foreach (var grupo in porTema.OrderByDescending(g => g.Count()))
        {
            resumen.AppendLine($"\n📁 {grupo.Key} ({grupo.Count()} cambios):");
            foreach (var c in grupo.Take(10))
            {
                resumen.AppendLine($"  • [{c.FechaCambio:dd/MM}] {c.TituloCambio}");
                if (!string.IsNullOrEmpty(c.DescripcionBreve))
                    resumen.AppendLine($"    └─ {c.DescripcionBreve}");
            }
            if (grupo.Count() > 10)
                resumen.AppendLine($"    ... y {grupo.Count() - 10} más");
        }

        // Estadísticas
        resumen.AppendLine($"\n📊 ESTADÍSTICAS:");
        resumen.AppendLine($"  • Total de cambios: {cambios.Count}");
        resumen.AppendLine($"  • Nuevas funcionalidades: {cambios.Count(c => c.TipoCambio == "Nueva Funcionalidad")}");
        resumen.AppendLine($"  • Mejoras: {cambios.Count(c => c.TipoCambio == "Mejora")}");
        resumen.AppendLine($"  • Correcciones: {cambios.Count(c => c.TipoCambio == "Corrección")}");
        resumen.AppendLine($"  • Con migraciones BD: {cambios.Count(c => c.RequiereMigracion)}");

        return resumen.ToString();
    }

    public async Task<List<HistorialCambioSistema>> ObtenerCambiosRecientesAsync(int cantidad = 50, string? tema = null)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        
        var query = ctx.HistorialCambiosSistema
            .OrderByDescending(h => h.FechaCambio)
            .AsQueryable();

        if (!string.IsNullOrEmpty(tema))
            query = query.Where(h => h.Tema == tema);

        return await query.Take(cantidad).ToListAsync();
    }

    public async Task<List<HistorialCambioSistema>> BuscarCambiosAsync(BusquedaCambiosDto filtros)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync();
        
        var query = ctx.HistorialCambiosSistema.AsQueryable();

        if (filtros.FechaDesde.HasValue)
            query = query.Where(h => h.FechaCambio >= filtros.FechaDesde.Value);
        
        if (filtros.FechaHasta.HasValue)
            query = query.Where(h => h.FechaCambio <= filtros.FechaHasta.Value);
        
        if (!string.IsNullOrEmpty(filtros.Tema))
            query = query.Where(h => h.Tema == filtros.Tema);
        
        if (!string.IsNullOrEmpty(filtros.TipoCambio))
            query = query.Where(h => h.TipoCambio == filtros.TipoCambio);
        
        if (!string.IsNullOrEmpty(filtros.ModuloAfectado))
            query = query.Where(h => h.ModuloAfectado == filtros.ModuloAfectado);
        
        if (!string.IsNullOrEmpty(filtros.TextoBusqueda))
            query = query.Where(h => 
                h.TituloCambio.Contains(filtros.TextoBusqueda) ||
                (h.DescripcionBreve != null && h.DescripcionBreve.Contains(filtros.TextoBusqueda)) ||
                (h.DescripcionTecnica != null && h.DescripcionTecnica.Contains(filtros.TextoBusqueda)) ||
                (h.Tags != null && h.Tags.Contains(filtros.TextoBusqueda)));
        
        if (!string.IsNullOrEmpty(filtros.Tags))
            query = query.Where(h => h.Tags != null && h.Tags.Contains(filtros.Tags));

        return await query
            .OrderByDescending(h => h.FechaCambio)
            .Take(filtros.Cantidad ?? 100)
            .ToListAsync();
    }
}

// DTOs para el servicio
public class RegistroCambioDto
{
    public string? Version { get; set; }
    public required string Titulo { get; set; }
    public required string Tema { get; set; }
    public string? TipoCambio { get; set; } = "Mejora";
    public string? ModuloAfectado { get; set; }
    public string? Prioridad { get; set; } = "Media";
    public required string DescripcionBreve { get; set; }
    public string? DescripcionTecnica { get; set; }
    public string? ArchivosModificados { get; set; }
    public string? Tags { get; set; }
    public string? Referencias { get; set; }
    public string? Notas { get; set; }
    public string? ImplementadoPor { get; set; }
    public string? ReferenciaTicket { get; set; }
    public int? IdConversacionIA { get; set; }
    public bool RequiereMigracion { get; set; }
    public string? NombreMigracion { get; set; }
}

public class ActualizarConversacionDto
{
    public string? ResumenEjecutivo { get; set; }
    public string? ObjetivosSesion { get; set; }
    public string? ResultadosObtenidos { get; set; }
    public string? TareasPendientes { get; set; }
    public string? ModulosTrabajados { get; set; }
    public string? ArchivosCreados { get; set; }
    public string? ArchivosModificados { get; set; }
    public string? MigracionesGeneradas { get; set; }
    public string? ProblemasResoluciones { get; set; }
    public string? DecisionesTecnicas { get; set; }
    public string? Etiquetas { get; set; }
    public string? Complejidad { get; set; }
    public int? DuracionMinutos { get; set; }
}

public class BusquedaCambiosDto
{
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public string? Tema { get; set; }
    public string? TipoCambio { get; set; }
    public string? ModuloAfectado { get; set; }
    public string? TextoBusqueda { get; set; }
    public string? Tags { get; set; }
    public int? Cantidad { get; set; } = 100;
}
