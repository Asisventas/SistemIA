using SistemIA.Models;
using Microsoft.EntityFrameworkCore;

namespace SistemIA.Services
{
    public interface IDataInitializationService
    {
        Task InicializarDatosListasPreciosAsync();
    Task InicializarGeografiaSifenAsync();
    Task<bool> ImportarCatalogoGeograficoAhoraAsync();
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
    }
}
