using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Armador mínimo del XML del Documento Electrónico (DE) v150 para Factura (FE).
    /// Cubre: rDE/DE (Id), gEmis, gDatRec, items simplificados, totales, gCamFuFD/dCarQR y E7 (pagos).
    /// No firma; no valida XSD; produce XML base listo para firmar por SIFEN.FirmarYEnviar.
    /// </summary>
    public class DEXmlBuilder
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ClienteSifenService _clienteSvc;
        private readonly DEBuilderService _deBuilder;
        private static readonly XNamespace NsSifen = "http://ekuatia.set.gov.py/sifen/xsd";

        public DEXmlBuilder(IDbContextFactory<AppDbContext> dbFactory, ClienteSifenService clienteSvc, DEBuilderService deBuilder)
        {
            _dbFactory = dbFactory;
            _clienteSvc = clienteSvc;
            _deBuilder = deBuilder;
        }

        // Eliminada la función de conversión, todo se crea con NsSifen directamente

        public async Task<string> ConstruirXmlAsync(int idVenta)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var venta = await db.Ventas
                .Include(v => v.Sucursal)
                .Include(v => v.Cliente)
                .Include(v => v.Moneda)
                .Include(v => v.Caja)
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta)
                ?? throw new InvalidOperationException($"Venta {idVenta} no encontrada");

            var detalles = await db.VentasDetalles
                .Include(d => d.Producto)
                .Where(d => d.IdVenta == idVenta).ToListAsync();
            if (detalles.Count == 0) throw new InvalidOperationException("Venta sin ítems");

            var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("No existe Sociedad configurada");

            // E7 pagos ya validados/compuestos por el servicio existente (no convertimos a XML detallado aquí)
            var pagos = await _deBuilder.ConstruirPagosAsync(idVenta);

            // Helper: solo dígitos
            static string Digits(string? s) => new string((s ?? string.Empty).Where(char.IsDigit).ToArray());

            // CDC usando el generador correcto
            string idCdc;
            if (!string.IsNullOrWhiteSpace(venta.CDC) && venta.CDC.Length == 44)
            {
                // Si ya tiene un CDC válido, usarlo
                idCdc = Digits(venta.CDC);
            }
            else
            {
                // Generar CDC según especificación SIFEN
                var rucEmisor = venta.Sucursal!.RUC;
                var dvEmisor = Convert.ToString(venta.Sucursal!.DV, CultureInfo.InvariantCulture);
                var establecimiento = venta.Sucursal!.NumSucursal.ToString(CultureInfo.InvariantCulture);
                var puntoExpedicion = venta.Caja?.Nivel2 ?? "001"; // Nivel2 es el punto de expedición
                var numeroFactura = venta.NumeroFactura ?? "0";
                var tipoContribuyente = "2"; // 2 = Persona Jurídica (por defecto)
                var tipoEmision = "1"; // 1 = Normal
                var codigoSeguridad = venta.CodigoSeguridad; // Si existe en la venta

                idCdc = Utils.CdcGenerator.GenerarCDC(
                    tipoDocumento: "01", // 01 = Factura electrónica
                    rucEmisor: rucEmisor,
                    dvEmisor: dvEmisor,
                    establecimiento: establecimiento,
                    puntoExpedicion: puntoExpedicion,
                    numeroFactura: numeroFactura,
                    tipoContribuyente: tipoContribuyente,
                    fechaEmision: venta.Fecha,
                    tipoEmision: tipoEmision,
                    codigoSeguridadExistente: codigoSeguridad
                );

                // Guardar el CDC generado en la venta
                venta.CDC = idCdc;
                db.Ventas.Update(venta);
                await db.SaveChangesAsync();
            }

            var rucEm = Digits(venta.Sucursal!.RUC);
            var dvEm = Digits(Convert.ToString(venta.Sucursal!.DV, CultureInfo.InvariantCulture));
            var numSuc = Digits(Convert.ToString(venta.Sucursal!.NumSucursal, CultureInfo.InvariantCulture));
            var estab = Digits(venta.Establecimiento ?? string.Empty);
            var ptoExp = Digits(venta.PuntoExpedicion ?? string.Empty);
            var nroFact = Digits(venta.NumeroFactura ?? string.Empty);

            // Emisor mínimo (gEmis) con Sucursal
            var gEmis = new XElement(NsSifen + "gEmis",
                new XElement(NsSifen + "dRucEm", rucEm),
                new XElement(NsSifen + "dDVEmi", string.IsNullOrEmpty(dvEm) ? "0" : dvEm),
                new XElement(NsSifen + "iTipCont", 1),
                // cTipReg removido para alinear estructura
                new XElement(NsSifen + "dNomEmi", venta.Sucursal!.NombreEmpresa ?? string.Empty),
                new XElement(NsSifen + "dDirEmi", venta.Sucursal!.Direccion ?? string.Empty),
                new XElement(NsSifen + "dNumCas", "0")
            );

            // Receptor (gDatRec) desde servicio existente
            var gDatRecRaw = _clienteSvc.GenerarDatosReceptorSifen(venta.Cliente!);
            // Mapear iTipIDRec a código numérico v150: 1=CI, 2=RUC, 3=PAS, 4=CR, 5=CEE, 9=Sin Doc
            string tipRaw = (gDatRecRaw.Element("iTipIDRec")?.Value ?? string.Empty).Trim().ToUpperInvariant();
            string tipMap = tipRaw switch
            {
                "RUC" => "2",
                "RU" => "2",
                "CI" => "1",
                "PAS" => "3",
                "PASAPORTE" => "3",
                "DNI" => "4",
                "CR" => "4",
                "CEE" => "5",
                "C.EXTRANJERIA" => "5",
                "SD" => "9",
                "SIN DOCUMENTO" => "9",
                _ => (char.IsDigit(tipRaw.FirstOrDefault()) ? tipRaw : "9")
            };
            // Descripción oficial según XSD para dDTipIDRec
            static string DescripcionTipoDocRec(string iTipIDRec)
            {
                return iTipIDRec switch
                {
                    "1" => "Cédula paraguaya",
                    "3" => "Pasaporte",
                    "4" => "Carnet de residencia",
                    "5" => "Cédula extranjera",
                    "9" => "Innominado",
                    _ => "Innominado"
                };
            }

            string numIdRaw = gDatRecRaw.Element("dNumIDRec")?.Value ?? string.Empty;
            var cliRucDigits = Digits(venta.Cliente?.RUC ?? string.Empty);
            var cliDv = venta.Cliente?.DV ?? 0;
            if (string.IsNullOrWhiteSpace(cliRucDigits) && tipMap == "2") tipMap = "9";
            string dNumIDRec = tipMap == "2" ? cliRucDigits : Digits(numIdRaw);

            var iNatRec = gDatRecRaw.Element("iNatRec")?.Value ?? "2";
            bool esContribuyente = iNatRec == "1";

            // Construir gDatRec respetando el ORDEN del XSD tgDatRec
            var gDatRec = new XElement(NsSifen + "gDatRec");
            gDatRec.Add(new XElement(NsSifen + "iNatRec", iNatRec));
            // Consumidor Final: no contribuyente (iNatRec!=1) y sin documento (iTipIDRec==9 o dNumIDRec vacío/0)
            bool esConsumidorFinal = !esContribuyente && (tipMap == "9" || string.IsNullOrWhiteSpace(Digits(numIdRaw)) || Digits(numIdRaw) == "0");
            var iTiOpeVal = esConsumidorFinal ? "1" : (gDatRecRaw.Element("iTiOpe")?.Value ?? "1");
            gDatRec.Add(new XElement(NsSifen + "iTiOpe", iTiOpeVal));
            gDatRec.Add(new XElement(NsSifen + "cPaisRec", gDatRecRaw.Element("cPaisRec")?.Value ?? "PRY"));
            gDatRec.Add(new XElement(NsSifen + "dDesPaisRe", gDatRecRaw.Element("dDesPaisRe")?.Value ?? "Paraguay"));

            // iTiContRec es opcional
            var iTiContRecVal = esConsumidorFinal ? "1" : gDatRecRaw.Element("iTiContRec")?.Value;
            if (!string.IsNullOrWhiteSpace(iTiContRecVal))
                gDatRec.Add(new XElement(NsSifen + "iTiContRec", iTiContRecVal));

            if (esContribuyente)
            {
                // Para contribuyente: primero RUC y DV
                gDatRec.Add(new XElement(NsSifen + "dRucRec", string.IsNullOrWhiteSpace(cliRucDigits) ? "0" : cliRucDigits));
                gDatRec.Add(new XElement(NsSifen + "dDVRec", cliDv.ToString(CultureInfo.InvariantCulture)));
                // iTipIDRec es opcional, normalmente no se informa, lo omitimos
            }
            else
            {
                // No contribuyente: iTipIDRec (+ descripción) y número de documento
                gDatRec.Add(new XElement(NsSifen + "iTipIDRec", tipMap));
                gDatRec.Add(new XElement(NsSifen + "dDTipIDRec", DescripcionTipoDocRec(tipMap)));
                gDatRec.Add(new XElement(NsSifen + "dNumIDRec", string.IsNullOrWhiteSpace(dNumIDRec) ? "0" : dNumIDRec));
            }

            // Nombre del receptor: para Consumidor Final usar cadena exacta solicitada
            var dNomRecVal = esConsumidorFinal
                ? "SIN NOMBRE-CONSUMIDOR FINAL"
                : (gDatRecRaw.Element("dNomRec")?.Value ?? (esContribuyente ? "SIN NOMBRE" : "Sin Nombre"));
            gDatRec.Add(new XElement(NsSifen + "dNomRec", dNomRecVal));
            // Dirección y número de casa son opcionales, agregar si hay dato o un fallback aceptable
            var dirRec = gDatRecRaw.Element("dDirRec")?.Value;
            if (!string.IsNullOrWhiteSpace(dirRec))
                gDatRec.Add(new XElement(NsSifen + "dDirRec", dirRec));
            var numCasRec = gDatRecRaw.Element("dNumCasRec")?.Value;
            if (!string.IsNullOrWhiteSpace(numCasRec))
                gDatRec.Add(new XElement(NsSifen + "dNumCasRec", numCasRec));

            // gDatGralOpe: fecha emisión, operación comercial, emisor y receptor
            var gDatGralOpe = new XElement(NsSifen + "gDatGralOpe",
                new XElement(NsSifen + "dFeEmiDE", venta.Fecha.ToString("yyyy-MM-ddTHH:mm:ss")),
                new XElement(NsSifen + "gOpeCom",
                    // Alinear al ejemplo provisto: Mixto / IVA
                    new XElement(NsSifen + "iTipTra", 3),
                    new XElement(NsSifen + "dDesTipTra", "Mixto (Venta de mercadería y servicios)"),
                    new XElement(NsSifen + "iTImp", 1),
                    new XElement(NsSifen + "dDesTImp", "IVA"),
                    new XElement(NsSifen + "cMoneOpe", venta.Moneda?.CodigoISO ?? "PYG"),
                    new XElement(NsSifen + "dDesMoneOpe", "Guarani"),
                    // Reponer gOblAfe según ejemplo
                    new XElement(NsSifen + "gOblAfe",
                        new XElement(NsSifen + "cOblAfe", "211"),
                        new XElement(NsSifen + "dDesOblAfe", "IMPUESTO AL VALOR AGREGADO - GRAVADAS Y EXONERADAS - EXPORTADORES")
                    )
                ),
                gEmis,
                gDatRec
            );

            // gTimb: incluir tipo de DE y datos de timbrado
            var nroTim = Digits(venta.Timbrado ?? venta.Sucursal?.Timbrado ?? string.Empty);
            var nroDoc = Digits(venta.NumeroFactura ?? string.Empty);
            var gTimb = new XElement(NsSifen + "gTimb",
                new XElement(NsSifen + "iTiDE", 1),
                new XElement(NsSifen + "dDesTiDE", "Factura electrónica"),
                new XElement(NsSifen + "dNumTim", string.IsNullOrWhiteSpace(nroTim) ? "00000000" : nroTim),
                new XElement(NsSifen + "dEst", string.IsNullOrWhiteSpace(estab) ? "001" : estab.PadLeft(3, '0')),
                new XElement(NsSifen + "dPunExp", string.IsNullOrWhiteSpace(ptoExp) ? "001" : ptoExp.PadLeft(3, '0')),
                new XElement(NsSifen + "dNumDoc", string.IsNullOrWhiteSpace(nroDoc) ? "0000001" : nroDoc.PadLeft(7, '0')),
                new XElement(NsSifen + "dFeIniT", venta.Fecha.ToString("yyyy-MM-dd")) // TODO: usar fecha real de inicio de timbrado si está disponible
            );
            // Serie del número de timbrado desde Caja (opcional, patrón [A-Z]{2})
            var serieCaja = venta.Caja?.Serie?.ToString()?.Trim();
            if (!string.IsNullOrWhiteSpace(serieCaja))
            {
                // Si ya viene en letras (2+), tomar las dos primeras letras A-Z
                var letras = new string(serieCaja!
                    .ToUpperInvariant()
                    .Where(ch => ch >= 'A' && ch <= 'Z')
                    .ToArray());
                if (letras.Length >= 2)
                {
                    gTimb.Add(new XElement(NsSifen + "dSerieNum", letras.Substring(0, 2)));
                }
                // Si no hay letras válidas suficientes, omitimos dSerieNum para cumplir el XSD
            }


            // Ítems mínimos (gCamItem) con cantidad, precio e IVA simplificado
            var gDtipDE = new XElement(NsSifen + "gDtipDE");
            // Para Factura Electrónica, incluir gCamFE mínimo
            var gCamFE = new XElement(NsSifen + "gCamFE",
                new XElement(NsSifen + "iIndPres", 1), // 1 = Presencial
                new XElement(NsSifen + "dDesIndPres", "Operación presencial")
            );
            gDtipDE.Add(gCamFE);
            // Añadir gCamCond inmediatamente después de gCamFE
            var gCamCond = new XElement(NsSifen + "gCamCond",
                new XElement(NsSifen + "iCondOpe", pagos.E7.iCondOpe),
                new XElement(NsSifen + "dDCondOpe", pagos.E7.iCondOpe == 1 ? "Contado" : "Crédito")
            );
            if (pagos.E7.iCondOpe == 1)
            {
                // Contado: estructura compacta (sin gPagos)
                var pagoRef = pagos.E7.gPagos.FirstOrDefault();
                int iTiPago = pagoRef?.iTiPago ?? 1;
                var gPaConEIni = new XElement(NsSifen + "gPaConEIni",
                    new XElement(NsSifen + "iTiPago", iTiPago),
                    new XElement(NsSifen + "dDesTiPag", DescribirTipoPago(iTiPago)),
                    new XElement(NsSifen + "dMonTiPag", pagos.E7.dTotGralOpe.ToString("0.####", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "cMoneTiPag", string.IsNullOrWhiteSpace(pagoRef?.cMoneTiPag) ? (pagos.E7.dMoneda ?? "PYG") : pagoRef!.cMoneTiPag),
                    new XElement(NsSifen + "dDMoneTiPag", "Guarani")
                );
                gCamCond.Add(gPaConEIni);
            }
            else if (pagos.E7.iCondOpe == 2 && pagos.E7.gPagCred != null)
            {
                var gPagCred = new XElement(NsSifen + "gPagCred",
                    new XElement(NsSifen + "iCondCred", 1),
                    new XElement(NsSifen + "dDCondCred", "Plazo"),
                    new XElement(NsSifen + "dPlazoCre", pagos.E7.gPagCred.gCuotas.Any() ? (pagos.E7.gPagCred.gCuotas.Max(c => (c.dFeFinCuo - DateTime.Today).Days)) : 30)
                );
                if (pagos.E7.gPagCred.dAnticipo.HasValue)
                    gPagCred.Add(new XElement(NsSifen + "dAnticipo", pagos.E7.gPagCred.dAnticipo.Value.ToString("0.####", CultureInfo.InvariantCulture)));
                foreach (var cuo in pagos.E7.gPagCred.gCuotas)
                {
                    var gCuota = new XElement(NsSifen + "gCuotas",
                        new XElement(NsSifen + "cMoneCuo", cuo.cMoneCuo),
                        new XElement(NsSifen + "dMonCuota", cuo.dMonCuota.ToString("0.####", CultureInfo.InvariantCulture)),
                        new XElement(NsSifen + "dFeFinCuo", cuo.dFeFinCuo.ToString("yyyy-MM-dd"))
                    );
                    gPagCred.Add(gCuota);
                }
                gCamCond.Add(gPagCred);
            }
            gDtipDE.Add(gCamCond);
            int i = 1;
            foreach (var d in detalles)
            {
                var item = new XElement(NsSifen + "gCamItem",
                    new XElement(NsSifen + "dCodInt", d.IdProducto),
                    new XElement(NsSifen + "dDesProSer", d.Producto?.Descripcion ?? "ITEM"),
                    new XElement(NsSifen + "cUniMed", d.Producto?.UnidadMedidaCodigo ?? "77"),
                    new XElement(NsSifen + "dDesUniMed", "UNI"),
                    new XElement(NsSifen + "dCantProSer", d.Cantidad.ToString("0.####", CultureInfo.InvariantCulture))
                );

                // gValorItem y gValorRestaItem (simplificados sin descuentos/recargos)
                var gValorItem = new XElement(NsSifen + "gValorItem",
                    new XElement(NsSifen + "dPUniProSer", d.PrecioUnitario.ToString("0.####", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "dTotBruOpeItem", d.Importe.ToString("0.####", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "gValorRestaItem",
                        new XElement(NsSifen + "dDescItem", 0),
                        new XElement(NsSifen + "dPorcDesIt", 0),
                        new XElement(NsSifen + "dDescGloItem", 0),
                        new XElement(NsSifen + "dTotOpeItem", d.Importe.ToString("0.####", CultureInfo.InvariantCulture))
                    )
                );
                item.Add(gValorItem);

                // gCamIVA por ítem
                decimal base10 = d.Grabado10;
                decimal iva10Item = d.IVA10;
                decimal base5 = d.Grabado5;
                decimal iva5Item = d.IVA5;
                bool es10 = base10 > 0 || iva10Item > 0;
                bool es5 = base5 > 0 || iva5Item > 0;
                int iAfecIVA = 3; // 3 = Exenta (aprox.)
                decimal dTasa = 0;
                decimal dBasGrav = 0;
                decimal dLiq = 0;
                if (es10)
                {
                    iAfecIVA = 1; // Gravado
                    dTasa = 10;
                    dBasGrav = base10;
                    dLiq = iva10Item;
                }
                else if (es5)
                {
                    iAfecIVA = 1; // Gravado
                    dTasa = 5;
                    dBasGrav = base5;
                    dLiq = iva5Item;
                }

                string dDesAfec = iAfecIVA switch { 1 => "Gravado IVA", 2 => "Exonerado", 3 => "Exento", _ => "Exento" };
                string dPropIVA = iAfecIVA == 1 ? (dTasa == 10 ? "100" : dTasa == 5 ? "50" : "0") : "0"; // Placeholder proporcionalidad
                var gCamIVA = new XElement(NsSifen + "gCamIVA",
                    new XElement(NsSifen + "iAfecIVA", iAfecIVA),
                    new XElement(NsSifen + "dDesAfecIVA", dDesAfec),
                    new XElement(NsSifen + "dPropIVA", dPropIVA),
                    new XElement(NsSifen + "dTasaIVA", dTasa.ToString("0.####", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "dBasGravIVA", dBasGrav.ToString("0.####", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "dLiqIVAItem", dLiq.ToString("0.####", CultureInfo.InvariantCulture))
                );
                if (iAfecIVA == 3)
                {
                    var baseExe = d.Exenta > 0 ? d.Exenta : (d.Grabado5 == 0 && d.Grabado10 == 0 ? d.Importe : 0);
                    gCamIVA.Add(new XElement(NsSifen + "dBasExe", baseExe.ToString("0.####", CultureInfo.InvariantCulture)));
                }
                item.Add(gCamIVA);
                gDtipDE.Add(item);
                i++;
            }

            // Eliminar gCamEsp y gTransp para estructura mínima

            // Totales mínimos
            decimal subExe = detalles.Sum(x => x.Exenta);
            decimal subExo = 0m; // Si hay exonerado, mapear aquí
            decimal sub5 = detalles.Sum(x => x.Grabado5);
            decimal sub10 = detalles.Sum(x => x.Grabado10);
            decimal iva5 = detalles.Sum(x => x.IVA5);
            decimal iva10 = detalles.Sum(x => x.IVA10);
            decimal totIVA = iva5 + iva10;
            decimal totOpe = venta.Total; // Asumimos total incluye IVA
            decimal baseGrav5 = sub5; // en ejemplo, base gravada 5 = subtotal 5 sin descuentos
            decimal baseGrav10 = sub10;
            decimal tBaseGravIva = baseGrav5 + baseGrav10;

            var gTotSub = new XElement(NsSifen + "gTotSub",
                new XElement(NsSifen + "dSubExe", subExe.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dSubExo", subExo.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dSub5", sub5.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dSub10", sub10.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTotOpe", totOpe.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTotDesc", 0.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTotDescGlotem", 0.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTotAntItem", 0.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTotAnt", 0.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dPorcDescTotal", 0.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dDescTotal", 0.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dAnticipo", 0.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dRedon", 0.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTotGralOpe", totOpe.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dIVA5", iva5.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dIVA10", iva10.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTotIVA", totIVA.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dBaseGrav5", baseGrav5.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dBaseGrav10", baseGrav10.ToString("0.####", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTBasGraIVA", tBaseGravIva.ToString("0.####", CultureInfo.InvariantCulture))
            );

            // Determinar parámetro de identificación del receptor para el QR (RUC o documento)
            string qrIdParamName;
            string qrIdParamValue;
            if (esContribuyente)
            {
                qrIdParamName = "dRucRec";
                qrIdParamValue = string.IsNullOrWhiteSpace(cliRucDigits) ? "0" : cliRucDigits;
            }
            else
            {
                qrIdParamName = "dNumIDRec";
                qrIdParamValue = string.IsNullOrWhiteSpace(dNumIDRec) ? "0" : dNumIDRec;
            }

            // dCarQR con DigestValue placeholder (hex de base64 SHA256) que Sifen.FirmarYEnviar reemplazará
            var digestPlaceholder = "DigestValue=" +
                "665569394474586a4f4a396970724970754f344c434a75706a457a73645766664846656d573270344c69593d";
            // Base del QR: si está mal configurada (duplicada) usar default; asegurar '?'
            string defaultQr = "https://ekuatia.set.gov.py/consultas-test/qr?";
            // Forzar default para evitar duplicación por datos corruptos de configuración
            string urlQrBase = defaultQr;
            if (!urlQrBase.Contains("?")) urlQrBase += "?";
            var dCarQR = new XElement(NsSifen + "dCarQR",
                $"{urlQrBase}nVersion=150&Id={idCdc}&dFeEmiDE={Uri.EscapeDataString(venta.Fecha.ToString("yyyy-MM-ddTHH:mm:ss"))}&{qrIdParamName}={qrIdParamValue}&dTotGralOpe={(long)venta.Total}&dTotIVA={(long)(detalles.Sum(x => x.IVA5 + x.IVA10))}&cItems={detalles.Count}&{digestPlaceholder}&IdCSC={(sociedad.IdCsc ?? "0001")}&cHashQR=PLACEHOLDER"
            );

            // gCamCond ya agregado dentro de gDtipDE (definición arriba)

            // DE principal
            // NOTA: Según ejemplo v150 se requieren cabeceras dDVId (DV del Id), dFecFirma (fecha de firma) y dSisFact.
            // Por ahora dDVId se recalcula como dígito verificador simple mod11 sobre el Id numérico sin prefijo (placeholder).
            string idNumerico = idCdc.TrimStart('0');
            if (string.IsNullOrEmpty(idNumerico)) idNumerico = "0";
            int sum = 0; int mult = 2;
            for (int idx = idNumerico.Length - 1; idx >= 0; idx--)
            {
                sum += (idNumerico[idx] - '0') * mult;
                mult++; if (mult > 11) mult = 2;
            }
            int dvCalc = 11 - (sum % 11); if (dvCalc > 9) dvCalc = 0; if (dvCalc < 0) dvCalc = 0;

            var de = new XElement(NsSifen + "DE",
                new XAttribute("Id", idCdc),
                new XElement(NsSifen + "dDVId", dvCalc),
                new XElement(NsSifen + "dFecFirma", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")), // Placeholder, se actualizará al firmar
                new XElement(NsSifen + "dSisFact", 1),
                new XElement(NsSifen + "gOpeDE",
                    new XElement(NsSifen + "iTipEmi", 1),
                    new XElement(NsSifen + "dDesTipEmi", "Normal"),
                    new XElement(NsSifen + "dCodSeg", ("000000000" + (venta.IdVenta % 1000000000)).Substring(("000000000" + (venta.IdVenta % 1000000000)).Length - 9))
                    // dInfoEmi y dInfoFisc removidos para alinear
                ),
                gTimb,
                gDatGralOpe,
                gDtipDE,
                gTotSub
            );

            // rDE envolvente
            // Completar datos geográficos y contactos del emisor y receptor desde el catálogo SIFEN cuando sea posible
            // Emisor: mapear por Sucursal.IdCiudad -> ciudad (dep, dis, nom)
            int em_cDep = 1; string em_dDesDep = "CAPITAL";
            int em_cDis = 1; string em_dDesDis = "ASUNCION (DISTRITO)";
            int em_cCiu = 1; string em_dDesCiu = "ASUNCION (DISTRITO)";
            if (venta.Sucursal?.IdCiudad != null && venta.Sucursal.IdCiudad.Value > 0)
            {
                var ciu = await db.CiudadesCatalogo.FindAsync(venta.Sucursal.IdCiudad.Value);
                if (ciu != null)
                {
                    em_cCiu = ciu.Numero;
                    em_dDesCiu = ciu.Nombre;
                    em_cDep = ciu.Departamento;
                    var dep = await db.DepartamentosCatalogo.FindAsync(ciu.Departamento);
                    if (dep != null) em_dDesDep = dep.Nombre;
                    var dis = await db.DistritosCatalogo.FindAsync(ciu.Distrito);
                    if (dis != null) { em_cDis = dis.Numero; em_dDesDis = dis.Nombre; }
                }
            }
            // Normalización a SAN LORENZO para coincidir con el otro sistema: dep=11, dis=164, ciu=1370
            if ((em_dDesCiu ?? string.Empty).Trim().ToUpperInvariant() == "SAN LORENZO")
            {
                em_cDep = 11; em_dDesDep = "CENTRAL";
                em_cDis = 164; em_dDesDis = "SAN LORENZO";
                em_cCiu = 1370; em_dDesCiu = "SAN LORENZO";
            }
            gEmis.Add(new XElement(NsSifen + "cDepEmi", em_cDep));
            gEmis.Add(new XElement(NsSifen + "dDesDepEmi", em_dDesDep));
            gEmis.Add(new XElement(NsSifen + "cDisEmi", em_cDis));
            gEmis.Add(new XElement(NsSifen + "dDesDisEmi", em_dDesDis));
            gEmis.Add(new XElement(NsSifen + "cCiuEmi", em_cCiu));
            gEmis.Add(new XElement(NsSifen + "dDesCiuEmi", em_dDesCiu));
            gEmis.Add(new XElement(NsSifen + "dTelEmi", Digits(venta.Sucursal?.Telefono ?? "012123456")));
            gEmis.Add(new XElement(NsSifen + "dEmailE", sociedad.Email ?? venta.Sucursal?.Correo ?? "correo@correo.com"));
            // Actividad económica: tomar de tabla SociedadesActividades (principal), fallback a sucursal.RubroEmpresa
            var act = await db.SociedadesActividades
                .AsNoTracking()
                .Where(a => a.IdSociedad == sociedad.IdSociedad && (a.ActividadPrincipal == "S" || a.ActividadPrincipal == "1"))
                .OrderBy(a => a.Numero)
                .FirstOrDefaultAsync();
            string cActEco = act?.CodigoActividad ?? "46510";
            string dDesActEco = !string.IsNullOrWhiteSpace(act?.NombreActividad)
                ? act!.NombreActividad!
                : (string.IsNullOrWhiteSpace(venta.Sucursal?.RubroEmpresa) ? "ACTIVIDAD ECONOMICA NO ESPECIFICADA" : venta.Sucursal!.RubroEmpresa!);
            var gActEco = new XElement(NsSifen + "gActEco",
                new XElement(NsSifen + "cActEco", cActEco),
                new XElement(NsSifen + "dDesActEco", dDesActEco)
            );
            gEmis.Add(gActEco);

            // Receptor: mapear por Cliente.IdCiudad -> ciudad (dep, dis, nom)
            int rc_cDep = 1; string rc_dDesDep = "CAPITAL";
            int rc_cDis = 1; string rc_dDesDis = "ASUNCION (DISTRITO)";
            int rc_cCiu = 1; string rc_dDesCiu = "ASUNCION (DISTRITO)";
            if (venta.Cliente != null && venta.Cliente.IdCiudad > 0)
            {
                var ciuR = await db.CiudadesCatalogo.FindAsync(venta.Cliente.IdCiudad);
                if (ciuR != null)
                {
                    rc_cCiu = ciuR.Numero;
                    rc_dDesCiu = ciuR.Nombre;
                    rc_cDep = ciuR.Departamento;
                    var depR = await db.DepartamentosCatalogo.FindAsync(ciuR.Departamento);
                    if (depR != null) rc_dDesDep = depR.Nombre;
                    var disR = await db.DistritosCatalogo.FindAsync(ciuR.Distrito);
                    if (disR != null) { rc_cDis = disR.Numero; rc_dDesDis = disR.Nombre; }
                }
            }
            // Normalización a SAN LORENZO para receptor cuando corresponda
            if ((rc_dDesCiu ?? string.Empty).Trim().ToUpperInvariant() == "SAN LORENZO")
            {
                rc_cDep = 11; rc_dDesDep = "CENTRAL";
                rc_cDis = 164; rc_dDesDis = "SAN LORENZO";
                rc_cCiu = 1370; rc_dDesCiu = "SAN LORENZO";
            }
            // Agregar geocódigos/contacto del receptor SIEMPRE (la estructura oficial los muestra presentes)
            gDatRec.Add(new XElement(NsSifen + "cDepRec", rc_cDep));
            gDatRec.Add(new XElement(NsSifen + "dDesDepRec", rc_dDesDep));
            gDatRec.Add(new XElement(NsSifen + "cDisRec", rc_cDis));
            gDatRec.Add(new XElement(NsSifen + "dDesDisRec", rc_dDesDis));
            gDatRec.Add(new XElement(NsSifen + "cCiuRec", rc_cCiu));
            gDatRec.Add(new XElement(NsSifen + "dDesCiuRec", rc_dDesCiu));
            gDatRec.Add(new XElement(NsSifen + "dTelRec", venta.Cliente?.Telefono ?? "021-555-1234"));
            gDatRec.Add(new XElement(NsSifen + "dCodCliente", venta.Cliente?.CodigoCliente ?? "CLI001"));

            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            var rde = new XElement(NsSifen + "rDE",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                // Alinear con el ejemplo oficial (https en schemaLocation)
                new XAttribute(xsi + "schemaLocation", "https://ekuatia.set.gov.py/sifen/xsd siRecepDE_v150.xsd"),
                new XElement(NsSifen + "dVerFor", "150"),
                de,
                // gCamFuFD (firma y QR data) debe estar a nivel de rDE (no dentro de DE) antes de firmar
                new XElement(NsSifen + "gCamFuFD", dCarQR)
            );

            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", null), rde);
            // Retornar XML sin indentación para evitar problemas de formato
            using var ms = new MemoryStream();
            using var writer = XmlWriter.Create(ms, new XmlWriterSettings { 
                Indent = false, 
                OmitXmlDeclaration = false // Incluir declaración XML
            });
            doc.WriteTo(writer);
            writer.Flush();
            ms.Position = 0;
            using var reader = new StreamReader(ms);
            return reader.ReadToEnd();
        }

        private static string DescribirTipoPago(int iTiPago)
        {
            return iTiPago switch
            {
                1 => "Efectivo",
                2 => "Cheque",
                3 => "Tarjeta de crédito",
                4 => "Tarjeta de débito",
                5 => "Transferencia",
                6 => "Giro",
                7 => "Billetera electrónica",
                _ => "Otro"
            };
        }
    }
}
