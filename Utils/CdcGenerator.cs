using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SistemIA.Utils
{
    /// <summary>
    /// Generador del CDC (Código de Control) según especificación SIFEN versión 150
    /// Formato: 44 dígitos distribuidos según manual técnico
    /// </summary>
    public static class CdcGenerator
    {
        /// <summary>
        /// Genera el CDC completo de 44 dígitos para una factura electrónica
        /// </summary>
        /// <param name="tipoDocumento">Tipo de documento (01=Factura, 04=Autofactura, etc.)</param>
        /// <param name="rucEmisor">RUC del emisor (8 dígitos)</param>
        /// <param name="dvEmisor">Dígito verificador del RUC emisor (1 dígito)</param>
        /// <param name="establecimiento">Número de establecimiento/sucursal (3 dígitos)</param>
        /// <param name="puntoExpedicion">Punto de expedición/caja (3 dígitos)</param>
        /// <param name="numeroFactura">Número de factura (7 dígitos)</param>
        /// <param name="tipoContribuyente">Tipo de contribuyente (1=Persona física, 2=Persona jurídica)</param>
        /// <param name="fechaEmision">Fecha de emisión</param>
        /// <param name="tipoEmision">Tipo de emisión (1=Normal, 2=Contingencia)</param>
        /// <param name="codigoSeguridadExistente">Código de seguridad de 9 dígitos (opcional, se genera si es null)</param>
        /// <returns>CDC de 44 dígitos</returns>
        public static string GenerarCDC(
            string tipoDocumento,
            string rucEmisor,
            string dvEmisor,
            string establecimiento,
            string puntoExpedicion,
            string numeroFactura,
            string tipoContribuyente,
            DateTime fechaEmision,
            string tipoEmision,
            string? codigoSeguridadExistente = null)
        {
            // Limpiar y formatear cada componente
            tipoDocumento = LimpiarYPadLeft(tipoDocumento, 2);
            rucEmisor = LimpiarYPadLeft(rucEmisor, 8);
            dvEmisor = LimpiarYPadLeft(dvEmisor, 1);
            establecimiento = LimpiarYPadLeft(establecimiento, 3);
            puntoExpedicion = LimpiarYPadLeft(puntoExpedicion, 3);
            numeroFactura = LimpiarYPadLeft(numeroFactura, 7);
            tipoContribuyente = LimpiarYPadLeft(tipoContribuyente, 1);
            string fechaStr = fechaEmision.ToString("yyyyMMdd"); // 8 dígitos
            tipoEmision = LimpiarYPadLeft(tipoEmision, 1);

            // Generar o usar código de seguridad de 9 dígitos
            string codigoSeguridad = string.IsNullOrWhiteSpace(codigoSeguridadExistente)
                ? GenerarCodigoSeguridad()
                : LimpiarYPadLeft(codigoSeguridadExistente, 9);

            // Construir los primeros 43 dígitos
            string cdc43 = tipoDocumento + rucEmisor + dvEmisor + establecimiento + 
                          puntoExpedicion + numeroFactura + tipoContribuyente + 
                          fechaStr + tipoEmision + codigoSeguridad;

            // Calcular el dígito verificador (posición 44)
            string digitoVerificador = CalcularDigitoVerificador(cdc43);

            // CDC completo de 44 dígitos
            string cdcCompleto = cdc43 + digitoVerificador;

            return cdcCompleto;
        }

        /// <summary>
        /// Calcula el dígito verificador usando módulo 11 con base 2-9
        /// </summary>
        private static string CalcularDigitoVerificador(string cdc43)
        {
            int suma = 0;
            int multiplicador = 2;

            // Recorrer de derecha a izquierda
            for (int i = cdc43.Length - 1; i >= 0; i--)
            {
                int digito = int.Parse(cdc43[i].ToString());
                suma += digito * multiplicador;

                multiplicador++;
                if (multiplicador > 9)
                    multiplicador = 2;
            }

            int modulo = suma % 11;
            int dv = 11 - modulo;

            // Si el dígito verificador es 10 u 11, se usa 0
            if (dv >= 10)
                dv = 0;

            return dv.ToString();
        }

        /// <summary>
        /// Genera un código de seguridad aleatorio de 9 dígitos
        /// </summary>
        private static string GenerarCodigoSeguridad()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[4];
                rng.GetBytes(bytes);
                int numero = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000000;
                return numero.ToString("D9");
            }
        }

        /// <summary>
        /// Limpia una cadena dejando solo dígitos y la rellena con ceros a la izquierda
        /// </summary>
        private static string LimpiarYPadLeft(string valor, int longitud)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return new string('0', longitud);

            // Extraer solo dígitos
            string soloDigitos = new string(valor.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(soloDigitos))
                return new string('0', longitud);

            // Si es más largo, tomar los últimos dígitos
            if (soloDigitos.Length > longitud)
                return soloDigitos.Substring(soloDigitos.Length - longitud);

            // Si es más corto, rellenar con ceros a la izquierda
            return soloDigitos.PadLeft(longitud, '0');
        }

        /// <summary>
        /// Valida el formato de un CDC
        /// </summary>
        public static bool ValidarCDC(string cdc)
        {
            if (string.IsNullOrWhiteSpace(cdc))
                return false;

            // Limpiar
            string cdcLimpio = new string(cdc.Where(char.IsDigit).ToArray());

            if (cdcLimpio.Length != 44)
                return false;

            // Extraer los primeros 43 dígitos y el dígito verificador
            string cdc43 = cdcLimpio.Substring(0, 43);
            string dvRecibido = cdcLimpio.Substring(43, 1);

            // Calcular el DV esperado
            string dvCalculado = CalcularDigitoVerificador(cdc43);

            return dvRecibido == dvCalculado;
        }

        /// <summary>
        /// Extrae componentes del CDC para análisis
        /// </summary>
        public static CdcInfo ExtraerInfo(string cdc)
        {
            if (string.IsNullOrWhiteSpace(cdc))
                throw new ArgumentException("CDC no puede estar vacío");

            string cdcLimpio = new string(cdc.Where(char.IsDigit).ToArray());

            if (cdcLimpio.Length != 44)
                throw new ArgumentException($"CDC debe tener 44 dígitos, tiene {cdcLimpio.Length}");

            return new CdcInfo
            {
                TipoDocumento = cdcLimpio.Substring(0, 2),
                RucEmisor = cdcLimpio.Substring(2, 8),
                DvEmisor = cdcLimpio.Substring(10, 1),
                Establecimiento = cdcLimpio.Substring(11, 3),
                PuntoExpedicion = cdcLimpio.Substring(14, 3),
                NumeroFactura = cdcLimpio.Substring(17, 7),
                TipoContribuyente = cdcLimpio.Substring(24, 1),
                FechaEmision = cdcLimpio.Substring(25, 8),
                TipoEmision = cdcLimpio.Substring(33, 1),
                CodigoSeguridad = cdcLimpio.Substring(34, 9),
                DigitoVerificador = cdcLimpio.Substring(43, 1)
            };
        }

        /// <summary>
        /// Formatea el CDC con espacios cada 4 dígitos para mejor legibilidad
        /// </summary>
        public static string FormatearCDC(string cdc)
        {
            if (string.IsNullOrWhiteSpace(cdc))
                return string.Empty;

            string cdcLimpio = new string(cdc.Where(char.IsDigit).ToArray());

            if (cdcLimpio.Length != 44)
                return cdc;

            var partes = new System.Collections.Generic.List<string>();
            for (int i = 0; i < cdcLimpio.Length; i += 4)
            {
                int longitud = Math.Min(4, cdcLimpio.Length - i);
                partes.Add(cdcLimpio.Substring(i, longitud));
            }

            return string.Join(" ", partes);
        }
    }

    /// <summary>
    /// Información desglosada del CDC
    /// </summary>
    public class CdcInfo
    {
        public string TipoDocumento { get; set; } = string.Empty;
        public string RucEmisor { get; set; } = string.Empty;
        public string DvEmisor { get; set; } = string.Empty;
        public string Establecimiento { get; set; } = string.Empty;
        public string PuntoExpedicion { get; set; } = string.Empty;
        public string NumeroFactura { get; set; } = string.Empty;
        public string TipoContribuyente { get; set; } = string.Empty;
        public string FechaEmision { get; set; } = string.Empty;
        public string TipoEmision { get; set; } = string.Empty;
        public string CodigoSeguridad { get; set; } = string.Empty;
        public string DigitoVerificador { get; set; } = string.Empty;

        public DateTime ParseFechaEmision()
        {
            return DateTime.ParseExact(FechaEmision, "yyyyMMdd", null);
        }

        public override string ToString()
        {
            return $"Tipo Doc: {TipoDocumento}, RUC: {RucEmisor}-{DvEmisor}, " +
                   $"Est: {Establecimiento}, Pto.Exp: {PuntoExpedicion}, " +
                   $"Nro: {NumeroFactura}, Fecha: {FechaEmision}";
        }
    }
}
