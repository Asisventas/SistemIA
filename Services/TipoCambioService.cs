using SistemIA.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using HtmlAgilityPack;

namespace SistemIA.Services
{
    public interface ITipoCambioService
    {
        Task<decimal> ObtenerTipoCambioAsync(string monedaOrigen, string monedaDestino);
        Task ActualizarTiposCambioAsync();
        Task<List<TipoCambio>> ObtenerTiposCambioActualesAsync();
    }

    public class TipoCambioService : ITipoCambioService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TipoCambioService> _logger;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        // APIs gratuitas para tipos de cambio
    private const string API_EXCHANGERATE = "https://api.exchangerate-api.com/v4/latest/USD";
    private const string API_FIXER = "https://api.fixer.io/latest?access_key=YOUR_API_KEY&base=USD";
    // Fuente local preferida: Cambios Chaco (Casa Central - Asunción). Documentación pública no oficial.
    // Nota: Si cambia el endpoint, parametrizar en appsettings.
        private static readonly string[] API_CAMBIOS_CHACO_ENDPOINTS = new[]
        {
            // Endpoint sugerido por el usuario (HTTP)
            "http://www.cambioschaco.com.py/api/branch_office/1/exchange",
            // Variante HTTPS nueva (puede fallar por DNS en algunos entornos)
            "https://api.cambioschaco.com.py/api/branch_office/1/exchange"
        };
        
        public TipoCambioService(HttpClient httpClient, ILogger<TipoCambioService> logger, IDbContextFactory<AppDbContext> dbFactory)
        {
            _httpClient = httpClient;
            _logger = logger;
            _dbFactory = dbFactory;
        }

        public async Task<decimal> ObtenerTipoCambioAsync(string monedaOrigen, string monedaDestino)
        {
            try
            {
                // Primero intentar obtener de la base de datos (más rápido)
                await using var context = await _dbFactory.CreateDbContextAsync();
                var tipoCambioDb = await context.TiposCambio
                    .Include(tc => tc.MonedaOrigen)
                    .Include(tc => tc.MonedaDestino)
                    .Where(tc => tc.MonedaOrigen.CodigoISO == monedaOrigen && 
                                 tc.MonedaDestino.CodigoISO == monedaDestino &&
                                 tc.Estado && 
                                 tc.FechaTipoCambio.Date == DateTime.Today)
                    .FirstOrDefaultAsync();

                if (tipoCambioDb != null)
                {
                    _logger.LogInformation($"Tipo de cambio obtenido de BD: {monedaOrigen} -> {monedaDestino} = {tipoCambioDb.TasaCambio}");
                    return tipoCambioDb.TasaCambio;
                }

                // Si no existe en BD o no es actual, obtener de API
                return await ObtenerTipoCambioDesdeAPIAsync(monedaOrigen, monedaDestino);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener tipo de cambio {monedaOrigen} -> {monedaDestino}");
                return 0;
            }
        }

        private async Task<decimal> ObtenerTipoCambioDesdeAPIAsync(string monedaOrigen, string monedaDestino)
        {
            try
            {
                // 1) Intentar primero fuente local (PY): Cambios Chaco
                try
                {
                    var okLocal = await ActualizarDesdeCambiosChaco();
                    if (okLocal)
                    {
                        // Buscar el par actualizado en la BD
                        await using var ctx = await _dbFactory.CreateDbContextAsync();
                        var registro = await ctx.TiposCambio
                            .Include(t => t.MonedaOrigen)
                            .Include(t => t.MonedaDestino)
                            .Where(t => t.MonedaOrigen.CodigoISO == monedaOrigen &&
                                        t.MonedaDestino.CodigoISO == monedaDestino &&
                                        t.FechaTipoCambio.Date == DateTime.Today)
                            .OrderByDescending(t => t.FechaModificacion ?? t.FechaCreacion)
                            .FirstOrDefaultAsync();

                        if (registro != null)
                        {
                            _logger.LogInformation($"Tipo de cambio (Chaco) {monedaOrigen}->{monedaDestino}: {registro.TasaCambio}");
                            return registro.TasaCambio;
                        }
                    }
                }
                catch (Exception exLocal)
                {
                    _logger.LogWarning(exLocal, "Fallo Cambios Chaco; se usará fallback global");
                }

                // 2) Fallback global: ExchangeRate-API (basada en USD)
                var response = await _httpClient.GetStringAsync(API_EXCHANGERATE);
                var data = JsonSerializer.Deserialize<ExchangeRateResponse>(response);

                if (data?.rates != null)
                {
                    decimal tasaOrigen = monedaOrigen == "USD" ? 1 : GetRate(data.rates, monedaOrigen);
                    decimal tasaDestino = monedaDestino == "USD" ? 1 : GetRate(data.rates, monedaDestino);

                    if (tasaOrigen > 0 && tasaDestino > 0)
                    {
                        decimal tipoCambio = tasaDestino / tasaOrigen;
                        await GuardarTipoCambioAsync(monedaOrigen, monedaDestino, tipoCambio, "ExchangeRate-API");
                        _logger.LogInformation($"Tipo de cambio obtenido de API: {monedaOrigen} -> {monedaDestino} = {tipoCambio}");
                        return tipoCambio;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener tipo de cambio desde API: {monedaOrigen} -> {monedaDestino}");
                return 0;
            }
        }

        private decimal GetRate(Dictionary<string, decimal> rates, string currency)
        {
            return rates.TryGetValue(currency, out decimal rate) ? rate : 0;
        }

        private async Task GuardarTipoCambioAsync(string monedaOrigen, string monedaDestino, decimal tasa, string fuente, decimal? tasaCompra = null)
        {
            try
            {
                await using var context = await _dbFactory.CreateDbContextAsync();
                
                var monedaOrigenObj = await context.Monedas.FirstOrDefaultAsync(m => m.CodigoISO == monedaOrigen);
                var monedaDestinoObj = await context.Monedas.FirstOrDefaultAsync(m => m.CodigoISO == monedaDestino);

                if (monedaOrigenObj != null && monedaDestinoObj != null)
                {
                    // Verificar si ya existe un tipo de cambio para hoy
                    var tipoCambioExistente = await context.TiposCambio
                        .FirstOrDefaultAsync(tc => tc.IdMonedaOrigen == monedaOrigenObj.IdMoneda &&
                                                  tc.IdMonedaDestino == monedaDestinoObj.IdMoneda &&
                                                  tc.FechaTipoCambio.Date == DateTime.Today);

                    if (tipoCambioExistente != null)
                    {
                        // Actualizar existente
                        tipoCambioExistente.TasaCambio = tasa;
                        tipoCambioExistente.TasaCompra = tasaCompra;
                        tipoCambioExistente.Fuente = fuente;
                        tipoCambioExistente.FechaModificacion = DateTime.Now;
                        tipoCambioExistente.UsuarioModificacion = "Sistema";
                    }
                    else
                    {
                        // Crear nuevo
                        var nuevoTipoCambio = new TipoCambio
                        {
                            IdMonedaOrigen = monedaOrigenObj.IdMoneda,
                            IdMonedaDestino = monedaDestinoObj.IdMoneda,
                            TasaCambio = tasa,
                            TasaCompra = tasaCompra,
                            FechaTipoCambio = DateTime.Now,
                            Fuente = fuente,
                            EsAutomatico = true,
                            Estado = true,
                            FechaCreacion = DateTime.Now,
                            UsuarioCreacion = "Sistema"
                        };
                        context.TiposCambio.Add(nuevoTipoCambio);
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation($"💾 Guardado: {monedaOrigen} → {monedaDestino} = {tasa:N4} (Venta: {tasaCompra:N4})");
                }
                else
                {
                    _logger.LogWarning($"⚠️ Monedas no encontradas en BD: {monedaOrigen} o {monedaDestino}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al guardar tipo de cambio: {monedaOrigen} → {monedaDestino}");
            }
        }

        public async Task ActualizarTiposCambioAsync()
        {
            try
            {
                _logger.LogInformation("🔄 Iniciando actualización de tipos de cambio desde APIs externas");

                var obtenidos = 0;

                // 1) Intentar Cambios Chaco (preferida en PY)
                var okChaco = await ActualizarDesdeCambiosChaco();
                if (okChaco)
                {
                    _logger.LogInformation("✅ Actualización principal desde Cambios Chaco completa");
                    obtenidos += 1; // bandera de éxito
                }
                else
                {
                    _logger.LogWarning("⚠️ No se pudo actualizar desde Cambios Chaco; intentando fallback global");
                    // Intentar otras fuentes PY para USD/PYG
                    var ventaUsd = await ObtenerUsdVentaDesdeFuentesAlternas();
                    if (ventaUsd > 0)
                    {
                        await GuardarTipoCambioAsync("USD", "PYG", ventaUsd, "Fuentes Alternas (PY)", ventaUsd);
                        obtenidos++;
                    }
                }

                // 2) Fallback: asegurar al menos USD/PYG vía ExchangeRate si no hay registro del día
                if (!await ExisteHoy("USD", "PYG"))
                {
                    var tasa = await ObtenerTipoCambioDesdeAPIAsync("USD", "PYG");
                    if (tasa > 0) obtenidos++;
                }

                _logger.LogInformation($"✅ Actualización completada. Fuentes aplicadas: {obtenidos}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en actualización de tipos de cambio");
                throw;
            }
        }

        // ---------- Integración Cambios Chaco ----------
        // Clases auxiliares (cuando el JSON coincide con esta forma)
        private class CambiosChacoResponse
        {
            public string? name { get; set; }
            public DateTime? last_update { get; set; }
            public List<ChacoCurrency> currencies { get; set; } = new();
            public List<ChacoCurrency> data { get; set; } = new();
        }

        private class ChacoCurrency
        {
            public string? name { get; set; }
            public string? iso { get; set; } // USD, BRL, ARS, EUR, etc
            public string? code { get; set; }
            public decimal? buy { get; set; } // Compra en PYG
            public decimal? sell { get; set; } // Venta en PYG
            // Variantes comunes
            public decimal? buying_price { get; set; }
            public decimal? selling_price { get; set; }
            public decimal? buy_price { get; set; }
            public decimal? sell_price { get; set; }
            public decimal? purchase { get; set; }
            public decimal? sale { get; set; }
            public decimal? compra { get; set; }
            public decimal? venta { get; set; }
        }

        private async Task<bool> ActualizarDesdeCambiosChaco()
        {
            try
            {
                _logger.LogInformation("🌐 Consultando API Cambios Chaco...");
                string? json = null;
                foreach (var ep in API_CAMBIOS_CHACO_ENDPOINTS)
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        json = await _httpClient.GetStringAsync(ep, cts.Token);
                        if (!string.IsNullOrWhiteSpace(json)) { _logger.LogInformation($"[Chaco] Endpoint OK: {ep}"); break; }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"[Chaco] Falla endpoint: {ep}");
                    }
                }
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogWarning("Cambios Chaco devolvió vacío (fallaron todos los endpoints)");
                    return false;
                }
                var guardados = 0;
                foreach (var reg in ParseCambiosChaco(json))
                {
                    var tasaBase = reg.compra > 0 ? reg.compra : (reg.venta ?? 0m);
                    if (tasaBase > 0)
                    {
                        await GuardarTipoCambioAsync(reg.iso, "PYG", tasaBase, "Cambios Chaco", reg.venta);
                        guardados++;
                        _logger.LogInformation($"💱 {reg.iso}/PYG: Compra {(reg.compra > 0 ? reg.compra.ToString("N1") : "-")} | Venta {reg.venta?.ToString("N1") ?? "-"}");
                    }
                }

                _logger.LogInformation($"Cambios Chaco guardados: {guardados}");
                return guardados > 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo al actualizar desde Cambios Chaco");
                return false;
            }
        }

        private IEnumerable<(string iso, decimal compra, decimal? venta)> ParseCambiosChaco(string json)
        {
            var resultados = new List<(string iso, decimal compra, decimal? venta)>();
            try
            {
                using var doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                IEnumerable<JsonElement> items = Enumerable.Empty<JsonElement>();
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("currencies", out var currencies) && currencies.ValueKind == JsonValueKind.Array)
                        items = currencies.EnumerateArray();
                    else if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                        items = data.EnumerateArray();
                    else if (root.TryGetProperty("exchange", out var exchange) && exchange.ValueKind == JsonValueKind.Array)
                        items = exchange.EnumerateArray();
                    else if (root.TryGetProperty("items", out var itemsArr) && itemsArr.ValueKind == JsonValueKind.Array)
                        items = itemsArr.EnumerateArray();
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    items = root.EnumerateArray();
                }

                if (!items.Any())
                {
                    _logger.LogWarning("[Chaco] No se encontraron elementos en el JSON");
                    return resultados;
                }

                var objetivos = new HashSet<string> { "USD", "BRL", "ARS", "EUR", "CLP", "UYU", "COP", "MXN" };

                foreach (var it in items)
                {
                    var iso = GetStringCaseInsensitive(it, "iso", "code", "currency", "isoCode", "iso_code", "currencyCode", "currency_code");
                    if (string.IsNullOrWhiteSpace(iso)) continue;
                    iso = iso.Trim().ToUpperInvariant();
                    if (iso.Length > 3)
                    {
                        // Derivar por nombre si llega como “Dólar Americano”
                        var name = GetStringCaseInsensitive(it, "name", "label");
                        if (!string.IsNullOrEmpty(name))
                        {
                            if (name.Contains("Dólar", StringComparison.OrdinalIgnoreCase)) iso = "USD";
                            else if (name.Contains("Real", StringComparison.OrdinalIgnoreCase)) iso = "BRL";
                            else if (name.Contains("Argentino", StringComparison.OrdinalIgnoreCase)) iso = "ARS";
                            else if (name.Contains("Euro", StringComparison.OrdinalIgnoreCase)) iso = "EUR";
                        }
                    }
                    if (!objetivos.Contains(iso)) continue;

                    var compra = GetDecimalCaseInsensitive(it, "buy", "buyPrice", "buying_price", "buy_price", "buyingPrice", "purchase", "purchasePrice", "compra", "bid");
                    var venta = GetDecimalCaseInsensitiveNullable(it, "sell", "salePrice", "selling_price", "sell_price", "sellingPrice", "sale", "venta", "ask");

                    if ((compra.HasValue && compra.Value > 0) || (venta.HasValue && venta.Value > 0))
                    {
                        resultados.Add((iso, compra ?? 0m, venta));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Chaco] Error parseando JSON");
            }
            return resultados;
        }

        private string GetStringCaseInsensitive(JsonElement obj, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (obj.TryGetProperty(k, out var prop) && prop.ValueKind == JsonValueKind.String)
                    return prop.GetString() ?? string.Empty;
                // intentar con otras mayúsculas/minúsculas
                foreach (var propK in obj.EnumerateObject())
                {
                    if (string.Equals(propK.Name, k, StringComparison.OrdinalIgnoreCase) && propK.Value.ValueKind == JsonValueKind.String)
                        return propK.Value.GetString() ?? string.Empty;
                }
            }
            return string.Empty;
        }

        private decimal? GetDecimalCaseInsensitiveNullable(JsonElement obj, params string[] keys)
        {
            var val = GetDecimalCaseInsensitive(obj, keys);
            return val;
        }

        private decimal? GetDecimalCaseInsensitive(JsonElement obj, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (obj.TryGetProperty(k, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDecimal(out var num))
                        return num;
                    if (prop.ValueKind == JsonValueKind.String)
                    {
                        var s = prop.GetString();
                        if (TryParseDecimalFlexible(s, out var numS)) return numS;
                    }
                }
                // Buscar ignorando mayúsculas
                foreach (var propK in obj.EnumerateObject())
                {
                    if (string.Equals(propK.Name, k, StringComparison.OrdinalIgnoreCase))
                    {
                        if (propK.Value.ValueKind == JsonValueKind.Number && propK.Value.TryGetDecimal(out var num2))
                            return num2;
                        if (propK.Value.ValueKind == JsonValueKind.String)
                        {
                            var s2 = propK.Value.GetString();
                            if (TryParseDecimalFlexible(s2, out var numS2)) return numS2;
                        }
                    }
                }
            }
            return null;
        }

        private bool TryParseDecimalFlexible(string? text, out decimal value)
        {
            value = 0m;
            if (string.IsNullOrWhiteSpace(text)) return false;
            // El backend podría enviar "7.220" como string con punto decimal o miles
            // Estrategia: quitar separadores de miles comunes y reemplazar coma por punto
            var s = text.Trim();
            // Si contiene espacios o símbolos, quítalos
            s = new string(s.Where(ch => char.IsDigit(ch) || ch == ',' || ch == '.' || ch == '-').ToArray());
            // Si hay ambos ',' y '.', asumir que '.' es miles y ',' decimal (formato es-ES)
            if (s.Contains(',') && s.Contains('.'))
            {
                // Quitar puntos (miles)
                s = s.Replace(".", string.Empty);
                // Reemplazar coma por punto (decimal)
                s = s.Replace(',', '.');
            }
            else if (s.Count(c => c == ',') == 1 && s.IndexOf(',') > s.Length - 4)
            {
                // Solo coma cercana al final → decimal
                s = s.Replace(',', '.');
            }
            else if (s.Contains('.') && !s.Contains(','))
            {
                // Solo puntos. Heurística: si el último grupo tiene exactamente 3 dígitos (p.ej. 7.220, 9.100)
                // o si hay más de un punto, trátalos como separadores de miles.
                var parts = s.Split('.', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    var lastLen = parts[^1].Length;
                    if (parts.Length > 2 || lastLen == 3)
                    {
                        s = string.Join(string.Empty, parts); // quitar todos los puntos
                    }
                }
            }
            // Intentar parsear invariante
            return decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
        }

        private async Task<bool> ExisteHoy(string monedaOrigen, string monedaDestino)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            return await context.TiposCambio
                .Include(t => t.MonedaOrigen)
                .Include(t => t.MonedaDestino)
                .AnyAsync(t => t.MonedaOrigen.CodigoISO == monedaOrigen &&
                              t.MonedaDestino.CodigoISO == monedaDestino &&
                              t.FechaTipoCambio.Date == DateTime.Today);
        }

        // ---------- Fuentes alternas locales ----------
        private async Task<decimal> ObtenerUsdVentaDesdeFuentesAlternas()
        {
            // Orden de preferencia: Alberdi, BCP, Eurocambio
            var chaco = await ObtenerCambioDesdeCambiosChacoUSDVentaSolo();
            if (chaco > 0) return chaco;
            var alberdi = await ObtenerCambioDesdeAlberdi();
            if (alberdi > 0) return alberdi;
            var bcp = await ObtenerCambioDesdeBCP();
            if (bcp > 0) return bcp;
            var euro = await ObtenerCambioDesdeEurocambio();
            if (euro > 0) return euro;
            return 0m;
        }

        // Solo USD venta desde Chaco (rápido)
        private async Task<decimal> ObtenerCambioDesdeCambiosChacoUSDVentaSolo()
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                string? json = null;
                foreach (var ep in API_CAMBIOS_CHACO_ENDPOINTS)
                {
                    try
                    {
                        json = await _httpClient.GetStringAsync(ep, cts.Token);
                        if (!string.IsNullOrWhiteSpace(json)) break;
                    }
                    catch { }
                }
                if (string.IsNullOrWhiteSpace(json)) return 0m;
                foreach (var r in ParseCambiosChaco(json))
                {
                    if (r.iso == "USD" && r.venta.HasValue)
                    {
                        var v = r.venta.Value;
                        // Normalizar: si parece miles abreviados (7.21), multiplicar por 1000
                        if (v > 0 && v < 100) v *= 1000m;
                        return v;
                    }
                }
            }
            catch { }
            return 0m;
        }

        private async Task<decimal> ObtenerCambioDesdeAlberdi()
        {
            try
            {
                var url = "https://www.cambiosalberdi.com/langes/index.php#sectionCotizacion";
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var html = await _httpClient.GetStringAsync(url, cts.Token);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var table = doc.DocumentNode.SelectSingleNode("//table[contains(@class,'table')]");
                if (table == null) return 0m;
                var rows = table.SelectNodes(".//tr");
                if (rows == null) return 0m;
                foreach (var row in rows)
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells != null && cells.Count >= 3 && cells[0].InnerText.Contains("Dólar Americano", StringComparison.OrdinalIgnoreCase))
                    {
                        var txt = cells[2].InnerText.Trim();
                        if (TryParseDecimalFlexible(txt, out var valor))
                        {
                            // Normalizar a PYG si viene abreviado (ej: 7.21)
                            if (valor > 0 && valor < 100) valor *= 1000m;
                            return valor;
                        }
                    }
                }
            }
            catch { }
            return 0m;
        }

        private async Task<decimal> ObtenerCambioDesdeBCP()
        {
            try
            {
                var url = "https://www.bcp.gov.py/webapps/web/cotizacion/monedas";
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var html = await _httpClient.GetStringAsync(url, cts.Token);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var ventaNode = doc.DocumentNode.SelectSingleNode("(//tr/td[4])[1]");
                if (ventaNode == null) return 0m;
                var txt = ventaNode.InnerText.Trim();
                if (TryParseDecimalFlexible(txt, out var valor))
                {
                    if (valor > 0 && valor < 100) valor *= 1000m;
                    return valor;
                }
                return 0m;
            }
            catch { }
            return 0m;
        }

        private async Task<decimal> ObtenerCambioDesdeEurocambio()
        {
            try
            {
                var url = "https://eurocambios.com.py/v2/sgi/utilsDto.php";
                var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "param", "getCotizacionesbySucursal" },
                    { "sucursal", "1" }
                });
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var resp = await _httpClient.PostAsync(url, form, cts.Token);
                resp.EnsureSuccessStatusCode();
                var body = await resp.Content.ReadAsStringAsync(cts.Token);
                using var docJson = JsonDocument.Parse(body);
                if (docJson.RootElement.ValueKind == JsonValueKind.Array && docJson.RootElement.GetArrayLength() > 0)
                {
                    var first = docJson.RootElement[0];
                    var venta = GetDecimalCaseInsensitive(first, "venta");
                    if (venta.HasValue)
                    {
                        var v = venta.Value;
                        if (v > 0 && v < 100) v *= 1000m;
                        return v;
                    }
                }
            }
            catch { }
            return 0m;
        }

        public async Task<List<TipoCambio>> ObtenerTiposCambioActualesAsync()
        {
            try
            {
                await using var context = await _dbFactory.CreateDbContextAsync();
                
                // Primero, vamos a verificar qué columnas existen en la tabla
                _logger.LogInformation("🔍 Verificando estructura de tabla TiposCambio...");
                
                // Usar una consulta SQL directa para ver la estructura
                var columnas = await context.Database.SqlQueryRaw<string>(
                    "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TiposCambio' ORDER BY ORDINAL_POSITION"
                ).ToListAsync();
                
                _logger.LogInformation($"📋 Columnas encontradas en TiposCambio: {string.Join(", ", columnas)}");
                
                // Consultar registros del día
                var hoy = DateTime.Today;
                var lista = await context.TiposCambio
                    .Include(tc => tc.MonedaOrigen)
                    .Include(tc => tc.MonedaDestino)
                    .Where(tc => tc.Estado && tc.FechaTipoCambio.Date == hoy)
                    .OrderBy(tc => tc.MonedaOrigen.CodigoISO)
                    .ThenBy(tc => tc.MonedaDestino.CodigoISO)
                    .ToListAsync();

                // Evaluar si se requiere actualización
                bool requiereActualizacion = false;
                var principales = new HashSet<string> { "USD", "BRL", "ARS", "EUR" };
                if (lista == null || !lista.Any())
                {
                    requiereActualizacion = true;
                }
                else
                {
                    var destinoPyg = lista.Where(x => x.MonedaDestino?.CodigoISO == "PYG").ToList();
                    var presentes = destinoPyg.Select(x => x.MonedaOrigen?.CodigoISO ?? "").ToHashSet();
                    if (!principales.IsSubsetOf(presentes)) requiereActualizacion = true;

                    // Falta precio de venta o fuente distinta
                    if (destinoPyg.Any(x => !x.TasaCompra.HasValue || string.IsNullOrWhiteSpace(x.Fuente) || x.Fuente != "Cambios Chaco"))
                        requiereActualizacion = true;

                    // Desactualizado por hora (más de 4 horas)
                    var ultima = destinoPyg.Max(x => x.FechaModificacion ?? x.FechaCreacion);
                    if ((DateTime.Now - ultima) > TimeSpan.FromHours(4)) requiereActualizacion = true;
                }

                // Si hace falta, intentar actualizar automáticamente desde Cambios Chaco
                if (requiereActualizacion)
                {
                    _logger.LogInformation("⏱️ No hay tipos de cambio de hoy. Actualizando desde Cambios Chaco...");
                    var ok = await ActualizarDesdeCambiosChaco();
                    if (ok)
                    {
                        lista = await context.TiposCambio
                            .Include(tc => tc.MonedaOrigen)
                            .Include(tc => tc.MonedaDestino)
                            .Where(tc => tc.Estado && tc.FechaTipoCambio.Date == hoy)
                            .OrderBy(tc => tc.MonedaOrigen.CodigoISO)
                            .ThenBy(tc => tc.MonedaDestino.CodigoISO)
                            .ToListAsync();
                    }
                }

                return lista ?? new List<TipoCambio>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener tipos de cambio actuales");
                return new List<TipoCambio>();
            }
        }
    }

    // Modelo para deserializar la respuesta de la API
    public class ExchangeRateResponse
    {
        public string @base { get; set; } = string.Empty;
        public string date { get; set; } = string.Empty;
        public Dictionary<string, decimal> rates { get; set; } = new();
    }
}
