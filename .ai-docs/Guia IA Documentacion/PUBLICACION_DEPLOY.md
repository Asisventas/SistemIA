# Guía de Publicación y Despliegue

## Problema Resuelto
El cliente tenía .NET 5.0 instalado pero la aplicación requiere .NET 8.0. Al publicar con `--no-self-contained`, la app no iniciaba.

## Solución: Publicación Self-Contained

Siempre usar **self-contained** para incluir el runtime de .NET en la publicación:

```powershell
dotnet publish -c Release -o publish_selfcontained --self-contained true -r win-x64
```

### Parámetros importantes:
- `-c Release`: Configuración Release (optimizada)
- `-o publish_selfcontained`: Carpeta de salida
- `--self-contained true`: **CRÍTICO** - Incluye el runtime de .NET
- `-r win-x64`: Runtime identifier para Windows 64-bit

### Para Windows 32-bit (raro):
```powershell
dotnet publish -c Release -o publish_selfcontained --self-contained true -r win-x86
```

## Crear paquete ZIP

```powershell
$fecha = Get-Date -Format "yyyy.MM.dd.HHmm"
$zipPath = "c:\asis\SistemIA\Releases\SistemIA_SelfContained_$fecha.zip"
Compress-Archive -Path "c:\asis\SistemIA\publish_selfcontained\*" -DestinationPath $zipPath -Force
```

## En el cliente

1. Descomprimir en `C:\SistemIA` (o la ruta deseada)
2. Ejecutar directamente: `SistemIA.exe` (ya no necesita comando `dotnet`)

### Desde PowerShell:
```powershell
cd C:\SistemIA
.\SistemIA.exe
```

### Con puerto específico:
```powershell
cd C:\SistemIA
$env:ASPNETCORE_URLS='http://0.0.0.0:5095'
.\SistemIA.exe
```

## Problema de Cultura/Localización (Decimales)

### Síntoma
Error en consola del navegador:
```
The specified value "1,05" cannot be parsed, or is out of range.
```

### Causa
El servidor usa cultura española/paraguaya donde el separador decimal es **coma** (`,`), pero `<input type="number">` de HTML siempre espera **punto** (`.`).

### Solución
Usar `CultureInfo.InvariantCulture` en los valores de inputs numéricos:

```razor
<!-- INCORRECTO -->
<input type="number" value="@factorPrecio" />

<!-- CORRECTO -->
<input type="number" value="@(factorPrecio?.ToString(System.Globalization.CultureInfo.InvariantCulture))" />
```

### Archivos afectados
- `Pages/Productos.razor`: Campos Costo, Factor, Porcentaje, Precio

## Tamaños típicos de paquetes

| Tipo | Tamaño aprox. |
|------|---------------|
| Self-contained (win-x64) | ~85-90 MB |
| Framework-dependent | ~200-210 MB |

## Scripts del Instalador

### CrearBaseDatos.sql
Script idempotente generado desde EF Core migrations. **Regenerar después de cada migración**:
```powershell
dotnet ef migrations script --idempotent -o "Installer\CrearBaseDatos.sql"
```

### LimpiarDatos.sql
Script para limpiar datos transaccionales manteniendo catálogos.

**Se ELIMINA:**
- Ventas, Compras, Presupuestos, Notas de Crédito
- Productos, Stock, Movimientos
- Clientes (excepto ID 1), Proveedores (excepto ID 1)
- Timbrados, Pagos, Cobros, Cierres de caja
- Asistencias, Auditoría

**NO se elimina (Catálogos):**
- Sociedades, Sucursales, Cajas, Depósitos
- Usuarios, Roles, Permisos
- Monedas, Tipos IVA, Tipos Pago
- Marcas, Clasificaciones
- Catálogos geográficos
- Actividades económicas
- **RucDnit** (1.5M registros de DNIT)

## Fecha de documentación
28 de diciembre de 2025

