using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Usuario_Correo_EmbeddingFacial_Opcionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hacer la columna Correo nullable
            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Usuarios",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            // Hacer la columna EmbeddingFacial nullable
            migrationBuilder.AlterColumn<byte[]>(
                name: "EmbeddingFacial",
                table: "Usuarios",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir Correo a NOT NULL (poner valor por defecto para registros existentes)
            migrationBuilder.Sql("UPDATE Usuarios SET Correo = '' WHERE Correo IS NULL");
            
            migrationBuilder.AlterColumn<string>(
                name: "Correo",
                table: "Usuarios",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            // Revertir EmbeddingFacial a NOT NULL
            migrationBuilder.Sql("UPDATE Usuarios SET EmbeddingFacial = 0x WHERE EmbeddingFacial IS NULL");
            
            migrationBuilder.AlterColumn<byte[]>(
                name: "EmbeddingFacial",
                table: "Usuarios",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);
        }
    }
}
