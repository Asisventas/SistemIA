using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class ProductosSucursalNotNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Sucursal_suc",
                table: "Productos");

            // Asegurar que no queden valores nulos antes de cambiar a NOT NULL
            migrationBuilder.Sql(@"
                DECLARE @sid INT;
                SELECT TOP 1 @sid = Id FROM Sucursal ORDER BY Id;
                IF @sid IS NULL
                BEGIN
                    -- Si no hay sucursales, creamos una mínima para cumplir la FK
                    INSERT INTO Sucursal (NumSucursal, NombreSucursal, NombreEmpresa, Direccion, IdCiudad, RUC, DV)
                    VALUES ('0000001','Sucursal Principal','Empresa','Sin Direccion', 1, '00000000', 0);
                    SELECT TOP 1 @sid = Id FROM Sucursal ORDER BY Id;
                END
                UPDATE Productos SET suc = @sid WHERE suc IS NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "suc",
                table: "Productos",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Sucursal_suc",
                table: "Productos",
                column: "suc",
                principalTable: "Sucursal",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Sucursal_suc",
                table: "Productos");

            migrationBuilder.AlterColumn<int>(
                name: "suc",
                table: "Productos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Sucursal_suc",
                table: "Productos",
                column: "suc",
                principalTable: "Sucursal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
