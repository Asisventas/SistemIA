# Guía del Módulo de Cobros (Cuentas por Cobrar)

## Descripción General

El módulo de **Cobros** gestiona las cuentas por cobrar originadas por ventas a crédito. Permite registrar cobros parciales o totales sobre cuotas, con soporte para múltiples medios de pago y monedas.

---

## Modelo de Datos

### Entidad Principal: `CuentaPorCobrar` (Crédito)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdCuentaPorCobrar` | int (PK) | Identificador único del crédito |
| `IdVenta` | int (FK) | Venta que originó el crédito |
| `IdCliente` | int (FK, Required) | Cliente deudor |
| `IdSucursal` | int (FK, Required) | Sucursal donde se originó |
| `FechaCredito` | DateTime | Fecha de otorgamiento del crédito |
| `FechaVencimiento` | DateTime? | Fecha de vencimiento general |
| `MontoTotal` | decimal(18,4) | Monto total del crédito |
| `SaldoPendiente` | decimal(18,4) | Saldo que falta pagar |
| `NumeroCuotas` | int | Cantidad de cuotas (default: 1) |
| `PlazoDias` | int | Plazo en días (default: 30) |
| `Estado` | string(20) | PENDIENTE, PAGADO, VENCIDO, CANCELADO |
| `IdMoneda` | int? | Moneda del crédito |
| `Observaciones` | string(280)? | Notas adicionales |
| `IdUsuarioAutorizo` | int? | Usuario que autorizó el crédito |

**Navegaciones:**
- `Venta` → Venta origen
- `Cliente` → Cliente deudor
- `Sucursal` → Sucursal
- `Moneda` → Moneda
- `Cuotas` → Colección de `CuentaPorCobrarCuota`
- `Cobros` → Colección de `CobroCuota`

---

### Entidad: `CuentaPorCobrarCuota` (Cuotas)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdCuota` | int (PK) | Identificador único de la cuota |
| `IdCuentaPorCobrar` | int (FK, Required) | Crédito al que pertenece |
| `NumeroCuota` | int | Número de cuota (1, 2, 3...) |
| `MontoCuota` | decimal(18,4) | Monto original de la cuota |
| `SaldoCuota` | decimal(18,4) | Saldo pendiente de la cuota |
| `FechaVencimiento` | DateTime | Fecha de vencimiento de esta cuota |
| `FechaPago` | DateTime? | Fecha en que se pagó completamente |
| `Estado` | string(20) | PENDIENTE, PAGADO, VENCIDO |
| `Observaciones` | string(280)? | Notas |

---

### Entidad: `CobroCuota` (Registro de Cobro)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdCobro` | int (PK) | Identificador único del cobro |
| `IdCuentaPorCobrar` | int (FK, Required) | Crédito que se está cobrando |
| `IdCliente` | int (FK, Required) | Cliente que paga |
| `FechaCobro` | DateTime | Fecha y hora del cobro |
| `MontoTotal` | decimal(18,4) | Monto total cobrado en esta transacción |
| `IdMoneda` | int? | Moneda del cobro |
| `CambioDelDia` | decimal(18,4)? | Tipo de cambio si es moneda extranjera |
| `Estado` | string(20) | CONFIRMADO, ANULADO |
| `Observaciones` | string(280)? | Notas del cobro |
| `IdUsuario` | int? | Usuario que registró |
| `IdCaja` | int? | Caja donde se realizó |
| `IdSucursal` | int? | Sucursal del cobro |
| `Turno` | int? | Turno de caja |
| `NumeroRecibo` | string(20)? | Número de comprobante |

**Navegaciones:**
- `CuentaPorCobrar` → Crédito
- `Cliente` → Cliente
- `Moneda` → Moneda
- `Usuario` → Usuario
- `Caja` → Caja
- `Sucursal` → Sucursal
- `Detalles` → Colección de `CobroDetalle`

---

### Entidad: `CobroDetalle` (Medios de Pago)

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `IdCobroDetalle` | int (PK) | Identificador único |
| `IdCobro` | int (FK, Required) | Cobro al que pertenece |
| `IdCuota` | int? (FK) | Cuota específica (para pagos parciales) |
| `MedioPago` | string(20, Required) | EFECTIVO, TARJETA, CHEQUE, TRANSFERENCIA, QR |
| `Monto` | decimal(18,4) | Monto de este medio de pago |
| `IdMoneda` | int? | Moneda del pago |
| `CambioDelDia` | decimal(18,4)? | Tipo de cambio si difiere |
| `BancoTarjeta` | string(50)? | Banco/Emisor de tarjeta |
| `Ultimos4Tarjeta` | string(4)? | Últimos 4 dígitos de tarjeta |
| `NumeroAutorizacion` | string(50)? | Código de autorización |
| `NumeroCheque` | string(50)? | Número de cheque |
| `BancoCheque` | string(50)? | Banco del cheque |
| `NumeroTransferencia` | string(50)? | Referencia de transferencia |
| `Observaciones` | string(280)? | Notas |

---

## Estructura de Páginas

### 1. Página Principal: `/cobros` (Cobros.razor)

**Ruta:** `/cobros`  
**Permiso:** VIEW sobre `/clientes/cobros`

**Características:**
- Vista de cuentas por cobrar pendientes
- Filtros por cliente y estado
- Expansión de cuotas por cuenta
- Modal de registro de cobro
- Soporte multi-moneda con conversión

---

### 2. Listado Histórico: `/cobros/listado` (CobrosListado.razor)

**Ruta:** `/cobros/listado`  
**Permiso:** VIEW sobre `/clientes/cobros`

**Características:**
- Historial de todos los cobros realizados
- Filtros: Fecha, Caja, Turno, Cliente
- Resumen por medio de pago
- Exportación a Excel
- Impresión de listado

---

### 3. Informe de Cuentas por Cobrar: `/informes/cuentas-por-cobrar`

**Ruta:** `/informes/cuentas-por-cobrar`  
**Permiso:** VIEW sobre `/informes/cuentas-por-cobrar`

**Características:**
- Listado de cuentas pendientes y cobradas
- Resumen: Total por cobrar, vencido, por vencer, al día
- Filtros por fecha, cliente, estado
- Exportación y envío por correo

---

## Flujo del Proceso de Cobro

### Origen del Crédito (en Ventas):

```
1. Venta a crédito confirmada
    ↓
2. Sistema crea CuentaPorCobrar:
    - MontoTotal = Total de la venta
    - SaldoPendiente = Total (sin pagos)
    - NumeroCuotas según condición de pago
    ↓
3. Sistema crea CuentaPorCobrarCuota (una por cuota):
    - MontoCuota = MontoTotal / NumeroCuotas
    - SaldoCuota = MontoCuota
    - FechaVencimiento calculada según plazo
    ↓
4. Cuenta aparece en lista de pendientes
```

### Registro de Cobro:

```
1. Seleccionar cuenta por cobrar
    ↓
2. Sistema muestra:
    - Saldo pendiente
    - Detalle de cuotas
    - Moneda del crédito
    ↓
3. Configurar cobro:
    - Seleccionar caja y turno
    - Fecha del cobro
    ↓
4. Agregar medios de pago:
    │
    ├─ EFECTIVO: Solo monto
    ├─ TARJETA: Banco, últimos 4, autorización
    ├─ CHEQUE: Número, banco
    ├─ TRANSFERENCIA: Número de transacción
    └─ QR: Solo monto
    ↓
5. Si moneda diferente:
    - Cargar tipo de cambio del día
    - Convertir montos automáticamente
    ↓
6. Confirmar cobro:
    │
    ├─ Crear CobroCuota
    ├─ Crear CobroDetalle por cada medio de pago
    ├─ Actualizar SaldoPendiente de CuentaPorCobrar
    ├─ Actualizar SaldoCuota de cuotas afectadas
    ├─ Si SaldoPendiente = 0 → Estado = PAGADO
    └─ Generar número de recibo
    ↓
7. Mostrar opciones:
    - Imprimir recibo
    - Nuevo cobro
```

---

## Modal de Cobro

### Información Mostrada:

| Campo | Descripción |
|-------|-------------|
| Cliente | Nombre del cliente deudor |
| Saldo Pendiente | Monto que falta pagar |
| Saldo después del cobro | Cálculo en tiempo real |
| Tipo de Cambio | Si hay monedas diferentes |

### Configuración de Caja:

| Campo | Descripción |
|-------|-------------|
| Fecha del Cobro | Editable (default: fecha de caja) |
| Caja | Selector de cajas disponibles |
| Turno | Selector de turno (si la caja tiene turnos) |

**Nota:** Si se selecciona "Sin caja", el cobro no afecta el resumen de caja.

### Medios de Pago Soportados:

| Medio | Campos Adicionales |
|-------|-------------------|
| EFECTIVO | Solo monto |
| TARJETA | Banco/Tarjeta, Últimos 4 dígitos |
| CHEQUE | Número de cheque, Banco |
| TRANSFERENCIA | Número de transacción |
| QR | Solo monto |

### Multi-Moneda:

- Cada medio de pago puede tener su propia moneda
- El sistema convierte al tipo de cambio del día
- Muestra detalle de conversión en resumen

---

## Cálculos Importantes

### Saldo Restante:
```
SaldoRestante = SaldoPendiente - TotalCobro
```

### Total Cobro (moneda única):
```
TotalCobro = Σ(Monto de cada detalle de pago)
```

### Total Cobro con Conversión:
```
TotalConvertido = Σ(Monto × TipoCambio para cada detalle en moneda diferente)
```

### Distribución a Cuotas:
Los pagos se aplican automáticamente a las cuotas pendientes en orden de vencimiento.

---

## Estados del Sistema

### Estados de CuentaPorCobrar:

| Estado | Descripción | Color |
|--------|-------------|-------|
| PENDIENTE | Tiene saldo por cobrar | Amarillo |
| VENCIDO | Fecha vencimiento pasada | Rojo |
| PAGADO | SaldoPendiente = 0 | Verde |
| CANCELADO | Anulado manualmente | Gris |

### Estados de CobroCuota:

| Estado | Descripción |
|--------|-------------|
| CONFIRMADO | Cobro válido y activo |
| ANULADO | Cobro revertido |

### Estados de Cuota:

| Estado | Descripción |
|--------|-------------|
| PENDIENTE | Tiene saldo por pagar |
| VENCIDO | Pasó fecha de vencimiento |
| PAGADO | SaldoCuota = 0 |

---

## Integración con Otros Módulos

### Ventas
- Las ventas a crédito generan CuentaPorCobrar automáticamente
- Referencia a IdVenta para trazabilidad

### Caja
- Los cobros afectan el resumen de caja del turno
- Selector de caja y turno en el cobro

### Monedas y Tipos de Cambio
- Soporte para créditos en moneda extranjera
- Conversión automática en cobros
- Tipo de cambio del día desde BD

### Clientes
- Vínculo directo con el cliente deudor
- Filtros por cliente en todos los listados

### Sucursales
- Filtrado por sucursal del usuario
- Trazabilidad de origen del crédito

---

## Informes Disponibles

### 1. Listado de Cobros (CobrosListado)

**Filtros:**
- Rango de fechas
- Caja específica
- Turno
- Cliente

**Resumen:**
- Total cobrado
- Total en efectivo
- Total en tarjetas
- Total otros medios

### 2. Informe de Cuentas por Cobrar

**Filtros:**
- Rango de fechas
- Cliente
- Estado (Pendiente, Vencido, Cobrado)

**Resumen:**
- Total por cobrar
- Total vencido
- Por vencer (7 días)
- Al día

**Exportación:**
- Excel
- Impresión
- Envío por correo

---

## Impresión de Recibos

### Formatos Disponibles:

1. **Recibo Estándar** (CobroRecibo.razor) - Formato ticket
2. **Recibo A4** (CobroReciboA4.razor) - Formato carta

### Contenido del Recibo:

- Datos de la empresa y sucursal
- Número de recibo
- Fecha y hora
- Datos del cliente
- Referencia a venta/factura
- Detalle de medios de pago
- Monto total cobrado
- Saldo pendiente (si aplica)

---

## Validaciones

### Al Registrar Cobro:
- Caja seleccionada (o explícitamente sin caja)
- Al menos un medio de pago con monto > 0
- Total del cobro no puede exceder saldo pendiente
- Datos completos según medio de pago seleccionado

### Tipo de Cambio:
- Obligatorio cuando hay monedas diferentes
- Se carga automáticamente del día
- Puede editarse manualmente

### Integridad:
- No se puede cobrar una cuenta ya PAGADA
- Los cobros ANULADOS no afectan saldos

---

## Consideraciones Técnicas

### Transaccionalidad:
- El cobro y actualización de saldos se procesan atómicamente
- Si falla la actualización de cuotas, se revierte todo

### Concurrencia:
- Verificación de saldo antes de confirmar
- Previene cobros duplicados

### Auditoría:
- Usuario que registró el cobro
- Fecha y hora exactas
- Caja y turno para cierre

### Rendimiento:
- Listados limitados a 300 registros
- Carga bajo demanda de cuotas expandidas

---

## Casos de Uso Comunes

### 1. Cobro Total de Crédito
- Cliente paga el saldo completo
- Una transacción liquida toda la deuda
- Estado cambia a PAGADO

### 2. Cobro Parcial
- Cliente paga parte del saldo
- Se actualiza SaldoPendiente
- Continúa en estado PENDIENTE

### 3. Cobro en Múltiples Medios de Pago
- Cliente paga parte en efectivo, parte con tarjeta
- Cada medio se registra como CobroDetalle
- Total suma de todos los medios

### 4. Cobro en Moneda Diferente
- Crédito en USD, cliente paga en Guaraníes
- Sistema convierte al tipo de cambio del día
- Registra moneda original y convertida

### 5. Consulta de Vencidos
- Filtrar por estado VENCIDO
- Ver días de atraso por cuenta
- Priorizar cobros de alto riesgo

---

## Diagrama de Relaciones

```
Venta (origen del crédito)
    │
    └── CuentaPorCobrar
            │
            ├── Cliente (deudor)
            │
            ├── Moneda
            │
            ├── Cuotas → CuentaPorCobrarCuota[]
            │               │
            │               └── Estado (PENDIENTE/VENCIDO/PAGADO)
            │
            └── Cobros → CobroCuota[]
                            │
                            ├── Cliente
                            ├── Caja
                            ├── Sucursal
                            ├── Moneda
                            │
                            └── Detalles → CobroDetalle[]
                                            │
                                            ├── MedioPago
                                            ├── Moneda
                                            └── Datos específicos (tarjeta/cheque/etc)
```

---

## Flujo de Estados

```
[Venta a Crédito]
        ↓
[CuentaPorCobrar: PENDIENTE]
        │
        ├── (Fecha vence) → [VENCIDO]
        │                        │
        │                        ├── Cobro Total → [PAGADO]
        │                        └── Cobro Parcial → Actualiza saldo
        │
        ├── Cobro Parcial → Actualiza saldo, sigue PENDIENTE
        │
        └── Cobro Total → [PAGADO]

[CobroCuota: CONFIRMADO]
        │
        └── Anulación → [ANULADO] → Revierte saldos
```

---

## Permisos Sugeridos

| Permiso | Descripción |
|---------|-------------|
| VIEW | Ver cuentas por cobrar y cobros realizados |
| CREATE | Registrar nuevos cobros |
| EDIT | Modificar observaciones (cobros no se editan) |
| DELETE | Anular cobros |

**Nota:** Los cobros normalmente no se editan, solo se anulan y se registra uno nuevo si hay error.
