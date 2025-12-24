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

        public static string SHA256ToString(string s)
        {
            using var alg = SHA256.Create();
            byte[] hash = alg.ComputeHash(Encoding.UTF8.GetBytes(s));
            return Convert.ToHexString(hash).ToLower();
        }

        public static string StringToZip(string originalString)
        {
            using var memoryStream = new MemoryStream();
            using (var gzip = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                // Usar UTF-8 sin BOM para evitar bytes extra al inicio
                using var writer = new StreamWriter(gzip, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                writer.Write(originalString);
            }
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public string StringToHex(string hexstring)
        {
            return string.Concat(hexstring.Select(c => Convert.ToInt32(c).ToString("x2")));
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
                    else if (documento.Contains("<rEnviDeRequest", StringComparison.OrdinalIgnoreCase)) op = "rEnviDeRequest";
                    else if (documento.Contains("<rEnviConsLoteDe", StringComparison.OrdinalIgnoreCase)) { op = "rEnviConsLoteDe"; esConsulta = true; }
                    else if (documento.Contains("<rEnviConsDeRequest", StringComparison.OrdinalIgnoreCase)) { op = "rEnviConsDeRequest"; esConsulta = true; }
                    else if (documento.Contains("<rEnviConsRUC", StringComparison.OrdinalIgnoreCase)) { op = "rEnviConsRUC"; esConsulta = true; }
                }
                catch { /* noop */ }
                
                // IMPORTANTE: Según la implementación de referencia (facturacionelectronicapy-setapi),
                // las consultas usan Content-Type: application/xml; charset=utf-8
                // mientras que los envíos de DE usan application/soap+xml; charset=utf-8; action="..."
                string contentType;
                if (esConsulta)
                {
                    // Para consultas: application/xml; charset=utf-8 (según referencia)
                    contentType = "application/xml; charset=utf-8";
                }
                else
                {
                    // Para envíos DE: application/soap+xml con action
                    string? actionOp = op;
                    contentType = (actionOp == null)
                        ? "application/soap+xml; charset=utf-8"
                        : $"application/soap+xml; charset=utf-8; action=\"http://ekuatia.set.gov.py/sifen/xsd/{actionOp}\"";
                }
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
                
                using var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                
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

        public string FormatearXML(string XML)
        {
            if (string.IsNullOrWhiteSpace(XML)) return string.Empty;

            try
            {
                using var ms = new MemoryStream();
                using var writer = XmlWriter.Create(ms, new XmlWriterSettings { Indent = true });
                var doc = new XmlDocument();
                doc.LoadXml(XML);
                doc.WriteTo(writer);
                writer.Flush();
                ms.Position = 0;
                using var reader = new StreamReader(ms);
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
                // La firma debe ser ENVELOPED pero colocada FUERA de DE, como sibling en rDE
                var rdeNode = doc.DocumentElement;
                if (rdeNode != null)
                {
                    rdeNode.InsertAfter(doc.ImportNode(signature, true), node);
                }

                // Reubicar gCamFuFD: debe ser hermano de DE (bajo rDE), después de Signature
                try
                {
                    var gCamFuFDNodes = doc.GetElementsByTagName("gCamFuFD");
                    if (gCamFuFDNodes.Count > 0)
                    {
                        var gCamFuFD = gCamFuFDNodes[0];
                        if (gCamFuFD != null && gCamFuFD.ParentNode != null)
                        {
                            var parent = gCamFuFD.ParentNode;
                            parent.RemoveChild(gCamFuFD);
                            // Insertar después de Signature
                            var signatureNodes = doc.GetElementsByTagName("Signature");
                            if (signatureNodes.Count > 0)
                            {
                                var signatureNode = signatureNodes[0];
                                parent.InsertAfter(gCamFuFD, signatureNode);
                            }
                            else
                            {
                                // Fallback, después de DE
                                parent.InsertAfter(gCamFuFD, node);
                            }
                        }
                    }
                }
                catch { /* no crítico */ }

                var digestValue = doc.GetElementsByTagName("DigestValue")
                    .Cast<XmlNode>()
                    .Select(n => StringToHex(n.InnerText))
                    .FirstOrDefault() ?? string.Empty;

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

                        // Recalcular cHashQR
                        var qrHash = SHA256ToString(qrText);
                        if (qrText.Contains("cHashQR="))
                        {
                            qrText = System.Text.RegularExpressions.Regex.Replace(qrText, @"cHashQR=[A-Za-z0-9]+", $"cHashQR={qrHash}");
                        }
                        else
                        {
                            qrText += (qrText.Contains("?") ? "&" : "?") + $"cHashQR={qrHash}";
                        }

                        qrNode.InnerText = qrText;
                }

                var xmlContent = doc.OuterXml;
                // Construir SOAP de envío para recepción de lote (primero: xDE=Base64(GZip(rLoteDE)) con rEnviLoteDe)
                string variante = "rEnviLoteDe_zip";
                var finalXml = ConstruirSoapEnvioLoteZipBase64(xmlContent, dId);

                var result = await Enviar(url, finalXml, p12FilePath, certificatePassword);

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
                // Sanitizar para JSON simple
                string esc(string s) => (s ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");

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

            return soapDoc.OuterXml;
        }

        // Construye el SOAP rEnvioLote con xDE como Base64(GZip(<rLoteDE><rDE/>...</rLoteDE>)) listo para enviar
    public string ConstruirSoapEnvioLoteZipBase64(string xmlRdeFirmado, string? dId = null)
        {
            if (string.IsNullOrWhiteSpace(xmlRdeFirmado)) throw new ArgumentException("xmlRdeFirmado vacío");

            // Documento interno: rLoteDE con rDE importado
            var inner = new XmlDocument();
            var sifenNs = "http://ekuatia.set.gov.py/sifen/xsd";
            // Agregar declaración XML UTF-8 explícita
            var declInner = inner.CreateXmlDeclaration("1.0", "UTF-8", null);
            inner.AppendChild(declInner);
            var rLote = inner.CreateElement("rLoteDE", sifenNs);
            inner.AppendChild(rLote);

            var rdeDoc = new XmlDocument();
            rdeDoc.LoadXml(xmlRdeFirmado);
            var rdeNode = rdeDoc.DocumentElement ?? throw new InvalidOperationException("rDE raíz no encontrado");
            var imported = inner.ImportNode(rdeNode, true);
            rLote.AppendChild(imported);

            var zipped = StringToZip(inner.OuterXml);

            // SOAP externo con xDE en base64+gzip
            // IMPORTANTE: Usar prefijo "soap:" y NO incluir Header (formato probado que funciona)
            var soapDoc = new XmlDocument();
            var soapNs = "http://www.w3.org/2003/05/soap-envelope";
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
            xde.InnerText = zipped;
            req.AppendChild(xde);

            return soapDoc.OuterXml;
        }

        // Variante alternativa: mismo payload (zip+base64) pero wrapper rEnvioLote
        public string ConstruirSoapEnvioLoteZipBase64_EnvioLote(string xmlRdeFirmado, string? dId = null)
        {
            if (string.IsNullOrWhiteSpace(xmlRdeFirmado)) throw new ArgumentException("xmlRdeFirmado vacío");

            var inner = new XmlDocument();
            var sifenNs = "http://ekuatia.set.gov.py/sifen/xsd";
            var declInner = inner.CreateXmlDeclaration("1.0", "UTF-8", null);
            inner.AppendChild(declInner);
            var rLote = inner.CreateElement("rLoteDE", sifenNs);
            inner.AppendChild(rLote);

            var rdeDoc = new XmlDocument();
            rdeDoc.LoadXml(xmlRdeFirmado);
            var rdeNode = rdeDoc.DocumentElement ?? throw new InvalidOperationException("rDE raíz no encontrado");
            var imported = inner.ImportNode(rdeNode, true);
            rLote.AppendChild(imported);

            var zipped = StringToZip(inner.OuterXml);

            var soapDoc = new XmlDocument();
            var decl = soapDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            soapDoc.AppendChild(decl);
            var soapNs = "http://www.w3.org/2003/05/soap-envelope";
            // IMPORTANTE: Usar prefijo "soap:" y NO incluir Header (formato probado que funciona)
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

            return soapDoc.OuterXml;
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

            var rLote = soapDoc.CreateElement("rLoteDE", sifenNs);
            xde.AppendChild(rLote);

            var imported = soapDoc.ImportNode(rdeNode, true);
            rLote.AppendChild(imported);

            return soapDoc.OuterXml;
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
            // La firma debe ir como sibling de DE en rDE, no dentro de DE
            var rdeNode = doc.DocumentElement;
            if (rdeNode != null)
            {
                rdeNode.InsertAfter(doc.ImportNode(signature, true), node);
            }

            // Reubicar gCamFuFD a nivel de rDE (hermano de DE), después de Signature
            try
            {
                var gCamFuFDNodes = doc.GetElementsByTagName("gCamFuFD");
                if (gCamFuFDNodes.Count > 0)
                {
                    var gCamFuFD = gCamFuFDNodes[0];
                    if (gCamFuFD != null && gCamFuFD.ParentNode != null)
                    {
                        var parent = gCamFuFD.ParentNode;
                        parent.RemoveChild(gCamFuFD);
                        // Insertar después de Signature
                        var signatureNodes = doc.GetElementsByTagName("Signature");
                        if (signatureNodes.Count > 0)
                        {
                            var signatureNode = signatureNodes[0];
                            parent.InsertAfter(gCamFuFD, signatureNode);
                        }
                        else
                        {
                            // Fallback, después de DE
                            parent.InsertAfter(gCamFuFD, node);
                        }
                    }
                }
            }
            catch { /* no crítico */ }

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
                var qrHash = SHA256ToString(qrText);
                if (qrText.Contains("cHashQR="))
                    qrText = System.Text.RegularExpressions.Regex.Replace(qrText, @"cHashQR=[A-Za-z0-9]+", $"cHashQR={qrHash}");
                else
                    qrText += (qrText.Contains("?") ? "&" : "?") + $"cHashQR={qrHash}";
                qrNode.InnerText = qrText;
            }

            var xmlFirmado = doc.OuterXml;
            if (tipoFirmado == "1" && devolverBase64Zip)
            {
                // Para envío síncrono, SIFEN espera xDE = Base64(GZip(DE)) (solo el nodo DE), no rDE completo
                var deOnly = new XmlDocument();
                var declDe = deOnly.CreateXmlDeclaration("1.0", "UTF-8", null);
                deOnly.AppendChild(declDe);
                var importedDe = deOnly.ImportNode(node, true);
                deOnly.AppendChild(importedDe);
                var zipped = StringToZip(deOnly.OuterXml);
                // Generar dId de 16 dígitos (yyyyMMddHHmmssNN) como en el envío por lote
                var dId = DateTime.Now.ToString("yyyyMMddHHmmss") + (Environment.TickCount % 100).ToString("00");
                // IMPORTANTE: Usar prefijo "soap:" y NO incluir Header (formato probado que funciona)
                var soap = $"<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><rEnvioLote xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>{dId}</dId><xDE>{zipped}</xDE></rEnvioLote></soap:Body></soap:Envelope>";
                return soap;
            }
            return xmlFirmado;
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
