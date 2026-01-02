using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para calcular y aplicar descuentos automáticos basados en la configuración del sistema.
    /// </summary>
    public class DescuentoService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ILogger<DescuentoService> _logger;

        public DescuentoService(IDbContextFactory<AppDbContext> dbFactory, ILogger<DescuentoService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el porcentaje de descuento automático aplicable a un producto.
        /// Orden de prioridad:
        /// 1. Si el producto tiene UsaDescuentoEspecifico = true → DescuentoAutomaticoProducto
        /// 2. Descuento por Marca (mayor prioridad primero)
        /// 3. Descuento por Clasificación (mayor prioridad primero)
        /// 4. Descuento general "TODOS" (mayor prioridad primero)
        /// </summary>
        /// <param name="producto">Producto para calcular el descuento</param>
        /// <returns>Porcentaje de descuento (0-100) o null si no hay descuento configurado</returns>
        public async Task<decimal?> ObtenerDescuentoAutomaticoAsync(Producto producto)
        {
            if (producto == null || !producto.PermiteDescuento)
                return null;

            // 1. Si el producto tiene descuento específico habilitado
            if (producto.UsaDescuentoEspecifico && producto.DescuentoAutomaticoProducto.HasValue)
            {
                return producto.DescuentoAutomaticoProducto.Value;
            }

            // Buscar en la configuración general
            await using var db = await _dbFactory.CreateDbContextAsync();

            // Obtener todos los descuentos activos ordenados por prioridad
            var descuentos = await db.DescuentosCategorias
                .Where(d => d.Activo)
                .OrderByDescending(d => d.Prioridad)
                .ToListAsync();

            if (!descuentos.Any())
                return null;

            // 2. Buscar descuento por Marca
            if (producto.IdMarca.HasValue)
            {
                var descuentoMarca = descuentos
                    .FirstOrDefault(d => d.TipoCategoria == "MARCA" && d.IdMarca == producto.IdMarca);
                
                if (descuentoMarca != null)
                    return descuentoMarca.PorcentajeDescuento;
            }

            // 3. Buscar descuento por Clasificación
            if (producto.IdClasificacion.HasValue)
            {
                var descuentoClasif = descuentos
                    .FirstOrDefault(d => d.TipoCategoria == "CLASIFICACION" && d.IdClasificacion == producto.IdClasificacion);
                
                if (descuentoClasif != null)
                    return descuentoClasif.PorcentajeDescuento;
            }

            // 4. Buscar descuento general "TODOS"
            var descuentoTodos = descuentos
                .FirstOrDefault(d => d.TipoCategoria == "TODOS");
            
            if (descuentoTodos != null)
                return descuentoTodos.PorcentajeDescuento;

            return null;
        }

        /// <summary>
        /// Obtiene el descuento automático por ID de producto.
        /// </summary>
        public async Task<decimal?> ObtenerDescuentoAutomaticoAsync(int idProducto)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var producto = await db.Productos.FindAsync(idProducto);
            
            if (producto == null)
                return null;
                
            return await ObtenerDescuentoAutomaticoAsync(producto);
        }

        /// <summary>
        /// Valida si un descuento está dentro de los límites permitidos.
        /// </summary>
        /// <param name="producto">Producto al que se aplica el descuento</param>
        /// <param name="porcentajeDescuento">Porcentaje de descuento solicitado</param>
        /// <param name="precioVenta">Precio de venta antes del descuento</param>
        /// <returns>Tupla (esValido, mensaje)</returns>
        public async Task<(bool EsValido, string Mensaje)> ValidarDescuentoAsync(
            Producto producto, 
            decimal porcentajeDescuento, 
            decimal precioVenta)
        {
            if (producto == null)
                return (false, "Producto no válido");

            if (!producto.PermiteDescuento)
                return (false, "Este producto no permite descuentos");

            if (porcentajeDescuento < 0 || porcentajeDescuento > 100)
                return (false, "El porcentaje debe estar entre 0 y 100");

            // Verificar máximo del producto
            if (producto.DescuentoMaximoProducto.HasValue && 
                porcentajeDescuento > producto.DescuentoMaximoProducto.Value)
            {
                return (false, $"El descuento máximo para este producto es {producto.DescuentoMaximoProducto:N2}%");
            }

            // Verificar máximo del sistema
            await using var db = await _dbFactory.CreateDbContextAsync();
            var config = await db.ConfiguracionSistema.FirstOrDefaultAsync();

            if (config?.PorcentajeDescuentoMaximo.HasValue == true &&
                porcentajeDescuento > config.PorcentajeDescuentoMaximo.Value)
            {
                return (false, $"El descuento máximo del sistema es {config.PorcentajeDescuentoMaximo:N2}%");
            }

            // Verificar que no baje del costo
            var precioConDescuento = precioVenta * (1 - porcentajeDescuento / 100);
            if (producto.CostoUnitarioGs > 0 && precioConDescuento < producto.CostoUnitarioGs)
            {
                return (false, $"El descuento no puede resultar en un precio menor al costo (₲ {producto.CostoUnitarioGs:N0})");
            }

            return (true, "OK");
        }

        /// <summary>
        /// Obtiene la información completa de descuento aplicable a un producto.
        /// Incluye: descuento base + margen adicional para cajero.
        /// </summary>
        /// <param name="producto">Producto para obtener info de descuento</param>
        /// <returns>Tupla con: TieneDescuentoConfigurado, DescuentoBase, MargenAdicionalCajero, DescuentoMaximoTotal, Origen</returns>
        public async Task<(bool TieneDescuento, decimal DescuentoBase, decimal MargenCajero, decimal MaximoTotal, string Origen)> 
            ObtenerInfoDescuentoCompletoAsync(Producto producto)
        {
            if (producto == null || !producto.PermiteDescuento)
                return (false, 0, 0, 0, "No permitido");

            // 1. Si el producto tiene descuento específico habilitado
            if (producto.UsaDescuentoEspecifico && producto.DescuentoAutomaticoProducto.HasValue)
            {
                var descBase = producto.DescuentoAutomaticoProducto.Value;
                var margen = producto.MargenAdicionalCajeroProducto ?? 0;
                return (true, descBase, margen, descBase + margen, "Producto específico");
            }

            // Buscar en la configuración general
            await using var db = await _dbFactory.CreateDbContextAsync();

            // Obtener todos los descuentos activos ordenados por prioridad
            var descuentos = await db.DescuentosCategorias
                .Where(d => d.Activo)
                .OrderByDescending(d => d.Prioridad)
                .ToListAsync();

            if (!descuentos.Any())
                return (false, 0, 0, 0, "Sin configuración");

            // 2. Buscar descuento por Marca
            if (producto.IdMarca.HasValue)
            {
                var descuentoMarca = descuentos
                    .FirstOrDefault(d => d.TipoCategoria == "MARCA" && d.IdMarca == producto.IdMarca);
                
                if (descuentoMarca != null)
                {
                    var margen = descuentoMarca.MargenAdicionalCajero;
                    return (true, descuentoMarca.PorcentajeDescuento, margen, 
                        descuentoMarca.PorcentajeDescuento + margen, $"Marca");
                }
            }

            // 3. Buscar descuento por Clasificación
            if (producto.IdClasificacion.HasValue)
            {
                var descuentoClasif = descuentos
                    .FirstOrDefault(d => d.TipoCategoria == "CLASIFICACION" && d.IdClasificacion == producto.IdClasificacion);
                
                if (descuentoClasif != null)
                {
                    var margen = descuentoClasif.MargenAdicionalCajero;
                    return (true, descuentoClasif.PorcentajeDescuento, margen,
                        descuentoClasif.PorcentajeDescuento + margen, $"Clasificación");
                }
            }

            // 4. Buscar descuento general "TODOS"
            var descuentoTodos = descuentos
                .FirstOrDefault(d => d.TipoCategoria == "TODOS");
            
            if (descuentoTodos != null)
            {
                var margen = descuentoTodos.MargenAdicionalCajero;
                return (true, descuentoTodos.PorcentajeDescuento, margen,
                    descuentoTodos.PorcentajeDescuento + margen, "Todos");
            }

            return (false, 0, 0, 0, "Sin configuración");
        }
    }
}
