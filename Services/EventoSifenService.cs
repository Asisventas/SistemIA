using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using SistemIA.Models;
using SistemIA.Utils;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para enviar eventos a SIFEN (Cancelación, Inutilización)
    /// 
    /// ESTRUCTURA XML EVENTO CANCELACIÓN (SEGÚN LOG POWER BUILDER QUE FUNCIONA):
    /// 
    /// &lt;soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope"&gt;
    ///   &lt;soap:Header/&gt;
    ///   &lt;soap:Body&gt;
    ///     &lt;rEnviEventoDe xmlns="http://ekuatia.set.gov.py/sifen/xsd"&gt;
    ///       &lt;dId&gt;{numero}&lt;/dId&gt;
    ///       &lt;dEvReg&gt;
    ///         &lt;gGroupGesEve xsi:schemaLocation="http://ekuatia.set.gov.py/sifen/xsd siRecepEvento_v150.xsd"
    ///                       xmlns="http://ekuatia.set.gov.py/sifen/xsd" 
    ///                       xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"&gt;
    ///           &lt;rGesEve&gt;
    ///             &lt;rEve Id="{numero}"&gt;
    ///               &lt;dFecFirma&gt;{fecha}&lt;/dFecFirma&gt;
    ///               &lt;dVerFor&gt;150&lt;/dVerFor&gt;
    ///               &lt;gGroupTiEvt&gt;
    ///                 &lt;rGeVeCan&gt;
    ///                   &lt;Id&gt;{CDC}&lt;/Id&gt;
    ///                   &lt;mOtEve&gt;{motivo}&lt;/mOtEve&gt;
    ///                 &lt;/rGeVeCan&gt;
    ///               &lt;/gGroupTiEvt&gt;
    ///             &lt;/rEve&gt;
    ///             &lt;Signature xmlns="http://www.w3.org/2000/09/xmldsig#"&gt;...&lt;/Signature&gt;
    ///           &lt;/rGesEve&gt;
    ///         &lt;/gGroupGesEve&gt;
    ///       &lt;/dEvReg&gt;
    ///     &lt;/rEnviEventoDe&gt;
    ///   &lt;/soap:Body&gt;
    /// &lt;/soap:Envelope&gt;
    /// 
    /// NOTAS IMPORTANTES:
    /// - dId y el atributo Id de rEve son un número simple (ej: 12529), NO el CDC
    /// - NO se usa dTiGDE, el tipo de evento se determina por el elemento rGeVeCan
    /// - La firma va DENTRO de rGesEve, DESPUÉS de /rEve
    /// - SOAP es 1.2 (http://www.w3.org/2003/05/soap-envelope)
    /// - Respuesta exitosa: dCodRes=0600 "Evento registrado correctamente"
    /// </summary>
    public interface IEventoSifenService
    {
        /// <summary>
        /// Envía un evento de cancelación a SIFEN para anular un DTE aprobado.
        /// Solo puede cancelarse dentro de las 48 horas posteriores a la aprobación.
        /// </summary>
        Task<EventoSifenResult> EnviarCancelacionAsync(int idVenta, string motivo);

        /// <summary>
        /// Verifica si una venta puede ser cancelada vía evento SIFEN.
        /// </summary>
        Task<(bool PuedeCancelar, string Mensaje)> VerificarPuedeCancelarAsync(int idVenta);
        
        /// <summary>
        /// Envía un evento de cancelación a SIFEN para anular una Nota de Crédito Electrónica aprobada.
        /// Solo puede cancelarse dentro de las 48 horas posteriores a la aprobación.
        /// </summary>
        Task<EventoSifenResult> EnviarCancelacionNCAsync(int idNotaCredito, string motivo);

        /// <summary>
        /// Verifica si una Nota de Crédito puede ser cancelada vía evento SIFEN.
        /// </summary>
        Task<(bool PuedeCancelar, string Mensaje)> VerificarPuedeCancelarNCAsync(int idNotaCredito);
    }

    public class EventoSifenResult
    {
        public bool Exito { get; set; }
        public string? Codigo { get; set; }
        public string? Mensaje { get; set; }
        public string? XmlEnviado { get; set; }
        public string? XmlRespuesta { get; set; }
    }

    public class EventoSifenService : IEventoSifenService
    {
        private readonly AppDbContext _context;

        private const string SIFEN_NS = "http://ekuatia.set.gov.py/sifen/xsd";
        private const string XSI_NS = "http://www.w3.org/2001/XMLSchema-instance";
        private const string XMLDSIG_NS = "http://www.w3.org/2000/09/xmldsig#";
        private const string SOAP12_NS = "http://www.w3.org/2003/05/soap-envelope";

        public EventoSifenService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea un HttpClient con certificado de cliente para autenticación mTLS con SIFEN
        /// </summary>
        private static HttpClient CrearHttpClientConCertificado(string certPath, string certPassword)
        {
            var certificate = new X509Certificate2(certPath, certPassword,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                SslProtocols = SslProtocols.Tls12
            };
            handler.ClientCertificates.Add(certificate);

            return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(120) };
        }

        public async Task<(bool PuedeCancelar, string Mensaje)> VerificarPuedeCancelarAsync(int idVenta)
        {
            var venta = await _context.Ventas
                .Include(v => v.Caja)
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta);

            if (venta == null)
                return (false, "Venta no encontrada");

            // Verificar que tiene CDC (fue enviado a SIFEN)
            if (string.IsNullOrEmpty(venta.CDC))
                return (false, "La venta no tiene CDC (no fue enviada a SIFEN)");

            // Verificar estado SIFEN
            var estadosSifenNoCancelables = new[] { "RECHAZADO", "CANCELADO", "ANULADO" };
            if (estadosSifenNoCancelables.Contains(venta.EstadoSifen?.ToUpper()))
                return (false, $"La venta ya está {venta.EstadoSifen}");

            // Verificar que está aprobado o en procesamiento
            var estadosValidos = new[] { "ACEPTADO", "APROBADO", "ENVIADO" };
            if (!estadosValidos.Contains(venta.EstadoSifen?.ToUpper()))
                return (false, $"Estado SIFEN '{venta.EstadoSifen}' no permite cancelación");

            // Verificar límite de 48 horas desde la aprobación
            if (venta.FechaEnvioSifen.HasValue)
            {
                var horasTranscurridas = (DateTime.Now - venta.FechaEnvioSifen.Value).TotalHours;
                if (horasTranscurridas > 48)
                    return (false, $"Han pasado más de 48 horas desde el envío a SIFEN ({horasTranscurridas:F1} horas). No se puede cancelar.");
            }

            return (true, "OK");
        }

        public async Task<EventoSifenResult> EnviarCancelacionAsync(int idVenta, string motivo)
        {
            var result = new EventoSifenResult();
            Console.WriteLine($"[EVENTO SIFEN] ========================================");
            Console.WriteLine($"[EVENTO SIFEN] Iniciando cancelación de venta {idVenta}");
            Console.WriteLine($"[EVENTO SIFEN] Motivo: {motivo}");

            try
            {
                // Validar motivo
                if (string.IsNullOrWhiteSpace(motivo) || motivo.Length < 5 || motivo.Length > 500)
                {
                    result.Mensaje = "El motivo debe tener entre 5 y 500 caracteres";
                    Console.WriteLine($"[EVENTO SIFEN] Error validación: {result.Mensaje}");
                    return result;
                }

                // Verificar si puede cancelar
                Console.WriteLine($"[EVENTO SIFEN] Verificando si puede cancelar...");
                var (puedeCancelar, mensajeVerif) = await VerificarPuedeCancelarAsync(idVenta);
                if (!puedeCancelar)
                {
                    result.Mensaje = mensajeVerif;
                    Console.WriteLine($"[EVENTO SIFEN] No puede cancelar: {result.Mensaje}");
                    return result;
                }
                Console.WriteLine($"[EVENTO SIFEN] Verificación OK");

                // Cargar venta con Caja
                Console.WriteLine($"[EVENTO SIFEN] Cargando venta...");
                var venta = await _context.Ventas
                    .Include(v => v.Caja)
                    .FirstOrDefaultAsync(v => v.IdVenta == idVenta);

                if (venta == null)
                {
                    result.Mensaje = "Venta no encontrada";
                    Console.WriteLine($"[EVENTO SIFEN] Error: {result.Mensaje}");
                    return result;
                }
                Console.WriteLine($"[EVENTO SIFEN] Venta cargada: CDC={venta.CDC}, Estado={venta.EstadoSifen}");

                // Cargar Sociedad
                Console.WriteLine($"[EVENTO SIFEN] Cargando sociedad...");
                var sociedad = await _context.Sociedades.FirstOrDefaultAsync();
                if (sociedad == null)
                {
                    result.Mensaje = "No se encontró la sociedad configurada";
                    Console.WriteLine($"[EVENTO SIFEN] Error: {result.Mensaje}");
                    return result;
                }

                // Obtener ambiente y certificado
                var ambiente = sociedad.ServidorSifen ?? "test";
                var certPath = sociedad.PathCertificadoP12;
                var certPassword = sociedad.PasswordCertificadoP12;
                Console.WriteLine($"[EVENTO SIFEN] Ambiente: {ambiente}");
                Console.WriteLine($"[EVENTO SIFEN] CertPath: {certPath}");

                if (string.IsNullOrEmpty(certPath) || !File.Exists(certPath))
                {
                    result.Mensaje = $"Certificado no encontrado: {certPath}";
                    Console.WriteLine($"[EVENTO SIFEN] Error: {result.Mensaje}");
                    return result;
                }

                // Generar ID único para el evento (número simple, como en PowerBuilder)
                var eventoId = GenerarIdEvento();
                Console.WriteLine($"[EVENTO SIFEN] EventoId generado: {eventoId}");

                // Construir XML interno del evento (sin SOAP, sin firma)
                Console.WriteLine($"[EVENTO SIFEN] Construyendo XML interno...");
                var xmlInterno = ConstruirXmlInternoEvento(eventoId, venta.CDC!, motivo);
                Console.WriteLine($"[EVENTO SIFEN] XML interno construido ({xmlInterno.Length} chars)");

                // Firmar el evento
                Console.WriteLine($"[EVENTO SIFEN] Firmando evento...");
                var xmlFirmado = FirmarEvento(xmlInterno, eventoId, certPath!, certPassword!);
                Console.WriteLine($"[EVENTO SIFEN] Evento firmado ({xmlFirmado.Length} chars)");

                // Construir SOAP completo
                Console.WriteLine($"[EVENTO SIFEN] Construyendo SOAP...");
                var soap = ConstruirSoapCompleto(xmlFirmado, eventoId);
                result.XmlEnviado = soap;
                Console.WriteLine($"[EVENTO SIFEN] SOAP construido ({soap.Length} chars)");

                // Guardar SOAP para debug
                try
                {
                    var debugDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Debug");
                    if (!Directory.Exists(debugDir)) Directory.CreateDirectory(debugDir);
                    var soapPath = Path.Combine(debugDir, "evento_sifen_soap_enviado.xml");
                    File.WriteAllText(soapPath, soap);
                    Console.WriteLine($"[EVENTO SIFEN] SOAP guardado en: {soapPath}");
                }
                catch { /* ignorar errores de logging */ }

                // Enviar a SIFEN
                var url = SifenConfig.GetEnvioEventoUrl(ambiente);
                Console.WriteLine($"[EVENTO SIFEN] Enviando a: {url}");

                using var httpClient = CrearHttpClientConCertificado(certPath!, certPassword!);
                
                // Content-Type correcto para SOAP 1.2
                var content = new StringContent(soap, Encoding.UTF8, "application/soap+xml");
                
                // Headers
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Java/1.8.0_341");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/soap+xml, application/xml, text/xml");
                httpClient.DefaultRequestHeaders.Add("Connection", "close");

                Console.WriteLine($"[EVENTO SIFEN] Enviando {soap.Length} bytes...");
                var response = await httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();
                result.XmlRespuesta = responseBody;

                Console.WriteLine($"[EVENTO SIFEN] Response Status: {response.StatusCode}");
                Console.WriteLine($"[EVENTO SIFEN] Response Length: {responseBody.Length}");
                Console.WriteLine($"[EVENTO SIFEN] Response Body:");
                Console.WriteLine(responseBody);

                // Guardar respuesta para debug
                try
                {
                    var debugDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Debug");
                    var logPath = Path.Combine(debugDir, "evento_sifen_response.txt");
                    var logContent = $"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                     $"Status: {response.StatusCode}\n" +
                                     $"CDC: {venta.CDC}\n" +
                                     $"EventoId: {eventoId}\n" +
                                     $"Motivo: {motivo}\n\n" +
                                     $"=== RESPUESTA ===\n{responseBody}\n\n" +
                                     $"=== SOAP ENVIADO ===\n{soap}";
                    File.WriteAllText(logPath, logContent);
                    Console.WriteLine($"[EVENTO SIFEN] Respuesta guardada en: {logPath}");
                }
                catch { /* ignorar errores de logging */ }

                // Parsear respuesta
                var (codigo, mensaje) = ParsearRespuestaEvento(responseBody);
                result.Codigo = codigo;
                result.Mensaje = mensaje ?? responseBody;

                Console.WriteLine($"[EVENTO SIFEN] Código respuesta: {codigo}");
                Console.WriteLine($"[EVENTO SIFEN] Mensaje respuesta: {mensaje}");

                // Código 0600 = "Evento registrado correctamente" (éxito)
                if (codigo == "0600")
                {
                    result.Exito = true;

                    // Actualizar SOLO estado SIFEN (el estado local "Anulada" lo maneja VentasExplorar)
                    venta.EstadoSifen = "CANCELADO";
                    venta.MensajeSifen = $"Cancelado vía evento SIFEN. Motivo: {motivo}";
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"[EVENTO SIFEN] ✅ ÉXITO - Venta {idVenta} cancelada correctamente");
                }
                else
                {
                    Console.WriteLine($"[EVENTO SIFEN] ❌ ERROR - Código: {codigo}, Mensaje: {mensaje}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EVENTO SIFEN] ❌ EXCEPCIÓN: {ex.Message}");
                Console.WriteLine($"[EVENTO SIFEN] Stack: {ex.StackTrace}");
                result.Mensaje = $"Error: {ex.Message}";
                return result;
            }
        }

        // ========== MÉTODOS PARA NOTAS DE CRÉDITO ==========

        public async Task<(bool PuedeCancelar, string Mensaje)> VerificarPuedeCancelarNCAsync(int idNotaCredito)
        {
            var nc = await _context.NotasCreditoVentas.FindAsync(idNotaCredito);
            
            if (nc == null)
                return (false, "Nota de crédito no encontrada");

            if (string.IsNullOrEmpty(nc.CDC))
                return (false, "La nota de crédito no tiene CDC (no fue enviada a SIFEN)");

            if (nc.EstadoSifen != "ACEPTADO" && nc.EstadoSifen != "APROBADO" && nc.EstadoSifen != "ENVIADO")
                return (false, $"La nota de crédito tiene estado '{nc.EstadoSifen}'. Solo se pueden cancelar NC con estado ACEPTADO/APROBADO");

            if (nc.EstadoSifen == "CANCELADO")
                return (false, "La nota de crédito ya fue cancelada");

            // Verificar tiempo (48 horas máximo)
            if (nc.FechaEnvioSifen.HasValue)
            {
                var horasTranscurridas = (DateTime.Now - nc.FechaEnvioSifen.Value).TotalHours;
                Console.WriteLine($"[EVENTO SIFEN NC] Horas desde envío: {horasTranscurridas:F1}");
                
                if (horasTranscurridas > 48)
                    return (false, $"Han pasado más de 48 horas desde el envío a SIFEN ({horasTranscurridas:F1} horas). No se puede cancelar.");
            }

            return (true, "OK");
        }

        public async Task<EventoSifenResult> EnviarCancelacionNCAsync(int idNotaCredito, string motivo)
        {
            var result = new EventoSifenResult();
            Console.WriteLine($"[EVENTO SIFEN NC] ========================================");
            Console.WriteLine($"[EVENTO SIFEN NC] Iniciando cancelación de NC {idNotaCredito}");
            Console.WriteLine($"[EVENTO SIFEN NC] Motivo: {motivo}");

            try
            {
                // Validar motivo
                if (string.IsNullOrWhiteSpace(motivo) || motivo.Length < 5 || motivo.Length > 500)
                {
                    result.Mensaje = "El motivo debe tener entre 5 y 500 caracteres";
                    Console.WriteLine($"[EVENTO SIFEN NC] Error validación: {result.Mensaje}");
                    return result;
                }

                // Verificar si puede cancelar
                Console.WriteLine($"[EVENTO SIFEN NC] Verificando si puede cancelar...");
                var (puedeCancelar, mensajeVerif) = await VerificarPuedeCancelarNCAsync(idNotaCredito);
                if (!puedeCancelar)
                {
                    result.Mensaje = mensajeVerif;
                    Console.WriteLine($"[EVENTO SIFEN NC] No puede cancelar: {result.Mensaje}");
                    return result;
                }
                Console.WriteLine($"[EVENTO SIFEN NC] Verificación OK");

                // Cargar NC con Caja
                Console.WriteLine($"[EVENTO SIFEN NC] Cargando nota de crédito...");
                var nc = await _context.NotasCreditoVentas
                    .Include(n => n.Caja)
                    .FirstOrDefaultAsync(n => n.IdNotaCredito == idNotaCredito);

                if (nc == null)
                {
                    result.Mensaje = "Nota de crédito no encontrada";
                    Console.WriteLine($"[EVENTO SIFEN NC] Error: {result.Mensaje}");
                    return result;
                }
                Console.WriteLine($"[EVENTO SIFEN NC] NC cargada: CDC={nc.CDC}, Estado={nc.EstadoSifen}");

                // Cargar Sociedad
                Console.WriteLine($"[EVENTO SIFEN NC] Cargando sociedad...");
                var sociedad = await _context.Sociedades.FirstOrDefaultAsync();
                if (sociedad == null)
                {
                    result.Mensaje = "No se encontró la sociedad configurada";
                    Console.WriteLine($"[EVENTO SIFEN NC] Error: {result.Mensaje}");
                    return result;
                }

                // Obtener ambiente y certificado
                var ambiente = sociedad.ServidorSifen ?? "test";
                var certPath = sociedad.PathCertificadoP12;
                var certPassword = sociedad.PasswordCertificadoP12;
                Console.WriteLine($"[EVENTO SIFEN NC] Ambiente: {ambiente}");
                Console.WriteLine($"[EVENTO SIFEN NC] CertPath: {certPath}");

                if (string.IsNullOrEmpty(certPath) || !File.Exists(certPath))
                {
                    result.Mensaje = $"Certificado no encontrado: {certPath}";
                    Console.WriteLine($"[EVENTO SIFEN NC] Error: {result.Mensaje}");
                    return result;
                }

                // Generar ID único para el evento
                var eventoId = GenerarIdEvento();
                Console.WriteLine($"[EVENTO SIFEN NC] EventoId generado: {eventoId}");

                // Construir XML interno del evento (sin SOAP, sin firma)
                Console.WriteLine($"[EVENTO SIFEN NC] Construyendo XML interno...");
                var xmlInterno = ConstruirXmlInternoEvento(eventoId, nc.CDC!, motivo);
                Console.WriteLine($"[EVENTO SIFEN NC] XML interno construido ({xmlInterno.Length} chars)");

                // Firmar el evento
                Console.WriteLine($"[EVENTO SIFEN NC] Firmando evento...");
                var xmlFirmado = FirmarEvento(xmlInterno, eventoId, certPath!, certPassword!);
                Console.WriteLine($"[EVENTO SIFEN NC] Evento firmado ({xmlFirmado.Length} chars)");

                // Construir SOAP completo
                Console.WriteLine($"[EVENTO SIFEN NC] Construyendo SOAP...");
                var soap = ConstruirSoapCompleto(xmlFirmado, eventoId);
                result.XmlEnviado = soap;

                Console.WriteLine($"[EVENTO SIFEN NC] SOAP construido ({soap.Length} chars)");

                // Obtener URL del endpoint de eventos
                var url = ambiente.ToLower() == "prod" || ambiente.ToLower() == "produccion"
                    ? "https://sifen.set.gov.py/de/ws/eventos/evento.wsdl"
                    : "https://sifen-test.set.gov.py/de/ws/eventos/evento.wsdl";

                Console.WriteLine($"[EVENTO SIFEN NC] URL: {url}");

                // Enviar al webservice
                Console.WriteLine($"[EVENTO SIFEN NC] Enviando evento a SIFEN...");
                using var httpClient = CrearHttpClientConCertificado(certPath!, certPassword!);
                
                // Content-Type correcto para SOAP 1.2
                var content = new StringContent(soap, Encoding.UTF8, "application/soap+xml");
                
                // Headers
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Java/1.8.0_341");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/soap+xml, application/xml, text/xml");
                httpClient.DefaultRequestHeaders.Add("Connection", "close");

                Console.WriteLine($"[EVENTO SIFEN NC] Enviando {soap.Length} bytes...");
                var response = await httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();
                result.XmlRespuesta = responseBody;

                Console.WriteLine($"[EVENTO SIFEN NC] Response Status: {response.StatusCode}");
                Console.WriteLine($"[EVENTO SIFEN NC] Response Length: {responseBody.Length}");
                Console.WriteLine($"[EVENTO SIFEN NC] Response Body:");
                Console.WriteLine(responseBody);

                // Guardar respuesta para debug
                try
                {
                    var debugDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Debug");
                    var logPath = Path.Combine(debugDir, "evento_sifen_nc_response.txt");
                    var logContent = $"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                     $"Status: {response.StatusCode}\n" +
                                     $"CDC: {nc.CDC}\n" +
                                     $"EventoId: {eventoId}\n" +
                                     $"Motivo: {motivo}\n\n" +
                                     $"=== RESPUESTA ===\n{responseBody}\n\n" +
                                     $"=== SOAP ENVIADO ===\n{soap}";
                    File.WriteAllText(logPath, logContent);
                    Console.WriteLine($"[EVENTO SIFEN NC] Respuesta guardada en: {logPath}");
                }
                catch { /* ignorar errores de logging */ }

                // Parsear respuesta
                var (codigo, mensaje) = ParsearRespuestaEvento(responseBody);
                result.Codigo = codigo;
                result.Mensaje = mensaje ?? responseBody;

                Console.WriteLine($"[EVENTO SIFEN NC] Código respuesta: {codigo}");
                Console.WriteLine($"[EVENTO SIFEN NC] Mensaje respuesta: {mensaje}");

                // Código 0600 = "Evento registrado correctamente" (éxito)
                if (codigo == "0600")
                {
                    result.Exito = true;

                    // Actualizar estado de la NC
                    nc.EstadoSifen = "CANCELADO";
                    nc.MensajeSifen = $"Cancelado vía evento SIFEN. Motivo: {motivo}";
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"[EVENTO SIFEN NC] ✅ ÉXITO - NC {idNotaCredito} cancelada correctamente");
                }
                else
                {
                    Console.WriteLine($"[EVENTO SIFEN NC] ❌ ERROR - Código: {codigo}, Mensaje: {mensaje}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EVENTO SIFEN NC] ❌ EXCEPCIÓN: {ex.Message}");
                Console.WriteLine($"[EVENTO SIFEN NC] Stack: {ex.StackTrace}");
                result.Mensaje = $"Error: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Genera un ID numérico para el evento (similar a PowerBuilder que usa números como 12529)
        /// </summary>
        private string GenerarIdEvento()
        {
            // Usar últimos 5 dígitos del timestamp + random para evitar colisiones
            var timestamp = DateTime.Now.ToString("HHmmss").Substring(0, 5);
            return timestamp;
        }

        /// <summary>
        /// Construye el XML interno del evento (estructura dentro del SOAP, antes de firmar)
        /// Estructura basada en el log de Power Builder que funciona
        /// </summary>
        private string ConstruirXmlInternoEvento(string eventoId, string cdc, string motivo)
        {
            var fechaFirma = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

            // Estructura EXACTA del XML que funciona en Power Builder
            // NOTA: NO incluye dTiGDE, el tipo se determina por el elemento rGeVeCan
            var xml = $@"<gGroupGesEve xsi:schemaLocation=""{SIFEN_NS} siRecepEvento_v150.xsd"" xmlns=""{SIFEN_NS}"" xmlns:xsi=""{XSI_NS}"">
	<rGesEve>
		<rEve Id=""{eventoId}"">
			<dFecFirma>{fechaFirma}</dFecFirma>
			<dVerFor>150</dVerFor>
			<gGroupTiEvt>
				<rGeVeCan>
					<Id>{cdc}</Id>
					<mOtEve>{EscapeXml(motivo)}</mOtEve>
				</rGeVeCan>
			</gGroupTiEvt>
		</rEve>
	</rGesEve>
</gGroupGesEve>";

            return xml;
        }

        /// <summary>
        /// Firma el XML del evento. La firma va DENTRO de rGesEve, DESPUÉS de /rEve
        /// </summary>
        private string FirmarEvento(string xmlInterno, string eventoId, string certPath, string certPassword)
        {
            // Crear documento XML
            var doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.LoadXml(xmlInterno);

            // Buscar nodo rEve para firmar
            var rEveNode = doc.GetElementsByTagName("rEve")[0]
                ?? throw new InvalidOperationException("No se encontró el nodo rEve");

            Console.WriteLine($"[EVENTO FIRMA] Nodo rEve encontrado, Id={eventoId}");

            // Cargar certificado
            var cert = new X509Certificate2(certPath, certPassword,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);

            Console.WriteLine($"[EVENTO FIRMA] Certificado: {cert.Subject}");
            Console.WriteLine($"[EVENTO FIRMA] HasPrivateKey: {cert.HasPrivateKey}");

            // Obtener clave RSA
            var rsaKey = cert.GetRSAPrivateKey()
                ?? throw new CryptographicException("No se pudo obtener clave RSA del certificado");

            Console.WriteLine($"[EVENTO FIRMA] RSA KeySize: {rsaKey.KeySize}");

            // Configurar firma
            var signedXml = new SignedXmlWithId(doc) { SigningKey = rsaKey };

            var reference = new Reference
            {
                Uri = "#" + eventoId,
                DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256"
            };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NTransform());

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert));

            signedXml.AddReference(reference);
            if (signedXml.SignedInfo != null)
            {
                signedXml.SignedInfo.CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";
                signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            }
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();
            var signature = signedXml.GetXml();

            Console.WriteLine($"[EVENTO FIRMA] Firma computada correctamente");

            // Importar firma al documento
            XmlNode importedSignature = doc.ImportNode(signature, true);

            // Insertar firma DENTRO de rGesEve, DESPUÉS de rEve (como en el log de PowerBuilder)
            var rGesEveNode = doc.GetElementsByTagName("rGesEve")[0];
            if (rGesEveNode != null)
            {
                rGesEveNode.AppendChild(importedSignature);
                Console.WriteLine($"[EVENTO FIRMA] Signature insertado en rGesEve, después de rEve");
            }
            else
            {
                throw new InvalidOperationException("No se encontró el nodo rGesEve para insertar la firma");
            }

            return doc.OuterXml;
        }

        /// <summary>
        /// Construye el SOAP completo para enviar a SIFEN
        /// Estructura basada en el log de Power Builder que funciona
        /// </summary>
        private string ConstruirSoapCompleto(string xmlFirmado, string eventoId)
        {
            // Estructura SOAP 1.2 como en Power Builder
            var soap = $@"<?xml version=""1.0"" encoding=""UTF-8""?><soap:Envelope xmlns:soap=""{SOAP12_NS}""><soap:Header /><soap:Body><rEnviEventoDe xmlns=""{SIFEN_NS}""><dId>{eventoId}</dId><dEvReg>{xmlFirmado}</dEvReg></rEnviEventoDe></soap:Body></soap:Envelope>";

            return soap;
        }

        /// <summary>
        /// Parsea la respuesta SIFEN para extraer código y mensaje
        /// </summary>
        private (string? codigo, string? mensaje) ParsearRespuestaEvento(string responseXml)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(responseXml);

                // Buscar en gResProc (respuesta de evento)
                // NOTA: Los elementos pueden venir con prefijo ns2: o sin prefijo
                var codRes = doc.GetElementsByTagName("dCodRes").Cast<XmlNode>().FirstOrDefault()?.InnerText
                          ?? doc.GetElementsByTagName("ns2:dCodRes").Cast<XmlNode>().FirstOrDefault()?.InnerText;
                var msgRes = doc.GetElementsByTagName("dMsgRes").Cast<XmlNode>().FirstOrDefault()?.InnerText
                          ?? doc.GetElementsByTagName("ns2:dMsgRes").Cast<XmlNode>().FirstOrDefault()?.InnerText;

                // También buscar estado del resultado
                var estRes = doc.GetElementsByTagName("dEstRes").Cast<XmlNode>().FirstOrDefault()?.InnerText
                          ?? doc.GetElementsByTagName("ns2:dEstRes").Cast<XmlNode>().FirstOrDefault()?.InnerText;
                if (!string.IsNullOrEmpty(estRes))
                {
                    Console.WriteLine($"[EVENTO SIFEN] Estado resultado: {estRes}");
                }

                // Buscar protocolo de autorización
                var protAut = doc.GetElementsByTagName("dProtAut").Cast<XmlNode>().FirstOrDefault()?.InnerText
                           ?? doc.GetElementsByTagName("ns2:dProtAut").Cast<XmlNode>().FirstOrDefault()?.InnerText;
                if (!string.IsNullOrEmpty(protAut))
                {
                    Console.WriteLine($"[EVENTO SIFEN] Protocolo autorización: {protAut}");
                }

                return (codRes, msgRes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EVENTO SIFEN] Error parseando respuesta: {ex.Message}");
                return (null, $"Error parseando respuesta: {ex.Message}");
            }
        }

        private static string EscapeXml(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }
    }

    /// <summary>
    /// SignedXml personalizado para soportar Id con prefijo vacío
    /// (requerido por SIFEN que usa Id="..." sin prefijo)
    /// </summary>
    public class SignedXmlWithId : SignedXml
    {
        public SignedXmlWithId(XmlDocument doc) : base(doc) { }

        public override XmlElement? GetIdElement(XmlDocument document, string idValue)
        {
            // Primero intentar el método estándar
            var elem = base.GetIdElement(document, idValue);
            if (elem != null) return elem;

            // Buscar por atributo Id (sin namespace)
            var nodeList = document.SelectNodes($"//*[@Id='{idValue}']");
            if (nodeList?.Count > 0) return nodeList[0] as XmlElement;

            return null;
        }
    }
}
