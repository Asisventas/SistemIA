using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    // Neutralización de migración previamente generada que intentaba recrear el catálogo
    public partial class Drop_Legacy_Ciudades_Distritos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: evitar recrear tablas 'departamento', 'distrito', 'ciudad'
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op
        }
    }
}
