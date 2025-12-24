using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Interfaz para el servicio de historial de tipos de cambio
    /// </summary>
    public interface ITipoCambioHistoricoService
    {
        /// <summary>
        /// Guarda el historial de tipos de cambio actual
        /// </summary>
        Task GuardarHistorialAsync();

        /// <summary>
        /// Obtiene el historial de un tipo de cambio específico en un rango de fechas
        /// </summary>
        Task<List<TipoCambioHistorico>> ObtenerHistorialAsync(string monedaOrigen, string monedaDestino, DateTime fechaDesde, DateTime fechaHasta);

        /// <summary>
        /// Obtiene el historial de todos los tipos de cambio del último mes
        /// </summary>
        Task<List<TipoCambioHistorico>> ObtenerHistorialUltimoMesAsync();

        /// <summary>
        /// Obtiene datos para gráfico de evolución de monedas
        /// </summary>
        Task<Dictionary<string, List<object>>> ObtenerDatosGraficoAsync(int diasAtras = 30);

        /// <summary>
        /// Calcula la variación porcentual de un tipo de cambio en un período
        /// </summary>
        Task<decimal> CalcularVariacionAsync(string monedaOrigen, string monedaDestino, int diasAtras = 7);
    }
}
