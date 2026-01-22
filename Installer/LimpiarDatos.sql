-- ============================================
-- SISTEMÍA - Script de Limpieza de Datos
-- Elimina datos TRANSACCIONALES manteniendo catálogos
-- ============================================
-- 
-- DATOS QUE SE ELIMINAN:
--   - Ventas, Compras, Presupuestos y sus detalles
--   - Notas de crédito (ventas y compras) y sus detalles
--   - Productos (y stock, lotes, movimientos, etc.)
--   - Clientes (excepto ID 1 = Consumidor Final)
--   - Proveedores (excepto ID 1 = Proveedor General)
--   - Timbrados, Pagos, Cobros, Cierres de caja
--   - Cuentas por cobrar/pagar, Cuotas
--   - Transferencias, Salidas de stock, Ajustes
--   - Asistencias, Auditoría
--   - Asistente IA (conversaciones, preguntas, etc.)
--   - Historial de cambios del sistema
--   - Tipos de cambio históricos
--
-- DATOS QUE SE CONSERVAN (Catálogos):
--   - Sociedades (con certificados SIFEN)
--   - Sucursales, Cajas, Depósitos
--   - Usuarios, Roles, Permisos, Módulos
--   - Monedas, Tipos de IVA, Tipos de Pago
--   - Tipos de documento, contribuyentes
--   - Marcas, Clasificaciones
--   - Listas de precios (estructura)
--   - Catálogos geográficos (Países, Departamentos, Ciudades)
--   - Actividades económicas
--   - **RucDnit** (catálogo de RUC de DNIT - 1.5M registros)
--   - **ConfiguracionSistema** (modo farmacia, etc.)
--   - **DescuentosCategorias** (descuentos automáticos)
--   - **ConfiguracionesAsistenteIA** (config del asistente)
--   - **ArticulosConocimiento** (base de conocimiento IA)
--   - **CategoriasConocimiento** (categorías del asistente)
--
-- ============================================

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;

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
PRINT '[5/15] Limpiando pagos, cobros y cuentas...'

-- Cuotas de cuentas por cobrar
IF OBJECT_ID('CuentasPorCobrarCuotas', 'U') IS NOT NULL
    DELETE FROM CuentasPorCobrarCuotas;

-- Cuentas por cobrar
IF OBJECT_ID('CuentasPorCobrar', 'U') IS NOT NULL
    DELETE FROM CuentasPorCobrar;

-- Cobros de cuotas
IF OBJECT_ID('CobrosCuotas', 'U') IS NOT NULL
    DELETE FROM CobrosCuotas;

-- Cuotas de ventas
IF OBJECT_ID('VentasCuotas', 'U') IS NOT NULL
    DELETE FROM VentasCuotas;

-- Detalles de pagos de ventas
IF OBJECT_ID('VentasPagosDetalles', 'U') IS NOT NULL
    DELETE FROM VentasPagosDetalles;

-- Pagos de ventas
IF OBJECT_ID('VentasPagos', 'U') IS NOT NULL
    DELETE FROM VentasPagos;

-- Cuotas de cuentas por pagar
IF OBJECT_ID('CuentasPorPagarCuotas', 'U') IS NOT NULL
    DELETE FROM CuentasPorPagarCuotas;

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
-- 6. LIMPIAR CIERRES DE CAJA Y ENTREGAS
-- ============================================
PRINT '[6/15] Limpiando cierres de caja y entregas...'

-- Detalles de composiciones de caja
IF OBJECT_ID('ComposicionesCajaDetalles', 'U') IS NOT NULL
    DELETE FROM ComposicionesCajaDetalles;

-- Composiciones de caja
IF OBJECT_ID('ComposicionesCaja', 'U') IS NOT NULL
    DELETE FROM ComposicionesCaja;

IF OBJECT_ID('ComposicionCajaCierres', 'U') IS NOT NULL
    DELETE FROM ComposicionCajaCierres;

-- Entregas de caja
IF OBJECT_ID('EntregasCaja', 'U') IS NOT NULL
    DELETE FROM EntregasCaja;

IF OBJECT_ID('CierresCaja', 'U') IS NOT NULL
    DELETE FROM CierresCaja;
GO

-- ============================================
-- 7. LIMPIAR PRODUCTOS, STOCK, LOTES Y TRANSFERENCIAS
-- ============================================
PRINT '[7/15] Limpiando productos, stock, lotes y transferencias...'

-- Movimientos de lotes
IF OBJECT_ID('MovimientosLotes', 'U') IS NOT NULL
    DELETE FROM MovimientosLotes;

-- Lotes de productos
IF OBJECT_ID('ProductosLotes', 'U') IS NOT NULL
    DELETE FROM ProductosLotes;

-- Componentes de productos (productos compuestos)
IF OBJECT_ID('ProductosComponentes', 'U') IS NOT NULL
    DELETE FROM ProductosComponentes;

-- Movimientos de inventario
IF OBJECT_ID('MovimientosInventario', 'U') IS NOT NULL
    DELETE FROM MovimientosInventario;

-- Detalles de transferencias de depósito
IF OBJECT_ID('TransferenciasDepositoDetalle', 'U') IS NOT NULL
    DELETE FROM TransferenciasDepositoDetalle;

-- Transferencias de depósito
IF OBJECT_ID('TransferenciasDeposito', 'U') IS NOT NULL
    DELETE FROM TransferenciasDeposito;

-- Detalles de salidas de stock
IF OBJECT_ID('SalidasStockDetalle', 'U') IS NOT NULL
    DELETE FROM SalidasStockDetalle;

-- Salidas de stock
IF OBJECT_ID('SalidasStock', 'U') IS NOT NULL
    DELETE FROM SalidasStock;

-- Detalles de remisiones internas
IF OBJECT_ID('RemisionesInternasDetalles', 'U') IS NOT NULL
    DELETE FROM RemisionesInternasDetalles;

-- Remisiones internas
IF OBJECT_ID('RemisionesInternas', 'U') IS NOT NULL
    DELETE FROM RemisionesInternas;

-- Ajustes de stock detalles
IF OBJECT_ID('AjustesStockDetalles', 'U') IS NOT NULL
    DELETE FROM AjustesStockDetalles;

-- Ajustes de stock
IF OBJECT_ID('AjustesStock', 'U') IS NOT NULL
    DELETE FROM AjustesStock;

-- Stock por depósito
IF OBJECT_ID('ProductosDepositos', 'U') IS NOT NULL
    DELETE FROM ProductosDepositos;

-- Detalles de listas de precios
IF OBJECT_ID('ListasPreciosDetalles', 'U') IS NOT NULL
    DELETE FROM ListasPreciosDetalles;

-- Precios por cliente
IF OBJECT_ID('ClientesPrecios', 'U') IS NOT NULL
    DELETE FROM ClientesPrecios;

-- Recetas de ventas
IF OBJECT_ID('RecetasVentas', 'U') IS NOT NULL
    DELETE FROM RecetasVentas;

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
GO

-- ============================================
-- 12. LIMPIAR AUDITORÍA
-- ============================================
PRINT '[12/15] Limpiando auditoría...'

IF OBJECT_ID('RegistrosAuditoria', 'U') IS NOT NULL
    DELETE FROM RegistrosAuditoria;

IF OBJECT_ID('AuditoriasAcciones', 'U') IS NOT NULL
    DELETE FROM AuditoriasAcciones;
GO

-- ============================================
-- 12B. LIMPIAR CONFIGURACIÓN DE CORREO
-- ============================================
PRINT '[12B/15] Limpiando configuración de correo...'

IF OBJECT_ID('DestinatariosInforme', 'U') IS NOT NULL
    DELETE FROM DestinatariosInforme;

IF OBJECT_ID('ConfiguracionesCorreo', 'U') IS NOT NULL
    DELETE FROM ConfiguracionesCorreo;
GO

-- ============================================
-- 12C. LIMPIAR ASISTENTE IA (Conversaciones, no base de conocimiento)
-- ============================================
PRINT '[12C/15] Limpiando datos del asistente IA...'

-- Acciones de usuario
IF OBJECT_ID('AccionesUsuario', 'U') IS NOT NULL
    DELETE FROM AccionesUsuario;

-- Solicitudes de soporte de IA
IF OBJECT_ID('SolicitudesSoporteIA', 'U') IS NOT NULL
    DELETE FROM SolicitudesSoporteIA;

-- Solicitudes de soporte del asistente (legacy)
IF OBJECT_ID('SolicitudesSoporteAsistente', 'U') IS NOT NULL
    DELETE FROM SolicitudesSoporteAsistente;

-- Preguntas sin respuesta
IF OBJECT_ID('PreguntasSinRespuesta', 'U') IS NOT NULL
    DELETE FROM PreguntasSinRespuesta;

-- Conversaciones del asistente
IF OBJECT_ID('ConversacionesAsistente', 'U') IS NOT NULL
    DELETE FROM ConversacionesAsistente;

-- Registros de error de IA
IF OBJECT_ID('RegistrosErrorIA', 'U') IS NOT NULL
    DELETE FROM RegistrosErrorIA;
GO

-- ============================================
-- 12D. LIMPIAR HISTORIAL DEL SISTEMA
-- ============================================
PRINT '[12D/15] Limpiando historial del sistema...'

-- Conversaciones IA del historial (documentación de desarrollo)
IF OBJECT_ID('ConversacionesIAHistorial', 'U') IS NOT NULL
    DELETE FROM ConversacionesIAHistorial;

-- Historial de cambios del sistema
IF OBJECT_ID('HistorialCambiosSistema', 'U') IS NOT NULL
    DELETE FROM HistorialCambiosSistema;

-- Tipos de cambio históricos
IF OBJECT_ID('TiposCambioHistorico', 'U') IS NOT NULL
    DELETE FROM TiposCambioHistorico;
GO

-- ============================================
-- 12E. LIMPIAR PRESUPUESTOS DEL SISTEMA (Cotizaciones comerciales)
-- ============================================
PRINT '[12E/15] Limpiando presupuestos del sistema...'

IF OBJECT_ID('PresupuestosSistemaDetalles', 'U') IS NOT NULL
    DELETE FROM PresupuestosSistemaDetalles;

IF OBJECT_ID('PresupuestosSistema', 'U') IS NOT NULL
    DELETE FROM PresupuestosSistema;
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

IF OBJECT_ID('CobrosCuotas', 'U') IS NOT NULL
    DBCC CHECKIDENT ('CobrosCuotas', RESEED, 0);

IF OBJECT_ID('PagosProveedores', 'U') IS NOT NULL
    DBCC CHECKIDENT ('PagosProveedores', RESEED, 0);

IF OBJECT_ID('VentasPagos', 'U') IS NOT NULL
    DBCC CHECKIDENT ('VentasPagos', RESEED, 0);

IF OBJECT_ID('VentasCuotas', 'U') IS NOT NULL
    DBCC CHECKIDENT ('VentasCuotas', RESEED, 0);

-- Cuentas por cobrar/pagar
IF OBJECT_ID('CuentasPorCobrar', 'U') IS NOT NULL
    DBCC CHECKIDENT ('CuentasPorCobrar', RESEED, 0);

IF OBJECT_ID('CuentasPorCobrarCuotas', 'U') IS NOT NULL
    DBCC CHECKIDENT ('CuentasPorCobrarCuotas', RESEED, 0);

IF OBJECT_ID('CuentasPorPagar', 'U') IS NOT NULL
    DBCC CHECKIDENT ('CuentasPorPagar', RESEED, 0);

IF OBJECT_ID('CuentasPorPagarCuotas', 'U') IS NOT NULL
    DBCC CHECKIDENT ('CuentasPorPagarCuotas', RESEED, 0);

-- Cierres y entregas de caja
IF OBJECT_ID('CierresCaja', 'U') IS NOT NULL
    DBCC CHECKIDENT ('CierresCaja', RESEED, 0);

IF OBJECT_ID('EntregasCaja', 'U') IS NOT NULL
    DBCC CHECKIDENT ('EntregasCaja', RESEED, 0);

IF OBJECT_ID('ComposicionesCaja', 'U') IS NOT NULL
    DBCC CHECKIDENT ('ComposicionesCaja', RESEED, 0);

-- Productos
IF OBJECT_ID('Productos', 'U') IS NOT NULL
    DBCC CHECKIDENT ('Productos', RESEED, 0);

IF OBJECT_ID('MovimientosInventario', 'U') IS NOT NULL
    DBCC CHECKIDENT ('MovimientosInventario', RESEED, 0);

IF OBJECT_ID('AjustesStock', 'U') IS NOT NULL
    DBCC CHECKIDENT ('AjustesStock', RESEED, 0);

-- Lotes y transferencias
IF OBJECT_ID('ProductosLotes', 'U') IS NOT NULL
    DBCC CHECKIDENT ('ProductosLotes', RESEED, 0);

IF OBJECT_ID('MovimientosLotes', 'U') IS NOT NULL
    DBCC CHECKIDENT ('MovimientosLotes', RESEED, 0);

IF OBJECT_ID('TransferenciasDeposito', 'U') IS NOT NULL
    DBCC CHECKIDENT ('TransferenciasDeposito', RESEED, 0);

IF OBJECT_ID('SalidasStock', 'U') IS NOT NULL
    DBCC CHECKIDENT ('SalidasStock', RESEED, 0);

IF OBJECT_ID('RemisionesInternas', 'U') IS NOT NULL
    DBCC CHECKIDENT ('RemisionesInternas', RESEED, 0);

-- Timbrados
IF OBJECT_ID('Timbrados', 'U') IS NOT NULL
    DBCC CHECKIDENT ('Timbrados', RESEED, 0);

-- Asistencias
IF OBJECT_ID('Asistencias', 'U') IS NOT NULL
    DBCC CHECKIDENT ('Asistencias', RESEED, 0);

-- Auditoría
IF OBJECT_ID('RegistrosAuditoria', 'U') IS NOT NULL
    DBCC CHECKIDENT ('RegistrosAuditoria', RESEED, 0);

IF OBJECT_ID('AuditoriasAcciones', 'U') IS NOT NULL
    DBCC CHECKIDENT ('AuditoriasAcciones', RESEED, 0);

-- Configuración de Correo
IF OBJECT_ID('DestinatariosInforme', 'U') IS NOT NULL
    DBCC CHECKIDENT ('DestinatariosInforme', RESEED, 0);

IF OBJECT_ID('ConfiguracionesCorreo', 'U') IS NOT NULL
    DBCC CHECKIDENT ('ConfiguracionesCorreo', RESEED, 0);

-- Asistente IA
IF OBJECT_ID('ConversacionesAsistente', 'U') IS NOT NULL
    DBCC CHECKIDENT ('ConversacionesAsistente', RESEED, 0);

IF OBJECT_ID('SolicitudesSoporteAsistente', 'U') IS NOT NULL
    DBCC CHECKIDENT ('SolicitudesSoporteAsistente', RESEED, 0);

IF OBJECT_ID('SolicitudesSoporteIA', 'U') IS NOT NULL
    DBCC CHECKIDENT ('SolicitudesSoporteIA', RESEED, 0);

IF OBJECT_ID('PreguntasSinRespuesta', 'U') IS NOT NULL
    DBCC CHECKIDENT ('PreguntasSinRespuesta', RESEED, 0);

IF OBJECT_ID('AccionesUsuario', 'U') IS NOT NULL
    DBCC CHECKIDENT ('AccionesUsuario', RESEED, 0);

IF OBJECT_ID('RegistrosErrorIA', 'U') IS NOT NULL
    DBCC CHECKIDENT ('RegistrosErrorIA', RESEED, 0);

-- Historial del sistema
IF OBJECT_ID('HistorialCambiosSistema', 'U') IS NOT NULL
    DBCC CHECKIDENT ('HistorialCambiosSistema', RESEED, 0);

IF OBJECT_ID('ConversacionesIAHistorial', 'U') IS NOT NULL
    DBCC CHECKIDENT ('ConversacionesIAHistorial', RESEED, 0);

-- Presupuestos del sistema
IF OBJECT_ID('PresupuestosSistema', 'U') IS NOT NULL
    DBCC CHECKIDENT ('PresupuestosSistema', RESEED, 0);
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
PRINT '  - Productos, Stock, Lotes, Movimientos'
PRINT '  - Transferencias de deposito, Salidas de stock'
PRINT '  - Remisiones internas, Ajustes de stock'
PRINT '  - Cuentas por cobrar y pagar con cuotas'
PRINT '  - Pagos de ventas, Cobros, Cuotas'
PRINT '  - Clientes (excepto ID 1)'
PRINT '  - Proveedores (excepto ID 1)'
PRINT '  - Timbrados, Pagos proveedores'
PRINT '  - Cierres de caja, Entregas de caja, Composiciones'
PRINT '  - Asistencias'
PRINT '  - Auditoria, Acciones auditadas'
PRINT '  - Configuracion de Correo y Destinatarios'
PRINT '  - Conversaciones del Asistente IA, Errores IA'
PRINT '  - Historial de cambios del sistema'
PRINT '  - Tipos de cambio historicos'
PRINT '  - Presupuestos del sistema (cotizaciones comerciales)'
PRINT ''
PRINT 'Datos CONSERVADOS:'
PRINT '  - Sociedades, Sucursales, Cajas, Depositos'
PRINT '  - Usuarios, Roles, Permisos, Modulos'
PRINT '  - Monedas, Tipos IVA, Tipos Pago'
PRINT '  - Marcas, Clasificaciones, Categorias'
PRINT '  - Listas de precios (estructura)'
PRINT '  - Catalogos geograficos'
PRINT '  - Actividades economicas'
PRINT '  - RucDnit (catalogo DNIT)'
PRINT '  - ConfiguracionSistema, DescuentosCategorias'
PRINT '  - ConfiguracionesAsistenteIA'
PRINT '  - ArticulosConocimiento (base de conocimiento IA)'
PRINT '  - CategoriasConocimiento'
PRINT ''
PRINT '============================================'
GO
