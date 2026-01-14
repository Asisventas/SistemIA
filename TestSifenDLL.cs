// Script para probar la DLL de SIFEN directamente
// Ejecutar con: dotnet script TestSifenDLL.cs

using System;
using System.IO;
using System.Reflection;

class TestSifenDLL
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== TEST DLL SIFEN ===");
        Console.WriteLine($"Fecha: {DateTime.Now}");
        
        try
        {
            // Cargar la DLL
            string dllPath = @"C:\asis\SistemIA\.ai-docs\SIFEN\Datos_nuevos_envios_dll_sifen\Sifen_Fuente\bin\Release2\Sifen.dll";
            Console.WriteLine($"Cargando DLL: {dllPath}");
            
            Assembly assembly = Assembly.LoadFrom(dllPath);
            Console.WriteLine($"Assembly cargado: {assembly.FullName}");
            
            // Buscar el tipo Sifen
            Type? sifenType = assembly.GetType("SIFEN.Sifen");
            if (sifenType == null)
            {
                // Listar todos los tipos
                Console.WriteLine("Tipos disponibles en el assembly:");
                foreach (var t in assembly.GetTypes())
                {
                    Console.WriteLine($"  - {t.FullName}");
                }
                return;
            }
            
            Console.WriteLine($"Tipo encontrado: {sifenType.FullName}");
            
            // Crear instancia
            object? sifenInstance = Activator.CreateInstance(sifenType);
            Console.WriteLine("Instancia creada");
            
            // Listar métodos públicos
            Console.WriteLine("\nMétodos públicos:");
            foreach (var method in sifenType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var parameters = string.Join(", ", Array.ConvertAll(method.GetParameters(), p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($"  - {method.ReturnType.Name} {method.Name}({parameters})");
            }
            
            // Leer el XML de prueba (el que generamos nosotros)
            string xmlPath = @"C:\asis\SistemIA\Debug\ultimo_273\compressed.txt";
            if (!File.Exists(xmlPath))
            {
                Console.WriteLine($"ERROR: No existe el archivo XML: {xmlPath}");
                return;
            }
            
            string xmlContent = File.ReadAllText(xmlPath);
            // Extraer solo el rDE (sin rLoteDE wrapper)
            int rdeStart = xmlContent.IndexOf("<rDE ");
            int rdeEnd = xmlContent.LastIndexOf("</rDE>") + "</rDE>".Length;
            if (rdeStart >= 0 && rdeEnd > rdeStart)
            {
                string rdeXml = xmlContent.Substring(rdeStart, rdeEnd - rdeStart);
                Console.WriteLine($"\nrDE extraído: {rdeXml.Length} caracteres");
                Console.WriteLine($"Primeros 200 chars: {rdeXml.Substring(0, Math.Min(200, rdeXml.Length))}...");
            }
            
            Console.WriteLine("\n=== FIN TEST ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
