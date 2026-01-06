# 📋 Historial de Cambios - SistemIA

## Versión 2026.01.06

### 👥 Módulo de Asistencia - Mejoras
- **NUEVO**: Campo Turno en registro de asistencia (entrada/salida)
- **NUEVO**: Campo Caja asociada al registro de asistencia
- **MEJORA**: Informe de Asistencia con formato profesional de impresión
  - Header con logo, empresa, RUC y sucursal
  - Tabla con badges de colores para estado y tipo
  - Formato A4 landscape para mejor visualización
- **MEJORA**: Exportación a CSV y Excel con ClosedXML
- **MEJORA**: Botones compactos estilo Resumen de Caja (Buscar, CSV, Excel, Imprimir, Correo)

---

## Versión 2026.01.05

### 🤖 Asistente IA - Sincronización Automática
- **NUEVO**: Los artículos de conocimiento de la IA se sincronizan automáticamente al actualizar
- Los datos del cliente (conversaciones, artículos personalizados) se preservan
- 23 artículos de conocimiento incluidos para ayudar a usuarios

### 📧 Sistema de Correo Electrónico
- Envío automático de informes por correo con PDF adjunto
- Configuración de destinatarios por tipo de informe
- Soporte para múltiples destinatarios con diferentes preferencias
- Envío de facturas por correo al cliente (configurable por cliente)

### 📊 Informes Mejorados
- Informe de Notas de Crédito de Compras
- Informe de Productos Valorizado (stock con valores)
- Informes agrupados y detallados de ventas
- Panel de control con filtro por sucursal

---

## Versión 2026.01.02

### 📝 Notas de Crédito de Compras
- Módulo completo de NC de Compras
- Afecta stock y cuentas por pagar
- Integración con cierre de caja
- Formatos de impresión A4 y Ticket

### ⚙️ Configuración del Sistema
- Nueva página de configuración centralizada
- Gestión de descuentos por producto/cliente
- Configuración de correo SMTP mejorada

### 💰 Descuentos
- Descuentos configurables por producto
- Descuentos por cliente/clasificación
- Aplicación automática en ventas

---

## Versión 2025.12.28

### ♻️ Mejoras en Compras
- Función "Reciclar Compra" para repetir compras anteriores
- PrecioVentaRef en detalle de compras para seguimiento de márgenes

### 🧾 Notas de Crédito Ventas
- Módulo completo con soporte SIFEN
- Motivos: Devolución, Descuento, Bonificación, Crédito incobrable
- Afecta stock automáticamente (configurable)
- Formatos A4 y Ticket 80mm
- Soporte multimonedas

### 📈 Cierre de Caja Mejorado
- Resumen por sucursal y caja
- Detalle de NC de ventas y compras
- Cobros de crédito incluidos
- Pagos a proveedores integrados

### 🖨️ Impresión
- VentasExplorar con impresión de ticket directo
- Mejoras UI en informes de pagos/cobros

### 🔧 Correcciones
- IdSucursal en cierres y cobros
- Permisos de módulos actualizados

---

## Versiones Anteriores

### 2025.12.15 - Sistema de Permisos
- Sistema completo de permisos por rol
- Protección de páginas por módulo
- Auditoría de accesos

### 2025.12.01 - SIFEN
- Integración completa con SIFEN Paraguay
- Facturación electrónica
- Envío de lotes al SET
- Consulta de estado de documentos

---

## 📌 Notas de Actualización

### Para clientes existentes:
1. **Backup**: Siempre realizar backup antes de actualizar
2. **Migraciones**: Marcar "Aplicar migraciones" si hay cambios de BD
3. **Reinicio**: Usar "Actualizar con Reinicio" en servidores de producción

### Preservación de datos:
- ✅ Ventas, Compras, Clientes, Productos → Se preservan
- ✅ Configuraciones personalizadas → Se preservan
- ✅ Datos de la IA del cliente → Se preservan
- ✅ Historial de conversaciones IA → Se preserva

---

*Última actualización: 5 de enero de 2026*
