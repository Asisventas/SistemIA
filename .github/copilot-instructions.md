# Instrucciones para GitHub Copilot - SistemIA

## 📋 Descripción del Proyecto
SistemIA es un sistema de gestión empresarial desarrollado en **Blazor Server** con integración a **SIFEN** (Facturación Electrónica de Paraguay - SET).

## 🛠️ Stack Tecnológico
- **Framework:** Blazor Server (.NET 8)
- **ORM:** Entity Framework Core
- **Base de datos:** SQL Server (`SERVERSIS\SQL2022`, BD: `asiswebapp`)
- **UI:** Bootstrap 5 + CSS personalizado con sistema de temas
- **Facturación Electrónica:** SIFEN (Sistema Integrado de Facturación Electrónica - Paraguay)

## 📁 Estructura del Proyecto

```
Models/          → Entidades y modelos de datos
Pages/           → Páginas Razor (CRUD, listados, impresión)
Services/        → Servicios de negocio (SIFEN, impresión, etc.)
Shared/          → Componentes compartidos, layouts, vistas previas
Components/      → Componentes de protección y permisos
Controllers/     → API endpoints (descargas, PDF, impresión)
Migrations/      → Migraciones de EF Core
wwwroot/css/     → Estilos (site.css es el principal)
.ai-docs/        → Documentación técnica de referencia
```

## 📖 Documentación de Referencia
**IMPORTANTE:** Consultar `.ai-docs/` antes de implementar:
- `MODULO_NUEVO_GUIA.md` - Guía completa para crear módulos nuevos
- `PATRONES_CSS.md` - Patrones CSS y sistema de temas
- `GUIA_MIGRACIONES_EF_CORE.md` - Migraciones Entity Framework
- `PUBLICACION_DEPLOY.md` - Publicación y problemas de cultura/decimales
- `FLEXBOX_SCROLL_SIDEBAR.md` - Solución para scroll en sidebar

## 🔑 Convenciones de Código

### Idioma
- **Nombres de variables, métodos, clases:** Español
- **Comentarios:** Español
- **Nombres de tablas y columnas:** Español

### Modelos
- PK con prefijo `Id` + Entidad: `IdCliente`, `IdVenta`, `IdProducto`
- El modelo `Usuario` usa `Id_Usu` como PK (excepción histórica)
- Contraseñas: `ContrasenaHash` (SHA256)
- Usar `[Column(TypeName = "decimal(18,4)")]` para montos
- Agrupar propiedades con comentarios: `// ========== SECCIÓN ==========`

### Páginas Razor
- CRUD principal: `[Modulo].razor`
- Listado/Explorador: `[Modulo]Explorar.razor`
- Impresión: `[Modulo]Imprimir.razor`
- Vista previa: `[Modulo]VistaPrevia.razor` en Shared/

### CSS
- Usar variables de tema: `var(--bg-surface)`, `var(--text-primary)`
- Estilos globales en `wwwroot/css/site.css`
- Temas soportados: tenue (default), claro, oscuro

## ⚙️ Configuración

### Puertos de desarrollo
- **HTTP:** `http://localhost:5095`
- **HTTPS:** `https://localhost:7060`

### Contraseñas importantes
- Certificado instalador (PFX): `SistemIA2024!`
- Certificado mkcert: `changeit`

## 🧾 SIFEN (Facturación Electrónica)

### Tipos de Documentos
- Factura Electrónica (FE)
- Nota de Crédito Electrónica (NCE)
- Nota de Débito Electrónica (NDE)
- Autofactura Electrónica (AFE)
- Nota de Remisión Electrónica (NRE)

### Estructura XML
- Seguir estrictamente la especificación del SET
- Namespace: `http://ekuatia.set.gov.py/sifen/xsd`
- Los servicios SIFEN están en `Services/`

### Campos SIFEN comunes
- `CDC` - Código de Control (44 caracteres)
- `IdLote` - Identificador de lote enviado
- `EstadoSifen` - Estado del documento en SIFEN
- `MensajeSifen` - Mensaje de respuesta del SET

## 🗃️ Entity Framework Core - REGLAS CRÍTICAS

### 🚫 PROHIBIDO: Crear o Alterar Tablas por SQL Directo
> **NUNCA crear tablas, agregar columnas o modificar estructura de BD usando scripts SQL directos.**
> 
> Los cambios de estructura SIEMPRE deben hacerse mediante **migraciones EF Core** para que:
> 1. Se apliquen automáticamente en los clientes al actualizar
> 2. Queden registrados en el historial de migraciones
> 3. Sean reversibles con `Down()`

```powershell
# ❌ PROHIBIDO - No crear tablas así
sqlcmd -Q "CREATE TABLE MiTabla (...)"

# ❌ PROHIBIDO - No alterar tablas así  
sqlcmd -Q "ALTER TABLE MiTabla ADD Columna INT"

# ✅ CORRECTO - Usar migraciones EF Core
# 1. Modificar el modelo en Models/
# 2. Crear migración: dotnet ef migrations add Agregar_Columna_MiTabla
# 3. Aplicar: dotnet ef database update
```

### Migraciones Idempotentes (Para Tablas que Podrían Existir)
Si necesitas crear una migración que funcione tanto en BD nuevas como existentes:
```csharp
// En el método Up() de la migración:
migrationBuilder.Sql(@"
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MiTabla')
    BEGIN
        CREATE TABLE [MiTabla] (...);
    END
");

migrationBuilder.Sql(@"
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MiTabla') AND name = 'NuevaColumna')
    BEGIN
        ALTER TABLE [MiTabla] ADD [NuevaColumna] nvarchar(100) NULL;
    END
");
```

### ⚠️ NUNCA usar `--no-build` al CREAR migraciones
```powershell
# ✅ CORRECTO - Crear migración (SIN --no-build)
dotnet ef migrations add NombreMigracion

# ✅ CORRECTO - Aplicar migración (puede usar --no-build)
dotnet ef database update --no-build

# Remover última migración
dotnet ef migrations remove
```

### ❌ Error común que genera migraciones vacías
```powershell
# ❌ INCORRECTO - Puede crear migración vacía
dotnet ef migrations add NombreMigracion --no-build
```

### Convenciones de Migraciones
- **Nombres descriptivos en español**: `Agregar_Campo_Producto`, `Crear_Tabla_Ventas`
- **Solo datos (UPDATE/INSERT)**: Usar `migrationBuilder.Sql()` en Up() y Down()
- **Verificar antes de aplicar**: Revisar el archivo generado en `Migrations/`
- **Migraciones de datos**: No requieren cambios en modelos, solo SQL directo
- **Scripts SQL auxiliares**: Solo para insertar datos de catálogo, NUNCA para DDL

## ⚠️ Consideraciones Importantes

1. **Decimales en publicación:** Usar cultura invariante para evitar problemas con separador decimal
2. **Usuario.Id_Usu:** NO usar "Id" para el modelo Usuario
3. **Scroll en sidebar:** Usar patrón flexbox documentado
4. **Permisos:** Sistema de permisos con componentes `RequirePermission.razor` y `PageProtection.razor`

---

## 🎨 CSS - REGLAS CRÍTICAS

### Sistema de Temas
- **3 temas:** tenue (default), claro, oscuro
- **Siempre usar variables**, NUNCA colores hardcodeados:
```css
/* ✅ CORRECTO */
background: var(--bg-surface);
color: var(--text-primary);

/* ❌ INCORRECTO */
background: #ffffff;
color: #333;
```

### Variables Principales
| Variable | Uso |
|----------|-----|
| `--bg-page` | Fondo de página |
| `--bg-surface` | Fondo de cards/paneles |
| `--text-primary` | Texto principal |
| `--text-muted` | Texto secundario |
| `--bar-bg` | Fondo de barras |
| `--bar-border` | Bordes de barras |

### Archivos CSS - Orden de Prioridad
1. `bootstrap.min.css` (NO modificar)
2. `main-layout.css` (layout)
3. `nav-menu.css` (menú)
4. `site.css` ← **Principal, tiene prioridad**
5. `SistemIA.styles.css` ← CSS aislado (auto-generado)

---

## 📜 Scroll en Sidebar - PATRÓN OBLIGATORIO

### Problema Común
Submenús expandidos quedan cortados y no hacen scroll.

### Solución: Flexbox con `min-height: 0`
```css
/* 1. Sidebar NO hace scroll */
.sidebar { overflow: hidden !important; height: 100vh; }

/* 2. nav-menu es el ÚNICO que hace scroll */
.nav-menu {
    display: flex;
    flex-direction: column;
    min-height: 0;        /* ← CRUCIAL */
    overflow-y: auto;     /* ← ÚNICO scroll */
}

/* 3. Submenús SIN límite de altura */
.submenu-container.show { max-height: 9999px !important; }
```

### Regla de Oro
> **Solo UN contenedor** debe tener `overflow-y: auto`. Los padres: `overflow: hidden`, los hijos: `overflow: visible`.

---

## 📦 Publicación - REGLAS CRÍTICAS

### Siempre Self-Contained
```powershell
dotnet publish -c Release -o publish_selfcontained --self-contained true -r win-x64
```
**¿Por qué?** El cliente puede no tener .NET 8 instalado.

### Problema de Decimales (Cultura)
**Síntoma:** Error `"1,05" cannot be parsed` en inputs numéricos.

**Causa:** Servidor usa coma (`,`), HTML espera punto (`.`).

**Solución:**
```razor
<!-- ❌ INCORRECTO -->
<input type="number" value="@factorPrecio" />

<!-- ✅ CORRECTO -->
<input type="number" value="@(factorPrecio?.ToString(CultureInfo.InvariantCulture))" />
```

### Script de Base de Datos - Regenerar después de cada migración
```powershell
dotnet ef migrations script --idempotent -o "Installer\CrearBaseDatos.sql"
```

---

## 🆕 Crear Módulo Nuevo - CHECKLIST

### Estructura de Archivos
```
Models/
├── [Entidad].cs                    # Modelo principal
├── [Entidad]Detalle.cs             # Detalle (si tiene líneas)

Pages/
├── [Modulo].razor                  # CRUD principal
├── [Modulo]Explorar.razor          # Listado/búsqueda
├── [Modulo]Imprimir.razor          # Impresión

Shared/
├── [Modulo]TicketVistaPrevia.razor # Vista previa ticket
├── Reportes/
    ├── Kude[Modulo].razor          # Formato A4/KuDE
    └── Kude[Modulo].razor.css      # ← NO OLVIDAR!
```

### Modelo - Campos Estándar
```csharp
// ========== NUMERACIÓN ==========
[MaxLength(3)] public string? Establecimiento { get; set; }
[MaxLength(3)] public string? PuntoExpedicion { get; set; }
public int Numero { get; set; }

// ========== TOTALES (siempre decimal 18,4) ==========
[Column(TypeName = "decimal(18,4)")] public decimal Subtotal { get; set; }
[Column(TypeName = "decimal(18,4)")] public decimal TotalIVA10 { get; set; }
[Column(TypeName = "decimal(18,4)")] public decimal TotalIVA5 { get; set; }
[Column(TypeName = "decimal(18,4)")] public decimal TotalExenta { get; set; }
[Column(TypeName = "decimal(18,4)")] public decimal Total { get; set; }

// ========== ESTADO ==========
[MaxLength(20)] public string Estado { get; set; } = "Borrador";

// ========== SIFEN ==========
[MaxLength(8)] public string? Timbrado { get; set; }
[MaxLength(64)] public string? CDC { get; set; }
[MaxLength(30)] public string? EstadoSifen { get; set; }
```

### Validación de Cantidad Decimal por Producto
```csharp
// En modelo detalle - Propiedad NO mapeada
[NotMapped]
public bool PermiteDecimal { get; set; }

// En input
<input type="number" 
       step="@(det.PermiteDecimal ? "0.01" : "1")" 
       min="@(det.PermiteDecimal ? "0.01" : "1")" />

// Al agregar producto
det.PermiteDecimal = producto.PermiteDecimal;

// Validación
if (!det.PermiteDecimal)
    det.Cantidad = Math.Max(1, Math.Round(det.Cantidad, 0));
```

### KuDE (Reporte A4) - NO OLVIDAR el CSS
```css
/* Kude[Modulo].razor.css - SIEMPRE crear */
@media print {
  @page { size: A4 portrait; margin: 8mm 10mm 10mm 10mm; }
  .kude .doc-a4 { width: 210mm !important; max-width: none !important; }
}
```

---

## 🇵🇾 Reglas de Negocio Paraguay - SIFEN

### Cálculo de IVA (Método Inverso)
```csharp
// Desde precio con IVA incluido:
decimal iva10 = precioConIva / 11m;           // 10% → dividir entre 11
decimal iva5  = precioConIva / 21m;           // 5%  → dividir entre 21
decimal exenta = 0;                            // Sin IVA

// Ejemplo: Producto Gs 110.000 con IVA 10%
// IVA = 110.000 / 11 = 10.000
// Gravada = 110.000 - 10.000 = 100.000
```

### Tipos de Operación (B2B/B2C)
```csharp
// Regla: RUC >= 50.000.000 = B2B (Empresas/Extranjeros)
//        RUC <  50.000.000 = B2C (Personas Físicas)
string tipoOperacion = (long.TryParse(ruc, out var rucNum) && rucNum >= 50_000_000) ? "1" : "2";

// Código 1 = B2B - Empresa a Empresa/Extranjero
// Código 2 = B2C - Empresa a Cliente
```

### Formato RUC en SIFEN
```csharp
// SIFEN requiere RUC SIN puntos ni guiones, CON dígito verificador
string rucSifen = ruc.Replace(".", "").Replace("-", "");  // "80012345-6" → "800123456"
```

### Monedas Soportadas
| Código | Moneda | Símbolo | Decimales |
|--------|--------|---------|-----------|
| PYG | Guaraníes | Gs | 0 |
| USD | Dólares | $ | 2 |
| BRL | Reales | R$ | 2 |

---

## 🔄 Flujos de Estado

### Ventas / Facturas
```
Borrador → Confirmada → [Enviada SIFEN] → Aprobada SIFEN
                ↓              ↓
             Anulada    Rechazada SIFEN
```

### Compras
```
Borrador → Confirmada → Anulada
```

### Notas de Crédito
```
Borrador → Confirmada → [Enviada SIFEN] → Aprobada SIFEN
```

---

## 🗺️ Relaciones de Modelos Principales

```
Sociedad (empresa)
    └── Sucursal
          └── Caja
                └── Venta/Compra/NC
                      ├── Cliente/Proveedor
                      ├── Timbrado
                      ├── Moneda
                      └── [Entidad]Detalle
                            └── Producto
                                  ├── Categoria
                                  ├── TipoIVA
                                  └── Deposito
```

---

## 🏪 Estructura de Cajas - LÓGICA CRÍTICA

### Concepto Fundamental
El sistema maneja **múltiples cajas por sucursal**, cada una con un propósito específico:

| IdCaja | Nombre | Uso |
|--------|--------|-----|
| 1 | Caja Tienda | Ventas al público, cobros, pagos operativos |
| 2 | Caja Administración | Pagos a proveedores, operaciones administrativas |
| N | Caja N | Según necesidad del negocio |

### Filtros Obligatorios para Reportes/Cierres
**SIEMPRE** filtrar por estos 4 criterios:
1. **IdSucursal** - Sucursal donde ocurrió la operación
2. **IdCaja** - Caja específica (Tienda, Administración, etc.)
3. **Fecha / FechaCaja** - Fecha de la operación
4. **Turno** - Turno de trabajo (1, 2, 3...)

### Regla de Afectación de Caja
> **Si una operación tiene `IdCaja` asignada, afecta ESA caja.**
> 
> No se necesitan campos adicionales como "AfectaCaja". La lógica es simple:
> - Pago desde Caja #1 (Tienda) → Afecta Caja Tienda
> - Pago desde Caja #2 (Admin) → Afecta Caja Administración
> - NC de Compra con IdCaja = 1 → Aparece en cierre de Caja Tienda

### Ejemplo de Consulta Correcta
```csharp
// ✅ CORRECTO - Filtrar por Caja, Fecha, Turno
var notasCredito = await ctx.NotasCreditoVentas
    .Where(nc => nc.Fecha.Date == fechaCaja.Date 
              && nc.IdCaja == idCaja 
              && nc.Turno == turnoActual.ToString()
              && nc.Estado == "Confirmada")
    .ToListAsync();

// ✅ También para NC de Compras
var ncCompras = await ctx.NotasCreditoCompras
    .Where(nc => nc.Fecha.Date == fechaCaja.Date 
              && nc.IdCaja == idCaja 
              && nc.Turno == turnoActual
              && nc.Estado == "Confirmada")
    .ToListAsync();
```

### Operaciones que Afectan Caja (con IdCaja)
- Ventas contado/crédito
- Cobros de crédito (CobrosCuotas)
- Compras contado (en efectivo)
- Pagos a proveedores (PagosProveedores)
- Notas de Crédito Ventas (devoluciones al cliente = EGRESO)
- Notas de Crédito Compras (crédito del proveedor = INGRESO)

---

## 🐛 Errores Comunes y Soluciones

| Error | Causa | Solución |
|-------|-------|----------|
| `"1,05" cannot be parsed` | Cultura con coma decimal | `ToString(CultureInfo.InvariantCulture)` |
| Migración vacía | Usar `--no-build` al crear | **NUNCA** usar `--no-build` en `migrations add` |
| `CircuitHost disconnected` | `StateHasChanged` fuera del contexto | Verificar `disposed` antes de llamar |
| FK violation al insertar | Orden incorrecto | Insertar padres antes que hijos |
| `Object reference null` en Include | Falta `Include()` en query | Agregar `.Include(x => x.Relacion)` |
| CSS no aplica en tema | Color hardcodeado | Usar `var(--variable)` |

### Anti-patrones a Evitar
```csharp
// ❌ INCORRECTO - Query en el render
@foreach (var item in _db.Productos.ToList())

// ✅ CORRECTO - Cargar en OnInitializedAsync
private List<Producto> productos = new();
protected override async Task OnInitializedAsync()
{
    productos = await _db.Productos.ToListAsync();
}

// ❌ INCORRECTO - StateHasChanged sin verificar
await Task.Delay(100);
StateHasChanged();

// ✅ CORRECTO - Verificar si componente está vivo
if (!disposed)
    await InvokeAsync(StateHasChanged);
```

---

## 🎯 Homogeneidad de UI - REGLAS OBLIGATORIAS

### Estructura de Página Explorar
```razor
@page "/[modulo]/explorar"

<PageProtection Modulo="/[modulo]" Permiso="VIEW">

<!-- 1. ENCABEZADO -->
<div class="d-flex justify-content-between align-items-center mb-4">
  <h3 class="mb-0">
    <i class="bi bi-[icono] text-primary me-2"></i>
    Explorador de [Módulo]
  </h3>
  <div class="text-muted">
    <i class="bi bi-clock me-1"></i>
    Total: <span class="fw-bold">@lista.Count</span> registro(s)
  </div>
</div>

<!-- 2. CARD DE FILTROS -->
<div class="card mb-3 shadow-sm">
  <div class="card-header bg-light">
    <h6 class="mb-0"><i class="bi bi-funnel me-2"></i>Filtros de Búsqueda</h6>
  </div>
  <div class="card-body">
    <div class="row g-3">
      <!-- Filtros aquí -->
    </div>
  </div>
</div>

<!-- 3. CARD DE RESULTADOS -->
<div class="card shadow-sm">
  <div class="card-header bg-white border-bottom">
    <div class="d-flex justify-content-between align-items-center">
      <h6 class="mb-0"><i class="bi bi-table me-2"></i>Resultados</h6>
      <small class="text-muted">Mostrando @lista.Count resultado(s)</small>
    </div>
  </div>
  <div class="table-responsive">
    <table class="table table-hover align-middle mb-0">
      <!-- Tabla -->
    </table>
  </div>
</div>

</PageProtection>
```

### Estructura de Menú (NavMenu.razor)
```razor
<!-- SUBMENÚ - Patrón obligatorio -->
<div class="nav-item mb-1">
    <!-- Botón del submenú -->
    <button class="nav-link submenu-button w-100 d-flex align-items-center justify-content-between @(isSubMenuOpen ? "active" : "")"
            @onclick="ToggleSubMenu"
            type="button"
            title="@(IsCollapsed ? "Nombre" : "")">
        <span class="d-flex align-items-center">
            <i class="bi bi-[icono] me-2"></i>
            @if (!IsCollapsed){<span class="link-text text-nowrap">Nombre</span>}
        </span>
        @if (!IsCollapsed)
        {
            <i class="bi @(isSubMenuOpen ? "bi-chevron-down" : "bi-chevron-right") ms-2"></i>
        }
    </button>

    <!-- Items del submenú -->
    <div class="submenu-container @(isSubMenuOpen ? "show" : "collapse")">
        <div class="submenu-items">
            <!-- Acción principal primero -->
            <NavLink class="nav-link" href="/modulo" @onclick="OnAnyNavigate">
                <i class="bi bi-plus-square me-2"></i>
                @if (!IsCollapsed){<span class="link-text">Crear Nuevo</span>}
            </NavLink>
            <!-- Explorador después -->
            <NavLink class="nav-link" href="/modulo/explorar" @onclick="OnAnyNavigate">
                <i class="bi bi-search me-2"></i>
                @if (!IsCollapsed){<span class="link-text">Explorador</span>}
            </NavLink>
            <!-- Separador para secciones relacionadas -->
            <hr class="my-1 mx-2 border-secondary opacity-25" />
            <!-- Items secundarios -->
        </div>
    </div>
</div>
```

### Iconos Bootstrap Estándar
| Acción | Icono | Uso |
|--------|-------|-----|
| Crear/Nuevo | `bi-plus-square` | Botón de crear |
| Buscar/Explorar | `bi-search` | Explorador |
| Editar | `bi-pencil` | Botón editar |
| Eliminar | `bi-trash` | Botón eliminar (text-danger) |
| Ver/Detalle | `bi-eye` | Ver registro |
| Imprimir | `bi-printer` | Impresión |
| Descargar | `bi-download` | Exportar |
| Configurar | `bi-gear` | Configuración |
| Historial | `bi-clock-history` | Históricos |
| Filtros | `bi-funnel` | Sección filtros |
| Tabla | `bi-table` | Sección resultados |

### Botones - Colores Estándar
```razor
<!-- Acción principal -->
<button class="btn btn-primary">
    <i class="bi bi-check-lg me-1"></i>Guardar
</button>

<!-- Acción secundaria -->
<button class="btn btn-outline-secondary">
    <i class="bi bi-x-lg me-1"></i>Cancelar
</button>

<!-- Acción peligrosa -->
<button class="btn btn-danger">
    <i class="bi bi-trash me-1"></i>Eliminar
</button>

<!-- Acción de éxito/exportar -->
<button class="btn btn-outline-success">
    <i class="bi bi-file-earmark-excel"></i>
</button>
```

### Labels de Filtros
```razor
<!-- SIEMPRE usar este formato -->
<label class="form-label small text-muted">Nombre Campo</label>
<input class="form-control" ... />
```

### Formato de Tabla
```razor
<table class="table table-hover align-middle mb-0">
  <thead class="table-light">
    <tr>
      <th style="width: 5%"><i class="bi bi-hash me-1"></i>ID</th>
      <th>Descripción</th>
      <th class="text-end">Monto</th>
      <th style="width: 10%" class="text-center">Acciones</th>
    </tr>
  </thead>
  <tbody>
    @foreach (var item in lista)
    {
      <tr>
        <td>@item.Id</td>
        <td>@item.Descripcion</td>
        <td class="text-end">@item.Monto.ToString("N0")</td>
        <td class="text-center">
          <div class="btn-group btn-group-sm">
            <button class="btn btn-outline-primary" title="Editar">
              <i class="bi bi-pencil"></i>
            </button>
            <button class="btn btn-outline-danger" title="Eliminar">
              <i class="bi bi-trash"></i>
            </button>
          </div>
        </td>
      </tr>
    }
  </tbody>
</table>
```

### Formato de Montos
```csharp
// Guaraníes (sin decimales)
monto.ToString("N0")  // 1.500.000

// Dólares/Reales (2 decimales)
monto.ToString("N2")  // 1,500.00

// En inputs (cultura invariante)
monto.ToString(CultureInfo.InvariantCulture)
```

---

## � Sistema de Correo Electrónico

### Modelos Principales
```
Models/
├── ConfiguracionCorreo.cs     # Configuración SMTP por sucursal
├── DestinatarioInforme.cs     # Destinatarios y qué informes reciben
└── TipoInforme.cs             # Enum de tipos de informe
```

### Servicios
```
Services/
├── CorreoService.cs           # Envío de correos (ICorreoService)
└── InformeCorreoService.cs    # Generación y envío de informes (IInformeCorreoService)
```

### ConfiguracionCorreo - Campos Principales
```csharp
public int IdConfiguracionCorreo { get; set; }
public int IdSucursal { get; set; }

// ========== SERVIDOR SMTP ==========
public string ServidorSmtp { get; set; }     // smtp.gmail.com
public int PuertoSmtp { get; set; }          // 587
public bool UsarSsl { get; set; }            // true
public string UsuarioSmtp { get; set; }      // correo@empresa.com
public string ContrasenaSmtp { get; set; }   // contraseña/app password

// ========== REMITENTE ==========
public string CorreoRemitente { get; set; }  // correo@empresa.com
public string NombreRemitente { get; set; }  // "Mi Empresa S.A."

// ========== ENVÍO AUTOMÁTICO ==========
public bool EnviarAlCierreSistema { get; set; }
public bool EnviarResumenDiario { get; set; }
public TimeSpan? HoraEnvioDiario { get; set; }
public bool Activo { get; set; }
```

### DestinatarioInforme - Configurar qué informes recibe
```csharp
public int IdDestinatarioInforme { get; set; }
public int IdConfiguracionCorreo { get; set; }
public string Email { get; set; }
public string? NombreDestinatario { get; set; }

// ========== INFORMES QUE RECIBE ==========
public bool RecibeResumenCierre { get; set; }      // Resumen al cierre
public bool RecibeVentasDetallado { get; set; }    // Informe ventas detallado
public bool RecibeVentasAgrupado { get; set; }     // Informe ventas agrupado
public bool RecibeComprasDetallado { get; set; }   // Informe compras
public bool RecibeNotasCredito { get; set; }       // NC de ventas
public bool RecibeNCDetallado { get; set; }        // NC detallado
public bool RecibeNCCompras { get; set; }          // NC de compras
public bool RecibeProductosValorizado { get; set; } // Stock valorizado
public bool RecibeMovimientosStock { get; set; }   // Movimientos de stock
public bool RecibeCuentasPorCobrar { get; set; }   // CxC pendientes
public bool RecibeCuentasPorPagar { get; set; }    // CxP pendientes
public bool RecibeResumenCaja { get; set; }        // Resumen de caja
public bool RecibeAsistencia { get; set; }         // Control asistencia
public bool Activo { get; set; }
```

### TipoInformeEnum - Tipos de Informes Disponibles
```csharp
public enum TipoInformeEnum
{
    // Ventas
    VentasDiarias = 1,
    VentasDetallado = 2,
    VentasAgrupado = 3,
    VentasPorClasificacion = 4,
    
    // Compras
    ComprasGeneral = 10,
    ComprasDetallado = 11,
    
    // Notas de Crédito
    NotasCreditoVentas = 20,
    NotasCreditoDetallado = 21,
    NotasCreditoCompras = 22,
    
    // Inventario
    StockValorizado = 30,
    StockDetallado = 31,
    MovimientosStock = 32,
    AjustesStock = 33,
    AlertaStockBajo = 34,
    
    // Caja
    CierreCaja = 40,
    ResumenCaja = 41,
    
    // Financieros
    CuentasPorCobrar = 50,
    CuentasPorPagar = 51,
    
    // RRHH
    ControlAsistencia = 60,
    
    // SIFEN
    ResumenSifen = 70,
    
    // Sistema
    ResumenCierreSistema = 100
}
```

### Uso del Servicio de Informes
```csharp
@inject IInformeCorreoService _informeCorreoService

// Enviar informe específico
await _informeCorreoService.EnviarInformeAsync(
    TipoInformeEnum.VentasDiarias, 
    sucursalId, 
    fechaDesde, 
    fechaHasta);

// Enviar todos los informes al cierre
var (exito, mensaje, cantidad) = await _informeCorreoService
    .EnviarInformesCierreAsync(sucursalId);

// Enviar resumen diario/semanal/mensual
await _informeCorreoService.EnviarResumenDiarioAsync(sucursalId, DateTime.Today);
```

### Envío de Factura por Correo a Cliente
```csharp
// En Cliente.cs
public bool EnviarFacturaPorCorreo { get; set; }  // Si true, envía PDF automático

// En Ventas.razor.cs después de confirmar venta
await EnviarFacturaCorreoSiCorrespondeAsync(venta, sucursalId);
```

### Configuración Gmail (App Password)
1. Ir a cuenta Google → Seguridad → Verificación en 2 pasos (activar)
2. Ir a Contraseñas de aplicaciones
3. Crear nueva contraseña para "Correo"
4. Usar esa contraseña (16 caracteres sin espacios) en `ContrasenaSmtp`

```
ServidorSmtp: smtp.gmail.com
PuertoSmtp: 587
UsarSsl: true
UsuarioSmtp: tucorreo@gmail.com
ContrasenaSmtp: xxxx xxxx xxxx xxxx (app password)
```

### Agregar Nuevo Informe al Sistema de Correos
Para agregar un nuevo informe que se pueda enviar por correo, seguir estos pasos:

#### 1. Agregar al Enum (`Models/TipoInforme.cs`)
```csharp
// En TipoInformeEnum
[Display(Name = "Mi Nuevo Informe")]
MiNuevoInforme = 100,  // número único
```

#### 2. Agregar al Catálogo (`Models/TipoInforme.cs`)
```csharp
// En ObtenerInformesCategorizados() → categoría correspondiente
new(TipoInformeEnum.MiNuevoInforme, "Mi Nuevo Informe", "Descripción", "RecibeMiNuevoInforme"),
```

#### 3. Agregar campo bool en DestinatarioInforme (`Models/DestinatarioInforme.cs`)
```csharp
public bool RecibeMiNuevoInforme { get; set; } = false;
```

#### 4. Actualizar método `RecibeInforme()` (`Models/DestinatarioInforme.cs`)
```csharp
"MiNuevoInforme" => RecibeMiNuevoInforme,
```

#### 5. Actualizar `TieneHabilitadoInforme()` (`Services/InformeCorreoService.cs`)
```csharp
TipoInformeEnum.MiNuevoInforme => dest.RecibeMiNuevoInforme,
```
> **Nota:** Si el destinatario tiene `RecibeTodosLosInformes = true`, recibirá automáticamente cualquier informe nuevo.

#### 6. Crear método de generación HTML (`Services/InformeCorreoService.cs`)
```csharp
// En GenerarHtmlInformeAsync switch:
TipoInformeEnum.MiNuevoInforme => await GenerarHtmlMiNuevoInformeAsync(ctx, sucursalId, desde, hasta, nombreEmpresa, nombreSucursal),

// Implementar método:
private async Task<string> GenerarHtmlMiNuevoInformeAsync(...) { ... }
```

#### 7. Agregar checkbox en UI (`Pages/ConfiguracionCorreo.razor`)
```razor
<div class="form-check small">
    <input type="checkbox" class="form-check-input" @bind="_destinatarioEditando.RecibeMiNuevoInforme" />
    <label class="form-check-label">Mi Nuevo Informe</label>
</div>
```

#### 8. Crear migración EF Core
```powershell
dotnet ef migrations add Agregar_RecibeMiNuevoInforme
dotnet ef database update
```

### Envío de Factura por Correo al Cliente
El sistema determina automáticamente el formato de factura basándose en la configuración de la **Caja** (`Cajas.TipoFacturacion`):
- **"Factura Electrónica"** → Genera PDF con QR del CDC
- **"Factura Autoimpresor"** → Genera PDF sin QR (formato tradicional)

La lógica está en `Services/PdfFacturaService.cs`:
```csharp
// Usa la caja de la venta para determinar el tipo
var cajaConfig = await context.Cajas.FirstOrDefaultAsync(c => c.IdCaja == venta.IdCaja);
var tipoFacturacion = cajaConfig?.TipoFacturacion ?? "AUTOIMPRESOR";
bool esFacturaElectronica = tipoFacturacion?.ToUpper() == "ELECTRONICA" 
                         || tipoFacturacion?.ToUpper() == "FACTURA ELECTRONICA";
```

---
## 🤖 Asistente IA Integrado

### Descripción
El sistema incluye un **asistente IA conversacional** integrado que ayuda a los usuarios con preguntas sobre el uso del sistema. Aparece como un chat flotante en todas las páginas.

### Arquitectura

#### Modelos (`Models/AsistenteIA/`)
```
ConocimientoBase.cs
├── BaseConocimiento          # Contenedor principal del conocimiento
├── IntencionUsuario          # Patrones regex para detectar intenciones
├── ArticuloConocimiento      # Artículo para JSON
├── ArticuloConocimientoDB    # Artículo almacenado en BD (editable)
├── ConversacionAsistente     # Historial de conversaciones
├── ConfiguracionAsistenteIA  # Configuración (correo soporte, mensajes)
└── SolicitudSoporteAsistente # Solicitudes de soporte enviadas
```

#### Servicio Principal (`Services/AsistenteIAService.cs`)
```csharp
public interface IAsistenteIAService
{
    Task<RespuestaAsistente> ProcesarMensajeAsync(string mensaje, int? idUsuario, string? nombreUsuario, string? paginaActual);
    Task<bool> AprenderAsync(string contenido, int idUsuario);
    Task GuardarConversacionAsync(ConversacionAsistente conversacion);
    Task<List<ConversacionAsistente>> ObtenerHistorialAsync(int? idUsuario, int cantidad = 20);
}
```

#### Páginas
| Página | Ruta | Descripción |
|--------|------|-------------|
| `ChatAsistente.razor` | (Componente) | Chat flotante en MainLayout |
| `AdminAsistenteIA.razor` | `/admin/asistente-ia` | Panel de administración |

### Sistema de Intenciones

El asistente detecta la intención del usuario mediante **patrones regex**:

```csharp
// En CrearIntencionesIniciales()
new() {
    Nombre = "backup",
    TipoAccion = "explicacion_backup",
    Patrones = new() { @"backup", @"respaldo", @"copia.+seguridad" }
}
```

#### Intenciones Disponibles
| Intención | TipoAccion | Patrones de Ejemplo |
|-----------|------------|---------------------|
| `saludo` | saludo | hola, buenos días, hey |
| `despedida` | despedida | adiós, chau, hasta luego |
| `ayuda` | ayuda | ayuda, help, cómo funciona |
| `navegacion_ventas` | navegacion | ir a ventas, crear venta |
| `configurar_correo` | explicacion_correo | correo, email, smtp |
| `configurar_sifen` | explicacion_sifen | sifen, factura electrónica |
| `backup` | explicacion_backup | backup, respaldo, copia seguridad |
| `cierre_caja` | explicacion_cierre_caja | cierre caja, arqueo |
| `nota_credito` | explicacion_nota_credito | nota crédito, devolución |
| `ajuste_stock` | explicacion_ajuste_stock | ajustar stock, inventario |
| `cuentas_cobrar` | explicacion_cuentas_cobrar | cuentas por cobrar, deuda cliente |
| `cuentas_pagar` | explicacion_cuentas_pagar | cuentas por pagar, pagar proveedor |
| `crear_usuario` | explicacion_usuario | crear usuario, permisos |
| `actualizacion` | explicacion_actualizacion | actualizar sistema, nueva versión |
| `presupuesto` | explicacion_presupuesto | presupuesto, cotización |

### Artículos de Conocimiento (BD)

Los artículos se almacenan en `ArticulosConocimiento` y son editables desde el panel de admin:

```csharp
public class ArticuloConocimientoDB
{
    public int IdArticulo { get; set; }
    public string Categoria { get; set; }        // Ventas, Compras, Sistema...
    public string? Subcategoria { get; set; }
    public string Titulo { get; set; }
    public string Contenido { get; set; }        // Markdown soportado
    public string? PalabrasClave { get; set; }   // Separadas por coma
    public string? RutaNavegacion { get; set; }  // Ej: /ventas/explorar
    public int Prioridad { get; set; }           // 1-10, mayor = más relevante
    public int VecesUtilizado { get; set; }      // Contador de uso
}
```

#### Categorías de Artículos
- **Ventas**: Crear venta, Anular, NC, Presupuestos
- **Compras**: Registrar compra, Pagos proveedores
- **Caja**: Cierre, Turnos
- **Inventario**: Ajustes stock, Transferencias
- **Clientes**: Cobros, Cuentas por cobrar
- **Productos**: Crear producto, Precios diferenciados
- **Sistema**: Backup, Restaurar, Actualizar
- **Usuarios**: Crear usuario, Permisos
- **Configuración**: Empresa, SIFEN, Correo

### Script de Datos Iniciales
> **OBSOLETO**: Ya no se usa script SQL manual. Ver sección siguiente.

### ⚠️ Sincronización Automática de Artículos IA (IMPORTANTE)

Los artículos de conocimiento de la IA se **sincronizan automáticamente** al iniciar la aplicación.

#### ¿Cómo funciona?
1. Al iniciar SistemIA, se ejecuta `DataInitializationService.InicializarArticulosAsistenteIAAsync()`
2. Compara los artículos en código (`ObtenerArticulosIniciales()`) vs los existentes en BD
3. **Solo agrega los artículos nuevos** (por Título), sin tocar los existentes
4. Los datos del cliente (conversaciones, artículos personalizados) **se preservan**

#### Agregar Nuevo Artículo para Distribución

**OBLIGATORIO para cada publicación**: Si agregas un artículo nuevo, debe ir en el código.

```csharp
// En Services/DataInitializationService.cs → ObtenerArticulosIniciales()
new()
{
    Categoria = "MiCategoria",
    Subcategoria = "SubCategoria",
    Titulo = "Título del Artículo",  // ← CLAVE ÚNICA para sincronización
    Contenido = @"Contenido en **Markdown**:

1️⃣ Primer paso
2️⃣ Segundo paso

💡 **Tip**: Información adicional",
    PalabrasClave = "palabra1, palabra2, palabra3",
    RutaNavegacion = "/ruta/navegacion",
    Icono = "bi-icono",
    Prioridad = 8,
    FechaCreacion = ahora,
    FechaActualizacion = ahora,
    Activo = true
},
```

#### Flujo para Agregar Artículos (NUEVO)
1. ✅ Agregar el artículo en `DataInitializationService.cs` → `ObtenerArticulosIniciales()`
2. ✅ Compilar y publicar
3. ✅ Al actualizar cliente, el artículo se inserta automáticamente si no existe

#### ¿Qué se preserva en el cliente?
| Dato | ¿Se preserva? |
|------|---------------|
| Artículos existentes (sin modificar) | ✅ Sí |
| Artículos personalizados del cliente | ✅ Sí |
| Conversaciones históricas | ✅ Sí |
| Configuración del asistente | ✅ Sí |
| VecesUtilizado (contador) | ✅ Sí |

#### ¿Qué se sincroniza?
| Escenario | Acción |
|-----------|--------|
| Artículo nuevo en código | Se inserta en BD del cliente |
| Artículo ya existe (mismo título) | NO se toca |
| Artículo eliminado del código | Permanece en BD del cliente |

> **⚠️ REGLA DE ORO**: No usar el Panel Admin para artículos "oficiales" que deben distribuirse. 
> Siempre agregarlos en `ObtenerArticulosIniciales()` para que se propaguen con actualizaciones.

### Agregar Nueva Intención

#### 1. Agregar patrón en `CrearIntencionesIniciales()`
```csharp
new() {
    Nombre = "mi_nueva_intencion",
    TipoAccion = "explicacion_mi_tema",
    Patrones = new() { @"palabra1", @"palabra2", @"expresion.+regex" }
}
```

#### 2. Agregar manejador en `ProcesarIntencionAsync()`
```csharp
case "explicacion_mi_tema":
    respuesta.Mensaje = $"{nombreUsuario}, para **hacer algo**:\n\n" +
        "1️⃣ Primer paso\n" +
        "2️⃣ Segundo paso\n" +
        "💡 **Tip**: Información adicional";
    respuesta.TipoRespuesta = "navegacion";
    respuesta.RutaNavegacion = "/ruta/destino";
    respuesta.Icono = "bi-icono";
    respuesta.Sugerencias = new List<string> { "Opción 1", "Opción 2" };
    break;
```

### Agregar Nuevo Artículo de Conocimiento

#### Opción 1: En el Código (RECOMENDADA - se propaga a clientes)
Agregar en `Services/DataInitializationService.cs` → `ObtenerArticulosIniciales()`:
```csharp
new()
{
    Categoria = "MiCategoria", Subcategoria = "SubCat", Titulo = "Título Único",
    Contenido = @"Contenido en **Markdown**...",
    PalabrasClave = "palabra1, palabra2",
    RutaNavegacion = "/ruta", Icono = "bi-icono", Prioridad = 8,
    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
},
```

#### Opción 2: Desde el Panel Admin (NO se propaga a clientes)
1. Ir a `/admin/asistente-ia`
2. Pestaña "Artículos de Conocimiento"
3. Click en "Nuevo Artículo"
4. Completar: Categoría, Título, Contenido (Markdown), Palabras Clave

> ⚠️ Los artículos creados en Panel Admin solo existen en ESA instalación.

#### Opción 3: SQL Directo (solo para instalación específica)
```sql
INSERT INTO ArticulosConocimiento 
(Categoria, Subcategoria, Titulo, Contenido, PalabrasClave, 
 RutaNavegacion, Icono, Prioridad, FechaCreacion, FechaActualizacion, Activo, VecesUtilizado)
VALUES 
('MiCategoria', 'SubCat', 'Título del Artículo',
 'Contenido en **Markdown**:\n\n1. Paso 1\n2. Paso 2',
 'palabra1, palabra2, palabra3',
 '/ruta/navegacion', 'bi-icono', 8, GETDATE(), GETDATE(), 1, 0);
```

### Configuración del Asistente

La tabla `ConfiguracionesAsistenteIA` almacena:
```csharp
public class ConfiguracionAsistenteIA
{
    public int IdConfiguracion { get; set; }
    public string? MensajeBienvenida { get; set; }
    public string? MensajeSinRespuesta { get; set; }
    public string? CorreoSoporte { get; set; }
    public string? NombreSoporte { get; set; }
    public bool HabilitarVozEntrada { get; set; }
    public bool HabilitarVozSalida { get; set; }
    public bool HabilitarCapturaPantalla { get; set; }
    public bool HabilitarGrabacionVideo { get; set; }
    public bool HabilitarEnvioSoporte { get; set; }
    public int MaxSegundosVideo { get; set; }
}
```

### Flujo de Procesamiento

```
Usuario escribe mensaje
        ↓
DetectarIntencion() - busca patrones regex
        ↓
¿Intención encontrada?
    Sí → ProcesarIntencionAsync() - respuesta predefinida
    No → BuscarArticulos() - búsqueda por palabras clave en BD
        ↓
¿Artículo encontrado?
    Sí → Devuelve contenido del artículo
    No → Mensaje genérico + opción de soporte
        ↓
GuardarConversacionAsync() - registra en historial
```

### Tablas de Base de Datos

| Tabla | Descripción |
|-------|-------------|
| `ArticulosConocimiento` | Artículos editables por admin |
| `ConversacionesAsistente` | Historial de preguntas/respuestas |
| `ConfiguracionesAsistenteIA` | Configuración general |
| `SolicitudesSoporteAsistente` | Solicitudes enviadas a soporte |

---
## 🔄 Sistema de Actualización (SistemIA.Actualizador)

### Descripción
Proyecto **independiente** que maneja las actualizaciones de SistemIA. Corre en un puerto separado (5096) para poder:
- Detener SistemIA principal (5095)
- Actualizar archivos sin que la página se cierre
- Verificar que la actualización fue exitosa
- Reiniciar SistemIA

### Arquitectura
```
Usuario → SistemIA (5095) → Abre Actualizador (5096)
                                  ↓
                             Selecciona ZIP
                                  ↓
                             Detiene SistemIA (5095)
                                  ↓
                             Copia archivos (con progreso)
                                  ↓
                             Inicia SistemIA (5095)
                                  ↓
                             Verifica archivos
                                  ↓
                             Redirige a SistemIA (5095)
```

### Ubicación del Proyecto
- **Proyecto:** `c:\asis\SistemIA.Actualizador\`
- **Puerto:** 5096 (configurado en Program.cs)
- **Solución:** Agregado al mismo `.sln` de SistemIA

### Archivos Principales
```
SistemIA.Actualizador/
├── Program.cs              # Config puerto 5096
├── Pages/
│   ├── _Host.cshtml        # Layout HTML con estilos
│   └── Index.razor         # Página principal del actualizador
├── App.razor               # Router Blazor
└── _Imports.razor          # Usings
```

### Funcionalidades de Index.razor
1. **Detectar ruta SistemIA**: Busca en `C:\SistemIA`, `C:\Program Files\SistemIA`, etc.
2. **Verificar estado**: Consulta si SistemIA (5095) está activo
3. **Cargar ZIPs disponibles**: Lista archivos de `Releases/` y Escritorio
4. **Crear backup**: Opcional, antes de actualizar
5. **Detener SistemIA**: Usando `sc.exe stop` y/o `Process.Kill()`
6. **Extraer y copiar**: Extrae ZIP, copia archivos (excepto appsettings)
7. **Iniciar SistemIA**: Ejecuta `SistemIA.exe`
8. **Verificar**: Confirma que archivos fueron actualizados recientemente

### Compilar y Publicar
```powershell
# Compilar
dotnet build "c:\asis\SistemIA.Actualizador\SistemIA.Actualizador.csproj"

# Publicar self-contained
dotnet publish "c:\asis\SistemIA.Actualizador\SistemIA.Actualizador.csproj" -c Release -o "c:\asis\SistemIA.Actualizador\publish" --self-contained true -r win-x64

# Ejecutar en desarrollo
Set-Location "c:\asis\SistemIA.Actualizador"; dotnet run
```

### Despliegue en Cliente
El Actualizador debe publicarse junto con SistemIA, típicamente en:
```
C:\SistemIA\
├── SistemIA.exe           # App principal (puerto 5095)
├── Actualizador\
│   └── SistemIA.Actualizador.exe  # Actualizador (puerto 5096)
└── Releases\
    └── *.zip              # Paquetes de actualización
```

### Flujo de Uso
1. Usuario abre `http://localhost:5096` (Actualizador)
2. Selecciona paquete ZIP de actualización
3. Marca opciones (backup, migraciones)
4. Click "Iniciar Actualización"
5. Ve progreso en barra y logs
6. Al terminar, click "Abrir SistemIA" → redirige a 5095

---
## 🚀 Tareas Disponibles (tasks.json)
- `build` - Compilar proyecto
- `watch` - Ejecutar con hot reload
- `Run Blazor Server (watch)` - Ejecutar en modo desarrollo
- Varias tareas para migraciones EF Core

---
## 📝 Sistema de Historial de Cambios - REGISTRO AUTOMÁTICO IA

### ⚠️ IMPORTANTE - La IA DEBE registrar todos los cambios implementados

El sistema cuenta con tablas y servicios para registrar automáticamente los cambios realizados por la IA. Esto permite mantener el contexto entre sesiones y documentar el progreso del sistema.

### Tablas de Base de Datos

#### HistorialCambiosSistema (Cambios del Sistema)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| IdHistorialCambio | int (PK) | ID único del cambio |
| Version | string(20) | Versión del sistema (ej: "2.1.0") |
| FechaCambio | DateTime | Fecha de implementación |
| TituloCambio | string(200) | Título descriptivo del cambio |
| **Tema** | string(100) | **TEMA DE CONSULTA** (ej: "Ventas", "SIFEN", "Reportes") |
| TipoCambio | string(50) | "Nueva Funcionalidad", "Mejora", "Corrección", "Refactorización" |
| ModuloAfectado | string(100) | Módulo/página afectada |
| Prioridad | string(20) | "Alta", "Media", "Baja" |
| DescripcionBreve | string(500) | Descripción corta para listados |
| DescripcionTecnica | string(max) | Detalles técnicos completos |
| ArchivosModificados | string(max) | Lista de archivos creados/modificados |
| **Tags** | string(500) | **ETIQUETAS de búsqueda** (separadas por coma) |
| **Referencias** | string(500) | **REFERENCIAS** a documentación/tickets |
| Notas | string(max) | Notas adicionales |
| ImplementadoPor | string(100) | "Claude Opus 4.5" o usuario |
| ReferenciaTicket | string(100) | Número de ticket/issue si aplica |
| IdConversacionIA | int? | FK a ConversacionIAHistorial |
| Estado | string(30) | "Implementado", "En Progreso", "Pendiente" |
| RequiereMigracion | bool | Si necesita migración EF Core |
| NombreMigracion | string(200) | Nombre de la migración generada |

#### ConversacionesIAHistorial (Sesiones de IA)
| Campo | Tipo | Descripción |
|-------|------|-------------|
| IdConversacionIA | int (PK) | ID de la conversación |
| FechaInicio | DateTime | Inicio de sesión |
| FechaFin | DateTime? | Fin de sesión |
| ModeloIA | string(50) | "Claude Opus 4.5", "GPT-4", etc. |
| Titulo | string(200) | Título/objetivo de la sesión |
| ResumenEjecutivo | string(max) | Resumen ejecutivo |
| ObjetivosSesion | string(max) | Objetivos planteados |
| ResultadosObtenidos | string(max) | Qué se logró |
| TareasPendientes | string(max) | Qué quedó pendiente |
| ModulosTrabajados | string(500) | Módulos afectados |
| ArchivosCreados | string(max) | Archivos nuevos |
| ArchivosModificados | string(max) | Archivos editados |
| MigracionesGeneradas | string(max) | Migraciones creadas |
| ProblemasResoluciones | string(max) | Problemas encontrados y cómo se resolvieron |
| DecisionesTecnicas | string(max) | Decisiones de diseño tomadas |
| Etiquetas | string(500) | Tags de la sesión |
| Complejidad | string(20) | "Simple", "Moderado", "Complejo" |
| DuracionMinutos | int? | Duración estimada |
| CantidadCambios | int | Cantidad de cambios registrados |

### Temas de Consulta (Estándar)
Usar estos temas para organizar los cambios:

| Tema | Descripción |
|------|-------------|
| `Ventas` | Módulo de ventas, facturas, tickets |
| `Compras` | Módulo de compras, proveedores |
| `Inventario` | Stock, productos, depósitos |
| `Clientes` | Gestión de clientes, créditos |
| `SIFEN` | Facturación electrónica Paraguay |
| `Reportes` | Informes, listados, exportaciones |
| `Caja` | Cierres, turnos, arqueos |
| `Usuarios` | Permisos, seguridad, autenticación |
| `Configuración` | Parámetros del sistema |
| `UI/UX` | Interfaz, estilos, usabilidad |
| `Base de Datos` | Migraciones, índices, optimización |
| `Correo` | Sistema de correos automáticos |
| `Asistente IA` | Chatbot integrado |
| `Actualizador` | Sistema de actualizaciones |
| `Infraestructura` | Servicios, DI, configuración |

### 🔧 Servicio para Registrar Cambios

Usar `IHistorialCambiosService` inyectado en servicios/páginas:

```csharp
// Registrar un cambio
await _historialService.RegistrarCambioAsync(new RegistroCambioDto
{
    Titulo = "Agregar filtro por fecha en explorador de ventas",
    Tema = "Ventas",
    TipoCambio = "Mejora",
    ModuloAfectado = "VentasExplorar",
    Prioridad = "Media",
    DescripcionBreve = "Se agregó filtro de rango de fechas en el explorador",
    DescripcionTecnica = "Agregados campos DateTime FechaDesde/FechaHasta con lógica de filtrado...",
    ArchivosModificados = "Pages/VentasExplorar.razor",
    Tags = "filtros, fechas, explorador",
    Referencias = "Solicitud usuario 2024-01-15",
    ImplementadoPor = "Claude Opus 4.5",
    RequiereMigracion = false
});

// Obtener contexto de cambios recientes (para la IA)
var resumen = await _historialService.ObtenerResumenCambiosRecientesAsync(dias: 30);
```

### 📋 Cuándo Registrar Cambios (OBLIGATORIO)

La IA **DEBE** registrar cambios al:
1. ✅ Crear archivos nuevos (páginas, servicios, modelos)
2. ✅ Modificar archivos existentes con cambios funcionales
3. ✅ Crear migraciones de base de datos
4. ✅ Corregir bugs reportados
5. ✅ Agregar nuevas funcionalidades
6. ✅ Refactorizar código existente
7. ✅ Cambiar configuraciones importantes

### 🔍 Consultar Historial para Contexto

Al inicio de una nueva sesión, la IA puede consultar:

```csharp
// Obtener cambios recientes para entender el contexto
var cambiosRecientes = await _historialService.ObtenerCambiosRecientesAsync(50, tema: "Ventas");

// Buscar cambios específicos
var cambios = await _historialService.BuscarCambiosAsync(new BusquedaCambiosDto
{
    Tema = "SIFEN",
    TextoBusqueda = "CDC",
    FechaDesde = DateTime.Now.AddDays(-30)
});

// Generar resumen textual
var resumen = await _historialService.ObtenerResumenCambiosRecientesAsync(dias: 30);
```

### 📱 Páginas de Exploración

| Página | Ruta | Descripción |
|--------|------|-------------|
| HistorialCambiosExplorar | `/sistema/historial-cambios` | Ver todos los cambios del sistema |
| ConversacionesIAExplorar | `/sistema/conversaciones-ia` | Ver sesiones de IA |

### 💡 Ejemplo de Registro al Final de Sesión

```csharp
// Al finalizar una sesión de trabajo, registrar todos los cambios:
var conv = await _historialService.IniciarConversacionAsync(
    "Implementar módulo de historial de cambios", 
    "Claude Opus 4.5");

await _historialService.RegistrarCambioAsync(new RegistroCambioDto
{
    Titulo = "Crear modelo HistorialCambioSistema",
    Tema = "Infraestructura",
    TipoCambio = "Nueva Funcionalidad",
    ModuloAfectado = "Models",
    DescripcionBreve = "Modelo para almacenar cambios del sistema",
    ArchivosModificados = "Models/HistorialCambioSistema.cs",
    Tags = "historial, cambios, documentación",
    IdConversacionIA = conv.IdConversacionIA,
    RequiereMigracion = true,
    NombreMigracion = "AddHistorialCambios"
});

await _historialService.FinalizarConversacionAsync(conv.IdConversacionIA,
    resumenFinal: "Se implementó el módulo completo de historial de cambios",
    tareasPendientes: "Agregar links en el menú principal");
```

### 🗄️ Acceso Directo a Base de Datos (Solo lectura para contexto)

Si necesitas consultar directamente para obtener contexto:

```sql
-- Cambios recientes por tema
SELECT TOP 20 
    FechaCambio, TituloCambio, Tema, TipoCambio, ModuloAfectado, DescripcionBreve
FROM HistorialCambiosSistema
WHERE Tema = 'Ventas'
ORDER BY FechaCambio DESC;

-- Conversaciones de IA recientes
SELECT TOP 10 
    FechaInicio, Titulo, ResumenEjecutivo, ModulosTrabajados, TareasPendientes
FROM ConversacionesIAHistorial
ORDER BY FechaInicio DESC;

-- Buscar por tags
SELECT * FROM HistorialCambiosSistema
WHERE Tags LIKE '%sifen%' OR Tags LIKE '%factura%'
ORDER BY FechaCambio DESC;
```

### ⚡ Registrar al Finalizar Conversación

> **REGLA:** Al final de cada sesión de trabajo significativa, la IA debe crear un registro resumiendo qué se hizo.

Ejemplo de mensaje al usuario al finalizar:
```
✅ **Cambios registrados en el historial:**
- [Nueva Funcionalidad] Crear página HistorialCambiosExplorar
- [Nueva Funcionalidad] Crear servicio HistorialCambiosService
- [Mejora] Agregar campos Tema, Tags, Referencias al modelo

📁 Tema: Infraestructura
🏷️ Tags: historial, cambios, documentación, IA
```
