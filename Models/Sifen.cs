using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Security.Cryptography;
using System.Net.Http;
using System.IO.Compression;
using System.Security.Cryptography.Xml;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Security.Authentication;

namespace SistemIA.Models
{
    [Guid("b1d23d8e-4e7b-4dc8-81fb-bfdb47d7f521")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class Sifen
    {
        private static readonly HttpClient _httpClient;

        static Sifen()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Serializa un XmlDocument a string con encoding UTF-8 correcto (sin BOM).
        /// CRÍTICO: XmlDocument.OuterXml puede corromper caracteres UTF-8.
        /// Esta función garantiza que caracteres como "í" se mantengan correctos.
        /// </summary>
        /// <param name="omitDeclaration">Si true, omite la declaración XML (para envío sync a SIFEN)</param>
        public static string SerializeXmlToUtf8String(XmlDocument doc, bool indent = false, bool omitDeclaration = false)
        {
            using var ms = new MemoryStream();
            using (var writer = XmlWriter.Create(ms, new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                Indent = indent,
                OmitXmlDeclaration = omitDeclaration
            }))
            {
                doc.WriteTo(writer);
                writer.Flush();
            }
            ms.Position = 0;
            using var reader = new StreamReader(ms, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        public static string SHA256ToString(string s)
        {
            using var alg = SHA256.Create();
            byte[] hash = alg.ComputeHash(Encoding.UTF8.GetBytes(s));
            return Convert.ToHexString(hash).ToLower();
        }

        /// <summary>
        /// Escribe un log al archivo Debug/sifen_debug.log con encoding UTF-8.
        /// Esta función garantiza que los caracteres especiales se guarden correctamente,
        /// evitando la corrupción de UTF-8 que ocurre en la terminal de PowerShell.
        /// </summary>
        public static void LogSifenDebug(string mensaje, string? contenido = null)
        {
            try
            {
                // Usar ruta fija para garantizar que se encuentre el archivo
                var debugDir = @"c:\asis\SistemIA\Debug";
                if (!Directory.Exists(debugDir))
                    Directory.CreateDirectory(debugDir);
                    
                var logFile = Path.Combine(debugDir, "sifen_debug.log");
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                
                var sb = new StringBuilder();
                sb.AppendLine($"\n{new string('=', 80)}");
                sb.AppendLine($"[{timestamp}] {mensaje}");
                sb.AppendLine($"{new string('=', 80)}");
                
                if (!string.IsNullOrEmpty(contenido))
                {
                    sb.AppendLine(contenido);
                }
                
                // Usar UTF8 encoding sin BOM para preservar caracteres especiales
                File.AppendAllText(logFile, sb.ToString(), new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LogSifenDebug] Error escribiendo log: {ex.Message}");
            }
        }

        /// <summary>
        /// Limpia el archivo de log SIFEN para empezar una nueva sesión de debugging.
        /// </summary>
        public static void ClearSifenDebugLog()
        {
            try
            {
                var logFile = @"c:\asis\SistemIA\Debug\sifen_debug.log";
                if (File.Exists(logFile))
                {
                    File.Delete(logFile);
                }
            }
            catch { }
        }
        
        /// <summary>
        /// Inicia una nueva sesión de debugging SIFEN con nombre descriptivo.
        /// Guarda archivos individuales en Debug/sesion_{timestamp}/ para análisis posterior.
        /// </summary>
        public static string IniciarSesionDebug(string descripcion, int? idVenta = null)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var sesionId = $"sesion_{timestamp}";
                var sesionDir = Path.Combine(@"c:\asis\SistemIA\Debug", sesionId);
                
                if (!Directory.Exists(sesionDir))
                    Directory.CreateDirectory(sesionDir);
                
                // Limpiar log principal
                ClearSifenDebugLog();
                
                // Crear archivo de sesión
                var infoPath = Path.Combine(sesionDir, "00_INFO.txt");
                var sb = new StringBuilder();
                sb.AppendLine($"═══════════════════════════════════════════════════════════════════");
                sb.AppendLine($"SESIÓN DE PRUEBAS SIFEN - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"═══════════════════════════════════════════════════════════════════");
                sb.AppendLine($"Descripción: {descripcion}");
                if (idVenta.HasValue) sb.AppendLine($"ID Venta: {idVenta}");
                sb.AppendLine($"Sesión ID: {sesionId}");
                sb.AppendLine($"Directorio: {sesionDir}");
                sb.AppendLine($"═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();
                
                File.WriteAllText(infoPath, sb.ToString(), new UTF8Encoding(false));
                LogSifenDebug($"NUEVA SESIÓN: {descripcion}", $"Sesión ID: {sesionId}\nVenta ID: {idVenta}");
                
                return sesionDir;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IniciarSesionDebug] Error: {ex.Message}");
                return @"c:\asis\SistemIA\Debug";
            }
        }
        
        /// <summary>
        /// Guarda un archivo de debug en la sesión actual.
        /// </summary>
        public static void GuardarArchivoSesion(string sesionDir, string nombreArchivo, string contenido, string? descripcion = null)
        {
            try
            {
                if (!Directory.Exists(sesionDir))
                    Directory.CreateDirectory(sesionDir);
                    
                var filePath = Path.Combine(sesionDir, nombreArchivo);
                
                if (!string.IsNullOrEmpty(descripcion))
                {
                    var header = $"<!-- {descripcion} - {DateTime.Now:yyyy-MM-dd HH:mm:ss} -->\n";
                    contenido = header + contenido;
                }
                
                File.WriteAllText(filePath, contenido, new UTF8Encoding(false));
                LogSifenDebug($"ARCHIVO GUARDADO: {nombreArchivo}", $"Path: {filePath}\nTamaño: {contenido.Length} caracteres");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GuardarArchivoSesion] Error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Agrega un paso al log de la sesión actual.
        /// </summary>
        public static void LogPasoSesion(string sesionDir, int numeroPaso, string titulo, string? detalle = null)
        {
            try
            {
                var logPath = Path.Combine(sesionDir, "00_PASOS.txt");
                var sb = new StringBuilder();
                sb.AppendLine($"\n[{DateTime.Now:HH:mm:ss.fff}] PASO {numeroPaso}: {titulo}");
                sb.AppendLine(new string('-', 60));
                if (!string.IsNullOrEmpty(detalle))
                    sb.AppendLine(detalle);
                sb.AppendLine();
                
                File.AppendAllText(logPath, sb.ToString(), new UTF8Encoding(false));
                LogSifenDebug($"PASO {numeroPaso}: {titulo}", detalle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LogPasoSesion] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// <summary>
        /// Comprime XML usando ZIP real para envío a SIFEN (modo LOTE).
        /// ========================================================================
        /// FIX 12-Ene-2026: Cambiado de GZipStream a ZipArchive (ZIP real)
        /// ========================================================================
        /// El PDF "Recomendaciones y mejores prácticas SIFEN" (página 8) muestra
        /// que el contenido de xDE empieza con "UEs..." que es la signatura de ZIP
        /// (PK en Base64 = 50 4B), NO GZip (H4sI = 1F 8B).
        /// 
        /// Power Builder usaba GZip pero SIFEN espera ZIP real según el manual.
        /// ========================================================================
        /// </summary>
        public static string StringToZip(string originalString)
        {
            byte[] compressedBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Crear archivo ZIP real (no GZip)
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    // FIX 12-Ene-2026: El DLL de nextsys que FUNCIONA usa "compressed.txt"
                    // NO un nombre con fecha como DE_DDMMYYYY.xml
                    var entry = zipArchive.CreateEntry("compressed.txt", CompressionLevel.Optimal);
                    
                    using (var entryStream = entry.Open())
                    using (var writer = new StreamWriter(entryStream, new UTF8Encoding(false)))
                    {
                        writer.Write(originalString);
                        writer.Flush();
                    }
                }
                compressedBytes = memoryStream.ToArray();
            }
            Console.WriteLine($"[SIFEN DEBUG] StringToZip ZIP bytes: {compressedBytes.Length}");
            return Convert.ToBase64String(compressedBytes);
        }

        /// <summary>
        /// Quita el atributo xmlns de un elemento XML y todos sus descendientes recursivamente.
        /// FIX 16-Ene-2026: Necesario para que la Signature herede el namespace SIFEN del padre.
        /// </summary>
        private void QuitarNamespaceRecursivo(XmlElement element)
        {
            // Quitar xmlns del elemento actual
            element.RemoveAttribute("xmlns");
            
            // Procesar todos los hijos que sean elementos
            foreach (XmlNode child in element.ChildNodes)
            {
                if (child is XmlElement childElement)
                {
                    QuitarNamespaceRecursivo(childElement);
                }
            }
        }

        /// <summary>
        /// Convierte un string Base64 a su representación hexadecimal.
        /// SIFEN requiere que DigestValue (que viene en Base64 del XML) se convierta a HEX
        /// decodificando primero los bytes del Base64, NO convirtiendo los caracteres del string.
        /// 
        /// FIX CRÍTICO 31-Ene-2026:
        /// ANTES: Convertía cada carácter del string Base64 a HEX (incorrecto)
        ///        "obnJek3R..." → "6f626e4a656b33..." (HEX de los caracteres ASCII)
        /// 
        /// SIFEN requiere el HEX de los CARACTERES ASCII del texto Base64.
        /// IMPORTANTE: Esto contradice la documentación del Manual Técnico v150 (sección 13.8.4.3)
        /// pero es lo que SIFEN realmente acepta según XMLs aprobados en producción.
        /// 
        /// Ejemplo: "abc=" → "6162633d" (hex de cada carácter: a=61, b=62, c=63, ==3d)
        /// </summary>
        public string StringToHex(string textString)
        {
            // Convertir cada carácter a su valor ASCII y luego a hexadecimal
            return string.Concat(textString.Select(c => Convert.ToInt32(c).ToString("x2")));
        }

        public string GetTagValue(string xmlString, string tagName)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            var nodeList = xmlDoc.GetElementsByTagName(tagName);
            return nodeList.Count > 0 ? nodeList[0]?.InnerText ?? string.Empty : "Tag not found.";
        }

        public async Task<string> Enviar(string url, string documento, string pathArchivoP12, string passwordArchivoP12)
        {
            // Reintentos automáticos para problemas de conexión SSL/BIG-IP de SIFEN Paraguay
            // Los servidores SIFEN usan balanceadores BIG-IP que frecuentemente rechazan conexiones
            const int maxRetries = 5;
            int[] delaySeconds = { 1, 2, 3, 5, 8 }; // Fibonacci-like backoff
            Exception? lastException = null;
            string lastError = "";
            
            // Detectar si es consulta RUC o envío de documento
            bool esConsultaRuc = url.Contains("consulta-ruc") || documento.Contains("rEnviConsRUC");
            bool esEnvioDocumento = url.Contains("recibe") || documento.Contains("rEnviLote") || documento.Contains("rEnvioLote");
            
            for (int intento = 1; intento <= maxRetries; intento++)
            {
                try
                {
                    Console.WriteLine($"[SIFEN] Intento {intento}/{maxRetries} de envío a {url}");
                    var resultado = await EnviarInterno(url, documento, pathArchivoP12, passwordArchivoP12);
                    
                    // Verificar si es respuesta exitosa de SIFEN (contiene XML SOAP válido)
                    bool esRespuestaValida = !string.IsNullOrEmpty(resultado) && 
                        (resultado.Contains("soap:Envelope") || resultado.Contains("env:Envelope") ||
                         resultado.Contains("rRetEnvi") || resultado.Contains("rEnviConsRUCResponse") || 
                         resultado.Contains("dCodRes") || resultado.Contains("rProtDe") || resultado.Contains("ns2:"));
                    
                    // Para consulta RUC: verificar que sea respuesta del tipo correcto
                    // Si recibimos rRetEnviDe/rProtDe con error 0160 en consulta RUC, el BIG-IP mezcló respuestas
                    bool esRespuestaIncorrecta = false;
                    
                    if (esConsultaRuc && resultado.Contains("rRetEnviDe") && !resultado.Contains("rResEnviConsRUC"))
                    {
                        esRespuestaIncorrecta = true;
                        Console.WriteLine($"[SIFEN] ⚠️ Servidor devolvió tipo de respuesta incorrecta (rRetEnviDe en lugar de rResEnviConsRUC)");
                    }
                    // Para envío de documentos: verificar que no recibamos respuesta de consulta RUC
                    else if (esEnvioDocumento && resultado.Contains("rResEnviConsRUC") && !resultado.Contains("rRetEnvi"))
                    {
                        esRespuestaIncorrecta = true;
                        Console.WriteLine($"[SIFEN] ⚠️ Servidor devolvió tipo de respuesta incorrecta (rResEnviConsRUC en lugar de rRetEnviDe/rRetEnviLoteDe)");
                    }
                    
                    if (esRespuestaIncorrecta)
                    {
                        if (intento < maxRetries)
                        {
                            var delay = delaySeconds[intento - 1] * 1000;
                            Console.WriteLine($"[SIFEN] Esperando {delaySeconds[intento - 1]}s antes del próximo intento...");
                            await Task.Delay(delay);
                            continue;
                        }
                    }
                    
                    if (esRespuestaValida && !esRespuestaIncorrecta)
                    {
                        Console.WriteLine($"[SIFEN] ✓ Respuesta SIFEN válida en intento {intento}");
                        return resultado;
                    }
                    
                    // Verificar si es error que requiere reintento
                    bool esErrorConexion = string.IsNullOrEmpty(resultado) ||
                        resultado.Contains("\"error\":") ||
                        resultado.Contains("SSL") || 
                        resultado.Contains("conexión") || 
                        resultado.Contains("connection") ||
                        resultado.Contains("timeout") || 
                        resultado.Contains("refused") || 
                        resultado.Contains("reset") ||
                        resultado.Contains("BIG-IP") ||
                        resultado.Contains("logout") ||
                        resultado.Contains("<html") ||
                        resultado.Contains("500") ||
                        resultado.Contains("503") ||
                        resultado.Contains("502") ||
                        resultado.Contains("closed") ||
                        resultado.Contains("aborted");
                    
                    if (esErrorConexion && intento < maxRetries)
                    {
                        lastError = resultado ?? "Respuesta vacía";
                        var delay = delaySeconds[intento - 1] * 1000;
                        Console.WriteLine($"[SIFEN] ⚠️ Error de conexión en intento {intento}: {lastError.Substring(0, Math.Min(100, lastError.Length))}");
                        Console.WriteLine($"[SIFEN] Esperando {delaySeconds[intento - 1]}s antes del próximo intento...");
                        await Task.Delay(delay);
                        continue;
                    }
                    
                    // Si no es error de conexión o es último intento, retornar resultado
                    return resultado;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SIFEN] ❌ Excepción en intento {intento}: {ex.Message}");
                    lastException = ex;
                    lastError = ex.Message;
                    if (intento < maxRetries)
                    {
                        var delay = delaySeconds[intento - 1] * 1000;
                        Console.WriteLine($"[SIFEN] Esperando {delaySeconds[intento - 1]}s antes del próximo intento...");
                        await Task.Delay(delay);
                    }
                }
            }
            
            Console.WriteLine($"[SIFEN] ❌ Fallaron los {maxRetries} intentos de conexión");
            return $"{{\"error\": \"Fallaron {maxRetries} intentos de conexión a SIFEN\", \"lastError\": \"{lastError}\", \"suggestion\": \"El servidor SIFEN no responde. Intente nuevamente en unos segundos.\"}}";
        }

        private async Task<string> EnviarInterno(string url, string documento, string pathArchivoP12, string passwordArchivoP12)
        {
            X509Certificate2? certificate = null;
            try
            {
                // Configurar flags apropiados para Windows
                // Usar Exportable para permitir que la clave privada sea accesible
                var keyStorageFlags = X509KeyStorageFlags.Exportable | 
                                     X509KeyStorageFlags.PersistKeySet |
                                     X509KeyStorageFlags.UserKeySet;
                
                Console.WriteLine($"[DEBUG] Cargando certificado desde: {pathArchivoP12}");
                certificate = new X509Certificate2(pathArchivoP12, passwordArchivoP12, keyStorageFlags);
                
                Console.WriteLine($"[DEBUG] Certificado cargado: {certificate.Subject}");
                Console.WriteLine($"[DEBUG] Válido desde: {certificate.NotBefore} hasta: {certificate.NotAfter}");
                Console.WriteLine($"[DEBUG] Thumbprint: {certificate.Thumbprint}");
                Console.WriteLine($"[DEBUG] Tiene clave privada: {certificate.HasPrivateKey}");
                
                using var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(certificate);
                
                // Configuración SSL más permisiva para SIFEN (desarrollo)
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => 
                {
                    Console.WriteLine($"[SSL] Validando certificado del servidor: {cert?.Subject}");
                    Console.WriteLine($"[SSL] Issuer: {cert?.Issuer}");
                    Console.WriteLine($"[SSL] Errores SSL: {errors}");
                    Console.WriteLine($"[SSL] Thumbprint: {cert?.Thumbprint}");
                    
                    if (errors != System.Net.Security.SslPolicyErrors.None)
                    {
                        Console.WriteLine($"[SSL] ADVERTENCIA: Errores de validación SSL detectados, pero aceptando para desarrollo");
                        Console.WriteLine($"[SSL] Detalles del error: {errors}");
                        if (chain != null)
                        {
                            Console.WriteLine($"[SSL] Estado de la cadena: {chain.ChainStatus?.Length ?? 0} problemas");
                            foreach (var status in chain.ChainStatus ?? Array.Empty<System.Security.Cryptography.X509Certificates.X509ChainStatus>())
                            {
                                Console.WriteLine($"[SSL]   - {status.Status}: {status.StatusInformation}");
                            }
                        }
                    }
                    
                    // Aceptar TODOS los certificados (solo para desarrollo/pruebas)
                    Console.WriteLine($"[SSL] ✓ Certificado aceptado (modo desarrollo)");
                    return true;
                };
                
                // SIFEN Paraguay requiere explícitamente TLS 1.2
                handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                handler.CheckCertificateRevocationList = false;
                
                // Configuraciones adicionales para bypass de proxies BIG-IP
                handler.UseCookies = true; // Habilitar cookies para sesiones proxy
                handler.CookieContainer = new System.Net.CookieContainer(); // Contenedor de cookies para persistencia
                handler.AutomaticDecompression = System.Net.DecompressionMethods.None; // Sin descompresión automática
                handler.AllowAutoRedirect = false; // No seguir redirecciones automáticas del proxy
                handler.MaxConnectionsPerServer = 1; // Una conexión por servidor para evitar problemas de pool
                handler.PreAuthenticate = true; // Pre-autenticar con certificado cliente
                
                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(120); // Timeout extendido
                
                // Headers específicos para bypass de BIG-IP según documentación
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "Java/1.8.0_341");
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                // Origin robusto: si no contiene "/de/", usar el origen del host
                string originHeader;
                var idx = url.IndexOf("/de/", StringComparison.OrdinalIgnoreCase);
                if (idx > 0)
                {
                    originHeader = url.Substring(0, idx);
                }
                else
                {
                    try { originHeader = new Uri(url).GetLeftPart(UriPartial.Authority); }
                    catch { originHeader = url; }
                }
                client.DefaultRequestHeaders.Add("Origin", originHeader);
                client.DefaultRequestHeaders.Add("Referer", url);
                client.DefaultRequestHeaders.Add("Accept-Encoding", "identity");
                client.DefaultRequestHeaders.Add("Connection", "close");
                client.DefaultRequestHeaders.Add("Accept", "application/soap+xml, application/xml, text/xml");
                client.DefaultRequestHeaders.Add("Accept-Language", "es-PY,es;q=0.9");
                client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                
                using var content = new StringContent(documento, Encoding.UTF8, "application/soap+xml");
                // Headers para SOAP 1.2: Content-Type application/soap+xml; charset=utf-8 y action
                // No enviar SOAPAction separado (propio de SOAP 1.1)
                content.Headers.Clear();
                // Detectar la operación para setear el parámetro action correctamente
                string? op = null;
                bool esConsulta = false;
                try
                {
                    if (documento.Contains("<rEnviLoteDe", StringComparison.OrdinalIgnoreCase)) op = "rEnviLoteDe";
                    else if (documento.Contains("<rEnvioLote", StringComparison.OrdinalIgnoreCase)) op = "rEnvioLote";
                    else if (documento.Contains("<rEnviDe", StringComparison.OrdinalIgnoreCase)) op = "rEnviDe"; // FIX: Cambiado de rEnviDeRequest a rEnviDe
                    else if (documento.Contains("<rEnviConsLoteDe", StringComparison.OrdinalIgnoreCase)) { op = "rEnviConsLoteDe"; esConsulta = true; }
                    else if (documento.Contains("<rEnviConsDeRequest", StringComparison.OrdinalIgnoreCase)) { op = "rEnviConsDeRequest"; esConsulta = true; }
                    else if (documento.Contains("<rEnviConsRUC", StringComparison.OrdinalIgnoreCase)) { op = "rEnviConsRUC"; esConsulta = true; }
                }
                catch { /* noop */ }
                
                // IMPORTANTE: El código Java de PowerBuilder usa:
                // con.setRequestProperty("Content-Type", "text/xml; charset=utf-8");
                // Esto es CRÍTICO para que SIFEN acepte el XML
                // Ver: SifenResource.java línea 350-351
                string contentType = "text/xml; charset=utf-8";
                content.Headers.Add("Content-Type", contentType);
                // Fallback adicional: incluir SOAPAction como header (algunos balanceadores lo usan aún con SOAP 1.2)
                // Solo para operaciones que NO son consultas
                if (!string.IsNullOrEmpty(op) && !esConsulta)
                {
                    try
                    {
                        if (!client.DefaultRequestHeaders.Contains("SOAPAction"))
                            client.DefaultRequestHeaders.Add("SOAPAction", $"http://ekuatia.set.gov.py/sifen/xsd/{op}");
                    }
                    catch { }
                }
                
                Console.WriteLine($"[DEBUG] Enviando a URL: {url}");
                Console.WriteLine($"[DEBUG] Método: POST");
                Console.WriteLine($"[DEBUG] Content-Type: {content.Headers.ContentType}");
                Console.WriteLine($"[DEBUG] Documento length: {documento.Length}");
                Console.WriteLine($"[DEBUG] XML enviado:");
                Console.WriteLine(documento);
                Console.WriteLine($"[DEBUG] Headers request:");
                foreach (var header in client.DefaultRequestHeaders)
                {
                    Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                }
                
                // *** LOGGING A ARCHIVO (evita problemas de UTF-8 en terminal) ***
                var headersStr = new StringBuilder();
                foreach (var header in client.DefaultRequestHeaders)
                {
                    headersStr.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                }
                LogSifenDebug($"ENVÍO SIFEN - URL: {url}", 
                    $"Content-Type: {contentType}\nDocumento Length: {documento.Length}\n\n" +
                    $"HEADERS:\n{headersStr}\n\n" +
                    $"SOAP ENVIADO:\n{documento}");
                
                using var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                
                // *** LOGGING RESPUESTA A ARCHIVO ***
                LogSifenDebug($"RESPUESTA SIFEN - Status: {response.StatusCode} ({(int)response.StatusCode})", 
                    $"Response Length: {responseString.Length}\n\n" +
                    $"RESPUESTA:\n{responseString}");
                
                Console.WriteLine($"[DEBUG] Status Code: {response.StatusCode} ({(int)response.StatusCode})");
                Console.WriteLine($"[DEBUG] Response Headers:");
                foreach (var header in response.Headers)
                {
                    Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                }
                Console.WriteLine($"[DEBUG] Content Headers:");
                foreach (var header in response.Content.Headers)
                {
                    Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                }
                Console.WriteLine($"[DEBUG] Response length: {responseString.Length}");
                Console.WriteLine($"[DEBUG] Response preview: {(responseString.Length > 200 ? responseString.Substring(0, 200) + "..." : responseString)}");
                
                // Guardar respuesta completa a archivo para análisis
                try
                {
                    var debugDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Debug");
                    if (!Directory.Exists(debugDir)) Directory.CreateDirectory(debugDir);
                    var respFile = Path.Combine(debugDir, $"SIFEN_Response_{DateTime.Now:yyyyMMdd_HHmmss}.xml");
                    File.WriteAllText(respFile, responseString);
                    Console.WriteLine($"[DEBUG] Respuesta completa guardada en: {respFile}");
                }
                catch { /* mejor esfuerzo */ }
                
                // Verificar diferentes tipos de respuestas problemáticas
                if (responseString.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[ERROR] Respuesta HTML inesperada detectada");
                    
                    if (responseString.Contains("BIG-IP"))
                    {
                        Console.WriteLine($"[ERROR] Página BIG-IP logout detectada - problema de autenticación proxy");
                        return $"{{\"error\": \"Proxy BIG-IP rechazó la conexión\", \"details\": \"El balanceador de carga redirigió a página de logout. Verificar certificado cliente y configuración SSL.\", \"solution\": \"Revisar certificado .p12 y contraseña, o contactar soporte técnico SET.\"}}";
                    }
                    else if (responseString.Contains("login") || responseString.Contains("authentication"))
                    {
                        Console.WriteLine($"[ERROR] Página de autenticación detectada");
                        return $"{{\"error\": \"Página de autenticación detectada\", \"details\": \"El servidor requiere autenticación adicional.\", \"solution\": \"Verificar certificado cliente y credenciales.\"}}";
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] Respuesta HTML genérica inesperada");
                        return $"{{\"error\": \"Respuesta HTML inesperada del servidor SIFEN\", \"details\": \"El servidor retornó una página web en lugar de XML SOAP.\", \"html_preview\": \"{responseString.Substring(0, Math.Min(200, responseString.Length))}\"}}";
                    }
                }
                
                // Verificar respuesta XML válida de SIFEN
                if (responseString.Contains("soap:") && (responseString.Contains("rEnviConsRUCResponse") || responseString.Contains("rRetEnviLote") || responseString.Contains("rRetEnviDe") || responseString.Contains("rProtDe")))
                {
                    Console.WriteLine($"[SUCCESS] Respuesta SIFEN válida recibida");
                }
                else if (responseString.Contains("soap:Fault") || responseString.Contains("fault"))
                {
                    Console.WriteLine($"[ERROR] SOAP Fault recibido del servidor SIFEN");
                    return $"{{\"error\": \"SOAP Fault desde SIFEN\", \"details\": \"El servidor SIFEN retornó un error SOAP.\", \"fault_preview\": \"{responseString.Substring(0, Math.Min(300, responseString.Length))}\"}}";
                }
                
                return responseString;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[ERROR] HttpRequestException: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ERROR] Inner Exception: {ex.InnerException.Message}");
                }
                return $"{{\"error\": \"{ex.Message}\", \"innerError\": \"{ex.InnerException?.Message}\"}}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] General Exception: {ex.Message}");
                return $"{{\"error\": \"{ex.Message}\"}}";
            }
            finally
            {
                // Disponer el certificado correctamente
                certificate?.Dispose();
            }
        }

        /// <summary>
        /// Remueve el atributo xmlns del elemento rDE (para que herede del padre rEnviDe)
        /// </summary>
        private static string RemoverNamespaceDeRDE(string xml)
        {
            // Remover xmlns="http://ekuatia.set.gov.py/sifen/xsd" del <rDE>
            return System.Text.RegularExpressions.Regex.Replace(
                xml,
                @"<rDE\s+xmlns:xsi=""[^""]*""\s*xsi:schemaLocation=""[^""]*""\s*xmlns=""[^""]*""",
                "<rDE",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Remueve xmlns Y schemaLocation del elemento rDE
        /// </summary>
        private static string RemoverNamespaceYSchemaDeRDE(string xml)
        {
            // Remover todos los atributos de namespace del <rDE>
            var result = System.Text.RegularExpressions.Regex.Replace(
                xml,
                @"<rDE\s+[^>]*>",
                "<rDE>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return result;
        }

        /// <summary>
        /// Remueve solo el atributo schemaLocation del rDE (mantiene xmlns)
        /// </summary>
        private static string RemoverSchemaLocationDeRDE(string xml)
        {
            return System.Text.RegularExpressions.Regex.Replace(
                xml,
                @"\s*xsi:schemaLocation=""[^""]*""",
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Remueve schemaLocation pero mantiene xmlns:xsi
        /// </summary>
        private static string RemoverSchemaLocationMantenerXsi(string xml)
        {
            // Solo remueve el atributo schemaLocation
            return System.Text.RegularExpressions.Regex.Replace(
                xml,
                @"\s*xsi:schemaLocation=""[^""]*""",
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        public string FormatearXML(string XML)
        {
            if (string.IsNullOrWhiteSpace(XML)) return string.Empty;

            try
            {
                using var ms = new MemoryStream();
                // Usar UTF-8 sin BOM para consistencia con SIFEN
                // 31-Ene-2026: OmitXmlDeclaration = true (SIFEN no acepta declaración)
                using var writer = XmlWriter.Create(ms, new XmlWriterSettings 
                { 
                    Indent = true,
                    Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    OmitXmlDeclaration = true
                });
                var doc = new XmlDocument();
                doc.LoadXml(XML);
                doc.WriteTo(writer);
                writer.Flush();
                ms.Position = 0;
                // Leer explícitamente en UTF-8
                using var reader = new StreamReader(ms, Encoding.UTF8);
                return reader.ReadToEnd();
            }
            catch (XmlException)
            {
                return XML;
            }
        }

        public async Task<string> FirmarYEnviar(string url, string urlQR, string xmlString, string p12FilePath, 
            string certificatePassword, string tipoFirmado = "1")
        {
            try
            {
                var doc = new XmlDocument();
                // ========================================================================
                // FIX CRÍTICO 15-Ene-2026: PreserveWhitespace ANTES de LoadXml
                // ========================================================================
                // Sin esto, el DOM normaliza espacios y el DigestValue no coincide
                // SIFEN rechaza con "El documento XML no tiene firma"
                // ========================================================================
                doc.PreserveWhitespace = true;
                doc.LoadXml(xmlString);

                // Eliminar cualquier firma existente (reemplazar por nuestra firma)
                try
                {
                    var toRemove = doc.SelectNodes("//*[local-name()='Signature']");
                    if (toRemove != null)
                    {
                        foreach (XmlNode n in toRemove)
                        {
                            n.ParentNode?.RemoveChild(n);
                        }
                    }
                }
                catch { /* mejor esfuerzo */ }

                // Generar dId de 16 dígitos (yyyyMMddHHmmssNN)
                string GenerateDId16()
                {
                    var baseTs = DateTime.Now.ToString("yyyyMMddHHmmss");
                    // Sufijo pseudoaleatorio 2 dígitos para evitar colisiones en envíos rápidos
                    var rnd = (Environment.TickCount % 100);
                    return baseTs + rnd.ToString("00");
                }
                var dId = GenerateDId16();
                // Ajuste: usar estructura de lote "vista" (no base64/zip) tal como en el ejemplo requerido
                // Construiremos el sobre SOAP con <rEnvioLote><dId/><xDE><rLoteDE><rDE .../></rLoteDE></xDE></rEnvioLote>

                var node = doc.GetElementsByTagName(tipoFirmado == "1" ? "DE" : "rEve")[0] 
                    ?? throw new InvalidOperationException("No se encontró el nodo a firmar");

                var idAttribute = node.Attributes?["Id"] 
                    ?? throw new InvalidOperationException("No se encontró el atributo Id");
                var nodeId = idAttribute.Value;

                // ========================================================================
                // FIX 14-Ene-2026: Usar API RSA MODERNA (sin RSACryptoServiceProvider)
                // ========================================================================
                // PROBLEMA: RSACryptoServiceProvider con CspParameters(24) falla en Windows 10/11
                // con error "Se ha especificado un tipo de proveedor no válido"
                //
                // SOLUCIÓN: Usar GetRSAPrivateKey() directamente como hace el DLL funcional
                // del proyecto SifenProyecto2026 (versión 26 - CORREGIDA)
                //
                // El DLL funcional tiene código exacto:
                //   RSA rsaKey = cert.GetRSAPrivateKey();
                //   signedXml.SigningKey = rsaKey;  // SIN RSACryptoServiceProvider
                //
                // Referencia: .ai-docs/SIFEN/SifenProyecto2026/Sifen2026Proyec/DOCUMENTACION_SOLUCION_FINAL.md
                // ========================================================================
                
                // Cargar certificado con diferentes flags hasta encontrar compatible
                X509Certificate2? cert = null;
                Exception? lastFlagException = null;
                X509KeyStorageFlags[] flagsToTry = new X509KeyStorageFlags[]
                {
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable,
                    X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable,
                    X509KeyStorageFlags.DefaultKeySet | X509KeyStorageFlags.Exportable,
                    X509KeyStorageFlags.Exportable
                };
                
                foreach (var flags in flagsToTry)
                {
                    try
                    {
                        Console.WriteLine($"[FIRMA] Intentando cargar certificado con flags: {flags}");
                        cert = new X509Certificate2(p12FilePath, certificatePassword, flags);
                        if (cert.HasPrivateKey)
                        {
                            Console.WriteLine($"[FIRMA] Certificado cargado OK con flags: {flags}");
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"[FIRMA] Certificado sin clave privada con flags: {flags}");
                            cert.Dispose();
                            cert = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[FIRMA] Fallo con flags {flags}: {ex.Message}");
                        lastFlagException = ex;
                        cert?.Dispose();
                        cert = null;
                    }
                }
                
                if (cert == null)
                {
                    throw new CryptographicException(
                        "No se pudo cargar el certificado con ninguna combinación de flags", 
                        lastFlagException);
                }
                
                Console.WriteLine($"[FIRMA] Certificado: {cert.Subject}");
                Console.WriteLine($"[FIRMA] Thumbprint: {cert.Thumbprint}");
                Console.WriteLine($"[FIRMA] HasPrivateKey: {cert.HasPrivateKey}");
                
                // MÉTODO RSA MODERNO (Compatible Windows 10/11)
                // Usar GetRSAPrivateKey() directamente - NO RSACryptoServiceProvider
                RSA rsaKey = cert.GetRSAPrivateKey() 
                    ?? throw new CryptographicException("No se pudo obtener clave RSA del certificado");
                
                Console.WriteLine($"[FIRMA] RSA obtenida via GetRSAPrivateKey()");
                Console.WriteLine($"[FIRMA] RSA KeySize: {rsaKey.KeySize}");
                Console.WriteLine($"[FIRMA] RSA SignatureAlgorithm: {rsaKey.SignatureAlgorithm}");

                // Usar la clave RSA directamente (sin conversiones ni RSACryptoServiceProvider)
                var signedXml = new SignedXmlWithId(doc) { SigningKey = rsaKey };
                
                var reference = new Reference
                {
                    Uri = "#" + nodeId,
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
                
                // ========================================================================
                // DEBUG FIRMA 10-Ene-2026: Logging para diagnóstico de error de firma
                // ========================================================================
                try
                {
                    var digestValueNode = signature.GetElementsByTagName("DigestValue")[0];
                    var signatureValueNode = signature.GetElementsByTagName("SignatureValue")[0];
                    Console.WriteLine($"[FIRMA DEBUG] DigestValue (Base64): {digestValueNode?.InnerText}");
                    Console.WriteLine($"[FIRMA DEBUG] SignatureValue (primeros 50): {signatureValueNode?.InnerText?.Substring(0, Math.Min(50, signatureValueNode?.InnerText?.Length ?? 0))}...");
                    Console.WriteLine($"[FIRMA DEBUG] Reference URI: #{nodeId}");
                    Console.WriteLine($"[FIRMA DEBUG] Certificado Subject: {cert.Subject}");
                    
                    // Verificar que el nodo DE no se modificó
                    var deXml = node.OuterXml;
                    Console.WriteLine($"[FIRMA DEBUG] Nodo DE longitud: {deXml.Length} chars");
                    Console.WriteLine($"[FIRMA DEBUG] Nodo DE (primeros 200): {deXml.Substring(0, Math.Min(200, deXml.Length))}...");
                }
                catch (Exception exDebug) { Console.WriteLine($"[FIRMA DEBUG ERROR] {exDebug.Message}"); }
                // ========================================================================
                
                // ========================================================================
                // FIX 15-Ene-2026 v3: CORRECCIÓN DEFINITIVA SEGÚN MANUAL SIFEN v150
                // ========================================================================
                // Estructura OBLIGATORIA según Manual Técnico SIFEN v150:
                //   <rDE>
                //     <dVerFor>150</dVerFor>
                //     <DE>
                //       ... contenido ...
                //       <gTotSub>...</gTotSub>
                //       <gCamGen/>              ← OBLIGATORIO (aunque esté vacío)
                //       <Signature>...</Signature>  ← DENTRO de DE, ÚLTIMO hijo
                //     </DE>
                //     <gCamFuFD>...</gCamFuFD>    ← FUERA de DE
                //   </rDE>
                //
                // IMPORTANTE: El prevalidador web valida XSD general.
                // El backend SIFEN valida XSD estricto + canonicalización + orden.
                // ========================================================================
                
                // ========================================================================
                // FIX 16-Ene-2026 v2: ELIMINAR gCamGen vacío
                // ========================================================================
                // DESCUBRIMIENTO: El XML aprobado (xmlRequestVenta_273_sync.xml) NO tiene
                // elemento <gCamGen /> vacío. Si existe y está vacío, ELIMINARLO.
                // ========================================================================
                var gCamGenNodes = doc.GetElementsByTagName("gCamGen");
                foreach (XmlNode gCamGenNode in gCamGenNodes)
                {
                    if (gCamGenNode != null && !gCamGenNode.HasChildNodes)
                    {
                        gCamGenNode.ParentNode?.RemoveChild(gCamGenNode);
                        Console.WriteLine("[FIRMA] gCamGen vacío ELIMINADO (no está en XML de referencia)");
                    }
                }
                
                // 2) Asegurar que gCamFuFD esté FUERA de DE (como hijo de rDE)
                var rdeNode = doc.DocumentElement;
                if (rdeNode != null)
                {
                    var gCamFuFDNodes = doc.GetElementsByTagName("gCamFuFD");
                    if (gCamFuFDNodes.Count > 0)
                    {
                        var gCamFuFD = gCamFuFDNodes[0];
                        if (gCamFuFD != null && gCamFuFD.ParentNode != rdeNode)
                        {
                            // Mover gCamFuFD para que sea hijo directo de rDE
                            gCamFuFD.ParentNode?.RemoveChild(gCamFuFD);
                            rdeNode.AppendChild(gCamFuFD);
                            Console.WriteLine("[FIRMA] gCamFuFD movido como hijo de rDE (fuera de DE)");
                        }
                    }
                }
                
                // ========================================================================
                // 3) INSERCIÓN CRÍTICA DE FIRMA - FIX 16-Ene-2026 v2
                // ========================================================================
                // CORRECCIÓN: La firma va FUERA de </DE>, como hijo de <rDE>
                // Estructura: <rDE> <dVerFor/> <DE>...</DE> <Signature/> <gCamFuFD/> </rDE>
                // ========================================================================
                
                // VERIFICAR que node es realmente <DE>
                Console.WriteLine($"[FIRMA INSERT] node.LocalName = '{node.LocalName}'");
                Console.WriteLine($"[FIRMA INSERT] node.NamespaceURI = '{node.NamespaceURI}'");
                
                if (node.LocalName != "DE")
                {
                    throw new InvalidOperationException($"CRÍTICO: node no es <DE>, es <{node.LocalName}>");
                }
                
                // VERIFICAR que signature existe y tiene contenido
                Console.WriteLine($"[FIRMA INSERT] signature != null: {signature != null}");
                Console.WriteLine($"[FIRMA INSERT] signature.OuterXml.Length: {signature?.OuterXml?.Length ?? 0}");
                
                if (signature == null || string.IsNullOrEmpty(signature.OuterXml))
                {
                    throw new InvalidOperationException("CRÍTICO: signature es null o vacío");
                }
                
                // ========================================================================
                // FIX 16-Ene-2026: POSICIÓN DE SIGNATURE - CORREGIDO 16-Ene-2026 v2
                // ========================================================================
                // DESCUBRIMIENTO CRÍTICO (analizando xmlRequestVenta_273_sync.xml aprobado):
                // 
                // 1. <Signature> DEBE TENER su namespace xmlns="http://www.w3.org/2000/09/xmldsig#"
                // 2. <Signature> debe estar FUERA de </DE>, como hijo de <rDE>, después de </DE>
                // 3. NO hay <gCamGen /> vacío en el XML aprobado
                //
                // Estructura correcta (xmlRequestVenta_273_sync.xml):
                //   <rDE>
                //     <dVerFor>150</dVerFor>
                //     <DE Id="...">
                //       ... contenido ...
                //       <gTotSub>...</gTotSub>
                //     </DE>                           ← </DE> cierra aquí
                //     <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
                //       ...                           ← FUERA de DE, hermano de DE
                //     </Signature>
                //     <gCamFuFD>...</gCamFuFD>         ← DESPUÉS de Signature
                //   </rDE>
                // ========================================================================
                
                // NO quitar el namespace de Signature - debe conservar xmlns="http://www.w3.org/2000/09/xmldsig#"
                Console.WriteLine($"[FIRMA] Namespace de Signature: {signature.GetAttribute("xmlns")}");
                
                // IMPORTAR la firma al documento - conservando su namespace XMLDSIG
                XmlNode importedSignature = doc.ImportNode(signature, true);
                
                Console.WriteLine($"[FIRMA INSERT] importedSignature != null: {importedSignature != null}");
                Console.WriteLine($"[FIRMA INSERT] importedSignature.OwnerDocument == doc: {importedSignature.OwnerDocument == doc}");
                
                // INSERTAR Signature como hijo de rDE (doc.DocumentElement), DESPUÉS de DE
                // Posición: rDE > DE > (fin), luego Signature, luego gCamFuFD
                var rDE = doc.DocumentElement;
                if (rDE == null)
                    throw new InvalidOperationException("CRÍTICO: No se encontró elemento raíz rDE");
                
                // Buscar gCamFuFD para insertar Signature ANTES de él
                var gCamFuFDNode = doc.GetElementsByTagName("gCamFuFD").Cast<XmlNode>().FirstOrDefault();
                
                if (gCamFuFDNode != null)
                {
                    // Insertar Signature ANTES de gCamFuFD (después de DE)
                    rDE.InsertBefore(importedSignature, gCamFuFDNode);
                    Console.WriteLine("[FIRMA INSERT] Signature insertado ANTES de gCamFuFD, como hijo de rDE");
                }
                else
                {
                    // Si no hay gCamFuFD, insertar Signature después de DE
                    rDE.InsertAfter(importedSignature, node);
                    Console.WriteLine("[FIRMA INSERT] Signature insertado después de DE (no hay gCamFuFD)");
                }
                
                // VERIFICAR QUE SE INSERTÓ (ahora debe estar en rDE, no en node)
                bool firmaInsertada = importedSignature.ParentNode == rDE;
                Console.WriteLine($"[FIRMA INSERT] ✅ Firma insertada en rDE: {firmaInsertada}");
                
                if (!firmaInsertada)
                {
                    throw new InvalidOperationException("CRÍTICO: La firma NO se insertó en <rDE>");
                }
                
                // VERIFICAR contando nodos Signature en el documento
                var signaturesPostInsert = doc.GetElementsByTagName("Signature");
                Console.WriteLine($"[FIRMA INSERT] Cantidad de <Signature> después de insertar: {signaturesPostInsert.Count}");
                
                // GUARDAR XML FIRMADO A ARCHIVO PARA DEBUG
                try
                {
                    var debugPath = @"c:\asis\SistemIA\Debug\xml_firmado_verificacion.xml";
                    File.WriteAllText(debugPath, doc.OuterXml, new UTF8Encoding(false));
                    Console.WriteLine($"[FIRMA INSERT] XML firmado guardado en: {debugPath}");
                }
                catch (Exception exSave) { Console.WriteLine($"[FIRMA INSERT] Error guardando: {exSave.Message}"); }

                var digestValue = doc.GetElementsByTagName("DigestValue")
                    .Cast<XmlNode>()
                    .Select(n => StringToHex(n.InnerText))
                    .FirstOrDefault() ?? string.Empty;
                
                // DEBUG: Mostrar conversión de DigestValue
                var digestValueBase64 = doc.GetElementsByTagName("DigestValue")
                    .Cast<XmlNode>()
                    .Select(n => n.InnerText)
                    .FirstOrDefault() ?? "";
                Console.WriteLine($"[QR DEBUG] DigestValue Base64: {digestValueBase64}");
                Console.WriteLine($"[QR DEBUG] DigestValue HEX: {digestValue}");

                foreach (XmlNode qrNode in doc.GetElementsByTagName("dCarQR"))
                {
                    if (qrNode == null) continue;
                        // Reemplazar placeholder de DigestValue por el valor real
                        var qrText = qrNode.InnerText.Replace(
                            "665569394474586a4f4a396970724970754f344c434a75706a457a73645766664846656d573270344c69593d",
                            digestValue);

                        // Si no inicia con http, prefijar urlQR (base provista por config)
                        if (!qrText.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            qrText = $"{urlQR}{qrText.TrimStart('?')}";
                        }

                        // Extraer CSC del parámetro temporal __CSC__ (agregado por DEXmlBuilder)
                        string cscValue = "ABCD0000000000000000000000000000"; // Default TEST
                        // Buscar __CSC__ con & simple (como genera DEXmlBuilder ahora)
                        var cscMatch = System.Text.RegularExpressions.Regex.Match(qrText, @"__CSC__=([^&]+)");
                        if (cscMatch.Success)
                        {
                            cscValue = cscMatch.Groups[1].Value;
                            // Remover el parámetro temporal __CSC__ del QR final (con & simple)
                            qrText = System.Text.RegularExpressions.Regex.Replace(qrText, @"&__CSC__=[^&]+", "");
                        }

                        // ========================================================================
                        // FIX CRÍTICO 26-Ene-2026: Cálculo de cHashQR según librería Roshka Java
                        // Referencia: ManualSifen/codigoabierto/.../DocumentoElectronico.java
                        // Método: generateQRLink() líneas 380-417
                        //
                        // DESCUBRIMIENTO CRÍTICO: El hash se calcula SOLO con los parámetros,
                        // SIN incluir la URL base (https://...qr?)
                        //
                        // Java Roshka:
                        //   String urlParamsString = buildUrlParams(params); // Solo params
                        //   String cHashQR = sha256Hex(urlParamsString + csc);
                        //
                        // Verificado contra XML aprobado (protocolo 48493331):
                        //   Hash calculado con params+CSC = fa02c1ed... ✅ COINCIDE
                        //   Hash calculado con URL+CSC    = 2c87cc21... ❌ NO COINCIDE
                        // ========================================================================
                        
                        // Paso 1: Obtener URL completa hasta &cHashQR= (sin incluirlo)
                        string urlCompleta = qrText;
                        if (urlCompleta.Contains("&cHashQR="))
                        {
                            urlCompleta = urlCompleta.Substring(0, urlCompleta.IndexOf("&cHashQR="));
                        }
                        else if (urlCompleta.Contains("cHashQR="))
                        {
                            urlCompleta = urlCompleta.Substring(0, urlCompleta.IndexOf("cHashQR="));
                            if (urlCompleta.EndsWith("&")) 
                                urlCompleta = urlCompleta.TrimEnd('&');
                        }
                        
                        // Paso 2: CRÍTICO - Extraer SOLO los parámetros (sin URL base)
                        // El hash se calcula sobre "nVersion=150&Id=..." NO sobre "https://...?nVersion=..."
                        string soloParametros = urlCompleta;
                        int indexInterrogacion = urlCompleta.IndexOf('?');
                        if (indexInterrogacion >= 0)
                        {
                            soloParametros = urlCompleta.Substring(indexInterrogacion + 1);
                        }
                        
                        // Paso 3: Concatenar CSC directamente al final (sin &)
                        // El CSC va PEGADO a los parámetros para calcular el hash
                        string datosParaHash = soloParametros + cscValue;
                        
                        // Paso 4: SHA256 de la concatenación
                        var qrHash = SHA256ToString(datosParaHash);
                        
                        // La URL final para el QR mantiene la estructura completa
                        string urlSinHash = urlCompleta;
                        
                        // Reconstruir QR con el hash calculado
                        qrText = urlSinHash + "&cHashQR=" + qrHash;
                        
                        // DEBUG: Mostrar cálculo del QR
                        Console.WriteLine($"[QR DEBUG] URL completa: {urlCompleta.Substring(0, Math.Min(80, urlCompleta.Length))}...");
                        Console.WriteLine($"[QR DEBUG] Solo parámetros: {soloParametros.Substring(0, Math.Min(80, soloParametros.Length))}...");
                        Console.WriteLine($"[QR DEBUG] CSC usado: {cscValue}");
                        Console.WriteLine($"[QR DEBUG] Hash calculado (params+CSC): {qrHash}");
                        
                        // ========================================================================
                        // FIX 14-Ene-2026 (CORREGIDO 14-Ene-2026 noche):
                        // El XML APROBADO por PowerBuilder usa &amp; (escape SIMPLE, NO doble)
                        // CDC aprobado: 01004952197001002000006112026011410720743237
                        // ========================================================================
                        // XmlWriter automáticamente escapa & → &amp; al serializar
                        // Por lo tanto, el InnerText debe tener & literal (sin escapar)
                        // Resultado en XML: &amp; (correcto)
                        // ========================================================================
                        qrNode.InnerText = qrText;
                }

                // ========================================================================
                // VERIFICACIÓN CRÍTICA: Confirmar que Signature está en el XML
                // ========================================================================
                var signaturesInDoc = doc.GetElementsByTagName("Signature");
                Console.WriteLine($"[FIRMA VERIFICACION] Cantidad de <Signature> en doc: {signaturesInDoc.Count}");
                if (signaturesInDoc.Count > 0)
                {
                    var sigNode = signaturesInDoc[0];
                    Console.WriteLine($"[FIRMA VERIFICACION] Signature padre: {sigNode.ParentNode?.Name}");
                    Console.WriteLine($"[FIRMA VERIFICACION] Signature namespace: {sigNode.NamespaceURI}");
                    Console.WriteLine($"[FIRMA VERIFICACION] Signature tiene SignatureValue: {sigNode.SelectSingleNode("//*[local-name()='SignatureValue']") != null}");
                }
                else
                {
                    Console.WriteLine("[FIRMA VERIFICACION] ❌ ERROR CRÍTICO: NO HAY <Signature> EN EL DOCUMENTO!");
                }
                
                // ========================================================================
                // FIX CRÍTICO: Serializar XML con encoding UTF-8 explícito
                // ========================================================================
                // PROBLEMA: doc.OuterXml puede corromper caracteres UTF-8 como "í" → "├¡"
                // SOLUCIÓN: Usar XmlWriter con UTF8Encoding explícito (sin BOM)
                // FIX 14-Ene-2026 NOCHE: OmitXmlDeclaration = false
                // El XML aprobado de PowerBuilder TIENE declaración XML
                // CDC aprobado: 01004952197001002000006112026011410720743237
                // ========================================================================
                string xmlContent;
                using (var ms = new MemoryStream())
                {
                    using (var writer = XmlWriter.Create(ms, new XmlWriterSettings
                    {
                        Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                        Indent = false,
                        OmitXmlDeclaration = false  // 14-Ene-2026 NOCHE: CON declaración XML (igual que PowerBuilder)
                    }))
                    {
                        doc.WriteTo(writer);
                        writer.Flush();
                    }
                    ms.Position = 0;
                    using var reader = new StreamReader(ms, Encoding.UTF8);
                    xmlContent = reader.ReadToEnd();
                }
                
                // ========================================================================
                // FIX CRÍTICO 16-Ene-2026: Quitar namespace XMLDSIG de <Signature>
                // ========================================================================
                // PROBLEMA: .NET SignedXml.GetXml() genera <Signature xmlns="http://www.w3.org/2000/09/xmldsig#">
                // SIFEN backend (Java JAXB) RECHAZA este namespace externo dentro del DE
                // SOLUCIÓN: Quitar el xmlns de Signature para que herede el namespace SIFEN del padre
                // Referencia: XML aprobado en producción NO tiene este namespace en Signature
                // ========================================================================
                const string nsXmlDsig = " xmlns=\"http://www.w3.org/2000/09/xmldsig#\"";
                if (xmlContent.Contains(nsXmlDsig))
                {
                    xmlContent = xmlContent.Replace(nsXmlDsig, "");
                    Console.WriteLine("[FIRMA] ✅ Quitado namespace XMLDSIG de <Signature>");
                }
                
                // ========================================================================
                // VERIFICACIÓN FINAL: El XML serializado debe contener <Signature>
                // ========================================================================
                if (xmlContent.Contains("<Signature"))
                {
                    Console.WriteLine("[FIRMA VERIFICACION] ✅ XML serializado CONTIENE <Signature>");
                    // Mostrar posición de la firma en el XML
                    var posSignature = xmlContent.IndexOf("<Signature");
                    var posDE = xmlContent.IndexOf("</DE>");
                    Console.WriteLine($"[FIRMA VERIFICACION] Posición <Signature>: {posSignature}, Posición </DE>: {posDE}");
                    if (posSignature < posDE)
                    {
                        Console.WriteLine("[FIRMA VERIFICACION] ✅ Signature está ANTES de </DE> (CORRECTO)");
                    }
                    else
                    {
                        Console.WriteLine("[FIRMA VERIFICACION] ❌ Signature está DESPUÉS de </DE> (INCORRECTO)");
                    }
                }
                else
                {
                    Console.WriteLine("[FIRMA VERIFICACION] ❌ ERROR: XML serializado NO contiene <Signature>!");
                    // Guardar XML para debug
                    LogSifenDebug("XML SIN FIRMA - ERROR CRÍTICO", xmlContent);
                }
                
                // El XML válido de SIFEN (producción) usa &amp; simple en dCarQR, NO doble escape
                
                // ========================================================================
                // FIX 31-Ene-2026: FORMATO CORRECTO CONFIRMADO con datos reales del DLL
                // ========================================================================
                // Los datos de ejecución de Datostxt/xml_firmado.txt muestran CLARAMENTE que:
                //   <rLoteDE><rDE xmlns="...">...(firma)...</rDE></rLoteDE>
                // 
                // El XML firmado DEBE estar envuelto en <rLoteDE> antes de comprimirse.
                // Esto fue confirmado con la respuesta exitosa 0300 "Lote recibido con éxito"
                // ========================================================================
                string variante = "Power_DLL_format_rLoteDE";
                
                // Envolver el XML firmado en <rLoteDE> ANTES de comprimir
                var xmlConWrapper = $"<rLoteDE>{xmlContent}</rLoteDE>";
                var zippedXml = StringToZip(xmlConWrapper);
                
                // Generar dId único
                var dIdValue = string.IsNullOrWhiteSpace(dId) 
                    ? DateTime.Now.ToString("yyyyMMddHHmmss") + (Environment.TickCount % 100).ToString("00") 
                    : dId!;
                
                // Envelope SOAP exacto del DLL de Power
                var finalXml = $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><rEnvioLote xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dIdValue}</dId><xDE>{zippedXml}</xDE></rEnvioLote></soap:Body></soap:Envelope>";

                var result = await Enviar(url, finalXml, p12FilePath, certificatePassword);
                
                // TEMPORALMENTE DESHABILITADO: Probar solo variante ZIP para diagnóstico
                // Las variantes fallback estaban confundiendo el diagnóstico
                // TODO: Reactivar fallbacks una vez que la variante principal funcione
                
                /*
                // Si 0160, probar alternativas automáticamente: vista y wrapper alternativo rEnvioLote
                bool Es0160(string s)
                {
                    try
                    {
                        var c1 = GetTagValue(s, "ns2:dCodRes");
                        if (!string.IsNullOrWhiteSpace(c1) && c1 != "Tag not found." && c1 == "0160") return true;
                        var c2 = GetTagValue(s, "dCodRes");
                        return c2 == "0160";
                    }
                    catch { return false; }
                }

                if (Es0160(result))
                {
                    // Alternativa A: rEnviLoteDe vista (xDE con rLoteDE sin zip/base64)
                    variante = "rEnviLoteDe_vista";
                    finalXml = ConstruirSoapEnvioLoteVista(xmlContent, dId);
                    var resultA = await Enviar(url, finalXml, p12FilePath, certificatePassword);
                    if (Es0160(resultA))
                    {
                        // Alternativa B: wrapper rEnvioLote zip
                        variante = "rEnvioLote_zip";
                        finalXml = ConstruirSoapEnvioLoteZipBase64_EnvioLote(xmlContent, dId);
                        var resultB = await Enviar(url, finalXml, p12FilePath, certificatePassword);
                        if (Es0160(resultB))
                        {
                            // Alternativa C: wrapper rEnvioLote vista
                            variante = "rEnvioLote_vista";
                            finalXml = ConstruirSoapEnvioLoteVista_EnvioLote(xmlContent, dId);
                            var resultC = await Enviar(url, finalXml, p12FilePath, certificatePassword);
                            // Usar el último resultado
                            result = resultC;
                        }
                        else
                        {
                            result = resultB;
                        }
                    }
                    else
                    {
                        result = resultA;
                    }
                }
                */

                // Extraer idLote de forma robusta (varias variantes de etiqueta)
                string idLote = GetTagValue(result, "ns2:dProtConsLote");
                if (string.IsNullOrWhiteSpace(idLote) || idLote == "Tag not found.") idLote = GetTagValue(result, "dProtConsLote");
                if (string.IsNullOrWhiteSpace(idLote) || idLote == "Tag not found.")
                {
                    try
                    {
                        // 1) Variante directa con o sin prefijo
                        var m = System.Text.RegularExpressions.Regex.Match(result, @"<(?:\w+:)?dProtConsLote>([^<]+)</(?:\w+:)?dProtConsLote>");
                        if (m.Success) idLote = m.Groups[1].Value;
                        // 2) Variante genérica: cualquier etiqueta que contenga 'Prot' y 'Lote' en ese orden
                        if (string.IsNullOrWhiteSpace(idLote) || idLote == "Tag not found.")
                        {
                            var m2 = System.Text.RegularExpressions.Regex.Match(result, @"<(?:\w+:)?d\w*Prot\w*Lote>([^<]+)</(?:\w+:)?d\w*Prot\w*Lote>");
                            if (m2.Success) idLote = m2.Groups[1].Value;
                            else
                            {
                                // 3) Algunos entornos devuelven como <dProt> en rProtDe
                                var m3 = System.Text.RegularExpressions.Regex.Match(result, @"<(?:\w+:)?dProt>([^<]+)</(?:\w+:)?dProt>");
                                if (m3.Success) idLote = m3.Groups[1].Value;
                            }
                        }
                    }
                    catch { /* ignore */ }
                }

                // Extraer CDC si viene ya aceptado
                string cdc = GetTagValue(result, "ns2:dCDC");
                if (string.IsNullOrWhiteSpace(cdc) || cdc == "Tag not found.") cdc = GetTagValue(result, "dCDC");

                var codigo = GetTagValue(result, "ns2:dCodRes");
                var mensaje = GetTagValue(result, "ns2:dMsgRes");
                // Sanitizar para JSON simple (incluyendo saltos de línea)
                string esc(string s) => (s ?? string.Empty)
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n")
                    .Replace("\t", "\\t");

                return $"{{\"codigo\":\"{esc(codigo)}\",\"mensaje\":\"{esc(mensaje)}\",\"qr\":\"{esc(digestValue)}\",\"idLote\":\"{esc(idLote)}\",\"cdc\":\"{esc(cdc)}\",\"variante\":\"{esc(variante)}\",\"documento\":\"{esc(finalXml)}\",\"respuesta\":\"{esc(result)}\"}}";
            }
            catch (Exception ex)
            {
                return $"{{\"codigo\":\"10002\",\"mensaje\":\"{ex.Message}\",\"qr\":\"null\",\"idLote\":\"null\",\"documento\":\"null\"}}";
            }
        }

    // Construye un SOAP rEnvioLote con xDE/rLoteDE/rDE (vista expandida, sin zip/base64) a partir de un rDE firmado
    public string ConstruirSoapEnvioLoteVista(string xmlRdeFirmado, string? dId = null)
        {
            if (string.IsNullOrWhiteSpace(xmlRdeFirmado)) throw new ArgumentException("xmlRdeFirmado vacío");

            var rdeDoc = new XmlDocument();
            rdeDoc.LoadXml(xmlRdeFirmado);
            var rdeNode = rdeDoc.DocumentElement ?? throw new InvalidOperationException("rDE raíz no encontrado");

            var soapDoc = new XmlDocument();
            var soapNs = "http://www.w3.org/2003/05/soap-envelope";
            var sifenNs = "http://ekuatia.set.gov.py/sifen/xsd";

            // IMPORTANTE: Usar prefijo "soap:" y NO incluir Header (formato probado que funciona)
            var envelope = soapDoc.CreateElement("soap", "Envelope", soapNs);
            soapDoc.AppendChild(envelope);
            var body = soapDoc.CreateElement("soap", "Body", soapNs);
            envelope.AppendChild(body);

            // Usar rEnvioLote (formato del código original que funciona)
            var req = soapDoc.CreateElement("rEnvioLote", sifenNs);
            body.AppendChild(req);

            var dIdNode = soapDoc.CreateElement("dId", sifenNs);
            dIdNode.InnerText = string.IsNullOrWhiteSpace(dId) ? DateTime.Now.ToString("yyyyMMddHHmmss") + (Environment.TickCount % 100).ToString("00") : dId!;
            req.AppendChild(dIdNode);

            var xde = soapDoc.CreateElement("xDE", sifenNs);
            req.AppendChild(xde);

            var rLote = soapDoc.CreateElement("rLoteDE", sifenNs);
            xde.AppendChild(rLote);

            var imported = soapDoc.ImportNode(rdeNode, true);
            rLote.AppendChild(imported);

            // CRÍTICO: Usar SerializeXmlToUtf8String para preservar caracteres UTF-8
            return SerializeXmlToUtf8String(soapDoc);
        }

        /// <summary>
        /// REPLICACIÓN EXACTA del DLL de Power Builder (Sifen_26/Sifen.cs).
        /// Este método genera el SOAP EXACTAMENTE como lo hace el código que SÍ FUNCIONA.
        /// FIX 12-Ene-2026: Copiado del DLL de Power Builder verificado en producción.
        /// </summary>
        /// <param name="xmlRdeFirmado">El XML del rDE ya firmado (con Signature y gCamFuFD)</param>
        /// <param name="dId">ID del envío (opcional, se genera automáticamente)</param>
        /// <returns>SOAP completo listo para enviar al endpoint LOTE</returns>
        public string ConstruirSoapComoPoweBuilder(string xmlRdeFirmado, string? dId = null)
        {
            // =====================================================================
            // CÓDIGO IDÉNTICO AL DLL DE POWER BUILDER (Sifen_26/Sifen.cs líneas 155-222)
            // =====================================================================
            
            // 1. Generar dId si no se proporciona
            string dIdFinal = string.IsNullOrWhiteSpace(dId) 
                ? DateTime.Now.ToString("yyyyMMddHHmmss") + (Environment.TickCount % 100).ToString("00") 
                : dId!;
            
            // 2. Comprimir el XML firmado con GZip y convertir a Base64
            //    Power Builder comprime el rDE COMPLETO (con <rDE>...</rDE>), NO rLoteDE
            string base64Gzip = StringToZip(xmlRdeFirmado);
            
            // 3. Construir el sobre SOAP EXACTAMENTE como Power Builder
            //    Power Builder usa string concatenation simple, NO XmlDocument
            string soapEnvelope = 
                "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\">" +
                "<soap:Body>" +
                "<rEnvioLote xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\">" +
                "<dId>" + dIdFinal + "</dId>" +
                "<xDE>" + base64Gzip + "</xDE>" +
                "</rEnvioLote>" +
                "</soap:Body>" +
                "</soap:Envelope>";
            
            Console.WriteLine($"[SIFEN] ConstruirSoapComoPoweBuilder: dId={dIdFinal}, GZip={base64Gzip.Length} chars");
            
            return soapEnvelope;
        }

        // Construye el SOAP rEnvioLote con xDE como Base64(ZIP(<rLoteDE><rDE/>...</rLoteDE>)) listo para enviar
        // NOTA: SIFEN requiere archivo ZIP real (application/zip), NO gzip
        // IMPORTANTE: Siguiendo el formato EXACTO del DLL Power Builder que FUNCIONA:
        // - rLoteDE SIN namespace (solo nombre local)
        // - SIN declaración XML en el contenido comprimido
        // - CON indentación (tabs)
    public string ConstruirSoapEnvioLoteZipBase64(string xmlRdeFirmado, string? dId = null)
        {
            if (string.IsNullOrWhiteSpace(xmlRdeFirmado)) throw new ArgumentException("xmlRdeFirmado vacío");

            // ========================================================================
            // FIX 31-Ene-2026: Formato EXACTO según DLL Power que FUNCIONA
            // ========================================================================
            // El DLL de Power genera (VERIFICADO del archivo sifen_xml_input.txt):
            // <rLoteDE>
            // \t<rDE xmlns="http://ekuatia.set.gov.py/sifen/xsd" xmlns:xsi="..." xsi:schemaLocation="...">
            //     ...elementos...
            // \t</rDE>
            // </rLoteDE>
            //
            // IMPORTANTE:
            // - SIN declaración XML (<?xml version="1.0"?>)
            // - rLoteDE SIN namespace (solo nombre local)
            // - CON tabulación al inicio del rDE
            // ========================================================================
            
            var inner = new XmlDocument();
            // FIX 31-Ene-2026: NO agregar declaración XML - Power DLL no la tiene
            // var declInner = inner.CreateXmlDeclaration("1.0", "UTF-8", null);
            // inner.AppendChild(declInner);
            
            // CRÍTICO: rLoteDE SIN namespace (solo nombre local)
            var rLote = inner.CreateElement("rLoteDE"); // SIN namespace
            inner.AppendChild(rLote);

            // Cargar el rDE firmado
            var rdeDoc = new XmlDocument();
            rdeDoc.LoadXml(xmlRdeFirmado);
            var rdeNode = rdeDoc.DocumentElement ?? throw new InvalidOperationException("rDE raíz no encontrado");
            
            // FIX 31-Ene-2026: SIN whitespace adicional - Power DLL no tiene \n\t entre rLoteDE y rDE
            // El XML del Power DLL es: <rLoteDE><rDE xmlns="...">...</rDE></rLoteDE> (todo en una línea)
            
            // Importar el nodo rDE - el namespace se preservará del original (xmlns="http://ekuatia.set.gov.py/sifen/xsd")
            var imported = inner.ImportNode(rdeNode, true);
            rLote.AppendChild(imported);
            
            // SIN whitespace adicional

            // Comprimir el contenido XML SIN declaración
            // CRÍTICO: Usar SerializeXmlToUtf8String CON omitDeclaration=true (Power DLL no tiene declaración)
            var zipped = StringToZip(SerializeXmlToUtf8String(inner, indent: false, omitDeclaration: true));

            // ========================================================================
            // FIX 31-Ene-2026: Formato SOAP exacto del DLL Power que FUNCIONA
            // ========================================================================
            var dIdValue = string.IsNullOrWhiteSpace(dId) 
                ? DateTime.Now.ToString("yyyyMMddHHmmss") + (Environment.TickCount % 100).ToString("00") 
                : dId!;
            
            // ========================================================================
            // FIX 31-Ene-2026: Formato SOAP exacto del DLL Power que FUNCIONA
            // ========================================================================
            // El DLL de Power usa este formato exacto (sin soap:Header, con namespace por defecto):
            // <soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
            //   <soap:Body>
            //     <rEnvioLote xmlns="http://ekuatia.set.gov.py/sifen/xsd">
            //       <dId>160420241700</dId>
            //       <xDE>ZIP_BASE64</xDE>
            //     </rEnvioLote>
            //   </soap:Body>
            // </soap:Envelope>
            // ========================================================================
            var soapString = $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><rEnvioLote xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dIdValue}</dId><xDE>{zipped}</xDE></rEnvioLote></soap:Body></soap:Envelope>";
            
            return soapString;
        }

        // Variante alternativa: mismo payload (zip+base64) pero wrapper rEnvioLote
        public string ConstruirSoapEnvioLoteZipBase64_EnvioLote(string xmlRdeFirmado, string? dId = null)
        {
            if (string.IsNullOrWhiteSpace(xmlRdeFirmado)) throw new ArgumentException("xmlRdeFirmado vacío");

            // CRÍTICO: rLoteDE SIN namespace pero CON declaración XML (Java Transformer lo hace)
            var inner = new XmlDocument();
            // Agregar declaración XML - Java Transformer lo hace por defecto
            var declInner = inner.CreateXmlDeclaration("1.0", "UTF-8", null);
            inner.AppendChild(declInner);
            var rLote = inner.CreateElement("rLoteDE"); // SIN namespace
            inner.AppendChild(rLote);

            var rdeDoc = new XmlDocument();
            rdeDoc.LoadXml(xmlRdeFirmado);
            var rdeNode = rdeDoc.DocumentElement ?? throw new InvalidOperationException("rDE raíz no encontrado");
            var imported = inner.ImportNode(rdeNode, true);
            rLote.AppendChild(imported);

            // CRÍTICO: Usar SerializeXmlToUtf8String en lugar de OuterXml para preservar UTF-8
            var zipped = StringToZip(SerializeXmlToUtf8String(inner));

            // El wrapper SOAP SÍ usa namespace SIFEN
            var sifenNs = "http://ekuatia.set.gov.py/sifen/xsd";
            var soapDoc = new XmlDocument();
            var decl = soapDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            soapDoc.AppendChild(decl);
            var soapNs = "http://www.w3.org/2003/05/soap-envelope";
            var envelope = soapDoc.CreateElement("soap", "Envelope", soapNs);
            soapDoc.AppendChild(envelope);
            var body = soapDoc.CreateElement("soap", "Body", soapNs);
            envelope.AppendChild(body);
            var req = soapDoc.CreateElement("rEnvioLote", sifenNs);
            body.AppendChild(req);
            var dIdNode = soapDoc.CreateElement("dId", sifenNs);
            dIdNode.InnerText = string.IsNullOrWhiteSpace(dId) ? DateTime.Now.ToString("yyyyMMddHHmmss") + (Environment.TickCount % 100).ToString("00") : dId!;
            req.AppendChild(dIdNode);
            var xde = soapDoc.CreateElement("xDE", sifenNs);
            xde.InnerText = zipped;
            req.AppendChild(xde);

            // Usar SerializeXmlToUtf8String por consistencia
            return SerializeXmlToUtf8String(soapDoc);
        }

        // Variante alternativa: wrapper rEnvioLote en modo vista (xDE contiene rLoteDE crudo)
        public string ConstruirSoapEnvioLoteVista_EnvioLote(string xmlRdeFirmado, string? dId = null)
        {
            if (string.IsNullOrWhiteSpace(xmlRdeFirmado)) throw new ArgumentException("xmlRdeFirmado vacío");

            var rdeDoc = new XmlDocument();
            rdeDoc.LoadXml(xmlRdeFirmado);
            var rdeNode = rdeDoc.DocumentElement ?? throw new InvalidOperationException("rDE raíz no encontrado");

            // IMPORTANTE: Usar prefijo "soap:" y NO incluir Header (formato probado que funciona)
            var soapDoc = new XmlDocument();
            var soapNs = "http://www.w3.org/2003/05/soap-envelope";
            var sifenNs = "http://ekuatia.set.gov.py/sifen/xsd";

            var envelope = soapDoc.CreateElement("soap", "Envelope", soapNs);
            soapDoc.AppendChild(envelope);
            var body = soapDoc.CreateElement("soap", "Body", soapNs);
            envelope.AppendChild(body);

            var req = soapDoc.CreateElement("rEnvioLote", sifenNs);
            body.AppendChild(req);

            var dIdNode = soapDoc.CreateElement("dId", sifenNs);
            dIdNode.InnerText = string.IsNullOrWhiteSpace(dId) ? DateTime.Now.ToString("yyyyMMddHHmmss") + (Environment.TickCount % 100).ToString("00") : dId!;
            req.AppendChild(dIdNode);

            var xde = soapDoc.CreateElement("xDE", sifenNs);
            req.AppendChild(xde);

            // CRÍTICO: rLoteDE SIN namespace (como en Java: addChildElement("rLoteDE") sin QName)
            var rLote = soapDoc.CreateElement("rLoteDE");
            xde.AppendChild(rLote);

            var imported = soapDoc.ImportNode(rdeNode, true);
            rLote.AppendChild(imported);

            // CRÍTICO: Usar SerializeXmlToUtf8String para preservar caracteres UTF-8
            return SerializeXmlToUtf8String(soapDoc);
        }

        public async Task<string> Consulta(string url, string id, string tipoConsulta, string p12FilePath, string certificatePassword)
        {
            // IMPORTANTE: Formato probado que funciona con SIFEN
            // - Usar prefijo "soap:" (no "env:")
            // - NO incluir <soap:Header/>
            // - dId puede ser un valor simple como "1"
            // - XML compacto sin espacios extra ni indentación
            var consulta = tipoConsulta switch
            {
                "1" => $@"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""><soap:Body><rEnviConsRUC xmlns=""http://ekuatia.set.gov.py/sifen/xsd""><dId>1</dId><dRUCCons>{id}</dRUCCons></rEnviConsRUC></soap:Body></soap:Envelope>",
                "2" => $@"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""><soap:Body><rEnviConsDeRequest xmlns=""http://ekuatia.set.gov.py/sifen/xsd""><dId>1</dId><dCDC>{id}</dCDC></rEnviConsDeRequest></soap:Body></soap:Envelope>",
                "3" => $@"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""><soap:Body><rEnviConsLoteDe xmlns=""http://ekuatia.set.gov.py/sifen/xsd""><dId>1</dId><dProtConsLote>{id}</dProtConsLote></rEnviConsLoteDe></soap:Body></soap:Envelope>",
                _ => throw new ArgumentException("Tipo de consulta no válido")
            };

            return await Enviar(url, consulta, p12FilePath, certificatePassword);
        }

        // Firma el DE y devuelve el XML firmado y el paquete SOAP listo para enviar, sin realizar el POST
        public string FirmarSinEnviar(string urlQR, string xmlString, string p12FilePath, string certificatePassword, string tipoFirmado = "1", bool devolverBase64Zip = true)
        {
            var doc = new XmlDocument();
            // FIX CRÍTICO: PreserveWhitespace para canonicalización correcta
            doc.PreserveWhitespace = true;
            doc.LoadXml(xmlString);

            // Eliminar cualquier firma existente (reemplazar por nuestra firma)
            try
            {
                var toRemove = doc.SelectNodes("//*[local-name()='Signature']");
                if (toRemove != null)
                {
                    foreach (XmlNode n in toRemove)
                    {
                        n.ParentNode?.RemoveChild(n);
                    }
                }
            }
            catch { /* mejor esfuerzo */ }

            var node = doc.GetElementsByTagName(tipoFirmado == "1" ? "DE" : "rEve")[0]
                ?? throw new InvalidOperationException("No se encontró el nodo a firmar");

            var idAttribute = node.Attributes?["Id"]
                ?? throw new InvalidOperationException("No se encontró el atributo Id");
            var nodeId = idAttribute.Value;

            using var cert = new X509Certificate2(p12FilePath, certificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
            using var rsa = cert.GetRSAPrivateKey()
                ?? throw new InvalidOperationException("No se pudo obtener la clave RSA");

            var signedXml = new SignedXmlWithId(doc) { SigningKey = rsa };
            var reference = new Reference
            {
                Uri = "#" + nodeId,
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
            
            // ========================================================================
            // FIX 16-Ene-2026 v2: Signature FUERA de DE, con namespace XMLDSIG
            // ========================================================================
            // CORRECCIÓN basada en xmlRequestVenta_273_sync.xml aprobado:
            // - <Signature> va FUERA de </DE>, como hermano
            // - <Signature> DEBE tener xmlns="http://www.w3.org/2000/09/xmldsig#"
            // - NO hay <gCamGen /> vacío
            // ========================================================================
            
            // 1) ELIMINAR gCamGen vacío si existe (no está en XML de referencia)
            var gCamGenNodes = doc.GetElementsByTagName("gCamGen");
            foreach (XmlNode gCamGenNode in gCamGenNodes)
            {
                if (gCamGenNode != null && !gCamGenNode.HasChildNodes)
                {
                    gCamGenNode.ParentNode?.RemoveChild(gCamGenNode);
                    Console.WriteLine("[FIRMA LOTE] gCamGen vacío ELIMINADO");
                }
            }
            
            // 2) Asegurar que gCamFuFD esté FUERA de DE
            var rdeNode = doc.DocumentElement;
            if (rdeNode != null)
            {
                var gCamFuFDNodes = doc.GetElementsByTagName("gCamFuFD");
                if (gCamFuFDNodes.Count > 0)
                {
                    var gCamFuFD = gCamFuFDNodes[0];
                    if (gCamFuFD != null && gCamFuFD.ParentNode != rdeNode)
                    {
                        gCamFuFD.ParentNode?.RemoveChild(gCamFuFD);
                        rdeNode.AppendChild(gCamFuFD);
                        Console.WriteLine("[FIRMA LOTE] gCamFuFD movido fuera de DE");
                    }
                }
            }
            
            // 3) Insertar Signature FUERA de DE, como hijo de rDE
            // ========================================================================
            // FIX 16-Ene-2026 v2: Signature FUERA de </DE>, CON namespace XMLDSIG
            // ========================================================================
            Console.WriteLine("[FIRMA LOTE INSERT] === INICIO INSERCIÓN ===");
            Console.WriteLine($"[FIRMA LOTE INSERT] node.LocalName = '{node.LocalName}'");
            
            if (node.LocalName != "DE")
                throw new InvalidOperationException($"[FIRMA LOTE] node NO es DE, es '{node.LocalName}'");
            
            if (signature == null)
                throw new InvalidOperationException("[FIRMA LOTE] signature es NULL - ComputeSignature() falló");
            
            // NO quitar namespace - Signature DEBE tener xmlns="http://www.w3.org/2000/09/xmldsig#"
            Console.WriteLine($"[FIRMA LOTE] Namespace de Signature: {signature.GetAttribute("xmlns")}");
            
            XmlNode importedSignature = doc.ImportNode(signature, true);
            Console.WriteLine($"[FIRMA LOTE INSERT] importedSignature.OwnerDocument == doc? {importedSignature.OwnerDocument == doc}");
            
            if (importedSignature.OwnerDocument != doc)
                throw new InvalidOperationException("[FIRMA LOTE] importedSignature NO pertenece al documento");
            
            // INSERTAR Signature como hijo de rDE (FUERA de DE), antes de gCamFuFD
            if (rdeNode == null)
                throw new InvalidOperationException("[FIRMA LOTE] rDE es null");
            
            var gCamFuFDNode = doc.GetElementsByTagName("gCamFuFD").Cast<XmlNode>().FirstOrDefault();
            
            if (gCamFuFDNode != null)
            {
                rdeNode.InsertBefore(importedSignature, gCamFuFDNode);
                Console.WriteLine("[FIRMA LOTE] Signature insertado ANTES de gCamFuFD");
            }
            else
            {
                rdeNode.InsertAfter(importedSignature, node);
                Console.WriteLine("[FIRMA LOTE] Signature insertado después de DE");
            }
            
            // Verificación POST-inserción
            bool firmaInsertada = importedSignature.ParentNode == rdeNode;
            Console.WriteLine($"[FIRMA LOTE INSERT] ✅ Firma insertada en rDE: {firmaInsertada}");
            
            if (!firmaInsertada)
                throw new InvalidOperationException("[FIRMA LOTE] La firma NO quedó como hijo de rDE");
            
            Console.WriteLine("[FIRMA LOTE INSERT] === FIN INSERCIÓN ===");

            // Reemplazar Digest en dCarQR y calcular cHashQR
            var digestValue = doc.GetElementsByTagName("DigestValue")
                .Cast<XmlNode>()
                .Select(n => StringToHex(n.InnerText))
                .FirstOrDefault() ?? string.Empty;

            foreach (XmlNode qrNode in doc.GetElementsByTagName("dCarQR"))
            {
                if (qrNode == null) continue;
                var qrText = qrNode.InnerText.Replace(
                    "665569394474586a4f4a396970724970754f344c434a75706a457a73645766664846656d573270344c69593d",
                    digestValue);
                if (!qrText.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    qrText = $"{urlQR}{qrText.TrimStart('?')}";
                }
                
                // Extraer CSC del parámetro temporal __CSC__ (agregado por DEXmlBuilder)
                string cscValue = "ABCD0000000000000000000000000000"; // Default TEST
                // Buscar __CSC__ con & simple (como genera DEXmlBuilder ahora)
                var cscMatch = System.Text.RegularExpressions.Regex.Match(qrText, @"__CSC__=([^&]+)");
                if (cscMatch.Success)
                {
                    cscValue = cscMatch.Groups[1].Value;
                    // Remover el parámetro temporal __CSC__ del QR final (con & simple)
                    qrText = System.Text.RegularExpressions.Regex.Replace(qrText, @"&__CSC__=[^&]+", "");
                }

                // ========================================================================
                // FIX CRÍTICO 26-Ene-2026: Cálculo de cHashQR según librería Roshka Java
                // El hash se calcula SOLO con los parámetros, SIN incluir la URL base
                // Referencia: ManualSifen/codigoabierto/.../DocumentoElectronico.java
                // ========================================================================
                string urlCompleta = qrText;
                // Buscar &cHashQR= con & simple (como viene de DEXmlBuilder)
                if (urlCompleta.Contains("&cHashQR="))
                {
                    urlCompleta = urlCompleta.Substring(0, urlCompleta.IndexOf("&cHashQR="));
                }
                else if (urlCompleta.Contains("cHashQR="))
                {
                    urlCompleta = urlCompleta.Substring(0, urlCompleta.IndexOf("cHashQR="));
                    if (urlCompleta.EndsWith("&"))
                        urlCompleta = urlCompleta.TrimEnd('&');
                }
                
                // CRÍTICO: Extraer SOLO los parámetros (sin URL base)
                string soloParametros = urlCompleta;
                int indexInterrogacion = urlCompleta.IndexOf('?');
                if (indexInterrogacion >= 0)
                {
                    soloParametros = urlCompleta.Substring(indexInterrogacion + 1);
                }
                
                // Calcular hash sobre parámetros + CSC (sin URL base)
                var qrHash = SHA256ToString(soloParametros + cscValue);
                
                // Construir URL final con el hash calculado
                qrText = urlCompleta + "&cHashQR=" + qrHash;
                // ========================================================================
                // FIX 14-Ene-2026 (CORREGIDO 14-Ene-2026 noche):
                // El XML APROBADO por PowerBuilder usa &amp; (escape SIMPLE, NO doble)
                // CDC aprobado: 01004952197001002000006112026011410720743237
                // ========================================================================
                // XmlWriter automáticamente escapa & → &amp; al serializar
                // Por lo tanto, el InnerText debe tener & literal (sin escapar)
                // Resultado en XML: &amp; (correcto)
                // ========================================================================
                qrNode.InnerText = qrText;
            }

            // ========================================================================
            // FIX CRÍTICO: Serializar XML con encoding UTF-8 explícito
            // ========================================================================
            // PROBLEMA: doc.OuterXml puede corromper caracteres UTF-8 como "í" → "├¡"
            // SOLUCIÓN: Usar XmlWriter con UTF8Encoding explícito (sin BOM)
            // FIX 14-Ene-2026 NOCHE: CON declaración XML (igual que PowerBuilder aprobado)
            // ========================================================================
            string xmlFirmado;
            using (var ms = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(ms, new XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    Indent = false,
                    OmitXmlDeclaration = false  // 14-Ene-2026 NOCHE: CON declaración XML
                }))
                {
                    doc.WriteTo(writer);
                    writer.Flush();
                }
                ms.Position = 0;
                using var reader = new StreamReader(ms, Encoding.UTF8);
                xmlFirmado = reader.ReadToEnd();
            }
            
            // ========================================================================
            // FIX 16-Ene-2026: QUITAR NAMESPACE XMLDSIG DE <Signature>
            // ========================================================================
            // SIFEN Paraguay NO acepta el namespace estándar XMLDSIG en <Signature>.
            // El backend SIFEN (parser Java JAXB) requiere que <Signature> herede
            // el namespace del documento padre (http://ekuatia.set.gov.py/sifen/xsd).
            //
            // El método QuitarNamespaceRecursivo() no funciona completamente porque
            // XmlDocument mantiene el namespace original al serializar.
            // SOLUCIÓN: Reemplazar el namespace directamente en el string XML.
            // ========================================================================
            const string nsXmlDsig = " xmlns=\"http://www.w3.org/2000/09/xmldsig#\"";
            if (xmlFirmado.Contains(nsXmlDsig))
            {
                Console.WriteLine("[FIRMA FIX NS STRING] Quitando namespace XMLDSIG del string XML");
                xmlFirmado = xmlFirmado.Replace(nsXmlDsig, "");
                Console.WriteLine($"[FIRMA FIX NS STRING] Namespace XMLDSIG encontrado y eliminado: {!xmlFirmado.Contains(nsXmlDsig)}");
            }
            
            // El XML válido de SIFEN (producción) usa &amp; simple en dCarQR, NO doble escape
            
            if (tipoFirmado == "1" && devolverBase64Zip)
            {
                // ========================================================================
                // FIX CRÍTICO 10-Ene-2026: Para endpoint SYNC (recibe.wsdl), el XML va DIRECTO
                // ========================================================================
                // IMPORTANTE: Según análisis de librerías oficiales (Java, PHP, TypeScript):
                // - SYNC (recibe.wsdl)  → XML SIN comprimir, directamente dentro de <xDE>
                // - ASYNC (recibe-lote.wsdl) → XML comprimido en ZIP + Base64
                //
                // La librería PHP lo confirma en sifen.php línea 502:
                //   $soapEnvelope = '...
                //     <rEnviDe xmlns="http://ekuatia.set.gov.py/sifen/xsd">
                //       <dId>25</dId>
                //       <xDE>' . $contenidoXML . '</xDE>  <-- XML directo, SIN comprimir
                //     </rEnviDe>...'
                //
                // La librería Java (DocumentoElectronico.java línea 255) también agrega
                // el XML como elemento SOAP hijo, no como texto Base64.
                // ========================================================================
                
                // Generar dId de 16 dígitos (yyyyMMddHHmmssNN)
                var dId = DateTime.Now.ToString("yyyyMMddHHmmss") + (Environment.TickCount % 100).ToString("00");
                
                // El contenido de xDE es el XML del rDE directamente (SIN ZIP, SIN Base64)
                // xmlFirmado ya contiene <rDE>...</rDE> con firma y QR
                // ========================================================================
                // FIX CRÍTICO: Remover declaración XML antes de insertar en <xDE>
                // ========================================================================
                // PROBLEMA: <?xml version="1.0" encoding="utf-8"?> NO puede estar anidado
                //           dentro de otro XML - causa "XML Mal Formado" (error 0160)
                // SOLUCIÓN: Remover la declaración XML dejando solo el elemento <rDE>
                // ========================================================================
                var xmlContent = xmlFirmado;
                if (xmlContent.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
                {
                    // Encontrar el fin de la declaración XML (el primer ?>)
                    var endDecl = xmlContent.IndexOf("?>", StringComparison.Ordinal);
                    if (endDecl > 0)
                    {
                        xmlContent = xmlContent.Substring(endDecl + 2).TrimStart();
                    }
                }
                
                // ========================================================================
                // FIX 14-Ene-2026: Formato SOAP EXACTO del archivo xmlRequestVenta_273_sync.xml
                // que SIFEN confirmó que está correcto (correo de Jonathan Garay, Analista SIFEN)
                // ========================================================================
                // El archivo .ai-docs/SIFEN/respuesta_correoSifen/xmlRequestVenta_273_sync.xml
                // muestra el formato EXACTO que SIFEN recibe y valida correctamente:
                //
                // <?xml version="1.0" encoding="UTF-8" standalone="no"?>
                // <env:Envelope xmlns:env="http://www.w3.org/2003/05/soap-envelope">
                //   <env:Header/>
                //   <env:Body>
                //     <rEnviDe xmlns="http://ekuatia.set.gov.py/sifen/xsd">
                //       <dId>123456</dId>
                //       <xDE>
                //         <rDE xmlns="..." ...>...</rDE>
                //       </xDE>
                //     </rEnviDe>
                //   </env:Body>
                // </env:Envelope>
                //
                // CONFIRMADO también por librería PHP (sifen.php líneas 497-508):
                //   - Prefijo "env:" (SOAP 1.2 estándar)
                //   - Declaración XML con standalone="no" (opcional pero recomendado)
                //   - Header vacío <env:Header/>
                //   - Body con <env:Body>
                // ========================================================================
                // ========================================================================
                // FIX 15-Ene-2026: Formato SOAP según diagnóstico de ChatGPT
                // ========================================================================
                // CAMBIOS CRÍTICOS:
                // 1. Usar prefijo "soap:" en lugar de "env:"
                // 2. NO incluir <soap:Header/> 
                // 3. Sin espacios/indentación para evitar problemas de parsing
                // 
                // ERRORES QUE CAUSABAN 0160:
                // ❌ <?xml version="1.0"?> dentro de <xDE>
                // ❌ ZIP/Base64 en endpoint SYNC
                // ❌ <rLoteDE> en endpoint SYNC
                // ❌ CDATA en <xDE>
                // ❌ <soap:Header/> vacío (algunos parsers lo rechazan)
                // ========================================================================
                
                // Validaciones pre-envío (diagnóstico ChatGPT)
                if (xmlContent.Contains("<?xml"))
                    throw new Exception("SIFEN: XML declaration detectada dentro del rDE - esto causa error 0160");
                
                if (!xmlContent.Contains("<Signature"))
                    throw new Exception("SIFEN: Firma NO presente en el XML");
                    
                if (!xmlContent.Contains("<gCamGen"))
                    throw new Exception("SIFEN: gCamGen faltante en el XML");
                
                // Verificar que Signature está DENTRO de DE (no después)
                var posSig = xmlContent.IndexOf("<Signature");
                var posEndDE = xmlContent.IndexOf("</DE>");
                if (posSig > posEndDE)
                    throw new Exception("SIFEN: Signature está FUERA de DE - debe estar dentro");
                
                // Formato SOAP corregido según ChatGPT (15-Ene-2026)
                var soap = $@"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"">
  <soap:Body>
    <rEnviDe xmlns=""http://ekuatia.set.gov.py/sifen/xsd"">
      <dId>{dId}</dId>
      <xDE>{xmlContent}</xDE>
    </rEnviDe>
  </soap:Body>
</soap:Envelope>";
                return soap;
            }
            return xmlFirmado;
        }

        /// <summary>
        /// Genera diferentes variantes de SOAP para probar cuál acepta SIFEN.
        /// Útil para debugging del error 0160 "XML Mal Formado".
        /// </summary>
        /// <param name="xmlFirmado">El XML del DE firmado (resultado de FirmarSinEnviar con devolverBase64Zip=false)</param>
        /// <param name="variante">Número de variante a probar (1-10)</param>
        /// <returns>Tupla con (soapGenerado, descripcionVariante)</returns>
        public (string soap, string descripcion) GenerarSoapVariante(string xmlFirmado, int variante)
        {
            // Generar dId de 16 dígitos
            var dId = DateTime.Now.ToString("yyyyMMddHHmmss") + (Environment.TickCount % 100).ToString("00");
            
            // Preparar el contenido XML (remover declaración XML si existe)
            var xmlContent = xmlFirmado;
            if (xmlContent.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            {
                var endDecl = xmlContent.IndexOf("?>", StringComparison.Ordinal);
                if (endDecl > 0)
                {
                    xmlContent = xmlContent.Substring(endDecl + 2).TrimStart();
                }
            }
            
            // Definir las variantes
            switch (variante)
            {
                case 1:
                    // Variante 1: Actual (env: + declaración XML)
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Header/><env:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlContent}</xDE></rEnviDe></env:Body></env:Envelope>",
                        "env: + declaración XML + XML directo"
                    );
                    
                case 2:
                    // Variante 2: soap: en vez de env: (como algunos ejemplos usan)
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Header/><soap:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlContent}</xDE></rEnviDe></soap:Body></soap:Envelope>",
                        "soap: + declaración XML + XML directo"
                    );
                    
                case 3:
                    // Variante 3: Sin declaración XML (env:)
                    return (
                        $"<env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Header/><env:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlContent}</xDE></rEnviDe></env:Body></env:Envelope>",
                        "env: SIN declaración XML + XML directo"
                    );
                    
                case 4:
                    // Variante 4: Sin declaración XML (soap:)
                    return (
                        $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Header/><soap:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlContent}</xDE></rEnviDe></soap:Body></soap:Envelope>",
                        "soap: SIN declaración XML + XML directo"
                    );
                    
                case 5:
                    // Variante 5: SOAP 1.1 (namespace diferente)
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Header/><soap:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlContent}</xDE></rEnviDe></soap:Body></soap:Envelope>",
                        "SOAP 1.1 (schemas.xmlsoap.org) + declaración XML"
                    );
                    
                case 6:
                    // Variante 6: env: con namespace del rEnviDe como atributo xsi (algunas libs usan esto)
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:xsd=\"http://ekuatia.set.gov.py/sifen/xsd\"><env:Header/><env:Body><xsd:rEnviDe><dId>{dId}</dId><xDE>{xmlContent}</xDE></xsd:rEnviDe></env:Body></env:Envelope>",
                        "env: con prefijo xsd: para rEnviDe"
                    );
                    
                case 7:
                    // Variante 7: Usar CDATA para el contenido del xDE
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Header/><env:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE><![CDATA[{xmlContent}]]></xDE></rEnviDe></env:Body></env:Envelope>",
                        "env: + declaración XML + CDATA en xDE"
                    );
                    
                case 8:
                    // Variante 8: env: sin Header (algunos servidores lo rechazan)
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlContent}</xDE></rEnviDe></env:Body></env:Envelope>",
                        "env: + declaración XML + SIN Header"
                    );
                    
                case 9:
                    // Variante 9: Igual a la librería Java de Roshka (estructura exacta)
                    // SOAPElement con namespace default heredado
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://www.w3.org/2003/05/soap-envelope\"><SOAP-ENV:Header/><SOAP-ENV:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlContent}</xDE></rEnviDe></SOAP-ENV:Body></SOAP-ENV:Envelope>",
                        "SOAP-ENV: (mayúsculas como Java) + declaración XML"
                    );
                    
                case 10:
                    // Variante 10: Con xDE comprimido en ZIP+Base64 (modo LOTE pero en endpoint SYNC)
                    // Por si SIFEN espera el ZIP incluso en sync
                    var zippedB64 = StringToZip(xmlFirmado); // xmlFirmado original CON declaración
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Header/><env:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{zippedB64}</xDE></rEnviDe></env:Body></env:Envelope>",
                        "env: + declaración XML + xDE con ZIP+Base64 (como lote)"
                    );
                    
                case 11:
                    // Variante 11: Quitar xmlns del rDE (hereda del padre rEnviDe)
                    // El problema puede ser que rDE redefine el mismo namespace
                    var xmlSinNs = RemoverNamespaceDeRDE(xmlContent);
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Header/><env:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlSinNs}</xDE></rEnviDe></env:Body></env:Envelope>",
                        "env: + rDE SIN namespace (hereda de rEnviDe)"
                    );
                    
                case 12:
                    // Variante 12: Quitar xmlns Y xsi:schemaLocation del rDE
                    var xmlSinNsYSchema = RemoverNamespaceYSchemaDeRDE(xmlContent);
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Header/><env:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlSinNsYSchema}</xDE></rEnviDe></env:Body></env:Envelope>",
                        "env: + rDE SIN namespace NI schemaLocation"
                    );
                    
                case 13:
                    // Variante 13: schemaLocation con HTTPS en vez de HTTP
                    var xmlConHttps = xmlContent.Replace(
                        "xsi:schemaLocation=\"http://ekuatia.set.gov.py/sifen/xsd siRecepDE_v150.xsd\"",
                        "xsi:schemaLocation=\"https://ekuatia.set.gov.py/sifen/xsd siRecepDE_v150.xsd\"");
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Header/><env:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlConHttps}</xDE></rEnviDe></env:Body></env:Envelope>",
                        "env: + schemaLocation con HTTPS"
                    );
                    
                case 14:
                    // Variante 14: Sin schemaLocation completamente
                    var xmlSinSchema = RemoverSchemaLocationDeRDE(xmlContent);
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Header/><env:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlSinSchema}</xDE></rEnviDe></env:Body></env:Envelope>",
                        "env: + rDE SIN schemaLocation (pero con namespace)"
                    );
                    
                case 15:
                    // Variante 15: Solo xmlns:xsi sin schemaLocation
                    var xmlSoloXsi = RemoverSchemaLocationMantenerXsi(xmlContent);
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Header/><env:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlSoloXsi}</xDE></rEnviDe></env:Body></env:Envelope>",
                        "env: + xmlns:xsi pero SIN schemaLocation"
                    );
                    
                case 16:
                    // ========================================================================
                    // Variante 16: FORMATO EXACTO DEL MANUAL TÉCNICO v150 (página 36-37)
                    // ========================================================================
                    // Diferencias clave con variantes anteriores:
                    // 1. <soap:body> con b MINÚSCULA (no Body)
                    // 2. Sin <?xml ...?> declaration
                    // 3. Namespace en rEnviDe, NO en Envelope
                    // ========================================================================
                    return (
                        $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Header/><soap:body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlContent}</xDE></rEnviDe></soap:body></soap:Envelope>",
                        "MANUAL TÉCNICO v150: soap:body minúscula + SIN declaración XML"
                    );
                    
                case 17:
                    // Variante 17: Manual Técnico CON declaración XML
                    return (
                        $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Header/><soap:body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlContent}</xDE></rEnviDe></soap:body></soap:Envelope>",
                        "MANUAL TÉCNICO v150: soap:body minúscula + CON declaración XML"
                    );
                    
                case 18:
                    // Variante 18: Manual Técnico sin Header (algunos servidores lo rechazan vacío)
                    return (
                        $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlContent}</xDE></rEnviDe></soap:body></soap:Envelope>",
                        "MANUAL TÉCNICO v150: soap:body minúscula + SIN Header"
                    );
                    
                case 19:
                    // Variante 19: Igual al Manual pero env: en vez de soap:
                    return (
                        $"<env:Envelope xmlns:env=\"http://www.w3.org/2003/05/soap-envelope\"><env:Header/><env:body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlContent}</xDE></rEnviDe></env:body></env:Envelope>",
                        "env:body minúscula (variante Manual Técnico)"
                    );
                    
                case 20:
                    // Variante 20: Body mayúscula pero namespace xsd: como PDF Mejores Prácticas
                    return (
                        $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:xsd=\"http://ekuatia.set.gov.py/sifen/xsd\"><soap:Header/><soap:Body><xsd:rEnviDe><xsd:dId>{dId}</xsd:dId><xsd:xDE>{xmlContent}</xsd:xDE></xsd:rEnviDe></soap:Body></soap:Envelope>",
                        "soap:Body MAYÚSCULA + prefijo xsd: (PDF Mejores Prácticas)"
                    );
                    
                case 21:
                    // ========================================================================
                    // Variante 21: FORMATO EXACTO DEL DLL DECOMPILADO (Sifen.dll funcional)
                    // ========================================================================
                    // Este es el formato que usa la librería DLL que SÍ FUNCIONA.
                    // Diferencias clave del DLL (confirmado por decompilación):
                    // 1. **CON** <?xml version="1.0" encoding="UTF-8"?> declaration (xmlDocument.OuterXml lo incluye)
                    // 2. SIN <soap:Header/>
                    // 3. Usa rEnvioLote (NO rEnviDe) - PARA ENDPOINT LOTE
                    // 4. El contenido xDE va comprimido en ZIP+Base64
                    // 5. Body con B mayúscula
                    // 6. ZIP entry name: "compressed.txt" (ya implementado)
                    // ========================================================================
                    {
                        // CRÍTICO 31-Ene-2026: El DLL usa xmlDocument.OuterXml que INCLUYE la declaración XML
                        // Nuestro FirmarSinEnviar usa OmitXmlDeclaration=true, así que debemos agregarla manualmente
                        var xmlConDeclaracion = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + xmlFirmado;
                        var zippedContent = StringToZip(xmlConDeclaracion);
                        return (
                            $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><rEnvioLote xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{zippedContent}</xDE></rEnvioLote></soap:Body></soap:Envelope>",
                            "DLL DECOMPILADO: rEnvioLote + ZIP (CON declaración XML) + SIN Header (ENDPOINT LOTE)"
                        );
                    }
                    
                case 22:
                    // Variante 22: Igual al DLL pero para endpoint SYNC (rEnviDe en vez de rEnvioLote)
                    // SIN ZIP, XML directo
                    return (
                        $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlContent}</xDE></rEnviDe></soap:Body></soap:Envelope>",
                        "DLL DECOMPILADO (adaptado SYNC): rEnviDe + XML directo + SIN Header + SIN declaración"
                    );
                    
                case 23:
                    // Variante 23: DLL + Content-Type exacto (solo "application/xml" sin charset)
                    // Esta variante usa el mismo SOAP que 22 pero indica que el Content-Type debe ser diferente
                    return (
                        $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><rEnviDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{xmlContent}</xDE></rEnviDe></soap:Body></soap:Envelope>",
                        "DLL DECOMPILADO: Content-Type=application/xml (SIN charset)"
                    );
                    
                case 24:
                    // ========================================================================
                    // Variante 24: COPIA EXACTA del DLL funcional
                    // - Mismo dId hardcodeado: 160420241700 (como en el DLL decompilado)
                    // - CON declaración XML (xmlDocument.OuterXml lo incluye)
                    // - ZIP con archivo "compressed.txt"
                    // - LOTE endpoint
                    // ========================================================================
                    {
                        var xmlConDecl24 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + xmlFirmado;
                        var zip24 = StringToZip(xmlConDecl24);
                        return (
                            $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><rEnvioLote xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>160420241700</dId><xDE>{zip24}</xDE></rEnvioLote></soap:Body></soap:Envelope>",
                            "DLL EXACTO: dId fijo 160420241700 + XML declaration + ZIP + LOTE"
                        );
                    }
                    
                case 25:
                    // ========================================================================
                    // Variante 25: LOTE con wrapper <rLoteDE> (formato documentación Java)
                    // El ZIP contiene: <rLoteDE><rDE xmlns="...">...</rDE></rLoteDE>
                    // SIN declaración XML en el contenido del ZIP
                    // ========================================================================
                    {
                        // Crear el wrapper rLoteDE SIN namespace (como indica la doc Java)
                        var loteXml25 = $"<rLoteDE>{xmlFirmado}</rLoteDE>";
                        var zip25 = StringToZip(loteXml25);
                        return (
                            $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><rEnvioLote xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{zip25}</xDE></rEnvioLote></soap:Body></soap:Envelope>",
                            "LOTE JAVA: <rLoteDE> wrapper + SIN declaración + ZIP + LOTE endpoint"
                        );
                    }
                    
                case 26:
                    // ========================================================================
                    // Variante 26: LOTE con wrapper <rLoteDE> + declaración XML
                    // El ZIP contiene: <?xml...?><rLoteDE><rDE xmlns="...">...</rDE></rLoteDE>
                    // ========================================================================
                    {
                        var loteXml26 = $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><rLoteDE>{xmlFirmado}</rLoteDE>";
                        var zip26 = StringToZip(loteXml26);
                        return (
                            $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><rEnvioLote xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{zip26}</xDE></rEnvioLote></soap:Body></soap:Envelope>",
                            "LOTE JAVA: <?xml?> + <rLoteDE> wrapper + ZIP + LOTE endpoint"
                        );
                    }
                    
                case 27:
                    // ========================================================================
                    // Variante 27: EXACTO como DLL pero con dId dinámico
                    // - CON declaración XML (xmlDocument.OuterXml lo incluye)
                    // - SIN wrapper <rLoteDE>
                    // - ZIP con archivo "compressed.txt"
                    // - dId dinámico (no hardcodeado)
                    // ========================================================================
                    {
                        var xmlConDecl27 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + xmlFirmado;
                        var zip27 = StringToZip(xmlConDecl27);
                        return (
                            $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><rEnvioLote xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{zip27}</xDE></rEnvioLote></soap:Body></soap:Envelope>",
                            "DLL con dId dinámico: <?xml?> + ZIP + LOTE endpoint"
                        );
                    }
                    
                case 28:
                    // ========================================================================
                    // Variante 28: DLL exacto con formato dId de 12 dígitos (DDMMYYYYHHMM)
                    // - El DLL usa formato "160420241700" = DDMMYYYYHHMM
                    // - Nuestro dId usa YYYYMMDDHHMMSSNN (16 dígitos)
                    // ========================================================================
                    {
                        // Generar dId con formato DLL: DDMMYYYYHHMM (12 dígitos)
                        var dId12 = DateTime.Now.ToString("ddMMyyyyHHmm");
                        var xmlConDecl28 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + xmlFirmado;
                        var zip28 = StringToZip(xmlConDecl28);
                        return (
                            $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><rEnvioLote xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId12}</dId><xDE>{zip28}</xDE></rEnvioLote></soap:Body></soap:Envelope>",
                            $"DLL exacto con dId12 ({dId12}): DDMMYYYYHHMM + <?xml?> + ZIP + LOTE"
                        );
                    }
                    
                default:
                    return (string.Empty, $"Variante {variante} no definida");
            }
        }

        /// <summary>
        /// Prueba automáticamente todas las variantes de SOAP hasta encontrar una que SIFEN acepte.
        /// </summary>
        public async Task<(bool exito, int varianteExitosa, string descripcion, string codigo, string mensaje, string soapEnviado, string respuesta)> 
            ProbarVariantesAsync(string xmlFirmado, string urlEnvio, string p12Path, string p12Pass, int maxVariantes = 20)
        {
            for (int i = 1; i <= maxVariantes; i++)
            {
                var (soap, desc) = GenerarSoapVariante(xmlFirmado, i);
                if (string.IsNullOrEmpty(soap)) continue;
                
                Console.WriteLine($"\n[SIFEN PRUEBA] ========== VARIANTE {i}: {desc} ==========");
                Console.WriteLine($"[SIFEN PRUEBA] Longitud SOAP: {soap.Length} caracteres");
                
                try
                {
                    var resp = await Enviar(urlEnvio, soap, p12Path, p12Pass);
                    
                    string codigo = GetTagValue(resp, "ns2:dCodRes");
                    if (string.IsNullOrWhiteSpace(codigo) || codigo == "Tag not found.") 
                        codigo = GetTagValue(resp, "dCodRes");
                    
                    string mensaje = GetTagValue(resp, "ns2:dMsgRes");
                    if (string.IsNullOrWhiteSpace(mensaje) || mensaje == "Tag not found.") 
                        mensaje = GetTagValue(resp, "dMsgRes");
                    
                    Console.WriteLine($"[SIFEN PRUEBA] Código: {codigo}, Mensaje: {mensaje}");
                    
                    // Si el código NO es 0160 (XML mal formado), puede ser éxito o error diferente
                    if (codigo != "0160" && codigo != "Tag not found.")
                    {
                        // Código 0260 = Aprobado, pero también otros códigos pueden indicar que el XML fue aceptado
                        bool esExito = codigo == "0260" || codigo == "0300" || codigo == "0362" || 
                                       !mensaje.Contains("mal formado", StringComparison.OrdinalIgnoreCase);
                        
                        Console.WriteLine($"[SIFEN PRUEBA] ✅ Variante {i} procesada sin error 0160!");
                        return (esExito, i, desc, codigo, mensaje, soap, resp);
                    }
                    
                    Console.WriteLine($"[SIFEN PRUEBA] ❌ Variante {i} rechazada con error 0160");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SIFEN PRUEBA] ⚠️ Variante {i} error de conexión: {ex.Message}");
                }
                
                // Esperar un poco entre intentos para no sobrecargar el servidor
                await Task.Delay(500);
            }
            
            return (false, 0, "Ninguna variante fue aceptada", "0160", "XML Mal Formado en todas las variantes", string.Empty, string.Empty);
        }

        // ========== Métodos de consulta con resultados estructurados ==========

        private readonly IConfiguration? _configuration;

        public Sifen() { }

        public Sifen(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Consulta los datos de un contribuyente por su RUC
        /// </summary>
        public async Task<(bool exito, RespuestaConsultaRUC? respuesta, string? error, string? xmlRespuesta)> ConsultarRUC(string ruc)
        {
            try
            {
                if (_configuration == null)
                    return (false, null, "Configuración no disponible", null);

                var ambiente = _configuration["Sifen:Ambiente"] ?? "test";
                var rutaCertificado = _configuration["Sifen:RutaCertificado"] ?? "";
                var passwordCertificado = _configuration["Sifen:PasswordCertificado"] ?? "";

                if (string.IsNullOrEmpty(rutaCertificado) || !File.Exists(rutaCertificado))
                    return (false, null, $"Certificado no encontrado: {rutaCertificado}", null);

                if (string.IsNullOrEmpty(passwordCertificado))
                    return (false, null, "Password del certificado no configurado", null);

                var baseUrl = ambiente == "prod"
                    ? "https://sifen.set.gov.py/de/ws"
                    : "https://sifen-test.set.gov.py/de/ws";
                var url = $"{baseUrl}/consultas/consulta-ruc.wsdl";

                Console.WriteLine($"[ConsultarRUC] Consultando RUC: {ruc}");
                Console.WriteLine($"[ConsultarRUC] URL: {url}");
                Console.WriteLine($"[ConsultarRUC] Ambiente: {ambiente}");

                var xmlRespuesta = await Consulta(url, ruc, "1", rutaCertificado, passwordCertificado);

                Console.WriteLine($"[ConsultarRUC] Respuesta recibida ({xmlRespuesta?.Length ?? 0} bytes)");

                if (string.IsNullOrEmpty(xmlRespuesta))
                    return (false, null, "Respuesta vacía del servidor", null);

                // Parsear la respuesta
                var respuesta = ParsearRespuestaRUC(xmlRespuesta);
                
                return (respuesta != null, respuesta, respuesta == null ? "Error al parsear respuesta" : null, xmlRespuesta);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConsultarRUC] Error: {ex.Message}");
                return (false, null, ex.Message, null);
            }
        }

        private RespuestaConsultaRUC? ParsearRespuestaRUC(string xml)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                var nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("ns2", "http://ekuatia.set.gov.py/sifen/xsd");

                var respuesta = new RespuestaConsultaRUC();

                // Buscar dCodRes y dMsgRes
                var codRes = doc.SelectSingleNode("//ns2:dCodRes", nsmgr) ?? doc.GetElementsByTagName("dCodRes")[0];
                var msgRes = doc.SelectSingleNode("//ns2:dMsgRes", nsmgr) ?? doc.GetElementsByTagName("dMsgRes")[0];

                respuesta.dCodRes = codRes?.InnerText ?? "";
                respuesta.dMsgRes = msgRes?.InnerText ?? "";

                // Si hay datos del contribuyente (código 0502 = encontrado)
                if (respuesta.dCodRes == "0502")
                {
                    var xContRUC = doc.SelectSingleNode("//ns2:xContRUC", nsmgr) ?? doc.GetElementsByTagName("xContRUC")[0];
                    if (xContRUC != null)
                    {
                        respuesta.xContRUC = new DatosContribuyente
                        {
                            dRUC = GetNodeValue(xContRUC, "dRUC"),
                            dDV = GetNodeValue(xContRUC, "dDV"),
                            dRazSoc = GetNodeValue(xContRUC, "dRazSoc"),
                            dNomFan = GetNodeValue(xContRUC, "dNomFan"),
                            dDesEstCont = GetNodeValue(xContRUC, "dDesEstCont"),
                            dDesTipCont = GetNodeValue(xContRUC, "dDesTipCont"),
                            dDesRegimen = GetNodeValue(xContRUC, "dDesRegimen")
                        };
                    }
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ParsearRespuestaRUC] Error: {ex.Message}");
                return null;
            }
        }

        private string GetNodeValue(XmlNode parent, string tagName)
        {
            var node = parent.SelectSingleNode($".//*[local-name()='{tagName}']");
            return node?.InnerText ?? "";
        }

        // Clases para respuestas estructuradas
        public class RespuestaConsultaRUC
        {
            public string dCodRes { get; set; } = "";
            public string dMsgRes { get; set; } = "";
            public DatosContribuyente? xContRUC { get; set; }
        }

        public class DatosContribuyente
        {
            public string dRUC { get; set; } = "";
            public string dDV { get; set; } = "";
            public string dRazSoc { get; set; } = "";
            public string dNomFan { get; set; } = "";
            public string dDesEstCont { get; set; } = "";
            public string dDesTipCont { get; set; } = "";
            public string dDesRegimen { get; set; } = "";
        }
    }
}
