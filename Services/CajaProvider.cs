using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SistemIA.Services
{
    public interface ICajaProvider
    {
        int? GetCajaId();
        string? GetCajaNombre();
    }

    public class CajaProvider : ICajaProvider
    {
        private readonly IHttpContextAccessor _http;
        
        public CajaProvider(IHttpContextAccessor http) 
        { 
            _http = http; 
        }

        public int? GetCajaId()
        {
            var user = _http.HttpContext?.User;
            var claim = user?.FindFirst("CajaId");
            if (claim == null) return null;
            return int.TryParse(claim.Value, out var id) ? id : null;
        }

        public string? GetCajaNombre()
        {
            var user = _http.HttpContext?.User;
            return user?.FindFirst("CajaNombre")?.Value;
        }
    }
}
