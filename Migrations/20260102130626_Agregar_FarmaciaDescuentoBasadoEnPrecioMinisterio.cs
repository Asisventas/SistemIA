using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_FarmaciaDescuentoBasadoEnPrecioMinisterio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "FarmaciaDescuentoBasadoEnPrecioMinisterio",
                table: "ConfiguracionSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FarmaciaDescuentoBasadoEnPrecioMinisterio",
                table: "ConfiguracionSistema");
        }
    }
}
