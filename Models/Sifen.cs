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



namespace  SistemIA.Models
{
    [Guid("b1d23d8e-4e7b-4dc8-81fb-bfdb47d7f521")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class Sifen
    {
        public static string SHA256ToString(string s)
        {
            using (SHA256Managed alg = new SHA256Managed())
            {
                byte[] hash = alg.ComputeHash(Encoding.UTF8.GetBytes(s));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static string StringToZip(string originalString)
        {
            byte[] compressedBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    using (StreamWriter writer = new StreamWriter(gzip))
                    {
                        writer.Write(originalString);
                    }
                }
                compressedBytes = memoryStream.ToArray();
            }
            return Convert.ToBase64String(compressedBytes);
        }

        public string StringToHex(string hexstring)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char t in hexstring)
            {
                sb.Append(Convert.ToInt32(t).ToString("x2"));
            }
            return sb.ToString();
        }

        public string getTagValue(string xmlString, string tagName)
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

        public string Enviar(string url, string documento, string pathArchivoP12, string passwordArchivoP12)
        {
            string result = "";

            X509Certificate2 certificate = new X509Certificate2(pathArchivoP12, passwordArchivoP12);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/xml";
            request.ClientCertificates.Add(certificate);
            try
            {
                byte[] xmlBytes = Encoding.UTF8.GetBytes(documento);
                request.ContentLength = xmlBytes.Length;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(xmlBytes, 0, xmlBytes.Length);
                }
                using (HttpWebResponse res = (HttpWebResponse)request.GetResponse())
                {
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
                        result = res.StatusCode.ToString();
                    }
                }
            }
            catch (WebException ex)
            {
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
                    result = ex.Message;
                }
            }
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
                XmlDocument doc = new XmlDocument();
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
                    SignedXmlWithId signedXml = new SignedXmlWithId(doc);
                    X509Certificate2 cert = new X509Certificate2(p12FilePath, certificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                    signedXml.SigningKey = cert.PrivateKey;
                    RSACryptoServiceProvider key = new RSACryptoServiceProvider(new CspParameters(24));
                    key.PersistKeyInCsp = false;
                    key.FromXmlString(
                        cert.PrivateKey.ToXmlString(true)
                    );
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
                    XmlElement signature = signedXml.GetXml();
                    node.ParentNode.InsertAfter(signature, node);
                    XmlNodeList nodeListDigest = doc.GetElementsByTagName("DigestValue");
                    string digestValue = "";
                    foreach (XmlNode nodeDigest in nodeListDigest)
                    {
                        digestValue = StringToHex(nodeDigest.InnerText);
                    }
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
                    string result = Enviar(url, startDocument + base64String + endDocument, p12FilePath, certificatePassword);
                    retorno = "{\"codigo\":" + "\"" + getTagValue(result, "ns2:dCodRes") + "\"," + "\"mensaje\":" + "\"" + getTagValue(result, "ns2:dMsgRes") + "\"," + "\"qr\":" + "\"" + qrValue + "\"," + "\"idLote\":" + "\"" + getTagValue(result, "ns2:dProtConsLote") + "\"," + "\"documento\":" + "\"" + (startDocument + builder.ToString() + endDocument) + "\"}";
                }
                else
                {
                    retorno = "{\"codigo\":" + "\"" + "10001" + "\"," + "\"mensaje\":" + "\"" + "No se encontró el nodo a firmar" + "\"," + "\"qr\":" + "\"" + "null" + "\"," + "\"idLote\":" + "\"" + "null" + "\"," + "\"documento\":" + "\"" + "null" + "\"}";
                }

            }
            catch (Exception ex)
            {
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
