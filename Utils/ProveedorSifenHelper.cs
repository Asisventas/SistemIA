using SistemIA.Models;

namespace SistemIA.Utils
{
    public static class ProveedorSifenHelper
    {
        // Diccionario de actividades económicas comunes en Paraguay
        public static readonly Dictionary<string, string> ActividadesEconomicasComunes = new()
        {
            { "46900", "VENTA AL POR MAYOR NO ESPECIALIZADA" },
            { "47190", "VENTA AL POR MENOR EN COMERCIOS NO ESPECIALIZADOS" },
            { "47110", "VENTA AL POR MENOR EN SUPERMERCADOS" },
            { "47300", "VENTA AL POR MENOR DE COMBUSTIBLE PARA VEHÍCULOS" },
            { "47520", "VENTA AL POR MENOR DE MATERIALES DE CONSTRUCCIÓN" },
            { "47730", "VENTA AL POR MENOR DE PRODUCTOS FARMACÉUTICOS" },
            { "56101", "RESTAURANTES" },
            { "56102", "BARES" },
            { "68100", "COMPRAVENTA DE BIENES RAÍCES" },
            { "45200", "MANTENIMIENTO Y REPARACIÓN DE VEHÍCULOS" },
            { "62010", "PROGRAMACIÓN INFORMÁTICA" },
            { "69200", "SERVICIOS DE CONTABILIDAD" },
            { "82990", "OTROS SERVICIOS DE APOYO A LAS EMPRESAS" },
            { "85410", "EDUCACIÓN SECUNDARIA GENERAL" },
            { "86210", "SERVICIOS DE MEDICINA GENERAL" },
            { "96020", "PELUQUERÍAS Y OTROS TRATAMIENTOS DE BELLEZA" }
        };

        // Ciudades principales de Paraguay con códigos departamentales
        public static readonly Dictionary<int, (string nombre, string codigoDep, string descripcionDep)> CiudadesPrincipales = new()
        {
            // Según catálogos SIFEN (cDep: 01..17)
            { 1, ("ASUNCIÓN", "01", "CAPITAL") },
            { 2, ("FERNANDO DE LA MORA", "11", "CENTRAL") },
            { 3, ("LAMBARÉ", "11", "CENTRAL") },
            { 4, ("LUQUE", "11", "CENTRAL") },
            { 5, ("MARIANO ROQUE ALONSO", "11", "CENTRAL") },
            { 6, ("ÑEMBY", "11", "CENTRAL") },
            { 7, ("SAN LORENZO", "11", "CENTRAL") },
            { 8, ("VILLA ELISA", "11", "CENTRAL") },
            { 9, ("CAPIATÁ", "11", "CENTRAL") },
            { 10, ("CIUDAD DEL ESTE", "10", "ALTO PARANÁ") },
            { 11, ("ENCARNACIÓN", "07", "ITAPÚA") },
            { 12, ("PEDRO JUAN CABALLERO", "13", "AMAMBAY") },
            { 13, ("CORONEL OVIEDO", "05", "CAAGUAZÚ") },
            { 14, ("CAACUPÉ", "03", "CORDILLERA") },
            { 15, ("VILLARRICA", "04", "GUAIRÁ") }
        };

        public static void ConfigurarValoresPorDefecto(ProveedorSifenMejorado proveedor)
        {
            proveedor.Ambiente = "test";
            proveedor.TipoContribuyente = 2; // Persona Jurídica
            proveedor.TipoRegimen = 1; // Régimen General
            proveedor.Establecimiento = "001";
            proveedor.PuntoExpedicion = "001";
            proveedor.Estado = true;
            proveedor.CodigoCiudad = 1; // Asunción por defecto
            proveedor.CodigoDepartamento = "01";
            proveedor.DescripcionDepartamento = "CAPITAL";
        }

        public static string GenerarCodigoProveedor(int ultimoNumero)
        {
            // Código correlativo simple: 01, 02, 03...
            return ultimoNumero.ToString("D2");
        }

        public static bool ValidarRucParaguayo(string ruc)
        {
            if (string.IsNullOrWhiteSpace(ruc)) return false;
            if (ruc.Length != 8) return false;
            if (!ruc.All(char.IsDigit)) return false;
            return true;
        }

        public static void CalcularDvRuc(ProveedorSifenMejorado proveedor)
        {
            if (string.IsNullOrWhiteSpace(proveedor.RUC) || proveedor.RUC.Length != 8)
            {
                proveedor.DV = 0;
                return;
            }

            // Algoritmo de cálculo de DV para RUC paraguayo (versión oficial SET)
            int[] pesos = { 2, 3, 4, 5, 6, 7, 8, 9 };
            int suma = 0;

            // De derecha a izquierda
            for (int pos = 7; pos >= 0; pos--)
            {
                int digito = int.Parse(proveedor.RUC[pos].ToString());
                suma += digito * pesos[7 - pos];
            }

            int resto = (suma * 10) % 11;
            proveedor.DV = resto;
            if (proveedor.DV == 10)
                proveedor.DV = 0;
        }

        public static void ConfigurarDatosGeograficos(ProveedorSifenMejorado proveedor, int codigoCiudad)
        {
            if (CiudadesPrincipales.TryGetValue(codigoCiudad, out var ciudad))
            {
                proveedor.CodigoDepartamento = ciudad.codigoDep;
                proveedor.DescripcionDepartamento = ciudad.descripcionDep;
            }
        }

        public static List<(string codigo, string descripcion)> SugerirActividadesEconomicas(string palabrasClave)
        {
            if (string.IsNullOrWhiteSpace(palabrasClave))
                return new List<(string, string)>();

            var palabras = palabrasClave.ToUpper().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            return ActividadesEconomicasComunes
                .Where(kvp => palabras.Any(palabra => kvp.Value.Contains(palabra)))
                .Take(5)
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToList();
        }

        public static List<string> GenerarResumenValidacion(ProveedorSifenMejorado proveedor)
        {
            var resumen = new List<string>();

            // Validación RUC
            if (ValidarRucParaguayo(proveedor.RUC))
            {
                resumen.Add($"✅ RUC: {proveedor.RUC}-{proveedor.DV}");
            }
            else
            {
                resumen.Add("❌ RUC: Formato inválido");
            }

            // Validación Razón Social
            if (!string.IsNullOrWhiteSpace(proveedor.RazonSocial))
            {
                resumen.Add($"✅ Razón Social: {proveedor.RazonSocial}");
            }
            else
            {
                resumen.Add("❌ Razón Social: Requerida");
            }

            // Validación Timbrado
            var (estadoTimbrado, dias, _) = VerificarTimbrado(proveedor.VencimientoTimbrado);
            if (dias >= 0)
            {
                resumen.Add($"✅ Timbrado: {proveedor.Timbrado} ({estadoTimbrado})");
            }
            else
            {
                resumen.Add($"❌ Timbrado: {proveedor.Timbrado} (Vencido)");
            }

            // Validación Actividad Económica
            if (!string.IsNullOrWhiteSpace(proveedor.CodigoActividadEconomica) && 
                !string.IsNullOrWhiteSpace(proveedor.DescripcionActividadEconomica))
            {
                resumen.Add($"✅ Actividad: {proveedor.CodigoActividadEconomica}");
            }
            else
            {
                resumen.Add("❌ Actividad Económica: Requerida");
            }

            // Validación Contacto
            if (!string.IsNullOrWhiteSpace(proveedor.Email) && !string.IsNullOrWhiteSpace(proveedor.Telefono))
            {
                resumen.Add("✅ Datos de Contacto: Completos");
            }
            else
            {
                resumen.Add("❌ Datos de Contacto: Incompletos");
            }

            // Validación Dirección
            if (!string.IsNullOrWhiteSpace(proveedor.Direccion) && proveedor.CodigoCiudad > 0)
            {
                resumen.Add("✅ Ubicación: Configurada");
            }
            else
            {
                resumen.Add("❌ Ubicación: Incompleta");
            }

            return resumen;
        }

        public static (string estado, int dias, string clase) VerificarTimbrado(DateTime? vencimiento)
        {
            if (!vencimiento.HasValue) 
                return ("No especificado", 0, "secondary");

            var dias = (vencimiento.Value.Date - DateTime.Today).Days;

            if (dias < 0)
                return ("Vencido", dias, "danger");
            else if (dias <= 30)
                return ("Por vencer", dias, "warning");
            else
                return ("Vigente", dias, "success");
        }
    }
}
