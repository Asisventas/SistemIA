using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemIA.Migrations
{
    /// <inheritdoc />
    public partial class Fix_ClientePrecio_FKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hacer esta migración idempotente: eliminar FKs/Índices/Columnas solo si existen
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Presupuestos_Clientes_ClienteIdCliente')
    ALTER TABLE [dbo].[Presupuestos] DROP CONSTRAINT [FK_Presupuestos_Clientes_ClienteIdCliente];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Presupuestos_Monedas_MonedaIdMoneda')
    ALTER TABLE [dbo].[Presupuestos] DROP CONSTRAINT [FK_Presupuestos_Monedas_MonedaIdMoneda];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Presupuestos_Sucursal_SucursalId')
    ALTER TABLE [dbo].[Presupuestos] DROP CONSTRAINT [FK_Presupuestos_Sucursal_SucursalId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PresupuestosDetalles_Presupuestos_PresupuestoIdPresupuesto')
    ALTER TABLE [dbo].[PresupuestosDetalles] DROP CONSTRAINT [FK_PresupuestosDetalles_Presupuestos_PresupuestoIdPresupuesto];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PresupuestosDetalles_Productos_ProductoIdProducto')
    ALTER TABLE [dbo].[PresupuestosDetalles] DROP CONSTRAINT [FK_PresupuestosDetalles_Productos_ProductoIdProducto];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Cajas_CajaIdCaja')
    ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_Cajas_CajaIdCaja];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Clientes_ClienteIdCliente')
    ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_Clientes_ClienteIdCliente];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Monedas_MonedaIdMoneda')
    ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_Monedas_MonedaIdMoneda];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Sucursal_SucursalId')
    ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_Sucursal_SucursalId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_TiposDocumentoOperacion_TipoDocumentoOperacionIdTipoDocumentoOperacion')
    ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_TiposDocumentoOperacion_TipoDocumentoOperacionIdTipoDocumentoOperacion];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_TiposPago_TipoPagoIdTipoPago')
    ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_TiposPago_TipoPagoIdTipoPago];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Usuarios_UsuarioId_Usu')
    ALTER TABLE [dbo].[Ventas] DROP CONSTRAINT [FK_Ventas_Usuarios_UsuarioId_Usu];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_VentasDetalles_Productos_ProductoIdProducto')
    ALTER TABLE [dbo].[VentasDetalles] DROP CONSTRAINT [FK_VentasDetalles_Productos_ProductoIdProducto];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_VentasDetalles_Ventas_VentaIdVenta')
    ALTER TABLE [dbo].[VentasDetalles] DROP CONSTRAINT [FK_VentasDetalles_Ventas_VentaIdVenta];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VentasDetalles_ProductoIdProducto' AND object_id = OBJECT_ID(N'[dbo].[VentasDetalles]'))
    DROP INDEX [IX_VentasDetalles_ProductoIdProducto] ON [dbo].[VentasDetalles];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VentasDetalles_VentaIdVenta' AND object_id = OBJECT_ID(N'[dbo].[VentasDetalles]'))
    DROP INDEX [IX_VentasDetalles_VentaIdVenta] ON [dbo].[VentasDetalles];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_CajaIdCaja' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    DROP INDEX [IX_Ventas_CajaIdCaja] ON [dbo].[Ventas];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_ClienteIdCliente' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    DROP INDEX [IX_Ventas_ClienteIdCliente] ON [dbo].[Ventas];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_MonedaIdMoneda' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    DROP INDEX [IX_Ventas_MonedaIdMoneda] ON [dbo].[Ventas];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_SucursalId' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    DROP INDEX [IX_Ventas_SucursalId] ON [dbo].[Ventas];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_TipoDocumentoOperacionIdTipoDocumentoOperacion' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    DROP INDEX [IX_Ventas_TipoDocumentoOperacionIdTipoDocumentoOperacion] ON [dbo].[Ventas];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_TipoPagoIdTipoPago' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    DROP INDEX [IX_Ventas_TipoPagoIdTipoPago] ON [dbo].[Ventas];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_UsuarioId_Usu' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    DROP INDEX [IX_Ventas_UsuarioId_Usu] ON [dbo].[Ventas];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PresupuestosDetalles_PresupuestoIdPresupuesto' AND object_id = OBJECT_ID(N'[dbo].[PresupuestosDetalles]'))
    DROP INDEX [IX_PresupuestosDetalles_PresupuestoIdPresupuesto] ON [dbo].[PresupuestosDetalles];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PresupuestosDetalles_ProductoIdProducto' AND object_id = OBJECT_ID(N'[dbo].[PresupuestosDetalles]'))
    DROP INDEX [IX_PresupuestosDetalles_ProductoIdProducto] ON [dbo].[PresupuestosDetalles];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Presupuestos_ClienteIdCliente' AND object_id = OBJECT_ID(N'[dbo].[Presupuestos]'))
    DROP INDEX [IX_Presupuestos_ClienteIdCliente] ON [dbo].[Presupuestos];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Presupuestos_MonedaIdMoneda' AND object_id = OBJECT_ID(N'[dbo].[Presupuestos]'))
    DROP INDEX [IX_Presupuestos_MonedaIdMoneda] ON [dbo].[Presupuestos];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Presupuestos_SucursalId' AND object_id = OBJECT_ID(N'[dbo].[Presupuestos]'))
    DROP INDEX [IX_Presupuestos_SucursalId] ON [dbo].[Presupuestos];

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ProductoIdProducto' AND Object_ID = Object_ID(N'[dbo].[VentasDetalles]'))
    ALTER TABLE [dbo].[VentasDetalles] DROP COLUMN [ProductoIdProducto];
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'VentaIdVenta' AND Object_ID = Object_ID(N'[dbo].[VentasDetalles]'))
    ALTER TABLE [dbo].[VentasDetalles] DROP COLUMN [VentaIdVenta];

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CajaIdCaja' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
    ALTER TABLE [dbo].[Ventas] DROP COLUMN [CajaIdCaja];
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ClienteIdCliente' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
    ALTER TABLE [dbo].[Ventas] DROP COLUMN [ClienteIdCliente];
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'MonedaIdMoneda' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
    ALTER TABLE [dbo].[Ventas] DROP COLUMN [MonedaIdMoneda];
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'SucursalId' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
    ALTER TABLE [dbo].[Ventas] DROP COLUMN [SucursalId];
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TipoDocumentoOperacionIdTipoDocumentoOperacion' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
    ALTER TABLE [dbo].[Ventas] DROP COLUMN [TipoDocumentoOperacionIdTipoDocumentoOperacion];
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TipoPagoIdTipoPago' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
    ALTER TABLE [dbo].[Ventas] DROP COLUMN [TipoPagoIdTipoPago];
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'UsuarioId_Usu' AND Object_ID = Object_ID(N'[dbo].[Ventas]'))
    ALTER TABLE [dbo].[Ventas] DROP COLUMN [UsuarioId_Usu];

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'PresupuestoIdPresupuesto' AND Object_ID = Object_ID(N'[dbo].[PresupuestosDetalles]'))
    ALTER TABLE [dbo].[PresupuestosDetalles] DROP COLUMN [PresupuestoIdPresupuesto];
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ProductoIdProducto' AND Object_ID = Object_ID(N'[dbo].[PresupuestosDetalles]'))
    ALTER TABLE [dbo].[PresupuestosDetalles] DROP COLUMN [ProductoIdProducto];

IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ClienteIdCliente' AND Object_ID = Object_ID(N'[dbo].[Presupuestos]'))
    ALTER TABLE [dbo].[Presupuestos] DROP COLUMN [ClienteIdCliente];
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'MonedaIdMoneda' AND Object_ID = Object_ID(N'[dbo].[Presupuestos]'))
    ALTER TABLE [dbo].[Presupuestos] DROP COLUMN [MonedaIdMoneda];
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'SucursalId' AND Object_ID = Object_ID(N'[dbo].[Presupuestos]'))
    ALTER TABLE [dbo].[Presupuestos] DROP COLUMN [SucursalId];

-- Renombrar IdSucursal -> suc solo si corresponde
IF COL_LENGTH('dbo.Ventas','suc') IS NULL AND COL_LENGTH('dbo.Ventas','IdSucursal') IS NOT NULL
    EXEC sp_rename 'dbo.Ventas.IdSucursal','suc','COLUMN';
IF COL_LENGTH('dbo.Presupuestos','suc') IS NULL AND COL_LENGTH('dbo.Presupuestos','IdSucursal') IS NOT NULL
    EXEC sp_rename 'dbo.Presupuestos.IdSucursal','suc','COLUMN';

");
            // A partir de aquí, continuar con ajustes de tipos/índices/relaciones

            migrationBuilder.AlterColumn<string>(
                name: "PuntoExpedicion",
                table: "Ventas",
                type: "nchar(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Establecimiento",
                table: "Ventas",
                type: "nchar(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            // Crear índices y FKs solo si no existen
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VentasDetalles_IdProducto' AND object_id = OBJECT_ID(N'[dbo].[VentasDetalles]'))
    CREATE INDEX [IX_VentasDetalles_IdProducto] ON [dbo].[VentasDetalles]([IdProducto]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VentasDetalles_IdVenta' AND object_id = OBJECT_ID(N'[dbo].[VentasDetalles]'))
    CREATE INDEX [IX_VentasDetalles_IdVenta] ON [dbo].[VentasDetalles]([IdVenta]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_IdCaja' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    CREATE INDEX [IX_Ventas_IdCaja] ON [dbo].[Ventas]([IdCaja]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_IdCliente' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    CREATE INDEX [IX_Ventas_IdCliente] ON [dbo].[Ventas]([IdCliente]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_IdMoneda' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    CREATE INDEX [IX_Ventas_IdMoneda] ON [dbo].[Ventas]([IdMoneda]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_IdTipoDocumentoOperacion' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    CREATE INDEX [IX_Ventas_IdTipoDocumentoOperacion] ON [dbo].[Ventas]([IdTipoDocumentoOperacion]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_IdTipoPago' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    CREATE INDEX [IX_Ventas_IdTipoPago] ON [dbo].[Ventas]([IdTipoPago]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_IdUsuario' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    CREATE INDEX [IX_Ventas_IdUsuario] ON [dbo].[Ventas]([IdUsuario]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Ventas_suc' AND object_id = OBJECT_ID(N'[dbo].[Ventas]'))
    CREATE INDEX [IX_Ventas_suc] ON [dbo].[Ventas]([suc]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PresupuestosDetalles_IdPresupuesto' AND object_id = OBJECT_ID(N'[dbo].[PresupuestosDetalles]'))
    CREATE INDEX [IX_PresupuestosDetalles_IdPresupuesto] ON [dbo].[PresupuestosDetalles]([IdPresupuesto]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PresupuestosDetalles_IdProducto' AND object_id = OBJECT_ID(N'[dbo].[PresupuestosDetalles]'))
    CREATE INDEX [IX_PresupuestosDetalles_IdProducto] ON [dbo].[PresupuestosDetalles]([IdProducto]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Presupuestos_IdCliente' AND object_id = OBJECT_ID(N'[dbo].[Presupuestos]'))
    CREATE INDEX [IX_Presupuestos_IdCliente] ON [dbo].[Presupuestos]([IdCliente]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Presupuestos_IdMoneda' AND object_id = OBJECT_ID(N'[dbo].[Presupuestos]'))
    CREATE INDEX [IX_Presupuestos_IdMoneda] ON [dbo].[Presupuestos]([IdMoneda]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Presupuestos_suc_NumeroPresupuesto' AND object_id = OBJECT_ID(N'[dbo].[Presupuestos]'))
    CREATE INDEX [IX_Presupuestos_suc_NumeroPresupuesto] ON [dbo].[Presupuestos]([suc],[NumeroPresupuesto]);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Presupuestos_Clientes_IdCliente')
BEGIN
    ALTER TABLE [dbo].[Presupuestos]  WITH CHECK ADD  CONSTRAINT [FK_Presupuestos_Clientes_IdCliente] FOREIGN KEY([IdCliente]) REFERENCES [dbo].[Clientes]([IdCliente]) ON DELETE NO ACTION;
    ALTER TABLE [dbo].[Presupuestos] CHECK CONSTRAINT [FK_Presupuestos_Clientes_IdCliente];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Presupuestos_Monedas_IdMoneda')
BEGIN
    ALTER TABLE [dbo].[Presupuestos]  WITH CHECK ADD  CONSTRAINT [FK_Presupuestos_Monedas_IdMoneda] FOREIGN KEY([IdMoneda]) REFERENCES [dbo].[Monedas]([IdMoneda]) ON DELETE NO ACTION;
    ALTER TABLE [dbo].[Presupuestos] CHECK CONSTRAINT [FK_Presupuestos_Monedas_IdMoneda];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Presupuestos_Sucursal_suc')
BEGIN
    ALTER TABLE [dbo].[Presupuestos]  WITH CHECK ADD  CONSTRAINT [FK_Presupuestos_Sucursal_suc] FOREIGN KEY([suc]) REFERENCES [dbo].[Sucursal]([Id]) ON DELETE NO ACTION;
    ALTER TABLE [dbo].[Presupuestos] CHECK CONSTRAINT [FK_Presupuestos_Sucursal_suc];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PresupuestosDetalles_Presupuestos_IdPresupuesto')
BEGIN
    ALTER TABLE [dbo].[PresupuestosDetalles]  WITH CHECK ADD  CONSTRAINT [FK_PresupuestosDetalles_Presupuestos_IdPresupuesto] FOREIGN KEY([IdPresupuesto]) REFERENCES [dbo].[Presupuestos]([IdPresupuesto]) ON DELETE CASCADE;
    ALTER TABLE [dbo].[PresupuestosDetalles] CHECK CONSTRAINT [FK_PresupuestosDetalles_Presupuestos_IdPresupuesto];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PresupuestosDetalles_Productos_IdProducto')
BEGIN
    ALTER TABLE [dbo].[PresupuestosDetalles]  WITH CHECK ADD  CONSTRAINT [FK_PresupuestosDetalles_Productos_IdProducto] FOREIGN KEY([IdProducto]) REFERENCES [dbo].[Productos]([IdProducto]) ON DELETE NO ACTION;
    ALTER TABLE [dbo].[PresupuestosDetalles] CHECK CONSTRAINT [FK_PresupuestosDetalles_Productos_IdProducto];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Cajas_IdCaja')
BEGIN
    ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Cajas_IdCaja] FOREIGN KEY([IdCaja]) REFERENCES [dbo].[Cajas]([id_caja]) ON DELETE NO ACTION;
    ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Cajas_IdCaja];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Clientes_IdCliente')
BEGIN
    ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Clientes_IdCliente] FOREIGN KEY([IdCliente]) REFERENCES [dbo].[Clientes]([IdCliente]) ON DELETE NO ACTION;
    ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Clientes_IdCliente];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Monedas_IdMoneda')
BEGIN
    ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Monedas_IdMoneda] FOREIGN KEY([IdMoneda]) REFERENCES [dbo].[Monedas]([IdMoneda]) ON DELETE NO ACTION;
    ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Monedas_IdMoneda];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Sucursal_suc')
BEGIN
    ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Sucursal_suc] FOREIGN KEY([suc]) REFERENCES [dbo].[Sucursal]([Id]) ON DELETE NO ACTION;
    ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Sucursal_suc];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_TiposDocumentoOperacion_IdTipoDocumentoOperacion')
BEGIN
    ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_TiposDocumentoOperacion_IdTipoDocumentoOperacion] FOREIGN KEY([IdTipoDocumentoOperacion]) REFERENCES [dbo].[TiposDocumentoOperacion]([IdTipoDocumentoOperacion]) ON DELETE NO ACTION;
    ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_TiposDocumentoOperacion_IdTipoDocumentoOperacion];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_TiposPago_IdTipoPago')
BEGIN
    ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_TiposPago_IdTipoPago] FOREIGN KEY([IdTipoPago]) REFERENCES [dbo].[TiposPago]([IdTipoPago]) ON DELETE NO ACTION;
    ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_TiposPago_IdTipoPago];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Ventas_Usuarios_IdUsuario')
BEGIN
    ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Usuarios_IdUsuario] FOREIGN KEY([IdUsuario]) REFERENCES [dbo].[Usuarios]([Id_Usu]) ON DELETE NO ACTION;
    ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Usuarios_IdUsuario];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_VentasDetalles_Productos_IdProducto')
BEGIN
    ALTER TABLE [dbo].[VentasDetalles]  WITH CHECK ADD  CONSTRAINT [FK_VentasDetalles_Productos_IdProducto] FOREIGN KEY([IdProducto]) REFERENCES [dbo].[Productos]([IdProducto]) ON DELETE NO ACTION;
    ALTER TABLE [dbo].[VentasDetalles] CHECK CONSTRAINT [FK_VentasDetalles_Productos_IdProducto];
END
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_VentasDetalles_Ventas_IdVenta')
BEGIN
    ALTER TABLE [dbo].[VentasDetalles]  WITH CHECK ADD  CONSTRAINT [FK_VentasDetalles_Ventas_IdVenta] FOREIGN KEY([IdVenta]) REFERENCES [dbo].[Ventas]([IdVenta]) ON DELETE CASCADE;
    ALTER TABLE [dbo].[VentasDetalles] CHECK CONSTRAINT [FK_VentasDetalles_Ventas_IdVenta];
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Clientes_IdCliente",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Monedas_IdMoneda",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Sucursal_suc",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_PresupuestosDetalles_Presupuestos_IdPresupuesto",
                table: "PresupuestosDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_PresupuestosDetalles_Productos_IdProducto",
                table: "PresupuestosDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Cajas_IdCaja",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Clientes_IdCliente",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Monedas_IdMoneda",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Sucursal_suc",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_TiposDocumentoOperacion_IdTipoDocumentoOperacion",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_TiposPago_IdTipoPago",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Usuarios_IdUsuario",
                table: "Ventas");

            migrationBuilder.DropForeignKey(
                name: "FK_VentasDetalles_Productos_IdProducto",
                table: "VentasDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_VentasDetalles_Ventas_IdVenta",
                table: "VentasDetalles");

            migrationBuilder.DropIndex(
                name: "IX_VentasDetalles_IdProducto",
                table: "VentasDetalles");

            migrationBuilder.DropIndex(
                name: "IX_VentasDetalles_IdVenta",
                table: "VentasDetalles");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_IdCaja",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_IdCliente",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_IdMoneda",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_IdTipoDocumentoOperacion",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_IdTipoPago",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_IdUsuario",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_suc",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_PresupuestosDetalles_IdPresupuesto",
                table: "PresupuestosDetalles");

            migrationBuilder.DropIndex(
                name: "IX_PresupuestosDetalles_IdProducto",
                table: "PresupuestosDetalles");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_IdCliente",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_IdMoneda",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_suc_NumeroPresupuesto",
                table: "Presupuestos");

            migrationBuilder.RenameColumn(
                name: "suc",
                table: "Ventas",
                newName: "IdSucursal");

            migrationBuilder.RenameColumn(
                name: "suc",
                table: "Presupuestos",
                newName: "IdSucursal");

            migrationBuilder.AddColumn<int>(
                name: "ProductoIdProducto",
                table: "VentasDetalles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VentaIdVenta",
                table: "VentasDetalles",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PuntoExpedicion",
                table: "Ventas",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nchar(3)",
                oldFixedLength: true,
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Establecimiento",
                table: "Ventas",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nchar(3)",
                oldFixedLength: true,
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CajaIdCaja",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClienteIdCliente",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MonedaIdMoneda",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoDocumentoOperacionIdTipoDocumentoOperacion",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoPagoIdTipoPago",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId_Usu",
                table: "Ventas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PresupuestoIdPresupuesto",
                table: "PresupuestosDetalles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductoIdProducto",
                table: "PresupuestosDetalles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClienteIdCliente",
                table: "Presupuestos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MonedaIdMoneda",
                table: "Presupuestos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Presupuestos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentasDetalles_ProductoIdProducto",
                table: "VentasDetalles",
                column: "ProductoIdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_VentasDetalles_VentaIdVenta",
                table: "VentasDetalles",
                column: "VentaIdVenta");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CajaIdCaja",
                table: "Ventas",
                column: "CajaIdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ClienteIdCliente",
                table: "Ventas",
                column: "ClienteIdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_MonedaIdMoneda",
                table: "Ventas",
                column: "MonedaIdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_SucursalId",
                table: "Ventas",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_TipoDocumentoOperacionIdTipoDocumentoOperacion",
                table: "Ventas",
                column: "TipoDocumentoOperacionIdTipoDocumentoOperacion");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_TipoPagoIdTipoPago",
                table: "Ventas",
                column: "TipoPagoIdTipoPago");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_UsuarioId_Usu",
                table: "Ventas",
                column: "UsuarioId_Usu");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosDetalles_PresupuestoIdPresupuesto",
                table: "PresupuestosDetalles",
                column: "PresupuestoIdPresupuesto");

            migrationBuilder.CreateIndex(
                name: "IX_PresupuestosDetalles_ProductoIdProducto",
                table: "PresupuestosDetalles",
                column: "ProductoIdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_ClienteIdCliente",
                table: "Presupuestos",
                column: "ClienteIdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_MonedaIdMoneda",
                table: "Presupuestos",
                column: "MonedaIdMoneda");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_SucursalId",
                table: "Presupuestos",
                column: "SucursalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Clientes_ClienteIdCliente",
                table: "Presupuestos",
                column: "ClienteIdCliente",
                principalTable: "Clientes",
                principalColumn: "IdCliente");

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Monedas_MonedaIdMoneda",
                table: "Presupuestos",
                column: "MonedaIdMoneda",
                principalTable: "Monedas",
                principalColumn: "IdMoneda");

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Sucursal_SucursalId",
                table: "Presupuestos",
                column: "SucursalId",
                principalTable: "Sucursal",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PresupuestosDetalles_Presupuestos_PresupuestoIdPresupuesto",
                table: "PresupuestosDetalles",
                column: "PresupuestoIdPresupuesto",
                principalTable: "Presupuestos",
                principalColumn: "IdPresupuesto");

            migrationBuilder.AddForeignKey(
                name: "FK_PresupuestosDetalles_Productos_ProductoIdProducto",
                table: "PresupuestosDetalles",
                column: "ProductoIdProducto",
                principalTable: "Productos",
                principalColumn: "IdProducto");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Cajas_CajaIdCaja",
                table: "Ventas",
                column: "CajaIdCaja",
                principalTable: "Cajas",
                principalColumn: "id_caja");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Clientes_ClienteIdCliente",
                table: "Ventas",
                column: "ClienteIdCliente",
                principalTable: "Clientes",
                principalColumn: "IdCliente");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Monedas_MonedaIdMoneda",
                table: "Ventas",
                column: "MonedaIdMoneda",
                principalTable: "Monedas",
                principalColumn: "IdMoneda");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Sucursal_SucursalId",
                table: "Ventas",
                column: "SucursalId",
                principalTable: "Sucursal",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_TiposDocumentoOperacion_TipoDocumentoOperacionIdTipoDocumentoOperacion",
                table: "Ventas",
                column: "TipoDocumentoOperacionIdTipoDocumentoOperacion",
                principalTable: "TiposDocumentoOperacion",
                principalColumn: "IdTipoDocumentoOperacion");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_TiposPago_TipoPagoIdTipoPago",
                table: "Ventas",
                column: "TipoPagoIdTipoPago",
                principalTable: "TiposPago",
                principalColumn: "IdTipoPago");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Usuarios_UsuarioId_Usu",
                table: "Ventas",
                column: "UsuarioId_Usu",
                principalTable: "Usuarios",
                principalColumn: "Id_Usu");

            migrationBuilder.AddForeignKey(
                name: "FK_VentasDetalles_Productos_ProductoIdProducto",
                table: "VentasDetalles",
                column: "ProductoIdProducto",
                principalTable: "Productos",
                principalColumn: "IdProducto");

            migrationBuilder.AddForeignKey(
                name: "FK_VentasDetalles_Ventas_VentaIdVenta",
                table: "VentasDetalles",
                column: "VentaIdVenta",
                principalTable: "Ventas",
                principalColumn: "IdVenta");
        }
    }
}
