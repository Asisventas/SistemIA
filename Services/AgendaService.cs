using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para gestionar la agenda/calendario del sistema
    /// </summary>
    public interface IAgendaService
    {
        // ========== CITAS ==========
        Task<List<CitaAgenda>> ObtenerCitasDelMesAsync(int idSucursal, int year, int month);
        Task<List<CitaAgenda>> ObtenerCitasPorRangoAsync(int idSucursal, DateTime desde, DateTime hasta);
        Task<List<CitaAgenda>> ObtenerCitasDelDiaAsync(int idSucursal, DateTime fecha);
        Task<List<CitaAgenda>> ObtenerCitasPorClienteAsync(int idCliente);
        Task<CitaAgenda?> ObtenerCitaPorIdAsync(int idCita);
        Task<CitaAgenda> CrearCitaAsync(CitaAgenda cita);
        Task<CitaAgenda> ActualizarCitaAsync(CitaAgenda cita);
        Task<bool> EliminarCitaAsync(int idCita);
        Task<bool> CambiarEstadoCitaAsync(int idCita, string nuevoEstado, string? resultado = null);
        
        // ========== RECORDATORIOS ==========
        Task<List<CitaAgenda>> ObtenerCitasConRecordatoriosPendientesAsync(int idSucursal);
        Task<bool> MarcarRecordatorioEnviadoAsync(int idCita);
        Task<List<RecordatorioCita>> ObtenerRecordatoriosPendientesAsync();
        
        // ========== COLORES DE CLIENTES ==========
        Task<string> ObtenerColorClienteAsync(int idCliente, int idSucursal);
        Task<bool> AsignarColorClienteAsync(int idCliente, int idSucursal, string colorFondo, string colorTexto);
        
        // ========== TIPOS Y ESTADOS ==========
        List<string> ObtenerTiposCita();
        List<string> ObtenerEstadosCita();
        List<string> ObtenerPrioridadesCita();
        List<(string Color, string Nombre)> ObtenerColoresDisponibles();
        
        // ========== ESTADÍSTICAS ==========
        Task<int> ContarCitasDelMesAsync(int idSucursal, int year, int month);
        Task<Dictionary<string, int>> ObtenerEstadisticasPorEstadoAsync(int idSucursal, DateTime desde, DateTime hasta);
    }

    public class AgendaService : IAgendaService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AgendaService> _logger;

        public AgendaService(AppDbContext context, ILogger<AgendaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ========== CITAS ==========

        public async Task<List<CitaAgenda>> ObtenerCitasDelMesAsync(int idSucursal, int year, int month)
        {
            var primerDia = new DateTime(year, month, 1);
            var ultimoDia = primerDia.AddMonths(1).AddDays(-1);

            return await _context.CitasAgenda
                .Include(c => c.Cliente)
                .Include(c => c.UsuarioAsignado)
                .Where(c => c.IdSucursal == idSucursal &&
                           c.FechaHoraInicio >= primerDia &&
                           c.FechaHoraInicio <= ultimoDia.AddDays(1))
                .OrderBy(c => c.FechaHoraInicio)
                .ToListAsync();
        }

        public async Task<List<CitaAgenda>> ObtenerCitasPorRangoAsync(int idSucursal, DateTime desde, DateTime hasta)
        {
            return await _context.CitasAgenda
                .Include(c => c.Cliente)
                .Include(c => c.UsuarioAsignado)
                .Where(c => c.IdSucursal == idSucursal &&
                           c.FechaHoraInicio >= desde &&
                           c.FechaHoraInicio <= hasta)
                .OrderBy(c => c.FechaHoraInicio)
                .ToListAsync();
        }

        public async Task<List<CitaAgenda>> ObtenerCitasDelDiaAsync(int idSucursal, DateTime fecha)
        {
            var inicioDia = fecha.Date;
            var finDia = fecha.Date.AddDays(1);

            return await _context.CitasAgenda
                .Include(c => c.Cliente)
                .Include(c => c.UsuarioAsignado)
                .Where(c => c.IdSucursal == idSucursal &&
                           c.FechaHoraInicio >= inicioDia &&
                           c.FechaHoraInicio < finDia)
                .OrderBy(c => c.FechaHoraInicio)
                .ToListAsync();
        }

        public async Task<List<CitaAgenda>> ObtenerCitasPorClienteAsync(int idCliente)
        {
            return await _context.CitasAgenda
                .Include(c => c.Sucursal)
                .Where(c => c.IdCliente == idCliente)
                .OrderByDescending(c => c.FechaHoraInicio)
                .ToListAsync();
        }

        public async Task<CitaAgenda?> ObtenerCitaPorIdAsync(int idCita)
        {
            return await _context.CitasAgenda
                .Include(c => c.Cliente)
                .Include(c => c.Sucursal)
                .Include(c => c.UsuarioAsignado)
                .Include(c => c.Recordatorios)
                .FirstOrDefaultAsync(c => c.IdCita == idCita);
        }

        public async Task<CitaAgenda> CrearCitaAsync(CitaAgenda cita)
        {
            // Calcular duración si no está especificada
            if (cita.DuracionMinutos == 0)
            {
                cita.DuracionMinutos = (int)(cita.FechaHoraFin - cita.FechaHoraInicio).TotalMinutes;
            }

            // Si hay cliente, copiar datos
            if (cita.IdCliente.HasValue && cita.Cliente == null)
            {
                var cliente = await _context.Clientes.FindAsync(cita.IdCliente.Value);
                if (cliente != null)
                {
                    cita.NombreCliente = cliente.RazonSocial;
                    cita.Telefono ??= cliente.Celular ?? cliente.Telefono;
                    cita.Email ??= cliente.Email;
                    
                    // Usar color del cliente si tiene
                    if (!string.IsNullOrEmpty(cliente.ColorAgenda))
                    {
                        cita.ColorFondo = cliente.ColorAgenda;
                    }
                    
                    // Copiar ubicación del cliente si no se especificó
                    if (!cita.Latitud.HasValue && cliente.Latitud.HasValue)
                    {
                        cita.Latitud = cliente.Latitud;
                        cita.Longitud = cliente.Longitud;
                        cita.Direccion ??= cliente.DireccionCompleta ?? cliente.Direccion;
                    }
                }
            }

            // Generar URL de Maps si tiene coordenadas
            cita.UrlMaps = cita.GenerarUrlMaps();

            // Crear recordatorios si están configurados
            if (cita.TieneRecordatorio)
            {
                cita.Recordatorios = new List<RecordatorioCita>();
                
                if (cita.MinutosRecordatorio1.HasValue)
                {
                    cita.Recordatorios.Add(new RecordatorioCita
                    {
                        FechaHoraProgramada = cita.FechaHoraInicio.AddMinutes(-cita.MinutosRecordatorio1.Value),
                        Tipo = cita.EnviarCorreo ? "Ambos" : "Notificacion",
                        Mensaje = $"Recordatorio: {cita.Titulo} en {cita.MinutosRecordatorio1} minutos"
                    });
                }
                
                if (cita.MinutosRecordatorio2.HasValue)
                {
                    cita.Recordatorios.Add(new RecordatorioCita
                    {
                        FechaHoraProgramada = cita.FechaHoraInicio.AddMinutes(-cita.MinutosRecordatorio2.Value),
                        Tipo = cita.EnviarCorreo ? "Ambos" : "Notificacion",
                        Mensaje = $"Recordatorio: {cita.Titulo} en {cita.MinutosRecordatorio2} minutos"
                    });
                }
            }

            cita.FechaCreacion = DateTime.Now;
            _context.CitasAgenda.Add(cita);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cita creada: {Id} - {Titulo} para {Fecha}", cita.IdCita, cita.Titulo, cita.FechaHoraInicio);
            return cita;
        }

        public async Task<CitaAgenda> ActualizarCitaAsync(CitaAgenda cita)
        {
            var citaExistente = await _context.CitasAgenda.FindAsync(cita.IdCita);
            if (citaExistente == null)
                throw new Exception($"Cita {cita.IdCita} no encontrada");

            // Actualizar propiedades
            citaExistente.TipoCita = cita.TipoCita;
            citaExistente.Titulo = cita.Titulo;
            citaExistente.Descripcion = cita.Descripcion;
            citaExistente.IdCliente = cita.IdCliente;
            citaExistente.NombreCliente = cita.NombreCliente;
            citaExistente.Telefono = cita.Telefono;
            citaExistente.Email = cita.Email;
            citaExistente.FechaHoraInicio = cita.FechaHoraInicio;
            citaExistente.FechaHoraFin = cita.FechaHoraFin;
            citaExistente.TodoElDia = cita.TodoElDia;
            citaExistente.DuracionMinutos = (int)(cita.FechaHoraFin - cita.FechaHoraInicio).TotalMinutes;
            citaExistente.Direccion = cita.Direccion;
            citaExistente.Latitud = cita.Latitud;
            citaExistente.Longitud = cita.Longitud;
            citaExistente.UrlMaps = cita.GenerarUrlMaps();
            citaExistente.ColorFondo = cita.ColorFondo;
            citaExistente.ColorTexto = cita.ColorTexto;
            citaExistente.Icono = cita.Icono;
            citaExistente.Estado = cita.Estado;
            citaExistente.Prioridad = cita.Prioridad;
            citaExistente.TieneRecordatorio = cita.TieneRecordatorio;
            citaExistente.MinutosRecordatorio1 = cita.MinutosRecordatorio1;
            citaExistente.MinutosRecordatorio2 = cita.MinutosRecordatorio2;
            citaExistente.EnviarCorreo = cita.EnviarCorreo;
            citaExistente.MostrarNotificacion = cita.MostrarNotificacion;
            citaExistente.IdUsuarioAsignado = cita.IdUsuarioAsignado;
            citaExistente.NombreAsignado = cita.NombreAsignado;
            citaExistente.Notas = cita.Notas;
            citaExistente.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Cita actualizada: {Id} - {Titulo}", cita.IdCita, cita.Titulo);
            return citaExistente;
        }

        public async Task<bool> EliminarCitaAsync(int idCita)
        {
            var cita = await _context.CitasAgenda
                .Include(c => c.Recordatorios)
                .FirstOrDefaultAsync(c => c.IdCita == idCita);
            
            if (cita == null) return false;

            // Eliminar recordatorios primero
            if (cita.Recordatorios?.Any() == true)
            {
                _context.RecordatoriosCitas.RemoveRange(cita.Recordatorios);
            }

            _context.CitasAgenda.Remove(cita);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Cita eliminada: {Id}", idCita);
            return true;
        }

        public async Task<bool> CambiarEstadoCitaAsync(int idCita, string nuevoEstado, string? resultado = null)
        {
            var cita = await _context.CitasAgenda.FindAsync(idCita);
            if (cita == null) return false;

            cita.Estado = nuevoEstado;
            if (!string.IsNullOrEmpty(resultado))
            {
                cita.Resultado = resultado;
            }
            cita.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Estado de cita {Id} cambiado a {Estado}", idCita, nuevoEstado);
            return true;
        }

        // ========== RECORDATORIOS ==========

        public async Task<List<CitaAgenda>> ObtenerCitasConRecordatoriosPendientesAsync(int idSucursal)
        {
            var ahora = DateTime.Now;
            
            return await _context.CitasAgenda
                .Include(c => c.Cliente)
                .Where(c => c.IdSucursal == idSucursal &&
                           c.TieneRecordatorio &&
                           c.Estado != "Completada" &&
                           c.Estado != "Cancelada" &&
                           !c.NotificacionMostrada &&
                           c.FechaHoraInicio > ahora)
                .OrderBy(c => c.FechaHoraInicio)
                .ToListAsync();
        }

        public async Task<bool> MarcarRecordatorioEnviadoAsync(int idCita)
        {
            var cita = await _context.CitasAgenda.FindAsync(idCita);
            if (cita == null) return false;

            cita.NotificacionMostrada = true;
            cita.FechaEnvioCorreo = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RecordatorioCita>> ObtenerRecordatoriosPendientesAsync()
        {
            var ahora = DateTime.Now;
            
            return await _context.RecordatoriosCitas
                .Include(r => r.Cita)
                    .ThenInclude(c => c!.Cliente)
                .Where(r => !r.Enviado &&
                           r.FechaHoraProgramada <= ahora &&
                           r.Cita != null &&
                           r.Cita.Estado != "Completada" &&
                           r.Cita.Estado != "Cancelada")
                .OrderBy(r => r.FechaHoraProgramada)
                .ToListAsync();
        }

        // ========== COLORES DE CLIENTES ==========

        public async Task<string> ObtenerColorClienteAsync(int idCliente, int idSucursal)
        {
            var color = await _context.ColoresClientesAgenda
                .FirstOrDefaultAsync(c => c.IdCliente == idCliente && c.IdSucursal == idSucursal);
            
            return color?.ColorFondo ?? "#3788d8";
        }

        public async Task<bool> AsignarColorClienteAsync(int idCliente, int idSucursal, string colorFondo, string colorTexto)
        {
            var colorExistente = await _context.ColoresClientesAgenda
                .FirstOrDefaultAsync(c => c.IdCliente == idCliente && c.IdSucursal == idSucursal);

            if (colorExistente != null)
            {
                colorExistente.ColorFondo = colorFondo;
                colorExistente.ColorTexto = colorTexto;
            }
            else
            {
                _context.ColoresClientesAgenda.Add(new ColorClienteAgenda
                {
                    IdCliente = idCliente,
                    IdSucursal = idSucursal,
                    ColorFondo = colorFondo,
                    ColorTexto = colorTexto
                });
            }

            // También actualizar en el cliente
            var cliente = await _context.Clientes.FindAsync(idCliente);
            if (cliente != null)
            {
                cliente.ColorAgenda = colorFondo;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ========== TIPOS Y ESTADOS ==========

        public List<string> ObtenerTiposCita()
        {
            return new List<string>
            {
                "Servicio",
                "Recordatorio",
                "Visita",
                "Reunión",
                "Entrega",
                "Llamada",
                "Cita Médica",
                "Mantenimiento",
                "Cobranza",
                "Capacitación",
                "Otro"
            };
        }

        public List<string> ObtenerEstadosCita()
        {
            return new List<string>
            {
                "Programada",
                "Confirmada",
                "EnProgreso",
                "Completada",
                "Cancelada",
                "NoAsistio"
            };
        }

        public List<string> ObtenerPrioridadesCita()
        {
            return new List<string>
            {
                "Baja",
                "Normal",
                "Alta",
                "Urgente"
            };
        }

        public List<(string Color, string Nombre)> ObtenerColoresDisponibles()
        {
            return new List<(string, string)>
            {
                ("#3788d8", "Azul"),
                ("#28a745", "Verde"),
                ("#dc3545", "Rojo"),
                ("#ffc107", "Amarillo"),
                ("#17a2b8", "Cyan"),
                ("#6f42c1", "Morado"),
                ("#fd7e14", "Naranja"),
                ("#e83e8c", "Rosa"),
                ("#20c997", "Turquesa"),
                ("#6c757d", "Gris"),
                ("#343a40", "Negro"),
                ("#007bff", "Azul Primario")
            };
        }

        // ========== ESTADÍSTICAS ==========

        public async Task<int> ContarCitasDelMesAsync(int idSucursal, int year, int month)
        {
            var primerDia = new DateTime(year, month, 1);
            var ultimoDia = primerDia.AddMonths(1);

            return await _context.CitasAgenda
                .CountAsync(c => c.IdSucursal == idSucursal &&
                                c.FechaHoraInicio >= primerDia &&
                                c.FechaHoraInicio < ultimoDia);
        }

        public async Task<Dictionary<string, int>> ObtenerEstadisticasPorEstadoAsync(int idSucursal, DateTime desde, DateTime hasta)
        {
            var citas = await _context.CitasAgenda
                .Where(c => c.IdSucursal == idSucursal &&
                           c.FechaHoraInicio >= desde &&
                           c.FechaHoraInicio <= hasta)
                .GroupBy(c => c.Estado)
                .Select(g => new { Estado = g.Key, Count = g.Count() })
                .ToListAsync();

            return citas.ToDictionary(x => x.Estado, x => x.Count);
        }
    }
}
