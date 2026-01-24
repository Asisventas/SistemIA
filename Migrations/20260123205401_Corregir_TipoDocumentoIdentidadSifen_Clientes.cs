using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Migración para corregir el catálogo TipoDocumentoIdentidadSifen en clientes existentes.
    /// Catálogo SIFEN v150 correcto (iTipIDRec):
    ///   1 = Cédula paraguaya
    ///   2 = Cédula extranjera
    ///   3 = Pasaporte
    ///   4 = Carnet de residencia
    ///   5 = Innominado
    ///   9 = Sin documento
    /// </summary>
    public partial class Corregir_TipoDocumentoIdentidadSifen_Clientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ═══════════════════════════════════════════════════════════════════
            // CORRECCIÓN DE TipoDocumentoIdentidadSifen según catálogo SIFEN v150
            // ═══════════════════════════════════════════════════════════════════

            // 1. Para clientes con TipoDocumento = 'CI' (Cédula de Identidad paraguaya)
            // Si TipoDocumentoIdentidadSifen es NULL o diferente de 1, corregir a 1
            migrationBuilder.Sql(@"
                UPDATE Clientes 
                SET TipoDocumentoIdentidadSifen = 1
                WHERE TipoDocumento = 'CI' 
                  AND (TipoDocumentoIdentidadSifen IS NULL OR TipoDocumentoIdentidadSifen <> 1)
            ");

            // 2. Para clientes CONSUMIDOR FINAL o similares: usar 5 (Innominado)
            migrationBuilder.Sql(@"
                UPDATE Clientes 
                SET TipoDocumentoIdentidadSifen = 5,
                    NaturalezaReceptor = 2
                WHERE UPPER(RazonSocial) LIKE '%CONSUMIDOR FINAL%'
                   OR UPPER(RazonSocial) LIKE '%SIN NOMBRE%'
            ");

            // 3. Para clientes que son contribuyentes (tienen RUC válido), asegurar NaturalezaReceptor = 1
            // y TipoDocumentoIdentidadSifen = NULL (contribuyentes no necesitan iTipIDRec)
            migrationBuilder.Sql(@"
                UPDATE Clientes 
                SET NaturalezaReceptor = 1,
                    TipoDocumentoIdentidadSifen = NULL
                WHERE RUC IS NOT NULL 
                  AND LEN(RUC) >= 5 
                  AND RUC <> '0'
                  AND NaturalezaReceptor <> 2
            ");

            // 4. Para clientes NO contribuyentes con NumeroDocumentoIdentidad pero sin TipoDocumentoIdentidadSifen
            // Asumir Cédula paraguaya (1) si el número parece CI paraguaya
            migrationBuilder.Sql(@"
                UPDATE Clientes 
                SET TipoDocumentoIdentidadSifen = 1
                WHERE NaturalezaReceptor = 2
                  AND NumeroDocumentoIdentidad IS NOT NULL 
                  AND LEN(NumeroDocumentoIdentidad) >= 5
                  AND TipoDocumentoIdentidadSifen IS NULL
            ");

            // 5. Corregir valores que estaban con catálogo invertido (2=Pasaporte → ahora 2=CI extranjera)
            // Si TipoDocumento era 'PA' (pasaporte) pero TipoDocumentoIdentidadSifen=2, corregir a 3
            migrationBuilder.Sql(@"
                UPDATE Clientes 
                SET TipoDocumentoIdentidadSifen = 3
                WHERE TipoDocumento = 'PA' 
                  AND TipoDocumentoIdentidadSifen = 2
            ");

            // 6. Si TipoDocumento era 'CEE' o 'CE' (cédula extranjera) y TipoDocumentoIdentidadSifen=3, corregir a 2
            migrationBuilder.Sql(@"
                UPDATE Clientes 
                SET TipoDocumentoIdentidadSifen = 2
                WHERE (TipoDocumento = 'CEE' OR TipoDocumento = 'CE')
                  AND TipoDocumentoIdentidadSifen = 3
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No es posible revertir con exactitud los valores originales
            // Se deja vacío intencionalmente
        }
    }
}
