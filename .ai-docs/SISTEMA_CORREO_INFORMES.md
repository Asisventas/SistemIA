# Sistema de Correo Electrónico e Informes Automáticos

## 📋 Descripción General

El sistema permite configurar el envío automático de correos electrónicos con informes del sistema. Cada sucursal puede tener su propia configuración SMTP y lista de destinatarios, donde cada destinatario puede elegir qué informes desea recibir.

---

## 🏗️ Arquitectura

### Modelos

| Modelo | Tabla | Descripción |
|--------|-------|-------------|
| `ConfiguracionCorreo` | `ConfiguracionesCorreo` | Configuración SMTP por sucursal |
| `DestinatarioInforme` | `DestinatariosInforme` | Destinatarios y sus preferencias de informes |
| `TipoInforme` | N/A (Enum) | Enumeración de tipos de informes |

### Servicios

| Servicio | Interface | Descripción |
|----------|-----------|-------------|
| `CorreoService` | `ICorreoService` | Envío de correos SMTP |
| `InformeCorreoService` | `IInformeCorreoService` | Generación y envío de informes |

---

## 📊 Tipos de Informes Disponibles

### Categoría: Ventas
- **VentasDiarias (1)** - Resumen de ventas del día
- **VentasDetallado (2)** - Ventas con detalle de productos
- **VentasAgrupado (3)** - Ventas agrupadas por cliente/vendedor
- **VentasPorClasificacion (4)** - Ventas por categoría de producto

### Categoría: Compras
- **ComprasGeneral (10)** - Resumen de compras
- **ComprasDetallado (11)** - Compras con detalle de productos

### Categoría: Notas de Crédito
- **NotasCreditoVentas (20)** - NC emitidas a clientes
- **NotasCreditoDetallado (21)** - NC con detalle
- **NotasCreditoCompras (22)** - NC de proveedores

### Categoría: Inventario
- **StockValorizado (30)** - Inventario con valores
- **StockDetallado (31)** - Detalle de stock por depósito
- **MovimientosStock (32)** - Historial de movimientos
- **AjustesStock (33)** - Ajustes realizados
- **AlertaStockBajo (34)** - Productos bajo mínimo

### Categoría: Caja
- **CierreCaja (40)** - Detalle de cierres
- **ResumenCaja (41)** - Resumen de operaciones

### Categoría: Financieros
- **CuentasPorCobrar (50)** - Saldos pendientes de clientes
- **CuentasPorPagar (51)** - Saldos pendientes a proveedores

### Categoría: RRHH
- **ControlAsistencia (60)** - Registro de asistencias

### Categoría: SIFEN
- **ResumenSifen (70)** - Estado de documentos electrónicos

### Categoría: Sistema
- **ResumenCierreSistema (100)** - Resumen completo al cerrar

---

## 🔧 Configuración

### 1. Configurar SMTP (ConfiguracionCorreo)

```csharp
var config = new ConfiguracionCorreo
{
    IdSucursal = sucursalId,
    ServidorSmtp = "smtp.gmail.com",
    PuertoSmtp = 587,
    UsarSsl = true,
    UsuarioSmtp = "empresa@gmail.com",
    ContrasenaSmtp = "xxxx xxxx xxxx xxxx", // App Password
    CorreoRemitente = "empresa@gmail.com",
    NombreRemitente = "Mi Empresa S.A.",
    EnviarAlCierreSistema = true,
    EnviarResumenDiario = false,
    Activo = true
};
```

### 2. Agregar Destinatarios

```csharp
var destinatario = new DestinatarioInforme
{
    IdConfiguracionCorreo = config.IdConfiguracionCorreo,
    Email = "gerente@empresa.com",
    NombreDestinatario = "Juan Pérez",
    
    // Seleccionar qué informes recibe
    RecibeResumenCierre = true,
    RecibeVentasDetallado = true,
    RecibeCuentasPorCobrar = true,
    RecibeResumenCaja = true,
    
    Activo = true
};
```

---

## 💻 Uso en Código

### Inyección de Dependencias

```csharp
// En Program.cs (ya registrado)
builder.Services.AddScoped<ICorreoService, CorreoService>();
builder.Services.AddScoped<IInformeCorreoService, InformeCorreoService>();
```

### En Páginas Razor

```razor
@inject ICorreoService _correoService
@inject IInformeCorreoService _informeCorreoService
```

### Enviar Correo Simple

```csharp
var (exito, mensaje) = await _correoService.EnviarCorreoAsync(
    sucursalId: 1,
    destinatario: "cliente@email.com",
    asunto: "Factura Electrónica",
    cuerpoHtml: "<h1>Su factura</h1>...",
    adjuntos: new List<(string nombre, byte[] contenido)>
    {
        ("factura.pdf", pdfBytes)
    }
);
```

### Enviar Informe Específico

```csharp
var (exito, mensaje) = await _informeCorreoService.EnviarInformeAsync(
    tipoInforme: TipoInformeEnum.VentasDiarias,
    sucursalId: 1,
    fechaDesde: DateTime.Today,
    fechaHasta: DateTime.Today
);
```

### Enviar Todos los Informes de Cierre

```csharp
var (exito, mensaje, cantidad) = await _informeCorreoService
    .EnviarInformesCierreAsync(sucursalId);

if (exito)
{
    Console.WriteLine($"Se enviaron {cantidad} informes");
}
```

### Generar HTML de Informe (sin enviar)

```csharp
string html = await _informeCorreoService.GenerarHtmlInformeAsync(
    TipoInformeEnum.CuentasPorCobrar,
    sucursalId,
    null, // fechaDesde (opcional)
    null  // fechaHasta (opcional)
);
```

---

## 📧 Configuración de Gmail

### Paso 1: Activar Verificación en 2 Pasos
1. Ir a [myaccount.google.com](https://myaccount.google.com)
2. Seguridad → Verificación en 2 pasos → Activar

### Paso 2: Crear Contraseña de Aplicación
1. Seguridad → Contraseñas de aplicaciones
2. Seleccionar "Correo" y "Windows"
3. Generar → Copiar la contraseña de 16 caracteres

### Paso 3: Configurar en SistemIA
```
Servidor SMTP: smtp.gmail.com
Puerto: 587
Usar SSL: Sí
Usuario: tucorreo@gmail.com
Contraseña: abcd efgh ijkl mnop (sin espacios)
```

---

## 🔄 Flujo de Envío al Cierre

```
Usuario cierra sistema
        ↓
¿EnviarAlCierreSistema = true?
        ↓ Sí
Obtener destinatarios activos
        ↓
Por cada destinatario:
    - Verificar qué informes tiene habilitados
    - Generar HTML de cada informe
    - Enviar correo con todos los informes
        ↓
Registrar resultado en log
```

---

## 📁 Archivos del Sistema

```
Models/
├── ConfiguracionCorreo.cs      # Configuración SMTP
├── DestinatarioInforme.cs      # Destinatarios y preferencias
└── TipoInforme.cs              # Enum y helpers

Services/
├── CorreoService.cs            # Servicio SMTP
└── InformeCorreoService.cs     # Generación de informes

Pages/
└── ConfiguracionCorreo.razor   # UI de configuración (TODO)
```

---

## 🐛 Solución de Problemas

### Error: "Authentication failed"
- Verificar que la contraseña sea un App Password, no la contraseña normal
- Verificar que el usuario SMTP sea correcto

### Error: "Connection refused"
- Verificar servidor y puerto (smtp.gmail.com:587)
- Verificar que UsarSsl = true

### Error: "No destinatarios configurados"
- Verificar que hay destinatarios activos
- Verificar que tienen al menos un informe habilitado

### Los informes no se envían al cierre
- Verificar `EnviarAlCierreSistema = true` en ConfiguracionCorreo
- Verificar `Activo = true` en ConfiguracionCorreo
- Verificar que hay al menos un destinatario con `RecibeResumenCierre = true`

---

## 📝 Notas de Implementación

1. **Los totales de IVA en Venta** no existen como campos - se calculan sumando VentaDetalle
2. **Producto.Descripcion** es el campo correcto, no "Nombre"
3. **Producto.CodigoBarras** es el campo correcto, no "CodigoBarra"
4. **CierreCaja.TotalEntregado** es el campo correcto, no "TotalArqueo"
5. **Venta no tiene navegación a Detalles** - usar join con VentasDetalles

---

## 🔮 Mejoras Futuras (TODO)

- [ ] Crear página de configuración de correo en UI
- [ ] Agregar programación de envíos (ej: todos los días a las 8pm)
- [ ] Agregar adjuntos PDF además del HTML
- [ ] Historial de correos enviados
- [ ] Reintento automático en caso de fallo
- [ ] Templates personalizables de correo
