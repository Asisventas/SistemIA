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
                // Configurar TLS 1.2/1.3 para compatibilidad con Windows 2025-2026
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                
                // Aceptar certificados del servidor (necesario para Windows actualizado)
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                
                // Cargar certificado con flags para Windows actualizado
                X509Certificate2 certificate = new X509Certificate2(pathArchivoP12, passwordArchivoP12, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/soap+xml; charset=utf-8";  // SOAP 1.2 Content-Type
                request.Accept = "*/*";
                request.UserAgent = "Java/1.8.0_341";  // Header que funciona con BIG-IP de SIFEN
                request.KeepAlive = true;  // Mantener conexión
                request.AllowAutoRedirect = true;  // Seguir redirects
                request.MaximumAutomaticRedirections = 3;
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

        public string firmarYEnviar(string url, string urlQR, string xmlString, string p12FilePath, string certificatePassword, string tipoFirmado = "1")
        {
            string retorno = "";
            string startDocument = @"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""><soap:Body><rEnvioLote xmlns=""http://ekuatia.set.gov.py/sifen/xsd""><dId>160420241700</dId><xDE>";
            string endDocument = "</xDE></rEnvioLote></soap:Body></soap:Envelope>";
            try
            {
                Log("=== INICIO FIRMA ===");
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
                
                // Configurar TLS
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                
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
                    Log("Node ID: " + nodeId);
                    
                    // Cargar certificado
                    X509Certificate2 cert = new X509Certificate2(p12FilePath, certificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                    Log("Certificado cargado: " + cert.Subject);
                    Log("Certificado Serial: " + cert.SerialNumber);
                    Log("Certificado Thumbprint: " + cert.Thumbprint);
                    
                    // Diagnóstico del tipo de PrivateKey
                    Log("PrivateKey Type: " + (cert.PrivateKey != null ? cert.PrivateKey.GetType().FullName : "NULL"));
                    Log("PrivateKey KeySize: " + (cert.PrivateKey != null ? cert.PrivateKey.KeySize.ToString() : "NULL"));
                    
                    SignedXmlWithId signedXml = new SignedXmlWithId(doc);
                    
                    // Crear RSACryptoServiceProvider con proveedor AES (24) para SHA-256
                    RSACryptoServiceProvider key = new RSACryptoServiceProvider(new CspParameters(24));
                    key.PersistKeyInCsp = false;
                    
                    // Intentar diferentes métodos para obtener la clave
                    try
                    {
                        // Método 1: Directo desde PrivateKey (original)
                        RSACryptoServiceProvider originalCsp = cert.PrivateKey as RSACryptoServiceProvider;
                        if (originalCsp != null && originalCsp.CspKeyContainerInfo.ProviderType != 1)
                        {
                            // El proveedor ya soporta SHA-256, usar directo
                            key.FromXmlString(originalCsp.ToXmlString(true));
                            Log("RSA Key via método ORIGINAL (ToXmlString)");
                        }
                        else if (originalCsp != null)
                        {
                            // Proveedor tipo 1 (base), necesita conversión
                            key.FromXmlString(originalCsp.ToXmlString(true));
                            Log("RSA Key via ToXmlString (ProviderType 1)");
                        }
                        else
                        {
                            // PrivateKey no es RSACryptoServiceProvider (puede ser RSACng)
                            Log("PrivateKey NO es RSACryptoServiceProvider, intentando GetRSAPrivateKey()");
                            RSA rsaKey = cert.GetRSAPrivateKey();
                            if (rsaKey != null)
                            {
                                RSAParameters rsaParams = rsaKey.ExportParameters(true);
                                key.ImportParameters(rsaParams);
                                Log("RSA Key via GetRSAPrivateKey().ExportParameters()");
                            }
                            else
                            {
                                throw new CryptographicException("No se pudo obtener RSA PrivateKey");
                            }
                        }
                    }
                    catch (Exception exKey)
                    {
                        Log("Error obteniendo clave: " + exKey.Message);
                        // Último intento: GetRSAPrivateKey con ExportParameters
                        RSA rsaKey = cert.GetRSAPrivateKey();
                        RSAParameters rsaParams = rsaKey.ExportParameters(true);
                        key.ImportParameters(rsaParams);
                        Log("RSA Key via fallback ExportParameters");
                    }
                    
                    Reference reference = new Reference();
                    reference.Uri = "#" + nodeId;
                    reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                    reference.AddTransform(new XmlDsigExcC14NTransform());
                    reference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";
                    KeyInfo keyInfo = new KeyInfo();
                    keyInfo.AddClause(new KeyInfoX509Data(cert));
                    signedXml.SigningKey = key;
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
                    
                    retorno = "{\"codigo\":" + "\"" + getTagValue(result, "ns2:dCodRes") + "\"," + "\"mensaje\":" + "\"" + getTagValue(result, "ns2:dMsgRes") + "\"," + "\"qr\":" + "\"" + qrValue + "\"," + "\"idLote\":" + "\"" + getTagValue(result, "ns2:dProtConsLote") + "\"," + "\"documento\":" + "\"" + (startDocument + builder.ToString() + endDocument) + "\"}";
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

        public string consulta(string url, string id, string tipoConsulta, string p12FilePath, string certificatePassword)
        {
            string consulta = "";
            if (tipoConsulta == "1")
            {
                consulta = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\">\n"
                        + "<soap:Body>\n"
                        + "<rEnviConsRUC xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\">\n"
                        + "    <dId>1</dId>\n"
                        + "    <dRUCCons>" + id + "</dRUCCons>\n"
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
                        + "<dCDC>" + id + "</dCDC>\n"
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
                        + "<dProtConsLote>" + id + "</dProtConsLote>\n"
                        + "</rEnviConsLoteDe>\n"
                        + "</soap:Body>\n"
                        + "</soap:Envelope>";
            }
            string response = Enviar(url, consulta, p12FilePath, certificatePassword);
            return response;
        }
    }
}

