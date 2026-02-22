# Implementación: Precios Diferenciados por Cliente

## Resumen
Se ha implementado un sistema completo de **precios diferenciados por cliente** que respeta automáticamente:
- **Precio Fijo**: Si el cliente tiene un precio fijo específico para un producto
- **Descuento Porcentual**: Si el cliente tiene un descuento porcentual para un producto
- **Conversión de Moneda**: Se aplica después de calcular el precio diferenciado

## Cambios Realizados

### 1. Modelo: Producto.cs
**Cambio**: Agregada navegación `ClientePrecios`
```csharp
// Precios diferenciados por cliente
public ICollection<ClientePrecio>? ClientePrecios { get; set; }
```

### 2. Página: Ventas.razor.cs

#### a) Carga de Productos (Línea 266)
**Antes**:
```csharp
Productos = await ctx.Productos.Include(p => p.TipoIva).Include(p => p.MonedaPrecio)...
```

**Después**:
```csharp
Productos = await ctx.Productos.Include(p => p.TipoIva).Include(p => p.MonedaPrecio).Include(p => p.ClientePrecios)...
```

**Por qué**: Necesita cargar los precios diferenciados para cada cliente.

#### b) Método: CalcularPrecioRespetandoClientePrecio() (Nuevo)
Este método es el **corazón** del sistema. Realiza:

1. **Obtiene el producto**
2. **Lee el precio base en Guaraníes**
3. **Si hay cliente seleccionado**:
   - Busca `ClientePrecio` activo para ese cliente+producto
   - Si existe `PrecioFijoGs`: **usa ese precio**
   - Si existe `PorcentajeDescuento`: **aplica el descuento** sobre el base
4. **Convierte el precio final a la moneda de la venta** (Gs → USD si corresponde)

```csharp
private decimal CalcularPrecioRespetandoClientePrecio(int idProducto, int? idCliente)
{
    // Obtener producto
    var prod = Productos.FirstOrDefault(p => p.IdProducto == idProducto);
    if (prod == null) return 0m;

    // Precio base siempre en Guaraníes
    var precioBaseEnGs = prod.PrecioUnitarioGs;

    // Si no hay cliente seleccionado, usar precio base
    if (!idCliente.HasValue || idCliente.Value <= 0)
    {
        return ConvertirPrecioSegunMonedaVenta(precioBaseEnGs);
    }

    // Buscar ClientePrecio activo para este cliente+producto
    var clientePrecio = prod.ClientePrecios?.FirstOrDefault(cp => 
        cp.IdCliente == idCliente.Value && cp.Activo);

    decimal precioFinalEnGs = precioBaseEnGs;

    if (clientePrecio != null)
    {
        // Opción 1: Precio fijo específico
        if (clientePrecio.PrecioFijoGs.HasValue && clientePrecio.PrecioFijoGs.Value > 0)
        {
            precioFinalEnGs = clientePrecio.PrecioFijoGs.Value;
            Console.WriteLine($"[ClientePrecio] Usando PRECIO FIJO: Gs. {precioFinalEnGs}...");
        }
        // Opción 2: Descuento porcentual
        else if (clientePrecio.PorcentajeDescuento.HasValue && clientePrecio.PorcentajeDescuento.Value > 0)
        {
            var descuentoGs = Math.Round(precioBaseEnGs * (clientePrecio.PorcentajeDescuento.Value / 100m), 4);
            precioFinalEnGs = precioBaseEnGs - descuentoGs;
            Console.WriteLine($"[ClientePrecio] Aplicando DESCUENTO: {clientePrecio.PorcentajeDescuento}%...");
        }
    }

    // Convertir al precio final según la moneda de venta
    return ConvertirPrecioSegunMonedaVenta(precioFinalEnGs);
}
```

#### c) Método: ConvertirPrecioSegunMonedaVenta() (Nuevo)
Helper que convierte precios de Guaraníes a la moneda de la venta:
- Si USD: divide por tipo de cambio
- Si Gs: retorna igual

```csharp
private decimal ConvertirPrecioSegunMonedaVenta(decimal precioEnGs)
{
    var idMonVenta = Cab.IdMoneda;
    decimal precio = precioEnGs;

    if (idMonVenta.HasValue)
    {
        var monVenta = Monedas.FirstOrDefault(m => m.IdMoneda == idMonVenta.Value);

        if (monVenta?.EsMonedaBase == false)
        {
            // Venta en moneda extranjera (USD): convertir Gs → USD
            var cambioVenta = Cab.CambioDelDia ?? 1m;
            precio = Math.Round(precioEnGs / cambioVenta, 4);
            NuevoDetalle.CambioDelDia = cambioVenta;
        }
    }

    return precio;
}
```

#### d) Método: OnCambioManualChanged() (Modificado)
**Antes**: Lógica compleja de conversión inline

**Después**: Usa `CalcularPrecioRespetandoClientePrecio()`
```csharp
protected void OnCambioManualChanged()
{
    if (NuevoDetalle.IdProducto > 0)
    {
        NuevoDetalle.PrecioUnitario = CalcularPrecioRespetandoClientePrecio(
            NuevoDetalle.IdProducto,
            Cab.IdCliente
        );
        RecalcularNuevoDetalle();
    }
    StateHasChanged();
}
```

#### e) Método: SeleccionarProductoAsync() (Simplificado)
**Antes**: ~90 líneas de lógica de conversión

**Después**: Usa el método centralizado
```csharp
private async Task SeleccionarProductoAsync(Producto p)
{
    NuevoDetalle.IdProducto = p.IdProducto;
    BuscarProducto = p.Descripcion ?? string.Empty;
    ProductoImagenUrl = p.Foto;
    
    // Calcular el precio respetando ClientePrecio
    NuevoDetalle.PrecioUnitario = CalcularPrecioRespetandoClientePrecio(p.IdProducto, Cab.IdCliente);
    
    Console.WriteLine($"[SeleccionarProducto] Producto: {p.Descripcion}, Precio final: {NuevoDetalle.PrecioUnitario}");
    RecalcularNuevoDetalle();
}
```

## Flujo de Ejecución

### Caso 1: Cliente tiene PRECIO FIJO
```
1. Cliente: ABC Corp
2. Producto: Widget Pro (Precio Base: 50,000 Gs)
3. ClientePrecio: PrecioFijoGs = 45,000 Gs
4. Resultado: Usa 45,000 Gs (ignora el 50,000 Gs)
5. Si venta en USD (TC: 7,000): 45,000 ÷ 7,000 = 6.43 USD
```

### Caso 2: Cliente tiene DESCUENTO
```
1. Cliente: XYZ Ltd
2. Producto: Widget Pro (Precio Base: 50,000 Gs)
3. ClientePrecio: PorcentajeDescuento = 10% → Descuento: 5,000 Gs
4. Resultado: 50,000 - 5,000 = 45,000 Gs
5. Si venta en USD (TC: 7,000): 45,000 ÷ 7,000 = 6.43 USD
```

### Caso 3: Cliente NO tiene precio diferenciado
```
1. Cliente: New Customer (sin ClientePrecio)
2. Producto: Widget Pro (Precio Base: 50,000 Gs)
3. Resultado: Usa 50,000 Gs (precio estándar del producto)
4. Si venta en USD (TC: 7,000): 50,000 ÷ 7,000 = 7.14 USD
```

## Logging
Se agregaron líneas de Console.WriteLine() para debugging:
- Cuando se aplica PRECIO FIJO
- Cuando se aplica DESCUENTO
- Cuando se selecciona un producto
- Permite verificar en la consola de VS Code qué precio se está usando

## Testing Manual

1. **Ir a Ventas**
2. **Seleccionar un cliente**
3. **En Ficha del Cliente (si existe)**: Crear ClientePrecio
   - Ej: PrecioFijoGs = 45,000 para Widget Pro
4. **Volver a Ventas**
5. **Cambiar cliente a ese cliente**
6. **Seleccionar Widget Pro**
7. **Verificar**: El precio debe mostrar el precio diferenciado, no el estándar
8. **En Console de VS Code**: Debe verse el mensaje de ClientePrecio

## Beneficios

✅ **Centralizado**: Un solo método calcula precios en todas las ubicaciones  
✅ **Consistente**: Siempre respeta ClientePrecio en Gs y luego convierte a moneda de venta  
✅ **Debuggeable**: Logs claros indican qué precio se usó  
✅ **Flexible**: Soporta precio fijo O descuento (no ambos simultáneamente)  
✅ **Performante**: Usa datos en memoria (Productos.ClientePrecios), sin queries adicionales  

## Estado de Compilación
✅ Build exitosa sin errores  
⚠️ 11 warnings preexistentes (no relacionados a este cambio)  

## Próximos Pasos
- Crear ficha de ingreso de ClientePrecio (CRUD)
- Agregar validaciones en Ficha de Cliente
- Documentar en Manual de Usuario
