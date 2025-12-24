using Microsoft.EntityFrameworkCore;
using SistemIA.Models;

namespace SistemIA.Services
{
    /// <summary>
    /// Servicio para gestión de permisos basado en roles
    /// </summary>
    public class PermisosService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public PermisosService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico en un módulo
        /// </summary>
        public async Task<bool> TienePermisoAsync(int idUsuario, string codigoModulo, string codigoPermiso)
        {
            try
            {
                await using var ctx = await _dbFactory.CreateDbContextAsync();

                var usuario = await ctx.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Id_Usu == idUsuario);

                if (usuario == null || !usuario.Estado_Usu || !usuario.Rol!.Estado)
                    return false;

                // Buscar el permiso
                var tienePermiso = await ctx.RolesModulosPermisos
                    .Include(rmp => rmp.Modulo)
                    .Include(rmp => rmp.Permiso)
                    .AnyAsync(rmp =>
                        rmp.IdRol == usuario.Id_Rol &&
                        rmp.Modulo.RutaPagina == codigoModulo &&
                        rmp.Permiso.Codigo == codigoPermiso &&
                        rmp.Concedido &&
                        rmp.Modulo.Activo &&
                        rmp.Permiso.Activo);

                return tienePermiso;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene todos los módulos con sus permisos para un rol específico
        /// </summary>
        public async Task<List<ModuloConPermisos>> ObtenerModulosConPermisosAsync(int idRol)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            var modulos = await ctx.Modulos
                .Where(m => m.Activo)
                .OrderBy(m => m.Orden)
                .ToListAsync();

            var permisos = await ctx.Permisos
                .Where(p => p.Activo)
                .OrderBy(p => p.Orden)
                .ToListAsync();

            var permisosAsignados = await ctx.RolesModulosPermisos
                .Where(rmp => rmp.IdRol == idRol)
                .ToListAsync();

            var resultado = new List<ModuloConPermisos>();

            foreach (var modulo in modulos)
            {
                var moduloConPermisos = new ModuloConPermisos
                {
                    Modulo = modulo,
                    Permisos = new List<PermisoAsignado>()
                };

                foreach (var permiso in permisos)
                {
                    var asignado = permisosAsignados.FirstOrDefault(pa =>
                        pa.IdModulo == modulo.IdModulo &&
                        pa.IdPermiso == permiso.IdPermiso);

                    moduloConPermisos.Permisos.Add(new PermisoAsignado
                    {
                        Permiso = permiso,
                        Concedido = asignado?.Concedido ?? false,
                        IdRolModuloPermiso = asignado?.IdRolModuloPermiso
                    });
                }

                resultado.Add(moduloConPermisos);
            }

            return resultado;
        }

        /// <summary>
        /// Asigna o revoca un permiso para un rol en un módulo
        /// </summary>
        public async Task<bool> AsignarPermisoAsync(int idRol, int idModulo, int idPermiso, bool conceder, string usuarioAsignacion)
        {
            try
            {
                await using var ctx = await _dbFactory.CreateDbContextAsync();

                var existente = await ctx.RolesModulosPermisos
                    .FirstOrDefaultAsync(rmp =>
                        rmp.IdRol == idRol &&
                        rmp.IdModulo == idModulo &&
                        rmp.IdPermiso == idPermiso);

                if (existente != null)
                {
                    if (conceder)
                    {
                        existente.Concedido = true;
                        existente.FechaAsignacion = DateTime.Now;
                        existente.UsuarioAsignacion = usuarioAsignacion;
                    }
                    else
                    {
                        ctx.RolesModulosPermisos.Remove(existente);
                    }
                }
                else if (conceder)
                {
                    ctx.RolesModulosPermisos.Add(new RolModuloPermiso
                    {
                        IdRol = idRol,
                        IdModulo = idModulo,
                        IdPermiso = idPermiso,
                        Concedido = true,
                        FechaAsignacion = DateTime.Now,
                        UsuarioAsignacion = usuarioAsignacion
                    });
                }

                await ctx.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene los módulos accesibles para un usuario
        /// </summary>
        public async Task<List<Modulo>> ObtenerModulosAccesiblesAsync(int idUsuario)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();

            var usuario = await ctx.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id_Usu == idUsuario);

            if (usuario == null || !usuario.Estado_Usu)
                return new List<Modulo>();

            var modulosConPermisos = await ctx.RolesModulosPermisos
                .Where(rmp => rmp.IdRol == usuario.Id_Rol && rmp.Concedido)
                .Select(rmp => rmp.IdModulo)
                .Distinct()
                .ToListAsync();

            return await ctx.Modulos
                .Where(m => modulosConPermisos.Contains(m.IdModulo) && m.Activo)
                .OrderBy(m => m.Orden)
                .ToListAsync();
        }
    }

    public class ModuloConPermisos
    {
        public Modulo Modulo { get; set; } = null!;
        public List<PermisoAsignado> Permisos { get; set; } = new();
    }

    public class PermisoAsignado
    {
        public Permiso Permiso { get; set; } = null!;
        public bool Concedido { get; set; }
        public int? IdRolModuloPermiso { get; set; }
    }
}
