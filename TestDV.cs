// Archivo temporal para probar el cálculo de DV
// Ejecutar con: dotnet run -- test-dv
// Eliminar después de verificar

namespace SistemIA.Utils
{
    public static class TestDV
    {
        public static void Probar()
        {
            var casos = new[] {
                ("4637249", 0),
                ("80033703", 4),
                ("80000110", 8),
                ("1234567", 3),
            };

            Console.WriteLine("=== Prueba de cálculo DV ===");
            foreach (var (ruc, esperado) in casos)
            {
                var resultado = RucHelper.CalcularDvRuc(ruc);
                var estado = resultado == esperado ? "✓" : "✗";
                Console.WriteLine($"{estado} RUC: {ruc} -> DV calculado: {resultado}, esperado: {esperado}");
            }
        }
    }
}
