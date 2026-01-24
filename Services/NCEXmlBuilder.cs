using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Constructor de XML para Nota de Crédito Electrónica (NCE) - iTiDE=5
    /// Basado en la especificación SIFEN v150 de Paraguay
    /// </summary>
    public class NCEXmlBuilder
    {
        private static readonly XNamespace NsSifen = "http://ekuatia.set.gov.py/sifen/xsd";

        /// <summary>
        /// Construye el XML del Documento Electrónico para una Nota de Crédito
        /// </summary>
        public static async Task<string> ConstruirXmlAsync(NotaCreditoVenta nc, AppDbContext ctx)
        {
            // Helper para extraer solo dígitos de un string
            static string Digits(string? s) => new string((s ?? string.Empty).Where(char.IsDigit).ToArray());
            
            // Cargar datos relacionados si no están cargados
            if (nc.Sucursal == null)
                nc.Sucursal = await ctx.Sucursal.FirstOrDefaultAsync(s => s.Id == nc.IdSucursal);
            if (nc.Cliente == null && nc.IdCliente.HasValue)
                nc.Cliente = await ctx.Clientes.FirstOrDefaultAsync(c => c.IdCliente == nc.IdCliente);
            if (nc.Detalles == null || !nc.Detalles.Any())
                nc.Detalles = await ctx.NotasCreditoVentasDetalles.Include(d => d.Producto).Where(d => d.IdNotaCredito == nc.IdNotaCredito).ToListAsync();
            if (nc.VentaAsociada == null && nc.IdVentaAsociada.HasValue)
                nc.VentaAsociada = await ctx.Ventas.FirstOrDefaultAsync(v => v.IdVenta == nc.IdVentaAsociada);
            if (nc.Caja == null)
                nc.Caja = await ctx.Cajas.FirstOrDefaultAsync(c => c.IdCaja == nc.IdCaja);
            if (nc.Moneda == null)
                nc.Moneda = await ctx.Monedas.FirstOrDefaultAsync(m => m.IdMoneda == nc.IdMoneda);

            var sucursal = nc.Sucursal ?? throw new Exception("Sucursal no encontrada");
            
            // Cargar Sociedad desde el DbSet (igual que DEXmlBuilder)
            var sociedad = await ctx.Sociedades.AsNoTracking().FirstOrDefaultAsync()
                ?? throw new Exception("Sociedad no encontrada");
            
            var cliente = nc.Cliente;
            var detalles = nc.Detalles?.ToList() ?? new List<NotaCreditoVentaDetalle>();
            var ventaAsociada = nc.VentaAsociada;
            var caja = nc.Caja;
            var moneda = nc.Moneda;

            // Validar que existe la venta asociada con CDC
            if (ventaAsociada == null || string.IsNullOrWhiteSpace(ventaAsociada.CDC))
                throw new Exception("La Nota de Crédito debe tener una venta asociada con CDC válido para generar NCE");

            // Tipo de documento electrónico: 5 = Nota de Crédito Electrónica
            const string tipoDocumento = "05";

            // Datos del emisor - usar SOCIEDAD (contribuyente registrado en SIFEN)
            string rucEmisor = sociedad.RUC?.Replace("-", "").Replace(".", "").Trim() ?? "";
            string dvEmisor = sociedad.DV?.ToString() ?? "0";
            // Determinar tipo de contribuyente del emisor (1 = Persona Física, 2 = Persona Jurídica)
            string tipoContribuyente = (sociedad.TipoContribuyente ?? 2).ToString();

            // Establecimiento y punto de expedición
            string establecimiento = (nc.Establecimiento ?? sucursal.NumSucursal?.PadLeft(3, '0') ?? "001").PadLeft(3, '0');
            string puntoExpedicion = (nc.PuntoExpedicion ?? caja?.Nivel2NC ?? caja?.Nivel2 ?? "001").PadLeft(3, '0');
            string numeroDoc = nc.NumeroNota.ToString().PadLeft(7, '0');

            // Generar CDC (44 dígitos)
            string idCdc = Utils.CdcGenerator.GenerarCDC(
                tipoDocumento: tipoDocumento,
                rucEmisor: rucEmisor,
                dvEmisor: dvEmisor,
                establecimiento: establecimiento,
                puntoExpedicion: puntoExpedicion,
                numeroFactura: numeroDoc,
                tipoContribuyente: tipoContribuyente,
                fechaEmision: nc.Fecha,
                tipoEmision: "1" // Normal
            );

            // Guardar CDC y código de seguridad en la NC
            nc.CDC = idCdc;
            nc.CodigoSeguridad = idCdc.Length >= 43 ? idCdc.Substring(34, 9) : "000000001";

            // Ambiente (test/prod) - FIX 22-Ene-2026: Detectar ambiente primero para decidir timbrado
            // CRÍTICO: Misma lógica que DEXmlBuilder.cs
            var ambiente = (sociedad.ServidorSifen ?? "test").ToLowerInvariant();
            
            // Timbrado - FIX 22-Ene-2026: Aplicar lógica TEST/PROD como DEXmlBuilder
            // CRÍTICO para ambiente TEST: dNumTim debe ser el RUC del emisor (sin DV) con padding a 8 caracteres
            // CRÍTICO para ambiente PROD: dNumTim es el número de timbrado asignado por SET
            string timbrado;
            if (ambiente == "prod")
            {
                // Producción: usar el timbrado real de la Caja para NC
                timbrado = Digits(nc.Timbrado ?? caja?.TimbradoNC ?? caja?.Timbrado ?? string.Empty).PadLeft(8, '0');
            }
            else
            {
                // TEST: usar el RUC de la Sociedad (sin DV) con padding a 8 caracteres
                // Ejemplo: RUC 495219 → "00495219"
                timbrado = Digits(sociedad.RUC).PadLeft(8, '0');
            }

            // Fecha inicio vigencia del timbrado
            string fechaIniT = caja?.VigenciaDelNC?.ToString("yyyy-MM-dd") ?? caja?.VigenciaDel?.ToString("yyyy-MM-dd") ?? nc.Fecha.AddMonths(-1).ToString("yyyy-MM-dd");

            // ========== gTimb (Timbrado) ==========
            var gTimb = new XElement(NsSifen + "gTimb",
                new XElement(NsSifen + "iTiDE", 5),  // 5 = Nota de Crédito Electrónica
                new XElement(NsSifen + "dDesTiDE", "Nota de crédito electrónica"),
                new XElement(NsSifen + "dNumTim", string.IsNullOrWhiteSpace(timbrado) ? "00000000" : timbrado),
                new XElement(NsSifen + "dEst", establecimiento),
                new XElement(NsSifen + "dPunExp", puntoExpedicion),
                new XElement(NsSifen + "dNumDoc", numeroDoc),
                new XElement(NsSifen + "dFeIniT", fechaIniT)
            );

            // ========== gOpeCom (Operación Comercial) ==========
            // NC no tiene iTipTra ni dDesTipTra (solo facturas)
            string codigoMoneda = moneda?.CodigoISO ?? "PYG";
            // FIX 22-Ene-2026: Usar descripciones del catálogo SIFEN, no de la tabla Monedas
            string nombreMoneda = codigoMoneda switch
            {
                "PYG" => "Guarani",
                "USD" => "Dolar americano",
                "BRL" => "Real",
                "EUR" => "Euro",
                "ARS" => "Peso Argentino",
                _ => "Guarani"
            };

            // FIX 22-Ene-2026: SÍ incluir gOblAfe - El log de NC aprobada por PowerBuilder lo incluye
            // Ver: .ai-docs/SifenProyecto2026/LogNCAprobado/sifen_xml_firmado.txt
            var gOblAfe = new XElement(NsSifen + "gOblAfe",
                new XElement(NsSifen + "cOblAfe", "211"),
                new XElement(NsSifen + "dDesOblAfe", "IMPUESTO AL VALOR AGREGADO - GRAVADAS Y EXONERADAS - EXPORTADORES")
            );

            var gOpeCom = new XElement(NsSifen + "gOpeCom",
                new XElement(NsSifen + "iTImp", 1),  // IVA
                new XElement(NsSifen + "dDesTImp", "IVA"),
                new XElement(NsSifen + "cMoneOpe", codigoMoneda),
                new XElement(NsSifen + "dDesMoneOpe", nombreMoneda),
                gOblAfe  // Requerido según log de NC aprobada
            );

            // Si es moneda extranjera, agregar tipo de cambio
            if (codigoMoneda != "PYG" && nc.CambioDelDia > 0)
            {
                gOpeCom.Add(
                    new XElement(NsSifen + "dCondTiCam", 1),
                    new XElement(NsSifen + "dTiCam", nc.CambioDelDia.ToString("0.0000", CultureInfo.InvariantCulture))
                );
            }

            // ========== gEmis (Emisor) ==========
            string nombreEmisor = sociedad.Nombre ?? sucursal.NombreEmpresa ?? "EMPRESA";
            string nombreFantasia = sociedad.Nombre ?? nombreEmisor;
            string direccionEmisor = sociedad.Direccion ?? sucursal.Direccion ?? "DIRECCION";

            var gEmis = new XElement(NsSifen + "gEmis",
                new XElement(NsSifen + "dRucEm", rucEmisor),
                new XElement(NsSifen + "dDVEmi", dvEmisor),
                new XElement(NsSifen + "iTipCont", int.Parse(tipoContribuyente)),
                new XElement(NsSifen + "dNomEmi", nombreEmisor.Length > 100 ? nombreEmisor[..100] : nombreEmisor),
                new XElement(NsSifen + "dNomFanEmi", nombreFantasia.Length > 100 ? nombreFantasia[..100] : nombreFantasia),
                new XElement(NsSifen + "dDirEmi", direccionEmisor.Length > 255 ? direccionEmisor[..255] : direccionEmisor),
                new XElement(NsSifen + "dNumCas", sociedad.NumeroCasa ?? "0"),
                new XElement(NsSifen + "cDepEmi", sociedad.Departamento ?? 11),
                new XElement(NsSifen + "dDesDepEmi", ObtenerNombreDepartamento(sociedad.Departamento ?? 11)),
                new XElement(NsSifen + "cDisEmi", sociedad.Distrito ?? 164),
                new XElement(NsSifen + "dDesDisEmi", ObtenerNombreDistrito(sociedad.Distrito ?? 164, ctx)),
                new XElement(NsSifen + "cCiuEmi", sociedad.Ciudad ?? 3840),
                new XElement(NsSifen + "dDesCiuEmi", ObtenerNombreCiudad(sociedad.Ciudad ?? 3840, ctx))
            );

            // Teléfono y email del emisor (opcionales)
            if (!string.IsNullOrWhiteSpace(sucursal.Telefono))
                gEmis.Add(new XElement(NsSifen + "dTelEmi", sucursal.Telefono.Length > 15 ? sucursal.Telefono[..15] : sucursal.Telefono));
            if (!string.IsNullOrWhiteSpace(sociedad.Email))
                gEmis.Add(new XElement(NsSifen + "dEmailE", sociedad.Email.Length > 80 ? sociedad.Email[..80] : sociedad.Email));

            // Cargar actividades económicas de la sociedad
            var actividadesEconomicas = await ctx.SociedadesActividades
                .Where(a => a.IdSociedad == sociedad.IdSociedad)
                .OrderByDescending(a => a.ActividadPrincipal == "S")
                .Take(9)
                .ToListAsync();
            
            // Si no hay actividades, usar una por defecto
            if (!actividadesEconomicas.Any())
            {
                actividadesEconomicas.Add(new SociedadActividadEconomica
                {
                    CodigoActividad = "47190",
                    NombreActividad = "VENTA AL POR MENOR DE OTROS PRODUCTOS EN COMERCIOS NO ESPECIALIZADOS"
                });
            }
            
            // Agregar actividades económicas (obligatorio al menos 1)
            foreach (var act in actividadesEconomicas)
            {
                gEmis.Add(new XElement(NsSifen + "gActEco",
                    new XElement(NsSifen + "cActEco", act.CodigoActividad),
                    new XElement(NsSifen + "dDesActEco", act.NombreActividad ?? "ACTIVIDAD ECONOMICA")
                ));
            }

            // ========== gDatRec (Receptor) ==========
            bool esContribuyente = cliente?.NaturalezaReceptor == 1;
            int iNatRec = cliente?.NaturalezaReceptor ?? 2;
            int iTiOpe = iNatRec == 1 ? 1 : 2; // 1=B2B, 2=B2C

            string cliRuc = cliente?.RUC?.Replace("-", "").Replace(".", "").Trim() ?? "";
            string cliDv = cliente?.DV.ToString() ?? "0";
            string cliNombre = cliente?.RazonSocial ?? nc.NombreCliente ?? "SIN NOMBRE";
            string cliPais = cliente?.CodigoPais ?? "PRY";
            string cliPaisNombre = cliPais == "PRY" ? "Paraguay" : "Otro";

            var gDatRec = new XElement(NsSifen + "gDatRec",
                new XElement(NsSifen + "iNatRec", iNatRec),
                new XElement(NsSifen + "iTiOpe", iTiOpe),
                new XElement(NsSifen + "cPaisRec", cliPais),
                new XElement(NsSifen + "dDesPaisRe", cliPaisNombre)
            );

            // Tipo de contribuyente receptor (solo si es contribuyente)
            if (esContribuyente)
            {
                gDatRec.Add(new XElement(NsSifen + "iTiContRec", cliente?.IdTipoContribuyente ?? 2));
            }

            // RUC o documento de identidad
            string dNumIDRec = "";
            if (esContribuyente && !string.IsNullOrWhiteSpace(cliRuc))
            {
                string rucDigits = new string(cliRuc.Where(char.IsDigit).ToArray());
                gDatRec.Add(new XElement(NsSifen + "dRucRec", rucDigits));
                gDatRec.Add(new XElement(NsSifen + "dDVRec", cliDv));
            }
            else
            {
                // FIX 23-Ene-2026: Para No Contribuyentes, si tiene TipoDocumentoIdentidadSifen Y NumeroDocumentoIdentidad, usarlos.
                // Si no tiene datos de documento, caer a Innominado (tipo 5) para evitar errores SIFEN.
                int tipoDoc = cliente?.TipoDocumentoIdentidadSifen ?? 5;
                string numDoc = cliente?.NumeroDocumentoIdentidad ?? "";
                
                // Si el tipo es distinto de Innominado (5) pero no tiene número de documento, caer a Innominado
                if (tipoDoc != 5 && string.IsNullOrWhiteSpace(numDoc))
                {
                    tipoDoc = 5; // Innominado
                    numDoc = "0";
                }
                else if (string.IsNullOrWhiteSpace(numDoc))
                {
                    numDoc = "0";
                }
                
                dNumIDRec = numDoc;
                gDatRec.Add(new XElement(NsSifen + "iTipIDRec", tipoDoc));
                gDatRec.Add(new XElement(NsSifen + "dDTipIDRec", DescripcionTipoDocRec(tipoDoc)));

                gDatRec.Add(new XElement(NsSifen + "dNumIDRec", numDoc.Length > 20 ? numDoc[..20] : numDoc));
            }

            gDatRec.Add(new XElement(NsSifen + "dNomRec", cliNombre.Length > 255 ? cliNombre[..255] : cliNombre));

            // Dirección del receptor (opcional)
            if (!string.IsNullOrWhiteSpace(cliente?.Direccion))
            {
                gDatRec.Add(new XElement(NsSifen + "dDirRec", cliente.Direccion.Length > 255 ? cliente.Direccion[..255] : cliente.Direccion));
                gDatRec.Add(new XElement(NsSifen + "dNumCasRec", cliente.NumeroCasa ?? "0"));
            }

            // ========== gDatGralOpe (Datos Generales de la Operación) ==========
            var gDatGralOpe = new XElement(NsSifen + "gDatGralOpe",
                new XElement(NsSifen + "dFeEmiDE", nc.Fecha.ToString("yyyy-MM-ddTHH:mm:ss")),
                gOpeCom,
                gEmis,
                gDatRec
            );

            // ========== gCamNCDE (Campos específicos de Nota de Crédito) ==========
            int iMotEmi = ObtenerCodigoMotivo(nc.Motivo);
            string dDesMotEmi = nc.Motivo ?? "Devolución";

            var gCamNCDE = new XElement(NsSifen + "gCamNCDE",
                new XElement(NsSifen + "iMotEmi", iMotEmi),
                new XElement(NsSifen + "dDesMotEmi", dDesMotEmi.Length > 60 ? dDesMotEmi[..60] : dDesMotEmi)
            );

            // ========== gCamItem (Items) ==========
            var itemsXml = new List<XElement>();
            foreach (var det in detalles)
            {
                string codItem = det.CodigoProducto ?? det.Producto?.CodigoInterno ?? "PROD";
                string nomItem = det.NombreProducto ?? det.Producto?.Descripcion ?? "PRODUCTO";
                string unidadMedidaCodigo = det.Producto?.UnidadMedidaCodigo ?? "77"; // 77 = UNI

                decimal cantidad = det.Cantidad;
                decimal precioUnit = det.PrecioUnitario;
                decimal descuento = det.MontoDescuento;
                decimal importe = det.Importe;

                // IVA - Catálogo SIFEN v150:
                // 1 = Gravado IVA (incluye 10% y 5%)
                // 2 = Exonerado (Art. 83 Ley 125/91)
                // 3 = Exento
                // 4 = Gravado parcial (Grav-Exe)
                int tasaIva = det.TasaIVA;
                int iAfecIVA = tasaIva > 0 ? 1 : 3; // 1=Gravado IVA (10% o 5%), 3=Exento
                string dDesAfecIVA = tasaIva > 0 ? "Gravado IVA" : "Exento";
                
                decimal baseGravIva = tasaIva > 0 ? importe - (tasaIva == 10 ? det.IVA10 : det.IVA5) : 0;
                decimal liqIvaItem = tasaIva == 10 ? det.IVA10 : (tasaIva == 5 ? det.IVA5 : 0);

                var gCamItem = new XElement(NsSifen + "gCamItem",
                    new XElement(NsSifen + "dCodInt", codItem.Length > 50 ? codItem[..50] : codItem),
                    new XElement(NsSifen + "dDesProSer", nomItem.Length > 500 ? nomItem[..500] : nomItem),
                    new XElement(NsSifen + "cUniMed", unidadMedidaCodigo),
                    new XElement(NsSifen + "dDesUniMed", ObtenerNombreUnidadMedida(int.TryParse(unidadMedidaCodigo, out var um) ? um : 77)),
                    new XElement(NsSifen + "dCantProSer", cantidad.ToString("0.0000", CultureInfo.InvariantCulture))
                );

                // Valor del item
                var gValorItem = new XElement(NsSifen + "gValorItem",
                    new XElement(NsSifen + "dPUniProSer", precioUnit.ToString("0.00000000", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "dTotBruOpeItem", importe.ToString("0", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "gValorRestaItem",
                        new XElement(NsSifen + "dDescItem", descuento.ToString("0", CultureInfo.InvariantCulture)),
                        new XElement(NsSifen + "dPorcDesIt", det.PorcentajeDescuento.ToString("0.00", CultureInfo.InvariantCulture)),
                        new XElement(NsSifen + "dTotOpeItem", importe.ToString("0", CultureInfo.InvariantCulture))
                    )
                );
                gCamItem.Add(gValorItem);

                // IVA del item
                // FIX 22-Ene-2026: Según XML aprobado LogNCAprobado/sifen_xml_firmado.txt
                // dBasExe SIEMPRE es 0, independientemente del tipo de IVA (gravado, exento, exonerado)
                // El subtotal exento se calcula en gTotSub.dSubExe, no en gCamIVA.dBasExe
                var gCamIVA = new XElement(NsSifen + "gCamIVA",
                    new XElement(NsSifen + "iAfecIVA", iAfecIVA),
                    new XElement(NsSifen + "dDesAfecIVA", dDesAfecIVA),
                    new XElement(NsSifen + "dPropIVA", iAfecIVA == 1 ? 100 : 0), // 100% cuando gravado, 0% cuando exento
                    new XElement(NsSifen + "dTasaIVA", tasaIva),
                    new XElement(NsSifen + "dBasGravIVA", baseGravIva.ToString("0", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "dLiqIVAItem", liqIvaItem.ToString("0", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "dBasExe", "0")  // SIEMPRE 0 según XML aprobado
                );
                gCamItem.Add(gCamIVA);

                itemsXml.Add(gCamItem);
            }

            // ========== gDtipDE (Datos del tipo de DE) ==========
            var gDtipDE = new XElement(NsSifen + "gDtipDE", gCamNCDE);
            foreach (var item in itemsXml)
                gDtipDE.Add(item);

            // ========== gTotSub (Totales) ==========
            // Calcular subExe desde los detalles para que coincida con la suma de dBasExe de items exentos
            decimal subExe = detalles.Where(d => d.TasaIVA == 0).Sum(d => d.Importe);
            decimal sub5 = detalles.Where(d => d.TasaIVA == 5).Sum(d => d.Importe);
            decimal sub10 = detalles.Where(d => d.TasaIVA == 10).Sum(d => d.Importe);
            decimal totOpe = nc.Total;
            decimal iva5 = nc.TotalIVA5;
            decimal iva10 = nc.TotalIVA10;
            decimal totIVA = iva5 + iva10;
            decimal baseGrav5 = sub5 > 0 ? sub5 - iva5 : 0;
            decimal baseGrav10 = sub10 > 0 ? sub10 - iva10 : 0;
            decimal tBaseGravIva = baseGrav5 + baseGrav10;

            var gTotSub = new XElement(NsSifen + "gTotSub",
                new XElement(NsSifen + "dSubExe", subExe.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dSub5", sub5.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dSub10", sub10.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTotOpe", totOpe.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTotDesc", nc.TotalDescuento.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTotDescGlotem", "0"),
                new XElement(NsSifen + "dTotAntItem", "0"),
                new XElement(NsSifen + "dTotAnt", "0"),
                new XElement(NsSifen + "dPorcDescTotal", "0"),
                new XElement(NsSifen + "dDescTotal", "0"),
                new XElement(NsSifen + "dAnticipo", "0"),
                new XElement(NsSifen + "dRedon", "0"),
                new XElement(NsSifen + "dTotGralOpe", totOpe.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dIVA5", iva5.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dIVA10", iva10.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTotIVA", totIVA.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dBaseGrav5", baseGrav5.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dBaseGrav10", baseGrav10.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTBasGraIVA", tBaseGravIva.ToString("0", CultureInfo.InvariantCulture))
            );

            // ========== gCamDEAsoc (Documento Electrónico Asociado) ==========
            // Referencia a la factura original
            var gCamDEAsoc = new XElement(NsSifen + "gCamDEAsoc",
                new XElement(NsSifen + "iTipDocAso", 1),  // 1 = Electrónico
                new XElement(NsSifen + "dDesTipDocAso", "Electrónico"),
                new XElement(NsSifen + "dCdCDERef", ventaAsociada.CDC)  // CDC de la factura original
            );

            // ========== QR ==========
            string qrIdParamName;
            string qrIdParamValue;
            if (esContribuyente)
            {
                qrIdParamName = "dRucRec";
                string cliRucDigits = new string(cliRuc.Where(char.IsDigit).ToArray());
                qrIdParamValue = string.IsNullOrWhiteSpace(cliRucDigits) ? "0" : cliRucDigits;
            }
            else
            {
                qrIdParamName = "dNumIDRec";
                qrIdParamValue = string.IsNullOrWhiteSpace(dNumIDRec) ? "0" : dNumIDRec;
            }

            var digestPlaceholder = "DigestValue=" +
                "665569394474586a4f4a396970724970754f344c434a75706a457a73645766664846656d573270344c69593d";
            
            string defaultQr = ambiente == "prod" 
                ? "https://ekuatia.set.gov.py/consultas/qr?"
                : "https://ekuatia.set.gov.py/consultas-test/qr?";
            string urlQrBase = sociedad.DeUrlQr ?? defaultQr;
            if (!urlQrBase.Contains("?")) urlQrBase += "?";

            string fechaHex = BitConverter.ToString(Encoding.UTF8.GetBytes(nc.Fecha.ToString("yyyy-MM-ddTHH:mm:ss"))).Replace("-", "").ToLowerInvariant();

            string cscValue = sociedad.Csc ?? "ABCD0000000000000000000000000000";
            string idCscValue = (sociedad.IdCsc ?? "1").TrimStart('0');
            if (string.IsNullOrEmpty(idCscValue)) idCscValue = "1";

            string qrText = $"nVersion=150&Id={idCdc}&dFeEmiDE={fechaHex}&{qrIdParamName}={qrIdParamValue}&dTotGralOpe={totOpe.ToString("0", CultureInfo.InvariantCulture)}&dTotIVA={totIVA.ToString("0", CultureInfo.InvariantCulture)}&cItems={detalles.Count}&{digestPlaceholder}&IdCSC={idCscValue}{cscValue}";
            var dCarQR = new XElement(NsSifen + "dCarQR", qrText);

            // ========== DE principal ==========
            string dvId = idCdc.Length > 0 ? idCdc[^1].ToString() : "0";
            var codigoSeguridadDelCdc = idCdc.Length >= 43 ? idCdc.Substring(34, 9) : "000000001";

            var de = new XElement(NsSifen + "DE",
                new XAttribute("Id", idCdc),
                new XElement(NsSifen + "dDVId", dvId),
                new XElement(NsSifen + "dFecFirma", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")),
                new XElement(NsSifen + "dSisFact", 1),
                new XElement(NsSifen + "gOpeDE",
                    new XElement(NsSifen + "iTipEmi", 1),
                    new XElement(NsSifen + "dDesTipEmi", "Normal"),
                    new XElement(NsSifen + "dCodSeg", codigoSeguridadDelCdc)
                ),
                gTimb,
                gDatGralOpe,
                gDtipDE,
                gTotSub,
                gCamDEAsoc
            );

            // ========== rDE envolvente ==========
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

            var rde = new XElement(NsSifen + "rDE",
                new XAttribute("xmlns", NsSifen.NamespaceName),
                new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                new XAttribute(xsi + "schemaLocation", "http://ekuatia.set.gov.py/sifen/xsd siRecepDE_v150.xsd"),
                new XElement(NsSifen + "dVerFor", "150"),
                de,
                new XElement(NsSifen + "gCamFuFD", dCarQR)
            );

            var doc = new XDocument(rde);
            using var ms = new MemoryStream();
            using var writer = XmlWriter.Create(ms, new XmlWriterSettings
            {
                Indent = false,
                OmitXmlDeclaration = true,
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
            });
            doc.WriteTo(writer);
            writer.Flush();
            ms.Position = 0;
            using var reader = new StreamReader(ms, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Obtiene el código de motivo SIFEN según el texto del motivo
        /// </summary>
        private static int ObtenerCodigoMotivo(string? motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo)) return 2; // Default: Devolución

            return motivo.ToLower() switch
            {
                var m when m.Contains("ajuste") && m.Contains("precio") && m.Contains("devol") => 1,
                var m when m.Contains("devolución") || m.Contains("devolucion") => 2,
                var m when m.Contains("descuento") => 3,
                var m when m.Contains("bonificación") || m.Contains("bonificacion") => 4,
                var m when m.Contains("incobrable") => 5,
                var m when m.Contains("recupero") && m.Contains("costo") => 6,
                var m when m.Contains("recupero") && m.Contains("gasto") => 7,
                var m when m.Contains("ajuste") && m.Contains("precio") => 8,
                _ => 2 // Default: Devolución
            };
        }

        private static string DescripcionTipoDocRec(int tipo) => tipo switch
        {
            1 => "Cédula paraguaya",
            2 => "Pasaporte",
            3 => "Cédula extranjera",
            4 => "Carnet de residencia",
            5 => "Innominado",
            9 => "Sin documento",
            _ => "Otro"
        };

        private static string ObtenerNombreUnidadMedida(int codigo) => codigo switch
        {
            77 => "UNI",
            82 => "KG",
            83 => "L",
            85 => "M",
            86 => "M2",
            87 => "M3",
            _ => "UNI"
        };

        private static string ObtenerNombreDepartamento(int codigo) => codigo switch
        {
            0 or 1 => "CAPITAL",
            2 => "CONCEPCION",
            3 => "SAN PEDRO",
            4 => "CORDILLERA",
            5 => "GUAIRA",
            6 => "CAAGUAZU",
            7 => "CAAZAPA",
            8 => "ITAPUA",
            9 => "MISIONES",
            10 => "PARAGUARI",
            11 => "CENTRAL",
            12 => "ÑEEMBUCU",
            13 => "AMAMBAY",
            14 => "CANINDEYU",
            15 => "PRESIDENTE HAYES",
            16 => "ALTO PARAGUAY",
            17 => "BOQUERON",
            18 => "ALTO PARANA",
            _ => "CENTRAL"
        };

        private static string ObtenerNombreDistrito(int codigo, AppDbContext? ctx = null)
        {
            if (ctx != null)
            {
                var distrito = ctx.DistritosCatalogo.FirstOrDefault(d => d.Numero == codigo);
                if (distrito != null)
                    return distrito.Nombre?.ToUpper() ?? $"DISTRITO {codigo}";
            }
            return codigo switch
            {
                1 => "ASUNCION (DISTRITO)",
                164 => "SAN LORENZO",
                _ => $"DISTRITO {codigo}"
            };
        }

        private static string ObtenerNombreCiudad(int codigo, AppDbContext? ctx = null)
        {
            if (ctx != null)
            {
                var ciudad = ctx.CiudadesCatalogo.FirstOrDefault(c => c.Numero == codigo);
                if (ciudad != null)
                    return ciudad.Nombre?.ToUpper() ?? $"CIUDAD {codigo}";
            }
            return codigo switch
            {
                1 => "ASUNCION (DISTRITO)",
                3840 => "SAN LORENZO",
                _ => $"CIUDAD {codigo}"
            };
        }
    }
}
