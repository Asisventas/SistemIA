// Ejemplo de uso de los nuevos métodos descriptivos del ClienteSifenMejorado
// Este archivo muestra cómo usar las funcionalidades mejoradas de documentación SIFEN

using SistemIA.Models;

namespace SistemIA.Examples
{
    public class EjemploClienteSifen
    {
        public void MostrarEjemplosDeUso()
        {
            // Crear un cliente de ejemplo
            var cliente = new ClienteSifenMejorado
            {
                IdCliente = 1,
                RazonSocial = "Empresa Ejemplo S.A.",
                RUC = "12345678",
                DV = 9,
                NaturalezaReceptor = 1, // Contribuyente
                TipoOperacion = "2", // B2C
                CodigoPais = "PRY",
                CodigoDepartamento = "01",
                DescripcionDepartamento = "CAPITAL",
                CodigoDistrito = "0001", 
                DescripcionDistrito = "ASUNCION",
                NombreFantasia = "EmpresaEjemplo",
                IdTipoContribuyente = 1
            };

            // Usar los nuevos métodos descriptivos
            Console.WriteLine("=== INFORMACIÓN DESCRIPTIVA DEL CLIENTE ===");
            Console.WriteLine($"Naturaleza: {cliente.ObtenerDescripcionNaturaleza()}");
            Console.WriteLine($"Tipo de Operación: {cliente.ObtenerDescripcionTipoOperacion()}");
            Console.WriteLine($"Código SIFEN Naturaleza: {cliente.ObtenerNaturalezaSifen()}");
            Console.WriteLine($"Código SIFEN Operación: {cliente.ObtenerTipoOperacionSifen()}");
            
            // Validar cliente para SIFEN
            var (esValido, errores) = cliente.ValidarParaSifen();
            Console.WriteLine($"\n¿Es válido para SIFEN?: {(esValido ? "SÍ" : "NO")}");
            
            if (!esValido)
            {
                Console.WriteLine("Errores encontrados:");
                foreach (var error in errores)
                {
                    Console.WriteLine(error);
                }
            }

            // Generar resumen completo
            Console.WriteLine("\n" + cliente.GenerarResumenSifen());

            // Ejemplo con no contribuyente
            var clienteNoContribuyente = new ClienteSifenMejorado
            {
                IdCliente = 2,
                RazonSocial = "Juan Pérez",
                NaturalezaReceptor = 2, // No contribuyente
                TipoOperacion = "2", // B2C
                CodigoPais = "PRY",
                TipoDocumentoIdentidadSifen = 1, // Cédula paraguaya
                NumeroDocumentoIdentidad = "1234567"
            };

            Console.WriteLine("\n=== CLIENTE NO CONTRIBUYENTE ===");
            Console.WriteLine($"Tipo de Documento: {clienteNoContribuyente.ObtenerDescripcionTipoDocumento()}");
            Console.WriteLine(clienteNoContribuyente.GenerarResumenSifen());
        }
    }
}

/*
SALIDA ESPERADA:

=== INFORMACIÓN DESCRIPTIVA DEL CLIENTE ===
Naturaleza: Contribuyente - Persona con RUC activo
Tipo de Operación: B2C - Empresa a Consumidor Final
Código SIFEN Naturaleza: 1
Código SIFEN Operación: 2

¿Es válido para SIFEN?: SÍ

=== RESUMEN CLIENTE SIFEN ===
Cliente: Empresa Ejemplo S.A.
Naturaleza: Contribuyente - Persona con RUC activo
RUC: 12345678-9
Tipo de Operación: B2C - Empresa a Consumidor Final
País: PRY
Nombre de Fantasía: EmpresaEjemplo
Ubicación: CAPITAL - ASUNCION
Estado SIFEN: ✅ VÁLIDO

=== CLIENTE NO CONTRIBUYENTE ===
Tipo de Documento: Cédula de Identidad Paraguaya
=== RESUMEN CLIENTE SIFEN ===
Cliente: Juan Pérez
Naturaleza: No contribuyente - Consumidor final sin RUC
Documento: Cédula de Identidad Paraguaya - 1234567
Tipo de Operación: B2C - Empresa a Consumidor Final
País: PRY
Estado SIFEN: ✅ VÁLIDO
*/
