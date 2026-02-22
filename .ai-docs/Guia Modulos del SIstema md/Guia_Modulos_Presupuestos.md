# Guía del Módulo de Presupuestos (Cotizaciones)

## Descripción General

El sistema maneja **dos tipos de presupuestos** con propósitos diferentes:

1. **Presupuestos Comerciales** (`Presupuesto`): Cotizaciones de productos/servicios a clientes, que pueden convertirse en ventas.

2. **Presupuestos del Sistema** (`PresupuestoSistema`): Cotizaciones específicas del software SistemIA para clientes potenciales (venta del propio sistema).

---

## Parte 1: Presupuestos Comerciales (Cotizaciones a Clientes)

### Modelo de Datos

#### Entidad: `Presupuesto`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdPresupuesto` | int (PK) | Identificador único |
| `IdSucursal` | int (FK) | Sucursal que emite |
| `IdCliente` | int? (FK) | Cliente destinatario |
| `Fecha` | DateTime | Fecha de emisión |
| `NumeroPresupuesto` | string(20)? | Número de documento |

**Moneda:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdMoneda` | int? | Moneda del presupuesto |
| `SimboloMoneda` | string(4)? | Símbolo (Gs, $, R$) |
| `CambioDelDia` | decimal(18,4)? | Tipo de cambio aplicado |
| `EsMonedaExtranjera` | bool? | ¿Es moneda extranjera? |

**Totales y Validez:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Total` | decimal(18,4) | Total del presupuesto |
| `TotalEnLetras` | string(280)? | Total en palabras |
| `ValidezDias` | int? | Días de validez |
| `ValidoHasta` | DateTime? | Fecha de vencimiento |

**Estado:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Comentario` | string(280)? | Observaciones |
| `Estado` | string(20)? | Presupuesto, Convertido, Anulado |
| `IdVentaConvertida` | int? | Venta generada desde este presupuesto |

---

#### Entidad: `PresupuestoDetalle`

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdPresupuestoDetalle` | int (PK) | Identificador único |
| `IdPresupuesto` | int (FK) | Presupuesto padre |
| `IdProducto` | int (FK) | Producto cotizado |
| `Cantidad` | decimal(18,4) | Cantidad |
| `PrecioUnitario` | decimal(18,4) | Precio unitario |
| `Importe` | decimal(18,4) | Subtotal línea |

**IVA por Línea:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IVA10` | decimal(18,4) | IVA 10% de la línea |
| `IVA5` | decimal(18,4) | IVA 5% de la línea |
| `Exenta` | decimal(18,4) | Monto exento |
| `Grabado10` | decimal(18,4) | Base gravada 10% |
| `Grabado5` | decimal(18,4) | Base gravada 5% |
| `IdTipoIva` | int? | Tipo de IVA aplicado |
| `CambioDelDia` | decimal(18,4)? | Cambio al momento |

---

### Página: Explorador de Presupuestos

**Ruta:** `/presupuestos/explorar`  
**Permiso:** VIEW sobre `/ventas`

**Filtros:**
- RUC o Razón Social
- Fecha desde / hasta
- Estado (Todos, Presupuesto, Convertido, Anulado)

**Acciones por Registro:**
| Acción | Descripción |
|--------|-------------|
| Imprimir | Genera PDF/impresión del presupuesto |
| Convertir | Convierte a venta (navega a `/ventas?presupuesto={id}`) |

**Exportaciones:**
- CSV
- XLSX (Excel)

---

### Estados del Presupuesto Comercial

| Estado | Descripción | Siguiente |
|--------|-------------|-----------|
| Presupuesto | Cotización activa | Convertido o Anulado |
| Convertido | Generó una venta | Final |
| Anulado | Cancelado | Final |

---

### Flujo de Conversión a Venta

```
1. Presupuesto creado con estado "Presupuesto"
        ↓
2. Cliente acepta → Click "Convertir"
        ↓
3. Sistema navega a /ventas?presupuesto={id}
        ↓
4. Página de ventas carga datos del presupuesto:
   - Cliente
   - Productos y cantidades
   - Precios cotizados
        ↓
5. Venta se confirma → Presupuesto.IdVentaConvertida = IdVenta
        ↓
6. Estado cambia a "Convertido"
```

---

### Impresión de Presupuesto

El presupuesto se imprime en formato A4 con:

**Encabezado:**
- Logo de la empresa
- Nombre y RUC de la empresa
- Dirección y teléfono
- Título "PRESUPUESTO"
- Número y fecha
- Válido hasta (si aplica)

**Datos del Cliente:**
- Razón Social
- RUC
- Dirección
- Teléfono

**Detalle de Items:**
| Columna | Descripción |
|---------|-------------|
| # | Número de línea |
| Código | Código interno del producto |
| Descripción | Nombre del producto |
| Cantidad | Cantidad cotizada |
| Precio Unit. | Precio unitario |
| Subtotal | Importe de línea |

**Totales:**
- Total Exentas (si hay)
- Total Gravadas 5% (si hay)
- Total Gravadas 10% (si hay)
- IVA 5% (si hay)
- IVA 10% (si hay)
- **TOTAL GENERAL**

---

## Parte 2: Presupuestos del Sistema (Cotizaciones de SistemIA)

### Descripción

Este módulo permite generar **cotizaciones profesionales del software SistemIA** para clientes potenciales. Incluye:
- Precios de licencia (contado y leasing)
- Costos de implementación
- Módulos incluidos
- Costos adicionales opcionales

---

### Modelo de Datos

#### Entidad: `PresupuestoSistema`

**Datos del Cliente Potencial:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdPresupuesto` | int (PK) | Identificador único |
| `NombreCliente` | string(200) | Empresa/Razón Social |
| `RucCliente` | string(20)? | RUC del prospecto |
| `DireccionCliente` | string(300)? | Dirección |
| `TelefonoCliente` | string(50)? | Teléfono |
| `EmailCliente` | string(150)? | Email |
| `ContactoCliente` | string(100)? | Nombre del contacto |
| `CargoContacto` | string(100)? | Cargo del contacto |

**Numeración:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `NumeroPresupuesto` | int | Número correlativo |
| `FechaEmision` | DateTime | Fecha de emisión |
| `FechaVigencia` | DateTime | Válido hasta (default: +30 días) |

**Precios Contado (Licencia Única):**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PrecioContadoUsd` | decimal(18,2) | Licencia en USD |
| `PrecioContadoGs` | decimal(18,0) | Licencia en Gs |
| `TipoCambio` | decimal(18,2) | Tipo de cambio USD→Gs |
| `CostoImplementacionUsd` | decimal(18,2) | Implementación USD |
| `CostoImplementacionGs` | decimal(18,0) | Implementación Gs |
| `CostoCapacitacionUsd` | decimal(18,2) | Capacitación USD |
| `CostoCapacitacionGs` | decimal(18,0) | Capacitación Gs |
| `HorasCapacitacion` | int | Horas incluidas (default: 8) |

**Precios Leasing/Suscripción:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `PrecioLeasingMensualUsd` | decimal(18,2) | Cuota mensual USD |
| `PrecioLeasingMensualGs` | decimal(18,0) | Cuota mensual Gs |
| `CantidadCuotasLeasing` | int | Cantidad de cuotas (0=ilimitado) |
| `EntradaLeasingUsd` | decimal(18,2) | Entrada USD |
| `EntradaLeasingGs` | decimal(18,0) | Entrada Gs |

**Mantenimiento Mensual:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `CostoMantenimientoMensualUsd` | decimal(18,2) | Soporte mensual USD |
| `CostoMantenimientoMensualGs` | decimal(18,0) | Soporte mensual Gs |

**Costos Adicionales/Opcionales:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `CostoSucursalAdicionalUsd/Gs` | decimal | Por sucursal extra |
| `CostoUsuarioAdicionalUsd/Gs` | decimal | Por usuario extra |
| `CostoHoraDesarrolloUsd/Gs` | decimal | Desarrollo personalizado |
| `CostoVisitaTecnicaUsd/Gs` | decimal | Por visita técnica |

**Módulos Incluidos (flags booleanos):**
| Campo | Default | Descripción |
|-------|---------|-------------|
| `ModuloVentas` | true | Módulo de ventas |
| `ModuloCompras` | true | Módulo de compras |
| `ModuloInventario` | true | Stock e inventario |
| `ModuloClientes` | true | Gestión de clientes |
| `ModuloCaja` | true | Cierres de caja |
| `ModuloSifen` | true | Facturación electrónica |
| `ModuloInformes` | true | Reportes |
| `ModuloUsuarios` | true | Usuarios y permisos |
| `ModuloCorreo` | true | Envío de emails |
| `ModuloAsistenteIA` | true | Chatbot integrado |

**Cantidades Incluidas:**
| Campo | Default | Descripción |
|-------|---------|-------------|
| `CantidadSucursalesIncluidas` | 1 | Sucursales en licencia |
| `CantidadUsuariosIncluidos` | 5 | Usuarios en licencia |
| `CantidadCajasIncluidas` | 2 | Cajas en licencia |

**Condiciones:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Observaciones` | string(2000)? | Notas adicionales |
| `CondicionesPago` | string(2000)? | Default: "50% inicio, 50% fin" |
| `Garantias` | string(1000)? | Default: "30 días garantía..." |

**Estado:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Estado` | string(20) | Borrador, Enviado, Aceptado, Rechazado, Vencido |
| `FechaEnvio` | DateTime? | Cuando se envió al cliente |
| `FechaRespuesta` | DateTime? | Cuando respondió el cliente |

**Auditoría:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `FechaCreacion` | DateTime | Fecha de creación |
| `FechaModificacion` | DateTime | Última modificación |
| `UsuarioCreacion` | string(50)? | Usuario que creó |
| `IdSucursal` | int | Sucursal emisora |

---

#### Entidad: `PresupuestoSistemaDetalle`

Permite agregar ítems personalizados al presupuesto.

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdDetalle` | int (PK) | Identificador único |
| `IdPresupuesto` | int (FK) | Presupuesto padre |
| `NumeroLinea` | int | Orden de la línea |
| `Descripcion` | string(300) | Descripción del ítem |
| `DescripcionAdicional` | string(1000)? | Detalle extendido |
| `Codigo` | string(50)? | Código del ítem |
| `Cantidad` | decimal(18,4) | Cantidad (default: 1) |
| `Unidad` | string(20) | Unidad de medida |
| `PrecioUnitarioUsd` | decimal(18,2) | Precio USD |
| `PrecioUnitarioGs` | decimal(18,0) | Precio Gs |
| `PorcentajeDescuento` | decimal(5,2) | % descuento |
| `SubtotalUsd` | decimal(18,2) | Subtotal calculado USD |
| `SubtotalGs` | decimal(18,0) | Subtotal calculado Gs |
| `TipoItem` | string(30) | Producto, Servicio, Hardware, Otro |
| `EsOpcional` | bool | ¿Es opcional? |
| `Incluido` | bool | ¿Incluir en total? |

**Métodos:**
```csharp
void CalcularSubtotales()
void CalcularPrecioGs(decimal tipoCambio)
```

---

### Página: Presupuestos del Sistema

**Ruta:** `/configuracion/presupuestos-sistema`  
**Protección:** Super Administrador (SaProtection)

**Funcionalidades:**

1. **Listado de Presupuestos:**
   - Número, Cliente, Fecha, Vigencia
   - Estado con badge de color
   - Total contado y leasing

2. **Formulario de Edición (secciones):**

| Sección | Contenido |
|---------|-----------|
| Datos del Cliente | Empresa, RUC, Dirección, Email, Contacto |
| Precio Contado | Licencia + Implementación + Capacitación |
| Modo Leasing | Cuotas, entrada, cantidad cuotas |
| Mantenimiento | Soporte mensual |
| Costos Adicionales | Sucursales, usuarios, desarrollo extra |
| Módulos Incluidos | Checkboxes de módulos |
| Cantidades | Sucursales, usuarios, cajas incluidas |
| Observaciones | Condiciones de pago, garantías |

3. **Cálculo Automático:**
   - USD → Gs (o viceversa) según tipo de cambio
   - Botón para obtener cotización actual del dólar
   - Totales contado y leasing calculados

4. **Acciones:**
   - Guardar borrador
   - Vista previa PDF
   - Enviar por correo
   - Duplicar presupuesto
   - Cambiar estado

---

### Estados del Presupuesto Sistema

| Estado | Badge | Descripción |
|--------|-------|-------------|
| Borrador | Gris | En edición |
| Enviado | Cyan | Enviado al cliente |
| Aceptado | Verde | Cliente aceptó |
| Rechazado | Rojo | Cliente rechazó |
| Vencido | Naranja | Pasó fecha de vigencia |

---

### Cálculos de Totales

**Total Contado:**
```
TotalContadoUsd = PrecioContadoUsd + CostoImplementacionUsd + CostoCapacitacionUsd
TotalContadoGs = TotalContadoUsd × TipoCambio
```

**Total Leasing:**
```
Si CantidadCuotasLeasing > 0:
    TotalLeasingUsd = EntradaLeasingUsd + (PrecioLeasingMensualUsd × CantidadCuotasLeasing)
Si CantidadCuotasLeasing = 0:
    Es suscripción continua (mostrar cuota mensual)
```

---

### Generación de PDF

**Servicio:** `PdfPresupuestoSistemaService`

El PDF incluye:
1. Logo y datos de la empresa
2. Datos del cliente potencial
3. Tabla de precios (contado vs leasing)
4. Módulos incluidos con checkmarks
5. Cantidades incluidas
6. Costos adicionales/opcionales
7. Condiciones de pago
8. Garantías
9. Firma y fecha

---

### Envío por Correo

El presupuesto puede enviarse al cliente por email:
- Adjunta PDF generado
- Usa configuración de correo del sistema
- Registra fecha de envío
- Cambia estado a "Enviado"

---

## Integración entre Módulos

### Presupuesto Comercial → Venta

Cuando se convierte un presupuesto comercial a venta:

1. Se crea nueva venta en página `/ventas`
2. Se cargan automáticamente:
   - Cliente del presupuesto
   - Todos los productos del detalle
   - Precios cotizados
   - Moneda y tipo de cambio
3. Al confirmar venta:
   - `Presupuesto.IdVentaConvertida = Venta.IdVenta`
   - `Presupuesto.Estado = "Convertido"`

### Presupuesto Sistema → Cliente (Futuro)

El presupuesto del sistema es para prospectos. Si se acepta:
- Se puede crear cliente en el sistema
- Se registra como venta del servicio
- Se agenda implementación

---

## Permisos Requeridos

### Presupuestos Comerciales

| Acción | Módulo | Permiso |
|--------|--------|---------|
| Ver listado | `/ventas` | VIEW |
| Imprimir | `/ventas` | VIEW |
| Convertir a venta | `/ventas` | CREATE |

### Presupuestos Sistema

| Acción | Requisito |
|--------|-----------|
| Acceso total | Super Administrador (SaProtection) |

---

## Resumen de Rutas

| Ruta | Página | Tipo |
|------|--------|------|
| `/presupuestos/explorar` | Explorador de presupuestos comerciales | Comercial |
| `/ventas?presupuesto={id}` | Convertir presupuesto a venta | Comercial |
| `/configuracion/presupuestos-sistema` | Gestión presupuestos del sistema | Sistema |
| `/presupuesto-sistema/pdf/{id}` | PDF de presupuesto sistema | Sistema |

---

## Diagrama de Relaciones

```
Presupuesto (Comercial)
    ├── IdSucursal → Sucursal
    ├── IdCliente → Cliente
    ├── IdMoneda → Moneda
    ├── IdVentaConvertida → Venta (si se convirtió)
    └── Detalles[]
            ├── IdProducto → Producto
            └── IdTipoIva → TipoIva

PresupuestoSistema
    ├── IdSucursal → Sucursal
    └── Detalles[]
            └── Items personalizados (hardware, servicios, etc.)
```

---

## Casos de Uso

### Crear Presupuesto Comercial

1. Desde módulo de ventas, crear nuevo presupuesto
2. Seleccionar cliente
3. Agregar productos con cantidades y precios
4. Definir validez
5. Guardar e imprimir para entregar al cliente

### Convertir Presupuesto Aceptado

1. Ir a `/presupuestos/explorar`
2. Buscar presupuesto por cliente o fecha
3. Click "Convertir" en el presupuesto aceptado
4. Verificar datos en pantalla de ventas
5. Confirmar venta
6. Presupuesto queda marcado como "Convertido"

### Crear Presupuesto del Sistema

1. Ir a `/configuracion/presupuestos-sistema`
2. Click "Nuevo Presupuesto"
3. Completar datos del prospecto
4. Configurar precios (contado y/o leasing)
5. Seleccionar módulos a incluir
6. Agregar items personalizados si es necesario
7. Vista previa del PDF
8. Enviar por correo al prospecto

### Seguimiento de Presupuestos Sistema

1. Ver listado con estados
2. Filtrar por estado (Enviado, Vencido)
3. Actualizar estado según respuesta del cliente
4. Duplicar presupuesto para nuevo prospecto similar
