namespace SistemIA.Utils;

public static class NumeroALetras
{
    private static readonly string[] _unidades = {
        "", "UNO", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE"
    };

    private static readonly string[] _decenas = {
        "", "DIEZ", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", 
        "SESENTA", "SETENTA", "OCHENTA", "NOVENTA"
    };

    private static readonly string[] _especiales = {
        "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE",
        "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE"
    };

    private static readonly string[] _centenas = {
        "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS",
        "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS"
    };

    public static string ConvertirALetras(long numero, string codigoMoneda = "PYG")
    {
        if (numero == 0)
            return $"CERO {ObtenerNombreMonedaPlural(codigoMoneda)}";

        if (numero < 0)
            return "MENOS " + ConvertirALetras(Math.Abs(numero), codigoMoneda);

        string letras = ConvertirNumero(numero);
        
        // Agregar nombre de la moneda (singular o plural)
        if (numero == 1)
            letras += " " + ObtenerNombreMonedaSingular(codigoMoneda);
        else
            letras += " " + ObtenerNombreMonedaPlural(codigoMoneda);

        return letras.Trim();
    }

    private static string ObtenerNombreMonedaSingular(string codigoMoneda)
    {
        return codigoMoneda?.ToUpperInvariant() switch
        {
            "USD" => "DÓLAR AMERICANO",
            "PYG" => "GUARANÍ",
            "ARS" => "PESO ARGENTINO",
            "BRL" => "REAL BRASILEÑO",
            "EUR" => "EURO",
            _ => "GUARANÍ"
        };
    }

    private static string ObtenerNombreMonedaPlural(string codigoMoneda)
    {
        return codigoMoneda?.ToUpperInvariant() switch
        {
            "USD" => "DÓLARES AMERICANOS",
            "PYG" => "GUARANÍES",
            "ARS" => "PESOS ARGENTINOS",
            "BRL" => "REALES BRASILEÑOS",
            "EUR" => "EUROS",
            _ => "GUARANÍES"
        };
    }

    private static string ConvertirNumero(long numero)
    {
        if (numero == 0)
            return "";

        if (numero < 10)
            return _unidades[numero];

        if (numero >= 10 && numero < 20)
            return _especiales[numero - 10];

        if (numero >= 20 && numero < 100)
        {
            int decena = (int)(numero / 10);
            int unidad = (int)(numero % 10);
            
            if (unidad == 0)
                return _decenas[decena];
            else
                return _decenas[decena] + " Y " + _unidades[unidad];
        }

        if (numero >= 100 && numero < 1000)
        {
            int centena = (int)(numero / 100);
            long resto = numero % 100;
            
            if (numero == 100)
                return "CIEN";
            
            if (resto == 0)
                return _centenas[centena];
            else
                return _centenas[centena] + " " + ConvertirNumero(resto);
        }

        if (numero >= 1000 && numero < 1000000)
        {
            long miles = numero / 1000;
            long resto = numero % 1000;
            
            string textoMiles = miles == 1 ? "MIL" : ConvertirNumero(miles) + " MIL";
            
            if (resto == 0)
                return textoMiles;
            else
                return textoMiles + " " + ConvertirNumero(resto);
        }

        if (numero >= 1000000 && numero < 1000000000)
        {
            long millones = numero / 1000000;
            long resto = numero % 1000000;
            
            string textoMillones = millones == 1 ? "UN MILLÓN" : ConvertirNumero(millones) + " MILLONES";
            
            if (resto == 0)
                return textoMillones;
            else
                return textoMillones + " " + ConvertirNumero(resto);
        }

        if (numero >= 1000000000)
        {
            long billones = numero / 1000000000;
            long resto = numero % 1000000000;
            
            string textoBillones = billones == 1 ? "UN BILLÓN" : ConvertirNumero(billones) + " BILLONES";
            
            if (resto == 0)
                return textoBillones;
            else
                return textoBillones + " " + ConvertirNumero(resto);
        }

        return "";
    }
}
