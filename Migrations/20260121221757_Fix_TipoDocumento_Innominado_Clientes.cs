using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Fix_TipoDocumento_Innominado_Clientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FIX 21-Ene-2026: Corregir datos de tipo de documento SIFEN para clientes CONSUMIDOR FINAL
            // 
            // Errores corregidos:
            // - Error 1303: "Tipo de contribuyente receptor inválido"
            //   Causa: Se enviaba iTiContRec para no contribuyentes (iNatRec=2)
            //   Fix: DEXmlBuilder.cs ahora solo agrega iTiContRec para contribuyentes (iNatRec=1)
            //
            // - Error 1313: "Descripción del tipo de documento de identidad del receptor no corresponde"
            //   Causa: iTipIDRec=5 estaba mapeado a "Cédula extranjera" cuando debía ser "Innominado"
            //   Fix: DEXmlBuilder.cs corregido con catálogo SIFEN v150 correcto
            //
            // Catálogo SIFEN v150 - Tipos de documento de identidad receptor (iTipIDRec):
            // 1 = Cédula paraguaya
            // 3 = Pasaporte
            // 4 = Carnet de residencia
            // 5 = Innominado (para Consumidor Final)
            // 9 = Sin documento
            //
            // Este script normaliza clientes CONSUMIDOR FINAL en la BD
            
            migrationBuilder.Sql(@"
                -- Actualizar clientes CONSUMIDOR FINAL con tipo documento correcto
                UPDATE Clientes
                SET TipoDocumentoIdentidadSifen = 5
                WHERE UPPER(RazonSocial) LIKE '%CONSUMIDOR%FINAL%'
                AND (TipoDocumentoIdentidadSifen IS NULL OR TipoDocumentoIdentidadSifen NOT IN (5, 9));
                
                -- Asegurar que CONSUMIDOR FINAL sea No Contribuyente (NaturalezaReceptor = 2)
                UPDATE Clientes
                SET NaturalezaReceptor = 2
                WHERE UPPER(RazonSocial) LIKE '%CONSUMIDOR%FINAL%'
                AND (NaturalezaReceptor IS NULL OR NaturalezaReceptor != 2);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No se puede revertir - los datos corregidos son consistentes con SIFEN v150
        }
    }
}
