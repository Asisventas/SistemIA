using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemIA.Models;
using SistemIA.Models.Enums;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio que arma el Documento Electrónico (DE) secciones de pago E7 y valida totales.
    /// No implementa el SOAP; retorna un objeto serializable para integrarlo al armador SIFEN existente.
    /// </summary>
    public class DEBuilderService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        public DEBuilderService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        /// <summary>
        /// Valida si una venta tiene datos suficientes para construir un DE conforme a SIFEN (chequeos mínimos).
        /// No construye el XML, solo revisa presencia y coherencia básica.
        /// </summary>
        public async Task<DEValidacionResultado> ValidarVentaAsync(int idVenta)
        {
            var res = new DEValidacionResultado();
            await using var db = await _dbFactory.CreateDbContextAsync();

            var venta = await db.Ventas
                .Include(v => v.Sucursal)
                .Include(v => v.Cliente)
                .Include(v => v.Moneda)
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta);

            if (venta == null)
            {
                res.Errores.Add($"Venta {idVenta} no encontrada");
                return res;
            }

            // Emisor (Sucursal/Sociedad)
            if (venta.Sucursal == null)
            {
                res.Errores.Add("Sucursal no asociada a la venta");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(venta.Sucursal.RUC))
                    res.Errores.Add("RUC de la sucursal vacío");
                if (!venta.Sucursal.DV.HasValue)
                    res.Errores.Add("DV de la sucursal no definido");
                if (!venta.Sucursal.IdCiudad.HasValue)
                    res.Advertencias.Add("Sucursal sin ciudad (IdCiudad) vinculada a catálogo SIFEN");
            }

            var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
            if (sociedad == null)
            {
                res.Advertencias.Add("No existe registro en 'Sociedades' (se requiere para CSC/QR)");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(sociedad.IdCsc) || string.IsNullOrWhiteSpace(sociedad.Csc))
                    res.Advertencias.Add("Sociedad sin IdCsc/Csc definido (requerido para CDC/QR)");
            }

            // Numeración / Timbrado
            bool timbradoOk = !string.IsNullOrWhiteSpace(venta.Timbrado)
                              && !string.IsNullOrWhiteSpace(venta.Establecimiento)
                              && venta.Establecimiento!.Length == 3
                              && !string.IsNullOrWhiteSpace(venta.PuntoExpedicion)
                              && venta.PuntoExpedicion!.Length == 3
                              && !string.IsNullOrWhiteSpace(venta.NumeroFactura)
                              && venta.NumeroFactura!.Length == 7;
            if (!timbradoOk)
                res.Errores.Add("Numeración SIFEN incompleta (Timbrado/Establecimiento/Punto/NumeroFactura)");

            // Cliente (Receptor)
            if (venta.Cliente == null)
            {
                res.Errores.Add("Cliente no asociado");
            }
            else
            {
                if (venta.Cliente.NaturalezaReceptor == 1)
                {
                    // FIX 23-Ene-2026: DV puede ser 0 (es un valor válido)
                    // Solo validar que RUC no esté vacío. El DV=0 es válido para algunos RUCs.
                    if (string.IsNullOrWhiteSpace(venta.Cliente.RUC))
                        res.Errores.Add("Receptor contribuyente sin RUC válido");
                }
                else
                {
                    // Para no contribuyentes, el documento es opcional: si no se especifica, se usará Innominado (iTipIDRec=5, dNumIDRec=0)
                    if (venta.Cliente.TipoDocumentoIdentidadSifen.HasValue)
                    {
                        var tip = venta.Cliente.TipoDocumentoIdentidadSifen.Value;
                        var num = (venta.Cliente.NumeroDocumentoIdentidad ?? string.Empty).Trim();
                        if (tip == 5)
                        {
                            // Innominado: si no hay número, se usará "0"; no es error
                            if (string.IsNullOrWhiteSpace(num))
                                res.Advertencias.Add("Receptor no contribuyente Innominado sin número: se usará dNumIDRec=0");
                        }
                        else
                        {
                            // Si especifica un tipo de documento distinto a Innominado, el número es obligatorio
                            if (string.IsNullOrWhiteSpace(num))
                                res.Errores.Add("Receptor no contribuyente con tipo de documento sin número");
                        }
                    }
                    else
                    {
                        // Sin tipo de documento: aceptamos y caemos a Innominado en el generador de XML
                        res.Advertencias.Add("Receptor no contribuyente sin documento: se usará Innominado (iTipIDRec=5, dNumIDRec=0)");
                    }
                }
                if (venta.Cliente.IdCiudad == 0)
                    res.Advertencias.Add("Cliente sin ciudad (IdCiudad) del catálogo SIFEN");
            }

            // Moneda y tipo de cambio
            var iso = venta.Moneda?.CodigoISO ?? "PYG";
            if (iso != "PYG" && !(venta.CambioDelDia.HasValue && venta.CambioDelDia.Value > 0))
                res.Errores.Add("Venta en moneda extranjera sin tipo de cambio");

            // Detalles
            var detalles = await db.VentasDetalles.AsNoTracking().Where(d => d.IdVenta == idVenta).ToListAsync();
            if (detalles.Count == 0)
                res.Errores.Add("Venta sin ítems");
            else
            {
                foreach (var d in detalles)
                {
                    if (d.IdProducto == 0)
                        res.Errores.Add($"Detalle {d.IdVentaDetalle} sin producto");
                    if (!d.IdTipoIva.HasValue)
                        res.Advertencias.Add($"Detalle {d.IdVentaDetalle} sin tipo de IVA (IdTipoIva)");
                }
            }

            // Pagos (E7)
            var pago = await db.VentasPagos
                .Include(vp => vp.Detalles)
                .Include(vp => vp.Cuotas)
                .FirstOrDefaultAsync(vp => vp.IdVenta == idVenta);
            if (pago == null)
            {
                res.Errores.Add("No se registró el pago (VentasPagos) para la venta");
            }
            else
            {
                try
                {
                    // Reusar las validaciones del constructor E7 (sumas, cuotas)
                    var tmp = await ConstruirPagosAsync(idVenta);
                }
                catch (Exception ex)
                {
                    res.Errores.Add($"E7 inválido: {ex.Message}");
                }
            }

            res.Ok = res.Errores.Count == 0;
            res.IdVenta = idVenta;
            return res;
        }

        public async Task<DEPagoResult> ConstruirPagosAsync(int idVenta)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var venta = await db.Ventas.FirstOrDefaultAsync(v => v.IdVenta == idVenta)
                        ?? throw new InvalidOperationException($"Venta {idVenta} no encontrada");

            var pago = await db.VentasPagos
                .Include(vp => vp.Detalles)
                .Include(vp => vp.Cuotas)
                .FirstOrDefaultAsync(vp => vp.IdVenta == idVenta)
                ?? throw new InvalidOperationException("La venta no tiene registro de pago (VentasPagos)");

            // Validaciones de negocio
            var sumaDetallesGs = (pago.Detalles ?? new List<VentaPagoDetalle>()).Sum(d => d.MontoGs);
            if (pago.CondicionOperacion == 1)
            {
                // Contado: suma de pagos debe ser >= total (puede haber vuelto en efectivo)
                // El ImporteTotal ya está ajustado al total de la venta
                if (sumaDetallesGs < venta.Total)
                    throw new InvalidOperationException($"La suma de pagos (Gs) es menor que el total de la venta. Esperado>={venta.Total}, Actual={sumaDetallesGs}");
            }
            else if (pago.CondicionOperacion == 2)
            {
                // Crédito: anticipo (si existe) + suma pagos contado (si hubo) <= total y suma cuotas == saldo
                var anticipo = pago.Anticipo ?? 0m;
                if (pago.Cuotas == null || pago.Cuotas.Count == 0)
                    throw new InvalidOperationException("Venta en crédito sin cuotas definidas");

                var sumaCuotas = pago.Cuotas.Sum(c => c.MontoCuota);
                var saldoEsperado = venta.Total - anticipo;
                AssertCasiIgual(saldoEsperado, sumaCuotas, "La suma de cuotas no coincide con el saldo de la venta");
            }

            // Mapeo simplificado E7
            var e7 = new DEP_E7
            {
                iCondOpe = pago.CondicionOperacion,
                dMoneda = (await db.Monedas.FindAsync(pago.IdMoneda ?? venta.IdMoneda))?.CodigoISO ?? "PYG",
                dTiCam = pago.TipoCambio ?? venta.CambioDelDia ?? 1m,
                dTotGralOpe = venta.Total,
                gPagCred = PagoCredito.From(pago)
            };

            if (pago.Detalles != null)
            {
                foreach (var det in pago.Detalles)
                {
                    string? iso = null;
                    if (det.IdMoneda.HasValue)
                    {
                        var m = await db.Monedas.FindAsync(det.IdMoneda.Value);
                        iso = m?.CodigoISO;
                    }
                    e7.gPagos.Add(new PagoContado
                    {
                        iTiPago = (int)det.Medio,
                        dMonInt = det.Monto,
                        cMoneTiPag = iso,
                        dTiCamTiPag = det.TipoCambio,
                        dTiTarj = det.TipoTarjeta.HasValue ? (int)det.TipoTarjeta.Value : null,
                        dDesTiTarj = det.MarcaTarjeta,
                        dNomTit = det.NombreEmisorTarjeta,
                        dNumTarj = det.Ultimos4,
                        dAuthCode = det.NumeroAutorizacion,
                        dNomBco = det.BancoCheque,
                        dNumCheq = det.NumeroCheque,
                        dFeEmiCheq = det.FechaCobroCheque
                    });
                }
            }

            return new DEPagoResult
            {
                Venta = venta,
                Pago = pago,
                E7 = e7,
                Json = JsonSerializer.Serialize(e7)
            };
        }

        private static void AssertCasiIgual(decimal esperado, decimal actual, string mensaje)
        {
            var diff = Math.Abs(esperado - actual);
            if (diff > 1m) // tolerancia 1 guaraní
                throw new InvalidOperationException($"{mensaje}. Esperado={esperado}, Actual={actual}");
        }
    }

    #region DTOs DE (simplificados)
    public class DEP_E7
    {
        public int iCondOpe { get; set; } // 1 contado, 2 crédito
        public string dMoneda { get; set; } = "PYG";
        public decimal dTiCam { get; set; } = 1m;
        public decimal dTotGralOpe { get; set; }
        public List<PagoContado> gPagos { get; set; } = new();
        public PagoCredito? gPagCred { get; set; }
    }

    public class PagoContado
    {
        public int iTiPago { get; set; }
        public decimal dMonInt { get; set; }
        public string? cMoneTiPag { get; set; }
        public decimal? dTiCamTiPag { get; set; }

        // Tarjeta
        public int? dTiTarj { get; set; }
        public string? dDesTiTarj { get; set; }
        public string? dNomTit { get; set; }
        public string? dNumTarj { get; set; }
        public string? dAuthCode { get; set; }

        // Cheque
        public string? dNomBco { get; set; }
        public string? dNumCheq { get; set; }
        public DateTime? dFeEmiCheq { get; set; }

    // Factory inline movida al servicio
    }

    public class PagoCredito
    {
        public decimal? dAnticipo { get; set; }
        public List<PagoCuota> gCuotas { get; set; } = new();

        public static PagoCredito? From(VentaPago p)
        {
            if (p.CondicionOperacion != 2) return null;
            var pc = new PagoCredito { dAnticipo = p.Anticipo };
            if (p.Cuotas != null)
            {
                foreach (var c in p.Cuotas.OrderBy(x => x.NumeroCuota))
                {
                    pc.gCuotas.Add(new PagoCuota
                    {
                        cMoneCuo = p.Moneda?.CodigoISO ?? "PYG",
                        dMonCuota = c.MontoCuota,
                        dFeFinCuo = c.FechaVencimiento
                    });
                }
            }
            return pc;
        }
    }

    public class PagoCuota
    {
        public string cMoneCuo { get; set; } = "PYG";
        public decimal dMonCuota { get; set; }
        public DateTime dFeFinCuo { get; set; }
    }

    public class DEPagoResult
    {
        public Venta Venta { get; set; } = null!;
        public VentaPago Pago { get; set; } = null!;
        public DEP_E7 E7 { get; set; } = null!;
        public string Json { get; set; } = string.Empty;
    }

    public class DEValidacionResultado
    {
        public int IdVenta { get; set; }
        public bool Ok { get; set; }
        public List<string> Errores { get; } = new();
        public List<string> Advertencias { get; } = new();
    }
    #endregion
}
