using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Campos_Comandas_Caja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ComandaBarraEsRed",
                table: "Cajas",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ComandaCocinaEsRed",
                table: "Cajas",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CopiasComandaBarra",
                table: "Cajas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CopiasComandaCocina",
                table: "Cajas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImpresoraComandaBarra",
                table: "Cajas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImpresoraComandaCocina",
                table: "Cajas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ImprimirComandaAutomatica",
                table: "Cajas",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PuertoImpresoraBarra",
                table: "Cajas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PuertoImpresoraCocina",
                table: "Cajas",
                type: "int",
                nullable: true);

            // ========== AGREGAR MÓDULO PANTALLA COCINA AL SISTEMA DE PERMISOS ==========
            migrationBuilder.Sql(@"
                DECLARE @IdModuloPadre INT;
                
                -- Obtener el ID del módulo padre 'Mesas / Canchas'
                SELECT @IdModuloPadre = IdModulo FROM Modulos WHERE Nombre = 'Mesas / Canchas';
                
                -- Si no existe el módulo padre, crearlo
                IF @IdModuloPadre IS NULL
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Mesas / Canchas', 'Gestión de mesas de restaurante y canchas deportivas', NULL, 'bi-grid-3x3-gap', NULL, 15, 1, GETDATE());
                    SET @IdModuloPadre = SCOPE_IDENTITY();
                END

                -- Agregar módulo Pantalla Cocina si no existe
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/cocina')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Pantalla Cocina', 'Pantalla de visualización para cocina (KDS)', '/cocina', 'bi-fire', @IdModuloPadre, 5, 1, GETDATE());
                    
                    DECLARE @IdPantallaCocina INT = SCOPE_IDENTITY();
                    
                    -- Asignar permisos al rol Administrador (Id_Rol = 1) - Tabla se llama Rol, columna es Id_Rol
                    IF EXISTS (SELECT 1 FROM Rol WHERE Id_Rol = 1)
                    BEGIN
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (1, @IdPantallaCocina, 1, 1, GETDATE()), -- VIEW
                               (1, @IdPantallaCocina, 2, 1, GETDATE()), -- CREATE
                               (1, @IdPantallaCocina, 3, 1, GETDATE()), -- EDIT
                               (1, @IdPantallaCocina, 4, 1, GETDATE()); -- DELETE
                    END
                    
                    PRINT 'Módulo Pantalla Cocina creado con permisos';
                END
                ELSE
                BEGIN
                    PRINT 'Módulo Pantalla Cocina ya existe';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComandaBarraEsRed",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "ComandaCocinaEsRed",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "CopiasComandaBarra",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "CopiasComandaCocina",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "ImpresoraComandaBarra",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "ImpresoraComandaCocina",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "ImprimirComandaAutomatica",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "PuertoImpresoraBarra",
                table: "Cajas");

            migrationBuilder.DropColumn(
                name: "PuertoImpresoraCocina",
                table: "Cajas");
        }
    }
}
