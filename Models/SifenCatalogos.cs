namespace SistemIA.Models
{
    /// <summary>
    /// Constantes y catálogos de códigos SIFEN para facilitar su uso
    /// Basado en Manual Técnico SIFEN v150
    /// </summary>
    public static class SifenCatalogos
    {
        /// <summary>
        /// Naturaleza del receptor SIFEN (iNatRec)
        /// </summary>
        public static class NaturalezaReceptor
        {
            public const int CONTRIBUYENTE = 1;
            public const int NO_CONTRIBUYENTE = 2;

            public static string ObtenerDescripcion(int codigo)
            {
                return codigo switch
                {
                    CONTRIBUYENTE => "Contribuyente - Persona con RUC activo",
                    NO_CONTRIBUYENTE => "No contribuyente - Consumidor final sin RUC",
                    _ => "Código no válido"
                };
            }
        }

        /// <summary>
        /// Tipos de operación SIFEN (iTiOpe)
        /// </summary>
        public static class TipoOperacion
        {
            public const string B2B = "1"; // Business to Business
            public const string B2C = "2"; // Business to Consumer  
            public const string B2G = "3"; // Business to Government
            public const string B2F = "4"; // Business to Foreigner

            public static string ObtenerDescripcion(string codigo)
            {
                return codigo switch
                {
                    B2B => "B2B - Empresa a Empresa",
                    B2C => "B2C - Empresa a Consumidor Final",
                    B2G => "B2G - Empresa a Gobierno",
                    B2F => "B2F - Empresa a Extranjero",
                    _ => "Código no válido"
                };
            }

            public static int ObtenerCodigoNumerico(string codigo)
            {
                return codigo switch
                {
                    B2B => 1,
                    B2C => 2,
                    B2G => 3,
                    B2F => 4,
                    _ => 1 // Por defecto B2B
                };
            }
        }

        /// <summary>
        /// Tipos de documento de identidad para no contribuyentes (iTipIDRec)
        /// </summary>
        public static class TipoDocumentoIdentidad
        {
            public const int CEDULA_PARAGUAYA = 1;
            public const int PASAPORTE = 2;
            public const int CEDULA_EXTRANJERA = 3;
            public const int OTRO_DOCUMENTO = 4;

            public static string ObtenerDescripcion(int codigo)
            {
                return codigo switch
                {
                    CEDULA_PARAGUAYA => "Cédula de Identidad Paraguaya",
                    PASAPORTE => "Pasaporte",
                    CEDULA_EXTRANJERA => "Cédula de Identidad Extranjera",
                    OTRO_DOCUMENTO => "Otro documento de identidad",
                    _ => "Código no válido"
                };
            }
        }

        /// <summary>
        /// Códigos de departamentos de Paraguay según SIFEN
        /// Lista parcial de los más comunes - consultar catálogo completo en Manual SIFEN
        /// </summary>
        public static class Departamentos
        {
            public const string CAPITAL = "01";
            public const string SAN_PEDRO = "02";
            public const string CORDILLERA = "03";
            public const string GUAIRA = "04";
            public const string CAAGUAZU = "05";
            public const string CAAZAPA = "06";
            public const string ITAPUA = "07";
            public const string MISIONES = "08";
            public const string PARAGUARI = "09";
            public const string ALTO_PARANA = "10";
            public const string CENTRAL = "11";
            public const string ÑEEMBUCU = "12";
            public const string AMAMBAY = "13";
            public const string CANINDEYU = "14";
            public const string PRESIDENTE_HAYES = "15";
            public const string ALTO_PARAGUAY = "16";
            public const string BOQUERON = "17";

            public static string ObtenerNombre(string codigo)
            {
                return codigo switch
                {
                    CAPITAL => "CAPITAL",
                    SAN_PEDRO => "SAN PEDRO",
                    CORDILLERA => "CORDILLERA",
                    GUAIRA => "GUAIRA",
                    CAAGUAZU => "CAAGUAZU",
                    CAAZAPA => "CAAZAPA",
                    ITAPUA => "ITAPUA",
                    MISIONES => "MISIONES",
                    PARAGUARI => "PARAGUARI",
                    ALTO_PARANA => "ALTO PARANA",
                    CENTRAL => "CENTRAL",
                    ÑEEMBUCU => "ÑEEMBUCU",
                    AMAMBAY => "AMAMBAY",
                    CANINDEYU => "CANINDEYU",
                    PRESIDENTE_HAYES => "PDTE. HAYES",
                    ALTO_PARAGUAY => "ALTO PARAGUAY",
                    BOQUERON => "BOQUERON",
                    _ => "DEPARTAMENTO NO IDENTIFICADO"
                };
            }

            /// <summary>
            /// Retorna lista de todos los departamentos para dropdowns
            /// </summary>
            public static List<(string Codigo, string Nombre)> ObtenerTodos()
            {
                return new List<(string, string)>
                {
                    (CAPITAL, ObtenerNombre(CAPITAL)),
                    (SAN_PEDRO, ObtenerNombre(SAN_PEDRO)),
                    (CORDILLERA, ObtenerNombre(CORDILLERA)),
                    (GUAIRA, ObtenerNombre(GUAIRA)),
                    (CAAGUAZU, ObtenerNombre(CAAGUAZU)),
                    (CAAZAPA, ObtenerNombre(CAAZAPA)),
                    (ITAPUA, ObtenerNombre(ITAPUA)),
                    (MISIONES, ObtenerNombre(MISIONES)),
                    (PARAGUARI, ObtenerNombre(PARAGUARI)),
                    (ALTO_PARANA, ObtenerNombre(ALTO_PARANA)),
                    (CENTRAL, ObtenerNombre(CENTRAL)),
                    (ÑEEMBUCU, ObtenerNombre(ÑEEMBUCU)),
                    (AMAMBAY, ObtenerNombre(AMAMBAY)),
                    (CANINDEYU, ObtenerNombre(CANINDEYU)),
                    (PRESIDENTE_HAYES, ObtenerNombre(PRESIDENTE_HAYES)),
                    (ALTO_PARAGUAY, ObtenerNombre(ALTO_PARAGUAY)),
                    (BOQUERON, ObtenerNombre(BOQUERON))
                };
            }
        }

        /// <summary>
        /// Algunos distritos más comunes - la lista completa está en el Manual SIFEN
        /// </summary>
        public static class Distritos
        {
            // Capital
            public const string ASUNCION = "0001";
            
            // Central  
            public const string SAN_LORENZO = "0002";
            public const string LUQUE = "0003";
            public const string FERNANDO_DE_LA_MORA = "0004";
            public const string LAMBARE = "0005";
            public const string MARIANO_ROQUE_ALONSO = "0006";
            public const string ÑEMBY = "0007";
            public const string VILLA_ELISA = "0008";

            public static string ObtenerNombre(string codigo)
            {
                return codigo switch
                {
                    ASUNCION => "ASUNCION",
                    SAN_LORENZO => "SAN LORENZO", 
                    LUQUE => "LUQUE",
                    FERNANDO_DE_LA_MORA => "FERNANDO DE LA MORA",
                    LAMBARE => "LAMBARE",
                    MARIANO_ROQUE_ALONSO => "MARIANO R. ALONSO",
                    ÑEMBY => "ÑEMBY",
                    VILLA_ELISA => "VILLA ELISA",
                    _ => "DISTRITO NO IDENTIFICADO"
                };
            }
        }

        /// <summary>
        /// Países más comunes según catálogo SIFEN
        /// </summary>
        public static class Paises
        {
            public const string PARAGUAY = "PRY";
            public const string ARGENTINA = "ARG";
            public const string BRASIL = "BRA";
            public const string BOLIVIA = "BOL";
            public const string URUGUAY = "URY";
            public const string CHILE = "CHL";

            public static string ObtenerNombre(string codigo)
            {
                return codigo switch
                {
                    PARAGUAY => "Paraguay",
                    ARGENTINA => "Argentina",
                    BRASIL => "Brasil", 
                    BOLIVIA => "Bolivia",
                    URUGUAY => "Uruguay",
                    CHILE => "Chile",
                    _ => "País no identificado"
                };
            }
        }
    }
}
