using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Modulos_Suscripciones_FacturasAutomaticas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar módulos de Suscripciones y Facturas Automáticas
            migrationBuilder.Sql(@"
                DECLARE @IdModuloPadre INT;
                
                -- Buscar el módulo padre 'Clientes'
                SELECT @IdModuloPadre = IdModulo FROM Modulos WHERE Nombre = 'Clientes' OR RutaPagina = '/clientes';
                
                -- Si no existe, buscar alternativa
                IF @IdModuloPadre IS NULL
                BEGIN
                    SELECT @IdModuloPadre = IdModulo FROM Modulos WHERE Nombre LIKE '%Cliente%' AND IdModuloPadre IS NULL;
                END

                -- Agregar módulo Suscripciones si no existe
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/suscripciones')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Suscripciones', 'Gestion de suscripciones y cuotas de clientes', '/suscripciones', 'bi-calendar-check', @IdModuloPadre, 10, 1, GETDATE());
                    
                    DECLARE @IdSuscripciones INT = SCOPE_IDENTITY();
                    
                    -- Asignar permisos al rol Administrador (Id_Rol = 1)
                    IF EXISTS (SELECT 1 FROM Rol WHERE Id_Rol = 1)
                    BEGIN
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (1, @IdSuscripciones, 1, 1, GETDATE()), -- VIEW
                               (1, @IdSuscripciones, 2, 1, GETDATE()), -- CREATE
                               (1, @IdSuscripciones, 3, 1, GETDATE()), -- EDIT
                               (1, @IdSuscripciones, 4, 1, GETDATE()); -- DELETE
                    END
                END

                -- Agregar módulo Monitor Facturas Automaticas si no existe
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/monitor-facturas-automaticas')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Facturas Automaticas', 'Monitor de facturacion automatica y recurrente', '/monitor-facturas-automaticas', 'bi-receipt', @IdModuloPadre, 11, 1, GETDATE());
                    
                    DECLARE @IdMonitorFacturas INT = SCOPE_IDENTITY();
                    
                    -- Asignar permisos al rol Administrador (Id_Rol = 1)
                    IF EXISTS (SELECT 1 FROM Rol WHERE Id_Rol = 1)
                    BEGIN
                        INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                        VALUES (1, @IdMonitorFacturas, 1, 1, GETDATE()), -- VIEW
                               (1, @IdMonitorFacturas, 2, 1, GETDATE()), -- CREATE
                               (1, @IdMonitorFacturas, 3, 1, GETDATE()), -- EDIT
                               (1, @IdMonitorFacturas, 4, 1, GETDATE()); -- DELETE
                    END
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Eliminar permisos de los módulos
                DELETE FROM RolesModulosPermisos WHERE IdModulo IN (
                    SELECT IdModulo FROM Modulos WHERE RutaPagina IN ('/suscripciones', '/monitor-facturas-automaticas')
                );
                
                -- Eliminar módulos
                DELETE FROM Modulos WHERE RutaPagina IN ('/suscripciones', '/monitor-facturas-automaticas');
            ");
        }
    }
}
