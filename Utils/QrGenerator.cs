using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SistemIA.Utils
{
    /// <summary>
    /// Generador de URL QR para facturas electrónicas según especificación SIFEN
    /// </summary>
    public static class QrGenerator
    {
        /// <summary>
        /// Genera la URL del QR para una factura electrónica
        /// </summary>
        /// <param name="cdc">CDC de 44 dígitos</param>
        /// <param name="fechaEmision">Fecha de emisión del documento</param>
        /// <param name="rucReceptor">RUC del receptor/cliente</param>
        /// <param name="totalOperacion">Total de la operación en guaraníes</param>
        /// <param name="totalIva">Total de IVA (suma de IVA5 + IVA10)</param>
        /// <param name="cantidadItems">Cantidad de ítems en la factura</param>
        /// <param name="digestValue">DigestValue de la firma (en base64)</param>
        /// <param name="idCsc">ID del Código de Seguridad del Contribuyente</param>
        /// <param name="hashQr">Hash del QR (calculado después de la firma)</param>
        /// <param name="ambiente">Ambiente (false=test, true=producción)</param>
        /// <returns>URL completa del QR</returns>
        public static string GenerarUrlQR(
            string cdc,
            DateTime fechaEmision,
            string rucReceptor,
            decimal totalOperacion,
            decimal totalIva,
            int cantidadItems,
            string digestValue,
            string idCsc = "1",
            string? hashQr = null,
            bool ambiente = false)
        {
            // URL base según ambiente
            string urlBase = ambiente
                ? "https://ekuatia.set.gov.py/consultas/qr"
                : "https://ekuatia.set.gov.py/consultas-test/qr";

            // Limpiar CDC (solo dígitos)
            string cdcLimpio = new string(cdc.Where(char.IsDigit).ToArray());

            // Formatear fecha en formato ISO 8601 y convertir a hex
            string fechaIso = fechaEmision.ToString("yyyy-MM-ddTHH:mm:ss");
            string fechaHex = ConvertirAHex(fechaIso);

            // Limpiar RUC receptor
            string rucLimpio = new string((rucReceptor ?? "0").Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(rucLimpio))
                rucLimpio = "0";

            // Totales como enteros (sin decimales)
            long totalOp = (long)Math.Round(totalOperacion);
            long totalIvaLong = (long)Math.Round(totalIva);

            // Convertir DigestValue a hex
            string digestHex = ConvertirBase64AHex(digestValue);

            // Si no se proporciona hashQr, usar PLACEHOLDER
            string hashQrFinal = string.IsNullOrWhiteSpace(hashQr) ? "PLACEHOLDER" : hashQr;

            // Construir URL
            var parametros = new[]
            {
                $"nVersion=150",
                $"Id={cdcLimpio}",
                $"dFeEmiDE={fechaHex}",
                $"dRucRec={rucLimpio}",
                $"dTotGralOpe={totalOp}",
                $"dTotIVA={totalIvaLong}",
                $"cItems={cantidadItems}",
                $"DigestValue={digestHex}",
                $"IdCSC={idCsc}",
                $"cHashQR={hashQrFinal}"
            };

            return $"{urlBase}?{string.Join("&", parametros)}";
        }

        /// <summary>
        /// Calcula el hash QR después de tener el DigestValue de la firma
        /// </summary>
        /// <param name="urlQrSinHash">URL del QR sin el parámetro cHashQR</param>
        /// <param name="codigoSeguridad">Código de seguridad del contribuyente (CSC)</param>
        /// <returns>Hash QR en formato hexadecimal</returns>
        public static string CalcularHashQR(string urlQrSinHash, string codigoSeguridad)
        {
            // El hash se calcula sobre: URL completa (sin cHashQR) + CSC
            string textoParaHash = urlQrSinHash + codigoSeguridad;

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(textoParaHash);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Convierte una cadena de texto a su representación hexadecimal
        /// </summary>
        private static string ConvertirAHex(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            byte[] bytes = Encoding.UTF8.GetBytes(texto);
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Convierte un valor en Base64 a su representación hexadecimal
        /// </summary>
        private static string ConvertirBase64AHex(string base64)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(base64))
                    return string.Empty;

                byte[] bytes = Convert.FromBase64String(base64);
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
            catch
            {
                // Si no es un Base64 válido, convertir como texto
                return ConvertirAHex(base64);
            }
        }

        /// <summary>
        /// Extrae los parámetros de una URL de QR existente
        /// </summary>
        public static QrInfo? ExtraerParametros(string urlQr)
        {
            if (string.IsNullOrWhiteSpace(urlQr))
                return null;

            try
            {
                var uri = new Uri(urlQr);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

                return new QrInfo
                {
                    Version = query["nVersion"] ?? string.Empty,
                    Cdc = query["Id"] ?? string.Empty,
                    FechaEmisionHex = query["dFeEmiDE"] ?? string.Empty,
                    RucReceptor = query["dRucRec"] ?? string.Empty,
                    TotalOperacion = long.TryParse(query["dTotGralOpe"], out long tot) ? tot : 0,
                    TotalIva = long.TryParse(query["dTotIVA"], out long iva) ? iva : 0,
                    CantidadItems = int.TryParse(query["cItems"], out int items) ? items : 0,
                    DigestValue = query["DigestValue"] ?? string.Empty,
                    IdCsc = query["IdCSC"] ?? string.Empty,
                    HashQr = query["cHashQR"] ?? string.Empty
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Valida que una URL de QR tenga todos los parámetros necesarios
        /// </summary>
        public static bool ValidarUrlQR(string urlQr)
        {
            var info = ExtraerParametros(urlQr);
            if (info == null)
                return false;

            return !string.IsNullOrWhiteSpace(info.Cdc) &&
                   !string.IsNullOrWhiteSpace(info.FechaEmisionHex) &&
                   !string.IsNullOrWhiteSpace(info.DigestValue) &&
                   !string.IsNullOrWhiteSpace(info.HashQr) &&
                   info.HashQr != "PLACEHOLDER";
        }
    }

    /// <summary>
    /// Información extraída de una URL de QR
    /// </summary>
    public class QrInfo
    {
        public string Version { get; set; } = string.Empty;
        public string Cdc { get; set; } = string.Empty;
        public string FechaEmisionHex { get; set; } = string.Empty;
        public string RucReceptor { get; set; } = string.Empty;
        public long TotalOperacion { get; set; }
        public long TotalIva { get; set; }
        public int CantidadItems { get; set; }
        public string DigestValue { get; set; } = string.Empty;
        public string IdCsc { get; set; } = string.Empty;
        public string HashQr { get; set; } = string.Empty;

        public DateTime? ParseFechaEmision()
        {
            try
            {
                // Convertir de hex a string
                byte[] bytes = new byte[FechaEmisionHex.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(FechaEmisionHex.Substring(i * 2, 2), 16);
                }
                string fechaStr = Encoding.UTF8.GetString(bytes);
                return DateTime.Parse(fechaStr);
            }
            catch
            {
                return null;
            }
        }

        public override string ToString()
        {
            return $"CDC: {Cdc}, RUC Receptor: {RucReceptor}, Total: {TotalOperacion:N0}";
        }
    }
}
