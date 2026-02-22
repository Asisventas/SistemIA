# Lecciones Aprendidas - Módulo Notas de Crédito de Compras

## 📋 Resumen del Módulo

El módulo de **Notas de Crédito de Compras** permite registrar documentos que reducen el saldo de facturas de compra por motivos como devoluciones, descuentos, bonificaciones o ajustes de precio.

### Archivos Creados

```
Models/
├── NotaCreditoCompra.cs              # Modelo principal (cabecera)
├── NotaCreditoCompraDetalle.cs       # Modelo de líneas/items

Pages/
├── NotasCreditoCompra.razor          # Página CRUD principal
├── NotasCreditoCompraExplorar.razor  # Listado/explorador
├── NotasCreditoCompraImprimir.razor  # Página de impresión (A4/Ticket)

Shared/
├── NotaCreditoCompraTicketVistaPrevia.razor  # Vista previa formato ticket 80mm
├── Reportes/
    ├── KudeNotaCreditoCompra.razor           # Reporte formato A4 (KuDE)
    └── KudeNotaCreditoCompra.razor.css       # Estilos del KuDE
    
Pages/ (Informes)
├── InformeNCComprasAgrupado.razor    # Informe agrupado
├── InformeNCComprasDetallado.razor   # Informe detallado
```

---

## 🚨 Problemas Encontrados y Soluciones

### 1. CSS de Impresión A4 - KuDE

**Problema:** El componente `KudeNotaCreditoCompra.razor` no mostraba formato A4 correctamente. La página se veía mal en pantalla y al imprimir.

**Causa:** Faltaba el archivo CSS asociado `KudeNotaCreditoCompra.razor.css`.

**Solución:** SIEMPRE crear el archivo `.razor.css` junto con el componente KuDE. Los estilos de KuDE son específicos para cada documento.

```css
/* Estructura básica de KuDE CSS */
.kude { 
  font-family: Arial, Helvetica, sans-serif; 
  color: #111; 
  font-size: clamp(12px, 1.5vw, 14px);
}

.kude .doc-a4 {
  width: 100%;
  max-width: 800px;
  margin: 0 auto 8px auto;
  padding: 15px;
  background: #fff;
  border: 1px solid var(--bs-border-color, #ced4da);
}

/* CRÍTICO: Estilos de impresión */
@media print {
  @page { size: A4 portrait; margin: 8mm 10mm 10mm 10mm; }
  
  html, body { width: 210mm; height: auto; margin:0; padding:0; background:#fff !important; }
  
  .kude .doc-a4 { 
    width: 210mm !important; 
    max-width: none !important;
    padding: 10mm !important;
  }
}
```

**Lección:** Copiar los estilos de un KuDE existente (ej: `KudeNotaCreditoVenta.razor.css`) y adaptarlos.

---

### 2. Validación de Cantidad Decimal por Producto

**Problema:** Productos que no permiten venta/compra con decimales (ej: productos unitarios) permitían ingresar cantidades como 1.5.

**Causa:** El input de cantidad usaba `step="any"` sin validación contra `Producto.PermiteDecimal`.

**Solución:** Agregar propiedad `[NotMapped]` en el modelo de detalle y validar en la UI.

#### Paso 1: Agregar propiedad al modelo de detalle
```csharp
// En NotaCreditoCompraDetalle.cs
[NotMapped]
public bool PermiteDecimal { get; set; }
```

#### Paso 2: Input con step dinámico
```razor
<input type="number" 
       step="@(det.PermiteDecimal ? "0.01" : "1")" 
       min="@(det.PermiteDecimal ? "0.01" : "1")" 
       @bind="det.Cantidad" 
       @onfocusout="() => ValidarYRecalcularLinea(det)" />
```

#### Paso 3: Asignar al agregar producto
```csharp
private void AgregarProducto(Producto p)
{
    var det = new NotaCreditoCompraDetalle
    {
        IdProducto = p.IdProducto,
        Producto = p,
        PermiteDecimal = p.PermiteDecimal, // ← CRÍTICO
        // ... otros campos
    };
    _detalles.Add(det);
}
```

#### Paso 4: Asignar al cargar documento existente
```csharp
// Después de cargar los detalles desde BD
foreach (var det in _detalles)
{
    det.PermiteDecimal = det.Producto?.PermiteDecimal ?? false;
}
```

#### Paso 5: Método de validación
```csharp
private void ValidarYRecalcularLinea(NotaCreditoCompraDetalle det)
{
    if (!det.PermiteDecimal)
    {
        det.Cantidad = Math.Max(1, Math.Round(det.Cantidad, 0));
    }
    else
    {
        det.Cantidad = Math.Max(0.01m, det.Cantidad);
    }
    CalcularLinea(det);
    RecalcularTotales();
}
```

**Lección:** Esta validación debe implementarse en TODOS los módulos que manejan cantidades de productos:
- ✅ NC Ventas (`NotasCredito.razor`)
- ✅ NC Compras (`NotasCreditoCompra.razor`)
- ✅ Compras (`Compras.razor`)
- ✅ Ajustes de Stock (`AjustesStock.razor`)
- Ventas (ya tenía implementación similar)

---

### 3. Estructura del Modelo - Campos Obligatorios

**Problema:** Errores al guardar por campos NULL o faltantes.

**Solución:** Estructura completa de modelo de cabecera:

```csharp
public class NotaCreditoCompra
{
    [Key]
    public int IdNotaCreditoCompra { get; set; }

    // ========== NUMERACIÓN ==========
    [MaxLength(3)]
    public string? Establecimiento { get; set; }
    
    [MaxLength(3)]
    public string? PuntoExpedicion { get; set; }
    
    [MaxLength(7)]
    public string? NumeroNota { get; set; }

    // ========== RELACIONES PRINCIPALES ==========
    public int IdSucursal { get; set; }
    public Sucursal? Sucursal { get; set; }

    public int? IdCaja { get; set; }  // Nullable para documentos sin caja
    public Caja? Caja { get; set; }

    public int IdProveedor { get; set; }  // Requerido
    public ProveedorSifenMejorado? Proveedor { get; set; }
    
    // IMPORTANTE: Guardar nombre/RUC como histórico
    [MaxLength(200)]
    public string? NombreProveedor { get; set; }
    
    [MaxLength(20)]
    public string? RucProveedor { get; set; }

    // ========== DOCUMENTO ASOCIADO ==========
    public int? IdCompraAsociada { get; set; }  // Nullable - puede no tener
    public Compra? CompraAsociada { get; set; }

    // Datos manuales del documento asociado
    [MaxLength(3)]
    public string? EstablecimientoAsociado { get; set; }
    
    [MaxLength(3)]
    public string? PuntoExpedicionAsociado { get; set; }
    
    [MaxLength(7)]
    public string? NumeroFacturaAsociado { get; set; }
    
    [MaxLength(8)]
    public string? TimbradoAsociado { get; set; }

    // ========== MOTIVO ==========
    [MaxLength(50)]
    public string Motivo { get; set; } = "Devolución";
    
    [MaxLength(500)]
    public string? Observaciones { get; set; }

    // ========== TOTALES - SIEMPRE decimal(18,4) ==========
    [Column(TypeName = "decimal(18,4)")]
    public decimal Subtotal { get; set; }
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal TotalIVA10 { get; set; }
    
    // ... más campos de totales

    // ========== CONTROL DE STOCK ==========
    public bool AfectaStock { get; set; } = true;  // Default true para NC
    public bool StockProcesado { get; set; }

    // ========== AUDITORÍA ==========
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    [MaxLength(50)]
    public string? UsuarioCreacion { get; set; }
    // ...

    // ========== NAVEGACIÓN ==========
    public virtual ICollection<NotaCreditoCompraDetalle>? Detalles { get; set; }
}
```

---

### 4. Registro en AppDbContext

**Problema:** Error "Entity type not found" al hacer consultas.

**Solución:** Registrar DbSet y configurar relaciones en `AppDbContext.cs`:

```csharp
// DbSets
public DbSet<NotaCreditoCompra> NotasCreditoCompras { get; set; }
public DbSet<NotaCreditoCompraDetalle> NotasCreditoComprasDetalles { get; set; }

// OnModelCreating - Configurar relaciones
modelBuilder.Entity<NotaCreditoCompra>(entity =>
{
    entity.HasOne(n => n.Proveedor)
        .WithMany()
        .HasForeignKey(n => n.IdProveedor)
        .OnDelete(DeleteBehavior.Restrict);
    
    entity.HasOne(n => n.CompraAsociada)
        .WithMany()
        .HasForeignKey(n => n.IdCompraAsociada)
        .OnDelete(DeleteBehavior.SetNull);
    
    entity.HasMany(n => n.Detalles)
        .WithOne(d => d.NotaCreditoCompra)
        .HasForeignKey(d => d.IdNotaCreditoCompra)
        .OnDelete(DeleteBehavior.Cascade);
});
```

---

### 5. Página de Impresión - Soporte Dual (A4/Ticket)

**Problema:** No se podía cambiar entre formato A4 y Ticket.

**Solución:** Estructura de página de impresión con soporte dual:

```razor
@page "/notas-credito-compra/imprimir/{Id:int}"
@page "/notas-credito-compra/imprimir/{Id:int}/{Formato}"

@code {
    [Parameter] public int Id { get; set; }
    [Parameter] public string? Formato { get; set; }
    
    private bool EsFormatoA4 => string.IsNullOrEmpty(Formato) || Formato?.ToLower() != "ticket";
}

@if (EsFormatoA4)
{
    <KudeNotaCreditoCompra IdNotaCreditoCompra="@Id" />
}
else
{
    <NotaCreditoCompraTicketVistaPrevia NotaCredito="@notaCredito" 
                                        Detalles="@detalles" 
                                        Empresa="@empresa" 
                                        MostrarBotonImprimir="false" />
}
```

**Lección:** Cargar datos de ticket solo cuando se necesita (lazy loading).

---

### 6. Componente KuDE - Patrón de Carga

**Problema:** El componente se recargaba múltiples veces o no notificaba cuando estaba listo.

**Solución:** Usar patrón con `_lastLoadedId` y callback `OnReady`:

```csharp
[Parameter] public int IdNotaCreditoCompra { get; set; }
[Parameter] public EventCallback OnReady { get; set; }

private int _lastLoadedId = -1;
private int _lastOnReadyId = -1;

protected override async Task OnParametersSetAsync()
{
    // Evitar recargas innecesarias
    if (_lastLoadedId == IdNotaCreditoCompra && nc != null)
    {
        if (OnReady.HasDelegate && _lastOnReadyId != IdNotaCreditoCompra)
        {
            _lastOnReadyId = IdNotaCreditoCompra;
            await OnReady.InvokeAsync();
        }
        return;
    }
    
    // Cargar datos...
    
    _lastLoadedId = IdNotaCreditoCompra;
    
    // Notificar que está listo
    if (OnReady.HasDelegate && _lastOnReadyId != IdNotaCreditoCompra)
    {
        _lastOnReadyId = IdNotaCreditoCompra;
        await OnReady.InvokeAsync();
    }
}
```

---

### 7. Explorador - Filtros y Paginación

**Estructura estándar del explorador:**

```razor
@* Filtros básicos *@
<div class="row g-3 mb-4">
    <div class="col-md-2">
        <label class="form-label">Desde</label>
        <input type="date" @bind="_fechaDesde" @bind:after="Filtrar" />
    </div>
    <div class="col-md-2">
        <label class="form-label">Hasta</label>
        <input type="date" @bind="_fechaHasta" @bind:after="Filtrar" />
    </div>
    <div class="col-md-2">
        <label class="form-label">Estado</label>
        <select @bind="_filtroEstado" @bind:after="Filtrar">
            <option value="">Todos</option>
            <option value="Borrador">Borrador</option>
            <option value="Confirmada">Confirmada</option>
            <option value="Anulada">Anulada</option>
        </select>
    </div>
    <div class="col-md-4">
        <label class="form-label">Buscar</label>
        <input type="text" @bind="_busqueda" @bind:event="oninput" @bind:after="Filtrar" />
    </div>
</div>

@code {
    private DateTime _fechaDesde = DateTime.Today.AddDays(-30);
    private DateTime _fechaHasta = DateTime.Today;
    private string _filtroEstado = "";
    private string _busqueda = "";
}
```

---

## 📝 Checklist para Nuevo Módulo

### Modelos
- [ ] Crear modelo principal con todos los campos necesarios
- [ ] Crear modelo de detalle si aplica
- [ ] Agregar `[NotMapped] PermiteDecimal` en detalle si maneja cantidades
- [ ] Registrar DbSet en `AppDbContext.cs`
- [ ] Configurar relaciones en `OnModelCreating`
- [ ] Crear migración EF Core

### Páginas
- [ ] Página CRUD principal (`[Modulo].razor`)
- [ ] Página explorador (`[Modulo]Explorar.razor`)
- [ ] Página impresión (`[Modulo]Imprimir.razor`)

### Componentes de Impresión
- [ ] KuDE para formato A4 (`Shared/Reportes/Kude[Modulo].razor`)
- [ ] **CSS del KuDE** (`Shared/Reportes/Kude[Modulo].razor.css`) ← NO OLVIDAR
- [ ] Ticket para formato 80mm (`Shared/[Modulo]TicketVistaPrevia.razor`)

### Validaciones
- [ ] Validación de `PermiteDecimal` en inputs de cantidad
- [ ] Asignar `PermiteDecimal` al agregar producto
- [ ] Asignar `PermiteDecimal` al cargar documento existente
- [ ] Método de validación al perder foco

### Informes (opcional)
- [ ] Informe agrupado
- [ ] Informe detallado

### Navegación
- [ ] Agregar al menú en `NavMenu.razor`
- [ ] Registrar permisos si aplica

---

## 🔧 Comandos Útiles

```powershell
# Crear migración
dotnet ef migrations add NombreDescriptivo --no-build

# Aplicar migración
dotnet ef database update --no-build

# Compilar para verificar errores
dotnet build

# Ejecutar en desarrollo
dotnet watch run
```

---

## 📌 Referencias

- [MODULO_NUEVO_GUIA.md](.ai-docs/MODULO_NUEVO_GUIA.md) - Guía general de módulos
- [PATRONES_CSS.md](.ai-docs/PATRONES_CSS.md) - Sistema de temas y CSS
- [GUIA_MIGRACIONES_EF_CORE.md](.ai-docs/GUIA_MIGRACIONES_EF_CORE.md) - Migraciones de BD

---

*Documento creado: 30 de diciembre de 2025*
*Basado en la implementación del módulo NC Compras*
