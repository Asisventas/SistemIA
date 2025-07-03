namespace SistemIA.Utils
{
    public static class RucHelper
    {
        /// <summary>
        /// Calcula el dígito verificador del RUC paraguayo (versión oficial SET, pesos 2-9, suma*10 % 11).
        /// </summary>
        public static int CalcularDvRuc(string ruc)
        {
            if (string.IsNullOrWhiteSpace(ruc) || !ruc.All(char.IsDigit))
                return 0;

            int[] pesos = { 2, 3, 4, 5, 6, 7, 8, 9, 2, 3, 4, 5, 6, 7, 8, 9 };
            if (ruc.Length > 16)
                throw new Exception("El RUC no soporta más de 16 dígitos.");

            int suma = 0;
            int idx = 0;

            // De derecha a izquierda
            for (int pos = ruc.Length - 1; pos >= 0; pos--)
            {
                int digito = int.Parse(ruc[pos].ToString());
                suma += digito * pesos[idx];
                idx++;
            }

            int resto = (suma * 10) % 11;
            int dv = resto;
            if (dv == 10)
                dv = 0;

            return dv;
        }
    }
}