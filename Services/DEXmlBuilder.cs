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
                // IMPORTANTE: Usar datos de SOCIEDAD (contribuyente registrado en SIFEN)
                var rucEmisor = sociedad.RUC;
                var dvEmisor = Convert.ToString(sociedad.DV, CultureInfo.InvariantCulture);
                var establecimiento = venta.Sucursal!.NumSucursal.ToString(CultureInfo.InvariantCulture);
                var puntoExpedicion = venta.Caja?.Nivel2 ?? "001"; // Nivel2 es el punto de expedición
                var numeroFactura = venta.NumeroFactura ?? "0";
                // TipoContribuyente: 1=Persona Física, 2=Persona Jurídica - obtener de Sociedad
                var tipoContribuyente = (sociedad.TipoContribuyente ?? 1).ToString(CultureInfo.InvariantCulture);
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

            // ========== DATOS DEL EMISOR ==========
            // Para Factura Electrónica: Usar datos de SOCIEDAD (contribuyente registrado en SIFEN)
            // El RUC, nombre y dirección deben coincidir con los datos del certificado digital
            var rucEm = Digits(sociedad.RUC);
            var dvEm = Digits(Convert.ToString(sociedad.DV, CultureInfo.InvariantCulture));
            var numSuc = Digits(Convert.ToString(venta.Sucursal!.NumSucursal, CultureInfo.InvariantCulture));
            var estab = Digits(venta.Establecimiento ?? string.Empty);
            var ptoExp = Digits(venta.PuntoExpedicion ?? string.Empty);
            var nroFact = Digits(venta.NumeroFactura ?? string.Empty);

            // Cargar actividades económicas de la sociedad
            var actividadesEconomicas = await db.SociedadesActividades
                .Where(a => a.IdSociedad == sociedad.IdSociedad)
                .OrderByDescending(a => a.ActividadPrincipal == "S")
                .Take(9) // Máximo 9 según XSD
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

            // Emisor COMPLETO (gEmis) con TODOS los campos obligatorios según XSD v150
            // TipoContribuyente: 1=Persona Física, 2=Persona Jurídica
            var iTipCont = sociedad.TipoContribuyente ?? 1;
            var gEmis = new XElement(NsSifen + "gEmis",
                new XElement(NsSifen + "dRucEm", rucEm),
                new XElement(NsSifen + "dDVEmi", string.IsNullOrEmpty(dvEm) ? "0" : dvEm),
                new XElement(NsSifen + "iTipCont", iTipCont),
                // cTipReg es opcional, se omite
                new XElement(NsSifen + "dNomEmi", sociedad.Nombre ?? "SIN NOMBRE"),
                // dNomFanEmi es opcional, se omite
                new XElement(NsSifen + "dDirEmi", sociedad.Direccion ?? "SIN DIRECCION"),
                new XElement(NsSifen + "dNumCas", sociedad.NumeroCasa ?? "0"),
                // dCompDir1 y dCompDir2 son opcionales, se omiten
                // CAMPOS OBLIGATORIOS DE UBICACIÓN
                new XElement(NsSifen + "cDepEmi", sociedad.Departamento ?? 11), // 11 = CENTRAL por defecto
                new XElement(NsSifen + "dDesDepEmi", ObtenerNombreDepartamento(sociedad.Departamento ?? 11)),
                // Distrito del emisor (opcional pero recomendado)
                new XElement(NsSifen + "cDisEmi", sociedad.Distrito ?? 1),
                new XElement(NsSifen + "dDesDisEmi", ObtenerNombreDistrito(sociedad.Distrito ?? 1, db)),
                new XElement(NsSifen + "cCiuEmi", sociedad.Ciudad ?? 1), // 1 = ASUNCION por defecto
                new XElement(NsSifen + "dDesCiuEmi", ObtenerNombreCiudad(sociedad.Ciudad ?? 1, db)),
                // CAMPOS OBLIGATORIOS DE CONTACTO
                new XElement(NsSifen + "dTelEmi", sociedad.Telefono ?? "000000000"),
                new XElement(NsSifen + "dEmailE", sociedad.Email ?? "sin@email.com")
                // dDenSuc es opcional, se omite
                // gActEco se agrega después (al menos 1 obligatorio)
                // gRespDE es opcional, se omite
            );
            
            // Agregar actividades económicas (obligatorio al menos 1)
            foreach (var act in actividadesEconomicas)
            {
                gEmis.Add(new XElement(NsSifen + "gActEco",
                    new XElement(NsSifen + "cActEco", act.CodigoActividad),
                    new XElement(NsSifen + "dDesActEco", act.NombreActividad ?? "ACTIVIDAD ECONOMICA")
                ));
            }

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
            // iTiOpe según manual SIFEN: 1=B2B (empresa a empresa), 2=B2C (empresa a consumidor final)
            // El XML aprobado de PowerBuilder usa iTiOpe=1 para contribuyentes
            // Para contado debe ser 1, para crédito puede ser 1 o 2 según si es B2B o B2C
            var iTiOpeVal = esContribuyente ? "1" : "2"; // B2B=1, B2C=2
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
            
            // Crear gOpeCom según XSD siRecepDE_v150.xsd
            // FIX 16-Ene-2026: RE-AGREGAR gOblAfe - El XML de referencia APROBADO por SIFEN lo incluye
            // El XML xmlRequestVenta_273_sync.xml tiene <gOblAfe><cOblAfe>211</cOblAfe>...
            var gOblAfe = new XElement(NsSifen + "gOblAfe",
                new XElement(NsSifen + "cOblAfe", "211"),
                new XElement(NsSifen + "dDesOblAfe", "IMPUESTO AL VALOR AGREGADO - GRAVADAS Y EXONERADAS - EXPORTADORES")
            );
            
            var gOpeCom = new XElement(NsSifen + "gOpeCom",
                new XElement(NsSifen + "iTipTra", 3),
                new XElement(NsSifen + "dDesTipTra", "Mixto (Venta de mercadería y servicios)"),
                new XElement(NsSifen + "iTImp", 1),
                new XElement(NsSifen + "dDesTImp", "IVA"),
                new XElement(NsSifen + "cMoneOpe", venta.Moneda?.CodigoISO ?? "PYG"),
                new XElement(NsSifen + "dDesMoneOpe", "Guarani"),
                gOblAfe  // FIX 16-Ene-2026: Requerido por SIFEN según XML de referencia
            );

            var gDatGralOpe = new XElement(NsSifen + "gDatGralOpe",
                new XElement(NsSifen + "dFeEmiDE", venta.Fecha.ToString("yyyy-MM-ddTHH:mm:ss")),
                gOpeCom,
                gEmis,
                gDatRec
            );

            // gTimb: incluir tipo de DE y datos de timbrado
            // CRÍTICO para ambiente TEST: dNumTim debe ser el RUC del emisor (sin DV) con padding a 8 caracteres
            // CRÍTICO para ambiente PROD: dNumTim es el número de timbrado asignado por SET
            // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
            var ambiente = (sociedad.ServidorSifen ?? "test").ToLowerInvariant();
            string nroTim;
            if (ambiente == "prod")
            {
                // Producción: usar el timbrado real de la Caja
                nroTim = Digits(venta.Timbrado ?? venta.Caja?.Timbrado ?? string.Empty).PadLeft(8, '0');
            }
            else
            {
                // TEST: usar el RUC de la Sociedad (sin DV) con padding a 8 caracteres
                nroTim = Digits(sociedad.RUC).PadLeft(8, '0');
            }
            var nroDoc = Digits(venta.NumeroFactura ?? string.Empty);
            // Fecha de inicio de vigencia del timbrado: usar VigenciaDel de Caja, si no existe usar fecha de venta como fallback
            var fechaInicioTimbrado = venta.Caja?.VigenciaDel ?? venta.Fecha;
            var gTimb = new XElement(NsSifen + "gTimb",
                new XElement(NsSifen + "iTiDE", 1),
                new XElement(NsSifen + "dDesTiDE", "Factura electrónica"),
                new XElement(NsSifen + "dNumTim", string.IsNullOrWhiteSpace(nroTim) ? "00000000" : nroTim.PadLeft(8, '0')),
                new XElement(NsSifen + "dEst", string.IsNullOrWhiteSpace(estab) ? "001" : estab.PadLeft(3, '0')),
                new XElement(NsSifen + "dPunExp", string.IsNullOrWhiteSpace(ptoExp) ? "001" : ptoExp.PadLeft(3, '0')),
                new XElement(NsSifen + "dNumDoc", string.IsNullOrWhiteSpace(nroDoc) ? "0000001" : nroDoc.PadLeft(7, '0')),
                new XElement(NsSifen + "dFeIniT", fechaInicioTimbrado.ToString("yyyy-MM-dd")) // Fecha inicio vigencia del timbrado desde Caja.VigenciaDel
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
                // Contado: estructura compacta (sin gPagos) - usar enteros para PYG
                var pagoRef = pagos.E7.gPagos.FirstOrDefault();
                int iTiPago = pagoRef?.iTiPago ?? 1;
                var gPaConEIni = new XElement(NsSifen + "gPaConEIni",
                    new XElement(NsSifen + "iTiPago", iTiPago),
                    new XElement(NsSifen + "dDesTiPag", DescribirTipoPago(iTiPago)),
                    new XElement(NsSifen + "dMonTiPag", Math.Round(pagos.E7.dTotGralOpe, 0).ToString("0", CultureInfo.InvariantCulture)),
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
                    gPagCred.Add(new XElement(NsSifen + "dAnticipo", Math.Round(pagos.E7.gPagCred.dAnticipo.Value, 0).ToString("0", CultureInfo.InvariantCulture)));
                foreach (var cuo in pagos.E7.gPagCred.gCuotas)
                {
                    var gCuota = new XElement(NsSifen + "gCuotas",
                        new XElement(NsSifen + "cMoneCuo", cuo.cMoneCuo),
                        new XElement(NsSifen + "dMonCuota", Math.Round(cuo.dMonCuota, 0).ToString("0", CultureInfo.InvariantCulture)),
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
                    new XElement(NsSifen + "dCantProSer", d.Cantidad.ToString("0.0000", CultureInfo.InvariantCulture)) // 4 decimales según XML aprobado
                );

                // gValorItem y gValorRestaItem (según XML aprobado por SIFEN - SIN dDescGloItem)
                // Para PYG usar valores enteros (excepto cantidades que usan 4 decimales)
                var gValorItem = new XElement(NsSifen + "gValorItem",
                    new XElement(NsSifen + "dPUniProSer", Math.Round(d.PrecioUnitario, 0).ToString("0", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "dTotBruOpeItem", Math.Round(d.Importe, 0).ToString("0", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "gValorRestaItem",
                        new XElement(NsSifen + "dDescItem", "0"),
                        new XElement(NsSifen + "dPorcDesIt", "0.00"),  // 2 decimales según XML aprobado
                        // NOTA: dDescGloItem fue eliminado - el XML aprobado NO lo incluye
                        new XElement(NsSifen + "dTotOpeItem", Math.Round(d.Importe, 0).ToString("0", CultureInfo.InvariantCulture))
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
                // dPropIVA = proporcionalidad del IVA (100 = 100% gravado). NO es la tasa de IVA.
                string dPropIVAValue = iAfecIVA == 1 ? "100" : "0"; // 100% gravado cuando iAfecIVA=1
                // Para PYG, usar valores enteros (scale = 0)
                bool esPYG = true; // TODO: obtener de moneda
                string formatoDecimal = esPYG ? "0" : "0.####";
                
                // ========================================================================
                // FIX 15-Ene-2026 v2: dBasExe SIEMPRE es 0
                // ========================================================================
                // Según XMLs aprobados por SIFEN (ejemplo: 01800261658019002007094122023091018521597072.xml)
                // ========================================================================
                // FIX 16-Ene-2026: dBasExe SÍ EXISTE en el XML de referencia APROBADO por SIFEN
                // ========================================================================
                // El XML xmlRequestVenta_273_sync.xml tiene:
                //   <dLiqIVAItem>0</dLiqIVAItem><dBasExe>0</dBasExe>
                // Incluso cuando es 0, debe incluirse según el XML de referencia aprobado
                // ========================================================================
                
                var gCamIVA = new XElement(NsSifen + "gCamIVA",
                    new XElement(NsSifen + "iAfecIVA", iAfecIVA),
                    new XElement(NsSifen + "dDesAfecIVA", dDesAfec),
                    new XElement(NsSifen + "dPropIVA", dPropIVAValue),
                    new XElement(NsSifen + "dTasaIVA", dTasa.ToString("0", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "dBasGravIVA", Math.Round(dBasGrav, 0).ToString("0", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "dLiqIVAItem", Math.Round(dLiq, 0).ToString("0", CultureInfo.InvariantCulture)),
                    new XElement(NsSifen + "dBasExe", "0")  // FIX 16-Ene-2026: Requerido por SIFEN según XML de referencia
                );
                item.Add(gCamIVA);
                gDtipDE.Add(item);
                i++;
            }

            // Eliminar gCamEsp y gTransp para estructura mínima

            // Totales mínimos - Para PYG usar valores enteros (scale = 0)
            decimal subExe = Math.Round(detalles.Sum(x => x.Exenta), 0);
            decimal subExo = 0; // Subtotal exonerado (no aplica en Paraguay normalmente)
            decimal sub5 = Math.Round(detalles.Sum(x => x.Grabado5), 0);
            decimal sub10 = Math.Round(detalles.Sum(x => x.Grabado10), 0);
            decimal iva5 = Math.Round(detalles.Sum(x => x.IVA5), 0);
            decimal iva10 = Math.Round(detalles.Sum(x => x.IVA10), 0);
            decimal totIVA = iva5 + iva10;
            decimal totOpe = Math.Round(venta.Total, 0); // Asumimos total incluye IVA
            decimal baseGrav5 = sub5; // en ejemplo, base gravada 5 = subtotal 5 sin descuentos
            decimal baseGrav10 = sub10;
            decimal tBaseGravIva = baseGrav5 + baseGrav10;

            // gTotSub según estructura de XML APROBADO por SIFEN (Respuesta_ConsultaDE_Exitosa.xml)
            // Para PYG todos los valores son enteros (sin decimales)
            // IMPORTANTE: El orden de campos debe coincidir EXACTAMENTE con el XSD
            // NOTA: dSubExo eliminado - el XML funcional NO lo incluye
            // NOTA: dLiqTotIVA5 y dLiqTotIVA10 ELIMINADOS - no aparecen en XML exitoso
            var gTotSub = new XElement(NsSifen + "gTotSub",
                new XElement(NsSifen + "dSubExe", subExe.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dSub5", sub5.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dSub10", sub10.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTotOpe", totOpe.ToString("0", CultureInfo.InvariantCulture)),
                new XElement(NsSifen + "dTotDesc", "0"),
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
            // FIX 13-Ene-2026: Usar URL de BD (sociedad.DeUrlQr) como prioridad
            // Si no hay URL configurada, usar URL según ambiente de la sucursal (variable 'ambiente' ya existe en línea 281)
            string defaultQr = ambiente == "prod" 
                ? "https://ekuatia.set.gov.py/consultas/qr?"
                : "https://ekuatia.set.gov.py/consultas-test/qr?";
            // PRIORIDAD: URL de BD > default según ambiente
            string urlQrBase = sociedad.DeUrlQr ?? defaultQr;
            if (!urlQrBase.Contains("?")) urlQrBase += "?";
            // SIFEN requiere dFeEmiDE en formato HEX (no URI-escaped)
            string fechaHex = BitConverter.ToString(Encoding.UTF8.GetBytes(venta.Fecha.ToString("yyyy-MM-ddTHH:mm:ss"))).Replace("-", "").ToLowerInvariant();
            // ========================================================================
            // FIX CRÍTICO 31-Ene-2026: Formato QR según DLL funcional de Power Builder
            // ========================================================================
            // Referencia: Sifen2026Proyec/txtlog/sifen_xml_input.txt y sifen_qr.txt
            //
            // ENTRADA del DLL (en el XML):
            //   &IdCSC=1ABCD0000000000000000000000000000
            //   (IdCSC pegado con CSC de 32 caracteres al final)
            //
            // PROCESO del DLL:
            //   1. Reemplazar placeholder DigestValue con hex real
            //   2. qrHash = SHA256(toda_la_URL_completa)
            //   3. Quitar últimos 32 chars (el CSC pegado)
            //   4. Agregar &cHashQR={hash}
            //
            // SALIDA del DLL (QR final):
            //   ...&IdCSC=1&cHashQR=7867368ad88ffd187d619fcd2d89813c2a78a06f9e21150c6e2aacacc39fb5b4
            //
            // IMPORTANTE: 
            // - NO incluir &cHashQR=PLACEHOLDER en la entrada
            // - NO usar __CSC__ separado
            // - El CSC va PEGADO al IdCSC, no como parámetro separado
            // ========================================================================
            string cscValue = sociedad.Csc ?? "ABCD0000000000000000000000000000";
            string idCscValue = (sociedad.IdCsc ?? "1").TrimStart('0');
            if (string.IsNullOrEmpty(idCscValue)) idCscValue = "1";
            
            // QR SIN url base (Sifen.cs agregará urlQR al inicio)
            // Formato: nVersion=...&IdCSC={id}{csc} (32 chars CSC pegados al final)
            string qrText = $"nVersion=150&Id={idCdc}&dFeEmiDE={fechaHex}&{qrIdParamName}={qrIdParamValue}&dTotGralOpe={totOpe.ToString("0", CultureInfo.InvariantCulture)}&dTotIVA={totIVA.ToString("0", CultureInfo.InvariantCulture)}&cItems={detalles.Count}&{digestPlaceholder}&IdCSC={idCscValue}{cscValue}";
            var dCarQR = new XElement(NsSifen + "dCarQR", qrText);

            // gCamCond ya agregado dentro de gDtipDE (definición arriba)

            // DE principal
            // dDVId es el dígito verificador del CDC (último carácter, posición 44)
            // El CDC ya incluye el DV calculado por CdcGenerator.CalcularDigitoVerificador()
            string dvId = idCdc.Length > 0 ? idCdc[^1].ToString() : "0";

            // CRÍTICO: El dCodSeg DEBE coincidir con los dígitos 35-43 del CDC
            // CDC tiene 44 dígitos: pos 35-43 = código de seguridad (9 dígitos)
            var codigoSeguridadDelCdc = idCdc.Length >= 43 ? idCdc.Substring(34, 9) : "000000001";
            
            // ========================================================================
            // FIX 16-Ene-2026: gCamGen NO aparece en el XML de referencia APROBADO por SIFEN
            // El XML xmlRequestVenta_273_sync.xml NO tiene <gCamGen /> vacío
            // Solo agregar si hay contenido real (condiciones de pago a crédito, etc.)
            // Para ventas simples al contado, NO incluir gCamGen
            // ========================================================================

            var de = new XElement(NsSifen + "DE",
                new XAttribute("Id", idCdc),
                new XElement(NsSifen + "dDVId", dvId),
                // CRÍTICO: dFecFirma debe ser hora LOCAL de Paraguay (UTC-3 o UTC-4), NO UTC
                new XElement(NsSifen + "dFecFirma", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")),
                new XElement(NsSifen + "dSisFact", 1),
                new XElement(NsSifen + "gOpeDE",
                    new XElement(NsSifen + "iTipEmi", 1),
                    new XElement(NsSifen + "dDesTipEmi", "Normal"),
                    new XElement(NsSifen + "dCodSeg", codigoSeguridadDelCdc) // Extraído del CDC
                ),
                gTimb,
                gDatGralOpe,
                gDtipDE,
                gTotSub
                // gCamGen ELIMINADO - no aparece en XML de referencia APROBADO
            );

            // rDE envolvente
            // NOTA: Se eliminaron campos geográficos (cDepRec, cDisRec, cCiuRec) y de contacto (dTelRec, dCodCliente)
            // dNumCasRec ya se añadió condicionalmente en líneas anteriores

            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
            
            // FIX 31-Ene-2026: Orden EXACTO de atributos según DLL Power que FUNCIONA
            // Power genera: xmlns="..." xmlns:xsi="..." xsi:schemaLocation="..."
            // Para controlar el orden exacto, primero declaramos xmlns ANTES que xmlns:xsi
            var rde = new XElement(NsSifen + "rDE",
                // CRÍTICO: xmlns default DEBE ir PRIMERO - XAttribute(XNamespace.None + "xmlns", ...) NO funciona con XElement
                // Por eso usamos el namespace en NsSifen + "rDE" que lo declara implícitamente
                // Pero para garantizar el orden, agregamos explícitamente
                new XAttribute("xmlns", NsSifen.NamespaceName), // PRIMERO: xmlns="http://ekuatia.set.gov.py/sifen/xsd"
                new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName), // SEGUNDO: xmlns:xsi="..."
                // FIX 27-Ene-2026: schemaLocation según librería Java oficial (roshkadev/rshk-jsifenlib)
                // Constants.java línea 15: SIFEN_NS_URI_RECEP_DE = SIFEN_NS_URI + " siRecepDE_v150.xsd"
                // Resultado: "http://ekuatia.set.gov.py/sifen/xsd siRecepDE_v150.xsd"
                new XAttribute(xsi + "schemaLocation", "http://ekuatia.set.gov.py/sifen/xsd siRecepDE_v150.xsd"),
                new XElement(NsSifen + "dVerFor", "150"),
                de,
                // gCamFuFD (firma y QR data) debe estar a nivel de rDE (no dentro de DE) antes de firmar
                new XElement(NsSifen + "gCamFuFD", dCarQR)
            );

            var doc = new XDocument(rde);  // Sin XDeclaration - SIFEN no espera declaración XML
            // Retornar XML sin indentación para evitar problemas de formato
            using var ms = new MemoryStream();
            using var writer = XmlWriter.Create(ms, new XmlWriterSettings { 
                Indent = false, 
                OmitXmlDeclaration = true, // 10-Ene-2026: NO incluir declaración XML (igual que ejemplo Java)
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false) // UTF-8 sin BOM
            });
            doc.WriteTo(writer);
            writer.Flush();
            ms.Position = 0;
            // CRÍTICO: Leer con UTF-8 explícito para mantener caracteres especiales (tildes, ñ, etc.)
            using var reader = new StreamReader(ms, Encoding.UTF8);
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

        /// <summary>
        /// Obtiene el nombre del departamento según el código SIFEN
        /// </summary>
        private static string ObtenerNombreDepartamento(int codigo)
        {
            return codigo switch
            {
                0 => "CAPITAL",
                1 => "CAPITAL",
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
        }

        /// <summary>
        /// Obtiene el nombre del distrito desde la base de datos
        /// </summary>
        private static string ObtenerNombreDistrito(int codigo, AppDbContext? ctx = null)
        {
            if (ctx != null)
            {
                var distrito = ctx.DistritosCatalogo
                    .FirstOrDefault(d => d.Numero == codigo);
                if (distrito != null)
                    return distrito.Nombre?.ToUpper() ?? $"DISTRITO {codigo}";
            }
            
            // Fallback para casos sin contexto
            return codigo switch
            {
                1 => "ASUNCION (DISTRITO)",
                164 => "SAN LORENZO",
                _ => $"DISTRITO {codigo}"
            };
        }

        /// <summary>
        /// Obtiene el nombre de la ciudad desde la base de datos
        /// </summary>
        private static string ObtenerNombreCiudad(int codigo, AppDbContext? ctx = null)
        {
            if (ctx != null)
            {
                var ciudad = ctx.CiudadesCatalogo
                    .FirstOrDefault(c => c.Numero == codigo);
                if (ciudad != null)
                    return ciudad.Nombre?.ToUpper() ?? $"CIUDAD {codigo}";
            }
            
            // Fallback para casos sin contexto
            return codigo switch
            {
                1 => "ASUNCION (DISTRITO)",
                3840 => "SAN LORENZO",
                _ => $"CIUDAD {codigo}"
            };
        }
    }
}
