# Sesión 30 Enero 2026 - Agregar Soporte de Lotes a Ajustes de Stock

## Objetivo
Agregar funcionalidad de selección de lotes en la página "Ajuste de Stock" para permitir ajustes de stock por lote específico. Esto completa el flujo: usuario intenta eliminar lote → "debe tener stock 0" → usa Ajustes de Stock para ajustar.

## Problema Reportado
- Usuario: "El ajuste de stock no tiene la funcionalidad de trabajar con lotes. puedes verificar y agregar"
- Contexto: Después de intentar eliminar un lote con stock, el sistema indicaba que debe usarse Ajustes de Stock
- Pero esa página NO tenía soporte para lotes

## Cambios Implementados

### 1. Pages/AjustesStock.razor (962 líneas)
**Línea 9:** Inyectado LoteService
```csharp
@inject SistemIA.Services.ILoteService LoteService
```

**Líneas 308-310:** Agregadas variables de estado para lotes
```csharp
private bool productoControlaLote = false;
private List<ProductoLote>? lotesDisponibles = null;
private int? selectedLoteId = null;
```

**Líneas 850-872:** Extendida clase LineaVM con campos de lote
```csharp
public int? IdLote { get; set; }
public string? NumeroLote { get; set; }
public DateTime? FechaVencimiento { get; set; }
public bool ControlaLote { get; set; }
```

**Líneas 381-454:** Reescrita CargarContextoProducto() para cargar lotes
- Cuando `producto.ControlaLote = true`, obtiene lista de lotes disponibles via `LoteService.ObtenerLotesProductoAsync()`
- Carga stock del lote en `stockSistema`
- Agregado método `OnLoteChanged()` para actualizar stock al cambiar lote

**Líneas 115-167:** Agregado selector de lote en UI input
```html
@if (productoControlaLote && lotesDisponibles?.Any() == true)
{
  <select class="form-select" @onchange="OnLoteChanged">
    <option value="">-- Seleccionar Lote --</option>
    @foreach(var l in lotesDisponibles)
    {
      <option value="@l.IdProductoLote">@l.NumeroLote @(l.FechaVencimiento.HasValue ? $"(Venc: {l.FechaVencimiento:dd/MM/yy})" : "")</option>
    }
  </select>
}
```

**Líneas 175-210:** Agregada columna "Lote" en tabla de líneas
- Después de "Producto", antes de "Paq."
- Muestra `NumeroLote` y `FechaVencimiento` si producto controla lotes

**Líneas 495-556:** Modificado método AgregarLinea()
- Valida que lote sea seleccionado cuando necesario
- Captura información del lote seleccionado
- Verifica duplicados por producto+depósito+lote (no solo producto+depósito)
- Agrega campos IdLote, NumeroLote, FechaVencimiento, ControlaLote a LineaVM

**Líneas 249:** Actualizado colspan en tfoot de 9 a 10 (nueva columna)

### 2. Services/AjusteStockService.cs

**Línea 12:** Actualizado record LineaAjusteInput
```csharp
public record LineaAjusteInput(int IdProducto, int IdDeposito, decimal StockSistema, decimal StockAjuste, decimal PrecioCostoGs, int? IdLote = null);
```

**Línea 67:** Agregado IdProductoLote en AjusteStockDetalle
```csharp
IdProductoLote = l.IdLote,
```

**Líneas 77-90:** Agregar lógica para actualizar stock del lote específico
- Si existe IdLote, busca el ProductoLote y actualiza su stock directamente
- Siempre actualiza el stock general del producto vía InventarioService

### 3. Models/AjusteStockDetalle.cs (PENDIENTE)

**A Agregar:** Campo `int? IdProductoLote`
- Relación FK a ProductoLote
- Para rastrear qué lote específico fue ajustado

## Estado de Compilación

### ✅ Build Exitoso
- Proyecto compila sin errores: `Build succeeded`

### ⏳ Pendientes

1. **Migración EF Core** - Agregar campo `IdProductoLote` a `AjusteStockDetalle`
   ```bash
   dotnet ef migrations add Add_IdProductoLote_To_AjusteStockDetalle
   dotnet ef database update
   ```

2. **Verificación en BD** - Confirmar que:
   - Campo se creó correctamente
   - FK a ProductoLotes está definida

3. **Testing del flujo completo**:
   - Crear ajuste de stock con lote
   - Verificar que stock del lote se ajusta correctamente
   - Intentar eliminar lote que ahora tiene stock 0

## Problema Identificado (Requiere Investigación)

**Reporte de Usuario:** "trae mal el stock del deposito tienda del lote"

- El stock que se carga desde el lote NO coincide con el esperado
- Posible causa: `CargarContextoProducto()` busca depósito en stock del lote pero:
  - ¿Filtra correctamente por IdDeposito?
  - ¿El lote tiene stock registrado para ese depósito?
  - ¿La vista o modelo trae el stock correcto?

**Verificación necesaria en** `ILoteService.ObtenerLotesProductoAsync()`:
- Ver si filtra por IdDeposito correctamente
- Ver si trae el campo Stock del lote
- Ver si hay múltiples registros por lote/depósito

## Archivos Modificados

| Archivo | Cambios | Estado |
|---------|---------|--------|
| `Pages/AjustesStock.razor` | +170 líneas (inyección, variables, métodos, UI) | ✅ Completado |
| `Services/AjusteStockService.cs` | Record + lógica lote | ✅ Completado |
| `Models/AjusteStockDetalle.cs` | Campo IdProductoLote | ⏳ Pendiente migración |

## Próximos Pasos

1. **Investigar problema de stock de lote**
   - Revisar `LoteService.ObtenerLotesProductoAsync()`
   - Verificar que carga Stock correctamente por depósito
   - Debug: Qué valor trae para lote en Depósito Tienda

2. **Crear migración** para campo `IdProductoLote`

3. **Testing completo**:
   - Prueba con producto que controla lotes
   - Ajusta stock a 0 para un lote
   - Intenta eliminar ese lote (debe permitir)

4. **Documentación**:
   - Explicar en UI que los lotes controlan stock por depósito
   - Aclarar diferencia entre "Stock del Lote" vs "Stock General del Producto"

## Notas Técnicas

### Modelo ProductoLote
- `IdProductoLote`, `IdProducto`, `IdDeposito`, `NumeroLote`, `FechaVencimiento`, `Stock`, `Estado`
- Stock es POR depósito - un lote puede tener stock en Tienda pero no en Bodega

### Flujo de Carga de Stock
1. Usuario selecciona Producto
2. Si `Producto.ControlaLote = true`:
   - Cargar lista de lotes disponibles via `LoteService`
   - Al seleccionar lote, cargar su stock para el depósito actual
3. Usuario ingresa StockAjuste
4. Al guardar:
   - Crear línea con IdLote
   - Actualizar `ProductoLote.Stock` directamente
   - Actualizar stock general del producto via `InventarioService`

### Consideración Importante
El stock del lote es POR DEPÓSITO. Si un lote tiene:
- Depósito Tienda: 50 unidades
- Depósito Bodega: 20 unidades

Entonces en Ajustes de Stock:
- Si selecciono Depósito Tienda, debo ver 50 unidades
- Si selecciono Depósito Bodega, debo ver 20 unidades

**Este es probablemente el problema reportado** - necesita verificar que la carga sea correcta por depósito.
