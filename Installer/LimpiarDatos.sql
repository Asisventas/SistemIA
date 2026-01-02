-- ============================================
-- SISTEMÍA - Script de Limpieza de Datos
-- Elimina datos TRANSACCIONALES manteniendo catálogos
-- ============================================
-- 
-- DATOS QUE SE ELIMINAN:
--   - Ventas, Compras, Presupuestos y sus detalles
--   - Notas de crédito (ventas y compras) y sus detalles
--   - Productos (y stock, movimientos, etc.)
--   - Clientes (excepto ID 1 = Consumidor Final)
--   - Proveedores (excepto ID 1 = Proveedor General)
--   - Timbrados, Pagos, Cobros, Cierres de caja
--   - Asistencias, Ajustes de stock, Auditoría
--
-- DATOS QUE SE CONSERVAN (Catálogos):
--   - Sociedades (con certificados SIFEN)
--   - Sucursales, Cajas, Depósitos
--   - Usuarios, Roles, Permisos
--   - Monedas, Tipos de IVA, Tipos de Pago
--   - Tipos de documento, contribuyentes
--   - Marcas, Clasificaciones
--   - Listas de precios (estructura)
--   - Catálogos geográficos (Países, Departamentos, Ciudades)
--   - Actividades económicas
--   - **RucDnit** (catálogo de RUC de DNIT - 1.5M registros)
--   - **ConfiguracionSistema** (modo farmacia, etc.)
--   - **DescuentosCategorias** (descuentos automáticos)
--
-- ============================================

SET NOCOUNT ON;

PRINT '============================================'
PRINT 'SISTEMÍA - Iniciando limpieza de datos...'
PRINT '============================================'
PRINT ''

-- ============================================
-- 1. DESACTIVAR CONSTRAINTS TEMPORALMENTE
-- ============================================
PRINT '[1/15] Desactivando constraints...'
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
GO

-- ============================================
-- 2. LIMPIAR TABLAS DE VENTAS
-- ============================================
PRINT '[2/15] Limpiando ventas...'

-- Detalles de notas de crédito
IF OBJECT_ID('NotasCreditoVentasDetalles', 'U') IS NOT NULL
    DELETE FROM NotasCreditoVentasDetalles;

-- Notas de crédito
IF OBJECT_ID('NotasCreditoVentas', 'U') IS NOT NULL
    DELETE FROM NotasCreditoVentas;

-- Detalles de ventas
IF OBJECT_ID('VentasDetalles', 'U') IS NOT NULL
    DELETE FROM VentasDetalles;

-- Ventas
IF OBJECT_ID('Ventas', 'U') IS NOT NULL
    DELETE FROM Ventas;
GO

-- ============================================
-- 3. LIMPIAR TABLAS DE COMPRAS Y NOTAS DE CRÉDITO COMPRAS
-- ============================================
PRINT '[3/15] Limpiando compras y NC compras...'

-- Detalles de notas de crédito compras
IF OBJECT_ID('NotasCreditoComprasDetalles', 'U') IS NOT NULL
    DELETE FROM NotasCreditoComprasDetalles;

-- Notas de crédito compras
IF OBJECT_ID('NotasCreditoCompras', 'U') IS NOT NULL
    DELETE FROM NotasCreditoCompras;

-- Detalles de compras
IF OBJECT_ID('ComprasDetalles', 'U') IS NOT NULL
    DELETE FROM ComprasDetalles;

-- Compras
IF OBJECT_ID('Compras', 'U') IS NOT NULL
    DELETE FROM Compras;
GO

-- ============================================
-- 4. LIMPIAR TABLAS DE PRESUPUESTOS
-- ============================================
PRINT '[4/15] Limpiando presupuestos...'

IF OBJECT_ID('PresupuestosDetalles', 'U') IS NOT NULL
    DELETE FROM PresupuestosDetalles;

IF OBJECT_ID('Presupuestos', 'U') IS NOT NULL
    DELETE FROM Presupuestos;
GO

-- ============================================
-- 5. LIMPIAR PAGOS Y COBROS
-- ============================================
PRINT '[5/15] Limpiando pagos y cobros...'

IF OBJECT_ID('PagosProveedoresDetalles', 'U') IS NOT NULL
    DELETE FROM PagosProveedoresDetalles;

IF OBJECT_ID('PagosProveedores', 'U') IS NOT NULL
    DELETE FROM PagosProveedores;

IF OBJECT_ID('CuentasPorPagar', 'U') IS NOT NULL
    DELETE FROM CuentasPorPagar;

IF OBJECT_ID('CobrosDetalles', 'U') IS NOT NULL
    DELETE FROM CobrosDetalles;

IF OBJECT_ID('Cobros', 'U') IS NOT NULL
    DELETE FROM Cobros;

IF OBJECT_ID('CuotasCobros', 'U') IS NOT NULL
    DELETE FROM CuotasCobros;
GO

-- ============================================
-- 6. LIMPIAR CIERRES DE CAJA
-- ============================================
PRINT '[6/15] Limpiando cierres de caja...'

IF OBJECT_ID('ComposicionCajaCierres', 'U') IS NOT NULL
    DELETE FROM ComposicionCajaCierres;

IF OBJECT_ID('CierresCaja', 'U') IS NOT NULL
    DELETE FROM CierresCaja;
GO

-- ============================================
-- 7. LIMPIAR PRODUCTOS Y STOCK
-- ============================================
PRINT '[7/15] Limpiando productos y stock...'

-- Movimientos de inventario
IF OBJECT_ID('MovimientosInventario', 'U') IS NOT NULL
    DELETE FROM MovimientosInventario;

-- Ajustes de stock detalles
IF OBJECT_ID('AjusteStockDetalles', 'U') IS NOT NULL
    DELETE FROM AjusteStockDetalles;

-- Ajustes de stock
IF OBJECT_ID('AjustesStock', 'U') IS NOT NULL
    DELETE FROM AjustesStock;

-- Stock por depósito
IF OBJECT_ID('ProductoDepositos', 'U') IS NOT NULL
    DELETE FROM ProductoDepositos;

-- Detalles de listas de precios
IF OBJECT_ID('ListasPreciosDetalles', 'U') IS NOT NULL
    DELETE FROM ListasPreciosDetalles;

-- Precios por cliente
IF OBJECT_ID('ClientesPrecios', 'U') IS NOT NULL
    DELETE FROM ClientesPrecios;

-- Productos
IF OBJECT_ID('Productos', 'U') IS NOT NULL
    DELETE FROM Productos;
GO

-- ============================================
-- 8. LIMPIAR CLIENTES (excepto ID 1)
-- ============================================
PRINT '[8/15] Limpiando clientes (excepto Consumidor Final)...'

IF OBJECT_ID('Clientes', 'U') IS NOT NULL
    DELETE FROM Clientes WHERE IdCliente > 1;
GO

-- ============================================
-- 9. LIMPIAR PROVEEDORES (excepto ID 1)
-- ============================================
PRINT '[9/15] Limpiando proveedores (excepto Proveedor General)...'

IF OBJECT_ID('ProveedoresSifen', 'U') IS NOT NULL
    DELETE FROM ProveedoresSifen WHERE IdProveedor > 1;
GO

-- ============================================
-- 10. LIMPIAR TIMBRADOS
-- ============================================
PRINT '[10/15] Limpiando timbrados...'

IF OBJECT_ID('Timbrados', 'U') IS NOT NULL
    DELETE FROM Timbrados;
GO

-- ============================================
-- 11. LIMPIAR ASISTENCIAS
-- ============================================
PRINT '[11/15] Limpiando asistencias...'

IF OBJECT_ID('Asistencias', 'U') IS NOT NULL
    DELETE FROM Asistencias;

IF OBJECT_ID('JustificacionesAsistencia', 'U') IS NOT NULL
    DELETE FROM JustificacionesAsistencia;
GO

-- ============================================
-- 12. LIMPIAR AUDITORÍA
-- ============================================
PRINT '[12/15] Limpiando auditoría...'

IF OBJECT_ID('RegistrosAuditoria', 'U') IS NOT NULL
    DELETE FROM RegistrosAuditoria;
GO

-- ============================================
-- 13. REACTIVAR CONSTRAINTS
-- ============================================
PRINT '[13/15] Reactivando constraints...'
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'
GO

-- ============================================
-- 14. RESETEAR IDENTIDADES
-- ============================================
PRINT '[14/15] Reseteando secuencias de identidad...'

-- Ventas
IF OBJECT_ID('Ventas', 'U') IS NOT NULL
    DBCC CHECKIDENT ('Ventas', RESEED, 0);

IF OBJECT_ID('VentasDetalles', 'U') IS NOT NULL
    DBCC CHECKIDENT ('VentasDetalles', RESEED, 0);

IF OBJECT_ID('NotasCreditoVentas', 'U') IS NOT NULL
    DBCC CHECKIDENT ('NotasCreditoVentas', RESEED, 0);

IF OBJECT_ID('NotasCreditoVentasDetalles', 'U') IS NOT NULL
    DBCC CHECKIDENT ('NotasCreditoVentasDetalles', RESEED, 0);

-- Compras
IF OBJECT_ID('Compras', 'U') IS NOT NULL
    DBCC CHECKIDENT ('Compras', RESEED, 0);

IF OBJECT_ID('ComprasDetalles', 'U') IS NOT NULL
    DBCC CHECKIDENT ('ComprasDetalles', RESEED, 0);

-- Notas de Crédito Compras
IF OBJECT_ID('NotasCreditoCompras', 'U') IS NOT NULL
    DBCC CHECKIDENT ('NotasCreditoCompras', RESEED, 0);

IF OBJECT_ID('NotasCreditoComprasDetalles', 'U') IS NOT NULL
    DBCC CHECKIDENT ('NotasCreditoComprasDetalles', RESEED, 0);

-- Presupuestos
IF OBJECT_ID('Presupuestos', 'U') IS NOT NULL
    DBCC CHECKIDENT ('Presupuestos', RESEED, 0);

IF OBJECT_ID('PresupuestosDetalles', 'U') IS NOT NULL
    DBCC CHECKIDENT ('PresupuestosDetalles', RESEED, 0);

-- Pagos/Cobros
IF OBJECT_ID('Cobros', 'U') IS NOT NULL
    DBCC CHECKIDENT ('Cobros', RESEED, 0);

IF OBJECT_ID('CobrosDetalles', 'U') IS NOT NULL
    DBCC CHECKIDENT ('CobrosDetalles', RESEED, 0);

IF OBJECT_ID('PagosProveedores', 'U') IS NOT NULL
    DBCC CHECKIDENT ('PagosProveedores', RESEED, 0);

-- Cierres
IF OBJECT_ID('CierresCaja', 'U') IS NOT NULL
    DBCC CHECKIDENT ('CierresCaja', RESEED, 0);

-- Productos
IF OBJECT_ID('Productos', 'U') IS NOT NULL
    DBCC CHECKIDENT ('Productos', RESEED, 0);

IF OBJECT_ID('MovimientosInventario', 'U') IS NOT NULL
    DBCC CHECKIDENT ('MovimientosInventario', RESEED, 0);

IF OBJECT_ID('AjustesStock', 'U') IS NOT NULL
    DBCC CHECKIDENT ('AjustesStock', RESEED, 0);

-- Timbrados
IF OBJECT_ID('Timbrados', 'U') IS NOT NULL
    DBCC CHECKIDENT ('Timbrados', RESEED, 0);

-- Asistencias
IF OBJECT_ID('Asistencias', 'U') IS NOT NULL
    DBCC CHECKIDENT ('Asistencias', RESEED, 0);

-- Auditoría
IF OBJECT_ID('RegistrosAuditoria', 'U') IS NOT NULL
    DBCC CHECKIDENT ('RegistrosAuditoria', RESEED, 0);
GO

-- ============================================
-- 15. RESUMEN
-- ============================================
PRINT ''
PRINT '[15/15] Limpieza completada'
PRINT '============================================'
PRINT ''
PRINT 'Datos ELIMINADOS:'
PRINT '  - Ventas, Compras, Presupuestos y NC (ventas y compras)'
PRINT '  - Productos, Stock, Movimientos'
PRINT '  - Clientes (excepto ID 1)'
PRINT '  - Proveedores (excepto ID 1)'
PRINT '  - Timbrados, Pagos, Cobros'
PRINT '  - Cierres de caja, Asistencias'
PRINT '  - Auditoría'
PRINT ''
PRINT 'Datos CONSERVADOS:'
PRINT '  - Sociedades, Sucursales, Cajas, Depósitos'
PRINT '  - Usuarios, Roles, Permisos'
PRINT '  - Monedas, Tipos IVA, Tipos Pago'
PRINT '  - Marcas, Clasificaciones'
PRINT '  - Catálogos geográficos'
PRINT '  - Actividades económicas'
PRINT '  - RucDnit (catálogo DNIT)'
PRINT '  - ConfiguracionSistema, DescuentosCategorias'
PRINT ''
PRINT '============================================'
GO
