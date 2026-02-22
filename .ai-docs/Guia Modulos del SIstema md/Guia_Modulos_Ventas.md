# 📖 Guía del Módulo de Ventas - SistemIA

> **Documentación técnica para el Asistente IA (chatbootia)**  
> Última actualización: Enero 2026

---

## 📋 Descripción General

El **Módulo de Ventas** es el núcleo comercial de SistemIA, diseñado para gestionar todo el ciclo de ventas, desde presupuestos hasta facturas electrónicas con integración SIFEN (Sistema Integrado de Facturación Electrónica de Paraguay).

### Características Principales
- **Facturación Electrónica SIFEN** con generación de CDC, QR y envío al SET
- **Multi-moneda** con conversión automática (PYG, USD, BRL)
- **Venta por Paquete/Unidad** con conversión inteligente
- **Sistema de Descuentos** configurable por producto, categoría y global
- **Control de Lotes (FEFO)** con trazabilidad completa
- **Modo Farmacia** con Precio Ministerio y validaciones especiales
- **Presupuestos** convertibles a ventas
- **Composición de Caja** para pagos mixtos (efectivo + tarjeta + QR, etc.)
- **Impresión flexible** en Ticket (térmica 80mm) y A4 (KuDE)

---

## 🗃️ Modelo de Datos

### Entidad Principal: `Venta`
**Archivo:** `Models/Venta.cs`

#### Campos de Identificación
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdVenta` | int (PK) | ID interno auto-incremental |
| `TipoIngreso` | string(20) | **"VENTAS"** o **"PRESUPUESTO"** |
| `IdPresupuestoOrigen` | int? | FK si fue convertido desde presupuesto |
| `NroPedido` | string(50)? | Número de pedido de referencia externa |

#### Campos de Numeración Fiscal
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Establecimiento` | string(3) | Código de establecimiento (ej: "001") |
| `PuntoExpedicion` | string(3) | Punto de expedición (ej: "001") |
| `NumeroFactura` | string(7) | Número secuencial (ej: "0000123") |
| `Timbrado` | string(8)? | Número de timbrado vigente |
| `Serie` | int? | Serie del timbrado |

#### Campos SIFEN (Facturación Electrónica)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `CDC` | string(64)? | **Código de Control del Documento** (44 dígitos) |
| `CodigoSeguridad` | string(9)? | Código aleatorio de 9 dígitos para el CDC |
| `EstadoSifen` | string(30)? | PENDIENTE, ENVIADO, ACEPTADO, RECHAZADO, CANCELADO |
| `MensajeSifen` | string? | Mensaje de respuesta del SET |
| `XmlCDE` | string? | XML firmado del documento electrónico |
| `IdLote` | string(50)? | ID del lote enviado al SET |
| `UrlQrSifen` | string? | URL completa del QR con `cHashQR` (dCarQR) |
| `FechaEnvioSifen` | DateTime? | Fecha/hora de envío al SET |

#### Campos Multi-Moneda
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdMoneda` | int? | FK a tabla Monedas |
| `EsMonedaExtranjera` | bool | True si moneda ≠ PYG |
| `CambioDelDia` | decimal(18,4)? | Tipo de cambio aplicado |
| `SimboloMoneda` | string(10)? | Símbolo (Gs, $, R$) |

#### Campos de Condición de Pago
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `FormaPago` | string(30)? | Contado, Crédito |
| `CodigoCondicion` | int? | 1=Contado, 2=Crédito |
| `MedioPago` | string(30)? | Efectivo, Tarjeta, Cheque, Transferencia, QR |
| `IdTipoPago` | int? | FK a TiposPago |
| `Plazo` | int? | Días de plazo para crédito |
| `NumeroCuotas` | int? | Cantidad de cuotas |
| `FechaVencimiento` | DateTime? | Fecha vencimiento crédito/presupuesto |
| `CreditoSaldo` | decimal(18,4)? | Saldo pendiente de pago |

#### Campos de Presupuesto
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ValidezDias` | int? | Días de validez del presupuesto |
| `ValidoHasta` | DateTime? | Fecha límite de validez |

#### Campos de Totales
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Subtotal` | decimal(18,4) | Suma de importes |
| `TotalDescuento` | decimal(18,4) | Total de descuentos aplicados |
| `TotalIVA10` | decimal(18,4) | Total IVA 10% |
| `TotalIVA5` | decimal(18,4) | Total IVA 5% |
| `TotalExenta` | decimal(18,4) | Total exentas |
| `Total` | decimal(18,4) | **Total general de la venta** |

#### Campos de Control
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Fecha` | DateTime | Fecha/hora de la venta |
| `FechaCaja` | DateTime? | Fecha de caja para cierre |
| `Turno` | string(10)? | Turno de trabajo |
| `Estado` | string(20) | **Borrador**, **Confirmado**, **Anulado** |
| `IdCliente` | int? | FK a Clientes |
| `IdSucursal` | int | FK a Sucursales |
| `IdCaja` | int | FK a Cajas |
| `IdUsuario` | int? | FK a Usuarios |
| `Observaciones` | string? | Notas adicionales |

---

### Entidad de Detalle: `VentaDetalle`
**Archivo:** `Models/VentaDetalle.cs`

#### Campos Principales
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdVentaDetalle` | int (PK) | ID auto-incremental |
| `IdVenta` | int (FK) | Referencia a Venta |
| `IdProducto` | int | FK a Productos |
| `Cantidad` | decimal(18,4) | Cantidad vendida (puede ser decimal) |
| `PrecioUnitario` | decimal(18,4) | Precio por unidad |
| `Importe` | decimal(18,4) | Total de la línea |

#### Campos de IVA
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IVA10` | decimal(18,4) | Monto IVA 10% |
| `IVA5` | decimal(18,4) | Monto IVA 5% |
| `Grabado10` | decimal(18,4) | Base gravada 10% |
| `Grabado5` | decimal(18,4) | Base gravada 5% |
| `Exenta` | decimal(18,4) | Monto exento |

#### Campos de Costo (para rentabilidad)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `CostoUnitario` | decimal(18,4)? | Costo al momento de la venta |

#### Campos de Farmacia
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PrecioMinisterio` | decimal(18,4)? | Precio regulado por Ministerio |
| `PorcentajeDescuento` | decimal(18,4)? | % de descuento aplicado |

#### Campos de Modo Paquete (Persistidos)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `ModoIngresoPersistido` | string(20)? | **"paquete"** o **"unidad"** |
| `CantidadPorPaqueteMomento` | decimal(18,4)? | Unidades por paquete al momento |
| `PrecioPaqueteMomento` | decimal(18,4)? | Precio del paquete al momento |
| `PrecioMinisterioPaqueteMomento` | decimal(18,4)? | Precio Ministerio del paquete |

#### Campos de Tipo de Cambio por Línea
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `CambioDelDia` | decimal(18,4)? | TC aplicado a esta línea |

#### Campos de Control de Lotes (FEFO)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdProductoLote` | int? | FK a ProductosLotes |
| `NumeroLoteMomento` | string? | Número de lote descontado |
| `FechaVencimientoLoteMomento` | DateTime? | Fecha vencimiento del lote |

#### Propiedades Auxiliares (NotMapped)
| Propiedad | Tipo | Uso |
|-----------|------|-----|
| `CantidadPorPaquete` | int? | Unidades/paquete del producto |
| `PermiteVentaPorUnidad` | bool | Si permite vender unidades sueltas |
| `ModoIngreso` | string | Modo seleccionado en UI |
| `CantidadIngresada` | decimal | Cantidad digitada por usuario |
| `PermiteDecimal` | bool | Si permite cantidades decimales |

---

## 📄 Páginas del Módulo

### 1. Registro de Ventas (`/ventas`)
**Archivo:** `Pages/Ventas.razor`

#### Funcionalidades

##### Encabezado de Documento
- **Sucursal y Caja**: Muestra actual, permite cambio si tiene permiso
- **Fecha de Caja**: Fecha operativa del sistema
- **Turno**: Turno de trabajo activo
- **Tipo de Documento**: Badge indicando VENTA o PRESUPUESTO
- **Timbrado**: Número de timbrado vigente

##### Búsqueda de Cliente
- **Autocompletado** por RUC o Razón Social
- **Búsqueda SIFEN** automática al escribir RUC (consulta al SET)
- **Creación rápida** de cliente desde modal
- **Campo Email** para envío automático de factura

##### Datos Fiscales
- **Establecimiento**: Código del punto de venta
- **Punto de Expedición**: Código de la caja
- **Número**: Secuencial automático o manual

##### Configuración de Venta
- **Tipo de Documento de Operación**: Selección del tipo
- **Tipo de Pago**: Contado / Crédito
- **Campos de Crédito** (si aplica): Número de cuotas, Plazo (días), Fecha vencimiento

##### Selector VENTAS / PRESUPUESTO
- **VENTAS**: Genera factura y descuenta stock
- **PRESUPUESTO**: No descuenta stock, tiene validez configurable
- **Validez días**: Solo para presupuestos
- **Válido hasta**: Fecha límite del presupuesto

##### Multi-Moneda
- **Selector de Moneda**: PYG, USD, BRL, etc.
- **Tipo de Cambio**: Editable cuando es moneda extranjera

##### Búsqueda de Productos
- **Autocompletado** con sugerencias
- **Stock disponible** mostrado en sugerencias
- **Imagen del producto** en panel lateral

##### Modo Paquete/Unidad
Aparece cuando el producto tiene `CantidadPorPaquete > 1`:
- **Por Paquete**: Vende cajas/packs completos
- **Por Unidad**: Vende unidades sueltas
- **Conversión automática**: Muestra equivalencia (ej: "1 paq = 12u")

##### Entrada de Cantidad y Precio
- **Cantidad**: Acepta decimales si el producto lo permite
- **Precio**: Editable según permiso `PuedeEditarPrecio`
- **Descuento**: Visible si `PermitirVenderConDescuento = true`

##### Vista Previa de Línea (antes de agregar)
- Gravado 10% / 5%
- IVA 10% / 5%
- Exenta
- Total de línea
- Tipo de cambio aplicado

##### Tabla de Detalles
Columnas dinámicas según configuración:
| Columna | Condición | Contenido |
|---------|-----------|-----------|
| Producto | Siempre | Nombre + badge modo (paquete/unidad) |
| Lote | ModoFarmacia | Número lote + fecha vencimiento |
| Cant. | Siempre | Paquetes / Unidades |
| Precio | Siempre | Precio unitario o por paquete |
| P.Min | ModoFarmacia | Precio Ministerio |
| Desc.% | PermitirDescuento | Porcentaje aplicado |
| Importe | Siempre | Total de línea |
| TC | Multi-moneda | Tipo de cambio |
| Quitar | Editable | Botón eliminar línea |

##### Panel de Totales
- Gravado 10%
- Gravado 5%
- IVA 10%
- IVA 5%
- Exentas
- **Total** (destacado)
- Monto en letras

##### Acciones
- **Guardar**: Confirma la venta (descuenta stock, genera número)
- **Limpiar**: Reinicia el formulario

##### Modal de Composición de Caja
Para ventas con pago mixto:
- **Medios disponibles**: Efectivo, Tarjeta, Cheque, Transferencia, QR
- **Multi-moneda**: Cada detalle puede ser en diferente moneda
- **Conversión automática** a Guaraníes
- **Panel de resumen**: Total Venta, Total Cobrado, Vuelto/Faltante

##### Modal de Nuevo Cliente Rápido
- Tipo de documento (CI, RUC, Pasaporte)
- RUC/CI con cálculo automático de DV
- Búsqueda en SIFEN (valida existencia)
- Razón Social, Teléfono, Email

##### Modal de Producto Vencido
- Bloquea venta de productos vencidos
- Muestra fecha de vencimiento

##### Modal de Receta Médica
Para productos controlados:
- Número de Registro
- Fecha de la Receta
- Nombre del Médico
- Nombre del Paciente

---

### 2. Explorador de Ventas (`/ventas/explorar`)
**Archivo:** `Pages/VentasExplorar.razor`

#### Panel de Filtros
| Filtro | Tipo | Descripción |
|--------|------|-------------|
| Nº Interno | number | ID de venta |
| Cliente | text | RUC o Razón Social |
| Desde | date | Fecha inicial |
| Hasta | date | Fecha final |
| Estado | select | Borrador, Confirmado, Anulado |
| Estado SIFEN | select | PENDIENTE, ENVIADO, ACEPTADO, RECHAZADO |

#### Botones de Exportación
- **CSV**: Exporta listado con separador `;`
- **XLSX**: Excel con formato y anchos ajustados
- **Imprimir**: Listado A4 con logo y encabezado

#### Resumen Superior
- Total de registros encontrados
- Suma Total Gs
- Suma Total USD

#### Tabla de Resultados
| Columna | Contenido |
|---------|-----------|
| Nº Int. | ID interno |
| Fecha | Fecha de la venta |
| Cliente | Nombre + RUC |
| Documento | Número completo + Timbrado + IdLote |
| Moneda | Badge (PYG, USD, BRL) |
| Cambio | Tipo de cambio si moneda extranjera |
| Total Gs | Monto en Guaraníes |
| Total $ | Monto en Dólares |
| Estado | Badges múltiples |

#### Badges de Estado
| Badge | Color | Significado |
|-------|-------|-------------|
| Anulado | Rojo | Venta anulada |
| Confirmado | Verde | Venta válida |
| Borrador | Gris | Sin confirmar |
| Caja | Amarillo | Tiene composición de caja |

#### Badges SIFEN
| Badge | Color | Significado |
|-------|-------|-------------|
| ACEPTADO | Verde | Aprobado por SET |
| ENVIADO | Amarillo | Esperando respuesta |
| RECHAZADO | Rojo | Rechazado por SET |
| CDC | Info | Link al código de control |

#### Acciones por Fila

##### Grupo 1: Ver e Imprimir
| Acción | Icono | Función |
|--------|-------|---------|
| Ver | eye | Abre la venta en modo lectura |
| Imprimir A4 | printer | KuDE para factura electrónica |
| Ticket | receipt | Vista previa de ticket térmica |

##### Grupo 2: SIFEN
| Acción | Icono | Condición | Función |
|--------|-------|-----------|---------|
| Ver QR | qr-code | Tiene CDC | Muestra QR escaneableConsultar SIFEN | cloud-download | Tiene IdLote o CDC | Consulta estado en SET |
| Enviar SIFEN | cloud-upload | Confirmada + No enviada | Envía al SET |
| Ver XML | code | Tiene CDC | Muestra XML firmado |
| Reenviar correo | envelope | Tiene email cliente | Reenvía factura por correo |

##### Grupo 3: Gestión
| Acción | Icono | Condición | Función |
|--------|-------|-----------|---------|
| Composición | cash-stack | Tiene composición | Ver detalles de pago |
| Anular | x-circle | Confirmada | Anula venta (devuelve stock) |
| Recuperar | arrow-counterclockwise | Anulada | Recupera venta anulada |
| Eliminar | trash3 | Sin CDC aceptado | Elimina permanentemente |

#### Paginación
- Selector de registros por página
- Navegación: Primera, Anterior, Número, Siguiente, Última

#### Modales del Explorador

##### Modal QR
- Código QR escaneableURL de consulta SIFEN
- CDC completo

##### Modal Resultado SIFEN
- Icono de éxito (verde) o error (rojo)
- Código de respuesta
- Mensaje del SET
- CDC si fue aprobado

##### Modal Consulta SIFEN
- SOAP enviado
- Respuesta recibida
- Botones para copiar

##### Modal Composición de Caja
- Tabla con detalles de pago
- Total Venta vs Total Pagado
- Vuelto calculado

##### Modal Restricción Anulación
Muestra restricciones cuando no se puede anular:
- Composición de caja existente
- Cobros de cuotas confirmados
- Links para ir a resolver

---

## 🔄 Flujos de Negocio

### Flujo de Venta al Contado
```
1. Seleccionar Cliente (o crear nuevo)
2. Configurar Tipo Pago = Contado
3. Agregar productos al detalle
4. Revisar totales
5. Click "Guardar"
   ├── Valida stock disponible
   ├── Descuenta stock (movimiento tipo 2 = Salida)
   ├── Descuenta lotes FEFO si aplica
   ├── Genera número de factura
   ├── Estado = "Confirmado"
   └── Si es Factura Electrónica → Modal Composición Caja
6. Impresión automática según formato de caja
7. Si cliente tiene email + EnviarFacturaPorCorreo → envío automático
```

### Flujo de Venta a Crédito
```
1. Seleccionar Cliente (requerido)
2. Configurar Tipo Pago = Crédito
3. Ingresar: Número de Cuotas, Plazo, Fecha Vencimiento
4. Agregar productos
5. Click "Guardar"
   ├── Mismas validaciones que contado
   ├── Crea registro en CuentasPorCobrar
   ├── Genera cuotas según configuración
   └── CreditoSaldo = Total
6. Cliente puede pagar cuotas desde módulo Cobros
```

### Flujo de Presupuesto
```
1. Cambiar TipoIngreso a "PRESUPUESTO"
2. Configurar Validez (días o fecha)
3. Agregar productos
4. Click "Guardar"
   ├── NO descuenta stock
   ├── NO genera número fiscal
   └── Estado = "Borrador" o "Confirmado"
5. Desde el explorador: Convertir a Venta
   ├── Crea nueva venta con IdPresupuestoOrigen
   ├── Copia todos los detalles
   └── Ahora SÍ descuenta stock
```

### Flujo SIFEN (Facturación Electrónica)
```
1. Venta Confirmada (caja tipo "Factura Electrónica")
2. Click "Enviar SIFEN" en explorador
   ├── Validación previa (endpoint /admin/de/validar)
   ├── Genera CDC (44 dígitos)
   ├── Construye XML del DE
   ├── Firma con certificado digital
   ├── Envía al SET (endpoint sync)
   └── Procesa respuesta
3. Si ACEPTADO:
   ├── Guarda CDC, IdLote, UrlQrSifen
   ├── EstadoSifen = "ACEPTADO"
   └── Muestra modal de éxito
4. Si RECHAZADO:
   ├── Guarda mensaje de error
   ├── EstadoSifen = "RECHAZADO"
   └── Muestra modal de error
```

### Flujo de Anulación
```
1. Click "Anular" en explorador
2. Validaciones:
   ├── ¿Tiene cobros confirmados? → Modal restricción
   ├── ¿Es Factura Electrónica ACEPTADA?
   │   ├── ¿Dentro de 48 horas? → Pide motivo → Envía Evento Cancelación SIFEN
   │   └── ¿Más de 48 horas? → No se puede anular, emitir NC
   └── Confirmación del usuario
3. Ejecución:
   ├── Devuelve stock (movimiento tipo 1 = Entrada)
   ├── Revierte lotes FEFO
   ├── Elimina composición de caja
   ├── Cancela cuenta por cobrar si existe
   └── Estado = "Anulado"
```

### Flujo de Eliminación
```
1. Click "Eliminar" en explorador
2. Validaciones:
   ├── ¿Estado = Anulado? → No permitido (stock ya devuelto)
   ├── ¿Tiene CDC ACEPTADO? → No permitido (registrada en SIFEN)
   ├── ¿Tiene CDC RECHAZADO? → SÍ permitido
   ├── ¿Tiene NC asociadas activas? → No permitido
   └── ¿Tiene cobros confirmados? → No permitido
3. Ejecución:
   ├── Devuelve stock
   ├── Revierte lotes
   ├── Elimina cobros anulados, cuotas, cuenta por cobrar
   ├── Elimina composición de caja
   ├── Elimina detalles y venta
   └── Retrocede contador si era última factura
```

---

## 🔧 Funcionalidades Especiales

### Modo Paquete vs Unidad
**Aplicable cuando:** `Producto.CantidadPorPaquete > 1`

| Modo | Cantidad digitada | Cantidad guardada | Precio mostrado |
|------|-------------------|-------------------|-----------------|
| Paquete | 2 paquetes | 24 unidades (2×12) | Precio×12 |
| Unidad | 5 unidades | 5 unidades | Precio×1 |

**Persistencia en detalle:**
- `ModoIngresoPersistido` = "paquete" o "unidad"
- `CantidadPorPaqueteMomento` = valor al momento de la venta
- `PrecioPaqueteMomento` = precio del paquete
- `Cantidad` = siempre en **unidades**

### Sistema de Descuentos

**Condiciones para habilitar campo de descuento:**
1. Configuración global `PermitirVenderConDescuento = true`
2. El producto tiene `PermiteDescuento = true`
3. El producto o su categoría tiene descuento configurado

**Validaciones:**
- El descuento no puede superar el máximo configurado
- Se aplica sobre el precio unitario
- Se guarda en `VentaDetalle.PorcentajeDescuento`

### Control de Lotes (FEFO)
**First Expired, First Out**

Cuando `Producto.ControlLote = true`:
1. Al agregar producto, el sistema:
   - Busca lotes con stock > 0
   - Ordena por fecha de vencimiento (más próximo primero)
   - Descuenta automáticamente del lote más antiguo
2. Se guarda en el detalle:
   - `IdProductoLote`
   - `NumeroLoteMomento`
   - `FechaVencimientoLoteMomento`
3. En Modo Farmacia se muestra badge con lote y vencimiento

### Composición de Caja (Pago Mixto)
Permite registrar múltiples medios de pago para una venta:

**Medios disponibles:**
- EFECTIVO
- TARJETA
- CHEQUE
- TRANSFERENCIA
- QR

**Cada detalle incluye:**
- Medio de pago
- Moneda
- Monto (en moneda seleccionada)
- MontoGs (convertido a Guaraníes)
- Número de comprobante (tarjeta, cheque, transferencia)

**Panel de resumen:**
- Total de la Venta
- Total Cobrado
- Vuelto (si cobrado > venta) o Faltante

### Precio Ministerio (Farmacia)
Para productos farmacéuticos con precio regulado:
- `Producto.PrecioMinisterio` define el precio máximo
- Se muestra columna adicional en grilla de detalles
- Validación que precio de venta ≤ precio ministerio

### Productos Controlados (Receta)
Para medicamentos que requieren receta médica:
- `Producto.RequiereReceta = true`
- Al agregar, se abre modal para capturar:
  - Número de Registro de Receta
  - Fecha de la Receta
  - Nombre del Médico
  - Nombre del Paciente
- Los datos se guardan para trazabilidad

---

## 📊 Estados del Sistema

### Estado de Venta
| Estado | Descripción | Stock | Número |
|--------|-------------|-------|--------|
| Borrador | En edición | No descontado | No generado |
| Confirmado | Venta válida | Descontado | Generado |
| Anulado | Venta anulada | Devuelto | Conservado |

### Estado SIFEN
| Estado | Descripción | Acción Disponible |
|--------|-------------|-------------------|
| (vacío) | No enviada | Enviar SIFEN |
| PENDIENTE | Esperando envío | Enviar SIFEN |
| ENVIADO | En procesamiento | Consultar SIFEN |
| ACEPTADO | Aprobado por SET | Ver QR, Consultar |
| RECHAZADO | Rechazado por SET | Corregir y reenviar |
| CANCELADO | Anulado en SIFEN | Solo consultar |

---

## 🖨️ Impresión

### Ticket (Térmica 80mm)
- Generación de bitmap para impresión directa
- Logo de empresa
- Datos fiscales completos
- Detalle de productos con modo paquete/unidad
- QR con CDC si es factura electrónica
- Mensaje promocional

### KuDE (A4 - Documento Electrónico)
- Formato según especificación SET
- QR con URL completa (UrlQrSifen)
- Código barras del CDC
- Todos los datos requeridos por SIFEN

### Envío por Correo
Cuando `Cliente.EnviarFacturaPorCorreo = true`:
- Se genera PDF de la factura
- Se envía automáticamente al email del cliente
- Usa configuración SMTP de la sucursal

---

## 🔐 Permisos del Módulo

| Permiso | Código | Descripción |
|---------|--------|-------------|
| Ver ventas | VIEW | Acceder al explorador |
| Crear ventas | CREATE | Registrar nuevas ventas |
| Editar ventas | EDIT | Modificar ventas en borrador |
| Eliminar ventas | DELETE | Eliminar ventas no fiscales |
| Anular ventas | ANULAR | Anular ventas confirmadas |
| Editar precio | EDIT_PRICE | Modificar precio en línea |
| Aplicar descuento | DISCOUNT | Campo de descuento visible |
| Cambiar caja | CHANGE_CAJA | Seleccionar otra caja |
| Enviar SIFEN | SIFEN_SEND | Botón enviar a SET |

---

## 🔗 Relaciones con Otros Módulos

| Módulo | Relación |
|--------|----------|
| **Clientes** | FK IdCliente, búsqueda SIFEN |
| **Productos** | FK IdProducto en detalles |
| **Inventario** | Descuento de stock al confirmar |
| **Lotes** | Control FEFO para farmacia |
| **Caja** | Numeración, composición de pagos |
| **CuentasPorCobrar** | Ventas a crédito generan cuenta |
| **Cobros** | Pagos de cuotas de crédito |
| **NotasCreditoVentas** | NC referenciando ventas |
| **TiposCambio** | Conversión multi-moneda |
| **SIFEN** | Facturación electrónica |
| **Correo** | Envío automático de factura |

---

## ⚠️ Consideraciones Importantes

### Restricciones de Anulación
- Ventas SIFEN ACEPTADAS solo pueden anularse dentro de 48 horas vía Evento Cancelación
- Después de 48 horas, solo puede emitirse Nota de Crédito
- Ventas con cobros confirmados requieren anular cobros primero

### Restricciones de Eliminación
- Ventas SIFEN ACEPTADAS no pueden eliminarse (requisito legal)
- Ventas RECHAZADAS sí pueden eliminarse
- Ventas anuladas no pueden eliminarse (stock ya devuelto)
- Ventas con NC activas requieren anular NC primero

### Stock Cero
- Productos físicos con stock 0 no pueden agregarse
- Se muestra modal informativo

### Productos Vencidos
- No se permite vender productos vencidos
- Se bloquea con modal de advertencia

### Formato de Número
- `Establecimiento-PuntoExpedicion-NumeroFactura`
- Ejemplo: `001-001-0000123`
- El número se genera automáticamente según caja

---

## 📚 Documentación Relacionada

- [SIFEN_DOCUMENTACION_COMPLETA.md](SIFEN_DOCUMENTACION_COMPLETA.md) - Integración SIFEN
- [Guia_Modulos_Productos.md](Guia_Modulos_Productos.md) - Gestión de productos
- [Guia_Modulos_Compras.md](Guia_Modulos_Compras.md) - Módulo de compras
- [copilot-instructions.md](../.github/copilot-instructions.md) - Instrucciones de desarrollo

---

*Documentación generada para referencia del Asistente IA (chatbootia) - Enero 2026*
