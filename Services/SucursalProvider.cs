using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SistemIA.Services
{
    public interface ISucursalProvider
    {
        int? GetSucursalId();
        string? GetSucursalNombre();
    }

    public class SucursalProvider : ISucursalProvider
    {
        private readonly IHttpContextAccessor _http;
        public SucursalProvider(IHttpContextAccessor http) { _http = http; }

        public int? GetSucursalId()
        {
            var user = _http.HttpContext?.User;
            var claim = user?.FindFirst("SucursalId");
            if (claim == null) return null;
            return int.TryParse(claim.Value, out var id) ? id : null;
        }

        public string? GetSucursalNombre()
        {
            var user = _http.HttpContext?.User;
            return user?.FindFirst("SucursalNombre")?.Value;
        }
    }
}
