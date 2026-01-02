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

## 🚀 Tareas Disponibles (tasks.json)
- `build` - Compilar proyecto
- `watch` - Ejecutar con hot reload
- `Run Blazor Server (watch)` - Ejecutar en modo desarrollo
- Varias tareas para migraciones EF Core
