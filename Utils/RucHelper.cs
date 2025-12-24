namespace SistemIA.Utils
{
    public static class RucHelper
    {
        /// <summary>
        /// Calcula el dígito verificador del RUC paraguayo según algoritmo SET oficial.
        /// Usa el mismo método que ProveedorSifenHelper.
        /// Ejemplos: 4637249 -> DV=0, 80033703 -> DV=4
        /// </summary>
        public static int CalcularDvRuc(string ruc)
        {
            if (string.IsNullOrWhiteSpace(ruc))
                return 0;

            // Limpiar el RUC - solo dígitos
            ruc = new string(ruc.Where(char.IsDigit).ToArray());
            
            if (string.IsNullOrEmpty(ruc))
                return 0;

            // Algoritmo de cálculo de DV para RUC paraguayo (versión oficial SET)
            // Mismo algoritmo que ProveedorSifenHelper
            int[] pesos = { 2, 3, 4, 5, 6, 7, 8, 9 };
            int suma = 0;

            // De derecha a izquierda, con pesos cíclicos
            int idx = 0;
            for (int pos = ruc.Length - 1; pos >= 0; pos--)
            {
                int digito = int.Parse(ruc[pos].ToString());
                suma += digito * pesos[idx % pesos.Length];
                idx++;
            }

            int resto = (suma * 10) % 11;
            int dv = resto;
            
            // Si el resultado es 10, el DV es 0
            if (dv == 10)
                dv = 0;

            return dv;
        }
    }
}