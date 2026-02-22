# Guía del Módulo de Caja (Gestión de Caja y Turnos)

## Descripción General

El módulo de **Caja** gestiona la operación diaria del punto de venta: control de turnos, numeración de comprobantes (facturas, NC, recibos), cierres de caja y conciliación de efectivo con medios de pago.

---

## Modelo de Datos

### Entidad Principal: `Caja` (Configuración del Punto de Venta)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdCaja` | int (PK) | Identificador único de la caja |
| `Nombre` | string(50)? | Nombre descriptivo (ej: "Caja Principal") |
| `IdSucursal` | int? | Sucursal a la que pertenece |
| `CantTurnos` | int? | Cantidad de turnos por día (1, 2, 3...) |
| `TurnoActual` | int? | Turno activo actualmente |
| `FechaActualCaja` | DateTime? | Fecha operativa de la caja |
| `CajaActual` | int? | 1 = Caja activa, 0 = Inactiva |
| `bloquear_fechaCaja` | bool? | Bloquear cambio de fecha manual |

**Campos de Factura:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Nivel1` | string(3)? | Establecimiento (001-999) |
| `Nivel2` | string(3)? | Punto de expedición (001-999) |
| `FacturaInicial` | string(7)? | Número inicial (0000001) |
| `Serie` | int? | Serie actual |
| `Timbrado` | string(8)? | Número de timbrado |
| `VigenciaDel` | DateTime? | Vigencia desde |
| `VigenciaAl` | DateTime? | Vigencia hasta |
| `NombreImpresora` | string(40)? | Impresora asignada |
| `BloquearFactura` | bool? | Bloquear número |
| `Imprimir_Factura` | int? | ¿Imprimir automáticamente? |

**Campos de Remisión (R):**
- `Nivel1R`, `Nivel2R`, `FacturaInicialR`, `SerieR`, `TimbradoR`, etc.

**Campos de Nota de Crédito (NC):**
- `Nivel1NC`, `Nivel2NC`, `NumeroNC`, `SerieNC`, `TimbradoNC`, etc.

**Campos de Recibo:**
- `Nivel1Recibo`, `Nivel2Recibo`, `NumeroRecibo`, `SerieRecibo`, `TimbradoRecibo`, etc.

**Configuración de Impresión:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `TipoFacturacion` | string(20)? | ELECTRONICA, AUTOIMPRESOR |
| `FormatoImpresion` | string(20)? | TICKET, A4, MATRICIAL |
| `MostrarLogo` | bool? | Incluir logo en impresión |
| `AnchoTicket` | int? | Ancho en caracteres (default: 42) |

---

### Entidad: `CierreCaja` (Registro de Cierre)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdCierreCaja` | int (PK) | Identificador único |
| `IdCaja` | int (FK) | Caja que se cierra |
| `IdSucursal` | int? | Sucursal del cierre |
| `FechaCierre` | DateTime | Fecha/hora del cierre |
| `FechaCaja` | DateTime | Fecha operativa (fecha de caja) |
| `Turno` | int | Turno que se cierra |
| `UsuarioCierre` | string(100)? | Usuario que realiza el cierre |
| `Estado` | string(20) | Pendiente, Cerrado, Revisado |
| `Observaciones` | string(500)? | Notas del cierre |

**Totales del Turno:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `TotalVentasContado` | decimal(18,2) | Total ventas contado |
| `TotalVentasCredito` | decimal(18,2) | Total ventas crédito |
| `TotalCobrosCredito` | decimal(18,2) | Total cobros de cuotas |
| `TotalAnulaciones` | decimal(18,2) | Total ventas anuladas |
| `TotalNotasCredito` | decimal(18,2) | NC de Ventas (egresos) |
| `CantNotasCredito` | int | Cantidad de NC Ventas |
| `TotalNotasCreditoCompras` | decimal(18,2) | NC de Compras (ingresos) |
| `CantNotasCreditoCompras` | int | Cantidad de NC Compras |
| `TotalComprasEfectivo` | decimal(18,2) | Compras pagadas en efectivo |
| `CantComprasEfectivo` | int | Cantidad compras efectivo |

**Totales por Medio de Pago:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `TotalEfectivo` | decimal(18,2) | Total en efectivo |
| `TotalTarjetas` | decimal(18,2) | Total en tarjetas |
| `TotalCheques` | decimal(18,2) | Total en cheques |
| `TotalTransferencias` | decimal(18,2) | Total transferencias |
| `TotalQR` | decimal(18,2) | Total pagos QR |
| `TotalOtros` | decimal(18,2) | Otros medios |

**Conciliación:**
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `TotalEsperado` | decimal(18,2) | Total según sistema |
| `TotalEntregado` | decimal(18,2) | Total declarado por cajero |
| `Diferencia` | decimal(18,2) | Entregado - Esperado |

**Navegación:** `Entregas` → Colección de `EntregaCaja`

---

### Entidad: `EntregaCaja` (Detalle de Entregas por Medio)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdEntregaCaja` | int (PK) | Identificador único |
| `IdCierreCaja` | int (FK) | Cierre al que pertenece |
| `Medio` | MedioPago (enum) | Tipo de medio de pago |
| `IdMoneda` | int? | Moneda de la entrega |
| `MontoEsperado` | decimal(18,2) | Monto según sistema |
| `MontoEntregado` | decimal(18,2) | Monto declarado |
| `Diferencia` | decimal(18,2) | Entregado - Esperado |
| `ReceptorEntrega` | string(100)? | Quien recibe la entrega |
| `Observaciones` | string(300)? | Notas de la entrega |
| `DetalleCheques` | string(500)? | Detalle de cheques |
| `CantidadVouchers` | int? | Cantidad vouchers tarjeta |

---

### Enum: `MedioPago`

```csharp
public enum MedioPago
{
    Efectivo = 1,
    Tarjeta = 2,
    Cheque = 3,
    ChequeDia = 4,
    ChequeDiferido = 5,
    Transferencia = 6,
    QR = 7,
    Otro = 8
}
```

---

## Estructura de Páginas

### 1. Configuración de Cajas: `/configuracion/cajas` (CajasConfig.razor)

**Ruta:** `/configuracion/cajas`  
**Permiso:** VIEW sobre `/configuracion`

**Funcionalidades:**
- Listar todas las cajas del sistema
- Crear nueva caja
- Editar configuración de caja existente
- Establecer caja activa

**Secciones de Configuración:**

| Sección | Campos |
|---------|--------|
| Datos de la Caja | Nombre, Fecha actual, Bloquear fecha, Cant. Turnos, Turno actual |
| Datos de la Factura | Serie (Nivel1-Nivel2), Timbrado, Vigencia, Impresora, Tipo facturación, Formato impresión |
| Datos de la Remisión | Mismos campos con sufijo R |
| Datos de la Nota de Crédito | Mismos campos con sufijo NC |
| Datos del Recibo | Mismos campos con sufijo Recibo |

---

### 2. Cierre de Caja: `/caja/cierre` (CierreCajaPage.razor)

**Ruta:** `/caja/cierre`  
**Permiso:** VIEW sobre `/ventas`

**Funcionalidades:**
- Ver información de caja actual (fecha, turno, timbrado)
- Ver resumen del turno:
  - Ventas contado
  - Ventas crédito
  - Cobros de crédito
  - Anulaciones
  - Notas de crédito (ventas y compras)
  - Compras en efectivo
- Registrar entregas por medio de pago
- Calcular diferencias
- Cerrar turno y avanzar al siguiente
- Eliminar cierre existente (con contraseña)

**Cálculo Neto en Caja:**
```
Neto = Ventas Contado + Cobros Crédito 
     - NC Ventas - Compras Efectivo + NC Compras
```

---

### 3. Informe Resumen de Caja: `/informes/resumen-caja` (InformeResumenCaja.razor)

**Ruta:** `/informes/resumen-caja`  
**Permiso:** VIEW sobre `/reportes`

**Filtros:**
- Fecha desde / hasta
- Caja específica
- Turno
- Usuario

**Secciones del Informe:**

| Sección | Descripción |
|---------|-------------|
| Ingresos | Ventas contado, Ventas crédito, Cobros, Anulaciones |
| NC Ventas | Notas de crédito (devoluciones) - resta |
| Egresos | Compras contado, Compras crédito, Pagos a proveedores |
| NC Compras | Crédito del proveedor - suma |
| Efectivo Esperado | Cálculo detallado de efectivo neto |
| Desglose Medios | Por tipo de medio de pago |

**Exportaciones:**
- Excel (XLSX)
- CSV
- Imprimir Ticket (térmica)
- Imprimir A4
- Enviar por correo

---

### 4. Historial de Cierres: `/caja/historial` (HistorialCierresCaja.razor)

**Ruta:** `/caja/historial`  
**Permiso:** VIEW sobre `/caja/cierre`

**Funcionalidades:**
- Ver todos los cierres realizados
- Filtrar por fecha, caja, turno, usuario
- Ver detalle de entregas por cierre
- Ver diferencias reportadas

---

## Sistema de Turnos

### Concepto de Turno

Cada caja puede configurar múltiples turnos diarios. El sistema gestiona:

| Campo | Descripción |
|-------|-------------|
| `CantTurnos` | Cantidad de turnos por día (1, 2, 3...) |
| `TurnoActual` | Turno activo en este momento |
| `FechaActualCaja` | Fecha operativa |

### Flujo de Cierre de Turno

```
Turno 1 de 3 (Fecha: 15/01/2026)
        ↓
    [Cierre Turno 1]
        ↓
Turno 2 de 3 (Fecha: 15/01/2026)  ← Mismo día
        ↓
    [Cierre Turno 2]
        ↓
Turno 3 de 3 (Fecha: 15/01/2026)
        ↓
    [Cierre Turno 3]
        ↓
Turno 1 de 3 (Fecha: 16/01/2026)  ← Nuevo día
```

### Reglas de Avance

1. Al cerrar el **último turno** del día:
   - `TurnoActual = 1`
   - `FechaActualCaja = FechaActualCaja + 1 día`

2. Al cerrar turnos intermedios:
   - `TurnoActual = TurnoActual + 1`
   - `FechaActualCaja` no cambia

---

## Proceso de Cierre de Caja

### 1. Verificación Inicial

```
Sistema verifica:
├─ ¿Existe cierre previo para este período?
│   Sí → Mostrar alerta, permitir eliminar si tiene permisos
│   No → Continuar
└─ Cargar resumen del turno
```

### 2. Carga de Datos del Turno

El sistema calcula automáticamente:

| Concepto | Fuente |
|----------|--------|
| Ventas Contado | Ventas con FormaPago="CONTADO", no anuladas |
| Ventas Crédito | Ventas con FormaPago="CREDITO", no anuladas |
| Anulaciones | Ventas con Estado="Anulado" |
| Cobros Crédito | CobrosCuotas del turno |
| NC Ventas | NotasCreditoVentas confirmadas |
| NC Compras | NotasCreditoCompras confirmadas |
| Compras Efectivo | Compras con MedioPago="EFECTIVO" |

### 3. Desglose por Medio de Pago

El sistema agrupa desde `ComposicionCajaDetalle`:

| Medio | Cálculo |
|-------|---------|
| Efectivo | Suma efectivo recibido - Vuelto - NC Ventas - Compras Efectivo + NC Compras |
| Tarjetas | Suma pagos con tarjeta |
| Cheques | Suma cheques (día + diferidos) |
| Transferencias | Suma transferencias |
| QR | Suma pagos QR |

### 4. Registro de Entregas

El cajero ingresa por cada medio:
- Monto entregado
- Observaciones
- Detalle de cheques (si aplica)
- Cantidad de vouchers (si aplica)

Sistema calcula diferencia:
```
Diferencia = MontoEntregado - MontoEsperado
```

### 5. Confirmación de Cierre

Al confirmar:
1. Se crea registro `CierreCaja` con todos los totales
2. Se crean registros `EntregaCaja` por cada medio
3. Se avanza el turno de la caja
4. Se envía informe por correo (si está configurado)
5. Se registra tracking de acción

---

## Fórmulas de Cálculo

### Efectivo Neto en Caja

```
EfectivoNeto = 
    + EfectivoRecibidoVentas
    - VueltoEntregado
    + CobrosEfectivo
    - EfectivoEgresosCompras
    - EfectivoEgresosPagos
    - NotasCreditoVentas
    + NotasCreditoCompras
```

### Total Ingresos

```
TotalIngresos = VentasContado + CobrosCredito + NCCompras
```

### Total Egresos

```
TotalEgresos = ComprasContado + PagosProveedores + NCVentas
```

### Balance del Período

```
Balance = TotalIngresos - TotalEgresos
```

---

## Tipos de Facturación

| Tipo | Descripción |
|------|-------------|
| ELECTRONICA | Factura electrónica SIFEN con CDC y QR |
| AUTOIMPRESOR | Factura tradicional preimpresa |

La caja determina qué tipo de documento generar según `TipoFacturacion`.

---

## Formatos de Impresión

| Formato | Descripción | Uso típico |
|---------|-------------|------------|
| TICKET | Impresora térmica 80mm | Retail, puntos de venta rápidos |
| A4 | Impresora láser/inyección | Oficina, clientes corporativos |
| MATRICIAL | Impresora de puntos | Facturas carbonadas |

---

## Integración con Otros Módulos

### Ventas
- Las ventas se asocian a `IdCaja` y `Turno`
- El cierre de caja agrupa ventas por estos campos
- La numeración de facturas viene de la configuración de caja

### Compras
- Compras en efectivo afectan el resumen de caja
- Se filtran por `IdCaja` y `Turno`

### Cobros
- CobrosCuotas se asocian a `IdCaja` y `Turno`
- Aumentan el efectivo esperado

### Pagos a Proveedores
- PagosProveedores con efectivo se incluyen como egresos
- Se filtran por `IdCaja` y `Turno`

### Notas de Crédito
- NC Ventas: Disminuyen efectivo (devoluciones)
- NC Compras: Aumentan efectivo (crédito proveedor)

### ComposicionCaja
- Detalle de medios de pago por venta
- Fuente para desglose por medio de pago

---

## Estados del Cierre

| Estado | Descripción |
|--------|-------------|
| Pendiente | Cierre en proceso |
| Cerrado | Cierre confirmado |
| Revisado | Cierre auditado/verificado |

---

## Servicio: CajaService

**Ubicación:** `Services/CajaService.cs`

**Responsabilidades:**
- Obtener caja actual (con cache)
- Limpiar cache cuando se actualiza configuración

**Métodos:**
```csharp
Task<Caja?> ObtenerCajaActualAsync()
void LimpiarCache()
```

**Cache:** 5 minutos para evitar consultas repetidas.

---

## Permisos Requeridos

| Acción | Módulo | Permiso |
|--------|--------|---------|
| Ver configuración de cajas | `/configuracion` | VIEW |
| Editar configuración | `/configuracion` | EDIT |
| Ver cierre de caja | `/ventas` | VIEW |
| Cerrar turno | `/caja/cierre` | CREATE |
| Eliminar cierre | `/caja/cierre` | DELETE + EDIT |
| Ver informes | `/reportes` | VIEW |
| Ver historial | `/caja/cierre` | VIEW |

---

## Casos de Uso Comunes

### 1. Configurar Nueva Caja

1. Ir a `/configuracion/cajas`
2. Click "Nueva Caja"
3. Completar datos básicos (nombre, sucursal)
4. Completar datos de facturación (timbrado, serie, vigencia)
5. Configurar impresora y formato
6. Guardar

### 2. Cierre de Turno Normal

1. Al finalizar turno, ir a `/caja/cierre`
2. Verificar resumen automático (ventas, cobros, etc.)
3. Contar efectivo físico
4. Ingresar montos entregados por medio de pago
5. Revisar diferencias
6. Agregar observaciones si hay diferencias
7. Click "Cerrar Turno y Entregar"

### 3. Corregir Cierre Erróneo

1. Ir a `/caja/cierre`
2. Ver alerta de cierre existente
3. Click "Eliminar y Regenerar" (requiere permisos)
4. Ingresar contraseña para confirmar
5. El cierre se elimina
6. Realizar nuevo cierre con datos correctos

### 4. Consultar Histórico

1. Ir a `/caja/historial`
2. Filtrar por rango de fechas o caja
3. Ver detalle de cierres
4. Analizar diferencias y observaciones

### 5. Generar Informe de Período

1. Ir a `/informes/resumen-caja`
2. Seleccionar rango de fechas
3. Filtrar por caja/turno si es necesario
4. Ver resumen consolidado
5. Exportar a Excel o enviar por correo

---

## Diagrama de Relaciones

```
Caja (configuración)
    │
    ├── Sucursal
    │
    ├── Ventas (IdCaja, Turno)
    │
    ├── Compras (IdCaja, Turno)
    │
    ├── CobrosCuotas (IdCaja, Turno)
    │
    ├── PagosProveedores (IdCaja, Turno)
    │
    ├── NotasCreditoVentas (IdCaja, Turno)
    │
    └── CierresCaja[]
            │
            └── EntregasCaja[]
```

---

## Consideraciones Técnicas

### Cache de Caja Actual
- La caja actual se cachea por 5 minutos
- Se debe limpiar el cache al cambiar configuración
- Evita múltiples consultas en operaciones frecuentes

### Transaccionalidad
- El cierre de caja es atómico
- Si falla algún paso, se revierte todo
- Incluye creación de registros y actualización de turno

### Concurrencia
- Solo puede existir un cierre por caja/fecha/turno
- Se verifica antes de permitir nuevo cierre
- Eliminar cierre requiere contraseña del usuario

### Auditoría
- Se registra usuario que realiza el cierre
- Se guarda fecha/hora exacta
- Tracking de acciones para análisis

### Envío de Informes
- Al cerrar turno, se puede enviar informe por correo
- Configurable por destinatario y tipo de informe

---

## Validaciones

### Al Cerrar Turno:
- Caja debe estar activa y con fecha definida
- No debe existir cierre previo (o debe eliminarse)
- Usuario debe tener permisos

### Al Eliminar Cierre:
- Requiere permisos EDIT + DELETE
- Requiere contraseña del usuario actual
- Solo el último cierre puede eliminarse (integridad)

### Al Configurar Caja:
- Timbrado debe estar vigente
- Serie debe ser válida
- Formato de numeración correcto
