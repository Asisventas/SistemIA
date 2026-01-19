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
    /// <summary>
    /// DLL Interceptor para capturar datos de PowerBuilder.
    /// Guarda TODO lo que recibe y envía en archivos de log para análisis.
    /// REEMPLAZA el Sifen.dll original temporalmente para capturar datos.
    /// 
    /// Archivos generados:
    /// - C:\nextsys - GLP\interceptor_entrada.json  -> Parámetros recibidos
    /// - C:\nextsys - GLP\interceptor_xml_input.txt -> XML completo de entrada
    /// - C:\nextsys - GLP\interceptor_soap.txt      -> SOAP que se enviaría
    /// - C:\nextsys - GLP\interceptor_resultado.txt -> Respuesta o error
    /// </summary>
    [Guid("b1d23d8e-4e7b-4dc8-81fb-bfdb47d7f521")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class Sifen
    {
        private static readonly string _logDir = @"C:\nextsys - GLP";
        private static readonly string _logPath = Path.Combine(_logDir, "interceptor_log.txt");
        
        private static void EnsureLogDir()
        {
            try
            {
                if (!Directory.Exists(_logDir))
                    Directory.CreateDirectory(_logDir);
            }
            catch { }
        }
        
        private static void Log(string message)
        {
            try
            {
                EnsureLogDir();
                string logEntry = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " - " + message + Environment.NewLine;
                File.AppendAllText(_logPath, logEntry, Encoding.UTF8);
            }
            catch { }
        }
        
        private static void SaveToFile(string filename, string content)
        {
            try
            {
                EnsureLogDir();
                string fullPath = Path.Combine(_logDir, filename);
                File.WriteAllText(fullPath, content, new UTF8Encoding(false));
                Log("Guardado: " + fullPath + " (" + content.Length + " bytes)");
            }
            catch (Exception ex)
            {
                Log("ERROR guardando " + filename + ": " + ex.Message);
            }
        }
        
        private static string SHA256ToString(string s)
        {
            using (var alg = SHA256.Create())
                return alg.ComputeHash(Encoding.UTF8.GetBytes(s)).Aggregate(new StringBuilder(), (sb, x) => sb.Append(x.ToString("x2"))).ToString();
        }

        private static string StringToZip(string originalString)
        {
            byte[] compressedBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry("compressed.txt", CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                    using (var writer = new StreamWriter(entryStream, new UTF8Encoding(false)))
                    {
                        writer.Write(originalString);
                    }
                }
                compressedBytes = memoryStream.ToArray();
            }
            return Convert.ToBase64String(compressedBytes);
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
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);
                XmlNodeList nodeList = xmlDoc.GetElementsByTagName(tagName);
                if (nodeList.Count > 0)
                {
                    return nodeList[0].InnerText;
                }
            }
            catch { }
            return "Tag not found.";
        }

        private string Enviar(string url, string documento, string pathArchivoP12, string passwordArchivoP12)
        {
            string result = "";
            
            Log("=== INICIO ENVIO ===");
            Log("URL: " + url);
            Log("Documento Length: " + documento.Length);
            
            // *** GUARDAR SOAP COMPLETO ***
            SaveToFile("interceptor_soap_enviado.txt", documento);
            
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.CheckCertificateRevocationList = false;
                ServicePointManager.DefaultConnectionLimit = 9999;
                
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
                request.Timeout = 30000;
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
            
            // *** GUARDAR RESPUESTA ***
            SaveToFile("interceptor_respuesta.txt", result);
            
            Log("Response Length: " + result.Length);
            Log("=== FIN ENVIO ===");
            return result;
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
            
            try
            {
                // =====================================================================
                // INTERCEPTOR: Guardar TODOS los parámetros de entrada
                // =====================================================================
                Log("============================================================");
                Log("INTERCEPTOR SIFEN - CAPTURA DE DATOS POWERBUILDER");
                Log("============================================================");
                Log("Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                Log("url: " + (url ?? "NULL"));
                Log("urlQR: " + (urlQR ?? "NULL"));
                Log("p12FilePath: " + (p12FilePath ?? "NULL"));
                Log("certificatePassword: " + (certificatePassword ?? "NULL")); // OJO: expone password
                Log("tipoFirmado: " + (tipoFirmado ?? "NULL"));
                Log("xmlString Length: " + (xmlString != null ? xmlString.Length.ToString() : "NULL"));
                
                // Guardar JSON con parámetros
                string jsonParams = @"{
  ""timestamp"": """ + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + @""",
  ""url"": """ + (url ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + @""",
  ""urlQR"": """ + (urlQR ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + @""",
  ""p12FilePath"": """ + (p12FilePath ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"") + @""",
  ""certificatePassword"": ""[CAPTURADO]"",
  ""tipoFirmado"": """ + (tipoFirmado ?? "") + @""",
  ""xmlStringLength"": " + (xmlString != null ? xmlString.Length.ToString() : "0") + @"
}";
                SaveToFile("interceptor_entrada.json", jsonParams);
                
                // Guardar XML de entrada COMPLETO
                SaveToFile("interceptor_xml_input.txt", xmlString ?? "");
                
                // =====================================================================
                // FIRMA Y ENVÍO REAL (código del DLL funcional)
                // =====================================================================
                
                string startDocument = @"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""><soap:Body><rEnvioLote xmlns=""http://ekuatia.set.gov.py/sifen/xsd""><dId>160420241700</dId><xDE>";
                string endDocument = "</xDE></rEnvioLote></soap:Body></soap:Envelope>";
                
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | (SecurityProtocolType)3072;
                ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.CheckCertificateRevocationList = false;
                
                // Cargar certificado con diferentes flags
                X509Certificate2 certTemp = null;
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
                        Log("Intentando flags: " + flags.ToString());
                        certTemp = new X509Certificate2(p12FilePath, certificatePassword, flags);
                        if (certTemp.HasPrivateKey)
                        {
                            Log("Certificado cargado OK con: " + flags.ToString());
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Fallo con " + flags.ToString() + ": " + ex.Message);
                    }
                }
                
                if (certTemp == null)
                {
                    throw new Exception("No se pudo cargar el certificado");
                }
                
                Log("Certificado: " + certTemp.Subject);
                Log("Thumbprint: " + certTemp.Thumbprint);
                Log("HasPrivateKey: " + certTemp.HasPrivateKey);
                
                // Limpiar BOM del XML
                if (xmlString != null)
                {
                    xmlString = xmlString.TrimStart('\uFEFF', '\u200B');
                    int xmlStart = xmlString.IndexOf('<');
                    if (xmlStart > 0)
                    {
                        xmlString = xmlString.Substring(xmlStart);
                    }
                }
                
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
                    string cdcValue = nodeId;
                    Log("CDC (Node ID): " + nodeId);
                    
                    SignedXmlWithId signedXml = new SignedXmlWithId(doc);
                    
                    // *** MÉTODO DE FIRMA RSA MODERNO ***
                    RSA rsaKey = certTemp.GetRSAPrivateKey();
                    if (rsaKey == null)
                    {
                        throw new CryptographicException("No se pudo obtener clave RSA");
                    }
                    
                    Log("RSA KeySize: " + rsaKey.KeySize);
                    
                    Reference reference = new Reference();
                    reference.Uri = "#" + nodeId;
                    reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                    reference.AddTransform(new XmlDsigExcC14NTransform());
                    reference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";
                    
                    KeyInfo keyInfo = new KeyInfo();
                    keyInfo.AddClause(new KeyInfoX509Data(certTemp));
                    
                    signedXml.SigningKey = rsaKey;  // API MODERNA DIRECTA
                    signedXml.AddReference(reference);
                    signedXml.SignedInfo.CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";
                    signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
                    signedXml.KeyInfo = keyInfo;
                    signedXml.ComputeSignature();
                    
                    Log("Firma computada OK");
                    
                    XmlElement signature = signedXml.GetXml();
                    node.ParentNode.InsertAfter(signature, node);
                    
                    // Procesar DigestValue y QR
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
                    
                    // Guardar XML firmado
                    string xmlFirmado = doc.OuterXml;
                    SaveToFile("interceptor_xml_firmado.txt", xmlFirmado);
                    
                    string base64String = "";
                    if (tipoFirmado == "1")
                    {
                        base64String = StringToZip(xmlFirmado);
                    }
                    else
                    {
                        base64String = xmlFirmado;
                    }
                    
                    string soapCompleto = startDocument + base64String + endDocument;
                    SaveToFile("interceptor_soap_completo.txt", soapCompleto);
                    
                    Log("=== ENVIANDO A SIFEN ===");
                    string result = Enviar(url, soapCompleto, p12FilePath, certificatePassword);
                    
                    // Extraer códigos de respuesta
                    string codigo = getTagValue(result, "ns2:dCodRes");
                    string mensaje = getTagValue(result, "ns2:dMsgRes");
                    string idLote = getTagValue(result, "ns2:dProtConsLote");
                    
                    Log("Código: " + codigo);
                    Log("Mensaje: " + mensaje);
                    Log("ID Lote: " + idLote);
                    
                    // Guardar archivos individuales para PowerBuilder
                    SaveToFile("sifen_cdc.txt", cdcValue);
                    SaveToFile("sifen_qr.txt", qrValue);
                    SaveToFile("sifen_codigo.txt", codigo);
                    SaveToFile("sifen_mensaje.txt", mensaje);
                    SaveToFile("sifen_idlote.txt", idLote);
                    
                    retorno = "{\"codigo\":\"" + codigo + "\",\"mensaje\":\"" + mensaje + "\",\"qr\":\"" + qrValue + "\",\"idLote\":\"" + idLote + "\",\"documento\":\"" + cdcValue + "\"}";
                    
                    SaveToFile("interceptor_retorno.json", retorno);
                    Log("RETORNO: " + retorno);
                }
                else
                {
                    Log("ERROR: No se encontró nodo a firmar");
                    retorno = "{\"codigo\":\"10001\",\"mensaje\":\"No se encontró el nodo a firmar\",\"qr\":\"null\",\"idLote\":\"null\",\"documento\":\"null\"}";
                }
            }
            catch (Exception ex)
            {
                Log("EXCEPTION: " + ex.GetType().Name + " - " + ex.Message);
                Log("StackTrace: " + ex.StackTrace);
                retorno = "{\"codigo\":\"10002\",\"mensaje\":\"" + ex.Message.Replace("\"", "'") + "\",\"qr\":\"null\",\"idLote\":\"null\",\"documento\":\"null\"}";
                SaveToFile("interceptor_error.txt", ex.ToString());
            }
            
            Log("============================================================");
            Log("FIN INTERCEPTOR");
            Log("============================================================");
            
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
            Log("CONSULTA: url=" + url + ", id=" + id + ", tipo=" + tipoConsulta);
            
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
            
            SaveToFile("interceptor_consulta_soap.txt", consulta);
            string response = Enviar(url, consulta, p12FilePath, certificatePassword);
            SaveToFile("interceptor_consulta_respuesta.txt", response);
            return response;
        }
    }
    
    public class SignedXmlWithId : SignedXml
    {
        public SignedXmlWithId(XmlDocument xml) : base(xml) { }
        public SignedXmlWithId(XmlElement xmlElement) : base(xmlElement) { }

        public override XmlElement GetIdElement(XmlDocument doc, string id)
        {
            XmlElement idElem = base.GetIdElement(doc, id);
            if (idElem == null)
            {
                XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
                nsManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                idElem = doc.SelectSingleNode("//*[@Id=\"" + id + "\"]", nsManager) as XmlElement;
            }
            return idElem;
        }
    }
}
