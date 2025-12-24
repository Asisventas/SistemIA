using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace SistemIA
{
    // Utilidad simple para aceptar payloads en varios formatos y devolver un rDE bien formado
    public static class ExternalXmlExtractor
    {
        public static string? TryExtractRde(string xmlOrSoap)
        {
            if (string.IsNullOrWhiteSpace(xmlOrSoap)) return null;
            var raw = xmlOrSoap.Trim('\uFEFF', '\u200B', '\u0000', ' ', '\n', '\r', '\t');
            try
            {
                // Caso 1: ya es rDE
                var docDirect = new XmlDocument();
                docDirect.LoadXml(raw);
                if (docDirect.DocumentElement != null && EqualsLocal(docDirect.DocumentElement, "rDE"))
                {
                    return docDirect.OuterXml;
                }
            }
            catch { /* intentar como SOAP */ }

            try
            {
                // Caso 2: SOAP: buscar xDE
                var doc = new XmlDocument();
                doc.LoadXml(raw);
                var nsm = new XmlNamespaceManager(doc.NameTable);
                nsm.AddNamespace("s", "http://www.w3.org/2003/05/soap-envelope");
                nsm.AddNamespace("sx", "http://ekuatia.set.gov.py/sifen/xsd");

                // Buscar cualquier nodo xDE
                var xdeNode = doc.SelectSingleNode("//*[local-name()='xDE']");
                if (xdeNode == null) return null;

                // Intento A: xDE contiene rLoteDE/rDE en texto XML plano
                // Si xdeNode tiene hijos, buscar rDE dentro
                if (xdeNode.HasChildNodes)
                {
                    var rdeInner = xdeNode.SelectSingleNode(".//*[local-name()='rDE']");
                    if (rdeInner is XmlElement rdeEl)
                    {
                        var tmp = new XmlDocument();
                        tmp.LoadXml(rdeEl.OuterXml);
                        return tmp.OuterXml;
                    }
                    // Si el hijo directo es rLoteDE con texto CDATA/InnerXml
                    if (xdeNode.FirstChild is XmlText or XmlCDataSection)
                    {
                        var text = xdeNode.InnerText?.Trim();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            // Podría ser base64+gzip
                            var maybe = TryBase64Gunzip(text);
                            if (!string.IsNullOrEmpty(maybe))
                            {
                                var innerDoc = new XmlDocument();
                                innerDoc.LoadXml(maybe);
                                var rde2 = innerDoc.SelectSingleNode("//*[local-name()='rLoteDE']/*[local-name()='rDE']");
                                if (rde2 is XmlElement rdeEl2)
                                {
                                    var tmp = new XmlDocument();
                                    tmp.LoadXml(rdeEl2.OuterXml);
                                    return tmp.OuterXml;
                                }
                            }
                        }
                    }
                }

                // Intento B: xDE es base64+gzip en InnerText
                var base64 = (xdeNode.InnerText ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(base64))
                {
                    var unzipped = TryBase64Gunzip(base64);
                    if (!string.IsNullOrWhiteSpace(unzipped))
                    {
                        var innerDoc = new XmlDocument();
                        innerDoc.LoadXml(unzipped);
                        var rdeNode2 = innerDoc.SelectSingleNode("//*[local-name()='rLoteDE']/*[local-name()='rDE']");
                        if (rdeNode2 is XmlElement rdeEl2)
                        {
                            var tmp = new XmlDocument();
                            tmp.LoadXml(rdeEl2.OuterXml);
                            return tmp.OuterXml;
                        }
                    }
                }
            }
            catch
            {
                // ignorar, devolver null
            }

            return null;
        }

        private static bool EqualsLocal(XmlElement el, string local) => string.Equals(el.LocalName, local, StringComparison.OrdinalIgnoreCase);

        private static string? TryBase64Gunzip(string base64)
        {
            try
            {
                var bytes = Convert.FromBase64String(base64);
                using var ms = new MemoryStream(bytes);
                using var gzip = new GZipStream(ms, CompressionMode.Decompress);
                using var reader = new StreamReader(gzip, new UTF8Encoding(false));
                return reader.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }
    }
}
