# Guía para Crear un Módulo Nuevo en SistemIA

Esta guía documenta el proceso paso a paso para crear un nuevo módulo en SistemIA, usando como ejemplo el módulo de **Notas de Crédito de Ventas**.

## 📁 Estructura de Archivos

Un módulo completo típicamente incluye:

```
Models/
├── [Entidad].cs           # Modelo principal (ej: NotaCreditoVenta.cs)
├── [Entidad]Detalle.cs    # Modelo de detalle si aplica (ej: NotaCreditoVentaDetalle.cs)

Pages/
├── [Modulo].razor         # Página principal CRUD (ej: NotasCredito.razor)
├── [Modulo]Explorar.razor # Listado/Explorador (ej: NotasCreditoExplorar.razor)
├── [Modulo]Imprimir.razor # Impresión si aplica (ej: NotasCreditoImprimir.razor)

Shared/
├── [Modulo]VistaPrevia.razor    # Vista previa/ticket (ej: NotaCreditoTicketVistaPrevia.razor)
├── Reportes/
    └── Kude[Modulo].razor       # Reporte KUDE si es para SIFEN
```

---

## 1️⃣ Crear el Modelo Principal

### Archivo: `Models/NotaCreditoVenta.cs`

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SistemIA.Models
{
    /// <summary>
    /// Nota de Crédito de Venta - Descripción del propósito
    /// </summary>
    [Index(nameof(IdSucursal), nameof(NumeroNota), IsUnique = true, Name = "IX_NotaCreditoVentas_Numeracion")]
    public class NotaCreditoVenta
    {
        [Key]
        public int IdNotaCredito { get; set; }

        // ========== NUMERACIÓN ==========
        [MaxLength(3)]
        public string? Establecimiento { get; set; }
        
        [MaxLength(3)]
        public string? PuntoExpedicion { get; set; }
        
        public int NumeroNota { get; set; }

        // ========== RELACIONES PRINCIPALES ==========
        public int IdSucursal { get; set; }
        public Sucursal? Sucursal { get; set; }

        public int IdCaja { get; set; }
        public Caja? Caja { get; set; }

        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }

        // ========== FECHAS ==========
        public DateTime Fecha { get; set; } = DateTime.Now;
        public DateTime? FechaContable { get; set; }

        // ========== MONEDA ==========
        public int IdMoneda { get; set; }
        public Moneda? Moneda { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal CambioDelDia { get; set; } = 1;

        // ========== TOTALES ==========
        [Column(TypeName = "decimal(18,4)")]
        public decimal Subtotal { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalIVA10 { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalIVA5 { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalExenta { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal Total { get; set; }

        // ========== ESTADO ==========
        [MaxLength(20)]
        public string Estado { get; set; } = "Borrador"; // Borrador / Confirmada / Anulada

        // ========== SIFEN ==========
        [MaxLength(8)]
        public string? Timbrado { get; set; }
        
        [MaxLength(64)]
        public string? CDC { get; set; }
        
        [MaxLength(30)]
        public string? EstadoSifen { get; set; }
        
        public string? XmlCDE { get; set; }

        // ========== AUDITORÍA ==========
        public int? IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }
        
        [MaxLength(100)]
        public string? CreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // ========== DETALLES ==========
        public ICollection<NotaCreditoVentaDetalle>? Detalles { get; set; }
    }
}
```

### Modelo de Detalle: `Models/NotaCreditoVentaDetalle.cs`

```csharp
public class NotaCreditoVentaDetalle
{
    [Key]
    public int IdNotaCreditoDetalle { get; set; }

    public int IdNotaCredito { get; set; }
    public NotaCreditoVenta? NotaCredito { get; set; }

    public int IdProducto { get; set; }
    public Producto? Producto { get; set; }
    
    [MaxLength(50)]
    public string? CodigoProducto { get; set; }
    
    [MaxLength(250)]
    public string? NombreProducto { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal Cantidad { get; set; }
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal PrecioUnitario { get; set; }
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal Importe { get; set; }

    public int TasaIVA { get; set; }
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal IVA10 { get; set; }
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal IVA5 { get; set; }
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal Exenta { get; set; }
}
```

---

## 2️⃣ Registrar en AppDbContext

### Agregar DbSet (línea ~100)

```csharp
public DbSet<NotaCreditoVenta> NotasCreditoVentas { get; set; }
public DbSet<NotaCreditoVentaDetalle> NotasCreditoVentasDetalles { get; set; }
```

### Configurar en OnModelCreating

```csharp
// Configuración NotaCreditoVenta
modelBuilder.Entity<NotaCreditoVenta>(entity =>
{
    entity.ToTable("NotasCreditoVentas");
    entity.HasKey(nc => nc.IdNotaCredito);
    
    // Propiedades decimal
    entity.Property(nc => nc.Subtotal).HasColumnType("decimal(18,4)");
    entity.Property(nc => nc.Total).HasColumnType("decimal(18,4)");
    
    // Relaciones
    entity.HasOne(nc => nc.Sucursal)
        .WithMany()
        .HasForeignKey(nc => nc.IdSucursal)
        .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(nc => nc.Cliente)
        .WithMany()
        .HasForeignKey(nc => nc.IdCliente)
        .OnDelete(DeleteBehavior.Restrict);
});

// Configuración Detalle
modelBuilder.Entity<NotaCreditoVentaDetalle>(entity =>
{
    entity.ToTable("NotasCreditoVentasDetalles");
    entity.HasKey(d => d.IdNotaCreditoDetalle);
    
    entity.HasOne(d => d.NotaCredito)
        .WithMany(nc => nc.Detalles)
        .HasForeignKey(d => d.IdNotaCredito)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(d => d.Producto)
        .WithMany()
        .HasForeignKey(d => d.IdProducto)
        .OnDelete(DeleteBehavior.Restrict);
});
```

---

## 3️⃣ Crear Migración EF Core

```powershell
# Compilar primero
dotnet build

# Crear migración
dotnet ef migrations add Crear_NotasCredito_Ventas --project c:\asis\SistemIA\SistemIA.csproj --startup-project c:\asis\SistemIA\SistemIA.csproj --no-build

# Aplicar migración
dotnet ef database update --project c:\asis\SistemIA\SistemIA.csproj --startup-project c:\asis\SistemIA\SistemIA.csproj --no-build
```

---

## 4️⃣ Crear la Página Razor Principal

### Archivo: `Pages/NotasCredito.razor`

```razor
@page "/notas-credito"
@page "/notas-credito/{Id:int}"
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@attribute [Authorize]
@using SistemIA.Models
@using Microsoft.EntityFrameworkCore
@inject IDbContextFactory<AppDbContext> DbFactory
@inject NavigationManager Nav
@inject ISucursalProvider SucursalProvider
@inject ICajaProvider CajaProvider
@inject IJSRuntime JS
@inject PermisosService PermisosService
@inject AuthenticationStateProvider AuthStateProvider

<PageProtection Modulo="/notas-credito" Permiso="CREATE">
<PageTitle>@(_esEdicion ? "Editar" : "Nueva") Nota de Crédito</PageTitle>

<div class="container-fluid">
    <div class="card shadow-sm">
        <div class="card-header bg-primary text-white">
            <h5 class="mb-0">
                <i class="bi bi-file-earmark-minus me-2"></i>
                @(_esEdicion ? "Editar" : "Nueva") Nota de Crédito
            </h5>
        </div>
        
        <div class="card-body">
            @* Contenido del formulario *@
        </div>
    </div>
</div>
</PageProtection>

@code {
    [Parameter] public int? Id { get; set; }
    
    private bool _loading = true;
    private bool _esEdicion => Id.HasValue && Id > 0;
    private NotaCreditoVenta _nc = new();
    private List<NotaCreditoVentaDetalle> _detalles = new();
    
    protected override async Task OnInitializedAsync()
    {
        await CargarDatos();
        _loading = false;
    }
    
    private async Task CargarDatos()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        
        if (_esEdicion)
        {
            _nc = await db.NotasCreditoVentas
                .Include(nc => nc.Detalles)
                .FirstOrDefaultAsync(nc => nc.IdNotaCredito == Id) ?? new();
        }
        else
        {
            _nc = new NotaCreditoVenta
            {
                Fecha = DateTime.Now,
                Estado = "Borrador"
            };
        }
    }
    
    private async Task Guardar()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        
        if (_esEdicion)
        {
            db.NotasCreditoVentas.Update(_nc);
        }
        else
        {
            db.NotasCreditoVentas.Add(_nc);
        }
        
        await db.SaveChangesAsync();
        Nav.NavigateTo("/notas-credito-explorar");
    }
}
```

---

## 5️⃣ Protección con Permisos

### Usar el componente PageProtection

```razor
<PageProtection Modulo="/notas-credito" Permiso="CREATE">
    @* Contenido protegido *@
</PageProtection>
```

### Permisos disponibles:
- `VIEW` - Ver/consultar
- `CREATE` - Crear nuevo
- `EDIT` - Editar existente
- `DELETE` - Eliminar
- `PRINT` - Imprimir
- `EXPORT` - Exportar

### Registrar módulo y permisos

En `Data/SeedPermisos.cs` agregar el módulo:

```csharp
new Modulo { Nombre = "Notas de Crédito", Ruta = "/notas-credito", Descripcion = "Gestión de notas de crédito", Activo = true }
```

---

## 6️⃣ Agregar al Menú

En `Shared/NavMenu.razor` agregar el enlace:

```razor
<NavLink class="nav-link" href="notas-credito-explorar">
    <span class="bi bi-file-earmark-minus" aria-hidden="true"></span> Notas de Crédito
</NavLink>
```

---

## 7️⃣ Patrones Importantes

### Obtener Sucursal/Caja Activa

```csharp
@inject ISucursalProvider SucursalProvider
@inject ICajaProvider CajaProvider

private async Task ObtenerContexto()
{
    _idSucursal = await SucursalProvider.GetSucursalIdAsync();
    _idCaja = await CajaProvider.GetCajaIdAsync();
}
```

### Calcular IVA Paraguay

```csharp
private void CalcularLinea(NotaCreditoVentaDetalle det)
{
    det.Importe = det.Cantidad * det.PrecioUnitario;
    
    switch (det.TasaIVA)
    {
        case 10:
            det.Grabado10 = Math.Round(det.Importe / 1.10m, 4);
            det.IVA10 = det.Importe - det.Grabado10;
            det.IVA5 = 0; det.Exenta = 0;
            break;
        case 5:
            det.Grabado5 = Math.Round(det.Importe / 1.05m, 4);
            det.IVA5 = det.Importe - det.Grabado5;
            det.IVA10 = 0; det.Exenta = 0;
            break;
        default:
            det.Exenta = det.Importe;
            det.IVA10 = 0; det.IVA5 = 0;
            break;
    }
}
```

### Autonumeración

```csharp
private async Task<int> ObtenerSiguienteNumero(AppDbContext db)
{
    var ultimo = await db.NotasCreditoVentas
        .Where(nc => nc.IdSucursal == _idSucursal)
        .MaxAsync(nc => (int?)nc.NumeroNota) ?? 0;
    
    return ultimo + 1;
}
```

---

## 8️⃣ Validación con SHA256 (para operaciones sensibles)

```csharp
@using System.Security.Cryptography
@inject AuthenticationStateProvider AuthStateProvider

private async Task<bool> ValidarPassword(string password)
{
    var authState = await AuthStateProvider.GetAuthenticationStateAsync();
    var userName = authState.User.Identity?.Name;
    
    await using var db = await DbFactory.CreateDbContextAsync();
    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.UsuarioNombre == userName);
    
    if (usuario?.ContrasenaHash == null) return false;
    
    using var sha = SHA256.Create();
    var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
    var hash = sha.ComputeHash(passwordBytes);
    
    return hash.SequenceEqual(usuario.ContrasenaHash);
}
```

---

## 9️⃣ Estados del Documento

Los documentos transaccionales siguen este ciclo:

```
Borrador → Confirmada → (Opcional: Anulada)
```

```csharp
private string GetBadgeClass(string estado) => estado switch
{
    "Borrador" => "bg-warning text-dark",
    "Confirmada" => "bg-success",
    "Anulada" => "bg-danger",
    _ => "bg-secondary"
};
```

---

## 📋 Checklist para Nuevo Módulo

- [ ] Crear modelo(s) en `/Models`
- [ ] Agregar DbSet en `AppDbContext`
- [ ] Configurar en `OnModelCreating`
- [ ] Crear y aplicar migración EF Core
- [ ] Crear página principal `.razor`
- [ ] Crear página explorador (listado)
- [ ] Agregar protección con `PageProtection`
- [ ] Registrar módulo en `SeedPermisos.cs`
- [ ] Agregar enlace en `NavMenu.razor`
- [ ] Crear vista previa/ticket si aplica
- [ ] Implementar integración SIFEN si aplica
