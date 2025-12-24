using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SistemIA.Utils
{
    public static class CatalogoVerifUtil
    {
        public static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var s = input.ToUpperInvariant().Trim();
          // Normalizar abreviaturas comunes en catálogos oficiales SIFEN/INE
          s = s.Replace("PTE.", "PRESIDENTE ")
              .Replace("PTE ", "PRESIDENTE ")
              .Replace("GRAL.", "GENERAL ")
              .Replace("GRAL ", "GENERAL ")
              .Replace("CNEL.", "CORONEL ")
              .Replace("CNEL ", "CORONEL ")
              .Replace("MCAL.", "MARISCAL ")
              .Replace("MCAL ", "MARISCAL ")
              .Replace("DR.", "DOCTOR ")
              .Replace("DR ", "DOCTOR ")
              .Replace("STA.", "SANTA ")
              .Replace("STA ", "SANTA ")
              .Replace("STO.", "SANTO ")
              .Replace("STO ", "SANTO ")
              .Replace("D. ", "DON ");
            s = Regex.Replace(s, "\\s*\\(.*?\\)", string.Empty); // quitar paréntesis y contenido
            s = Regex.Replace(s, "\\s+", " "); // normalizar espacios
            s = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in s)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }

    public sealed class VerificacionResultado
    {
        public int TotalDepartamentosCsv { get; set; }
        public int TotalDepartamentosBd { get; set; }
        public int TotalDistritosCsv { get; set; }
        public int TotalDistritosBd { get; set; }
        public int TotalCiudadesCsv { get; set; }
        public int TotalCiudadesBd { get; set; }

        public List<DepartamentoCodigoDiff> DepartamentosCodigoDifiere { get; } = new();
        public List<string> DepartamentosFaltantesEnBd { get; } = new();
        public List<string> DepartamentosSobrantesEnBd { get; } = new();

        public Dictionary<string, List<string>> DistritosFaltantes { get; } = new();
        public Dictionary<string, List<string>> DistritosSobrantes { get; } = new();

        public List<DistritoFix> DistritosMalAsociados { get; } = new();
        public List<CiudadFix> CiudadesMalAsociadas { get; } = new();

        [JsonIgnore]
        public Dictionary<string, List<ParDistritoCiudad>> CiudadesFaltantes { get; } = new();

        public List<string> SqlUpdates { get; } = new();
    }

    public readonly record struct DepartamentoCodigoDiff(string Nombre, int CodigoCsv, int CodigoBd);

    public sealed class DistritoFix
    {
        public int Numero { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int DepartamentoActual { get; set; }
        public int DepartamentoCorrecto { get; set; }
        public string Sql { get; set; } = string.Empty;
    }

    public sealed class CiudadFix
    {
        public int Numero { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int DepartamentoActual { get; set; }
        public int DistritoActual { get; set; }
        public int DepartamentoCorrecto { get; set; }
        public int DistritoCorrecto { get; set; }
        public string Sql { get; set; } = string.Empty;
    }

    public sealed record CsvRow(int cDep, string dDesDep, string dDesDis, string dDesCiu);

    public sealed class ParDistritoCiudad
    {
        public string Distrito { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
    }
}
