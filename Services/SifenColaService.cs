using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SistemIA.Data;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio de cola para reintentar envíos SIFEN pendientes cuando hay conexión a internet.
    /// Procesa automáticamente Ventas y Notas de Crédito que quedaron pendientes por falta de conectividad.
    /// 
    /// ⚠️ IMPORTANTE: Este servicio SOLO procesa documentos de FACTURACIÓN ELECTRÓNICA.
    /// Las facturas de autoimpresor NO se procesan a través de este servicio.
    /// La diferenciación se hace mediante el campo Caja.TipoFacturacion.
    /// </summary>
    public class SifenColaService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SifenColaService> _logger;
        
        /// <summary>
        /// Valor que indica facturación electrónica SIFEN.
        /// Se usa Contains() para compatibilidad con "ELECTRONICA" y "Electrónica".
        /// </summary>
        private const string TIPO_FACTURACION_ELECTRONICA = "ELECTR";
        
        // Configuración del servicio (valores por defecto, se sobreescriben desde BD)
        private TimeSpan _intervaloVerificacion = TimeSpan.FromMinutes(2);
        private TimeSpan _intervaloSinConexion = TimeSpan.FromSeconds(30);
        private int _maxReintentos = 3;
        private int _maxDocumentosPorCiclo = 10;
        private bool _colaActiva = true;
        
        private readonly string[] _hostsPrueba = { "sifen.set.gov.py", "sifen-test.set.gov.py", "google.com" };
        
        private bool _ultimaConexionExitosa = false;
        private DateTime _ultimaVerificacion = DateTime.MinValue;
        private DateTime _ultimaCargaConfig = DateTime.MinValue;

        public SifenColaService(IServiceProvider serviceProvider, ILogger<SifenColaService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        
        /// <summary>
        /// Carga la configuración desde la base de datos
        /// </summary>
        private async Task CargarConfiguracionAsync()
        {
            try
            {
                // Solo recargar cada 5 minutos para no sobrecargar
                if ((DateTime.Now - _ultimaCargaConfig).TotalMinutes < 5)
                    return;
                    
                using var scope = _serviceProvider.CreateScope();
                var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
                await using var db = await dbFactory.CreateDbContextAsync();
                
                var config = await db.ConfiguracionSistema.FirstOrDefaultAsync();
                if (config != null)
                {
                    _intervaloVerificacion = TimeSpan.FromMinutes(Math.Max(1, config.SifenIntervaloMinutos));
                    _maxReintentos = Math.Max(1, config.SifenMaxReintentos);
                    _maxDocumentosPorCiclo = Math.Max(1, config.SifenMaxDocumentosPorCiclo);
                    
                    // La cola se activa si:
                    // 1. Está explícitamente activada en configuración, O
                    // 2. Hay al menos una caja con facturación electrónica (activación automática)
                    _colaActiva = config.SifenColaActiva || await TieneCajasFacturacionElectronicaAsync(db);
                    
                    _logger.LogDebug("[SIFEN Cola] Configuración cargada: Intervalo={Intervalo}min, MaxDocs={MaxDocs}, MaxReintentos={MaxReintentos}, Activa={Activa}",
                        config.SifenIntervaloMinutos, _maxDocumentosPorCiclo, _maxReintentos, _colaActiva);
                }
                else
                {
                    // Si no hay configuración, verificar si hay cajas con facturación electrónica
                    _colaActiva = await TieneCajasFacturacionElectronicaAsync(db);
                }
                
                _ultimaCargaConfig = DateTime.Now;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[SIFEN Cola] No se pudo cargar configuración, usando valores por defecto");
            }
        }
        
        /// <summary>
        /// Verifica si existe al menos una caja configurada con facturación electrónica.
        /// Solo las cajas con TipoFacturacion que contiene "ELECTR" son consideradas electrónicas.
        /// Las cajas con TipoFacturacion = "AUTOIMPRESOR" o null NO activan el servicio.
        /// </summary>
        private async Task<bool> TieneCajasFacturacionElectronicaAsync(AppDbContext db)
        {
            try
            {
                var hayCajaElectronica = await db.Cajas
                    .AnyAsync(c => c.TipoFacturacion != null && 
                                   c.TipoFacturacion.ToUpper().Contains(TIPO_FACTURACION_ELECTRONICA));
                
                if (hayCajaElectronica)
                {
                    _logger.LogInformation("[SIFEN Cola] ✅ Servicio activado automáticamente - Se detectó caja con facturación electrónica");
                }
                else
                {
                    _logger.LogDebug("[SIFEN Cola] No hay cajas con facturación electrónica configuradas (solo autoimpresor o sin configurar)");
                }
                
                return hayCajaElectronica;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[SIFEN Cola] Error al verificar cajas con facturación electrónica");
                return false;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[SIFEN Cola] Servicio de cola SIFEN iniciado");
            
            // Esperar 30 segundos antes de comenzar (dar tiempo a que la app inicie completamente)
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Cargar configuración desde BD
                    await CargarConfiguracionAsync();
                    
                    // Si la cola está desactivada, solo esperar
                    if (!_colaActiva)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                        continue;
                    }
                    
                    var hayConexion = await VerificarConexionAsync();
                    
                    if (hayConexion)
                    {
                        // Si recuperamos conexión después de no tenerla, procesar cola
                        if (!_ultimaConexionExitosa)
                        {
                            _logger.LogInformation("[SIFEN Cola] ✅ Conexión a internet restaurada. Procesando documentos pendientes...");
                        }
                        
                        _ultimaConexionExitosa = true;
                        await ProcesarColaPendienteAsync(stoppingToken);
                        
                        // Esperar intervalo normal
                        await Task.Delay(_intervaloVerificacion, stoppingToken);
                    }
                    else
                    {
                        if (_ultimaConexionExitosa)
                        {
                            _logger.LogWarning("[SIFEN Cola] ⚠️ Sin conexión a internet. Los documentos pendientes se enviarán cuando se restaure.");
                        }
                        
                        _ultimaConexionExitosa = false;
                        
                        // Sin conexión: revisar más frecuentemente
                        await Task.Delay(_intervaloSinConexion, stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[SIFEN Cola] Error en el ciclo principal del servicio");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            
            _logger.LogInformation("[SIFEN Cola] Servicio de cola SIFEN detenido");
        }

        /// <summary>
        /// Verifica si hay conexión a internet probando múltiples hosts
        /// </summary>
        private async Task<bool> VerificarConexionAsync()
        {
            foreach (var host in _hostsPrueba)
            {
                try
                {
                    using var ping = new Ping();
                    var reply = await ping.SendPingAsync(host, 3000); // Timeout 3 segundos
                    if (reply.Status == IPStatus.Success)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Intentar siguiente host
                }
            }
            
            // Fallback: intentar conexión HTTP simple
            try
            {
                using var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await client.GetAsync("https://www.google.com/generate_204");
                return response.IsSuccessStatusCode || (int)response.StatusCode == 204;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Procesa todos los documentos pendientes de envío a SIFEN
        /// </summary>
        private async Task ProcesarColaPendienteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            
            // Procesar Ventas pendientes
            var ventasProcesadas = await ProcesarVentasPendientesAsync(scope.ServiceProvider, dbFactory, stoppingToken);
            
            // Procesar Notas de Crédito pendientes
            var ncProcesadas = await ProcesarNCPendientesAsync(scope.ServiceProvider, dbFactory, stoppingToken);
            
            if (ventasProcesadas > 0 || ncProcesadas > 0)
            {
                _logger.LogInformation("[SIFEN Cola] Procesamiento completado: {Ventas} ventas, {NC} notas de crédito", 
                    ventasProcesadas, ncProcesadas);
            }
        }

        /// <summary>
        /// Procesa ventas pendientes de envío a SIFEN.
        /// ⚠️ SOLO procesa ventas de cajas con TipoFacturacion = "ELECTRONICA".
        /// Las ventas de cajas autoimpresor son ignoradas.
        /// </summary>
        private async Task<int> ProcesarVentasPendientesAsync(
            IServiceProvider serviceProvider,
            IDbContextFactory<AppDbContext> dbFactory, 
            CancellationToken stoppingToken)
        {
            await using var db = await dbFactory.CreateDbContextAsync(stoppingToken);
            
            // Buscar ventas confirmadas que:
            // 1. Están en una CAJA con facturación ELECTRÓNICA (ignora autoimpresor)
            // 2. No tienen CDC (nunca se enviaron) O
            // 3. Tienen EstadoSifen = PENDIENTE o null O
            // 4. Tienen EstadoSifen = RECHAZADO con mensaje de error de conexión
            // NOTA: Usamos ToLower() en lugar de StringComparison para compatibilidad con EF Core
            // NOTA 2: Estado puede ser "Confirmada" o "Confirmado" (inconsistencia histórica)
            var ventasPendientes = await db.Ventas
                .Include(v => v.Sucursal)
                .Include(v => v.Cliente)
                .Include(v => v.Caja)
                .Where(v => (v.Estado == "Confirmada" || v.Estado == "Confirmado") &&
                            // ⚠️ FILTRO CRÍTICO: Solo facturas electrónicas, NO autoimpresor
                            v.Caja != null &&
                            v.Caja.TipoFacturacion != null &&
                            v.Caja.TipoFacturacion.ToUpper().Contains(TIPO_FACTURACION_ELECTRONICA) &&
                            (
                                string.IsNullOrEmpty(v.EstadoSifen) ||
                                v.EstadoSifen == "PENDIENTE" ||
                                (v.EstadoSifen == "RECHAZADO" && v.MensajeSifen != null && 
                                    (v.MensajeSifen.ToLower().Contains("conexión") ||
                                     v.MensajeSifen.ToLower().Contains("connection") ||
                                     v.MensajeSifen.ToLower().Contains("timeout") ||
                                     v.MensajeSifen.ToLower().Contains("network") ||
                                     v.MensajeSifen.ToLower().Contains("unable to connect")))
                            ))
                .OrderBy(v => v.Fecha)
                .Take(_maxDocumentosPorCiclo)
                .ToListAsync(stoppingToken);

            if (!ventasPendientes.Any())
                return 0;

            _logger.LogInformation("[SIFEN Cola] Encontradas {Count} ventas pendientes de envío", ventasPendientes.Count);

            var deSvc = serviceProvider.GetRequiredService<DEBuilderService>();
            var xmlBuilder = serviceProvider.GetRequiredService<DEXmlBuilder>();
            var sifen = serviceProvider.GetRequiredService<Sifen>();

            int procesadas = 0;
            foreach (var venta in ventasPendientes)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    var resultado = await EnviarVentaAsync(db, venta, deSvc, xmlBuilder, sifen);
                    if (resultado)
                    {
                        procesadas++;
                        _logger.LogInformation("[SIFEN Cola] ✅ Venta {Id} enviada exitosamente", venta.IdVenta);
                    }
                    else
                    {
                        _logger.LogWarning("[SIFEN Cola] ⚠️ Venta {Id} no pudo enviarse: {Mensaje}", 
                            venta.IdVenta, venta.MensajeSifen);
                    }
                    
                    // Pequeña pausa entre envíos para no saturar SIFEN
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[SIFEN Cola] Error al procesar venta {Id}", venta.IdVenta);
                    
                    // Si es error de conexión, parar el ciclo (no hay internet)
                    if (EsErrorConexion(ex))
                    {
                        _logger.LogWarning("[SIFEN Cola] Error de conexión detectado. Deteniendo procesamiento.");
                        break;
                    }
                }
            }

            return procesadas;
        }

        /// <summary>
        /// Procesa notas de crédito pendientes de envío a SIFEN.
        /// ⚠️ SOLO procesa NC de cajas con TipoFacturacion = "ELECTRONICA".
        /// Las NC de cajas autoimpresor son ignoradas.
        /// </summary>
        private async Task<int> ProcesarNCPendientesAsync(
            IServiceProvider serviceProvider,
            IDbContextFactory<AppDbContext> dbFactory,
            CancellationToken stoppingToken)
        {
            await using var db = await dbFactory.CreateDbContextAsync(stoppingToken);
            
            // Buscar NC confirmadas pendientes de envío:
            // 1. Están en una CAJA con facturación ELECTRÓNICA (ignora autoimpresor)
            // 2. La venta original tiene CDC (fue procesada por SIFEN)
            // 3. Tienen EstadoSifen pendiente o error de conexión
            // NOTA: Usamos ToLower() en lugar de StringComparison para compatibilidad con EF Core
            // NOTA 2: Estado puede ser "Confirmada" o "Confirmado" (inconsistencia histórica)
            var ncPendientes = await db.NotasCreditoVentas
                .Include(nc => nc.Sucursal)
                .Include(nc => nc.Cliente)
                .Include(nc => nc.Caja)
                .Include(nc => nc.VentaAsociada)
                .Where(nc => (nc.Estado == "Confirmada" || nc.Estado == "Confirmado") &&
                             // ⚠️ FILTRO CRÍTICO: Solo NC electrónicas, NO autoimpresor
                             nc.Caja != null &&
                             nc.Caja.TipoFacturacion != null &&
                             nc.Caja.TipoFacturacion.ToUpper().Contains(TIPO_FACTURACION_ELECTRONICA) &&
                             nc.VentaAsociada != null &&
                             !string.IsNullOrEmpty(nc.VentaAsociada.CDC) && // La venta original debe tener CDC
                             (
                                 string.IsNullOrEmpty(nc.EstadoSifen) ||
                                 nc.EstadoSifen == "PENDIENTE" ||
                                 (nc.EstadoSifen == "RECHAZADO" && nc.MensajeSifen != null &&
                                     (nc.MensajeSifen.ToLower().Contains("conexión") ||
                                      nc.MensajeSifen.ToLower().Contains("connection") ||
                                      nc.MensajeSifen.ToLower().Contains("timeout") ||
                                      nc.MensajeSifen.ToLower().Contains("network") ||
                                      nc.MensajeSifen.ToLower().Contains("unable to connect")))
                             ))
                .OrderBy(nc => nc.Fecha)
                .Take(_maxDocumentosPorCiclo)
                .ToListAsync(stoppingToken);

            if (!ncPendientes.Any())
                return 0;

            _logger.LogInformation("[SIFEN Cola] Encontradas {Count} NC pendientes de envío", ncPendientes.Count);

            int procesadas = 0;
            foreach (var nc in ncPendientes)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    var resultado = await EnviarNCAsync(db, nc, serviceProvider);
                    if (resultado)
                    {
                        procesadas++;
                        _logger.LogInformation("[SIFEN Cola] ✅ NC {Id} enviada exitosamente", nc.IdNotaCredito);
                    }
                    else
                    {
                        _logger.LogWarning("[SIFEN Cola] ⚠️ NC {Id} no pudo enviarse: {Mensaje}",
                            nc.IdNotaCredito, nc.MensajeSifen);
                    }
                    
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[SIFEN Cola] Error al procesar NC {Id}", nc.IdNotaCredito);
                    
                    if (EsErrorConexion(ex))
                    {
                        _logger.LogWarning("[SIFEN Cola] Error de conexión detectado. Deteniendo procesamiento.");
                        break;
                    }
                }
            }

            return procesadas;
        }

        /// <summary>
        /// Envía una venta individual a SIFEN
        /// </summary>
        private async Task<bool> EnviarVentaAsync(
            AppDbContext db,
            Venta venta,
            DEBuilderService deSvc,
            DEXmlBuilder xmlBuilder,
            Sifen sifen)
        {
            // Validar datos mínimos
            var valid = await deSvc.ValidarVentaAsync(venta.IdVenta);
            if (!valid.Ok)
            {
                venta.EstadoSifen = "RECHAZADO";
                venta.MensajeSifen = $"Validación fallida: {string.Join(", ", valid.Errores)}";
                await db.SaveChangesAsync();
                return false;
            }

            var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
            if (sociedad == null)
            {
                venta.EstadoSifen = "RECHAZADO";
                venta.MensajeSifen = "No existe registro de Sociedad configurado";
                await db.SaveChangesAsync();
                return false;
            }

            // Resolver configuración
            var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
            var urlEnvio = sociedad.DeUrlEnvioDocumentoLote ?? SistemIA.Utils.SifenConfig.GetEnvioLoteUrl(ambiente);
            var urlQrBase = sociedad.DeUrlQr ?? (SistemIA.Utils.SifenConfig.GetBaseUrl(ambiente) + 
                (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));

            var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
            var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;

            if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            {
                venta.EstadoSifen = "RECHAZADO";
                venta.MensajeSifen = "Falta configurar certificado .p12";
                await db.SaveChangesAsync();
                return false;
            }

            // Construir y enviar
            var xmlString = await xmlBuilder.ConstruirXmlAsync(venta.IdVenta);
            var resultJson = await sifen.FirmarYEnviar(urlEnvio, urlQrBase, xmlString, p12Path, p12Pass);

            // Procesar respuesta
            venta.FechaEnvioSifen = DateTime.Now;
            venta.MensajeSifen = resultJson;
            venta.XmlCDE = xmlString;

            // Determinar estado
            if (resultJson.Contains("\"codigo\":\"0302\"") || 
                resultJson.Contains("\"codigo\":\"0260\"") ||
                resultJson.ToLower().Contains("aceptado"))
            {
                venta.EstadoSifen = "ACEPTADO";
            }
            else if (resultJson.Contains("\"codigo\":\"0160\"") || 
                     resultJson.ToLower().Contains("xml mal formado"))
            {
                venta.EstadoSifen = "RECHAZADO";
            }
            else if (resultJson.Contains("\"codigo\":\"0300\""))
            {
                venta.EstadoSifen = "ENVIADO"; // Lote recibido, pendiente procesamiento
            }
            else
            {
                venta.EstadoSifen = "ENVIADO";
            }

            // Extraer CDC e IdLote
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(resultJson);
                
                if (doc.RootElement.TryGetProperty("cdc", out var cdcProp) && 
                    cdcProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var cdcVal = cdcProp.GetString();
                    if (!string.IsNullOrWhiteSpace(cdcVal) && cdcVal != "Tag not found.")
                        venta.CDC = cdcVal;
                }

                // Extraer URL del QR
                if (doc.RootElement.TryGetProperty("documento", out var docProp) && 
                    docProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var soapEnviado = docProp.GetString() ?? string.Empty;
                    var qrMatch = System.Text.RegularExpressions.Regex.Match(soapEnviado, @"<dCarQR>([^<]+)</dCarQR>");
                    if (qrMatch.Success)
                    {
                        var urlQr = qrMatch.Groups[1].Value.Replace("&amp;", "&");
                        if (!string.IsNullOrWhiteSpace(urlQr))
                            venta.UrlQrSifen = urlQr;
                    }
                }

                if (doc.RootElement.TryGetProperty("idLote", out var idLoteProp) && 
                    idLoteProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var idLoteVal = idLoteProp.GetString();
                    if (!string.IsNullOrWhiteSpace(idLoteVal) && idLoteVal != "Tag not found.")
                        venta.IdLote = idLoteVal;
                }
            }
            catch { /* best effort */ }

            await db.SaveChangesAsync();
            
            return venta.EstadoSifen == "ACEPTADO" || venta.EstadoSifen == "ENVIADO";
        }

        /// <summary>
        /// Envía una nota de crédito individual a SIFEN
        /// </summary>
        private async Task<bool> EnviarNCAsync(
            AppDbContext db,
            NotaCreditoVenta nc,
            IServiceProvider serviceProvider)
        {
            var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
            if (sociedad == null)
            {
                nc.EstadoSifen = "RECHAZADO";
                nc.MensajeSifen = "No existe registro de Sociedad configurado";
                await db.SaveChangesAsync();
                return false;
            }

            // Resolver configuración
            var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
            var urlEnvio = sociedad.DeUrlEnvioDocumentoLote ?? SistemIA.Utils.SifenConfig.GetEnvioLoteUrl(ambiente);
            var urlQrBase = sociedad.DeUrlQr ?? (SistemIA.Utils.SifenConfig.GetBaseUrl(ambiente) +
                (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));

            var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
            var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;

            if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            {
                nc.EstadoSifen = "RECHAZADO";
                nc.MensajeSifen = "Falta configurar certificado .p12";
                await db.SaveChangesAsync();
                return false;
            }

            // Construir XML usando NCEXmlBuilder (método estático)
            var sifen = serviceProvider.GetRequiredService<Sifen>();
            
            // Para NC, usar el builder estático NCEXmlBuilder
            var xmlString = await NCEXmlBuilder.ConstruirXmlAsync(nc, db);

            // Firmar y enviar
            var resultJson = await sifen.FirmarYEnviar(urlEnvio, urlQrBase, xmlString, p12Path, p12Pass);

            // Procesar respuesta
            nc.FechaEnvioSifen = DateTime.Now;
            nc.MensajeSifen = resultJson;
            nc.XmlCDE = xmlString;

            // Determinar estado
            if (resultJson.Contains("\"codigo\":\"0302\"") ||
                resultJson.Contains("\"codigo\":\"0260\"") ||
                resultJson.ToLower().Contains("aceptado"))
            {
                nc.EstadoSifen = "ACEPTADO";
            }
            else if (resultJson.Contains("\"codigo\":\"0160\"") ||
                     resultJson.ToLower().Contains("xml mal formado"))
            {
                nc.EstadoSifen = "RECHAZADO";
            }
            else if (resultJson.Contains("\"codigo\":\"0300\""))
            {
                nc.EstadoSifen = "ENVIADO";
            }
            else
            {
                nc.EstadoSifen = "ENVIADO";
            }

            // Extraer CDC e IdLote
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(resultJson);

                if (doc.RootElement.TryGetProperty("cdc", out var cdcProp) &&
                    cdcProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var cdcVal = cdcProp.GetString();
                    if (!string.IsNullOrWhiteSpace(cdcVal) && cdcVal != "Tag not found.")
                        nc.CDC = cdcVal;
                }

                // Extraer URL del QR
                if (doc.RootElement.TryGetProperty("documento", out var docProp) &&
                    docProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var soapEnviado = docProp.GetString() ?? string.Empty;
                    var qrMatch = System.Text.RegularExpressions.Regex.Match(soapEnviado, @"<dCarQR>([^<]+)</dCarQR>");
                    if (qrMatch.Success)
                    {
                        var urlQr = qrMatch.Groups[1].Value.Replace("&amp;", "&");
                        if (!string.IsNullOrWhiteSpace(urlQr))
                            nc.UrlQrSifen = urlQr;
                    }
                }

                if (doc.RootElement.TryGetProperty("idLote", out var idLoteProp) &&
                    idLoteProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var idLoteVal = idLoteProp.GetString();
                    if (!string.IsNullOrWhiteSpace(idLoteVal) && idLoteVal != "Tag not found.")
                        nc.IdLote = idLoteVal;
                }
            }
            catch { /* best effort */ }

            await db.SaveChangesAsync();

            return nc.EstadoSifen == "ACEPTADO" || nc.EstadoSifen == "ENVIADO";
        }

        /// <summary>
        /// Determina si una excepción es por error de conexión
        /// </summary>
        private bool EsErrorConexion(Exception ex)
        {
            var mensaje = ex.Message.ToLower();
            return mensaje.Contains("connection") ||
                   mensaje.Contains("timeout") ||
                   mensaje.Contains("network") ||
                   mensaje.Contains("unable to connect") ||
                   mensaje.Contains("no route") ||
                   mensaje.Contains("unreachable") ||
                   ex is System.Net.Http.HttpRequestException ||
                   ex is System.Net.Sockets.SocketException ||
                   ex is TaskCanceledException;
        }
    }
}
