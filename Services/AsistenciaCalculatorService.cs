using System;
using System.Linq;
using SistemIA.Models;
using Microsoft.EntityFrameworkCore;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para calcular y gestionar el control de asistencia con tiempos precisos
    /// </summary>
    public class AsistenciaCalculatorService
    {
        private readonly AppDbContext _context;

        public AsistenciaCalculatorService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calcula automáticamente todos los campos de control de tiempo para un registro de asistencia
        /// </summary>
        public async Task<Asistencia> CalcularControlTiempo(Asistencia asistencia)
        {
            // Obtener el horario del usuario para la fecha específica
            var horario = await ObtenerHorarioUsuario(asistencia.Id_Usuario, asistencia.FechaHora.Date);
            
            if (horario == null)
            {
                // No hay horario definido - marcar como día no laborable
                asistencia.EstadoPuntualidad = EstadoPuntualidad.NoLaborable.ToString();
                asistencia.EsDiaLaborable = false;
                return asistencia;
            }

            asistencia.Id_HorarioAplicado = horario.Id_Horario;
            asistencia.EsDiaLaborable = true;
            asistencia.DiaSemana = (int)asistencia.FechaHora.DayOfWeek == 0 ? 7 : (int)asistencia.FechaHora.DayOfWeek;

            // Calcular hora programada según el tipo de registro
            DateTime horaProgramada = CalcularHoraProgramada(asistencia, horario);
            asistencia.HoraProgramada = horaProgramada;

            // Calcular diferencia en minutos
            var diferenciaTotalMinutos = (int)(asistencia.FechaHora - horaProgramada).TotalMinutes;
            asistencia.DiferenciaMinutos = diferenciaTotalMinutos;

            // Obtener tolerancia de la empresa (configurable)
            int toleranciaMinutos = await ObtenerToleranciaEmpresa(asistencia.Sucursal);
            asistencia.MinutosToleranciAplicada = toleranciaMinutos;

            // Determinar estado de puntualidad
            asistencia.EstadoPuntualidad = DeterminarEstadoPuntualidad(
                asistencia.TipoRegistro, 
                diferenciaTotalMinutos, 
                toleranciaMinutos
            ).ToString();

            // Marcar si requiere justificación
            asistencia.RequiereJustificacion = RequiereJustificacion(diferenciaTotalMinutos, toleranciaMinutos, asistencia.TipoRegistro);
            
            if (asistencia.RequiereJustificacion)
            {
                asistencia.EstadoJustificacion = EstadoJustificacion.Pendiente.ToString();
            }

            // Calcular tiempo trabajado y horas extra si es salida
            if (asistencia.TipoRegistro.ToUpper() == "SALIDA")
            {
                await CalcularTiempoTrabajado(asistencia);
            }

            // Información adicional
            asistencia.ObservacionesSistema = GenerarObservacionesSistema(asistencia);

            return asistencia;
        }

        private async Task<HorarioTrabajo?> ObtenerHorarioUsuario(int idUsuario, DateTime fecha)
        {
            var diaSemana = fecha.DayOfWeek;
            
            // Buscar horario asignado al usuario a través de AsignacionesHorarios
            var asignacion = await _context.AsignacionesHorarios
                .Where(ah => ah.Id_Usuario == idUsuario)
                .Where(ah => ah.Estado) // Solo asignaciones activas
                .Where(ah => ah.FechaInicio <= fecha && (ah.FechaFin == null || ah.FechaFin >= fecha)) // Asignaciones vigentes
                .Include(ah => ah.HorarioTrabajo)
                .FirstOrDefaultAsync();

            if (asignacion?.HorarioTrabajo == null)
                return null;

            var horario = asignacion.HorarioTrabajo;

            // Verificar si el horario está activo y es día laboral
            if (!horario.EsActivo)
                return null;

            // Verificar si es día laboral según el horario
            var esDiaLaboral = diaSemana switch
            {
                DayOfWeek.Monday => horario.Lunes,
                DayOfWeek.Tuesday => horario.Martes,
                DayOfWeek.Wednesday => horario.Miercoles,
                DayOfWeek.Thursday => horario.Jueves,
                DayOfWeek.Friday => horario.Viernes,
                DayOfWeek.Saturday => horario.Sabado,
                DayOfWeek.Sunday => horario.Domingo,
                _ => false
            };

            if (!esDiaLaboral)
                return null;

            return horario;
        }

        private DateTime CalcularHoraProgramada(Asistencia asistencia, HorarioTrabajo horario)
        {
            var fechaBase = asistencia.FechaHora.Date;
            
            return asistencia.TipoRegistro.ToUpper() switch
            {
                "ENTRADA" => fechaBase.Add(horario.HoraEntrada),
                "SALIDA" => fechaBase.Add(horario.HoraSalida),
                "INICIOBREAK" => fechaBase.Add(horario.InicioBreak ?? TimeSpan.Zero),
                "FINBREAK" => fechaBase.Add(horario.FinBreak ?? TimeSpan.Zero),
                _ => fechaBase.Add(horario.HoraEntrada)
            };
        }

        private async Task<int> ObtenerToleranciaEmpresa(int sucursalId)
        {
            var sucursal = await _context.Sucursal.FindAsync(sucursalId);
            return sucursal?.ToleranciaEntradaMinutos ?? 10; // Por defecto 10 minutos
        }

        private EstadoPuntualidad DeterminarEstadoPuntualidad(string tipoRegistro, int diferenciaTotalMinutos, int tolerancia)
        {
            return tipoRegistro.ToUpper() switch
            {
                "ENTRADA" => diferenciaTotalMinutos switch
                {
                    > 0 when diferenciaTotalMinutos > tolerancia => EstadoPuntualidad.Tardanza,
                    < 0 when Math.Abs(diferenciaTotalMinutos) > 30 => EstadoPuntualidad.Adelanto,
                    _ => EstadoPuntualidad.Puntual
                },
                "SALIDA" => diferenciaTotalMinutos switch
                {
                    < 0 when Math.Abs(diferenciaTotalMinutos) > tolerancia => EstadoPuntualidad.SalidaTemprana,
                    > 0 when diferenciaTotalMinutos > 60 => EstadoPuntualidad.TiempoExtra,
                    _ => EstadoPuntualidad.Puntual
                },
                _ => EstadoPuntualidad.Puntual
            };
        }

        private bool RequiereJustificacion(int diferenciaTotalMinutos, int tolerancia, string tipoRegistro)
        {
            return tipoRegistro.ToUpper() switch
            {
                "ENTRADA" => diferenciaTotalMinutos > tolerancia,
                "SALIDA" => Math.Abs(diferenciaTotalMinutos) > tolerancia && diferenciaTotalMinutos < 0,
                _ => false
            };
        }

        private async Task CalcularTiempoTrabajado(Asistencia salidaAsistencia)
        {
            // Buscar la entrada correspondiente del mismo día
            var fechaInicio = salidaAsistencia.FechaHora.Date;
            var fechaFin = fechaInicio.AddDays(1);

            var entrada = await _context.Asistencias
                .Where(a => a.Id_Usuario == salidaAsistencia.Id_Usuario)
                .Where(a => a.TipoRegistro.ToUpper() == "ENTRADA")
                .Where(a => a.FechaHora >= fechaInicio && a.FechaHora < fechaFin)
                .OrderBy(a => a.FechaHora)
                .FirstOrDefaultAsync();

            if (entrada != null)
            {
                var tiempoTotal = (int)(salidaAsistencia.FechaHora - entrada.FechaHora).TotalMinutes;
                salidaAsistencia.TiempoTrabajadoMinutos = tiempoTotal;

                // Calcular horas extra (más de 8 horas = 480 minutos)
                var horasNormales = 480; // 8 horas
                if (tiempoTotal > horasNormales)
                {
                    salidaAsistencia.HorasExtraMinutos = tiempoTotal - horasNormales;
                }
            }
        }

        private string? GenerarObservacionesSistema(Asistencia asistencia)
        {
            var observaciones = new List<string>();

            if (asistencia.DiferenciaMinutos.HasValue)
            {
                var diferencia = asistencia.DiferenciaMinutos.Value;
                if (diferencia > 0)
                {
                    observaciones.Add($"Tardanza de {diferencia} minutos");
                }
                else if (diferencia < 0)
                {
                    observaciones.Add($"Adelanto de {Math.Abs(diferencia)} minutos");
                }
            }

            if (asistencia.HorasExtraMinutos.HasValue && asistencia.HorasExtraMinutos > 0)
            {
                var horasExtra = TimeSpan.FromMinutes(asistencia.HorasExtraMinutos.Value);
                observaciones.Add($"Horas extra: {horasExtra.Hours}h {horasExtra.Minutes}m");
            }

            if (asistencia.EsRegistroAutomatico)
            {
                observaciones.Add("Registro automático del sistema");
            }

            return observaciones.Count > 0 ? string.Join("; ", observaciones) : null;
        }

        /// <summary>
        /// Genera reporte de asistencia con todos los cálculos
        /// </summary>
        public async Task<AsistenciaReporte> GenerarReporte(int idUsuario, DateTime fechaInicio, DateTime fechaFin)
        {
            var asistencias = await _context.Asistencias
                .Where(a => a.Id_Usuario == idUsuario)
                .Where(a => a.FechaHora >= fechaInicio && a.FechaHora <= fechaFin)
                .Include(a => a.Usuario)
                .Include(a => a.HorarioAplicado)
                .OrderBy(a => a.FechaHora)
                .ToListAsync();

            return new AsistenciaReporte
            {
                IdUsuario = idUsuario,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                TotalAsistencias = asistencias.Count,
                TotalTardanzas = asistencias.Count(a => a.EstadoPuntualidad == EstadoPuntualidad.Tardanza.ToString()),
                TotalAusencias = asistencias.Count(a => a.EstadoPuntualidad == EstadoPuntualidad.Ausencia.ToString()),
                TotalHorasExtra = asistencias.Where(a => a.HorasExtraMinutos.HasValue).Sum(a => a.HorasExtraMinutos!.Value),
                TiempoTotalTrabajado = asistencias.Where(a => a.TiempoTrabajadoMinutos.HasValue).Sum(a => a.TiempoTrabajadoMinutos!.Value),
                Registros = asistencias
            };
        }
    }

    /// <summary>
    /// Modelo para reportes de asistencia
    /// </summary>
    public class AsistenciaReporte
    {
        public int IdUsuario { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalAsistencias { get; set; }
        public int TotalTardanzas { get; set; }
        public int TotalAusencias { get; set; }
        public int TotalHorasExtra { get; set; }
        public int TiempoTotalTrabajado { get; set; }
        public List<Asistencia> Registros { get; set; } = new();
        
        public TimeSpan TiempoTotalTrabajoTimeSpan => TimeSpan.FromMinutes(TiempoTotalTrabajado);
        public TimeSpan HorasExtraTimeSpan => TimeSpan.FromMinutes(TotalHorasExtra);
        public double PorcentajePuntualidad => TotalAsistencias > 0 ? ((double)(TotalAsistencias - TotalTardanzas - TotalAusencias) / TotalAsistencias) * 100 : 0;
    }
}
