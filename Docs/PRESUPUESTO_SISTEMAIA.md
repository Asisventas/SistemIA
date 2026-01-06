# SistemIA - Sistema de Gestión Empresarial

## Propuesta Comercial

---

## 1. Descripción General

**SistemIA** es un sistema integral de gestión empresarial diseñado para empresas comerciales en Paraguay, con integración completa al Sistema de Facturación Electrónica (SIFEN) del SET.

### Características Principales
- Sistema web accesible desde cualquier dispositivo (PC, tablet, celular)
- Interfaz moderna e intuitiva
- Asistente de IA integrado para ayuda al usuario
- Compatible con impresoras térmicas de tickets
- Generación de reportes en PDF
- Backup automático de datos
- Actualizaciones remotas sin pérdida de información

---

## 2. Módulos del Sistema

### 📦 MÓDULO DE INVENTARIO

| Funcionalidad | Descripción |
|---------------|-------------|
| Gestión de Productos | Alta, baja y modificación de productos con múltiples atributos |
| Control de Stock | Stock por depósito, stock mínimo, alertas de reposición |
| Códigos de Barras | Soporte para lectura con pistola de códigos |
| Categorías y Marcas | Organización jerárquica de productos |
| Listas de Precios | Múltiples listas de precios por cliente/clasificación |
| Ajustes de Inventario | Ajustes por diferencia, merma, rotura con historial |
| Transferencias | Movimiento de stock entre depósitos |
| Informes de Stock | Stock valorizado, movimientos, productos sin rotación |

### 🛒 MÓDULO DE VENTAS

| Funcionalidad | Descripción |
|---------------|-------------|
| Punto de Venta | Interfaz rápida para ventas al mostrador |
| Ventas a Crédito | Gestión de cuotas y vencimientos |
| Presupuestos | Cotizaciones convertibles a venta |
| Notas de Crédito | Devoluciones totales y parciales |
| Descuentos | Por producto, cliente o promociones |
| Múltiples Formas de Pago | Efectivo, tarjeta, transferencia, cheque |
| Impresión de Tickets | Formato 80mm para impresoras térmicas |
| Impresión A4 | Facturas formato completo |

### 🧾 FACTURACIÓN ELECTRÓNICA (SIFEN)

| Funcionalidad | Descripción |
|---------------|-------------|
| Factura Electrónica (FE) | Emisión y envío automático al SET |
| Nota de Crédito Electrónica | NC con referencia a factura original |
| Autofactura Electrónica | Para compras a no contribuyentes |
| Consulta de Estado | Verificación de documentos en SIFEN |
| Generación de KuDE | Representación gráfica con código QR |
| Lotes de Envío | Envío masivo de documentos |
| Gestión de Timbrados | Control de vigencia y numeración |

### 🛍️ MÓDULO DE COMPRAS

| Funcionalidad | Descripción |
|---------------|-------------|
| Registro de Compras | Ingreso de facturas de proveedores |
| Notas de Crédito | NC recibidas de proveedores |
| Cuentas por Pagar | Control de deudas con proveedores |
| Pagos a Proveedores | Registro de pagos parciales o totales |
| Historial de Precios | Seguimiento de costos por producto |
| Reciclar Compra | Repetir compras anteriores rápidamente |

### 👥 MÓDULO DE CLIENTES

| Funcionalidad | Descripción |
|---------------|-------------|
| Ficha de Cliente | Datos completos, RUC, dirección, contacto |
| Clasificación | Agrupación por tipo de cliente |
| Precios Diferenciados | Precios especiales por cliente |
| Cuentas por Cobrar | Saldos pendientes y vencimientos |
| Cobro de Cuotas | Registro de pagos recibidos |
| Historial de Compras | Detalle de todas las transacciones |
| Límite de Crédito | Control de montos máximos |

### 💰 MÓDULO DE CAJA

| Funcionalidad | Descripción |
|---------------|-------------|
| Apertura/Cierre de Caja | Control de turnos y responsables |
| Arqueo de Caja | Verificación de efectivo vs sistema |
| Composición de Caja | Detalle por denominación de billetes |
| Múltiples Cajas | Soporte para varias cajas simultáneas |
| Ingresos/Egresos | Movimientos no relacionados a ventas |
| Resumen de Cierre | Totales por forma de pago |

### 📊 MÓDULO DE INFORMES

| Funcionalidad | Descripción |
|---------------|-------------|
| Ventas Diarias | Resumen y detalle de ventas |
| Ventas por Producto | Análisis de productos más vendidos |
| Ventas por Cliente | Ranking de clientes |
| Compras por Período | Control de gastos |
| Stock Valorizado | Valor del inventario |
| Cuentas por Cobrar | Antigüedad de saldos |
| Cuentas por Pagar | Deudas pendientes |
| Cierre de Caja | Resumen de movimientos |
| Exportación Excel | Todos los informes exportables |

### 👤 MÓDULO DE USUARIOS Y SEGURIDAD

| Funcionalidad | Descripción |
|---------------|-------------|
| Gestión de Usuarios | Alta, baja, cambio de contraseña |
| Roles y Permisos | Control de acceso por módulo |
| Auditoría | Registro de acciones de usuarios |
| Múltiples Sucursales | Gestión centralizada o independiente |
| Control de Asistencia | Registro de entrada/salida |

### 📧 MÓDULO DE COMUNICACIONES

| Funcionalidad | Descripción |
|---------------|-------------|
| Envío de Facturas | Factura PDF por correo al cliente |
| Informes Automáticos | Envío programado de reportes |
| Múltiples Destinatarios | Configuración por tipo de informe |

### 🤖 ASISTENTE INTELIGENTE

| Funcionalidad | Descripción |
|---------------|-------------|
| Chat Integrado | Ayuda contextual en cada pantalla |
| Base de Conocimiento | Respuestas sobre uso del sistema |
| Soporte Técnico | Envío de consultas al equipo de soporte |

---

## 3. Requisitos Técnicos

### Servidor (donde se instala el sistema)
- **Sistema Operativo:** Windows 10/11 o Windows Server 2016+
- **Procesador:** Intel Core i3 o superior
- **Memoria RAM:** 8 GB mínimo (16 GB recomendado)
- **Disco:** 50 GB libres (SSD recomendado)
- **Red:** Conexión a internet para SIFEN

### Estaciones de Trabajo (acceso al sistema)
- Navegador web moderno (Chrome, Edge, Firefox)
- Conexión a la red local del servidor

### Impresoras Compatibles
- Impresoras térmicas 80mm (Epson, Star, Bixolon)
- Impresoras láser/inyección para formato A4

---

## 4. Servicios Incluidos

### Implementación
- [ ] Instalación y configuración del servidor
- [ ] Configuración de base de datos
- [ ] Configuración de SIFEN (certificado digital, timbrados)
- [ ] Migración de datos existentes (si aplica)
- [ ] Configuración de impresoras
- [ ] Creación de usuarios iniciales

### Capacitación
- [ ] Capacitación presencial al personal (__ horas)
- [ ] Manual de usuario digital
- [ ] Videos tutoriales de operaciones básicas

### Soporte Técnico
- [ ] Soporte por WhatsApp/Teléfono
- [ ] Soporte remoto (TeamViewer/AnyDesk)
- [ ] Actualizaciones del sistema
- [ ] Backup de seguridad

---

## 5. Modalidades de Contratación

### Opción A: Licencia Única + Mantenimiento

| Concepto | Precio |
|----------|--------|
| Licencia del Sistema (pago único) | Gs. __________ |
| Implementación y Capacitación | Gs. __________ |
| **TOTAL INICIAL** | **Gs. __________** |
| | |
| Mantenimiento Mensual (soporte + actualizaciones) | Gs. __________ /mes |

### Opción B: Suscripción Mensual (Todo Incluido)

| Concepto | Precio |
|----------|--------|
| Implementación y Capacitación (único) | Gs. __________ |
| | |
| Suscripción Mensual | Gs. __________ /mes |
| *Incluye: licencia, soporte, actualizaciones, backups* | |

### Opcionales

| Concepto | Precio |
|----------|--------|
| Sucursal Adicional | Gs. __________ |
| Usuario Adicional (más de 5) | Gs. __________ c/u |
| Capacitación Adicional (por hora) | Gs. __________ /hora |
| Visita Técnica Presencial | Gs. __________ |
| Desarrollo Personalizado | A cotizar |

---

## 6. Garantías

- ✅ **30 días de garantía** en la implementación
- ✅ **Actualizaciones incluidas** durante el período de soporte
- ✅ **Respaldo de datos** antes de cada actualización
- ✅ **Compatibilidad SIFEN** garantizada con normativas del SET

---

## 7. Condiciones Comerciales

### Forma de Pago
- Implementación: 50% al inicio, 50% al finalizar
- Mantenimiento/Suscripción: Pago mensual anticipado

### Vigencia de la Propuesta
Esta propuesta tiene validez de **30 días** a partir de la fecha de emisión.

---

## 8. Datos de Contacto

**Empresa:** _________________________________

**Dirección:** _________________________________

**Teléfono:** _________________________________

**Email:** _________________________________

**WhatsApp:** _________________________________

---

*Propuesta generada el: ____/____/________*

*Firma del Representante:* _________________________________

---

## Anexo: Capturas de Pantalla

*(Agregar capturas de las pantallas principales del sistema)*

1. Dashboard Principal
2. Punto de Venta
3. Gestión de Productos
4. Factura Electrónica
5. Informes

