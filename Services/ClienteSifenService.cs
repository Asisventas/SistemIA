using SistemIA.Models;
using System.Xml.Linq;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para convertir datos de Cliente al formato requerido por SIFEN
    /// Basado en Manual Técnico SIFEN v150
    /// </summary>
    public class ClienteSifenService
    {
        /// <summary>
        /// Genera el elemento gDatRec (Datos del Receptor) para SIFEN
        /// </summary>
        public XElement GenerarDatosReceptorSifen(Cliente cliente)
        {
            // Validar datos mínimos requeridos
            var (esValido, errores) = ValidarClienteParaSifen(cliente);
            if (!esValido)
            {
                throw new InvalidOperationException($"Cliente no válido para SIFEN: {string.Join(", ", errores)}");
            }

            var gDatRec = new XElement("gDatRec");

            // iNatRec - Naturaleza del receptor (1=Contribuyente, 2=No contribuyente)
            var naturaleza = ObtenerNaturalezaReceptor(cliente);
            gDatRec.Add(new XElement("iNatRec", naturaleza));

            // iTiOpe - Tipo de operación (1=B2B, 2=B2C, 3=B2G, 4=B2F)
            var tipoOperacion = ObtenerTipoOperacion(cliente);
            gDatRec.Add(new XElement("iTiOpe", tipoOperacion));

            // cPaisRec - Código del país del receptor
            gDatRec.Add(new XElement("cPaisRec", cliente.CodigoPais));

            // dDesPaisRe - Descripción del país
            // Datos de país 
            var paisReceptor = cliente.Pais?.Nombre ?? "Paraguay";
            gDatRec.Add(new XElement("dDesPaisRe", paisReceptor));

            // iTiContRec - Tipo de contribuyente del receptor (cuando aplica)
            gDatRec.Add(new XElement("iTiContRec", cliente.IdTipoContribuyente));

            if (naturaleza == 1) // Contribuyente
            {
                // dRucRec - RUC del receptor
                gDatRec.Add(new XElement("dRucRec", cliente.RUC));

                // dDVRec - Dígito verificador
                gDatRec.Add(new XElement("dDVRec", cliente.DV));
            }
            else // No contribuyente
            {
                // Para no contribuyentes, agregar tipo y número de documento (SIFEN)
                if (cliente.TipoDocumentoIdentidadSifen.HasValue && !string.IsNullOrWhiteSpace(cliente.NumeroDocumentoIdentidad))
                {
                    gDatRec.Add(new XElement("iTipIDRec", cliente.TipoDocumentoIdentidadSifen.Value));
                    gDatRec.Add(new XElement("dNumIDRec", cliente.NumeroDocumentoIdentidad));
                }
                else
                {
                    // Fallback a Innominado si no se proporcionan datos de documento
                    gDatRec.Add(new XElement("iTipIDRec", 5));
                    gDatRec.Add(new XElement("dDTipIDRec", "Innominado"));
                    gDatRec.Add(new XElement("dNumIDRec", "0"));
                }
            }

            // dNomRec - Nombre o razón social del receptor
            gDatRec.Add(new XElement("dNomRec", string.IsNullOrWhiteSpace(cliente.RazonSocial) && naturaleza == 2 ? "Sin Nombre" : cliente.RazonSocial));

            // Campos opcionales pero recomendados
            if (!string.IsNullOrEmpty(cliente.Direccion))
            {
                gDatRec.Add(new XElement("dDirRec", cliente.Direccion));
            }

            // dNumCasRec - SIEMPRE se agrega según XML aprobado por SIFEN (valor "0" si no hay dirección)
            // Corregido 22-Ene-2026: El XML aprobado muestra dNumCasRec="0" incluso sin dDirRec
            var numeroCasa = !string.IsNullOrEmpty(cliente.NumeroCasa) ? cliente.NumeroCasa : "0";
            gDatRec.Add(new XElement("dNumCasRec", numeroCasa));

            // Información geográfica (opcional, solo si hay dirección y ciudad)
            if (!string.IsNullOrEmpty(cliente.Direccion) && cliente.IdCiudad > 0)
            {
                // Recomendado: completar además cDepRec/dDesDepRec y cDisRec/dDesDisRec desde catálogo
                gDatRec.Add(new XElement("cCiuRec", cliente.IdCiudad));
                // dDesCiuRec opcional: no disponible aquí sin acceso a catálogo
            }

            // Teléfono (opcional)
            if (!string.IsNullOrEmpty(cliente.Telefono))
            {
                gDatRec.Add(new XElement("dTelRec", cliente.Telefono));
            }

            // Código interno del cliente (opcional)
            if (!string.IsNullOrEmpty(cliente.CodigoCliente))
            {
                gDatRec.Add(new XElement("dCodCliente", cliente.CodigoCliente));
            }

            return gDatRec;
        }

        /// <summary>
        /// Determina la naturaleza del receptor según los datos del cliente
        /// </summary>
        private int ObtenerNaturalezaReceptor(Cliente cliente)
        {
            // Priorizar el valor explícito almacenado
            if (cliente.NaturalezaReceptor == 1 || cliente.NaturalezaReceptor == 2)
                return cliente.NaturalezaReceptor;

            // Fallback por presencia de RUC
            if (!string.IsNullOrEmpty(cliente.RUC) && cliente.RUC != "0" && cliente.RUC.Length >= 3)
                return 1; // Contribuyente

            return 2; // No contribuyente por defecto
        }

        /// <summary>
        /// Obtiene el tipo de operación según el cliente
        /// Regla: RUC >= 50,000,000 = B2B (empresas/extranjeros), RUC < 50,000,000 = B2C (clientes/personas físicas)
        /// </summary>
        private int ObtenerTipoOperacion(Cliente cliente)
        {
            // Si el cliente tiene tipo de operación explícito, usarlo
            if (!string.IsNullOrEmpty(cliente.TipoOperacion))
            {
                return cliente.TipoOperacion switch
                {
                    "1" => 1, // B2B - Empresa a Empresa/Extranjero
                    "2" => 2, // B2C - Empresa a Cliente
                    "3" => 3, // B2G - Empresa a Gobierno
                    "4" => 4, // B2F - Empresa a Extranjero
                    _ => 1    // Por defecto B2B
                };
            }

            // Determinación automática basada en el valor del RUC
            // RUC >= 50,000,000 = B2B (empresas y extranjeros)
            // RUC < 50,000,000 = B2C (personas físicas/clientes)
            if (!string.IsNullOrEmpty(cliente.RUC) && long.TryParse(cliente.RUC, out long rucNumerico))
            {
                return rucNumerico >= 50_000_000 ? 1 : 2; // 1=B2B, 2=B2C
            }

            // Fallback: si no se puede determinar, usar B2C (más común)
            return 2; // B2C
        }

        /// <summary>
        /// Valida si el cliente cumple con los requisitos mínimos para SIFEN
        /// </summary>
        public (bool EsValido, List<string> Errores) ValidarClienteParaSifen(Cliente cliente)
        {
            var errores = new List<string>();

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(cliente.RazonSocial))
                errores.Add("La razón social es obligatoria para SIFEN");

            if (string.IsNullOrWhiteSpace(cliente.CodigoPais))
                errores.Add("El código de país es obligatorio para SIFEN");

            if (cliente.IdTipoContribuyente <= 0)
                errores.Add("El tipo de contribuyente debe ser válido para SIFEN");

            // Validaciones según naturaleza del receptor
            var naturaleza = ObtenerNaturalezaReceptor(cliente);

            if (naturaleza == 1) // Contribuyente
            {
                if (string.IsNullOrWhiteSpace(cliente.RUC))
                    errores.Add("El RUC es obligatorio para contribuyentes en SIFEN");

                if (cliente.DV <= 0)
                    errores.Add("El dígito verificador debe ser válido para contribuyentes en SIFEN");
            }
            else if (naturaleza == 2) // No contribuyente
            {
                // Permitir Innominado (iTipIDRec=5) si no se proporcionan datos, pero sugerir completarlos
                var tieneDoc = cliente.TipoDocumentoIdentidadSifen.HasValue && !string.IsNullOrWhiteSpace(cliente.NumeroDocumentoIdentidad);
                if (!tieneDoc)
                {
                    // No es error bloqueante porque se puede usar Innominado, pero advertimos
                    // errores.Add("Para no contribuyentes, se recomienda informar tipo y número de documento (o se usará Innominado)");
                }
            }

            // Validaciones de longitud según especificaciones SIFEN
            if (cliente.RazonSocial?.Length > 255)
                errores.Add("La razón social no puede exceder 255 caracteres");

            if (cliente.Direccion?.Length > 255)
                errores.Add("La dirección no puede exceder 255 caracteres");

            if (cliente.NumeroCasa?.Length > 6)
                errores.Add("El número de casa no puede exceder 6 caracteres");

            if (cliente.Telefono?.Length > 15)
                errores.Add("El teléfono no puede exceder 15 caracteres");

            return (errores.Count == 0, errores);
        }

        /// <summary>
        /// Genera un reporte de compatibilidad SIFEN para un cliente
        /// </summary>
        public string GenerarReporteCompatibilidad(Cliente cliente)
        {
            var (esValido, errores) = ValidarClienteParaSifen(cliente);
            var naturaleza = ObtenerNaturalezaReceptor(cliente);
            var tipoOperacion = ObtenerTipoOperacion(cliente);

            var reporte = $@"
=== REPORTE DE COMPATIBILIDAD SIFEN ===
Cliente: {cliente.RazonSocial}
RUC: {cliente.RUC}

ESTADO: {(esValido ? "✅ COMPATIBLE" : "❌ NO COMPATIBLE")}

CONFIGURACIÓN SIFEN:
- Naturaleza del receptor: {(naturaleza == 1 ? "Contribuyente" : "No contribuyente")}
- Tipo de operación: {ObtenerDescripcionTipoOperacion(tipoOperacion)}
- País: {cliente.CodigoPais} - {cliente.Pais?.Nombre ?? "No especificado"}

CAMPOS REQUERIDOS:
✅ Razón Social: {cliente.RazonSocial}
{(naturaleza == 1 ? $"✅ RUC: {cliente.RUC}" : $"✅ Documento: {cliente.TipoDocumento}-{cliente.NumeroDocumento}")}
{(naturaleza == 1 ? $"✅ DV: {cliente.DV}" : "")}
✅ Tipo Contribuyente: {cliente.IdTipoContribuyente}

CAMPOS OPCIONALES:
{(!string.IsNullOrEmpty(cliente.Direccion) ? $"✅ Dirección: {cliente.Direccion}" : "❌ Sin dirección")}
{(!string.IsNullOrEmpty(cliente.Telefono) ? $"✅ Teléfono: {cliente.Telefono}" : "❌ Sin teléfono")}
{(!string.IsNullOrEmpty(cliente.CodigoCliente) ? $"✅ Código Cliente: {cliente.CodigoCliente}" : "❌ Sin código cliente")}

{(errores.Any() ? $"ERRORES ENCONTRADOS:\n{string.Join("\n", errores.Select(e => $"❌ {e}"))}" : "")}

=== FIN DEL REPORTE ===";

            return reporte;
        }

        private string ObtenerDescripcionTipoOperacion(int tipo)
        {
            return tipo switch
            {
                1 => "B2B - Empresa a Empresa/Extranjero",
                2 => "B2C - Empresa a Cliente", 
                3 => "B2G - Empresa a Gobierno",
                4 => "B2F - Empresa a Extranjero",
                _ => "No definido"
            };
        }
    }
}
