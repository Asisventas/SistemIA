// ============================================================================
// DECOMPILADO DE: C:\asis\SistemIA\.ai-docs\SIFEN\Debug_power\nextsys\Sifen.dll
// FECHA: 12-Ene-2026
// HERRAMIENTA: ILSpy
// 
// ESTE DLL ES USADO POR NEXTSYS (PowerBuilder 21) Y FUNCIONA CON SIFEN!
// ============================================================================

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace Sifen;

public class Sifen
{
    private static string SHA256ToString(string s)
    {
        using SHA256 sHA = SHA256.Create();
        return sHA.ComputeHash(Encoding.UTF8.GetBytes(s)).Aggregate(new StringBuilder(), (StringBuilder sb, byte x) => sb.Append(x.ToString("x2"))).ToString();
    }

    /// <summary>
    /// ========================================================================
    /// CRÍTICO: El nombre del archivo DENTRO del ZIP es "compressed.txt"
    /// NO "DE_DDMMYYYY.xml" ni otro nombre!
    /// ========================================================================
    /// </summary>
    private static string StringToZip(string originalString)
    {
        byte[] inArray;
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                // IMPORTANTE: El archivo se llama "compressed.txt"
                ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry("compressed.txt", CompressionLevel.Optimal);
                using Stream stream = zipArchiveEntry.Open();
                using StreamWriter streamWriter = new StreamWriter(stream);
                streamWriter.Write(originalString);
            }
            inArray = memoryStream.ToArray();
        }
        return Convert.ToBase64String(inArray);
    }

    /// <summary>
    /// Convierte string a hex (cada carácter ASCII a su valor hex)
    /// </summary>
    private string StringToHex(string hexstring)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (char value in hexstring)
        {
            stringBuilder.Append(Convert.ToInt32(value).ToString("x2"));
        }
        return stringBuilder.ToString();
    }

    private string getTagValue(string xmlString, string tagName)
    {
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xmlString);
        XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName(tagName);
        if (elementsByTagName.Count > 0)
        {
            return elementsByTagName[0].InnerText;
        }
        return "Tag not found.";
    }

    /// <summary>
    /// ========================================================================
    /// CRÍTICO: Método de envío HTTP
    /// - ContentType: "application/xml" (SIN charset)
    /// - Usa HttpWebRequest con ClientCertificates
    /// ========================================================================
    /// </summary>
    private string Enviar(string url, string documento, string pathArchivoP12, string passwordArchivoP12)
    {
        string result = "";
        X509Certificate2 value = new X509Certificate2(pathArchivoP12, passwordArchivoP12);
        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        httpWebRequest.Method = "POST";
        httpWebRequest.ContentType = "application/xml";  // <-- SIN charset!
        httpWebRequest.ClientCertificates.Add(value);
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(documento);
            httpWebRequest.ContentLength = bytes.Length;
            using (Stream stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
            }
            using HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            if (httpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                using Stream stream2 = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream2, Encoding.UTF8);
                result = streamReader.ReadToEnd();
            }
            else
            {
                result = httpWebResponse.StatusCode.ToString();
            }
        }
        catch (WebException ex)
        {
            if (ex.Response != null)
            {
                using Stream stream3 = ex.Response.GetResponseStream();
                StreamReader streamReader2 = new StreamReader(stream3, Encoding.UTF8);
                result = streamReader2.ReadToEnd();
            }
            else
            {
                result = ex.Message;
            }
        }
        return result;
    }

    /// <summary>
    /// ========================================================================
    /// MÉTODO PRINCIPAL - firmarYEnviar
    /// ========================================================================
    /// 
    /// FORMATO SOAP usado:
    /// <soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
    ///   <soap:Body>
    ///     <rEnvioLote xmlns="http://ekuatia.set.gov.py/sifen/xsd">
    ///       <dId>160420241700</dId>
    ///       <xDE>{ZIP_BASE64}</xDE>
    ///     </rEnvioLote>
    ///   </soap:Body>
    /// </soap:Envelope>
    /// 
    /// NOTAS IMPORTANTES:
    /// 1. NO hay <soap:Header/>
    /// 2. Usa rEnvioLote (LOTE), no rEnviDe (SYNC)
    /// 3. El xDE contiene ZIP (Base64) con archivo "compressed.txt" dentro
    /// 4. dId tiene formato "160420241700" (fecha+hora)
    /// 5. El DigestValue del QR usa un placeholder que se reemplaza DESPUÉS de firmar
    /// 
    /// ========================================================================
    /// </summary>
    public string firmarYEnviar(string url, string urlQR, string xmlString, string p12FilePath, string certificatePassword, string tipoFirmado = "1")
    {
        string text = "";
        
        // FORMATO SOAP - SIN Header, con soap: prefijo
        string text2 = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"><soap:Body><rEnvioLote xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\"><dId>160420241700</dId><xDE>";
        string text3 = "</xDE></rEnvioLote></soap:Body></soap:Envelope>";
        
        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlString);
            XmlNode xmlNode;
            if (tipoFirmado == "1")
            {
                xmlNode = xmlDocument.GetElementsByTagName("DE")[0];
            }
            else
            {
                // Para eventos, no usa envelope SOAP
                text2 = "";
                text3 = "";
                xmlNode = xmlDocument.GetElementsByTagName("rEve")[0];
            }
            
            if (xmlNode != null)
            {
                XmlAttribute xmlAttribute = xmlNode.Attributes["Id"];
                string value = xmlAttribute.Value;
                
                // Firma XML con clase custom SignedXmlWithId
                SignedXmlWithId signedXmlWithId = new SignedXmlWithId(xmlDocument);
                X509Certificate2 x509Certificate = new X509Certificate2(p12FilePath, certificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                signedXmlWithId.SigningKey = x509Certificate.PrivateKey;
                
                // RSA con CspParameters tipo 24 (RSA-SHA256 compatible)
                RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(new CspParameters(24));
                rSACryptoServiceProvider.PersistKeyInCsp = false;
                rSACryptoServiceProvider.FromXmlString(x509Certificate.PrivateKey.ToXmlString(includePrivateParameters: true));
                
                Reference reference = new Reference();
                reference.Uri = "#" + value;
                reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                reference.AddTransform(new XmlDsigExcC14NTransform());
                reference.DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";
                
                KeyInfo keyInfo = new KeyInfo();
                keyInfo.AddClause(new KeyInfoX509Data(x509Certificate));
                
                signedXmlWithId.SigningKey = rSACryptoServiceProvider;
                signedXmlWithId.AddReference(reference);
                signedXmlWithId.SignedInfo.CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";
                signedXmlWithId.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
                signedXmlWithId.KeyInfo = keyInfo;
                signedXmlWithId.ComputeSignature();
                
                XmlElement xml = signedXmlWithId.GetXml();
                xmlNode.ParentNode.InsertAfter(xml, xmlNode);
                
                // DESPUÉS de firmar, obtener el DigestValue y actualizar el QR
                XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("DigestValue");
                string newValue = "";
                foreach (XmlNode item in elementsByTagName)
                {
                    // Convertir DigestValue (Base64) a Hex de los caracteres ASCII
                    newValue = StringToHex(item.InnerText);
                }
                
                // Reemplazar el placeholder fijo en el QR
                // NOTA: El XML original tiene este placeholder en dCarQR
                XmlNodeList elementsByTagName2 = xmlDocument.GetElementsByTagName("dCarQR");
                string text4 = "";
                string text5 = "";
                foreach (XmlNode item2 in elementsByTagName2)
                {
                    // Reemplazar placeholder fijo por el DigestValue real
                    item2.InnerText = item2.InnerText.Replace(
                        "665569394474586a4f4a396970724970754f344c434a75706a457a73645766664846656d573270344c69593d", 
                        newValue);
                    
                    text4 = item2.InnerText;
                    
                    // Calcular hash del QR
                    text5 = SHA256ToString(text4);
                    
                    // Reconstruir URL completa del QR con hash
                    item2.InnerText = urlQR + text4.Substring(0, text4.Length - 32) + "&cHashQR=" + text5;
                    text4 = item2.InnerText;
                }
                
                StringBuilder stringBuilder = new StringBuilder(xmlDocument.OuterXml);
                string text6 = "";
                
                // Para DE (tipoFirmado=1): comprimir en ZIP
                // Para eventos: enviar sin comprimir
                text6 = ((!(tipoFirmado == "1")) ? stringBuilder.ToString() : StringToZip(stringBuilder.ToString()));
                
                // ENVIAR
                string xmlString2 = Enviar(url, text2 + text6 + text3, p12FilePath, certificatePassword);
                
                // Parsear respuesta
                return "{\"codigo\":\"" + getTagValue(xmlString2, "ns2:dCodRes") 
                     + "\",\"mensaje\":\"" + getTagValue(xmlString2, "ns2:dMsgRes") 
                     + "\",\"qr\":\"" + text4 
                     + "\",\"idLote\":\"" + getTagValue(xmlString2, "ns2:dProtConsLote") 
                     + "\",\"documento\":\"" + text2 + stringBuilder.ToString() + text3 + "\"}";
            }
            return "{\"codigo\":\"10001\",\"mensaje\":\"No se encontró el nodo a firmar\",\"qr\":\"null\",\"idLote\":\"null\",\"documento\":\"null\"}";
        }
        catch (Exception ex)
        {
            return "{\"codigo\":\"10002\",\"mensaje\":\"" + ex.Message + "\",\"qr\":\"null\",\"idLote\":\"null\",\"documento\":\"null\"}";
        }
    }

    /// <summary>
    /// Consultas a SIFEN (RUC, DE por CDC, Lote)
    /// </summary>
    public string consulta(string url, string id, string tipoConsulta, string p12FilePath, string certificatePassword)
    {
        string documento = "";
        switch (tipoConsulta)
        {
        case "1": // Consulta RUC
            documento = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\">\n<soap:Body>\n<rEnviConsRUC xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\">\n    <dId>1</dId>\n    <dRUCCons>" + id + "</dRUCCons>\n</rEnviConsRUC>\n</soap:Body>\n</soap:Envelope>";
            break;
        case "2": // Consulta DE por CDC
            documento = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\">\n<soap:Body>\n<rEnviConsDeRequest xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\">\n<dId>1</dId>\n<dCDC>" + id + "</dCDC>\n</rEnviConsDeRequest>\n</soap:Body>\n</soap:Envelope>";
            break;
        case "3": // Consulta Lote
            documento = "<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\">\n<soap:Body>\n<rEnviConsLoteDe xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\">\n<dId>1</dId>\n<dProtConsLote>" + id + "</dProtConsLote>\n</rEnviConsLoteDe>\n</soap:Body>\n</soap:Envelope>";
            break;
        }
        return Enviar(url, documento, p12FilePath, certificatePassword);
    }
}

// ============================================================================
// Clase auxiliar para firma XML con atributo Id custom
// ============================================================================
public class SignedXmlWithId : SignedXml
{
    public SignedXmlWithId(XmlDocument xml)
        : base(xml)
    {
    }

    public SignedXmlWithId(XmlElement xmlElement)
        : base(xmlElement)
    {
    }

    public override XmlElement GetIdElement(XmlDocument doc, string id)
    {
        XmlElement xmlElement = base.GetIdElement(doc, id);
        if (xmlElement == null)
        {
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(doc.NameTable);
            xmlNamespaceManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            xmlElement = doc.SelectSingleNode("//*[@Id=\"" + id + "\"]", xmlNamespaceManager) as XmlElement;
        }
        return xmlElement;
    }
}
