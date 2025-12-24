using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace SistemIA.Utils
{
    public static class XmlValidationUtil
    {
        // Busca una carpeta que contenga DE_v150.xsd dentro de ManualSifen y devuelve su ruta
        public static string? FindSifenXsdFolder(string contentRootPath)
        {
            try
            {
                var manualDir = Path.Combine(contentRootPath, "ManualSifen");
                if (!Directory.Exists(manualDir)) return null;
                // Preferir carpeta con dominio estructurado
                var preferred = Path.Combine(manualDir, "codigoabierto", "docs", "set", "ekuatia.set.gov.py", "sifen", "xsd");
                if (Directory.Exists(preferred) && File.Exists(Path.Combine(preferred, "DE_v150.xsd")))
                    return preferred;

                // Búsqueda recursiva como fallback
                var xsdFiles = Directory.GetFiles(manualDir, "DE_v150.xsd", SearchOption.AllDirectories);
                if (xsdFiles.Length > 0)
                {
                    return Path.GetDirectoryName(xsdFiles[0]);
                }
            }
            catch { /* ignore */ }
            return null;
        }

        // Carga todos los XSDs de una carpeta en un XmlSchemaSet para validar el DE firmado
        private static XmlSchemaSet LoadSchemasFromFolder(string folder)
        {
            var set = new XmlSchemaSet();
            set.XmlResolver = new XmlUrlResolver();
            // Cargar todos los .xsd en la carpeta (evitar subcarpetas para minimizar conflictos de versiones)
            foreach (var file in Directory.EnumerateFiles(folder, "*.xsd", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    using var fs = File.OpenRead(file);
                    var schema = XmlSchema.Read(fs, null);
                    if (schema != null)
                    {
                        set.Add(schema);
                    }
                }
                catch
                {
                    // Continuar aunque un XSD falle al cargar
                }
            }
            set.CompilationSettings = new XmlSchemaCompilationSettings { EnableUpaCheck = false };
            set.Compile();
            return set;
        }

        // Valida un XML (rDE firmado) contra el conjunto de XSDs de SIFEN; devuelve lista de errores/advertencias
        public static List<string> ValidateDeSignedXml(string xml, string contentRootPath)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(xml))
            {
                errors.Add("XML vacío");
                return errors;
            }

            var xsdFolder = FindSifenXsdFolder(contentRootPath);
            if (xsdFolder == null)
            {
                errors.Add("No se encontró carpeta con XSDs de SIFEN (DE_v150.xsd)");
                return errors;
            }

            var schemas = LoadSchemasFromFolder(xsdFolder);

            var settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schemas,
                DtdProcessing = DtdProcessing.Ignore
            };
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationEventHandler += (sender, e) =>
            {
                var lineInfo = e.Exception != null ? $" (L{e.Exception.LineNumber}, C{e.Exception.LinePosition})" : string.Empty;
                errors.Add($"{e.Severity}: {e.Message}{lineInfo}");
            };

            try
            {
                using var sr = new StringReader(xml);
                using var reader = XmlReader.Create(sr, settings);
                while (reader.Read()) { /* recorrer para disparar validación */ }
            }
            catch (XmlException xex)
            {
                errors.Add($"XML mal formado: {xex.Message} (L{xex.LineNumber}, C{xex.LinePosition})");
            }
            catch (Exception ex)
            {
                errors.Add($"Error al validar: {ex.Message}");
            }

            // Normalizar duplicados y ordenar
            return errors.Distinct().OrderBy(s => s).ToList();
        }
    }
}
