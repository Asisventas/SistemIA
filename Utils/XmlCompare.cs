using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace SistemIA.Utils
{
    public class XmlCompareResult
    {
        public HashSet<string> ReferenceElements { get; set; } = new();
        public HashSet<string> OurElements { get; set; } = new();
        public List<string> MissingElements { get; set; } = new();
        public List<string> ExtraElements { get; set; } = new();
        public Dictionary<string, List<string>> MissingAttributes { get; set; } = new();
        public Dictionary<string, List<string>> ExtraAttributes { get; set; } = new();
    }

    public static class XmlCompare
    {
        private static void RemoveSignatureNodes(XmlDocument doc)
        {
            try
            {
                if (doc.DocumentElement == null) return;
                // Eliminar cualquier nodo de firma (ds:Signature o sin prefijo) en todo el documento
                var toRemove = new List<XmlNode>();
                void Walk(XmlNode n)
                {
                    foreach (XmlNode child in n.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            var el = (XmlElement)child;
                            if (string.Equals(el.LocalName, "Signature", StringComparison.OrdinalIgnoreCase))
                            {
                                toRemove.Add(el);
                            }
                            else
                            {
                                Walk(el);
                            }
                        }
                    }
                }
                Walk(doc.DocumentElement);
                foreach (var n in toRemove)
                {
                    n.ParentNode?.RemoveChild(n);
                }
            }
            catch { /* ignorar */ }
        }

        private static void Flatten(XmlNode? node, string path, HashSet<string> elements, Dictionary<string, HashSet<string>> attrs)
        {
            if (node == null) return;
            if (node.NodeType == XmlNodeType.Element)
            {
                var elem = (XmlElement)node;
                var name = elem.LocalName;
                var current = string.IsNullOrEmpty(path) ? name : path + "/" + name;
                elements.Add(current);
                if (elem.HasAttributes)
                {
                    if (!attrs.TryGetValue(current, out var set))
                    {
                        set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        attrs[current] = set;
                    }
                    foreach (XmlAttribute a in elem.Attributes)
                    {
                        set.Add("@" + a.LocalName);
                    }
                }
                foreach (XmlNode child in elem.ChildNodes)
                {
                    Flatten(child, current, elements, attrs);
                }
            }
        }

        public static XmlCompareResult Compare(string referenceXml, string ourXml)
        {
            var refDoc = new XmlDocument();
            refDoc.PreserveWhitespace = true;
            refDoc.LoadXml(referenceXml);
            var ourDoc = new XmlDocument();
            ourDoc.PreserveWhitespace = true;
            ourDoc.LoadXml(ourXml);

            var refElems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var ourElems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var refAttrs = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            var ourAttrs = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            Flatten(refDoc.DocumentElement, string.Empty, refElems, refAttrs);
            Flatten(ourDoc.DocumentElement, string.Empty, ourElems, ourAttrs);

            var missing = refElems.Except(ourElems).OrderBy(x => x).ToList();
            var extra = ourElems.Except(refElems).OrderBy(x => x).ToList();

            var missingAttrs = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var extraAttrs = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in refElems.Intersect(ourElems))
            {
                var refA = refAttrs.TryGetValue(path, out var ra) ? ra : new HashSet<string>();
                var ourA = ourAttrs.TryGetValue(path, out var oa) ? oa : new HashSet<string>();
                var missA = refA.Except(ourA).OrderBy(x => x).ToList();
                var extraA = ourA.Except(refA).OrderBy(x => x).ToList();
                if (missA.Count > 0) missingAttrs[path] = missA;
                if (extraA.Count > 0) extraAttrs[path] = extraA;
            }

            return new XmlCompareResult
            {
                ReferenceElements = refElems,
                OurElements = ourElems,
                MissingElements = missing,
                ExtraElements = extra,
                MissingAttributes = missingAttrs,
                ExtraAttributes = extraAttrs
            };
        }

        // Igual que Compare, pero ignorando cualquier nodo de firma XML (ds:Signature)
        public static XmlCompareResult CompareIgnoringSignature(string referenceXml, string ourXml)
        {
            var refDoc = new XmlDocument();
            refDoc.PreserveWhitespace = true;
            refDoc.LoadXml(referenceXml);
            var ourDoc = new XmlDocument();
            ourDoc.PreserveWhitespace = true;
            ourDoc.LoadXml(ourXml);

            RemoveSignatureNodes(refDoc);
            RemoveSignatureNodes(ourDoc);

            var refElems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var ourElems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var refAttrs = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            var ourAttrs = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            Flatten(refDoc.DocumentElement, string.Empty, refElems, refAttrs);
            Flatten(ourDoc.DocumentElement, string.Empty, ourElems, ourAttrs);

            var missing = refElems.Except(ourElems).OrderBy(x => x).ToList();
            var extra = ourElems.Except(refElems).OrderBy(x => x).ToList();

            var missingAttrs = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var extraAttrs = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in refElems.Intersect(ourElems))
            {
                var refA = refAttrs.TryGetValue(path, out var ra) ? ra : new HashSet<string>();
                var ourA = ourAttrs.TryGetValue(path, out var oa) ? oa : new HashSet<string>();
                var missA = refA.Except(ourA).OrderBy(x => x).ToList();
                var extraA = ourA.Except(refA).OrderBy(x => x).ToList();
                if (missA.Count > 0) missingAttrs[path] = missA;
                if (extraA.Count > 0) extraAttrs[path] = extraA;
            }

            return new XmlCompareResult
            {
                ReferenceElements = refElems,
                OurElements = ourElems,
                MissingElements = missing,
                ExtraElements = extra,
                MissingAttributes = missingAttrs,
                ExtraAttributes = extraAttrs
            };
        }

        public static string? FindDefaultReferenceXml(string contentRoot)
        {
            // Busca el primer .xml dentro de ManualSifen/xml (si existe)
            try
            {
                var baseDir = Path.Combine(contentRoot, "ManualSifen", "xml");
                if (!Directory.Exists(baseDir)) return null;
                var first = Directory.GetFiles(baseDir, "*.xml", SearchOption.AllDirectories).FirstOrDefault();
                return first;
            }
            catch { return null; }
        }
    }
}
