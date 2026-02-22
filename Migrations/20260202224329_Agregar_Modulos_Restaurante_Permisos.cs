using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Agregar_Modulos_Restaurante_Permisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar módulos del sistema de Restaurante/Mesas/Canchas al sistema de permisos
            migrationBuilder.Sql(@"
                DECLARE @IdModuloPadre INT;

                -- 1. Crear módulo padre 'Mesas / Canchas'
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE Nombre = 'Mesas / Canchas')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Mesas / Canchas', 'Gestión de mesas de restaurante y canchas deportivas', NULL, 'bi-grid-3x3-gap', NULL, 15, 1, GETDATE());
                    SET @IdModuloPadre = SCOPE_IDENTITY();
                END
                ELSE
                BEGIN
                    SELECT @IdModuloPadre = IdModulo FROM Modulos WHERE Nombre = 'Mesas / Canchas';
                END

                -- 2. Panel de Mesas
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/mesas/panel')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Panel de Mesas', 'Vista de panel visual de mesas y su estado', '/mesas/panel', 'bi-grid-3x3-gap-fill', @IdModuloPadre, 1, 1, GETDATE());
                    DECLARE @IdPanelMesas INT = SCOPE_IDENTITY();
                    INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                    VALUES (1, @IdPanelMesas, 1, 1, GETDATE()), (1, @IdPanelMesas, 2, 1, GETDATE()), (1, @IdPanelMesas, 3, 1, GETDATE()), (1, @IdPanelMesas, 4, 1, GETDATE());
                END

                -- 3. Administrar Mesas
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/mesas')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Administrar Mesas', 'CRUD de mesas y canchas', '/mesas', 'bi-table', @IdModuloPadre, 2, 1, GETDATE());
                    DECLARE @IdAdminMesas INT = SCOPE_IDENTITY();
                    INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                    VALUES (1, @IdAdminMesas, 1, 1, GETDATE()), (1, @IdAdminMesas, 2, 1, GETDATE()), (1, @IdAdminMesas, 3, 1, GETDATE()), (1, @IdAdminMesas, 4, 1, GETDATE());
                END

                -- 4. Pedido de Mesa
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/mesas/pedido')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Pedido de Mesa', 'Toma de pedidos en mesas', '/mesas/pedido', 'bi-receipt', @IdModuloPadre, 3, 1, GETDATE());
                    DECLARE @IdPedidoMesa INT = SCOPE_IDENTITY();
                    INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                    VALUES (1, @IdPedidoMesa, 1, 1, GETDATE()), (1, @IdPedidoMesa, 2, 1, GETDATE()), (1, @IdPedidoMesa, 3, 1, GETDATE()), (1, @IdPedidoMesa, 4, 1, GETDATE());
                END

                -- 5. Explorar Pedidos
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/pedidos/explorar')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Explorar Pedidos', 'Historial y búsqueda de pedidos', '/pedidos/explorar', 'bi-search', @IdModuloPadre, 4, 1, GETDATE());
                    DECLARE @IdExplorarPedidos INT = SCOPE_IDENTITY();
                    INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                    VALUES (1, @IdExplorarPedidos, 1, 1, GETDATE()), (1, @IdExplorarPedidos, 2, 1, GETDATE()), (1, @IdExplorarPedidos, 3, 1, GETDATE()), (1, @IdExplorarPedidos, 4, 1, GETDATE());
                END

                -- 6. Reservas
                IF NOT EXISTS (SELECT 1 FROM Modulos WHERE RutaPagina = '/reservas')
                BEGIN
                    INSERT INTO Modulos (Nombre, Descripcion, RutaPagina, Icono, IdModuloPadre, Orden, Activo, FechaCreacion)
                    VALUES ('Reservas', 'Gestión de reservas de mesas y canchas', '/reservas', 'bi-calendar-check', @IdModuloPadre, 5, 1, GETDATE());
                    DECLARE @IdReservas INT = SCOPE_IDENTITY();
                    INSERT INTO RolesModulosPermisos (IdRol, IdModulo, IdPermiso, Concedido, FechaAsignacion)
                    VALUES (1, @IdReservas, 1, 1, GETDATE()), (1, @IdReservas, 2, 1, GETDATE()), (1, @IdReservas, 3, 1, GETDATE()), (1, @IdReservas, 4, 1, GETDATE());
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @IdModuloPadre INT = (SELECT IdModulo FROM Modulos WHERE Nombre = 'Mesas / Canchas' AND IdModuloPadre IS NULL);
                IF @IdModuloPadre IS NOT NULL
                BEGIN
                    DELETE rmp FROM RolesModulosPermisos rmp INNER JOIN Modulos m ON rmp.IdModulo = m.IdModulo WHERE m.IdModuloPadre = @IdModuloPadre;
                    DELETE FROM RolesModulosPermisos WHERE IdModulo = @IdModuloPadre;
                    DELETE FROM Modulos WHERE IdModuloPadre = @IdModuloPadre;
                    DELETE FROM Modulos WHERE IdModulo = @IdModuloPadre;
                END
            ");
        }
    }
}
