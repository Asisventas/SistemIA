using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Resultado de búsqueda de RUC unificada
    /// </summary>
    public class RucBusquedaResultado
    {
        public bool Encontrado { get; set; }
        public string? RazonSocial { get; set; }
        public string? RUC { get; set; }
        public int DV { get; set; }
        public string? Estado { get; set; }
        public string Fuente { get; set; } = string.Empty; // "Cliente", "Proveedor", "RucDnit", "Sifen", "NoEncontrado"
        public bool YaRegistrado { get; set; } // true si ya está en Clientes/Proveedores
        public int? IdExistente { get; set; } // IdCliente o IdProveedor si ya existe
        public string Mensaje { get; set; } = string.Empty;
    }

    public interface IRucDnitService
    {
        Task<RucDnit?> BuscarPorRucAsync(string ruc);
        Task<List<RucDnit>> BuscarPorNombreAsync(string nombre, int maxResultados = 20);
        Task<(string? Nombre, int? DV, string? Estado)> ObtenerDatosRucAsync(string ruc);
        
        /// <summary>
        /// Búsqueda unificada de RUC: Cliente/Proveedor → RucDnit → SIFEN
        /// </summary>
        Task<RucBusquedaResultado> BuscarRucUnificadoClienteAsync(string ruc);
        Task<RucBusquedaResultado> BuscarRucUnificadoProveedorAsync(string ruc);
    }

    public class RucDnitService : IRucDnitService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly Sifen _sifenService;

        public RucDnitService(IDbContextFactory<AppDbContext> dbFactory, Sifen sifenService)
        {
            _dbFactory = dbFactory;
            _sifenService = sifenService;
        }

        /// <summary>
        /// Busca un RUC exacto en el catálogo DNIT
        /// </summary>
        public async Task<RucDnit?> BuscarPorRucAsync(string ruc)
        {
            if (string.IsNullOrWhiteSpace(ruc)) return null;
            
            ruc = ruc.Trim().Replace("-", "").Replace(".", "");
            
            await using var db = await _dbFactory.CreateDbContextAsync();
            return await db.RucDnit
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RUC == ruc);
        }

        /// <summary>
        /// Busca RUCs por nombre (razón social) - búsqueda parcial
        /// </summary>
        public async Task<List<RucDnit>> BuscarPorNombreAsync(string nombre, int maxResultados = 20)
        {
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Length < 3) 
                return new List<RucDnit>();
            
            await using var db = await _dbFactory.CreateDbContextAsync();
            return await db.RucDnit
                .AsNoTracking()
                .Where(r => r.RazonSocial.Contains(nombre))
                .OrderBy(r => r.RazonSocial)
                .Take(maxResultados)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene nombre, DV y estado de un RUC - útil para autocompletar
        /// </summary>
        public async Task<(string? Nombre, int? DV, string? Estado)> ObtenerDatosRucAsync(string ruc)
        {
            var resultado = await BuscarPorRucAsync(ruc);
            if (resultado == null)
                return (null, null, null);
            
            return (resultado.RazonSocial, resultado.DV, resultado.Estado);
        }

        /// <summary>
        /// Búsqueda unificada para CLIENTES: Cliente → RucDnit → SIFEN
        /// </summary>
        public async Task<RucBusquedaResultado> BuscarRucUnificadoClienteAsync(string ruc)
        {
            if (string.IsNullOrWhiteSpace(ruc))
                return new RucBusquedaResultado { Encontrado = false, Fuente = "NoEncontrado", Mensaje = "RUC vacío" };

            ruc = new string(ruc.Where(char.IsDigit).ToArray());
            if (ruc.Length < 5)
                return new RucBusquedaResultado { Encontrado = false, Fuente = "NoEncontrado", Mensaje = "RUC muy corto (mínimo 5 dígitos)" };

            await using var db = await _dbFactory.CreateDbContextAsync();

            // 1. Buscar en Clientes
            var cliente = await db.Clientes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.RUC == ruc);
            
            if (cliente != null)
            {
                return new RucBusquedaResultado
                {
                    Encontrado = true,
                    RazonSocial = cliente.RazonSocial,
                    RUC = cliente.RUC,
                    DV = cliente.DV,
                    Estado = "ACTIVO",
                    Fuente = "Cliente",
                    YaRegistrado = true,
                    IdExistente = cliente.IdCliente,
                    Mensaje = $"✓ Cliente existente: {cliente.RazonSocial}"
                };
            }

            // 2. Buscar en RucDnit (catálogo DNIT 1.5M registros)
            var rucDnit = await db.RucDnit.AsNoTracking()
                .FirstOrDefaultAsync(r => r.RUC == ruc);
            
            if (rucDnit != null)
            {
                return new RucBusquedaResultado
                {
                    Encontrado = true,
                    RazonSocial = rucDnit.RazonSocial,
                    RUC = rucDnit.RUC,
                    DV = rucDnit.DV,
                    Estado = rucDnit.Estado ?? "ACTIVO",
                    Fuente = "RucDnit",
                    YaRegistrado = false,
                    Mensaje = $"✓ RUC encontrado (DNIT): {rucDnit.RazonSocial}"
                };
            }

            // 3. Buscar en SIFEN (API externa)
            var resultadoSifen = await BuscarEnSifenAsync(db, ruc);
            if (resultadoSifen.Encontrado)
            {
                resultadoSifen.YaRegistrado = false;
                return resultadoSifen;
            }

            // No encontrado en ninguna fuente
            return new RucBusquedaResultado
            {
                Encontrado = false,
                RUC = ruc,
                DV = Utils.RucHelper.CalcularDvRuc(ruc),
                Fuente = "NoEncontrado",
                YaRegistrado = false,
                Mensaje = "RUC no encontrado. Puede ingresarlo manualmente."
            };
        }

        /// <summary>
        /// Búsqueda unificada para PROVEEDORES: Proveedor → RucDnit → SIFEN
        /// </summary>
        public async Task<RucBusquedaResultado> BuscarRucUnificadoProveedorAsync(string ruc)
        {
            if (string.IsNullOrWhiteSpace(ruc))
                return new RucBusquedaResultado { Encontrado = false, Fuente = "NoEncontrado", Mensaje = "RUC vacío" };

            ruc = new string(ruc.Where(char.IsDigit).ToArray());
            if (ruc.Length < 5)
                return new RucBusquedaResultado { Encontrado = false, Fuente = "NoEncontrado", Mensaje = "RUC muy corto (mínimo 5 dígitos)" };

            await using var db = await _dbFactory.CreateDbContextAsync();

            // 1. Buscar en Proveedores
            var proveedor = await db.ProveedoresSifen.AsNoTracking()
                .FirstOrDefaultAsync(p => p.RUC == ruc || p.RUC == $"{ruc}-{Utils.RucHelper.CalcularDvRuc(ruc)}");
            
            if (proveedor != null)
            {
                return new RucBusquedaResultado
                {
                    Encontrado = true,
                    RazonSocial = proveedor.RazonSocial,
                    RUC = proveedor.RUC?.Split('-')[0] ?? ruc,
                    DV = proveedor.DV,
                    Estado = "ACTIVO",
                    Fuente = "Proveedor",
                    YaRegistrado = true,
                    IdExistente = proveedor.IdProveedor,
                    Mensaje = $"✓ Proveedor existente: {proveedor.RazonSocial}"
                };
            }

            // 2. Buscar en RucDnit
            var rucDnit = await db.RucDnit.AsNoTracking()
                .FirstOrDefaultAsync(r => r.RUC == ruc);
            
            if (rucDnit != null)
            {
                return new RucBusquedaResultado
                {
                    Encontrado = true,
                    RazonSocial = rucDnit.RazonSocial,
                    RUC = rucDnit.RUC,
                    DV = rucDnit.DV,
                    Estado = rucDnit.Estado ?? "ACTIVO",
                    Fuente = "RucDnit",
                    YaRegistrado = false,
                    Mensaje = $"✓ RUC encontrado (DNIT): {rucDnit.RazonSocial}"
                };
            }

            // 3. Buscar en SIFEN
            var resultadoSifen = await BuscarEnSifenAsync(db, ruc);
            if (resultadoSifen.Encontrado)
            {
                resultadoSifen.YaRegistrado = false;
                return resultadoSifen;
            }

            return new RucBusquedaResultado
            {
                Encontrado = false,
                RUC = ruc,
                DV = Utils.RucHelper.CalcularDvRuc(ruc),
                Fuente = "NoEncontrado",
                YaRegistrado = false,
                Mensaje = "RUC no encontrado. Puede ingresarlo manualmente."
            };
        }

        /// <summary>
        /// Consulta SIFEN para obtener datos de un RUC
        /// </summary>
        private async Task<RucBusquedaResultado> BuscarEnSifenAsync(AppDbContext db, string ruc)
        {
            try
            {
                var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
                if (sociedad == null || string.IsNullOrEmpty(sociedad.PathCertificadoP12))
                {
                    return new RucBusquedaResultado 
                    { 
                        Encontrado = false, 
                        Fuente = "Sifen", 
                        Mensaje = "⚠️ Sin certificado SIFEN configurado" 
                    };
                }

                var url = sociedad.DeUrlConsultaRuc ?? Utils.SifenConfig.GetConsultaRucUrl("test");
                var xmlRespuesta = await _sifenService.Consulta(url, ruc, "1", sociedad.PathCertificadoP12, sociedad.PasswordCertificadoP12 ?? "");

                if (string.IsNullOrEmpty(xmlRespuesta))
                {
                    return new RucBusquedaResultado 
                    { 
                        Encontrado = false, 
                        Fuente = "Sifen", 
                        Mensaje = "⚠️ Sin respuesta de SIFEN" 
                    };
                }

                if (xmlRespuesta.Contains("0160") || xmlRespuesta.Contains("XML Mal Formado"))
                {
                    return new RucBusquedaResultado 
                    { 
                        Encontrado = false, 
                        Fuente = "Sifen", 
                        Mensaje = "⚠️ Error de conexión SIFEN" 
                    };
                }

                var xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(xmlRespuesta);

                var codRes = xmlDoc.SelectSingleNode("//*[local-name()='dCodRes']")?.InnerText ?? "";

                if (codRes == "0502")
                {
                    var xContRUC = xmlDoc.SelectSingleNode("//*[local-name()='xContRUC']");
                    if (xContRUC != null)
                    {
                        var dRazCons = xContRUC.SelectSingleNode(".//*[local-name()='dRazCons']")?.InnerText?.Trim();
                        var dDesEstCons = xContRUC.SelectSingleNode(".//*[local-name()='dDesEstCons']")?.InnerText?.Trim();

                        return new RucBusquedaResultado
                        {
                            Encontrado = true,
                            RazonSocial = dRazCons,
                            RUC = ruc,
                            DV = Utils.RucHelper.CalcularDvRuc(ruc),
                            Estado = dDesEstCons ?? "ACTIVO",
                            Fuente = "Sifen",
                            Mensaje = $"✓ SIFEN: {dRazCons} ({dDesEstCons})"
                        };
                    }
                }

                return new RucBusquedaResultado 
                { 
                    Encontrado = false, 
                    RUC = ruc,
                    DV = Utils.RucHelper.CalcularDvRuc(ruc),
                    Fuente = "Sifen", 
                    Mensaje = "RUC no encontrado en SIFEN" 
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR BuscarEnSifenAsync] {ex.Message}");
                return new RucBusquedaResultado 
                { 
                    Encontrado = false, 
                    Fuente = "Sifen", 
                    Mensaje = $"⚠️ Error SIFEN: {ex.Message}" 
                };
            }
        }
    }
}

