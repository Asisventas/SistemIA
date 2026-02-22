using SistemIA.Models;
using SistemIA.Models.AsistenteIA;
using Microsoft.EntityFrameworkCore;

namespace SistemIA.Services
{
    public interface IDataInitializationService
    {
        Task InicializarDatosListasPreciosAsync();
        Task InicializarGeografiaSifenAsync();
        Task<bool> ImportarCatalogoGeograficoAhoraAsync();
        Task InicializarArticulosAsistenteIAAsync();
        Task InicializarConfiguracionVPNAsync();
    }

    public class DataInitializationService : IDataInitializationService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly ILogger<DataInitializationService> _logger;

        public DataInitializationService(IDbContextFactory<AppDbContext> dbFactory, ILogger<DataInitializationService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

    public async Task InicializarDatosListasPreciosAsync()
        {
            try
            {
                await using var context = await _dbFactory.CreateDbContextAsync();

                // 1. Crear monedas iniciales
                await CrearMonedasInicialesAsync(context);

                // 1.b Normalizar monedas: dejar solo PYG, USD, ARS y BRL
                await AsegurarSoloMonedasPermitidasAsync(context);

                // 2. Crear listas de precios iniciales
                await CrearListasPreciosInicialesAsync(context);

                _logger.LogInformation("Datos iniciales de listas de precios creados exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar datos de listas de precios");
                throw;
            }
        }

        /// <summary>
        /// Seed idempotente de Ciudades y Distritos principales basados en catálogos SIFEN.
        /// NOTA: La lista oficial completa está en el Excel referenciado por el Manual; aquí cargamos un subconjunto útil.
        /// </summary>
    public async Task InicializarGeografiaSifenAsync()
        {
            try
            {
        // Catálogo geográfico ya se carga por nuevas tablas; sin acción aquí.
        await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar geografía SIFEN");
            }
        }

        /// <summary>
        /// Permite forzar la importación del catálogo geográfico desde CSV en tiempo de ejecución.
        /// Devuelve true si se importó, false si no se encontró o falló.
        /// </summary>
        public async Task<bool> ImportarCatalogoGeograficoAhoraAsync()
        {
            try
            {
                await using var context = await _dbFactory.CreateDbContextAsync();
                var ok = await TryImportarCatalogoGeograficoCsvAsync(context);
                if (ok)
                {
                    _logger.LogInformation("Catálogo geográfico SIFEN importado manualmente desde CSV.");
                }
                else
                {
                    _logger.LogWarning("No se pudo importar el catálogo geográfico (¿CSV inexistente?).");
                }
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al importar catálogo geográfico bajo demanda");
                return false;
            }
        }

        /// <summary>
        /// Intenta importar el catálogo geográfico (Departamentos/Ciudades/Distritos) desde un CSV.
        /// Formato esperado (encabezados flexibles, orden común): cDep,dDesDep,cDis,dDesDis,cCiu,dDesCiu
        /// El archivo debe estar en ManualSifen/CODIGO_DE_REFERENCIA_GEOGRAFICA.csv o ManualSifen/catalogo_geografico.csv
        /// </summary>
    private async Task<bool> TryImportarCatalogoGeograficoCsvAsync(AppDbContext context)
        {
            try
            {
        // Importación CSV obsoleta (apuntaba a tablas antiguas). No hacer nada.
        await Task.CompletedTask;
        return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo importando CSV de catálogo geográfico SIFEN");
                return false;
            }
        }

        // CSV básico: soporta comillas dobles y comas dentro de comillas
        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false; var current = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"'); i++; // escapar ""
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (ch == ',' && !inQuotes)
                {
                    result.Add(current.ToString()); current.Clear();
                }
                else
                {
                    current.Append(ch);
                }
            }
            result.Add(current.ToString());
            return result.ToArray();
        }

        private static string ToTitulo(string s)
        {
            // Normalización simple a Título preservando acentos
            s = s.Trim();
            if (string.IsNullOrEmpty(s)) return s;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }

        private async Task CrearMonedasInicialesAsync(AppDbContext context)
        {
            // Verificar si ya existen monedas
            if (await context.Monedas.AnyAsync())
            {
                _logger.LogInformation("Las monedas ya existen, omitiendo creación inicial");
                return;
            }

            var monedas = new List<Moneda>
            {
                new Moneda
                {
                    CodigoISO = "PYG",
                    Nombre = "Guaraní Paraguayo",
                    Simbolo = "₲",
                    EsMonedaBase = true,
                    Estado = true,
                    Orden = 1,
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacion = "Sistema"
                },
                new Moneda
                {
                    CodigoISO = "USD",
                    Nombre = "Dólar Estadounidense",
                    Simbolo = "$",
                    EsMonedaBase = false,
                    Estado = true,
                    Orden = 2,
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacion = "Sistema"
                },
                new Moneda
                {
                    CodigoISO = "ARS",
                    Nombre = "Peso Argentino",
                    Simbolo = "$",
                    EsMonedaBase = false,
                    Estado = true,
                    Orden = 3,
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacion = "Sistema"
                },
                new Moneda
                {
                    CodigoISO = "BRL",
                    Nombre = "Real Brasileño",
                    Simbolo = "R$",
                    EsMonedaBase = false,
                    Estado = true,
                    Orden = 4,
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacion = "Sistema"
                }
            };

            context.Monedas.AddRange(monedas);
            await context.SaveChangesAsync();

            _logger.LogInformation($"Creadas {monedas.Count} monedas iniciales: PYG, USD, ARS, BRL");
        }

        /// <summary>
        /// Deja únicamente las monedas permitidas (PYG, USD, ARS, BRL). Elimina Tipos de Cambio para monedas no permitidas.
        /// Intenta eliminar monedas no permitidas que no tengan referencias; si tienen, las desactiva (Estado = 0).
        /// Idempotente y seguro para ejecutar en cada arranque.
        /// </summary>
        private async Task AsegurarSoloMonedasPermitidasAsync(AppDbContext context)
        {
            var permitidas = new[] { "PYG", "USD", "ARS", "BRL" };

            // Asegurar datos canónicos de las 4 permitidas (upsert sencillo)
            var definiciones = new List<Moneda>
            {
                new Moneda{ CodigoISO="PYG", Nombre="Guaraní Paraguayo", Simbolo="₲", EsMonedaBase=true, Estado=true, Orden=1 },
                new Moneda{ CodigoISO="USD", Nombre="Dólar Estadounidense", Simbolo="$", EsMonedaBase=false, Estado=true, Orden=2 },
                new Moneda{ CodigoISO="ARS", Nombre="Peso Argentino", Simbolo="$", EsMonedaBase=false, Estado=true, Orden=3 },
                new Moneda{ CodigoISO="BRL", Nombre="Real Brasileño", Simbolo="R$", EsMonedaBase=false, Estado=true, Orden=4 }
            };

            foreach (var def in definiciones)
            {
                var existente = await context.Monedas.FirstOrDefaultAsync(m => m.CodigoISO == def.CodigoISO);
                if (existente == null)
                {
                    def.FechaCreacion = DateTime.Now;
                    def.UsuarioCreacion = "Sistema";
                    context.Monedas.Add(def);
                }
                else
                {
                    existente.Nombre = def.Nombre;
                    existente.Simbolo = def.Simbolo;
                    existente.EsMonedaBase = def.EsMonedaBase;
                    existente.Estado = true; // activar
                    existente.Orden = def.Orden;
                    existente.FechaModificacion = DateTime.Now;
                    existente.UsuarioModificacion = "Sistema";
                }
            }

            await context.SaveChangesAsync();

            // 1) Borrar TiposCambio/Histórico que involucren monedas no permitidas
            // Se hace por SQL para eficiencia
            var sqlPurgeTipos = @"
DELETE tc FROM TiposCambio tc
WHERE tc.IdMonedaOrigen IN (SELECT IdMoneda FROM Monedas WHERE CodigoISO NOT IN ('PYG','USD','ARS','BRL'))
   OR tc.IdMonedaDestino IN (SELECT IdMoneda FROM Monedas WHERE CodigoISO NOT IN ('PYG','USD','ARS','BRL'));

DELETE th FROM TiposCambioHistorico th
WHERE th.IdMonedaOrigen IN (SELECT IdMoneda FROM Monedas WHERE CodigoISO NOT IN ('PYG','USD','ARS','BRL'))
   OR th.IdMonedaDestino IN (SELECT IdMoneda FROM Monedas WHERE CodigoISO NOT IN ('PYG','USD','ARS','BRL'));
";
            try { await context.Database.ExecuteSqlRawAsync(sqlPurgeTipos); } catch { /* best-effort */ }

            // 2) Intentar eliminar monedas no permitidas sin referencias fuertes
            var sqlDeleteMonedas = @"
DELETE m
FROM Monedas m
WHERE m.CodigoISO NOT IN ('PYG','USD','ARS','BRL')
  AND NOT EXISTS (SELECT 1 FROM Compras c WHERE c.IdMoneda = m.IdMoneda)
  AND NOT EXISTS (SELECT 1 FROM ListasPrecios lp WHERE lp.IdMoneda = m.IdMoneda)
  AND NOT EXISTS (SELECT 1 FROM Productos p WHERE p.IdMonedaPrecio = m.IdMoneda);
";
            try { await context.Database.ExecuteSqlRawAsync(sqlDeleteMonedas); } catch { /* restricción FK: continuar */ }

            // 3) Cualquier moneda no permitida restante (por referencias) se desactiva
            var restantes = await context.Monedas
                .Where(m => !permitidas.Contains(m.CodigoISO))
                .ToListAsync();
            if (restantes.Any())
            {
                foreach (var m in restantes)
                {
                    m.Estado = false;
                    m.FechaModificacion = DateTime.Now;
                    m.UsuarioModificacion = "Sistema";
                }
                await context.SaveChangesAsync();
                _logger.LogInformation($"Monedas no permitidas desactivadas: {string.Join(", ", restantes.Select(r=>r.CodigoISO))}");
            }
            else
            {
                _logger.LogInformation("No hay monedas no permitidas activas");
            }
        }

        private async Task CrearListasPreciosInicialesAsync(AppDbContext context)
        {
            // Verificar si ya existen listas de precios
            if (await context.ListasPrecios.AnyAsync())
            {
                _logger.LogInformation("Las listas de precios ya existen, omitiendo creación inicial");
                return;
            }

            // Obtener las monedas creadas
            var monedas = await context.Monedas.Where(m=>m.Estado).ToListAsync();
            var monedaPYG = monedas.First(m => m.CodigoISO == "PYG");
            var monedaUSD = monedas.First(m => m.CodigoISO == "USD");
            var monedaARS = monedas.First(m => m.CodigoISO == "ARS");
            var monedaBRL = monedas.First(m => m.CodigoISO == "BRL");

            var listasPrecios = new List<ListaPrecio>
            {
                new ListaPrecio
                {
                    Nombre = "Lista General Guaraníes",
                    Descripcion = "Lista de precios principal en Guaraníes Paraguayos",
                    IdMoneda = monedaPYG.IdMoneda,
                    EsPredeterminada = true,
                    Estado = true,
                    AplicarDescuentoGlobal = false,
                    PorcentajeDescuento = 0,
                    Orden = 1,
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacion = "Sistema"
                },
                new ListaPrecio
                {
                    Nombre = "Lista Precios USD",
                    Descripcion = "Lista de precios en Dólares Estadounidenses",
                    IdMoneda = monedaUSD.IdMoneda,
                    EsPredeterminada = false,
                    Estado = true,
                    AplicarDescuentoGlobal = false,
                    PorcentajeDescuento = 0,
                    Orden = 2,
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacion = "Sistema"
                },
                new ListaPrecio
                {
                    Nombre = "Lista Precios ARS",
                    Descripcion = "Lista de precios en Pesos Argentinos",
                    IdMoneda = monedaARS.IdMoneda,
                    EsPredeterminada = false,
                    Estado = true,
                    AplicarDescuentoGlobal = false,
                    PorcentajeDescuento = 0,
                    Orden = 3,
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacion = "Sistema"
                },
                new ListaPrecio
                {
                    Nombre = "Lista Precios BRL",
                    Descripcion = "Lista de precios en Reales Brasileños",
                    IdMoneda = monedaBRL.IdMoneda,
                    EsPredeterminada = false,
                    Estado = true,
                    AplicarDescuentoGlobal = false,
                    PorcentajeDescuento = 0,
                    Orden = 4,
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacion = "Sistema"
                }
            };

            context.ListasPrecios.AddRange(listasPrecios);
            await context.SaveChangesAsync();

            _logger.LogInformation($"Creadas {listasPrecios.Count} listas de precios iniciales para todas las monedas");
        }

        /// <summary>
        /// Inicializa los artículos de conocimiento del Asistente IA si la tabla está vacía.
        /// Los datos existentes del cliente NO se sobrescriben.
        /// </summary>
        public async Task InicializarArticulosAsistenteIAAsync()
        {
            try
            {
                await using var context = await _dbFactory.CreateDbContextAsync();
                
                // Obtener artículos iniciales del código
                var articulosIniciales = ObtenerArticulosIniciales();
                
                // Obtener títulos existentes en la BD del cliente
                var titulosExistentes = await context.ArticulosConocimiento
                    .Select(a => a.Titulo)
                    .ToListAsync();
                
                // Filtrar solo los artículos NUEVOS (que no existen por título)
                var articulosNuevos = articulosIniciales
                    .Where(a => !titulosExistentes.Contains(a.Titulo))
                    .ToList();
                
                if (articulosNuevos.Count == 0)
                {
                    _logger.LogInformation("Todos los artículos del Asistente IA ya existen, nada que sincronizar");
                    return;
                }

                _logger.LogInformation($"Sincronizando {articulosNuevos.Count} artículo(s) nuevo(s) del Asistente IA...");

                context.ArticulosConocimiento.AddRange(articulosNuevos);
                await context.SaveChangesAsync();

                _logger.LogInformation($"Se agregaron {articulosNuevos.Count} artículo(s) nuevo(s) del Asistente IA. Total en código: {articulosIniciales.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sincronizar artículos del Asistente IA");
            }
        }

        private static List<ArticuloConocimientoDB> ObtenerArticulosIniciales()
        {
            var ahora = DateTime.Now;
            return new List<ArticuloConocimientoDB>
            {
                // ========== VENTAS ==========
                new()
                {
                    Categoria = "Ventas", Subcategoria = "Operaciones", Titulo = "Crear una nueva venta",
                    Contenido = @"Para **crear una nueva venta**, sigue estos pasos:

1️⃣ Ve a **Ventas → Nueva Venta** o presiona el acceso directo
2️⃣ Selecciona el **cliente** (puedes buscar por nombre o RUC)
3️⃣ Agrega productos usando el **buscador** o escaneando código de barras
4️⃣ Ajusta las **cantidades** si es necesario
5️⃣ Selecciona la **forma de pago**: Contado, Crédito, etc.
6️⃣ Haz clic en **Confirmar Venta**

💡 **Tips**:
- Usa F2 para buscar cliente rápidamente
- Usa F3 para buscar producto
- Verifica el total antes de confirmar
- Si es crédito, define las cuotas y vencimientos",
                    PalabrasClave = "venta, factura, vender, facturar, nueva venta, crear venta",
                    RutaNavegacion = "/ventas", Icono = "bi-cart", Prioridad = 9,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Ventas", Subcategoria = "Operaciones", Titulo = "Anular una venta",
                    Contenido = @"Para **anular una venta**, sigue estos pasos:

1️⃣ Ve a **Ventas → Explorador de Ventas**
2️⃣ Busca la venta por número, fecha o cliente
3️⃣ Haz clic en la venta para ver detalle
4️⃣ Presiona el botón **Anular** (icono de papelera)
5️⃣ Confirma la anulación

⚠️ **Importante**:
- Solo puedes anular ventas del día actual
- Si ya pasó el día, debes crear una **Nota de Crédito**
- Las ventas enviadas a SIFEN no se pueden anular directamente
- Al anular, el stock se devuelve automáticamente",
                    PalabrasClave = "anular venta, cancelar venta, eliminar venta, borrar factura",
                    RutaNavegacion = "/ventas/explorar", Icono = "bi-x-circle", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Ventas", Subcategoria = "Notas de Crédito", Titulo = "Crear Nota de Crédito",
                    Contenido = @"Para **crear una Nota de Crédito** (devolución):

1️⃣ Ve a **Ventas → Notas de Crédito**
2️⃣ Haz clic en **Nueva NC**
3️⃣ Busca la **factura original** a la que aplicar la NC
4️⃣ Selecciona los **productos** a devolver
5️⃣ Ajusta las **cantidades** devueltas
6️⃣ Indica el **motivo** de la devolución
7️⃣ **Confirma** la Nota de Crédito

💡 **Opciones**:
- NC Total: devuelve toda la factura
- NC Parcial: devuelve solo algunos productos
- El stock se restaura automáticamente",
                    PalabrasClave = "nota credito, devolucion, nc, credito, devolver producto, anular factura anterior",
                    RutaNavegacion = "/notas-credito", Icono = "bi-receipt-cutoff", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Ventas", Subcategoria = "Presupuestos", Titulo = "Crear un presupuesto",
                    Contenido = @"Para **crear un presupuesto**:

1️⃣ Ve a **Ventas → Presupuestos**
2️⃣ Click en **Nuevo Presupuesto**
3️⃣ Selecciona el **cliente**
4️⃣ Agrega los **productos** con precios y cantidades
5️⃣ Define la **validez** del presupuesto (días)
6️⃣ **Guarda** el presupuesto

**Opciones posteriores**:
- ✅ **Convertir a Venta**: cuando el cliente acepta
- 📧 **Enviar por correo**: al cliente
- 🖨️ **Imprimir**: para entregar físicamente

💡 Los presupuestos no afectan stock ni generan movimientos fiscales.",
                    PalabrasClave = "presupuesto, cotizacion, proforma, precio estimado, crear presupuesto",
                    RutaNavegacion = "/presupuestos/explorar", Icono = "bi-file-earmark-text", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },

                // ========== COMPRAS ==========
                new()
                {
                    Categoria = "Compras", Subcategoria = "Operaciones", Titulo = "Registrar una compra",
                    Contenido = @"Para **registrar una compra**, sigue estos pasos:

1️⃣ Ve a **Compras → Nueva Compra**
2️⃣ Selecciona el **proveedor**
3️⃣ Ingresa el **número de factura** del proveedor
4️⃣ Agrega los **productos** comprados
5️⃣ Verifica los **precios de costo** y cantidades
6️⃣ Selecciona **Contado o Crédito**
7️⃣ **Confirma** la compra

💡 **Tips**:
- Los precios de costo se actualizan automáticamente
- El stock se suma al confirmar
- Puedes adjuntar imagen de la factura del proveedor",
                    PalabrasClave = "compra, comprar, nueva compra, registrar compra, ingreso mercaderia, factura proveedor",
                    RutaNavegacion = "/compras", Icono = "bi-bag", Prioridad = 9,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Compras", Subcategoria = "Pagos", Titulo = "Pagar a proveedores",
                    Contenido = @"Para **registrar un pago a proveedor**:

1️⃣ Ve a **Compras → Pagos a Proveedores**
2️⃣ Selecciona el **proveedor**
3️⃣ Verás las **facturas pendientes** de pago
4️⃣ Selecciona qué facturas vas a pagar
5️⃣ Ingresa el **monto** del pago
6️⃣ Selecciona la **forma de pago** (efectivo, cheque, transferencia)
7️⃣ **Confirma** el pago

📊 Para ver el historial: **Pagos → Historial de Pagos**
📋 Para ver deudas: **Informes → Cuentas por Pagar**",
                    PalabrasClave = "pago proveedor, pagar proveedor, deuda proveedor, cuentas por pagar, pago factura",
                    RutaNavegacion = "/pagos-proveedores", Icono = "bi-cash-coin", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },

                // ========== CAJA ==========
                new()
                {
                    Categoria = "Caja", Subcategoria = "Operaciones", Titulo = "Cierre de caja",
                    Contenido = @"Para **realizar el cierre de caja**:

1️⃣ Ve a **Ventas → Cierre de Caja**
2️⃣ Verifica que todas las ventas estén **confirmadas**
3️⃣ Revisa el **resumen de operaciones**:
   - Ventas del turno
   - Cobros recibidos
   - Pagos realizados
   - Notas de crédito emitidas
4️⃣ Ingresa el **efectivo contado** físicamente
5️⃣ El sistema calcula la **diferencia** (sobrante/faltante)
6️⃣ **Confirma** el cierre

💡 **Recomendaciones**:
- Cierra caja al final de cada turno
- Revisa los informes antes de cerrar
- Documenta cualquier diferencia encontrada",
                    PalabrasClave = "cierre caja, cerrar caja, arqueo, cuadrar caja, diferencia caja, sobrante, faltante",
                    RutaNavegacion = "/caja/cierre", Icono = "bi-cash-stack", Prioridad = 9,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Caja", Subcategoria = "Configuración", Titulo = "Cambiar turno de caja",
                    Contenido = @"Para **cambiar de turno** en la caja:

1️⃣ Primero realiza el **cierre del turno actual**
2️⃣ Ve a **Configuración → Cajas**
3️⃣ Selecciona la caja activa
4️⃣ Cambia el **número de turno** (1, 2, 3...)
5️⃣ Guarda los cambios

⚠️ **Importante**:
- Cada turno tiene su propio cierre independiente
- El historial de cierres separa por turno
- Configura la cantidad de turnos en la configuración de caja",
                    PalabrasClave = "turno, cambiar turno, siguiente turno, turno caja, turno mañana, turno tarde",
                    RutaNavegacion = "/configuracion/cajas", Icono = "bi-clock", Prioridad = 7,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },

                // ========== INVENTARIO ==========
                new()
                {
                    Categoria = "Inventario", Subcategoria = "Stock", Titulo = "Ajustar stock de productos",
                    Contenido = @"Para **ajustar el stock** de productos:

1️⃣ Ve a **Inventario → Ajustes de Stock**
2️⃣ Selecciona el **depósito** a ajustar
3️⃣ Busca el **producto**
4️⃣ Ingresa la **cantidad nueva** o el **ajuste (+/-)**
5️⃣ Selecciona el **motivo**:
   - Inventario físico
   - Merma/rotura
   - Vencimiento
   - Error de conteo
   - Otro
6️⃣ **Confirma** el ajuste

📊 Para ver historial: **Informes → Ajustes de Stock**",
                    PalabrasClave = "ajuste stock, ajustar inventario, modificar stock, corregir stock, merma, perdida, inventario fisico",
                    RutaNavegacion = "/inventario/ajustes", Icono = "bi-box-seam", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Inventario", Subcategoria = "Stock", Titulo = "Transferir stock entre depósitos",
                    Contenido = @"Para **transferir productos** entre depósitos:

1️⃣ Ve a **Inventario → Transferencias**
2️⃣ Selecciona el **depósito origen**
3️⃣ Selecciona el **depósito destino**
4️⃣ Agrega los **productos** a transferir
5️⃣ Indica las **cantidades**
6️⃣ **Confirma** la transferencia

💡 **Nota**: El stock se resta del origen y se suma al destino inmediatamente.",
                    PalabrasClave = "transferir stock, mover productos, transferencia deposito, enviar mercaderia, traslado",
                    RutaNavegacion = "/inventario/transferencias", Icono = "bi-arrow-left-right", Prioridad = 7,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },

                // ========== CLIENTES Y COBROS ==========
                new()
                {
                    Categoria = "Clientes", Subcategoria = "Cobros", Titulo = "Cobrar cuotas a clientes",
                    Contenido = @"Para **registrar un cobro** de cliente:

1️⃣ Ve a **Ventas → Cuentas por Cobrar**
2️⃣ Selecciona el **cliente**
3️⃣ Verás las **cuotas pendientes**
4️⃣ Selecciona las cuotas a cobrar
5️⃣ Ingresa el **monto recibido**
6️⃣ Selecciona la **forma de pago**
7️⃣ **Confirma** el cobro

📊 Para ver historial: **Cobros → Historial de Cobros**
📋 Para ver deudas: **Informes → Cuentas por Cobrar**",
                    PalabrasClave = "cobro, cobrar cliente, cuota, deuda cliente, credito, cuentas por cobrar, pago cliente",
                    RutaNavegacion = "/cobros", Icono = "bi-currency-dollar", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },

                // ========== CONFIGURACIÓN ==========
                new()
                {
                    Categoria = "Configuración", Subcategoria = "Empresa", Titulo = "Configurar datos de la empresa",
                    Contenido = @"Para **configurar los datos de la empresa**:

1️⃣ Ve a **Configuración → Sociedad/Empresa**
2️⃣ Completa los datos:
   - **Razón Social**: nombre legal de la empresa
   - **RUC**: número de contribuyente
   - **Dirección**: dirección fiscal
   - **Teléfono** y **correo**
3️⃣ Sube el **logo** de la empresa
4️⃣ **Guarda** los cambios

💡 Estos datos aparecen en facturas y documentos impresos.",
                    PalabrasClave = "empresa, sociedad, razon social, ruc, datos empresa, configurar empresa, logo",
                    RutaNavegacion = "/configuracion/sociedad", Icono = "bi-building", Prioridad = 7,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Configuración", Subcategoria = "SIFEN", Titulo = "Configurar timbrado y facturación electrónica",
                    Contenido = @"Para **configurar SIFEN** (Facturación Electrónica):

**1. Configurar Certificado Digital:**
- Ve a **Configuración → Sociedad**
- Carga el archivo **.pfx** del certificado
- Ingresa la **contraseña** del certificado

**2. Configurar Timbrado:**
- Ve a **Configuración → Cajas**
- Ingresa el **número de timbrado**
- Configura la **vigencia** (desde/hasta)
- Define el **número inicial** de facturas

**3. Seleccionar Ambiente:**
- **Test**: para pruebas (no válido fiscalmente)
- **Producción**: facturas reales

⚠️ El certificado debe estar vigente y ser emitido por el SET.",
                    PalabrasClave = "sifen, timbrado, factura electronica, certificado, set, cdc, vigencia, ambiente",
                    RutaNavegacion = "/configuracion/cajas", Icono = "bi-patch-check", Prioridad = 9,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Configuración", Subcategoria = "Correo", Titulo = "Configurar envío automático de correo",
                    Contenido = @"Para configurar el **envío automático de correo**:

1️⃣ Ve a **Configuración → Correo Electrónico**
2️⃣ Configura el **servidor SMTP**:
   - Servidor: smtp.gmail.com (para Gmail)
   - Puerto: 587
   - Usar SSL: Sí
3️⃣ Ingresa las **credenciales**:
   - Usuario: tu correo
   - Contraseña: contraseña de aplicación (16 caracteres)
4️⃣ Configura los **destinatarios** y qué informes reciben
5️⃣ Activa **Enviar al cierre** o **Resumen diario**

💡 **Para Gmail**: 
- Activa verificación en 2 pasos
- Crea contraseña de aplicación en seguridad de Google
- Usa esa contraseña (xxxx xxxx xxxx xxxx) en el sistema",
                    PalabrasClave = "correo, email, smtp, enviar correo, notificacion, gmail, outlook, informe email",
                    RutaNavegacion = "/configuracion/correo", Icono = "bi-envelope", Prioridad = 9,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },

                // ========== SISTEMA Y BACKUP ==========
                new()
                {
                    Categoria = "Sistema", Subcategoria = "Backup", Titulo = "Hacer backup de la base de datos",
                    Contenido = @"Para **realizar un backup** de la base de datos:

**Opción 1 - Desde SQL Server Management Studio:**
1. Abre SSMS y conecta al servidor
2. Click derecho en la base de datos **asiswebapp**
3. Tareas → **Copia de seguridad**
4. Selecciona destino y nombre del archivo .bak
5. Click en **Aceptar**

**Opción 2 - Comando SQL:**
```sql
BACKUP DATABASE asiswebapp 
TO DISK = 'C:\Backups\asiswebapp_YYYYMMDD.bak'
WITH FORMAT, COMPRESSION;
```

💡 **Recomendaciones**:
- Haz backup **diario** al menos
- Guarda copias en **ubicación externa** (nube, disco externo)
- Prueba restaurar periódicamente para verificar
- Programa backups automáticos en SQL Server Agent",
                    PalabrasClave = "backup, copia seguridad, respaldo, guardar datos, respaldar, base datos, bak",
                    RutaNavegacion = null, Icono = "bi-hdd", Prioridad = 10,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Sistema", Subcategoria = "Backup", Titulo = "Restaurar backup de base de datos",
                    Contenido = @"Para **restaurar un backup**:

**Desde SQL Server Management Studio:**
1. Abre SSMS
2. Click derecho en **Bases de datos**
3. **Restaurar base de datos...**
4. Selecciona **Dispositivo** → busca el archivo .bak
5. Verifica el nombre de la base de datos destino
6. Click en **Aceptar**

⚠️ **Importante**:
- Cierra la aplicación antes de restaurar
- La restauración **sobrescribe** todos los datos actuales
- Haz un backup del estado actual antes de restaurar
- Si la BD está en uso, marca 'Cerrar conexiones existentes'",
                    PalabrasClave = "restaurar, restore, recuperar, recuperar backup, cargar backup, reestablecer",
                    RutaNavegacion = null, Icono = "bi-arrow-counterclockwise", Prioridad = 9,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Sistema", Subcategoria = "Mantenimiento", Titulo = "Actualizar el sistema",
                    Contenido = @"Para **actualizar SistemIA**:

1️⃣ Ve a **Configuración → Actualización Sistema**
2️⃣ Haz clic en **Buscar Actualizaciones**
3️⃣ Si hay versión nueva disponible:
   - Revisa las **notas de la versión**
   - Haz **backup** antes de actualizar
   - Click en **Descargar e Instalar**
4️⃣ Reinicia la aplicación cuando termine

💡 **Recomendaciones**:
- Siempre haz backup antes de actualizar
- No interrumpas el proceso de actualización
- Actualiza fuera de horario pico
- Lee las notas de versión por cambios importantes",
                    PalabrasClave = "actualizar, update, version, nueva version, parche, actualizacion sistema",
                    RutaNavegacion = "/actualizacion-sistema", Icono = "bi-cloud-download", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },

                // ========== INFORMES ==========
                new()
                {
                    Categoria = "Informes", Subcategoria = "Ventas", Titulo = "Generar informes de ventas",
                    Contenido = @"Para **generar informes de ventas**:

1️⃣ Ve a **Informes** en el menú principal
2️⃣ Selecciona el tipo de informe:

📊 **Ventas Agrupado**: totales por día/vendedor/forma de pago
📋 **Ventas Detallado**: cada venta con sus productos, **lote y vencimiento**
📈 **Ventas por Clasificación**: agrupado por categoría de producto
💰 **Resumen de Caja**: movimientos de efectivo

3️⃣ Selecciona el **rango de fechas**
4️⃣ Aplica **filtros** (cliente, vendedor, etc.)
5️⃣ Click en **Generar**

💡 Puedes **exportar a Excel** o **imprimir** los informes.

📦 **Nuevo**: El informe detallado ahora muestra columnas de **Lote** y **Vencimiento** para productos con control de lote.",
                    PalabrasClave = "informe venta, reporte venta, estadistica venta, resumen venta, ver ventas, lote venta",
                    RutaNavegacion = "/informes/ventas-agrupado", Icono = "bi-graph-up", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Informes", Subcategoria = "Financieros", Titulo = "Ver cuentas por cobrar",
                    Contenido = @"Para ver las **cuentas por cobrar** (deudas de clientes):

1️⃣ Ve a **Informes → Cuentas por Cobrar**
2️⃣ Filtra por:
   - **Cliente específico** o todos
   - **Estado**: vencidas, por vencer, todas
   - **Rango de fechas**
3️⃣ El informe muestra:
   - Total adeudado por cliente
   - Cuotas pendientes con vencimientos
   - Días de atraso

💡 **Acciones**:
- Click en un cliente para ver detalle
- Exportar a Excel para seguimiento
- Desde aquí puedes ir a registrar cobros",
                    PalabrasClave = "cuentas cobrar, deudas clientes, creditos pendientes, morosos, vencidos, cartera",
                    RutaNavegacion = "/informes/cuentas-por-cobrar", Icono = "bi-people", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },

                // ========== USUARIOS ==========
                new()
                {
                    Categoria = "Usuarios", Subcategoria = "Gestión", Titulo = "Crear nuevo usuario",
                    Contenido = @"Para **crear un nuevo usuario**:

1️⃣ Ve a **Personal → Gestión de Usuarios**
2️⃣ Click en **Nuevo Usuario**
3️⃣ Completa los datos:
   - **Nombre de usuario** (para login)
   - **Contraseña**
   - **Nombres y apellidos**
   - **Rol** (Administrador, Vendedor, etc.)
4️⃣ Configura los **permisos** específicos
5️⃣ **Guarda** el usuario

💡 Los roles determinan los permisos base, pero puedes personalizar permisos individuales.",
                    PalabrasClave = "usuario, crear usuario, nuevo usuario, agregar usuario, empleado, personal",
                    RutaNavegacion = "/menu-usuarios", Icono = "bi-person-plus", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Usuarios", Subcategoria = "Permisos", Titulo = "Configurar permisos de usuario",
                    Contenido = @"Para **configurar permisos**:

1️⃣ Ve a **Personal → Permisos de Usuarios**
2️⃣ Selecciona el **usuario** o **rol**
3️⃣ Marca/desmarca los permisos por módulo:
   - ✅ Ver (acceso al módulo)
   - ✅ Crear (agregar registros)
   - ✅ Editar (modificar)
   - ✅ Eliminar (borrar)
   - ✅ Anular (anular documentos)
4️⃣ **Guarda** los cambios

⚠️ **Roles predefinidos**:
- **Administrador**: acceso total
- **Vendedor**: ventas y cobros
- **Cajero**: solo caja y ventas",
                    PalabrasClave = "permisos, acceso, roles, restriccion, seguridad, configurar permisos",
                    RutaNavegacion = "/personal/permisos-usuarios", Icono = "bi-shield-lock", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },

                // ========== PRODUCTOS ==========
                new()
                {
                    Categoria = "Productos", Subcategoria = "Gestión", Titulo = "Crear nuevo producto",
                    Contenido = @"Para **crear un nuevo producto**:

1️⃣ Ve a **Productos → Administrar Productos**
2️⃣ Click en **Nuevo Producto**
3️⃣ Completa los datos obligatorios:
   - **Código** (único, puede ser código de barras)
   - **Descripción** del producto
   - **Precio de venta**
   - **Tipo de IVA** (10%, 5%, Exenta)
4️⃣ Datos opcionales:
   - Categoría y marca
   - Precio de costo
   - Stock mínimo
   - Imagen del producto
5️⃣ **Guarda** el producto

💡 El stock inicial se carga con una compra o ajuste de inventario.",
                    PalabrasClave = "producto, crear producto, nuevo producto, agregar producto, articulo, item",
                    RutaNavegacion = "/productos", Icono = "bi-box", Prioridad = 9,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Productos", Subcategoria = "Precios", Titulo = "Configurar precios diferenciados",
                    Contenido = @"Para configurar **precios diferenciados** por cliente:

1️⃣ Ve a **Configuración → Precios y Descuentos**
2️⃣ Crea **Listas de Precios** (Mayorista, Minorista, etc.)
3️⃣ Asigna precios específicos por producto en cada lista
4️⃣ Asigna la lista al cliente en su ficha

**Opciones de precio**:
- Precio fijo por lista
- Descuento porcentual sobre precio base
- Precio por cantidad (escalas)

💡 Al vender, el sistema aplica automáticamente el precio de la lista asignada al cliente.",
                    PalabrasClave = "precio, lista precio, descuento, mayorista, minorista, precio especial, cliente precio",
                    RutaNavegacion = "/configuracion/precios-descuentos", Icono = "bi-tags", Prioridad = 7,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },

                // ========== INVENTARIO - LOTES Y VENCIMIENTOS ==========
                new()
                {
                    Categoria = "Inventario", Subcategoria = "Lotes", Titulo = "Gestión de Lotes y Vencimientos (FEFO)",
                    Contenido = @"El sistema soporta **control de lotes con FEFO** (First Expired, First Out) ideal para farmacias y productos perecederos.

## ¿Qué es FEFO?
FEFO significa **""Primero en Vencer, Primero en Salir""**. El sistema automáticamente selecciona el lote más próximo a vencer al realizar una venta.

## Cómo funciona:
1️⃣ Activa **""Controla Lote""** en el producto
2️⃣ Al comprar, ingresa el **número de lote** y **fecha de vencimiento**
3️⃣ Al vender, el sistema selecciona automáticamente el lote que vence primero
4️⃣ El stock se descuenta de ese lote específico

## ⚠️ Importante:
- El control de lotes es **OPCIONAL** y se activa por producto
- Los productos sin control de lote funcionan igual que antes
- Cada lote pertenece a un depósito específico
- El stock total del producto es la suma de todos sus lotes

## Páginas disponibles:
- **Inventario → Gestión de Lotes**: ver todos los lotes
- **Inventario → Alertas de Vencimiento**: productos próximos a vencer",
                    PalabrasClave = "lote, vencimiento, fefo, farmacia, lotes, fecha vencimiento, control lote, perecedero, caducidad",
                    RutaNavegacion = "/inventario/lotes", Icono = "bi-box-seam", Prioridad = 9,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Inventario", Subcategoria = "Lotes", Titulo = "Crear un nuevo lote de producto",
                    Contenido = @"Para **crear un nuevo lote** de producto:

1️⃣ Ve a **Inventario → Gestión de Lotes**
2️⃣ Click en **Nuevo Lote**
3️⃣ Busca el **producto** (debe tener ""Controla Lote"" activado)
4️⃣ Ingresa:
   - **Número de Lote** (ej: LOT-2026-001)
   - **Fecha de Vencimiento**
   - **Depósito** donde estará el stock
   - **Stock Inicial** (cantidad)
5️⃣ **Guarda** el lote

💡 **Tips**:
- También puedes crear lotes automáticamente al registrar una compra
- El número de lote suele venir impreso en el producto
- Los lotes sin stock se pueden eliminar",
                    PalabrasClave = "crear lote, nuevo lote, agregar lote, registrar lote, ingresar lote",
                    RutaNavegacion = "/inventario/lotes", Icono = "bi-plus-square", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Inventario", Subcategoria = "Lotes", Titulo = "Alertas de Vencimiento",
                    Contenido = @"Para ver **productos próximos a vencer**:

1️⃣ Ve a **Inventario → Alertas de Vencimiento**
2️⃣ Verás un resumen con:
   - 🔴 **Vencidos**: productos ya expirados
   - 🟠 **Próximos 30 días**: vencen pronto
   - 🟡 **Próximos 60 días**: atención
   - 🟢 **Próximos 90 días**: monitorear

## Acciones recomendadas:
- **Vencidos**: dar de baja con ajuste de inventario
- **Próximos a vencer**: promocionar para rotación
- **Con poco stock**: verificar si conviene reponer

💡 El sistema usa colores para facilitar la identificación visual.",
                    PalabrasClave = "vencimiento, alerta vencimiento, producto vencido, caducidad, expirar, vencer",
                    RutaNavegacion = "/inventario/alertas-vencimiento", Icono = "bi-exclamation-triangle", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Inventario", Subcategoria = "Lotes", Titulo = "Relación entre Lotes y Depósitos",
                    Contenido = @"## ¿Los lotes afectan los depósitos?

**NO**, el sistema de lotes es **independiente y opcional**:

| Aspecto | Comportamiento |
|---------|----------------|
| Stock normal | Sigue en `Producto.Stock` sin cambios |
| Stock por lote | Cada lote tiene su propio stock |
| Depósitos | Funcionan igual que antes |

## ¿Cómo se relacionan?
- Cada **lote** pertenece a **UN depósito**
- El stock del producto es la **suma** de todos sus lotes
- Las **transferencias** entre depósitos mueven lotes completos

## Ejemplo:
```
Depósito ""Principal""
├── Producto A (sin control lote) → Stock: 100
└── Producto B (con control lote)
    ├── Lote L001 (vence 15/02) → Stock: 30
    ├── Lote L002 (vence 20/03) → Stock: 50
    └── Stock total: 80
```

💡 Si no activas ""Controla Lote"" en ningún producto, todo funciona exactamente igual que antes.",
                    PalabrasClave = "lote deposito, relacion lote, stock lote, deposito stock, lote almacen",
                    RutaNavegacion = "/inventario/lotes", Icono = "bi-diagram-3", Prioridad = 7,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Inventario", Subcategoria = "Lotes", Titulo = "Activar control de lotes en un producto",
                    Contenido = @"Para **activar el control de lotes** en un producto:

1️⃣ Ve a **Productos → Administrar Productos**
2️⃣ Busca y **edita** el producto
3️⃣ Activa la opción **""Controla Lote""** ✅
4️⃣ **Guarda** el producto

## 🎉 Lotes Automáticos (NUEVO)
Al activar ""Controla Lote"" por primera vez en un producto **con stock existente**, el sistema crea automáticamente un lote llamado **""STOCK-INICIAL""** con:
- Todo el stock actual del producto
- Fecha de vencimiento: 1 año desde hoy
- Depósito: el predeterminado del producto

¡No necesitas crear lotes manualmente para productos existentes!

## ¿Cuándo activarlo?
✅ **Activar** para:
- Medicamentos y productos farmacéuticos
- Alimentos perecederos
- Productos con fecha de vencimiento
- Cualquier producto que requiera trazabilidad

❌ **No necesario** para:
- Productos sin vencimiento
- Artículos de ferretería
- Productos de consumo duradero

⚠️ **Importante**: Una vez que un producto tiene movimientos con lote, no se recomienda desactivar el control.",
                    PalabrasClave = "activar lote, habilitar lote, controla lote, producto lote, configurar lote, lote automatico, stock inicial",
                    RutaNavegacion = "/productos", Icono = "bi-toggle-on", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Inventario", Subcategoria = "Lotes", Titulo = "Lotes automáticos STOCK-INICIAL",
                    Contenido = @"## ¿Qué es el lote STOCK-INICIAL?

Cuando activas **""Controla Lote""** en un producto que ya tiene stock, el sistema crea automáticamente un lote llamado **STOCK-INICIAL**.

## ¿Cómo funciona?
1️⃣ Activas ""Controla Lote"" en el producto
2️⃣ Al **guardar**, el sistema detecta si hay stock existente
3️⃣ Crea el lote **STOCK-INICIAL** con:
   - 📦 Todo el stock actual
   - 📅 Vencimiento: 1 año desde hoy
   - 🏢 Depósito predeterminado del producto
4️⃣ Verás el mensaje: ""Se crearon X lote(s) automáticamente""

## ¿Por qué es útil?
- ✅ No pierdes el stock existente
- ✅ Puedes empezar a usar lotes inmediatamente
- ✅ Las próximas compras ya ingresan con su propio lote
- ✅ El sistema FEFO funciona correctamente

## Después del lote inicial:
- Edita el lote para ajustar la fecha de vencimiento real
- Las nuevas compras crean lotes separados
- El sistema FEFO prioriza el que vence antes

💡 **Tip**: Si el producto tiene stock en varios depósitos, se crea un lote STOCK-INICIAL para cada depósito.",
                    PalabrasClave = "stock inicial, lote inicial, lote automatico, crear lote automatico, primer lote, migrar lote",
                    RutaNavegacion = "/inventario/lotes", Icono = "bi-magic", Prioridad = 9,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },

                // ========== VENTAS - PAQUETES Y UNIDADES ==========
                new()
                {
                    Categoria = "Ventas", Subcategoria = "Paquetes", Titulo = "Vender por paquete o por unidad",
                    Contenido = @"El sistema permite vender productos **por paquete o por unidad**:

## ¿Qué es?
Un producto puede tener un **paquete** (caja, blister, pack) que contiene varias unidades. Por ejemplo:
- Caja de 12 unidades
- Blister de 10 pastillas
- Pack de 6 botellas

## En la venta:
1️⃣ Al agregar un producto, elige el **modo de venta**:
   - **Por Unidad**: precio individual
   - **Por Paquete**: precio del paquete completo
2️⃣ El sistema calcula automáticamente:
   - Stock afectado (en unidades)
   - Precio correcto según modo

## Ejemplo:
- Producto: Paracetamol 500mg
- Caja de 10 unidades a Gs 50.000
- Unidad a Gs 5.500

💡 El stock siempre se maneja en **unidades**, pero puedes vender en paquetes.",
                    PalabrasClave = "paquete, unidad, caja, blister, pack, vender caja, venta paquete, precio caja",
                    RutaNavegacion = "/ventas", Icono = "bi-box2", Prioridad = 9,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Productos", Subcategoria = "Paquetes", Titulo = "Configurar producto con paquete",
                    Contenido = @"Para configurar un producto que se vende **por paquete y unidad**:

1️⃣ Ve a **Productos → Administrar Productos**
2️⃣ **Edita** el producto
3️⃣ Configura:
   - **Cantidad por Paquete**: cuántas unidades tiene el paquete (ej: 12)
   - **Precio de Venta**: precio POR UNIDAD
   - **Precio Paquete** (opcional): precio especial del paquete completo
4️⃣ **Guarda** el producto

## Cálculo de precios:
- Si defines **Precio Paquete**: se usa ese precio al vender por paquete
- Si no lo defines: se calcula como Precio Unidad × Cantidad

## Ejemplo:
```
Cantidad por Paquete: 12
Precio Unidad: Gs 5.000
Precio Paquete: Gs 55.000 (descuento por caja)
```

💡 El stock siempre se lleva en unidades, el sistema convierte automáticamente.",
                    PalabrasClave = "configurar paquete, cantidad paquete, precio paquete, unidades por caja, producto caja",
                    RutaNavegacion = "/productos", Icono = "bi-box2-fill", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Compras", Subcategoria = "Paquetes", Titulo = "Comprar por paquete o unidad",
                    Contenido = @"Al registrar una **compra**, puedes ingresar por paquete o unidad:

## Modos de ingreso:
1️⃣ **Por Unidad**: ingresas la cantidad exacta de unidades
2️⃣ **Por Paquete**: ingresas cantidad de cajas/paquetes

## Ejemplo de compra por paquete:
- Producto: Ibuprofeno 400mg (caja de 20)
- Compras: 5 cajas
- El sistema registra: 100 unidades en stock

## Beneficios:
- ✅ Precio de costo correcto por unidad
- ✅ Control de margen por caja
- ✅ Reportes muestran ambas métricas
- ✅ El informe de compras detalla: cajas y unidades

💡 El modo se guarda con la compra para referencia futura.",
                    PalabrasClave = "compra paquete, comprar caja, ingreso paquete, costo paquete, precio caja proveedor",
                    RutaNavegacion = "/compras", Icono = "bi-bag-plus", Prioridad = 8,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                },
                new()
                {
                    Categoria = "Ventas", Subcategoria = "Paquetes", Titulo = "Ver ventas por paquete en reportes",
                    Contenido = @"Los **reportes** muestran información de paquetes vs unidades:

## En el Ticket de Venta:
- Muestra: ""2p/24u"" = 2 paquetes (24 unidades)
- El precio mostrado es por paquete si se vendió así

## En KuDE (Factura A4):
- Columna **U/M**: muestra ""PAQ"" o la unidad de medida
- Columna **Cajas**: cantidad de paquetes vendidos
- Descripción incluye ""(x12)"" indicando unidades por paquete

## En Informe de Ventas Detallado:
- Indica con badge 📦 si fue venta por paquete
- Muestra cantidad de paquetes y unidades totales
- Precio unitario calculado por unidad

## En Informe de Compras:
- Badge 📦 indica compra por paquete
- Columnas separadas para paquetes y unidades
- Precio por paquete y precio calculado por unidad

💡 Esta información se guarda con cada transacción para histórico.",
                    PalabrasClave = "reporte paquete, informe caja, ver paquetes, ticket paquete, factura caja",
                    RutaNavegacion = "/informes/ventas-detallado", Icono = "bi-file-earmark-bar-graph", Prioridad = 7,
                    FechaCreacion = ahora, FechaActualizacion = ahora, Activo = true
                }
            };
        }

        /// <summary>
        /// Inicializa la configuración VPN con valores por defecto si no existe.
        /// Los valores se replican a los clientes con cada actualización.
        /// </summary>
        public async Task InicializarConfiguracionVPNAsync()
        {
            try
            {
                await using var context = await _dbFactory.CreateDbContextAsync();
                
                // Verificar si ya existe configuración VPN
                var configExistente = await context.ConfiguracionesVPN.FirstOrDefaultAsync();
                if (configExistente != null)
                {
                    _logger.LogInformation("Configuración VPN ya existe, no se sobrescribe.");
                    return;
                }
                
                // Crear configuración VPN inicial con valores por defecto de la empresa
                var configVPN = new ConfiguracionVPN
                {
                    ServidorVPN = "190.104.149.35",           // IP del servidor Mikrotik
                    PuertoPPTP = 1723,                         // Puerto PPTP estándar
                    UsuarioVPN = "nextsys",                    // Usuario VPN
                    ContrasenaVPN = "P3tr0l30$",               // Contraseña VPN
                    NombreConexionWindows = "SistemIA VPN",    // Nombre que aparece en Windows
                    RangoRedVPN = "192.168.89",                // Primeros 3 octetos del pool VPN
                    IpLocalVPN = null,                         // Se asigna dinámicamente
                    ConectarAlIniciar = true,                  // Conectar automáticamente al iniciar servicio
                    IntentosReconexion = 3,                    // Intentos antes de fallar
                    SegundosEntreIntentos = 10,                // Espera entre intentos
                    MinutosVerificacion = 15,                  // Verificar conexión cada 15 minutos
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    FechaModificacion = DateTime.Now
                };
                
                context.ConfiguracionesVPN.Add(configVPN);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Configuración VPN inicializada con valores por defecto.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar configuración VPN");
            }
        }
    }
}
