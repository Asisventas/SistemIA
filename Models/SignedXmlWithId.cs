using System.Xml;
using System.Security.Cryptography.Xml;

namespace SistemIA.Models
{
    public class SignedXmlWithId : SignedXml
    {
        public SignedXmlWithId(XmlDocument xml) : base(xml)
        {
        }

        public SignedXmlWithId(XmlElement xmlElement)
            : base(xmlElement)
        {
        }

        public override XmlElement? GetIdElement(XmlDocument? doc, string id)
        {
            if (doc == null)
                throw new ArgumentNullException(nameof(doc));

            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            // Primero intentar con la búsqueda estándar
            XmlElement? idElem = base.GetIdElement(doc, id);

            if (idElem == null)
            {
                // Si no se encuentra, buscar por el atributo Id específico
                XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
                nsManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

                idElem = doc.SelectSingleNode($"//*[@Id='{id}']", nsManager) as XmlElement;
            }

            return idElem ?? throw new InvalidOperationException($"No se encontró el elemento con Id='{id}'");
        }
    }
}
