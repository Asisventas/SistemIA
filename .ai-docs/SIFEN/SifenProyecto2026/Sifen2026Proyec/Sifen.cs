using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Security.Cryptography;
using System.Net;
using System.IO.Compression;
using System.Security.Cryptography.Xml;
using System.Runtime.InteropServices;



namespace Sifen
{
    [Guid("b1d23d8e-4e7b-4dc8-81fb-bfdb47d7f521")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Sifen
    {
        private static string _logPath = @"C:\nextsys - GLP\sifen_log.txt";
        
        private static void Log(string message)
        {
            try
            {
                string logEntry = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + message + Environment.NewLine;
                File.AppendAllText(_logPath, logEntry);
            }
            catch { }
        }
        
        private static X509Certificate2 ImportCertificateToStore(string pfxPath, string password)
        {
            try
            {
                // Cargar certificado del archivo PFX
                X509Certificate2 pfxCert = new X509Certificate2(pfxPath, password, X509KeyStorageFlags.Exportable);
                string thumbprint = pfxCert.Thumbprint;
                
                Log("=== AUTO-REGISTRO CERTIFICADO ===");
                Log("Thumbprint del PFX: " + thumbprint);
                
                // Verificar si ya está en el almacén CurrentUser\My
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite);
                
                X509Certificate2Collection existingCerts = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                
                if (existingCerts.Count > 0)
                {
                    Log("Certificado YA REGISTRADO en CurrentUser\\My");
                    X509Certificate2 existingCert = existingCerts[0];
                    store.Close();
                    return existingCert;
                }
                else
                {
                    Log("Certificado NO encontrado en almacén. REGISTRANDO...");
                    
                    // Importar certificado al almacén con todas las propiedades necesarias
                    X509Certificate2 certToImport = new X509Certificate2(
                        pfxPath, 
                        password, 
                        X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable
                    );
                    
                    store.Add(certToImport);
                    store.Close();
                    
                    Log("Certificado REGISTRADO exitosamente en CurrentUser\\My");
                    Log("Subject: " + certToImport.Subject);
                    Log("Thumbprint: " + certToImport.Thumbprint);
                    
                    // Reabrir almacén y obtener el certificado registrado
                    X509Store store2 = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                    store2.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection registeredCerts = store2.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                    store2.Close();
                    
                    if (registeredCerts.Count > 0)
                    {
                        Log("Certificado verificado en almacén después de registro");
                        return registeredCerts[0];
                    }
                    else
                    {
                        Log("ADVERTENCIA: No se pudo recuperar del almacén, usando PFX directo");
                        return certToImport;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("ERROR en ImportCertificateToStore: " + ex.Message);
                Log("Usando certificado del archivo PFX directamente");
                return new X509Certificate2(pfxPath, password, X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);
            }
        }
        
        private static string SHA256ToString(string s)
        {
            using (var alg = SHA256.Create())
                return alg.ComputeHash(Encoding.UTF8.GetBytes(s)).Aggregate(new StringBuilder(), (sb, x) => sb.Append(x.ToString("x2"))).ToString();
        }

        private static string StringToZip(string originalString)
        {
            Log("=== INICIO COMPRESION ===");
            Log("XML Original Length: " + originalString.Length);
            Log("XML Original (primeros 500 chars): " + (originalString.Length > 500 ? originalString.Substring(0, 500) : originalString));
            
            byte[] compressedBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("compressed.txt", CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.Write(originalString);
                    }
                }
                compressedBytes = memoryStream.ToArray();
            }
            string base64Result = Convert.ToBase64String(compressedBytes);
            Log("Compressed Base64 Length: " + base64Result.Length);
            Log("=== FIN COMPRESION ===");
            return base64Result;
        }

        private string StringToHex(string hexstring)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char t in hexstring)
            {
                sb.Append(Convert.ToInt32(t).ToString("x2"));
            }
            return sb.ToString();
        }

        private string getTagValue(string xmlString, string tagName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            XmlNodeList nodeList = xmlDoc.GetElementsByTagName(tagName);
            if (nodeList.Count > 0)
            {
                string tagValue = nodeList[0].InnerText;
                return tagValue;
            }
            else
            {
                return "Tag not found.";
            }
        }

        private string Enviar(string url, string documento, string pathArchivoP12, string passwordArchivoP12)
        {
            string result = "";
            
            Log("=== INICIO ENVIO ===");
            Log("URL: " + url);
            Log("Documento Length: " + documento.Length);
            Log("Documento (primeros 1000 chars): " + (documento.Length > 1000 ? documento.Substring(0, 1000) : documento));
            
            try
            {
                // Configuración completa SSL/TLS para Windows 2025-2026
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.CheckCertificateRevocationList = false;
                ServicePointManager.DefaultConnectionLimit = 9999;
                
                // Cargar certificado con MachineKeySet para compatibilidad Schannel 2025-2026
                X509Certificate2 certificate = new X509Certificate2(
                    pathArchivoP12,
                    passwordArchivoP12,
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
                );
                Log("Certificado para ENVIO: " + certificate.Subject);
                Log("Certificado HasPrivateKey: " + certificate.HasPrivateKey);
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/xml";
                request.Timeout = 30000; // 30 segundos
                request.ClientCertificates.Add(certificate);
                
                byte[] xmlBytes = Encoding.UTF8.GetBytes(documento);
                request.ContentLength = xmlBytes.Length;
                Log("Enviando " + xmlBytes.Length + " bytes...");
                
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(xmlBytes, 0, xmlBytes.Length);
                }
                using (HttpWebResponse res = (HttpWebResponse)request.GetResponse())
                {
                    Log("Response StatusCode: " + res.StatusCode);
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream responseStream = res.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                            result = reader.ReadToEnd();
                        }
                    }
                    else
                    {
                        result = "HTTP_ERROR:" + res.StatusCode.ToString();
                    }
                }
            }
            catch (WebException ex)
            {
                Log("WebException: " + ex.Status + " - " + ex.Message);
                if (ex.Response != null)
                {
                    using (Stream responseStream = ex.Response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        result = reader.ReadToEnd();
                    }
                }
                else
                {
                    result = "WEBERROR:" + ex.Status.ToString() + " - " + ex.Message;
                }
            }
            catch (Exception ex)
            {
                Log("Exception: " + ex.GetType().Name + " - " + ex.Message);
                result = "ERROR:" + ex.GetType().Name + " - " + ex.Message;
            }
            
            Log("Response Length: " + result.Length);
            Log("Response (primeros 1000 chars): " + (result.Length > 1000 ? result.Substring(0, 1000) : result));
            Log("=== FIN ENVIO ===");
            return result;
        }

        public string FormatearXML(string XML)
        {
            string Result = "";
            MemoryStream MS = new MemoryStream();
            XmlTextWriter W = new XmlTextWriter(MS, Encoding.Unicode);
            XmlDocument D = new XmlDocument();
            try
            {
                D.LoadXml(XML);
                W.Formatting = Formatting.Indented;
                D.WriteContentTo(W);
                W.Flush();
                MS.Flush();
                MS.Position = 0;
                StreamReader SR = new StreamReader(MS);
                string FormattedXML = SR.ReadToEnd();
                Result = FormattedXML;
            }
            catch (XmlException)
            {
            }
            MS.Close();
            W.Close();
            return Result;
        }

        [return: MarshalAs(UnmanagedType.BStr)]
        public string firmarYEnviar(
            [MarshalAs(UnmanagedType.BStr)] string url, 
            [MarshalAs(UnmanagedType.BStr)] string urlQR, 
            [MarshalAs(UnmanagedType.BStr)] string xmlString, 
            [MarshalAs(UnmanagedType.BStr)] string p12FilePath, 
            [MarshalAs(UnmanagedType.BStr)] string certificatePassword, 
            [MarshalAs(UnmanagedType.BStr)] string tipoFirmado = "1")
        {
            string retorno = "";
            string startDocument = @"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""><soap:Body><rEnvioLote xmlns=""http://ekuatia.set.gov.py/sifen/xsd""><dId>160420241700</dId><xDE>";
            string endDocument = "</xDE></rEnvioLote></soap:Body></soap:Envelope>";
            try
            {
                Log("=== INICIO FIRMA ===");
                Log("=== VERSION DLL ===");
                Log("VERSION: Sifen_26 - CORREGIDA - Compatible Windows 10/11");
                Log("COMPILACION: 2026-01-14 - API RSA MODERNA (SIN CSP TIPO 24)");
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                Log("Assembly Version: " + assembly.GetName().Version);
                Log("Assembly Location: " + assembly.Location);
                Log("Assembly LastWriteTime: " + System.IO.File.GetLastWriteTime(assembly.Location).ToString("yyyy-MM-dd HH:mm:ss"));
                Log("=== PARAMETROS DE ENTRADA ===");
                Log("URL: " + url);
                Log("urlQR: " + urlQR);
                Log("p12FilePath: " + p12FilePath);
                Log("tipoFirmado: " + tipoFirmado);
                Log("XML Input Length: " + (xmlString != null ? xmlString.Length.ToString() : "NULL"));
                Log("Sistema Hora Local: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                Log("Sistema Hora UTC: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                
                // Guardar XML completo de entrada en archivo separado
                try
                {
                    string xmlLogPath = @"C:\nextsys - GLP\sifen_xml_input.txt";
                    File.WriteAllText(xmlLogPath, "=== XML INPUT " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ===" + Environment.NewLine + (xmlString ?? "NULL"));
                    Log("XML Input guardado en: " + xmlLogPath);
                }
                catch (Exception exLog) { Log("Error guardando XML: " + exLog.Message); }
                
                // Log primeros bytes para detectar BOM
                if (xmlString != null && xmlString.Length > 0)
                {
                    Log("Primeros 10 chars (códigos): " + string.Join(",", xmlString.Substring(0, Math.Min(10, xmlString.Length)).Select(c => ((int)c).ToString())));
                }
                
                // Limpiar BOM y caracteres especiales del XML de entrada
                if (xmlString != null)
                {
                    // Remover BOM si existe
                    xmlString = xmlString.TrimStart('\uFEFF', '\u200B');
                    // Asegurar que el XML comience correctamente
                    int xmlStart = xmlString.IndexOf('<');
                    if (xmlStart > 0)
                    {
                        Log("Caracteres removidos antes de '<': " + xmlStart);
                        xmlString = xmlString.Substring(xmlStart);
                    }
                }
                Log("XML Cleaned Length: " + (xmlString != null ? xmlString.Length.ToString() : "NULL"));
                
                // Configuración completa SSL/TLS para Windows 2025-2026
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.CheckCertificateRevocationList = false;
                ServicePointManager.DefaultConnectionLimit = 9999;
                
                // SOLUCIÓN PARA EQUIPOS NUEVOS: Probar diferentes flags hasta encontrar uno compatible
                Log("=== CARGA DE CERTIFICADO ===");
                X509Certificate2 certTemp = null;
                Exception lastException = null;
                
                // Intentar diferentes combinaciones de flags en orden de compatibilidad
                X509KeyStorageFlags[] flagsToTry = new X509KeyStorageFlags[]
                {
                    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable,
                    X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable,
                    X509KeyStorageFlags.DefaultKeySet | X509KeyStorageFlags.Exportable,
                    X509KeyStorageFlags.Exportable,
                    X509KeyStorageFlags.MachineKeySet,
                    X509KeyStorageFlags.UserKeySet
                };
                
                foreach (var flags in flagsToTry)
                {
                    try
                    {
                        Log("Intentando cargar certificado con flags: " + flags.ToString());
                        certTemp = new X509Certificate2(p12FilePath, certificatePassword, flags);
                        if (certTemp.HasPrivateKey)
                        {
                            Log("Certificado cargado EXITOSAMENTE con flags: " + flags.ToString());
                            break;
                        }
                        else
                        {
                            Log("Certificado cargado pero SIN clave privada con flags: " + flags.ToString());
                            certTemp = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Fallo con flags " + flags.ToString() + ": " + ex.Message);
                        lastException = ex;
                        certTemp = null;
                    }
                }
                
                if (certTemp == null)
                {
                    throw new CryptographicException("No se pudo cargar el certificado con ninguna combinación de flags", lastException);
                }
                
                Log("Certificado para FIRMA: " + certTemp.Subject);
                Log("Certificado HasPrivateKey: " + certTemp.HasPrivateKey);
                
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = false;
                doc.LoadXml(xmlString);
                XmlNode node;
                if (tipoFirmado == "1")
                {
                    node = doc.GetElementsByTagName("DE")[0];
                }
                else
                {
                    startDocument = "";
                    endDocument = "";
                    node = doc.GetElementsByTagName("rEve")[0];
                }
                if (node != null)
                {
                    XmlAttribute idAttribute = node.Attributes["Id"];
                    string nodeId = idAttribute.Value;
                    string cdcValue = nodeId; // El CDC es el ID del nodo DE
                    Log("Node ID (CDC): " + nodeId);
                    
                    // Usar el certificado precargado con flags compatibles
                    X509Certificate2 cert = certTemp;
                    Log("Certificado cargado: " + cert.Subject);
                    Log("Certificado Serial: " + cert.SerialNumber);
                    Log("Certificado Thumbprint: " + cert.Thumbprint);
                    
                    SignedXmlWithId signedXml = new SignedXmlWithId(doc);
                    
                    Log("=== METODO DE FIRMA ===");
                    Log("USANDO: API RSA MODERNA - GetRSAPrivateKey()");
                    Log("SIN: RSACryptoServiceProvider con CspParameters(24)");
                    Log("Compatible con: Windows 10/11 modernos");
                    
                    // SOLUCIÓN PARA EQUIPOS NUEVOS: Usar API moderna RSA directamente
                    // Esto evita el error "Se ha especificado un tipo de proveedor no válido"
                    RSA rsaKey = cert.GetRSAPrivateKey();
                    if (rsaKey == null)
                    {
                        throw new CryptographicException("No se pudo obtener la clave privada RSA del certificado");
                    }
                    
                    Log("RSA Key obtenida via GetRSAPrivateKey()");
                    Log("RSA KeySize: " + rsaKey.KeySize);
                    Log("RSA SignatureAlgorithm: " + rsaKey.SignatureAlgorithm);
                    
                    // Usar directamente la clave RSA moderna (compatible con SHA-256 y equipos nuevos)
                    // No es necesario crear RSACryptoServiceProvider con CSP tipo 24
                    
                    Reference reference = new Reference();
                    reference.Uri = "#" + nodeId;
                    reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                    reference.AddTransform(new XmlDsigExcC14NTransform());
                    reference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";
                    KeyInfo keyInfo = new KeyInfo();
                    keyInfo.AddClause(new KeyInfoX509Data(cert));
                    signedXml.SigningKey = rsaKey;
                    signedXml.AddReference(reference);
                    signedXml.SignedInfo.CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";
                    signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
                    signedXml.KeyInfo = keyInfo;
                    signedXml.ComputeSignature();
                    Log("Firma computada correctamente");
                    XmlElement signature = signedXml.GetXml();
                    
                    // Log valores de firma para diagnóstico
                    XmlNodeList sigValueNodes = signature.GetElementsByTagName("SignatureValue");
                    if (sigValueNodes.Count > 0)
                    {
                        string sigValue = sigValueNodes[0].InnerText;
                        Log("SignatureValue (primeros 100 chars): " + (sigValue.Length > 100 ? sigValue.Substring(0, 100) : sigValue));
                        Log("SignatureValue Length: " + sigValue.Length);
                    }
                    
                    node.ParentNode.InsertAfter(signature, node);
                    XmlNodeList nodeListDigest = doc.GetElementsByTagName("DigestValue");
                    string digestValue = "";
                    foreach (XmlNode nodeDigest in nodeListDigest)
                    {
                        digestValue = StringToHex(nodeDigest.InnerText);
                    }
                    Log("DigestValue (hex): " + digestValue.Substring(0, Math.Min(80, digestValue.Length)) + "...");
                    
                    XmlNodeList nodeListQR = doc.GetElementsByTagName("dCarQR");
                    string qrValue = "";
                    string qrHash = "";
                    foreach (XmlNode qrNode in nodeListQR)
                    {
                        qrNode.InnerText = qrNode.InnerText.Replace("665569394474586a4f4a396970724970754f344c434a75706a457a73645766664846656d573270344c69593d", digestValue);
                        qrValue = qrNode.InnerText;
                        qrHash = SHA256ToString(qrValue);
                        qrNode.InnerText = urlQR + qrValue.Substring(0, qrValue.Length - 32) + "&cHashQR=" + qrHash;
                        qrValue = qrNode.InnerText;
                    }
                    StringBuilder builder = new StringBuilder(doc.OuterXml);
                    string base64String = "";
                    if (tipoFirmado == "1")
                    {
                        base64String = StringToZip(builder.ToString());
                    }
                    else
                    {
                        base64String = builder.ToString();
                    }
                    
                    // Guardar XML firmado completo
                    try
                    {
                        string xmlFirmadoPath = @"C:\nextsys - GLP\sifen_xml_firmado.txt";
                        string soapCompleto = startDocument + base64String + endDocument;
                        File.WriteAllText(xmlFirmadoPath, "=== XML FIRMADO " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ===" + Environment.NewLine + "SOAP Envelope:" + Environment.NewLine + soapCompleto + Environment.NewLine + Environment.NewLine + "=== XML INTERNO ===" + Environment.NewLine + doc.OuterXml);
                        Log("XML Firmado guardado en: " + xmlFirmadoPath);
                    }
                    catch (Exception exLog) { Log("Error guardando XML Firmado: " + exLog.Message); }
                    
                    Log("=== FIN FIRMA - Enviando... ===");
                    string result = Enviar(url, startDocument + base64String + endDocument, p12FilePath, certificatePassword);
                    
                    // Guardar respuesta del servidor
                    try
                    {
                        string respuestaPath = @"C:\nextsys - GLP\sifen_respuesta.txt";
                        File.WriteAllText(respuestaPath, "=== RESPUESTA SIFEN " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ===" + Environment.NewLine + result);
                        Log("Respuesta guardada en: " + respuestaPath);
                    }
                    catch (Exception exLog) { Log("Error guardando respuesta: " + exLog.Message); }
                    
                    // Incluir CDC en el retorno
                    Log("CDC a retornar: " + cdcValue);
                    Log("QR a retornar: " + qrValue);
                    
                    // GUARDAR CDC EN ARCHIVO SEPARADO PARA QUE POWERBUILDER PUEDA LEERLO
                    try
                    {
                        File.WriteAllText(@"C:\nextsys - GLP\sifen_cdc.txt", cdcValue);
                        File.WriteAllText(@"C:\nextsys - GLP\sifen_qr.txt", qrValue);
                        File.WriteAllText(@"C:\nextsys - GLP\sifen_codigo.txt", getTagValue(result, "ns2:dCodRes"));
                        File.WriteAllText(@"C:\nextsys - GLP\sifen_mensaje.txt", getTagValue(result, "ns2:dMsgRes"));
                        File.WriteAllText(@"C:\nextsys - GLP\sifen_idlote.txt", getTagValue(result, "ns2:dProtConsLote"));
                        Log("Archivos CDC/QR guardados para PowerBuilder");
                    }
                    catch (Exception exFile) { Log("Error guardando archivos separados: " + exFile.Message); }
                    
                    // FORMATO EXACTO DEL ORIGINAL - PowerBuilder espera este orden exacto
                    // Original: codigo, mensaje, qr, idLote, documento
                    retorno = "{\"codigo\":" + "\"" + getTagValue(result, "ns2:dCodRes") + "\"," + 
                              "\"mensaje\":" + "\"" + getTagValue(result, "ns2:dMsgRes") + "\"," + 
                              "\"qr\":" + "\"" + qrValue + "\"," + 
                              "\"idLote\":" + "\"" + getTagValue(result, "ns2:dProtConsLote") + "\"," + 
                              "\"documento\":" + "\"" + cdcValue + "\"}";
                    
                    Log("Retorno JSON Length: " + retorno.Length);
                    Log("RETORNO COMPLETO: " + retorno);
                    
                    // Guardar retorno completo para diagnóstico
                    try
                    {
                        string retornoPath = @"C:\nextsys - GLP\sifen_retorno.txt";
                        File.WriteAllText(retornoPath, "=== RETORNO JSON " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ===" + Environment.NewLine + 
                            "Longitud: " + retorno.Length + Environment.NewLine +
                            "RETORNO COMPLETO:" + Environment.NewLine + 
                            retorno);
                        Log("Retorno guardado en: " + retornoPath);
                    }
                    catch (Exception exLog) { Log("Error guardando retorno: " + exLog.Message); }
                }
                else
                {
                    Log("ERROR: No se encontró el nodo a firmar");
                    retorno = "{\"codigo\":" + "\"" + "10001" + "\"," + "\"mensaje\":" + "\"" + "No se encontró el nodo a firmar" + "\"," + "\"qr\":" + "\"" + "null" + "\"," + "\"idLote\":" + "\"" + "null" + "\"," + "\"documento\":" + "\"" + "null" + "\"}";
                }

            }
            catch (Exception ex)
            {
                Log("ERROR en firmarYEnviar: " + ex.GetType().Name + " - " + ex.Message);
                Log("StackTrace: " + ex.StackTrace);
                retorno = "{\"codigo\":" + "\"" + "10002" + "\"," + "\"mensaje\":" + "\"" + ex.Message + "\"," + "\"qr\":" + "\"" + "null" + "\"," + "\"idLote\":" + "\"" + "null" + "\"," + "\"documento\":" + "\"" + "null" + "\"}";
            }
            return retorno;
        }

        [return: MarshalAs(UnmanagedType.BStr)]
        public string consulta(
            [MarshalAs(UnmanagedType.BStr)] string url, 
            [MarshalAs(UnmanagedType.BStr)] string id, 
            [MarshalAs(UnmanagedType.BStr)] string tipoConsulta, 
            [MarshalAs(UnmanagedType.BStr)] string p12FilePath, 
            [MarshalAs(UnmanagedType.BStr)] string certificatePassword)
        {
            // Si el ID está vacío o nulo, leer del archivo guardado
            string idReal = id;
            if (string.IsNullOrEmpty(id) || id.Trim() == "")
            {
                try
                {
                    if (tipoConsulta == "2") // Consulta por CDC
                    {
                        string cdcPath = @"C:\nextsys - GLP\sifen_cdc.txt";
                        if (File.Exists(cdcPath))
                        {
                            idReal = File.ReadAllText(cdcPath).Trim();
                            Log("CDC leído desde archivo: " + idReal);
                        }
                    }
                    else if (tipoConsulta == "3") // Consulta por Lote
                    {
                        string lotePath = @"C:\nextsys - GLP\sifen_idlote.txt";
                        if (File.Exists(lotePath))
                        {
                            idReal = File.ReadAllText(lotePath).Trim();
                            Log("IdLote leído desde archivo: " + idReal);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Error leyendo archivo de ID: " + ex.Message);
                }
            }
            
            string consulta = "";
            if (tipoConsulta == "1")
            {
                consulta = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\">\n"
                        + "<soap:Body>\n"
                        + "<rEnviConsRUC xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\">\n"
                        + "    <dId>1</dId>\n"
                        + "    <dRUCCons>" + idReal + "</dRUCCons>\n"
                        + "</rEnviConsRUC>\n"
                        + "</soap:Body>\n"
                        + "</soap:Envelope>";
            }
            else if (tipoConsulta == "2")
            {
                consulta = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\">\n"
                        + "<soap:Body>\n"
                        + "<rEnviConsDeRequest xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\">\n"
                        + "<dId>1</dId>\n"
                        + "<dCDC>" + idReal + "</dCDC>\n"
                        + "</rEnviConsDeRequest>\n"
                        + "</soap:Body>\n"
                        + "</soap:Envelope>";
            }
            else if (tipoConsulta == "3")
            {
                consulta = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\">\n"
                        + "<soap:Body>\n"
                        + "<rEnviConsLoteDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\">\n"
                        + "<dId>1</dId>\n"
                        + "<dProtConsLote>" + idReal + "</dProtConsLote>\n"
                        + "</rEnviConsLoteDe>\n"
                        + "</soap:Body>\n"
                        + "</soap:Envelope>";
            }
            string response = Enviar(url, consulta, p12FilePath, certificatePassword);
            return response;
        }
    }
}

