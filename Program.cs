using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SistemIA.Data;
using SistemIA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using SistemIA.Services;
using System.Text;
using System.Text.RegularExpressions;
using SistemIA.Utils;
using Microsoft.Extensions.Hosting.WindowsServices;
using QuestPDF.Infrastructure;

// Configurar licencia QuestPDF (Community para proyectos pequeños)
QuestPDF.Settings.License = LicenseType.Community;

// Configuración global SSL/TLS para SIFEN (desarrollo)
// ADVERTENCIA: En producción, validar certificados apropiadamente
// SIFEN Paraguay requiere explícitamente TLS 1.2
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
{
    // Para desarrollo y pruebas con SIFEN, aceptar todos los certificados
    Console.WriteLine($"[ServicePointManager] Validando certificado: {certificate?.Subject}");
    Console.WriteLine($"[ServicePointManager] Errores SSL: {sslPolicyErrors}");
    return true; // Aceptar todos los certificados en desarrollo
};

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() 
        ? AppContext.BaseDirectory 
        : default
});

// Habilitar ejecución como servicio de Windows
builder.Host.UseWindowsService();

// --- INICIO DE LA SECCIÓN DE REGISTRO DE SERVICIOS ---
// Todos los servicios deben registrarse aquí, ANTES de builder.Build()

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<IFaceRecognitionService, FaceRecognitionService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://api.dnit.gov.py/") });
builder.Services.AddScoped<Sifen>();
builder.Services.AddScoped<AsistenciaCalculatorService>();
builder.Services.AddScoped<BackupService>();
builder.Services.AddScoped<BackupCodigoFuenteService>();
builder.Services.AddScoped<InformeAsistenciaService>();
builder.Services.AddScoped<ClienteSifenService>();
builder.Services.AddHttpClient<ITipoCambioService, TipoCambioService>();
builder.Services.AddScoped<ITipoCambioHistoricoService, TipoCambioHistoricoService>();
builder.Services.AddScoped<IDataInitializationService, DataInitializationService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IAjusteStockService, AjusteStockService>();
builder.Services.AddScoped<IRucDnitService, RucDnitService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISucursalProvider, SucursalProvider>();
builder.Services.AddScoped<ICajaProvider, CajaProvider>();
builder.Services.AddScoped<DEBuilderService>();
builder.Services.AddScoped<DEXmlBuilder>();
builder.Services.AddScoped<PdfFacturaService>();
builder.Services.AddScoped<PdfPresupuestoSistemaService>();
builder.Services.AddScoped<ICajaService, CajaService>();
builder.Services.AddScoped<PagoProveedorService>();
builder.Services.AddScoped<AuditoriaService>();
builder.Services.AddScoped<PermisosService>();
builder.Services.AddScoped<ActualizacionService>();
builder.Services.AddScoped<DescuentoService>();
builder.Services.AddScoped<ICorreoService, CorreoService>();
builder.Services.AddScoped<IInformeCorreoService, InformeCorreoService>();
builder.Services.AddScoped<IAsistenteIAService, AsistenteIAService>();
builder.Services.AddScoped<IHtmlToPdfService, HtmlToPdfService>();
builder.Services.AddScoped<IInformePdfService, InformePdfService>();
builder.Services.AddScoped<IHistorialCambiosService, HistorialCambiosService>();
builder.Services.AddScoped<ISaAuthenticationService, SaAuthenticationService>(); // Autenticación SA con memoria de 30 min
builder.Services.AddScoped<ILoteService, LoteService>(); // Gestión de lotes FEFO para farmacia
builder.Services.AddScoped<IEventoSifenService, EventoSifenService>(); // Eventos SIFEN (cancelación, inutilización)
builder.Services.AddSingleton<ChatStateService>(); // Estado del chat (singleton por circuito)
builder.Services.AddSingleton<RutasSistemaService>(); // Escaneo automático de rutas del sistema
builder.Services.AddSingleton<ITrackingService, TrackingService>(); // Tracking de acciones del usuario
// Impresión directa sin diálogo (solo Windows)
if (OperatingSystem.IsWindows())
{
    builder.Services.AddScoped<ImpresionDirectaService>();
}
builder.Services.AddControllers();
// Útiles
// Nota: XmlValidationUtil se usa en endpoints de depuración

// Autenticación y Autorización (Cookies)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/auth/logout";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();


builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 2 * 1024 * 1024;
});

// Usar la configuración por defecto (launchSettings.json) para que arranque en https://localhost:7058
// Si necesitás volver a publicar en todas las IPs o forzar un PFX, reintroducí ConfigureKestrel aquí.

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// --- FIN DE LA SECCIÓN DE REGISTRO DE SERVICIOS ---


// Aquí se "construye" la aplicación. A partir de aquí, no se pueden añadir más servicios.
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// NOTA: En desarrollo, NO usar HttpsRedirection para permitir pruebas HTTP desde terminal
// En producción se recomienda habilitar
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

// Inicializar permisos y módulos (seed data)
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await SeedPermisos.InicializarPermisosAsync(dbFactory);
    await SeedPermisos.ActualizarModulosAsync(dbFactory);
}

// Pipeline de auth
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapControllers();
app.MapFallbackToPage("/_Host");

// Endpoints de autenticación
app.MapPost("/auth/login", async (HttpContext http, IDbContextFactory<AppDbContext> dbFactory) =>
{
    var form = await http.Request.ReadFormAsync();
    var userName = form["UserName"].ToString();
    var password = form["Password"].ToString();
    var returnUrl = http.Request.Query["returnUrl"].ToString();

    await using var db = await dbFactory.CreateDbContextAsync();
    var usuario = await db.Usuarios
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.UsuarioNombre == userName && u.Estado_Usu);

    bool ValidarPassword(byte[] hashBD, string plain)
    {
        if (hashBD == null || hashBD.Length == 0) return false;
        using var sha = SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(plain);
        var shaBytes = sha.ComputeHash(bytes); // 32 bytes
        return shaBytes.SequenceEqual(hashBD);
    }

    if (usuario is null || !ValidarPassword(usuario.ContrasenaHash, password))
    {
        // Redirigir nuevamente al login con error
        return Results.Redirect("/login?error=credenciales");
    }

    // Verificar cantidad de sucursales y decidir si asignar automáticamente o solicitar selección
    var sucursales = await db.Sucursal.AsNoTracking().OrderBy(s => s.Id).Select(s => new { s.Id, s.NombreSucursal }).ToListAsync();
    if (sucursales.Count == 0)
    {
        return Results.Redirect("/login?error=sin_sucursal");
    }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, usuario.Id_Usu.ToString()),
        new Claim(ClaimTypes.Name, usuario.UsuarioNombre),
        new Claim(ClaimTypes.GivenName, usuario.Nombres ?? string.Empty),
        new Claim(ClaimTypes.Surname, usuario.Apellidos ?? string.Empty),
        new Claim(ClaimTypes.Role, usuario.Id_Rol.ToString())
    };

    // Verificar si hay cajas disponibles para seleccionar
    var hayCajas = await db.Cajas.AnyAsync();
    
    // Si hay una sola sucursal Y NO hay cajas, setearla directo en los claims
    if (sucursales.Count == 1 && !hayCajas)
    {
        claims.Add(new Claim("SucursalId", sucursales[0].Id.ToString()));
        claims.Add(new Claim("SucursalNombre", sucursales[0].NombreSucursal ?? ""));
        var identityAuto = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principalAuto = new ClaimsPrincipal(identityAuto);
        await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principalAuto);
    }
    else
    {
        // Autenticar sin sucursal y redirigir a la selección (para elegir sucursal y/o caja)
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        var ret = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
        return Results.Redirect($"/seleccionar-sucursal?returnUrl={WebUtility.UrlEncode(ret)}");
    }

    static bool IsLocalUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        if (url.StartsWith('/')) return !url.StartsWith("//") && !url.StartsWith("/\\");
        return false;
    }

    var destino = IsLocalUrl(returnUrl) ? returnUrl : "/";
    return Results.Redirect(destino);
});

app.MapMethods("/auth/logout", new[] { "GET", "POST" }, async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

// Endpoint para fijar la sucursal y caja en los claims (post login)
app.MapPost("/auth/set-sucursal", async (HttpContext http, IDbContextFactory<AppDbContext> dbFactory) =>
{
    if (http.User?.Identity?.IsAuthenticated != true)
        return Results.Unauthorized();

    var form = await http.Request.ReadFormAsync();
    var sucStr = form["SucursalId"].ToString();
    var cajaStr = form["CajaId"].ToString();
    
    if (!int.TryParse(sucStr, out var sucursalId))
        return Results.BadRequest("Sucursal inválida");

    await using var db = await dbFactory.CreateDbContextAsync();
    var suc = await db.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sucursalId);
    if (suc == null) return Results.NotFound("Sucursal no encontrada");

    // Obtener información de la caja si se proporcionó
    string cajaNombre = "";
    int cajaId = 0;
    if (int.TryParse(cajaStr, out cajaId) && cajaId > 0)
    {
        var caja = await db.Cajas.AsNoTracking().FirstOrDefaultAsync(c => c.IdCaja == cajaId);
        if (caja != null)
        {
            cajaNombre = caja.Nombre ?? $"Caja {caja.IdCaja}";
        }
    }

    // Reconstruir claims conservando los existentes excepto los de sucursal y caja
    var claims = http.User.Claims
        .Where(c => c.Type != "SucursalId" && c.Type != "SucursalNombre" && c.Type != "CajaId" && c.Type != "CajaNombre")
        .ToList();
    claims.Add(new Claim("SucursalId", suc.Id.ToString()));
    claims.Add(new Claim("SucursalNombre", suc.NombreSucursal ?? string.Empty));
    
    if (cajaId > 0)
    {
        claims.Add(new Claim("CajaId", cajaId.ToString()));
        claims.Add(new Claim("CajaNombre", cajaNombre));
    }

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    var returnUrl = form["returnUrl"].ToString();
    if (string.IsNullOrWhiteSpace(returnUrl)) returnUrl = "/";
    return Results.Redirect(returnUrl);
});

// Endpoint para importar catálogo geográfico SIFEN desde CSV bajo demanda
app.MapPost("/admin/importar-catalogo-geografico", async (IDataInitializationService dataInitService) =>
{
    var ok = await dataInitService.ImportarCatalogoGeograficoAhoraAsync();
    return ok ? Results.Ok(new { ok = true }) : Results.BadRequest(new { ok = false, error = "No se encontró CSV o falló la importación" });
});

// Endpoint de prueba para construir sección E7 (pagos) del DE
app.MapGet("/admin/de/pagos/{idVenta:int}", async (int idVenta, DEBuilderService svc) =>
{
    try
    {
        var res = await svc.ConstruirPagosAsync(idVenta);
        return Results.Ok(new { ok = true, e7 = res.E7, venta = res.Venta.IdVenta });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint JSON para verificar catálogo SIFEN (CSV vs BD) y sugerir SQL de corrección
app.MapGet("/admin/verificar-catalogo-json", async (IDbContextFactory<AppDbContext> dbFactory, IWebHostEnvironment env) =>
{
    var result = new VerificacionResultado();
    try
    {
        var csvPath = Path.Combine(env.ContentRootPath, "ManualSifen", "catalogo_geografico.csv");
        if (!File.Exists(csvPath))
        {
            return Results.BadRequest(new { error = $"CSV no encontrado en {csvPath}" });
        }

        // Leer CSV básico: cDep,dDesDep,dDesDis,dDesCiu
        var lines = await File.ReadAllLinesAsync(csvPath, Encoding.UTF8);
    var csv = new List<CsvRow>();
        bool header = true;
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            if (header) { header = false; continue; }
            var parts = line.Split(',');
            if (parts.Length < 4) continue;
            if (!int.TryParse(parts[0].Trim(), out var cDep)) continue;
            csv.Add(new CsvRow(cDep, parts[1].Trim(), parts[2].Trim(), parts[3].Trim()));
        }

        // Agrupar CSV
        var depCsvByName = csv
            .GroupBy(r => CatalogoVerifUtil.Normalize(r.dDesDep))
            .ToDictionary(g => g.Key, g => g.First().cDep);

        var disCsvByDepName = csv
            .GroupBy(r => CatalogoVerifUtil.Normalize(r.dDesDep))
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => CatalogoVerifUtil.Normalize(r.dDesDis)).Distinct().OrderBy(x => x).ToList()
            );

        var ciudadesCsvByDepName = csv
            .GroupBy(r => CatalogoVerifUtil.Normalize(r.dDesDep))
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => (Distrito: CatalogoVerifUtil.Normalize(r.dDesDis), Ciudad: CatalogoVerifUtil.Normalize(r.dDesCiu))).Distinct().ToList()
            );

        result.TotalDepartamentosCsv = depCsvByName.Count;
        result.TotalDistritosCsv = disCsvByDepName.Values.Sum(v => v.Count);
        result.TotalCiudadesCsv = ciudadesCsvByDepName.Values.Sum(v => v.Count);

        await using var db = await dbFactory.CreateDbContextAsync();
        var depsBd = await db.DepartamentosCatalogo.AsNoTracking().ToListAsync();
        var disBd = await db.DistritosCatalogo.AsNoTracking().ToListAsync();
        var ciuBd = await db.CiudadesCatalogo.AsNoTracking().ToListAsync();

        result.TotalDepartamentosBd = depsBd.Count;
        result.TotalDistritosBd = disBd.Count;
        result.TotalCiudadesBd = ciuBd.Count;

    var depBdByName = depsBd.ToDictionary(d => CatalogoVerifUtil.Normalize(d.Nombre), d => d.Numero);

        // Departamentos faltantes/sobrantes y con código distinto
        foreach (var (depNameNorm, codCsv) in depCsvByName)
        {
            if (!depBdByName.TryGetValue(depNameNorm, out var codBd))
            {
                result.DepartamentosFaltantesEnBd.Add(depNameNorm);
            }
            else if (codCsv != codBd)
            {
                result.DepartamentosCodigoDifiere.Add(new DepartamentoCodigoDiff(depNameNorm, codCsv, codBd));
            }
        }
        foreach (var kv in depBdByName)
        {
            if (!disCsvByDepName.ContainsKey(kv.Key))
                result.DepartamentosSobrantesEnBd.Add(kv.Key);
        }

        // Detectar distritos mal asociados (por nombre coincide, pero en otro departamento)
        foreach (var (depNameNorm, distritosCsv) in disCsvByDepName)
        {
            if (!depBdByName.TryGetValue(depNameNorm, out var depNumCsv))
                continue;

            foreach (var disNameNorm in distritosCsv)
            {
                var candidatos = disBd.Where(d => CatalogoVerifUtil.Normalize(d.Nombre) == disNameNorm).ToList();
                if (candidatos.Count == 0) continue; // faltante, se reportará aparte
                foreach (var d in candidatos)
                {
                    if (d.Departamento != depNumCsv)
                    {
                        result.DistritosMalAsociados.Add(new DistritoFix
                        {
                            Numero = d.Numero,
                            Nombre = d.Nombre,
                            DepartamentoActual = d.Departamento,
                            DepartamentoCorrecto = depNumCsv,
                            Sql = $"UPDATE distrito SET Departamento = {depNumCsv} WHERE Numero = {d.Numero};"
                        });
                    }
                }
            }
        }

        // Faltantes/sobrantes de distritos por departamento
        foreach (var (depNameNorm, distritosCsv) in disCsvByDepName)
        {
            if (!depBdByName.TryGetValue(depNameNorm, out var depNumBd)) continue;
            var distritosBdNames = disBd.Where(d => d.Departamento == depNumBd)
                .Select(d => CatalogoVerifUtil.Normalize(d.Nombre)).Distinct().ToList();
            var falt = distritosCsv.Except(distritosBdNames).OrderBy(x => x).ToList();
            if (falt.Any()) result.DistritosFaltantes[depNameNorm] = falt;
            var sobr = distritosBdNames.Except(distritosCsv).OrderBy(x => x).ToList();
            if (sobr.Any()) result.DistritosSobrantes[depNameNorm] = sobr;
        }

        // Detectar ciudades mal asociadas (conservador):
        // Solo proponer UPDATE cuando:
        //  - El nombre coincide con CSV
        //  - El departamento actual ya es el correcto o la ciudad aparece en BD en múltiples deps y uno de ellos es el correcto
        //  - Si no hay distrito exacto por nombre en ese dep, no forzar cambio de distrito
        foreach (var (depNameNorm, ciudadesCsv) in ciudadesCsvByDepName)
        {
            if (!depBdByName.TryGetValue(depNameNorm, out var depNumCsv)) continue;

            foreach (var (disCsvNorm, ciuCsvNorm) in ciudadesCsv)
            {
                // distrito correcto (por nombre y dep según CSV)
                var distCorrecto = disBd.FirstOrDefault(d => CatalogoVerifUtil.Normalize(d.Nombre) == disCsvNorm && d.Departamento == depNumCsv);

                var ciuCandidatas = ciuBd.Where(c => CatalogoVerifUtil.Normalize(c.Nombre) == ciuCsvNorm).ToList();
                if (ciuCandidatas.Count == 0) continue; // faltante, se reportará aparte más abajo

                foreach (var c in ciuCandidatas)
                {
                    var debeDep = depNumCsv;
                    int? debeDist = distCorrecto?.Numero;
                    // criterio conservador: si el departamento ya es el correcto, podemos ajustar distrito; si no, solo ajustar cuando exista en múltiples deps y uno sea el correcto
                    var apareceEnDepCorrecto = ciuCandidatas.Any(x => x.Departamento == debeDep);
                    if (c.Departamento == debeDep)
                    {
                        if (debeDist.HasValue && c.Distrito != debeDist.Value)
                        {
                            var nuevoDist = debeDist.Value;
                            result.CiudadesMalAsociadas.Add(new CiudadFix
                            {
                                Numero = c.Numero,
                                Nombre = c.Nombre,
                                DepartamentoActual = c.Departamento,
                                DistritoActual = c.Distrito,
                                DepartamentoCorrecto = debeDep,
                                DistritoCorrecto = nuevoDist,
                                Sql = $"UPDATE ciudad SET Departamento = {debeDep}, Distrito = {nuevoDist} WHERE Numero = {c.Numero};"
                            });
                        }
                    }
                    else if (apareceEnDepCorrecto && debeDist.HasValue)
                    {
                        // Hay registros homónimos y al menos uno en el dep correcto; sugerimos mover los que no están
                        var nuevoDist = debeDist.Value;
                        result.CiudadesMalAsociadas.Add(new CiudadFix
                        {
                            Numero = c.Numero,
                            Nombre = c.Nombre,
                            DepartamentoActual = c.Departamento,
                            DistritoActual = c.Distrito,
                            DepartamentoCorrecto = debeDep,
                            DistritoCorrecto = nuevoDist,
                            Sql = $"UPDATE ciudad SET Departamento = {debeDep}, Distrito = {nuevoDist} WHERE Numero = {c.Numero};"
                        });
                    }
                }
            }
        }

        // Ciudades faltantes (por nombre) por departamento
        foreach (var (depNameNorm, ciudadesCsv) in ciudadesCsvByDepName)
        {
            if (!depBdByName.TryGetValue(depNameNorm, out var depNumBd)) continue;
            var setCiudadesBd = new HashSet<(string DisNorm, string CiuNorm)>(
                ciuBd.Where(c => c.Departamento == depNumBd)
                    .Select(c => (DisNorm: CatalogoVerifUtil.Normalize(disBd.FirstOrDefault(d => d.Numero == c.Distrito)?.Nombre ?? string.Empty),
                                   CiuNorm: CatalogoVerifUtil.Normalize(c.Nombre)))
            );
            var faltantes = new List<ParDistritoCiudad>();
            foreach (var par in ciudadesCsv)
            {
                if (!setCiudadesBd.Contains(par))
                    faltantes.Add(new ParDistritoCiudad { Distrito = par.Distrito, Ciudad = par.Ciudad });
            }
            if (faltantes.Any()) result.CiudadesFaltantes[depNameNorm] = faltantes;
        }

        // Copilar SQLs
        result.SqlUpdates.AddRange(result.DistritosMalAsociados.Select(d => d.Sql));
        result.SqlUpdates.AddRange(result.CiudadesMalAsociadas.Select(c => c.Sql));

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Endpoint para generar QR como imagen PNG (funciona offline)
app.MapGet("/api/qr", (string url) =>
{
    try
    {
        using var qrGenerator = new QRCoder.QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(url, QRCoder.QRCodeGenerator.ECCLevel.M);
        using var qrCode = new QRCoder.PngByteQRCode(qrCodeData);
        var qrBytes = qrCode.GetGraphic(5); // 5 pixels per module
        return Results.File(qrBytes, "image/png");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

// Endpoint para validar datos mínimos SIFEN de una venta (readiness del DE)
app.MapGet("/admin/de/validar/{idVenta:int}", async (int idVenta, DEBuilderService svc) =>
{
    try
    {
        var res = await svc.ValidarVentaAsync(idVenta);
        return Results.Ok(res);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint de diagnóstico: consulta RUC en SIFEN usando datos de Sociedad
app.MapGet("/debug/consulta-ruc/{ruc}", async (
    string ruc,
    IDbContextFactory<AppDbContext> dbFactory
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) 
            return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad configurado." });

        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        // URL de consulta RUC: asegurar que termine en .wsdl (requerido por SIFEN)
        var urlConsulta = (sociedad.DeUrlConsultaRuc ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(urlConsulta) && !urlConsulta.EndsWith(".wsdl", StringComparison.OrdinalIgnoreCase))
        {
            urlConsulta = urlConsulta + ".wsdl";
        }

        // Info de configuración (password enmascarada)
        var configInfo = new
        {
            rucSociedad = sociedad.RUC ?? "(no configurado)",
            nombreSociedad = sociedad.Nombre ?? "(no configurado)",
            pathCertificado = string.IsNullOrWhiteSpace(p12Path) ? "(no configurado)" : p12Path,
            passwordConfigured = !string.IsNullOrWhiteSpace(p12Pass),
            urlConsultaRuc = string.IsNullOrWhiteSpace(urlConsulta) ? "(no configurado)" : urlConsulta,
            certificadoExiste = !string.IsNullOrWhiteSpace(p12Path) && System.IO.File.Exists(p12Path)
        };

        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.Ok(new { ok = false, error = "Falta PathCertificadoP12 o PasswordCertificadoP12 en Sociedad", config = configInfo });

        if (!System.IO.File.Exists(p12Path))
            return Results.Ok(new { ok = false, error = $"No existe el archivo certificado: {p12Path}", config = configInfo });

        if (string.IsNullOrWhiteSpace(urlConsulta))
            return Results.Ok(new { ok = false, error = "Falta DeUrlConsultaRuc en Sociedad", config = configInfo });

        // Limpiar RUC a solo dígitos
        var rucLimpio = new string(ruc.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(rucLimpio))
            return Results.Ok(new { ok = false, error = "RUC vacío o inválido", config = configInfo });

        // Ejecutar consulta SIFEN
        var sifen = new SistemIA.Models.Sifen();
        var respuestaXml = await sifen.Consulta(urlConsulta, rucLimpio, "1", p12Path, p12Pass);

        // Parsear respuesta - Campos reales de SIFEN: dRUCCons, dRazCons, dCodEstCons, dDesEstCons, dRUCFactElec
        string? dRazCons = null, dDesEstCons = null, dCodEstCons = null, dRUCFactElec = null, dRUCCons = null;
        string? dCodRes = null, dMsgRes = null;
        try
        {
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(respuestaXml);
            
            dCodRes = xmlDoc.SelectSingleNode("//*[local-name()='dCodRes']")?.InnerText;
            dMsgRes = xmlDoc.SelectSingleNode("//*[local-name()='dMsgRes']")?.InnerText;
            
            // Buscar datos del contribuyente dentro de xContRUC
            var xContRUC = xmlDoc.SelectSingleNode("//*[local-name()='xContRUC']");
            if (xContRUC != null)
            {
                dRUCCons = xContRUC.SelectSingleNode(".//*[local-name()='dRUCCons']")?.InnerText;
                dRazCons = xContRUC.SelectSingleNode(".//*[local-name()='dRazCons']")?.InnerText;
                dCodEstCons = xContRUC.SelectSingleNode(".//*[local-name()='dCodEstCons']")?.InnerText;
                dDesEstCons = xContRUC.SelectSingleNode(".//*[local-name()='dDesEstCons']")?.InnerText;
                dRUCFactElec = xContRUC.SelectSingleNode(".//*[local-name()='dRUCFactElec']")?.InnerText;
            }
        }
        catch { /* continuar con valores null */ }

        return Results.Ok(new 
        { 
            ok = true, 
            rucConsultado = rucLimpio,
            config = configInfo,
            respuesta = new
            {
                dCodRes,
                dMsgRes,
                dRUCCons,
                dRazCons,
                dCodEstCons,
                dDesEstCons,
                dRUCFactElec
            },
            xmlCompleto = respuestaXml
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message, stackTrace = ex.StackTrace });
    }
});

// Endpoint de diagnóstico: prueba credenciales SIFEN usando los datos de la venta
app.MapGet("/debug/ventas/{idVenta:int}/probar-credenciales", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = $"Venta {idVenta} no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad; configure certificados y CSC." });

        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower() == "prod" ? "2" : "1"; // 1=test, 2=prod según util
        // Siempre usar certificado de Sociedad para Factura Electrónica
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });

        // Validación rápida en disco
        if (!System.IO.File.Exists(p12Path))
            return Results.BadRequest(new { ok = false, error = $"No existe el archivo .p12 en la ruta: {p12Path}" });

    var basic = await SistemIA.Utils.SifenTester.TestBasicConnectivity(ambiente);
    // Usar el RUC real de la empresa (priorizar Sucursal, fallback a Sociedad)
    string rucEmpresa = string.Empty;
    if (!string.IsNullOrWhiteSpace(venta.Sucursal?.RUC)) rucEmpresa = venta.Sucursal!.RUC;
    if (string.IsNullOrWhiteSpace(rucEmpresa) && !string.IsNullOrWhiteSpace(sociedad.RUC)) rucEmpresa = sociedad.RUC;
    // Limpiar a solo dígitos (SIFEN espera tRuc numérico sin guión)
    rucEmpresa = new string((rucEmpresa ?? string.Empty).Where(char.IsDigit).ToArray());
    var full = await SistemIA.Utils.SifenTester.TestConnection(ambiente, p12Path, p12Pass, rucEmpresa);

        return Results.Ok(new
        {
            ok = full.Success,
            ambiente = SistemIA.Utils.SifenTester.GetAmbienteInfo(ambiente),
            archivo = p12Path,
            basica = basic,
            completa = full,
            rucConsultado = rucEmpresa
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint para enviar una venta a SIFEN (wiring inicial)
// Nota: hoy valida y deja listo para integrar al generador XML del DE
app.MapPost("/ventas/{idVenta:int}/enviar-sifen", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEBuilderService deSvc,
    DEXmlBuilder xmlBuilder,
    Sifen sifen
) =>
{
    try
    {
        // 1) Validación previa (datos mínimos SIFEN)
        var valid = await deSvc.ValidarVentaAsync(idVenta);
        if (!valid.Ok)
        {
            // Intentar autocorrección: generar VentasPagos desde Composición de Caja y completar IdTipoIva en detalles
            await using (var dbTry = await dbFactory.CreateDbContextAsync())
            {
                bool hizoCambios = false;

                // 1) Si falta VentaPago, intentar construirlo desde Composición de Caja
                if (valid.Errores.Any(e => e.Contains("No se registró el pago (VentasPagos)")))
                {
                    var comp = await dbTry.ComposicionesCaja
                        .Include(c => c.Detalles!)
                        .FirstOrDefaultAsync(c => c.IdVenta == idVenta);
                    var venta0 = await dbTry.Ventas.FirstOrDefaultAsync(v => v.IdVenta == idVenta);
                    if (comp != null && venta0 != null)
                    {
                        var vp = await dbTry.VentasPagos
                            .Include(x => x.Detalles!)
                            .FirstOrDefaultAsync(x => x.IdVenta == idVenta);
                        if (vp == null)
                        {
                            vp = new VentaPago
                            {
                                IdVenta = idVenta,
                                CondicionOperacion = 1,
                                IdMoneda = venta0.IdMoneda,
                                TipoCambio = venta0.CambioDelDia,
                                ImporteTotal = venta0.Total
                            };
                            dbTry.VentasPagos.Add(vp);
                            await dbTry.SaveChangesAsync();
                        }
                        else
                        {
                            if (vp.Detalles != null && vp.Detalles.Count > 0)
                            {
                                dbTry.VentasPagosDetalles.RemoveRange(vp.Detalles);
                                await dbTry.SaveChangesAsync();
                            }
                            vp.CondicionOperacion = 1;
                            vp.IdMoneda = venta0.IdMoneda;
                            vp.TipoCambio = venta0.CambioDelDia;
                            vp.ImporteTotal = venta0.Total;
                            dbTry.VentasPagos.Update(vp);
                            await dbTry.SaveChangesAsync();
                        }

                        foreach (var d in comp.Detalles ?? new List<ComposicionCajaDetalle>())
                        {
                            var det = new VentaPagoDetalle
                            {
                                IdVentaPago = vp.IdVentaPago,
                                Medio = d.Medio,
                                IdMoneda = d.IdMoneda,
                                TipoCambio = d.TipoCambio,
                                Monto = d.Monto,
                                MontoGs = d.MontoGs,
                                TipoTarjeta = d.TipoTarjeta,
                                MarcaTarjeta = d.MarcaTarjeta,
                                NombreEmisorTarjeta = d.NombreEmisorTarjeta,
                                Ultimos4 = d.Ultimos4,
                                NumeroAutorizacion = d.NumeroAutorizacion,
                                BancoCheque = d.BancoCheque,
                                NumeroCheque = d.NumeroCheque,
                                FechaCobroCheque = d.FechaCobroCheque,
                                BancoTransferencia = d.BancoTransferencia,
                                NumeroComprobante = d.NumeroComprobante,
                                Observacion = d.Observacion
                            };
                            dbTry.VentasPagosDetalles.Add(det);
                        }
                        await dbTry.SaveChangesAsync();
                        hizoCambios = true;
                    }
                }

                // 2) Completar IdTipoIva de detalles si faltan, usando el producto
                var detallesSinIva = await dbTry.VentasDetalles.Where(d => d.IdVenta == idVenta && d.IdTipoIva == null).ToListAsync();
                if (detallesSinIva.Any())
                {
                    var prodIds = detallesSinIva.Select(d => d.IdProducto).Distinct().ToList();
                    var prods = await dbTry.Productos.Where(p => prodIds.Contains(p.IdProducto)).ToDictionaryAsync(p => p.IdProducto, p => p.IdTipoIva);
                    foreach (var d in detallesSinIva)
                    {
                        if (prods.TryGetValue(d.IdProducto, out var idTipoIva))
                        {
                            d.IdTipoIva = idTipoIva;
                        }
                    }
                    await dbTry.SaveChangesAsync();
                    hizoCambios = true;
                }

                if (hizoCambios)
                {
                    valid = await deSvc.ValidarVentaAsync(idVenta);
                }
            }

            if (!valid.Ok)
            {
                return Results.BadRequest(new { ok = false, errores = valid.Errores, advertencias = valid.Advertencias });
            }
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = $"Venta {idVenta} no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null)
            return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad; configure IdCSC/CSC y certificados." });

        // Obtener la caja para determinar el tipo de facturación
        var caja = venta.IdCaja.HasValue 
            ? await db.Cajas.AsNoTracking().FirstOrDefaultAsync(c => c.IdCaja == venta.IdCaja.Value)
            : null;
        var esFacturaElectronica = caja?.TipoFacturacion?.ToUpper()?.Contains("ELECTR") == true;

        // 2) Resolver ambiente y certificados
        // - Factura Electrónica: usar datos de SOCIEDAD
        // - Factura Autoimpresor: usar datos de SUCURSAL
        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        // Para rEnvioLote vista, usar recepción de lote - PRIORIDAD: BD > SifenConfig
        var urlEnvio = sociedad.DeUrlEnvioDocumentoLote ?? SistemIA.Utils.SifenConfig.GetEnvioLoteUrl(ambiente);
        var urlQrBase = sociedad.DeUrlQr ?? (SistemIA.Utils.SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));

        string p12Path, p12Pass;
        if (esFacturaElectronica)
        {
            // Factura Electrónica: SIEMPRE usar certificado de Sociedad
            p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
            p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        }
        else
        {
            // Autoimpresor: usar certificado de Sucursal (si tiene), sino de Sociedad
            p12Path = !string.IsNullOrWhiteSpace(venta.Sucursal?.CertificadoRuta) 
                ? venta.Sucursal!.CertificadoRuta! 
                : (sociedad.PathCertificadoP12 ?? string.Empty);
            p12Pass = !string.IsNullOrWhiteSpace(venta.Sucursal?.CertificadoPassword) 
                ? venta.Sucursal!.CertificadoPassword! 
                : (sociedad.PasswordCertificadoP12 ?? string.Empty);
        }
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12) en Sociedad." });

        // 3) Construir XML DE mínimo
    var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);

    // 4) Firmar y enviar
    var resultJson = await sifen.FirmarYEnviar(urlEnvio, urlQrBase, xmlString, p12Path, p12Pass);

        // 5) Persistir respuesta en Venta (mejorable: parsear JSON formalmente)
        venta.FechaEnvioSifen = DateTime.Now;
        venta.MensajeSifen = resultJson;
        // Inferir estado desde el código de respuesta
        if (resultJson.Contains("\"codigo\":\"0302\"") || resultJson.Contains("Aceptado", StringComparison.OrdinalIgnoreCase))
            venta.EstadoSifen = "ACEPTADO";
        else if (resultJson.Contains("\"codigo\":\"0160\"") || resultJson.Contains("XML Mal Formado", StringComparison.OrdinalIgnoreCase) || resultJson.Contains("error") || resultJson.Contains("Fault"))
            venta.EstadoSifen = "RECHAZADO";
        else
            venta.EstadoSifen = "ENVIADO";
        // Guardar también el XML DE base generado por nuestro builder por trazabilidad
        venta.XmlCDE = xmlString;
        // Extraer CDC e IdLote del JSON simple (ignorar "Tag not found.")
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(resultJson);
            if (doc.RootElement.TryGetProperty("cdc", out var cdcProp) && cdcProp.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var cdcVal = cdcProp.GetString();
                if (!string.IsNullOrWhiteSpace(cdcVal) && !string.Equals(cdcVal, "Tag not found.", StringComparison.OrdinalIgnoreCase))
                    venta.CDC = cdcVal;
            }
            
            // Extraer URL del QR (dCarQR) del documento SOAP enviado
            if (doc.RootElement.TryGetProperty("documento", out var docProp) && docProp.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var soapEnviado = docProp.GetString() ?? string.Empty;
                var qrMatch = System.Text.RegularExpressions.Regex.Match(soapEnviado, @"<dCarQR>([^<]+)</dCarQR>");
                if (qrMatch.Success)
                {
                    var urlQr = qrMatch.Groups[1].Value.Replace("&amp;", "&");
                    if (!string.IsNullOrWhiteSpace(urlQr))
                        venta.UrlQrSifen = urlQr;
                }
            }
            
            string? idLoteLocal = null;
            if (doc.RootElement.TryGetProperty("idLote", out var idLoteProp) && idLoteProp.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                idLoteLocal = idLoteProp.GetString();
            }
            if (string.IsNullOrWhiteSpace(idLoteLocal) || string.Equals(idLoteLocal, "Tag not found.", StringComparison.OrdinalIgnoreCase))
            {
                // Intentar extraer del campo 'respuesta' (SOAP completo) como fallback
                if (doc.RootElement.TryGetProperty("respuesta", out var respProp) && respProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var respXml = respProp.GetString() ?? string.Empty;
                    var m = System.Text.RegularExpressions.Regex.Match(respXml, "<(?:\\w+:)?dProtConsLote>([^<]+)</(?:\\w+:)?dProtConsLote>");
                    if (m.Success) idLoteLocal = m.Groups[1].Value;
                }
            }
            if (!string.IsNullOrWhiteSpace(idLoteLocal) && !string.Equals(idLoteLocal, "Tag not found.", StringComparison.OrdinalIgnoreCase))
            {
                venta.IdLote = idLoteLocal;
            }
        }
        catch { /* best effort */ }
        db.Ventas.Update(venta);
        await db.SaveChangesAsync();

        return Results.Ok(new { ok = true, estado = venta.EstadoSifen, idVenta, cdc = venta.CDC, idLote = venta.IdLote });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint para enviar una venta a SIFEN en modo SINCRÓNICO (recibe-de)
// Útil para diagnosticar si el 0160 proviene del wrapper de Lote o del DE en sí
app.MapPost("/ventas/{idVenta:int}/enviar-sifen-sync", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEBuilderService deSvc,
    DEXmlBuilder xmlBuilder,
    Sifen sifen
) =>
{
    try
    {
        // 1) Validar datos mínimos SIFEN
        var valid = await deSvc.ValidarVentaAsync(idVenta);
        if (!valid.Ok)
        {
            return Results.BadRequest(new { ok = false, errores = valid.Errores, advertencias = valid.Advertencias });
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = $"Venta {idVenta} no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null)
            return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad; configure IdCSC/CSC y certificados." });

        // Obtener la caja para determinar el tipo de facturación
        var caja = venta.IdCaja.HasValue 
            ? await db.Cajas.AsNoTracking().FirstOrDefaultAsync(c => c.IdCaja == venta.IdCaja.Value)
            : null;
        var esFacturaElectronica = caja?.TipoFacturacion?.ToUpper()?.Contains("ELECTR") == true;

        // 2) Resolver ambiente, URL de envío SINCRÓNICO y certificados
        // - Factura Electrónica: usar datos de SOCIEDAD
        // - Factura Autoimpresor: usar datos de SUCURSAL
        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        // PRIORIDAD: URL de BD > SifenConfig
        var urlEnvioDe = sociedad.DeUrlEnvioDocumento ?? SistemIA.Utils.SifenConfig.GetEnvioDeUrl(ambiente);
        var urlQrBase = sociedad.DeUrlQr ?? (SistemIA.Utils.SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));

        string p12Path, p12Pass;
        if (esFacturaElectronica)
        {
            // Factura Electrónica: SIEMPRE usar certificado de Sociedad
            p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
            p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        }
        else
        {
            // Autoimpresor: usar certificado de Sucursal (si tiene), sino de Sociedad
            p12Path = !string.IsNullOrWhiteSpace(venta.Sucursal?.CertificadoRuta) 
                ? venta.Sucursal!.CertificadoRuta! 
                : (sociedad.PathCertificadoP12 ?? string.Empty);
            p12Pass = !string.IsNullOrWhiteSpace(venta.Sucursal?.CertificadoPassword) 
                ? venta.Sucursal!.CertificadoPassword! 
                : (sociedad.PasswordCertificadoP12 ?? string.Empty);
        }
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12) en Sociedad." });

        if (!System.IO.File.Exists(p12Path))
            return Results.BadRequest(new { ok = false, error = $"No existe el archivo .p12 en la ruta: {p12Path}" });

        // 3) Construir y firmar el DE; devolver paquete SOAP rEnviDeRequest con xDE=Base64(GZip(DE))
        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);
        var soapSync = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", true);

        // 4) Enviar a SIFEN (recibe-de)
        var respSync = await sifen.Enviar(urlEnvioDe, soapSync, p12Path, p12Pass);

        // 5) Extraer datos de la respuesta SIFEN
        string codigo = sifen.GetTagValue(respSync, "ns2:dCodRes");
        if (string.IsNullOrWhiteSpace(codigo) || codigo == "Tag not found.") codigo = sifen.GetTagValue(respSync, "dCodRes");
        string mensaje = sifen.GetTagValue(respSync, "ns2:dMsgRes");
        if (string.IsNullOrWhiteSpace(mensaje) || mensaje == "Tag not found.") mensaje = sifen.GetTagValue(respSync, "dMsgRes");
        
        // FIX 20-Ene-2026: SIFEN retorna el CDC en <ns2:Id> dentro de rProtDe, NO en dCDC
        // La estructura es: rRetEnviDe > rProtDe > Id (44 dígitos del CDC)
        string cdc = sifen.GetTagValue(respSync, "ns2:Id");
        if (string.IsNullOrWhiteSpace(cdc) || cdc == "Tag not found.") cdc = sifen.GetTagValue(respSync, "Id");
        // Fallback: intentar extraer de dCDC por si alguna versión de SIFEN lo usa
        if (string.IsNullOrWhiteSpace(cdc) || cdc == "Tag not found." || cdc.Length != 44)
        {
            cdc = sifen.GetTagValue(respSync, "ns2:dCDC");
            if (string.IsNullOrWhiteSpace(cdc) || cdc == "Tag not found.") cdc = sifen.GetTagValue(respSync, "dCDC");
        }
        // Último fallback: extraer del XML enviado (atributo Id del DE)
        if (string.IsNullOrWhiteSpace(cdc) || cdc == "Tag not found." || cdc.Length != 44)
        {
            var cdcMatch = System.Text.RegularExpressions.Regex.Match(soapSync, @"<DE\s+Id=""(\d{44})""");
            if (cdcMatch.Success)
                cdc = cdcMatch.Groups[1].Value;
        }
        
        // Extraer URL del QR (dCarQR) del SOAP enviado para guardar en BD
        string urlQrSifen = sifen.GetTagValue(soapSync, "dCarQR");
        if (urlQrSifen == "Tag not found.") urlQrSifen = null;
        // Decodificar &amp; a & para URL legible
        urlQrSifen = urlQrSifen?.Replace("&amp;", "&");

        // 6) Persistir trazas y estado
        venta.FechaEnvioSifen = DateTime.Now;
        // Guardamos un JSON simple con documento (SOAP enviado) y respuesta (SOAP recibido)
        string esc(string s) => (s ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        var jsonMsg = $"{{\"modo\":\"sync\",\"codigo\":\"{esc(codigo)}\",\"mensaje\":\"{esc(mensaje)}\",\"documento\":\"{esc(soapSync)}\",\"respuesta\":\"{esc(respSync)}\"}}";
        venta.MensajeSifen = jsonMsg;
        
        // Determinar estado basado en código de respuesta
        bool esExitoso = codigo == "0260" || (mensaje?.Contains("satisfactoria", StringComparison.OrdinalIgnoreCase) ?? false);
        
        if (esExitoso && !string.IsNullOrWhiteSpace(cdc) && cdc.Length == 44)
        {
            venta.CDC = cdc;
            venta.EstadoSifen = "ACEPTADO";
            // Guardar URL del QR solo si fue aceptado
            if (!string.IsNullOrWhiteSpace(urlQrSifen))
                venta.UrlQrSifen = urlQrSifen;
        }
        else if ((codigo ?? string.Empty) == "0160" || (mensaje ?? string.Empty).IndexOf("mal formado", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            venta.EstadoSifen = "RECHAZADO";
        }
        else if (esExitoso)
        {
            // Código exitoso pero no pudimos extraer CDC - intentar del XML
            var cdcFromXml = System.Text.RegularExpressions.Regex.Match(xmlString, @"<DE\s+Id=""(\d{44})""");
            if (cdcFromXml.Success)
            {
                venta.CDC = cdcFromXml.Groups[1].Value;
                venta.EstadoSifen = "ACEPTADO";
                if (!string.IsNullOrWhiteSpace(urlQrSifen))
                    venta.UrlQrSifen = urlQrSifen;
            }
            else
            {
                venta.EstadoSifen = "ENVIADO";
            }
        }
        else
        {
            venta.EstadoSifen = "ENVIADO";
        }
        // Guardar el XML CDE base (sin firma SOAP) por trazabilidad
        venta.XmlCDE = xmlString;
        db.Ventas.Update(venta);
        await db.SaveChangesAsync();

        return Results.Ok(new { ok = true, estado = venta.EstadoSifen, idVenta, cdc = venta.CDC, codigo, mensaje });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// ========================================================================
// Endpoint para enviar una venta a SIFEN al estilo POWER BUILDER (DLL Sifen_26)
// FIX 12-Ene-2026: Replica exactamente el código del DLL que SÍ FUNCIONA
// ========================================================================
app.MapPost("/ventas/{idVenta:int}/enviar-sifen-powerbuilder", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEBuilderService deSvc,
    DEXmlBuilder xmlBuilder,
    Sifen sifen
) =>
{
    try
    {
        // 1) Validar datos mínimos SIFEN
        var valid = await deSvc.ValidarVentaAsync(idVenta);
        if (!valid.Ok)
        {
            return Results.BadRequest(new { ok = false, errores = valid.Errores });
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = $"Venta {idVenta} no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null)
            return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad; configure IdCSC/CSC y certificados." });

        // 2) Resolver ambiente, URLs y certificados
        // FIX 12-Ene-2026: rEnvioLote corresponde al endpoint LOTE (async), NO sync
        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        // PRIORIDAD: URL de BD > SifenConfig
        var urlEnvio = sociedad.DeUrlEnvioDocumentoLote ?? SistemIA.Utils.SifenConfig.GetEnvioLoteUrl(ambiente);  // LOTE! (recibe-lote.wsdl)
        var urlQrBase = ambiente == "prod"
            ? "https://ekuatia.set.gov.py/consultas/qr?"
            : "https://ekuatia.set.gov.py/consultas-test/qr?";

        string p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        string p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12) en Sociedad." });

        if (!System.IO.File.Exists(p12Path))
            return Results.BadRequest(new { ok = false, error = $"No existe el archivo .p12 en la ruta: {p12Path}" });

        // 3) Construir XML y firmar (SIN envolver en SOAP aún)
        //    FirmarSinEnviar con devolverBase64Zip=false devuelve el rDE firmado
        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);
        var xmlFirmado = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", false);
        
        // 4) Construir SOAP exactamente como Power Builder
        var soapPb = sifen.ConstruirSoapComoPoweBuilder(xmlFirmado);
        
        Console.WriteLine($"[SIFEN-PB] Enviando a: {urlEnvio}");
        Console.WriteLine($"[SIFEN-PB] SOAP length: {soapPb.Length}");
        
        // 5) Enviar a SIFEN (SYNC endpoint, pero con estructura rEnvioLote)
        var respuesta = await sifen.Enviar(urlEnvio, soapPb, p12Path, p12Pass);
        
        Console.WriteLine($"[SIFEN-PB] Respuesta: {respuesta?.Length ?? 0} chars");

        // 6) Extraer datos de respuesta
        string codigo = sifen.GetTagValue(respuesta, "ns2:dCodRes");
        if (string.IsNullOrWhiteSpace(codigo) || codigo == "Tag not found.") 
            codigo = sifen.GetTagValue(respuesta, "dCodRes");
        
        string mensaje = sifen.GetTagValue(respuesta, "ns2:dMsgRes");
        if (string.IsNullOrWhiteSpace(mensaje) || mensaje == "Tag not found.") 
            mensaje = sifen.GetTagValue(respuesta, "dMsgRes");
        
        string idLote = sifen.GetTagValue(respuesta, "ns2:dProtConsLote");
        if (string.IsNullOrWhiteSpace(idLote) || idLote == "Tag not found.") 
            idLote = sifen.GetTagValue(respuesta, "dProtConsLote");

        // 7) Actualizar venta
        venta.FechaEnvioSifen = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(idLote) && idLote != "Tag not found.")
        {
            venta.IdLote = idLote;
            venta.EstadoSifen = "ENVIADO";
        }
        else if (codigo == "0160")
        {
            venta.EstadoSifen = "RECHAZADO";
        }
        else
        {
            venta.EstadoSifen = "ENVIADO";
        }
        
        string esc(string s) => (s ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "");
        venta.MensajeSifen = $"{{\"modo\":\"powerbuilder\",\"codigo\":\"{esc(codigo)}\",\"mensaje\":\"{esc(mensaje)}\",\"idLote\":\"{esc(idLote)}\"}}";
        venta.XmlCDE = xmlFirmado;
        db.Ventas.Update(venta);
        await db.SaveChangesAsync();

        return Results.Ok(new { 
            ok = true, 
            estado = venta.EstadoSifen, 
            idVenta, 
            idLote = venta.IdLote,
            codigo, 
            mensaje,
            urlUsada = urlEnvio,
            soapLength = soapPb.Length
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SIFEN-PB] Error: {ex.Message}");
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// ========================================================================
// Endpoint para cancelar una venta en SIFEN (Evento Cancelación)
// Solo para ventas ACEPTADAS/APROBADAS dentro de las 48 horas
// ========================================================================
app.MapPost("/ventas/{idVenta:int}/cancelar-sifen", async (
    int idVenta,
    string motivo,
    IDbContextFactory<AppDbContext> dbFactory,
    IEventoSifenService eventoService
) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(motivo))
            return Results.BadRequest(new { ok = false, error = "Debe proporcionar un motivo (parámetro 'motivo')" });

        var result = await eventoService.EnviarCancelacionAsync(idVenta, motivo);
        
        if (result.Exito)
        {
            return Results.Ok(new { 
                ok = true, 
                mensaje = $"Venta {idVenta} cancelada exitosamente en SIFEN",
                codigo = result.Codigo,
                detalles = result.Mensaje
            });
        }
        else
        {
            return Results.BadRequest(new { 
                ok = false, 
                error = result.Mensaje,
                codigo = result.Codigo
            });
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[CANCELAR-SIFEN] Error: {ex.Message}");
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint para verificar si se puede cancelar una venta en SIFEN
app.MapGet("/ventas/{idVenta:int}/puede-cancelar-sifen", async (
    int idVenta,
    IEventoSifenService eventoService
) =>
{
    try
    {
        var (puedeCancelar, mensaje) = await eventoService.VerificarPuedeCancelarAsync(idVenta);
        return Results.Ok(new { ok = puedeCancelar, mensaje });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint para listar ventas aprobadas en SIFEN (para pruebas de cancelación)
app.MapGet("/ventas/sifen-aprobadas", async (IDbContextFactory<AppDbContext> dbFactory) =>
{
    await using var db = await dbFactory.CreateDbContextAsync();
    var ventas = await db.Ventas
        .Where(v => v.CDC != null && (v.EstadoSifen == "ACEPTADO" || v.EstadoSifen == "APROBADO" || v.EstadoSifen == "ENVIADO"))
        .OrderByDescending(v => v.FechaEnvioSifen)
        .Take(20)
        .Select(v => new {
            v.IdVenta,
            v.CDC,
            v.EstadoSifen,
            v.FechaEnvioSifen,
            v.Total,
            HorasDesdeEnvio = v.FechaEnvioSifen.HasValue 
                ? (int)Math.Round((DateTime.Now - v.FechaEnvioSifen.Value).TotalHours) 
                : (int?)null,
            PuedeCancelar = v.FechaEnvioSifen.HasValue 
                ? (DateTime.Now - v.FechaEnvioSifen.Value).TotalHours <= 48 
                : false
        })
        .ToListAsync();
    return Results.Ok(ventas);
});

// Endpoint para consultar el estado en SIFEN (por lote) y actualizar CDC/Estado
app.MapGet("/ventas/{idVenta:int}/consultar-sifen", async (
    int idVenta,
    string? idLote, // opcional por query
    IDbContextFactory<AppDbContext> dbFactory,
    Sifen sifen
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = $"Venta {idVenta} no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null)
            return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad; configure IdCSC/CSC y certificados." });

        // Tomar idLote: prioridad al query param; si no, extraer de MensajeSifen con varias estrategias
        string? idLoteLocal = string.IsNullOrWhiteSpace(idLote) ? null : idLote;
        try
        {
            if (idLoteLocal == null && !string.IsNullOrWhiteSpace(venta.MensajeSifen))
            {
                using var doc = System.Text.Json.JsonDocument.Parse(venta.MensajeSifen);
                var root = doc.RootElement;
                if (root.TryGetProperty("idLote", out var idLoteProp) && idLoteProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    idLoteLocal = idLoteProp.GetString();
                }
            }
        }
        catch { /* continuar con otras estrategias */ }

        if (idLoteLocal == null && !string.IsNullOrWhiteSpace(venta.MensajeSifen))
        {
            // Buscar en texto: propiedad JSON idLote
            try
            {
                var m = System.Text.RegularExpressions.Regex.Match(venta.MensajeSifen, "\\\"idLote\\\"\\s*:\\s*\\\"([^\\\"]+)\\\"");
                if (m.Success) idLoteLocal = m.Groups[1].Value;
            }
            catch { }

            // Buscar en XML: <dProtConsLote>...</dProtConsLote> con o sin prefijo
            if (idLoteLocal == null)
            {
                try
                {
                    var m2 = System.Text.RegularExpressions.Regex.Match(venta.MensajeSifen, "<(?:\\w+:)?dProtConsLote>([^<]+)</(?:\\w+:)?dProtConsLote>");
                    if (m2.Success) idLoteLocal = m2.Groups[1].Value;
                }
                catch { }
            }
        }

        // Validar idLote
        if (string.IsNullOrWhiteSpace(idLoteLocal) ||
            string.Equals(idLoteLocal, "Tag not found.", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(idLoteLocal, "null", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new { ok = false, error = "No se encontró idLote en la respuesta previa; reintente el envío o verifique MensajeSifen." });

        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        // PRIORIDAD: URL de BD > SifenConfig
        var urlConsulta = sociedad.DeUrlConsultaDocumentoLote ?? SistemIA.Utils.SifenConfig.GetConsultaLoteUrl(ambiente);
        // Siempre usar certificado de Sociedad para Factura Electrónica
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });

        // Construir manualmente la consulta (para poder devolverla al cliente y que pueda copiarla)
    // FIX 21-Ene-2026: dId debe ser 12 dígitos en formato DDMMYYYYHHMM (igual que en envío)
    var dId = DateTime.Now.ToString("ddMMyyyyHHmm");
    idLoteLocal = idLoteLocal?.Trim();

    // Construir XML con XmlDocument para asegurar formato correcto y namespaces esperados por SIFEN
    var xmlDoc = new System.Xml.XmlDocument();
    var decl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
    xmlDoc.AppendChild(decl);
    var soapNs = "http://www.w3.org/2003/05/soap-envelope";
    var sifenNs = "http://ekuatia.set.gov.py/sifen/xsd";
    var envelope = xmlDoc.CreateElement("soap", "Envelope", soapNs);
    xmlDoc.AppendChild(envelope);
    var header = xmlDoc.CreateElement("soap", "Header", soapNs);
    envelope.AppendChild(header);
    var body = xmlDoc.CreateElement("soap", "Body", soapNs);
    envelope.AppendChild(body);
    // Alinear con ejemplo oficial: usar rEnviConsLoteDe (sin sufijo 'Request')
    var req = xmlDoc.CreateElement("rEnviConsLoteDe", sifenNs);
    body.AppendChild(req);
    var dIdNode = xmlDoc.CreateElement("dId", sifenNs);
    dIdNode.InnerText = dId;
    req.AppendChild(dIdNode);
    var dProtNode = xmlDoc.CreateElement("dProtConsLote", sifenNs);
    dProtNode.InnerText = idLoteLocal ?? string.Empty;
    req.AppendChild(dProtNode);
    var consultaXml = xmlDoc.OuterXml;

    var resp = await sifen.Enviar(urlConsulta, consultaXml, p12Path, p12Pass);

        // Intentar extraer CDC y mensajes
        string codigo = sifen.GetTagValue(resp, "ns2:dCodRes");
        if (string.IsNullOrWhiteSpace(codigo)) codigo = sifen.GetTagValue(resp, "dCodRes");
        string mensaje = sifen.GetTagValue(resp, "ns2:dMsgRes");
        if (string.IsNullOrWhiteSpace(mensaje)) mensaje = sifen.GetTagValue(resp, "dMsgRes");
        string cdc = sifen.GetTagValue(resp, "ns2:dCDC");
        if (string.IsNullOrWhiteSpace(cdc) || cdc == "Tag not found.") cdc = sifen.GetTagValue(resp, "dCDC");

        // Fallback automático: si SIFEN responde 0160 (XML mal formado), reintentar sin sufijo "Request"
        if (codigo == "0160" || (mensaje?.IndexOf("mal formado", StringComparison.OrdinalIgnoreCase) >= 0))
        {
            var xmlDoc2 = new System.Xml.XmlDocument();
            var decl2 = xmlDoc2.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc2.AppendChild(decl2);
            var soapNs2 = "http://www.w3.org/2003/05/soap-envelope";
            var sifenNs2 = "http://ekuatia.set.gov.py/sifen/xsd";
            var envelope2 = xmlDoc2.CreateElement("soap", "Envelope", soapNs2);
            xmlDoc2.AppendChild(envelope2);
            var header2 = xmlDoc2.CreateElement("soap", "Header", soapNs2);
            envelope2.AppendChild(header2);
            var body2 = xmlDoc2.CreateElement("soap", "Body", soapNs2);
            envelope2.AppendChild(body2);
            var req2 = xmlDoc2.CreateElement("rEnviConsLoteDe", sifenNs2);
            body2.AppendChild(req2);
            var dIdNode2 = xmlDoc2.CreateElement("dId", sifenNs2);
            dIdNode2.InnerText = dId;
            req2.AppendChild(dIdNode2);
            var dProtNode2 = xmlDoc2.CreateElement("dProtConsLote", sifenNs2);
            dProtNode2.InnerText = idLoteLocal ?? string.Empty;
            req2.AppendChild(dProtNode2);
            var consultaXml2 = xmlDoc2.OuterXml;

            var resp2 = await sifen.Enviar(urlConsulta, consultaXml2, p12Path, p12Pass);
            // Recalcular datos con la nueva respuesta
            var codigo2 = sifen.GetTagValue(resp2, "ns2:dCodRes");
            if (string.IsNullOrWhiteSpace(codigo2)) codigo2 = sifen.GetTagValue(resp2, "dCodRes");
            var mensaje2 = sifen.GetTagValue(resp2, "ns2:dMsgRes");
            if (string.IsNullOrWhiteSpace(mensaje2)) mensaje2 = sifen.GetTagValue(resp2, "dMsgRes");
            var cdc2 = sifen.GetTagValue(resp2, "ns2:dCDC");
            if (string.IsNullOrWhiteSpace(cdc2) || cdc2 == "Tag not found.") cdc2 = sifen.GetTagValue(resp2, "dCDC");

            // Sustituir valores finales si la segunda consulta es más útil
            resp = resp2; consultaXml = consultaXml2; codigo = codigo2; mensaje = mensaje2; cdc = cdc2;
        }

        // Actualizar venta según resultado
    venta.MensajeSifen = (venta.MensajeSifen ?? string.Empty) + "\nCONSULTA_LOTE=" + System.Text.Json.JsonSerializer.Serialize(new { idLote = idLoteLocal, codigo, mensaje, cdc });
        if (!string.IsNullOrWhiteSpace(cdc) && cdc != "Tag not found.")
        {
            venta.CDC = cdc;
            venta.EstadoSifen = "ACEPTADO";
        }
        else
        {
            // Mantener ENVIADO si no hay CDC, pero propagar mensaje/código
            venta.EstadoSifen = string.IsNullOrWhiteSpace(venta.EstadoSifen) ? "ENVIADO" : venta.EstadoSifen;
        }
        db.Ventas.Update(venta);
        await db.SaveChangesAsync();

        // Metadatos adicionales de diagnóstico
        var detalles = new
        {
            ambiente,
            urlConsulta,
            dId,
            idLoteUsado = idLoteLocal,
            tamanos = new
            {
                consultaXml = (consultaXml ?? string.Empty).Length,
                respuestaXml = (resp ?? string.Empty).Length
            }
        };

        return Results.Ok(new
        {
            ok = true,
            idVenta,
            codigo,
            mensaje,
            cdc,
            consulta = sifen.FormatearXML(consultaXml),
            respuesta = sifen.FormatearXML(resp),
            detalles
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// ========== ENDPOINT: Consultar SIFEN por CDC (para ventas enviadas en modo SYNC) ==========
app.MapGet("/ventas/{idVenta:int}/consultar-sifen-cdc", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    Sifen sifen
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = $"Venta {idVenta} no encontrada" });

        // Verificar que tenga CDC
        if (string.IsNullOrWhiteSpace(venta.CDC) || venta.CDC.Length != 44)
            return Results.BadRequest(new { ok = false, error = "La venta no tiene un CDC válido (44 dígitos). Use consulta por lote si tiene IdLote." });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null)
            return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad; configure IdCSC/CSC y certificados." });

        // Configuración de ambiente y certificados
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlConsultaDe = sociedad.DeUrlConsultaDocumento ?? SistemIA.Utils.SifenConfig.GetConsultaDeUrl(ambiente);
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });

        // Construir SOAP para consulta de DE por CDC
        var dId = DateTime.Now.ToString("ddMMyyyyHHmm");
        var xmlDoc = new System.Xml.XmlDocument();
        var decl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(decl);
        
        var soapNs = "http://www.w3.org/2003/05/soap-envelope";
        var sifenNs = "http://ekuatia.set.gov.py/sifen/xsd";
        
        var envelope = xmlDoc.CreateElement("soap", "Envelope", soapNs);
        xmlDoc.AppendChild(envelope);
        
        var header = xmlDoc.CreateElement("soap", "Header", soapNs);
        envelope.AppendChild(header);
        
        var body = xmlDoc.CreateElement("soap", "Body", soapNs);
        envelope.AppendChild(body);
        
        // FIX 21-Ene-2026: Usar rEnviConsDeRequest (con sufijo Request) según DLL funcional
        // El DLL de referencia usa: rEnviConsDeRequest, NO rEnviConsDe
        var req = xmlDoc.CreateElement("rEnviConsDeRequest", sifenNs);
        body.AppendChild(req);
        
        var dIdNode = xmlDoc.CreateElement("dId", sifenNs);
        dIdNode.InnerText = dId;
        req.AppendChild(dIdNode);
        
        // dCDC: el CDC de 44 dígitos
        var dCdcNode = xmlDoc.CreateElement("dCDC", sifenNs);
        dCdcNode.InnerText = venta.CDC;
        req.AppendChild(dCdcNode);
        
        var consultaXml = xmlDoc.OuterXml;
        
        // Enviar consulta a SIFEN
        var resp = await sifen.Enviar(urlConsultaDe, consultaXml, p12Path, p12Pass);
        
        // Extraer datos de la respuesta
        // Códigos esperados: 0422 = CDC encontrado, 0423 = CDC no encontrado
        string codigo = sifen.GetTagValue(resp, "ns2:dCodRes");
        if (string.IsNullOrWhiteSpace(codigo) || codigo == "Tag not found.") 
            codigo = sifen.GetTagValue(resp, "dCodRes");
        
        string mensaje = sifen.GetTagValue(resp, "ns2:dMsgRes");
        if (string.IsNullOrWhiteSpace(mensaje) || mensaje == "Tag not found.") 
            mensaje = sifen.GetTagValue(resp, "dMsgRes");
        
        // Extraer estado del documento (dEstRes)
        string estadoDoc = sifen.GetTagValue(resp, "ns2:dEstRes");
        if (string.IsNullOrWhiteSpace(estadoDoc) || estadoDoc == "Tag not found.") 
            estadoDoc = sifen.GetTagValue(resp, "dEstRes");
        
        // Actualizar estado según respuesta
        if (codigo == "0422") // CDC encontrado
        {
            // Verificar si fue cancelado
            if (estadoDoc?.ToUpper().Contains("CANCEL") == true)
            {
                venta.EstadoSifen = "CANCELADO";
            }
            else
            {
                venta.EstadoSifen = "ACEPTADO";
            }
        }
        else if (codigo == "0423") // CDC no encontrado
        {
            venta.EstadoSifen = "NO_ENCONTRADO";
        }
        
        // Guardar trazas de consulta
        venta.MensajeSifen = (venta.MensajeSifen ?? string.Empty) + 
            "\nCONSULTA_CDC=" + System.Text.Json.JsonSerializer.Serialize(new { 
                cdc = venta.CDC, 
                codigo, 
                mensaje,
                estadoDocumento = estadoDoc,
                fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        
        db.Ventas.Update(venta);
        await db.SaveChangesAsync();
        
        return Results.Ok(new
        {
            ok = true,
            idVenta,
            cdc = venta.CDC,
            codigo,
            mensaje,
            estadoDocumento = estadoDoc,
            estadoSifen = venta.EstadoSifen,
            consulta = sifen.FormatearXML(consultaXml),
            respuesta = sifen.FormatearXML(resp),
            detalles = new { ambiente, urlConsultaDe, dId }
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint de depuración: muestra el último SOAP enviado (y respuesta) guardado en MensajeSifen
app.MapGet("/debug/ventas/{idVenta:int}/envio-detalle", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    Sifen sifen
) =>
{
    await using var db = await dbFactory.CreateDbContextAsync();
    var venta = await db.Ventas.FindAsync(idVenta);
    if (venta == null)
        return Results.NotFound(new { ok = false, error = "Venta no encontrada" });

    var msg = venta.MensajeSifen ?? string.Empty;
    string? codigo = null, mensaje = null, idLote = venta.IdLote, cdc = venta.CDC;
    string soapEnviado = string.Empty, respRecibida = string.Empty;
    try
    {
        using var doc = System.Text.Json.JsonDocument.Parse(msg);
        var root = doc.RootElement;
        if (root.TryGetProperty("codigo", out var pCodigo) && pCodigo.ValueKind == System.Text.Json.JsonValueKind.String)
            codigo = pCodigo.GetString();
        if (root.TryGetProperty("mensaje", out var pMsg) && pMsg.ValueKind == System.Text.Json.JsonValueKind.String)
            mensaje = pMsg.GetString();
        if (string.IsNullOrWhiteSpace(idLote) && root.TryGetProperty("idLote", out var pLote) && pLote.ValueKind == System.Text.Json.JsonValueKind.String)
            idLote = pLote.GetString();
        if (string.IsNullOrWhiteSpace(cdc) && root.TryGetProperty("cdc", out var pCdc) && pCdc.ValueKind == System.Text.Json.JsonValueKind.String)
            cdc = pCdc.GetString();
        if (root.TryGetProperty("documento", out var pDoc) && pDoc.ValueKind == System.Text.Json.JsonValueKind.String)
            soapEnviado = pDoc.GetString() ?? string.Empty;
        if (root.TryGetProperty("respuesta", out var pResp) && pResp.ValueKind == System.Text.Json.JsonValueKind.String)
            respRecibida = pResp.GetString() ?? string.Empty;
    }
    catch
    {
        // Si no es JSON (o está truncado), intentar heurística simple
    var mDoc = System.Text.RegularExpressions.Regex.Match(msg, "<env:Envelope[\\n\\r\\s\\S]*</env:Envelope>");
        if (mDoc.Success) soapEnviado = mDoc.Value;
    var mResp = System.Text.RegularExpressions.Regex.Match(msg, "<(?:(?:\\w+):)?Envelope[\\n\\r\\s\\S]*</(?:(?:\\w+):)?Envelope>");
        if (mResp.Success) respRecibida = mResp.Value;
    }

    var info = new
    {
        codigo,
        mensaje,
        idLote,
        cdc,
        tamanos = new { soap = soapEnviado.Length, respuesta = respRecibida.Length },
        soap = sifen.FormatearXML(soapEnviado),
        respuesta = sifen.FormatearXML(respRecibida)
    };

    return Results.Ok(new { ok = true, idVenta, estado = venta.EstadoSifen, info });
});

// Endpoint de depuración: decodifica el xDE (base64+ZIP) del último SOAP enviado y devuelve el XML interno rLoteDE
app.MapGet("/debug/ventas/{idVenta:int}/xde-decoded", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory
) =>
{
    await using var db = await dbFactory.CreateDbContextAsync();
    var venta = await db.Ventas.FindAsync(idVenta);
    if (venta == null)
        return Results.NotFound(new { ok = false, error = "Venta no encontrada" });

    var msg = venta.MensajeSifen ?? string.Empty;
    // Buscar el último SOAP enviado dentro del JSON simple
    var mSoap = System.Text.RegularExpressions.Regex.Match(msg, "<(?:(?:\\w+):)?Envelope[\\n\\r\\s\\S]*</(?:(?:\\w+):)?Envelope>");
    if (!mSoap.Success)
        return Results.BadRequest(new { ok = false, error = "No se encontró SOAP en MensajeSifen" });
    var soap = mSoap.Value;
    var mXde = System.Text.RegularExpressions.Regex.Match(soap, "<xDE>([A-Za-z0-9+/=]+)</xDE>");
    if (!mXde.Success)
        return Results.BadRequest(new { ok = false, error = "No se encontró xDE en el último SOAP" });
    try
    {
        var b64 = mXde.Groups[1].Value;
        var bytes = Convert.FromBase64String(b64);
        using var ms = new MemoryStream(bytes);
        
        // Intentar primero como ZIP (nuevo formato)
        try
        {
            using var zipArchive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Read);
            var entry = zipArchive.Entries.FirstOrDefault();
            if (entry == null)
                return Results.BadRequest(new { ok = false, error = "ZIP vacío" });
            using var entryStream = entry.Open();
            using var sr = new StreamReader(entryStream, Encoding.UTF8);
            var innerXml = sr.ReadToEnd();
            return Results.Ok(new { ok = true, formato = "ZIP", archivo = entry.FullName, xml = innerXml });
        }
        catch
        {
            // Si falla ZIP, intentar GZip (formato antiguo)
            ms.Position = 0;
            using var gz = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress);
            using var sr = new StreamReader(gz, Encoding.UTF8);
            var innerXml = sr.ReadToEnd();
            return Results.Ok(new { ok = true, formato = "GZip", xml = innerXml });
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint de depuración: muestra el SOAP que se VA A ENVIAR (antes de enviar)
app.MapGet("/debug/ventas/{idVenta:int}/preview-soap", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEXmlBuilder xmlBuilder,
    Sifen sifen,
    IConfiguration config
) =>
{
    await using var db = await dbFactory.CreateDbContextAsync();
    var venta = await db.Ventas
        .Include(v => v.Sucursal)
        .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
    if (venta == null)
        return Results.NotFound(new { ok = false, error = "Venta no encontrada" });

    try
    {
        // Generar XML del DE usando el builder existente
        string xmlRde = await xmlBuilder.ConstruirXmlAsync(idVenta);
        
        // Firmar el DE
        var p12Path = config["Sifen:CertificadoPath"] ?? @"C:\SistemIA\Certificados\WEN.pfx";
        var p12Pass = config["Sifen:CertificadoPassword"] ?? "!WEN@2024#$";
        var urlQR = config["Sifen:UrlQR"] ?? "https://ekuatia.set.gov.py/consultas/qr";
        
        string xmlRdeFirmado = sifen.FirmarSinEnviar(urlQR, xmlRde, p12Path, p12Pass);

        // Generar SOAP con ZIP
        string soapZip = sifen.ConstruirSoapEnvioLoteZipBase64(xmlRdeFirmado);

        // Extraer y decodificar el xDE para ver el contenido interno
        var mXde = System.Text.RegularExpressions.Regex.Match(soapZip, "<xDE>([A-Za-z0-9+/=]+)</xDE>");
        string contenidoZip = "";
        string nombreArchivo = "";
        if (mXde.Success)
        {
            var bytes = Convert.FromBase64String(mXde.Groups[1].Value);
            using var ms = new MemoryStream(bytes);
            using var zipArchive = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Read);
            var entry = zipArchive.Entries.FirstOrDefault();
            if (entry != null)
            {
                nombreArchivo = entry.FullName;
                using var entryStream = entry.Open();
                using var sr = new StreamReader(entryStream, Encoding.UTF8);
                contenidoZip = sr.ReadToEnd();
            }
        }

        return Results.Ok(new
        {
            ok = true,
            soapCompleto = sifen.FormatearXML(soapZip),
            zipContenido = new {
                nombreArchivo,
                xmlInterno = contenidoZip,
                tieneDeclaracionXml = contenidoZip.StartsWith("<?xml"),
                tieneNamespaceEnRLoteDE = contenidoZip.Contains("rLoteDE xmlns=") || contenidoZip.Contains("<rLoteDE xmlns"),
            },
            longitudBase64 = mXde.Success ? mXde.Groups[1].Value.Length : 0,
            nota = "Este es el SOAP que se enviará a SIFEN"
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message, stack = ex.StackTrace });
    }
});

// ===========================================================================
// ENDPOINT DE PRUEBA DE VARIANTES SOAP - Para debugging error 0160
// ===========================================================================
// Este endpoint prueba diferentes formatos de SOAP hasta encontrar uno que 
// SIFEN acepte. Útil para identificar el formato correcto cuando hay error 0160.
// ===========================================================================
app.MapPost("/debug/ventas/{idVenta:int}/probar-variantes", async (
    int idVenta,
    int? variante, // opcional: probar solo una variante específica (1-10)
    IDbContextFactory<AppDbContext> dbFactory,
    DEBuilderService deSvc,
    DEXmlBuilder xmlBuilder,
    Sifen sifen
) =>
{
    try
    {
        // Validar datos mínimos SIFEN
        var valid = await deSvc.ValidarVentaAsync(idVenta);
        if (!valid.Ok)
        {
            return Results.BadRequest(new { ok = false, errores = valid.Errores, advertencias = valid.Advertencias });
        }

        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = $"Venta {idVenta} no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null)
            return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });

        // Obtener configuración - PRIORIDAD: URL de BD > SifenConfig
        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlEnvioDe = sociedad.DeUrlEnvioDocumento ?? SistemIA.Utils.SifenConfig.GetEnvioDeUrl(ambiente);
        var urlEnvioLote = sociedad.DeUrlEnvioDocumentoLote ?? SistemIA.Utils.SifenConfig.GetEnvioLoteUrl(ambiente); // URL para LOTE
        var urlQrBase = sociedad.DeUrlQr ?? (SistemIA.Utils.SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;

        if (string.IsNullOrWhiteSpace(p12Path) || !System.IO.File.Exists(p12Path))
            return Results.BadRequest(new { ok = false, error = $"Certificado no encontrado: {p12Path}" });

        // Construir y firmar el XML (sin devolver SOAP, solo el XML firmado)
        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);
        var xmlFirmado = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", false);

        // Si se especificó una variante, probar solo esa
        if (variante.HasValue && variante.Value >= 1 && variante.Value <= 28)
        {
            var (soap, desc) = sifen.GenerarSoapVariante(xmlFirmado, variante.Value);
            Console.WriteLine($"\n[PROBAR VARIANTE] Probando variante {variante.Value}: {desc}");
            
            // Variantes 21, 24, 25, 26, 27, 28 usan formato LOTE, las demás usan SYNC
            var urlUsar = (variante.Value == 21 || variante.Value == 24 || variante.Value == 25 || variante.Value == 26 || variante.Value == 27 || variante.Value == 28) ? urlEnvioLote : urlEnvioDe;
            Console.WriteLine($"[PROBAR VARIANTE] URL: {urlUsar}");
            
            var resp = await sifen.Enviar(urlUsar, soap, p12Path, p12Pass);
            
            string codigo = sifen.GetTagValue(resp, "ns2:dCodRes");
            if (string.IsNullOrWhiteSpace(codigo) || codigo == "Tag not found.") 
                codigo = sifen.GetTagValue(resp, "dCodRes");
            string mensaje = sifen.GetTagValue(resp, "ns2:dMsgRes");
            if (string.IsNullOrWhiteSpace(mensaje) || mensaje == "Tag not found.") 
                mensaje = sifen.GetTagValue(resp, "dMsgRes");
            string cdc = sifen.GetTagValue(resp, "ns2:dCDC");
            if (string.IsNullOrWhiteSpace(cdc) || cdc == "Tag not found.") 
                cdc = sifen.GetTagValue(resp, "dCDC");
            
            // Para LOTE, buscar también el protocolo del lote
            string idLote = sifen.GetTagValue(resp, "ns2:dProtConsLote");
            if (string.IsNullOrWhiteSpace(idLote) || idLote == "Tag not found.") 
                idLote = sifen.GetTagValue(resp, "dProtConsLote");

            bool esExito = codigo != "0160" && codigo != "Tag not found.";
            
            return Results.Ok(new
            {
                ok = true,
                modoExito = esExito,
                variante = variante.Value,
                descripcion = desc,
                codigo,
                mensaje,
                cdc = string.IsNullOrWhiteSpace(cdc) || cdc == "Tag not found." ? null : cdc,
                idLote = string.IsNullOrWhiteSpace(idLote) || idLote == "Tag not found." ? null : idLote,
                urlUsada = urlUsar,
                soapEnviado = soap.Length > 5000 ? soap.Substring(0, 5000) + "... (truncado)" : soap,
                respuesta = sifen.FormatearXML(resp),
                nota = esExito ? "✅ Esta variante fue procesada sin error 0160" : "❌ Error 0160 - XML Mal Formado"
            });
        }

        // Si no se especificó variante, probar todas automáticamente
        var (exito, varExitosa, descExitosa, cod, msg, soapUsado, respFinal) = 
            await sifen.ProbarVariantesAsync(xmlFirmado, urlEnvioDe, p12Path, p12Pass, 23);

        if (exito)
        {
            return Results.Ok(new
            {
                ok = true,
                modoExito = true,
                varianteExitosa = varExitosa,
                descripcion = descExitosa,
                codigo = cod,
                mensaje = msg,
                nota = $"✅ La variante {varExitosa} fue aceptada por SIFEN",
                accion = $"Actualizar Sifen.cs para usar el formato de la variante {varExitosa}"
            });
        }
        else
        {
            return Results.Ok(new
            {
                ok = false,
                modoExito = false,
                mensaje = "Ninguna de las 23 variantes fue aceptada por SIFEN",
                ultimoCodigo = cod,
                ultimoMensaje = msg,
                nota = "❌ Todas las variantes recibieron error 0160. Revisar el XML interno.",
                sugerencia = "Verificar que el XML del DE (rDE) está correctamente estructurado"
            });
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message, stack = ex.StackTrace });
    }
});

// Endpoint de depuración para inspeccionar el estado SIFEN almacenado
app.MapGet("/debug/ventas/{id:int}/mensaje-sifen", async (int id, IDbContextFactory<AppDbContext> dbFactory) =>
{
    await using var db = await dbFactory.CreateDbContextAsync();
    var venta = await db.Ventas.FindAsync(id);
    if (venta == null)
        return Results.NotFound(new { ok = false, id, error = "Venta no encontrada" });

    return Results.Ok(new
    {
        ok = true,
        idVenta = id,
        estadoSifen = venta.EstadoSifen,
        idLote = venta.IdLote,
        cdc = venta.CDC,
        mensaje = venta.MensajeSifen,
    });
});

// Endpoint para obtener el xDE firmado (o paquete SOAP) sin enviar a SIFEN
app.MapGet("/debug/ventas/{idVenta:int}/xde", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEXmlBuilder xmlBuilder,
    Sifen sifen
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = "Venta no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });

        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlQrBase = sociedad.DeUrlQr ?? (SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
        // Siempre usar certificado de Sociedad para Factura Electrónica
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });

        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);

        var soapOrXml = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", true);
        return Results.Ok(new { ok = true, vista = "soap+xDE_base64_gzip", contenido = soapOrXml });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint de depuración: datos del emisor usados para SIFEN
app.MapGet("/debug/ventas/{idVenta:int}/emisor", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory
) =>
{
    await using var db = await dbFactory.CreateDbContextAsync();
    var venta = await db.Ventas
        .Include(v => v.Sucursal)
        .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
    if (venta == null) return Results.NotFound(new { ok = false, error = "Venta no encontrada" });

    var suc = venta.Sucursal;
    var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
    if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });

    var emisor = new
    {
        RUC = string.IsNullOrWhiteSpace(suc?.RUC) ? sociedad.RUC : suc!.RUC,
        DV = (suc?.DV) ?? sociedad.DV,
        RazonSocial = string.IsNullOrWhiteSpace(suc?.NombreEmpresa) ? (sociedad.Nombre ?? "") : suc!.NombreEmpresa!,
        Direccion = string.IsNullOrWhiteSpace(suc?.Direccion) ? (sociedad.Direccion ?? "") : suc!.Direccion!,
        Telefono = string.IsNullOrWhiteSpace(suc?.Telefono) ? (sociedad.Telefono ?? "") : suc!.Telefono!,
        Email = string.IsNullOrWhiteSpace(suc?.Correo) ? (sociedad.Email ?? "") : suc!.Correo!,
        Actividad = suc?.RubroEmpresa ?? "",
        Ambiente = suc?.Ambiente ?? "test",
        UrlQRBase = sociedad.DeUrlQr ?? SistemIA.Utils.SifenConfig.GetBaseUrl((suc?.Ambiente?.ToLower() == "prod") ? "prod" : "test")
    };

    return Results.Ok(new { ok = true, emisor });
});

// Endpoint para obtener el SOAP completo (sync) listo para enviar - DEBUG
app.MapGet("/debug/ventas/{idVenta:int}/soap-sync", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEXmlBuilder xmlBuilder,
    Sifen sifen
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = "Venta no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });

        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlQrBase = sociedad.DeUrlQr ?? (SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });

        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);
        // FirmarSinEnviar con devolverBase64Zip=true devuelve el SOAP completo para envío sync
        var soapSync = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", true);
        
        // Extraer solo el inicio para ver estructura
        var inicio500 = soapSync.Length > 500 ? soapSync.Substring(0, 500) : soapSync;
        
        return Results.Ok(new { 
            ok = true, 
            vista = "soap_sync_completo",
            descripcion = "SOAP completo listo para enviar al endpoint recibe.wsdl",
            inicio500chars = inicio500,
            soapCompleto = soapSync,
            largoTotal = soapSync.Length
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint para enviar un SOAP desde archivo a SIFEN - útil para probar XMLs de referencia
// Uso: POST /debug/enviar-soap-archivo?archivo=c:\ruta\al\archivo.xml
app.MapPost("/debug/enviar-soap-archivo", async (
    string archivo,
    Sifen sifen,
    IDbContextFactory<AppDbContext> dbFactory
) =>
{
    try
    {
        if (!System.IO.File.Exists(archivo))
            return Results.BadRequest(new { ok = false, error = $"Archivo no existe: {archivo}" });
        
        var soapXml = await System.IO.File.ReadAllTextAsync(archivo, Encoding.UTF8);
        Sifen.LogSifenDebug($"ENVÍO DESDE ARCHIVO: {archivo}", $"Longitud: {soapXml.Length}\n\n{soapXml}");
        
        await using var db = await dbFactory.CreateDbContextAsync();
        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null)
            return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });
        
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlEnvioDe = sociedad.DeUrlEnvioDocumento ?? SifenConfig.GetEnvioDeUrl(ambiente);
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(p12Path))
            return Results.BadRequest(new { ok = false, error = "Falta PathCertificadoP12 en Sociedad" });
        
        var respuesta = await sifen.Enviar(urlEnvioDe, soapXml, p12Path, p12Pass);
        
        var codigo = sifen.GetTagValue(respuesta, "ns2:dCodRes");
        var mensaje = sifen.GetTagValue(respuesta, "ns2:dMsgRes");
        var estado = sifen.GetTagValue(respuesta, "ns2:dEstRes");
        
        return Results.Ok(new {
            ok = true,
            archivoEnviado = archivo,
            urlDestino = urlEnvioDe,
            codigo,
            mensaje,
            estado,
            respuestaCompleta = respuesta
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message, stack = ex.StackTrace });
    }
});

// Endpoint para probar envío SIFEN con headers MÍNIMOS (estilo Java SOAP puro)
// Algunos proxies rechazan headers adicionales como X-Requested-With, Origin, etc.
app.MapPost("/debug/ventas/{idVenta:int}/enviar-sifen-minimo", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEXmlBuilder xmlBuilder,
    Sifen sifen
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .Include(v => v.Cliente)
            .Include(v => v.Caja)
            .Include(v => v.Moneda)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null)
            return Results.NotFound(new { ok = false, error = "Venta no encontrada" });
        
        var sociedad = await db.Sociedades.FirstOrDefaultAsync();
        if (sociedad == null)
            return Results.BadRequest(new { ok = false, error = "No hay Sociedad configurada" });
        
        // Generar XML (sin firmar aún)
        var xmlSinFirmar = await xmlBuilder.ConstruirXmlAsync(venta.IdVenta);
        var p12Path = sociedad.PathCertificadoP12 ?? "";
        var p12Pass = sociedad.PasswordCertificadoP12 ?? "";
        
        if (string.IsNullOrWhiteSpace(p12Path))
            return Results.BadRequest(new { ok = false, error = "Falta certificado P12" });
        
        // URL del QR para firmar
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        string urlQr = ambiente == "prod" 
            ? "https://ekuatia.set.gov.py/consultas/qr?"
            : "https://ekuatia.set.gov.py/consultas-test/qr?";
        
        // Firmar el XML (devuelve XML firmado en texto plano)
        var xdeFirmado = sifen.FirmarSinEnviar(urlQr, xmlSinFirmar, p12Path, p12Pass, "1", false);
        if (xdeFirmado.StartsWith("{\"error\":"))
            return Results.BadRequest(new { ok = false, error = xdeFirmado });
        
        // Construir SOAP
        var dIdEnvio = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var soap = $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
<env:Envelope
	xmlns:env=""http://www.w3.org/2003/05/soap-envelope"">
	<env:Header/>
	<env:Body>
		<rEnviDe
			xmlns=""http://ekuatia.set.gov.py/sifen/xsd"">
			<dId>{dIdEnvio}</dId>
			<xDE>
				{xdeFirmado}
			</xDE>
		</rEnviDe>
	</env:Body>
</env:Envelope>";
        
        // ENVIAR CON HEADERS MÍNIMOS
        var url = sociedad.DeUrlEnvioDocumento ?? SifenConfig.GetEnvioDeUrl(ambiente);
        
        Sifen.ClearSifenDebugLog();
        Sifen.LogSifenDebug("ENVÍO CON HEADERS MÍNIMOS", $"URL: {url}\nLongitud SOAP: {soap.Length}");
        
        // Configurar HttpClient con headers mínimos
        var handler = new HttpClientHandler();
        var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(p12Path, p12Pass, 
            System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.Exportable);
        handler.ClientCertificates.Add(cert);
        handler.ServerCertificateCustomValidationCallback = (msg, c, chain, errors) => true;
        handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        
        using var client = new HttpClient(handler);
        client.Timeout = TimeSpan.FromSeconds(120);
        
        // *** HEADERS IDÉNTICOS A LA LIBRERÍA JAVA OFICIAL (Roshka) ***
        // Java usa: Content-Type: application/xml; charset=utf-8
        // Java usa: User-Agent: rshk-jsifenlib/x.x.x (LVEA)
        using var content = new StringContent(soap, Encoding.UTF8);
        content.Headers.Clear();
        content.Headers.TryAddWithoutValidation("Content-Type", "application/xml; charset=utf-8");
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "rshk-jsifenlib/1.0.0 (LVEA)");
        
        Sifen.LogSifenDebug("Headers de envío (ESTILO JAVA ROSHKA)", "Content-Type: application/xml; charset=utf-8\nUser-Agent: rshk-jsifenlib/1.0.0 (LVEA)");
        Sifen.LogSifenDebug("SOAP completo", soap);
        
        var response = await client.PostAsync(url, content);
        var respuestaStr = await response.Content.ReadAsStringAsync();
        
        Sifen.LogSifenDebug($"RESPUESTA - Status: {response.StatusCode}", respuestaStr);
        
        var codigo = sifen.GetTagValue(respuestaStr, "ns2:dCodRes") ?? sifen.GetTagValue(respuestaStr, "dCodRes");
        var mensaje = sifen.GetTagValue(respuestaStr, "ns2:dMsgRes") ?? sifen.GetTagValue(respuestaStr, "dMsgRes");
        
        return Results.Ok(new {
            ok = true,
            modo = "Headers estilo Java Roshka (application/xml)",
            url,
            statusCode = (int)response.StatusCode,
            codigo,
            mensaje,
            idVenta,
            dId = dIdEnvio,
            verLogCompleto = "Debug/sifen_debug.log"
        });
    }
    catch (Exception ex)
    {
        Sifen.LogSifenDebug("ERROR", $"{ex.Message}\n{ex.StackTrace}");
        return Results.BadRequest(new { ok = false, error = ex.Message, stack = ex.StackTrace });
    }
});

// Endpoint para obtener el DE firmado en texto plano (sin gzip/base64), para comparar con el ejemplo oficial
app.MapGet("/debug/ventas/{idVenta:int}/de-firmado", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEXmlBuilder xmlBuilder,
    Sifen sifen
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = "Venta no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });

        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlQrBase = sociedad.DeUrlQr ?? (SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
        // Siempre usar certificado de Sociedad para Factura Electrónica
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });

        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);

        var xmlFirmado = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", false);
        // DEBUG 24-Ene-2026: Devolver XML raw (sin FormatearXML) para diagnóstico de encoding
        return Results.Ok(new { ok = true, vista = "de_firmado_xml", contenido = xmlFirmado, contenidoFormateado = sifen.FormatearXML(xmlFirmado) });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint para debug: mostrar el contenido interno del SOAP con ZIP (rLoteDE) antes de enviar
app.MapGet("/debug/ventas/{idVenta:int}/soap-zip-contenido", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEXmlBuilder xmlBuilder,
    Sifen sifen
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = "Venta no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });

        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlQrBase = sociedad.DeUrlQr ?? (SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });

        // Construir y firmar el XML del DE
        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);
        var xmlFirmado = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", false);

        // Ahora construir el contenido del ZIP para inspección
        // Este es el contenido INTERNO que va comprimido (rLoteDE sin namespace)
        var inner = new System.Xml.XmlDocument();
        var declInner = inner.CreateXmlDeclaration("1.0", "UTF-8", null);
        inner.AppendChild(declInner);
        var rLote = inner.CreateElement("rLoteDE"); // SIN namespace
        inner.AppendChild(rLote);

        var rdeDoc = new System.Xml.XmlDocument();
        rdeDoc.LoadXml(xmlFirmado);
        var rdeNode = rdeDoc.DocumentElement ?? throw new InvalidOperationException("rDE raíz no encontrado");
        var imported = inner.ImportNode(rdeNode, true);
        rLote.AppendChild(imported);

        // CRÍTICO: Usar SerializeXmlToUtf8String en lugar de OuterXml para preservar UTF-8
        var contenidoZip = SistemIA.Models.Sifen.SerializeXmlToUtf8String(inner);
        
        // Mostrar solo los primeros 500 chars del rLoteDE para ver si tiene namespace
        var inicio = contenidoZip.Length > 500 ? contenidoZip.Substring(0, 500) : contenidoZip;

        return Results.Ok(new { 
            ok = true, 
            vista = "contenido_interno_zip",
            descripcion = "Este es el XML que va DENTRO del ZIP (Base64). Verificar que <rLoteDE> NO tenga xmlns=''",
            inicioPrimeros500Chars = inicio,
            tieneXmlnsVacio = contenidoZip.Contains("xmlns=\"\""),
            tieneXmlnsSifen = contenidoZip.Contains("xmlns=\"http://ekuatia.set.gov.py/sifen/xsd\""),
            largoCOntenido = contenidoZip.Length
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint para descargar el XML firmado del DE (Factura Electrónica) por IdVenta
app.MapGet("/ventas/{idVenta:int}/xml", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEXmlBuilder xmlBuilder,
    Sifen sifen
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = "Venta no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });

        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlQrBase = sociedad.DeUrlQr ?? (SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
        // Siempre usar certificado de Sociedad para Factura Electrónica
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });

        // Construir el DE y firmarlo, devolviendo el XML firmado (no ZIP/Base64)
        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);
        var xmlFirmado = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", false);
        // CRÍTICO: NO formatear el XML firmado - rompe la firma digital
        // El XML debe permanecer exactamente como se firmó (minificado)
        var bytes = System.Text.Encoding.UTF8.GetBytes(xmlFirmado);
        var fileName = $"FacturaElectronica_{idVenta}.xml";
        return Results.File(bytes, "application/xml", fileName);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint de depuración: validar el DE firmado contra los XSD locales de SIFEN
app.MapGet("/debug/ventas/{idVenta:int}/validar-xsd", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEXmlBuilder xmlBuilder,
    Sifen sifen,
    IWebHostEnvironment env
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = "Venta no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });

        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlQrBase = sociedad.DeUrlQr ?? (SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
        // Siempre usar certificado de Sociedad para Factura Electrónica
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });

        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);
        var xmlFirmado = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", false);

        var errores = SistemIA.Utils.XmlValidationUtil.ValidateDeSignedXml(xmlFirmado, env.ContentRootPath);
        return Results.Ok(new
        {
            ok = errores.Count == 0,
            errores,
            resumen = new
            {
                totalErrores = errores.Count,
                estado = errores.Count == 0 ? "VALIDO" : "INVALIDO"
            }
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint: firmar y enviar un XML externo (sin Venta) para aislar problemas de modelo/firmado/transporte
app.MapPost("/debug/de-externo/enviar", async (
    EnvioExternoRequest req,
    IDbContextFactory<AppDbContext> dbFactory,
    Sifen sifen
) =>
{
    try
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Xml))
            return Results.BadRequest(new { ok = false, error = "Debe enviar 'xml' en el cuerpo." });
        var modo = (req.Modo ?? "lote").ToLower();
        var ambiente = (req.Ambiente ?? "test").ToLower() == "prod" ? "prod" : "test";

        await using var db = await dbFactory.CreateDbContextAsync();
        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad; configure certificados y URL QR." });

        // Permitir overrides opcionales desde el request (sin exponer secretos en logs), si agregamos futuras props
        var urlQrBase = sociedad.DeUrlQr ?? (SistemIA.Utils.SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12) en Sociedad." });
        if (!System.IO.File.Exists(p12Path))
            return Results.BadRequest(new { ok = false, error = $"No existe el archivo .p12 en la ruta: {p12Path}" });

        string codigo = string.Empty, mensaje = string.Empty, cdc = string.Empty, idLote = string.Empty;
        string soapEnviado = string.Empty, respRecibida = string.Empty;
        string urlDestino = string.Empty;

        // Aceptar XML en distintos formatos: rDE directo, o SOAP con xDE (rLoteDE/rDE o xDE en base64+gzip)
        string? rdeXml = SistemIA.ExternalXmlExtractor.TryExtractRde(req.Xml!);
        if (string.IsNullOrWhiteSpace(rdeXml))
            return Results.BadRequest(new { ok = false, error = "No se pudo encontrar un nodo rDE válido. Pegue rDE directamente o un SOAP con xDE que contenga rLoteDE/rDE o base64+gzip." });

        if (modo == "lote")
        {
            // PRIORIDAD: URL de BD > SifenConfig
            var urlEnvio = sociedad.DeUrlEnvioDocumentoLote ?? SistemIA.Utils.SifenConfig.GetEnvioLoteUrl(ambiente);
            urlDestino = urlEnvio;
            var resultJson = await sifen.FirmarYEnviar(urlEnvio, urlQrBase, rdeXml!, p12Path, p12Pass);
            // Devolver JSON tal cual, además de extraer campos útiles
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(resultJson);
                codigo = doc.RootElement.TryGetProperty("codigo", out var pCod) && pCod.ValueKind == System.Text.Json.JsonValueKind.String ? pCod.GetString() ?? string.Empty : string.Empty;
                mensaje = doc.RootElement.TryGetProperty("mensaje", out var pMsg) && pMsg.ValueKind == System.Text.Json.JsonValueKind.String ? pMsg.GetString() ?? string.Empty : string.Empty;
                cdc = doc.RootElement.TryGetProperty("cdc", out var pCdc) && pCdc.ValueKind == System.Text.Json.JsonValueKind.String ? pCdc.GetString() ?? string.Empty : string.Empty;
                idLote = doc.RootElement.TryGetProperty("idLote", out var pLote) && pLote.ValueKind == System.Text.Json.JsonValueKind.String ? pLote.GetString() ?? string.Empty : string.Empty;
                soapEnviado = doc.RootElement.TryGetProperty("documento", out var pDoc) && pDoc.ValueKind == System.Text.Json.JsonValueKind.String ? pDoc.GetString() ?? string.Empty : string.Empty;
                respRecibida = doc.RootElement.TryGetProperty("respuesta", out var pResp) && pResp.ValueKind == System.Text.Json.JsonValueKind.String ? pResp.GetString() ?? string.Empty : string.Empty;
            }
            catch { }

            return Results.Ok(new { ok = true, modo = "lote", ambiente, url = urlDestino, p12 = p12Path, soap = sifen.FormatearXML(soapEnviado), respuesta = sifen.FormatearXML(respRecibida), codigo, mensaje, cdc, idLote, bruto = resultJson });
        }
        else if (modo == "sync")
        {
            // PRIORIDAD: URL de BD > SifenConfig
            var urlEnvioDe = sociedad.DeUrlEnvioDocumento ?? SistemIA.Utils.SifenConfig.GetEnvioDeUrl(ambiente);
            urlDestino = urlEnvioDe;
            var soapSync = sifen.FirmarSinEnviar(urlQrBase, rdeXml!, p12Path, p12Pass, "1", true);
            soapEnviado = soapSync;
            respRecibida = await sifen.Enviar(urlEnvioDe, soapSync, p12Path, p12Pass);
            // Extraer campos comunes
            codigo = sifen.GetTagValue(respRecibida, "ns2:dCodRes");
            if (string.IsNullOrWhiteSpace(codigo) || codigo == "Tag not found.") codigo = sifen.GetTagValue(respRecibida, "dCodRes");
            mensaje = sifen.GetTagValue(respRecibida, "ns2:dMsgRes");
            if (string.IsNullOrWhiteSpace(mensaje) || mensaje == "Tag not found.") mensaje = sifen.GetTagValue(respRecibida, "dMsgRes");
            cdc = sifen.GetTagValue(respRecibida, "ns2:dCDC");
            if (string.IsNullOrWhiteSpace(cdc) || cdc == "Tag not found.") cdc = sifen.GetTagValue(respRecibida, "dCDC");

            return Results.Ok(new { ok = true, modo = "sync", ambiente, url = urlDestino, p12 = p12Path, codigo, mensaje, cdc, soap = sifen.FormatearXML(soapEnviado), respuesta = sifen.FormatearXML(respRecibida) });
        }
        else
        {
            return Results.BadRequest(new { ok = false, error = "Modo inválido. Use 'lote' o 'sync'." });
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint: consultar por CDC o por idLote (sin Venta)
app.MapPost("/debug/de-externo/consultar", async (
    ConsultaExternaRequest req,
    IDbContextFactory<AppDbContext> dbFactory,
    Sifen sifen
) =>
{
    try
    {
        if (req == null || string.IsNullOrWhiteSpace(req.Tipo) || string.IsNullOrWhiteSpace(req.Id))
            return Results.BadRequest(new { ok = false, error = "Debe enviar 'tipo' ('cdc'|'lote') y 'id'" });
        var tipo = req.Tipo.ToLower();
        var ambiente = (req.Ambiente ?? "test").ToLower() == "prod" ? "prod" : "test";

        await using var db = await dbFactory.CreateDbContextAsync();
        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad; configure certificados." });
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12) en Sociedad." });
        if (!System.IO.File.Exists(p12Path))
            return Results.BadRequest(new { ok = false, error = $"No existe el archivo .p12 en la ruta: {p12Path}" });

        string urlConsulta;
        string tipoConsulta;
        // PRIORIDAD: URL de BD > SifenConfig
        if (tipo == "cdc")
        {
            urlConsulta = sociedad.DeUrlConsultaDocumento ?? SistemIA.Utils.SifenConfig.GetConsultaDeUrl(ambiente);
            tipoConsulta = "2"; // por CDC
        }
        else if (tipo == "lote")
        {
            urlConsulta = sociedad.DeUrlConsultaDocumentoLote ?? SistemIA.Utils.SifenConfig.GetConsultaLoteUrl(ambiente);
            tipoConsulta = "3"; // por Lote
        }
        else
        {
            return Results.BadRequest(new { ok = false, error = "Tipo inválido. Use 'cdc' o 'lote'." });
        }

        var resp = await sifen.Consulta(urlConsulta, req.Id!, tipoConsulta, p12Path, p12Pass);
        return Results.Ok(new { ok = true, tipo, ambiente, url = urlConsulta, p12 = p12Path, respuesta = sifen.FormatearXML(resp) });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint: obtener configuración efectiva de envío SIFEN (para mostrar en UI)
app.MapGet("/debug/de-externo/config", async (
    string? ambiente,
    IDbContextFactory<AppDbContext> dbFactory
) =>
{
    var amb = (ambiente ?? "test").ToLower() == "prod" ? "prod" : "test";
    await using var db = await dbFactory.CreateDbContextAsync();
    var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
    var baseUrl = SistemIA.Utils.SifenConfig.GetBaseUrl(amb);
    var urlQr = sociedad?.DeUrlQr ?? (baseUrl + (amb == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
    return Results.Ok(new
    {
        ambiente = amb,
        urls = new
        {
            envioLote = SistemIA.Utils.SifenConfig.GetEnvioLoteUrl(amb),
            envioSync = SistemIA.Utils.SifenConfig.GetEnvioDeUrl(amb),
            consultaDe = SistemIA.Utils.SifenConfig.GetConsultaDeUrl(amb),
            consultaLote = SistemIA.Utils.SifenConfig.GetConsultaLoteUrl(amb),
            baseQr = urlQr
        },
        certificado = new
        {
            p12Path = sociedad?.PathCertificadoP12,
            // Por seguridad no devolvemos la contraseña
            tienePassword = string.IsNullOrWhiteSpace(sociedad?.PasswordCertificadoP12) ? false : true
        }
    });
});

// Endpoint de depuración: construir rEnvioLote (vista expandida y payload real) para una venta
app.MapGet("/debug/ventas/{idVenta:int}/soap-lote", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEXmlBuilder xmlBuilder,
    Sifen sifen
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = "Venta no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });

        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlQrBase = sociedad.DeUrlQr ?? (SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
        // Siempre usar certificado de Sociedad para Factura Electrónica
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });

        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);
        var xmlFirmado = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", false);

        var vista = sifen.ConstruirSoapEnvioLoteVista(xmlFirmado);
        var payload = sifen.ConstruirSoapEnvioLoteZipBase64(xmlFirmado);
        return Results.Ok(new { ok = true, vista = sifen.FormatearXML(vista), payload = sifen.FormatearXML(payload) });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint: comparar estructura del XML de referencia (ManualSifen/xml/*.xml) contra el XML firmado de una venta
app.MapGet("/debug/ventas/{idVenta:int}/comparar-xml", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEXmlBuilder xmlBuilder,
    Sifen sifen,
    IWebHostEnvironment env
) =>
{
    try
    {
        // Referencia desde carpeta ManualSifen/xml
        var refPath = SistemIA.Utils.XmlCompare.FindDefaultReferenceXml(env.ContentRootPath);
        if (string.IsNullOrWhiteSpace(refPath) || !System.IO.File.Exists(refPath))
            return Results.BadRequest(new { ok = false, error = "No se encontró XML de referencia en ManualSifen/xml" });
        var refXml = await System.IO.File.ReadAllTextAsync(refPath);

        // Construir el DE firmado (XML) para la venta
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas.Include(v => v.Sucursal).FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = "Venta no encontrada" });
        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });
        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlQrBase = sociedad.DeUrlQr ?? (SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
        // Siempre usar certificado de Sociedad para Factura Electrónica
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });
        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);
        var xmlFirmado = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", false);

        // Comparar estructuras (elementos/atributos) ignorando namespaces
        var result = SistemIA.Utils.XmlCompare.CompareIgnoringSignature(refXml, xmlFirmado);
        return Results.Ok(new
        {
            ok = true,
            referencia = refPath,
            missingElements = result.MissingElements,
            extraElements = result.ExtraElements,
            missingAttributes = result.MissingAttributes,
            extraAttributes = result.ExtraAttributes
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint: comparar contra el XML oficial "Extructura xml_DE.xml" del Manual SIFEN (raíz de ManualSifen)
app.MapGet("/debug/ventas/{idVenta:int}/comparar-xml-oficial", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEXmlBuilder xmlBuilder,
    Sifen sifen,
    IWebHostEnvironment env
) =>
{
    try
    {
        // Cargar referencia oficial desde ManualSifen/Extructura xml_DE.xml
        var refPath = Path.Combine(env.ContentRootPath, "ManualSifen", "Extructura xml_DE.xml");
        if (!System.IO.File.Exists(refPath))
            return Results.BadRequest(new { ok = false, error = $"No se encontró archivo de referencia en {refPath}" });
        var refXml = await System.IO.File.ReadAllTextAsync(refPath);

        // Construir el DE firmado (XML) para la venta
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas.Include(v => v.Sucursal).FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = "Venta no encontrada" });
        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });
        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlQrBase = sociedad.DeUrlQr ?? (SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
        // Siempre usar certificado de Sociedad para Factura Electrónica
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });

        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);
        var xmlFirmado = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", false);

        // Comparar estructuras (elementos/atributos) ignorando namespaces y firmas
        var result = SistemIA.Utils.XmlCompare.CompareIgnoringSignature(refXml, xmlFirmado);
        return Results.Ok(new
        {
            ok = true,
            referencia = refPath,
            missingElements = result.MissingElements,
            extraElements = result.ExtraElements,
            missingAttributes = result.MissingAttributes,
            extraAttributes = result.ExtraAttributes
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint: comparar estructura contra un XML de referencia provisto por el cliente (POST)
app.MapPost("/debug/ventas/{idVenta:int}/comparar-xml", async (
    int idVenta,
    HttpContext http,
    IDbContextFactory<AppDbContext> dbFactory,
    DEXmlBuilder xmlBuilder,
    Sifen sifen
) =>
{
    try
    {
        // Leer cuerpo crudo como texto (acepta application/xml, text/xml, text/plain)
        http.Request.EnableBuffering();
        string refXml;
        using (var reader = new StreamReader(http.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true))
        {
            refXml = await reader.ReadToEndAsync();
        }
        http.Request.Body.Position = 0;
        // Saneos mínimos: quitar BOMs y espacios exteriores
        refXml = (refXml ?? string.Empty).Trim('\uFEFF', '\u200B', '\u0000', ' ', '\n', '\r', '\t');
        if (string.IsNullOrWhiteSpace(refXml))
            return Results.BadRequest(new { ok = false, error = "Debe enviar en el cuerpo el XML de referencia" });

        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas.Include(v => v.Sucursal).FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = "Venta no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null) return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });
        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
                var urlQrBase = sociedad.DeUrlQr ?? (SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
        // Siempre usar certificado de Sociedad para Factura Electrónica
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });

        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);
        var xmlFirmado = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", false);

        var result = SistemIA.Utils.XmlCompare.CompareIgnoringSignature(refXml, xmlFirmado);
        return Results.Ok(new
        {
            ok = true,
            missingElements = result.MissingElements,
            extraElements = result.ExtraElements,
            missingAttributes = result.MissingAttributes,
            extraAttributes = result.ExtraAttributes
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// ============================================
// ENDPOINT DEBUG: Diagnóstico completo error 0160
// Muestra el XML enviado en todas las variantes y las respuestas de SIFEN
// ============================================
app.MapPost("/debug/ventas/{idVenta:int}/diagnostico-0160", async (
    int idVenta,
    IDbContextFactory<AppDbContext> dbFactory,
    DEXmlBuilder xmlBuilder,
    Sifen sifen
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var venta = await db.Ventas
            .Include(v => v.Sucursal)
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
        if (venta == null) return Results.NotFound(new { ok = false, error = $"Venta {idVenta} no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null)
            return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });

        // FIX 14-Ene-2026: Ambiente debe tomarse de Sociedad.ServidorSifen, no de Sucursal
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        // PRIORIDAD: URL de BD > SifenConfig
        var urlEnvio = sociedad.DeUrlEnvioDocumentoLote ?? SifenConfig.GetEnvioLoteUrl(ambiente);
        var urlQrBase = sociedad.DeUrlQr ?? (SifenConfig.GetBaseUrl(ambiente) + (ambiente == "prod" ? "/consultas/qr?" : "/consultas-test/qr?"));
        var p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        var p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;

        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12)." });

        // 1. Construir XML del DE
        var xmlString = await xmlBuilder.ConstruirXmlAsync(idVenta);
        
        // 2. Firmar el DE
        var xmlFirmado = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", false);
        
        // 3. Obtener CDC del DE firmado
        string cdc = "";
        try
        {
            var docFirmado = new System.Xml.XmlDocument();
            docFirmado.LoadXml(xmlFirmado);
            cdc = docFirmado.SelectSingleNode("//*[local-name()='Id']")?.InnerText ?? "";
        }
        catch { }

        // 4. Probar cada variante de envío y guardar resultados
        var resultados = new List<object>();
        var variantes = new[]
        {
            ("rEnviLoteDe_zip", (Func<string, string, string>)sifen.ConstruirSoapEnvioLoteZipBase64),
            ("rEnviLoteDe_vista", (Func<string, string, string>)sifen.ConstruirSoapEnvioLoteVista),
            ("rEnvioLote_zip", (Func<string, string, string>)sifen.ConstruirSoapEnvioLoteZipBase64_EnvioLote),
            ("rEnvioLote_vista", (Func<string, string, string>)sifen.ConstruirSoapEnvioLoteVista_EnvioLote)
        };

        string? dId = cdc.Length >= 41 ? cdc.Substring(0, 41) : cdc;
        
        foreach (var (nombreVariante, construirSoap) in variantes)
        {
            try
            {
                var soapEnvio = construirSoap(xmlFirmado, dId);
                var respuesta = await sifen.Enviar(urlEnvio, soapEnvio, p12Path, p12Pass);
                
                // Extraer código de respuesta
                string codRes = "", msgRes = "";
                try
                {
                    var m1 = System.Text.RegularExpressions.Regex.Match(respuesta, @"<(?:\w+:)?dCodRes>([^<]+)</(?:\w+:)?dCodRes>");
                    if (m1.Success) codRes = m1.Groups[1].Value;
                    var m2 = System.Text.RegularExpressions.Regex.Match(respuesta, @"<(?:\w+:)?dMsgRes>([^<]+)</(?:\w+:)?dMsgRes>");
                    if (m2.Success) msgRes = m2.Groups[1].Value;
                }
                catch { }

                // Detectar si es respuesta del endpoint incorrecto
                bool esRespuestaIncorrecta = respuesta.Contains("rResEnviConsRUC") && !respuesta.Contains("rRetEnvi");
                
                resultados.Add(new
                {
                    variante = nombreVariante,
                    codigo = codRes,
                    mensaje = msgRes,
                    esRespuestaIncorrecta,
                    respuestaCompleta = respuesta.Length > 2000 ? respuesta.Substring(0, 2000) + "..." : respuesta
                });

                // Si encontramos una variante que funciona, podemos parar
                if (codRes == "0302" || codRes == "0300")
                {
                    break;
                }
            }
            catch (Exception exVar)
            {
                resultados.Add(new
                {
                    variante = nombreVariante,
                    codigo = "ERROR",
                    mensaje = exVar.Message,
                    esRespuestaIncorrecta = false,
                    respuestaCompleta = ""
                });
            }
        }

        return Results.Ok(new
        {
            ok = true,
            idVenta,
            cdc,
            ambiente,
            urlEnvio,
            certificado = Path.GetFileName(p12Path),
            xmlDeFirmadoPrimeros500Chars = xmlFirmado.Substring(0, Math.Min(500, xmlFirmado.Length)),
            resultadosPorVariante = resultados,
            causasPosibles0160 = new[]
            {
                "1. BIG-IP mezcló respuestas (reintentar)",
                "2. URL no termina en .wsdl",
                "3. Formato SOAP incorrecto para este ambiente",
                "4. XML del DE tiene estructura inválida",
                "5. Certificado expirado o inválido",
                "6. CDC duplicado (ya enviado antes)"
            }
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message, stack = ex.StackTrace });
    }
});

// Aplicar migraciones (solo si hay pendientes) y luego inicializar datos
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using (var db = await dbFactory.CreateDbContextAsync())
    {
        try
        {
            var pending = await db.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                await db.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Fallo al aplicar migraciones en startup. Continuando sin bloquear la aplicación.");
        }
    }

    var dataInitService = scope.ServiceProvider.GetRequiredService<IDataInitializationService>();
    await dataInitService.InicializarDatosListasPreciosAsync();
    await dataInitService.InicializarGeografiaSifenAsync();
    await dataInitService.InicializarArticulosAsistenteIAAsync();
}

// ============================================
// ENDPOINTS: Pagos a Proveedores - Recibo
// ============================================

app.MapGet("/pagos-proveedores/recibo/{idPago:int}", async (
    int idPago,
    IDbContextFactory<AppDbContext> dbFactory) =>
{
    try
    {
        using var db = await dbFactory.CreateDbContextAsync();
        
        var pago = await db.PagosProveedores
            .Include(p => p.Proveedor)
            .Include(p => p.Compra)
            .Include(p => p.Usuario)
            .Include(p => p.Moneda)
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPagoProveedor == idPago);

        if (pago == null)
            return Results.NotFound("Pago no encontrado");

        // Cargar datos de la caja y sucursal
        Caja? caja = null;
        Sucursal? sucursal = null;
        if (pago.IdCaja.HasValue)
        {
            caja = await db.Cajas.FirstOrDefaultAsync(c => c.IdCaja == pago.IdCaja.Value);
            if (caja?.IdSucursal.HasValue == true)
                sucursal = await db.Sucursal.FirstOrDefaultAsync(s => s.Id == caja.IdSucursal.Value);
        }
        if (sucursal == null)
        {
            sucursal = await db.Sucursal.FirstOrDefaultAsync(s => s.Id == pago.IdSucursal);
        }
        
        // Cargar sociedad
        var sociedad = await db.Sociedades.FirstOrDefaultAsync();

        var html = GenerarHtmlReciboPagoProveedor(pago, caja, sucursal, sociedad);
        return Results.Content(html, "text/html");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al generar recibo: {ex.Message}");
    }
});

app.MapGet("/pagos-proveedores/comprobante-a4/{idPago:int}", async (
    int idPago,
    IDbContextFactory<AppDbContext> dbFactory) =>
{
    try
    {
        using var db = await dbFactory.CreateDbContextAsync();
        
        var pago = await db.PagosProveedores
            .Include(p => p.Proveedor)
            .Include(p => p.Compra)
            .Include(p => p.Usuario)
            .Include(p => p.Moneda)
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPagoProveedor == idPago);

        if (pago == null)
            return Results.NotFound("Pago no encontrado");

        // Cargar datos de la caja y sucursal
        Caja? caja = null;
        Sucursal? sucursal = null;
        if (pago.IdCaja.HasValue)
        {
            caja = await db.Cajas.FirstOrDefaultAsync(c => c.IdCaja == pago.IdCaja.Value);
            if (caja?.IdSucursal.HasValue == true)
                sucursal = await db.Sucursal.FirstOrDefaultAsync(s => s.Id == caja.IdSucursal.Value);
        }
        if (sucursal == null)
        {
            sucursal = await db.Sucursal.FirstOrDefaultAsync(s => s.Id == pago.IdSucursal);
        }
        
        // Cargar sociedad
        var sociedad = await db.Sociedades.FirstOrDefaultAsync();

        var html = GenerarHtmlComprobanteA4PagoProveedor(pago, caja, sucursal, sociedad);
        return Results.Content(html, "text/html");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al generar comprobante: {ex.Message}");
    }
});

// ============================================================================
// NOTA DE CREDITO ELECTRONICA (NCE) - SIFEN
// ============================================================================

// Endpoint para enviar una Nota de Crédito a SIFEN en modo SINCRÓNICO
app.MapPost("/notascredito/{idNotaCredito:int}/enviar-sifen", async (
    int idNotaCredito,
    IDbContextFactory<AppDbContext> dbFactory,
    Sifen sifen
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var nc = await db.NotasCreditoVentas
            .Include(n => n.Sucursal)
            .Include(n => n.Cliente)
            .Include(n => n.Caja)
            .Include(n => n.Moneda)
            .Include(n => n.Detalles)!.ThenInclude(d => d.Producto)
            .Include(n => n.VentaAsociada)
            .FirstOrDefaultAsync(n => n.IdNotaCredito == idNotaCredito);
        
        if (nc == null) 
            return Results.NotFound(new { ok = false, error = $"Nota de Crédito {idNotaCredito} no encontrada" });

        if (nc.Estado != "Confirmada")
            return Results.BadRequest(new { ok = false, error = "Solo se pueden enviar a SIFEN notas de crédito confirmadas" });

        // Validar que tenga venta asociada con CDC
        if (nc.VentaAsociada == null || string.IsNullOrWhiteSpace(nc.VentaAsociada.CDC))
            return Results.BadRequest(new { ok = false, error = "La Nota de Crédito debe tener una factura asociada con CDC válido" });

        // Cargar Sociedad desde el DbSet
        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null)
            return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad; configure IdCSC/CSC y certificados." });

        // Resolver ambiente, URL y certificados
        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlEnvio = sociedad.DeUrlEnvioDocumento ?? SistemIA.Utils.SifenConfig.GetEnvioDeUrl(ambiente);
        var urlQrBase = sociedad.DeUrlQr ?? (ambiente == "prod" 
            ? "https://ekuatia.set.gov.py/consultas/qr?"
            : "https://ekuatia.set.gov.py/consultas-test/qr?");

        string p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        string p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(p12Path) || string.IsNullOrWhiteSpace(p12Pass))
            return Results.BadRequest(new { ok = false, error = "Falta configurar ruta/contraseña del certificado (.p12) en Sociedad." });

        if (!System.IO.File.Exists(p12Path))
            return Results.BadRequest(new { ok = false, error = $"No existe el archivo .p12 en la ruta: {p12Path}" });

        // Construir XML del NCE
        var xmlString = await SistemIA.Services.NCEXmlBuilder.ConstruirXmlAsync(nc, db);
        
        // Firmar y construir SOAP sincrónico
        var soapSync = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", true);

        // Enviar a SIFEN
        var respSync = await sifen.Enviar(urlEnvio, soapSync, p12Path, p12Pass);

        // Extraer datos de la respuesta
        string codigo = sifen.GetTagValue(respSync, "ns2:dCodRes");
        if (string.IsNullOrWhiteSpace(codigo) || codigo == "Tag not found.") codigo = sifen.GetTagValue(respSync, "dCodRes");
        string mensaje = sifen.GetTagValue(respSync, "ns2:dMsgRes");
        if (string.IsNullOrWhiteSpace(mensaje) || mensaje == "Tag not found.") mensaje = sifen.GetTagValue(respSync, "dMsgRes");
        
        string cdc = sifen.GetTagValue(respSync, "ns2:Id");
        if (string.IsNullOrWhiteSpace(cdc) || cdc == "Tag not found.") cdc = sifen.GetTagValue(respSync, "Id");
        if (string.IsNullOrWhiteSpace(cdc) || cdc == "Tag not found." || cdc.Length != 44)
        {
            var cdcMatch = System.Text.RegularExpressions.Regex.Match(soapSync, @"<DE\s+Id=""(\d{44})""");
            if (cdcMatch.Success) cdc = cdcMatch.Groups[1].Value;
        }
        
        // Extraer URL del QR
        string urlQrSifen = sifen.GetTagValue(soapSync, "dCarQR");
        if (urlQrSifen == "Tag not found.") urlQrSifen = null;
        urlQrSifen = urlQrSifen?.Replace("&amp;", "&");

        // Actualizar NC
        nc.FechaEnvioSifen = DateTime.Now;
        string esc(string s) => (s ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        nc.MensajeSifen = $"{{\"modo\":\"sync\",\"codigo\":\"{esc(codigo)}\",\"mensaje\":\"{esc(mensaje)}\"}}";
        
        bool esExitoso = codigo == "0260" || (mensaje?.Contains("satisfactoria", StringComparison.OrdinalIgnoreCase) ?? false);
        
        if (esExitoso && !string.IsNullOrWhiteSpace(cdc) && cdc.Length == 44)
        {
            nc.CDC = cdc;
            nc.EstadoSifen = "ACEPTADO";
            if (!string.IsNullOrWhiteSpace(urlQrSifen))
                nc.UrlQrSifen = urlQrSifen;
        }
        else if (codigo == "0160")
        {
            nc.EstadoSifen = "RECHAZADO";
        }
        else
        {
            nc.EstadoSifen = "ENVIADO";
        }
        
        nc.XmlCDE = xmlString;
        db.NotasCreditoVentas.Update(nc);
        await db.SaveChangesAsync();

        return Results.Ok(new { 
            ok = true, 
            estado = nc.EstadoSifen, 
            idNotaCredito, 
            cdc = nc.CDC, 
            codigo, 
            mensaje 
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint para obtener el XML firmado de una NC (debug)
app.MapGet("/notascredito/{idNotaCredito:int}/xml-firmado", async (
    int idNotaCredito,
    IDbContextFactory<AppDbContext> dbFactory,
    Sifen sifen
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var nc = await db.NotasCreditoVentas
            .Include(n => n.Sucursal)
            .Include(n => n.Cliente)
            .Include(n => n.Caja)
            .Include(n => n.Moneda)
            .Include(n => n.Detalles)!.ThenInclude(d => d.Producto)
            .Include(n => n.VentaAsociada)
            .FirstOrDefaultAsync(n => n.IdNotaCredito == idNotaCredito);
        
        if (nc == null) 
            return Results.NotFound(new { ok = false, error = $"Nota de Crédito {idNotaCredito} no encontrada" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null)
            return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });

        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlQrBase = sociedad.DeUrlQr ?? (ambiente == "prod" 
            ? "https://ekuatia.set.gov.py/consultas/qr?"
            : "https://ekuatia.set.gov.py/consultas-test/qr?");

        string p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        string p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;

        if (!System.IO.File.Exists(p12Path))
            return Results.BadRequest(new { ok = false, error = $"No existe el archivo .p12 en la ruta: {p12Path}" });

        var xmlString = await SistemIA.Services.NCEXmlBuilder.ConstruirXmlAsync(nc, db);
        var xmlFirmado = sifen.FirmarSinEnviar(urlQrBase, xmlString, p12Path, p12Pass, "1", false);

        return Results.Ok(new { 
            ok = true, 
            cdc = nc.CDC,
            xmlSinFirmar = xmlString,
            xmlFirmado = xmlFirmado
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// Endpoint para consultar estado de NC en SIFEN por IdLote
app.MapGet("/notascredito/{idNotaCredito:int}/consultar-sifen", async (
    int idNotaCredito,
    IDbContextFactory<AppDbContext> dbFactory,
    Sifen sifen
) =>
{
    try
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var nc = await db.NotasCreditoVentas
            .Include(n => n.Sucursal)
            .FirstOrDefaultAsync(n => n.IdNotaCredito == idNotaCredito);
        
        if (nc == null) 
            return Results.NotFound(new { ok = false, error = $"Nota de Crédito {idNotaCredito} no encontrada" });

        if (string.IsNullOrWhiteSpace(nc.IdLote))
            return Results.BadRequest(new { ok = false, error = "La NC no tiene IdLote registrado" });

        var sociedad = await db.Sociedades.AsNoTracking().FirstOrDefaultAsync();
        if (sociedad == null)
            return Results.BadRequest(new { ok = false, error = "No existe registro de Sociedad" });

        var ambiente = (sociedad.ServidorSifen ?? "test").ToLower();
        var urlConsulta = sociedad.DeUrlConsultaDocumentoLote ?? SistemIA.Utils.SifenConfig.GetConsultaLoteUrl(ambiente);

        string p12Path = sociedad.PathCertificadoP12 ?? string.Empty;
        string p12Pass = sociedad.PasswordCertificadoP12 ?? string.Empty;

        // Construir XML de consulta (mismo formato que facturas)
        var dId = DateTime.Now.ToString("ddMMyyyyHHmm");
        var idLoteLocal = nc.IdLote?.Trim();

        var xmlDoc = new System.Xml.XmlDocument();
        var decl = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(decl);
        var soapNs = "http://www.w3.org/2003/05/soap-envelope";
        var sifenNs = "http://ekuatia.set.gov.py/sifen/xsd";
        var envelope = xmlDoc.CreateElement("soap", "Envelope", soapNs);
        xmlDoc.AppendChild(envelope);
        var header = xmlDoc.CreateElement("soap", "Header", soapNs);
        envelope.AppendChild(header);
        var body = xmlDoc.CreateElement("soap", "Body", soapNs);
        envelope.AppendChild(body);
        var req = xmlDoc.CreateElement("rEnviConsLoteDe", sifenNs);
        body.AppendChild(req);
        var dIdNode = xmlDoc.CreateElement("dId", sifenNs);
        dIdNode.InnerText = dId;
        req.AppendChild(dIdNode);
        var dProtNode = xmlDoc.CreateElement("dProtConsLote", sifenNs);
        dProtNode.InnerText = idLoteLocal ?? string.Empty;
        req.AppendChild(dProtNode);
        var soapConsulta = xmlDoc.OuterXml;

        var respuesta = await sifen.Enviar(urlConsulta, soapConsulta, p12Path, p12Pass);

        string codigo = sifen.GetTagValue(respuesta, "ns2:dCodRes");
        if (codigo == "Tag not found.") codigo = sifen.GetTagValue(respuesta, "dCodRes");
        string mensaje = sifen.GetTagValue(respuesta, "ns2:dMsgRes");
        if (mensaje == "Tag not found.") mensaje = sifen.GetTagValue(respuesta, "dMsgRes");
        string estadoRes = sifen.GetTagValue(respuesta, "ns2:dEstRes");
        if (estadoRes == "Tag not found.") estadoRes = sifen.GetTagValue(respuesta, "dEstRes");

        // Actualizar estado si cambió
        if (estadoRes?.ToLower() == "aprobado" || codigo == "0260")
        {
            nc.EstadoSifen = "ACEPTADO";
            nc.MensajeSifen = $"Aprobado - Código: {codigo}";
            db.NotasCreditoVentas.Update(nc);
            await db.SaveChangesAsync();
        }
        else if (codigo == "0160" || estadoRes?.ToLower() == "rechazado")
        {
            nc.EstadoSifen = "RECHAZADO";
            nc.MensajeSifen = $"Rechazado - {mensaje}";
            db.NotasCreditoVentas.Update(nc);
            await db.SaveChangesAsync();
        }

        return Results.Ok(new { 
            ok = true, 
            estadoSifen = nc.EstadoSifen,
            codigo, 
            mensaje,
            estadoRes,
            respuestaCompleta = respuesta
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { ok = false, error = ex.Message });
    }
});

// ============================================
// INICIO AUTOMÁTICO DEL ACTUALIZADOR (puerto 5096)
// ============================================
// DESHABILITADO TEMPORALMENTE PARA DIAGNÓSTICO:
// IniciarActualizadorSiNoEstaActivo();

app.Run();

// Función para iniciar el Actualizador automáticamente como proceso INDEPENDIENTE
void IniciarActualizadorSiNoEstaActivo()
{
    try
    {
        // Verificar si el puerto 5096 ya está en uso
        bool actualizadorActivo = false;
        try
        {
            using var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(2);
            var response = client.GetAsync("http://localhost:5096").Result;
            actualizadorActivo = response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound;
        }
        catch
        {
            actualizadorActivo = false;
        }

        if (!actualizadorActivo)
        {
            // Buscar la ruta del Actualizador
            var posiblesRutas = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Actualizador", "SistemIA.Actualizador.exe"),
                Path.Combine(AppContext.BaseDirectory, "..", "Actualizador", "SistemIA.Actualizador.exe"),
                @"C:\SistemIA\Actualizador\SistemIA.Actualizador.exe",
                @"C:\asis\SistemIA.Actualizador\bin\Debug\net8.0\SistemIA.Actualizador.exe",
                @"C:\asis\SistemIA.Actualizador\bin\Release\net8.0\SistemIA.Actualizador.exe"
            };

            string? rutaActualizador = posiblesRutas.FirstOrDefault(File.Exists);

            if (rutaActualizador != null)
            {
                // Usar WMI (wmic) para crear un proceso completamente independiente
                // que sobrevive al cierre del proceso padre
                var wmiCommand = $"process call create \"{rutaActualizador}\"";
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "wmic",
                    Arguments = wmiCommand,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = System.Diagnostics.Process.Start(startInfo);
                process?.WaitForExit(5000); // Esperar máximo 5 segundos
                
                Console.WriteLine($"[Actualizador] Iniciado como proceso independiente desde: {rutaActualizador}");
            }
            else
            {
                Console.WriteLine("[Actualizador] No se encontró el ejecutable. Rutas buscadas:");
                foreach (var ruta in posiblesRutas)
                    Console.WriteLine($"  - {ruta}");
            }
        }
        else
        {
            Console.WriteLine("[Actualizador] Ya está activo en puerto 5096");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Actualizador] Error al intentar iniciar: {ex.Message}");
    }
}

// ============================================
// FUNCIÓN: Generar HTML Recibo Pago Proveedor
// ============================================
string GenerarHtmlReciboPagoProveedor(PagoProveedor pago, Caja? caja, Sucursal? sucursal, Sociedad? sociedad)
{
    var mediosPago = pago.Detalles != null && pago.Detalles.Any()
        ? string.Join("<br>", pago.Detalles.Select(d => 
            $"{d.MedioPago}: {d.Monto:N0} {(pago.Moneda?.Simbolo ?? "Gs.")}"))
        : $"EFECTIVO: {pago.MontoTotal:N0} {(pago.Moneda?.Simbolo ?? "Gs.")}";

    var empresaNombre = sociedad?.Nombre ?? "SU EMPRESA S.A.";
    var empresaRuc = sociedad?.RUC ?? "80000000-0";
    var empresaDireccion = sociedad?.Direccion ?? "Asunción, Paraguay";
    var empresaTelefono = sociedad?.Telefono ?? "(021) 000-000";

    var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Recibo de Pago #{pago.NumeroRecibo}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ 
            font-family: 'Courier New', monospace; 
            padding: 20mm;
            font-size: 12pt;
        }}
        .recibo {{
            max-width: 80mm;
            margin: 0 auto;
            border: 2px solid #000;
            padding: 10mm;
        }}
        .header {{
            text-align: center;
            border-bottom: 2px dashed #000;
            padding-bottom: 8px;
            margin-bottom: 10px;
        }}
        .empresa {{ 
            font-weight: bold; 
            font-size: 14pt;
            margin-bottom: 5px;
        }}
        .empresa-datos {{
            font-size: 9pt;
            margin-bottom: 5px;
        }}
        .tipo-doc {{ 
            font-weight: bold; 
            font-size: 11pt;
            margin: 8px 0;
        }}
        .seccion {{
            margin: 10px 0;
            padding: 5px 0;
        }}
        .linea {{
            display: flex;
            justify-content: space-between;
            margin: 3px 0;
        }}
        .label {{ 
            font-weight: bold;
            min-width: 120px;
        }}
        .valor {{ 
            text-align: right;
            flex: 1;
        }}
        .total {{
            border-top: 2px solid #000;
            border-bottom: 2px double #000;
            margin-top: 10px;
            padding: 8px 0;
            font-weight: bold;
            font-size: 13pt;
        }}
        .footer {{
            border-top: 2px dashed #000;
            margin-top: 15px;
            padding-top: 10px;
            text-align: center;
            font-size: 10pt;
        }}
        @media print {{
            body {{ padding: 5mm; }}
            .recibo {{ border: none; }}
        }}
    </style>
</head>
<body onload='window.print()'>
    <div class='recibo'>
        <div class='header'>
            <div class='empresa'>{empresaNombre}</div>
            <div class='empresa-datos'>
                RUC: {empresaRuc}<br>
                {empresaDireccion}<br>
                Tel: {empresaTelefono}
            </div>
            <div class='tipo-doc'>ORDEN DE PAGO A PROVEEDOR</div>
            <div>Recibo N°: {pago.NumeroRecibo}</div>
        </div>

        <div class='seccion'>
            <div class='linea'>
                <span class='label'>Fecha/Hora:</span>
                <span class='valor'>{pago.FechaPago:dd/MM/yyyy HH:mm}</span>
            </div>
            <div class='linea'>
                <span class='label'>Pago N°:</span>
                <span class='valor'>{pago.IdPagoProveedor}</span>
            </div>
            <div class='linea'>
                <span class='label'>Compra N°:</span>
                <span class='valor'>#{pago.IdCompra}</span>
            </div>
            {(sucursal != null ? $@"
            <div class='linea'>
                <span class='label'>Sucursal:</span>
                <span class='valor'>{sucursal.NombreSucursal}</span>
            </div>" : "")}
            {(caja != null ? $@"
            <div class='linea'>
                <span class='label'>Caja:</span>
                <span class='valor'>Caja #{caja.IdCaja}{(pago.Turno.HasValue ? $" - Turno {pago.Turno}" : "")}</span>
            </div>" : "")}
        </div>

        <div class='seccion'>
            <div class='label'>PROVEEDOR:</div>
            <div>{pago.Proveedor?.RazonSocial ?? "N/A"}</div>
            <div>RUC: {pago.Proveedor?.RUC}-{pago.Proveedor?.DV}</div>
        </div>

        <div class='seccion'>
            <div class='label'>MEDIOS DE PAGO:</div>
            <div>{mediosPago}</div>
        </div>

        <div class='seccion'>
            <div class='linea'>
                <span class='label'>Cambio del día:</span>
                <span class='valor'>{pago.CambioDelDia:N2} Gs.</span>
            </div>
        </div>

        <div class='total'>
            <div class='linea'>
                <span class='label'>TOTAL PAGADO:</span>
                <span class='valor'>{pago.MontoTotal:N0} {pago.Moneda?.Simbolo ?? "Gs."}</span>
            </div>
        </div>

        {(string.IsNullOrWhiteSpace(pago.Observaciones) ? "" : $@"
        <div class='seccion'>
            <div class='label'>OBSERVACIONES:</div>
            <div>{pago.Observaciones}</div>
        </div>
        ")}

        <div class='footer'>
            <div>Cajero: {pago.Usuario?.UsuarioNombre ?? "Sistema"}</div>
            <div style='margin-top: 10px;'>*** GRACIAS POR SU PREFERENCIA ***</div>
            <div style='margin-top: 15px; font-size: 9pt;'>
                Estado: {pago.Estado.ToUpper()}
            </div>
        </div>
    </div>
</body>
</html>";

    return html;
}

// ============================================
// FUNCIÓN: Generar Comprobante A4 Pago Proveedor
// ============================================
string GenerarHtmlComprobanteA4PagoProveedor(PagoProveedor pago, Caja? caja, Sucursal? sucursal, Sociedad? sociedad)
{
    var mediosPagoHtml = "";
    if (pago.Detalles != null && pago.Detalles.Any())
    {
        foreach (var detalle in pago.Detalles)
        {
            mediosPagoHtml += $@"
                <tr>
                    <td>{detalle.MedioPago}</td>
                    <td class='text-end'>{detalle.Monto:N0} {pago.Moneda?.Simbolo ?? "Gs."}</td>
                    <td>{detalle.NumeroCheque ?? detalle.NumeroTransferencia ?? detalle.Ultimos4Tarjeta ?? "-"}</td>
                </tr>";
        }
    }
    else
    {
        mediosPagoHtml = $@"
            <tr>
                <td>EFECTIVO</td>
                <td class='text-end'>{pago.MontoTotal:N0} {pago.Moneda?.Simbolo ?? "Gs."}</td>
                <td>-</td>
            </tr>";
    }

    var empresaNombre = sociedad?.Nombre ?? "SU EMPRESA S.A.";
    var empresaRuc = sociedad?.RUC ?? "80000000-0";
    var empresaDireccion = sociedad?.Direccion ?? "Asunción, Paraguay";
    var empresaTelefono = sociedad?.Telefono ?? "(021) 000-000";

    var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Comprobante de Pago #{pago.NumeroRecibo}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ 
            font-family: 'Segoe UI', Arial, sans-serif; 
            padding: 10mm;
            font-size: 10pt;
            background: #f5f5f5;
        }}
        .comprobante {{
            max-width: 210mm;
            margin: 0 auto;
            background: white;
            padding: 10mm;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        .header {{
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            border-bottom: 2px solid #2c3e50;
            padding-bottom: 8px;
            margin-bottom: 12px;
        }}
        .empresa-info {{
            flex: 1;
        }}
        .empresa-nombre {{ 
            font-size: 16pt;
            font-weight: bold;
            color: #2c3e50;
            margin-bottom: 3px;
        }}
        .empresa-datos {{
            color: #555;
            font-size: 8pt;
            line-height: 1.3;
        }}
        .documento-info {{
            text-align: right;
            border: 2px solid #2c3e50;
            padding: 6px 10px;
            background: #ecf0f1;
        }}
        .documento-tipo {{
            font-size: 11pt;
            font-weight: bold;
            color: #2c3e50;
            margin-bottom: 2px;
        }}
        .documento-numero {{
            font-size: 13pt;
            font-weight: bold;
            color: #e74c3c;
        }}
        .seccion {{
            margin: 10px 0;
        }}
        .seccion-titulo {{
            background: #34495e;
            color: white;
            padding: 5px 8px;
            font-weight: bold;
            font-size: 9pt;
            text-transform: uppercase;
            margin-bottom: 6px;
        }}
        .datos-grid {{
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 8px;
            margin-bottom: 8px;
        }}
        .campo {{
            margin-bottom: 4px;
        }}
        .campo-label {{
            font-weight: bold;
            color: #555;
            font-size: 8pt;
            text-transform: uppercase;
            margin-bottom: 1px;
        }}
        .campo-valor {{
            color: #000;
            font-size: 9pt;
            padding: 3px 5px;
            background: #f8f9fa;
            border-left: 2px solid #3498db;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin: 8px 0;
        }}
        table thead {{
            background: #34495e;
            color: white;
        }}
        table th {{
            padding: 5px 8px;
            text-align: left;
            font-size: 9pt;
            font-weight: 600;
        }}
        table td {{
            padding: 5px 8px;
            border-bottom: 1px solid #ddd;
            font-size: 9pt;
        }}
        table tbody tr:hover {{
            background: #f8f9fa;
        }}
        .text-end {{
            text-align: right;
        }}
        .totales {{
            margin-top: 10px;
            border-top: 2px solid #2c3e50;
            padding-top: 8px;
        }}
        .total-row {{
            display: flex;
            justify-content: space-between;
            padding: 6px 8px;
            margin: 3px 0;
        }}
        .total-final {{
            background: #2c3e50;
            color: white;
            font-size: 12pt;
            font-weight: bold;
            padding: 8px 10px;
            margin-top: 5px;
        }}
        .observaciones {{
            margin: 10px 0;
            padding: 8px;
            background: #fff3cd;
            border-left: 3px solid #ffc107;
            font-size: 9pt;
        }}
        .footer {{
            margin-top: 15px;
            padding-top: 10px;
            border-top: 2px dashed #999;
            text-align: center;
            color: #777;
            font-size: 8pt;
        }}
        .firma-linea {{
            margin: 20px auto 8px;
            width: 200px;
            border-top: 1px solid #000;
            text-align: center;
            padding-top: 5px;
            font-size: 9pt;
        }}
        .estado {{
            position: absolute;
            top: 15mm;
            right: 15mm;
            transform: rotate(-15deg);
            font-size: 18pt;
            font-weight: bold;
            padding: 6px 12px;
            border: 3px solid;
            opacity: 0.25;
        }}
        .estado-pagado {{
            color: #27ae60;
            border-color: #27ae60;
        }}
        .estado-anulado {{
            color: #e74c3c;
            border-color: #e74c3c;
        }}
        @media print {{
            body {{ 
                padding: 0; 
                background: white;
            }}
            .comprobante {{ 
                box-shadow: none;
                padding: 10mm;
            }}
            @page {{
                size: A4;
                margin: 0;
            }}
        }}
    </style>
</head>
<body onload='window.print()'>
    <div class='comprobante'>
        {(pago.Estado == "Anulado" ? 
            "<div class='estado estado-anulado'>ANULADO</div>" : 
            "<div class='estado estado-pagado'>PAGADO</div>")}
        
        <div class='header'>
            <div class='empresa-info'>
                <div class='empresa-nombre'>{empresaNombre}</div>
                <div class='empresa-datos'>
                    RUC: {empresaRuc}<br>
                    Dirección: {empresaDireccion}<br>
                    Teléfono: {empresaTelefono}
                </div>
            </div>
            <div class='documento-info'>
                <div class='documento-tipo'>COMPROBANTE DE PAGO</div>
                <div class='documento-tipo'>A PROVEEDOR</div>
                <div class='documento-numero'>N° {pago.NumeroRecibo}</div>
            </div>
        </div>

        <div class='seccion'>
            <div class='seccion-titulo'>Información del Pago</div>
            <div class='datos-grid'>
                <div>
                    <div class='campo'>
                        <div class='campo-label'>Número de Pago</div>
                        <div class='campo-valor'>#{pago.IdPagoProveedor}</div>
                    </div>
                    <div class='campo'>
                        <div class='campo-label'>Fecha y Hora</div>
                        <div class='campo-valor'>{pago.FechaPago:dd/MM/yyyy HH:mm:ss}</div>
                    </div>
                    <div class='campo'>
                        <div class='campo-label'>Compra Relacionada</div>
                        <div class='campo-valor'>Compra #{pago.IdCompra}</div>
                    </div>
                </div>
                <div>
                    <div class='campo'>
                        <div class='campo-label'>Sucursal</div>
                        <div class='campo-valor'>{sucursal?.NombreSucursal ?? "-"}</div>
                    </div>
                    <div class='campo'>
                        <div class='campo-label'>Caja</div>
                        <div class='campo-valor'>{(caja != null ? $"Caja #{caja.IdCaja}" : "Sin caja")}{(pago.Turno.HasValue ? $" - Turno {pago.Turno}" : "")}</div>
                    </div>
                    <div class='campo'>
                        <div class='campo-label'>Cajero/Usuario</div>
                        <div class='campo-valor'>{pago.Usuario?.UsuarioNombre ?? "Sistema"} - {pago.Usuario?.Nombres}</div>
                    </div>
                </div>
            </div>
            <div class='datos-grid'>
                <div>
                    <div class='campo'>
                        <div class='campo-label'>Moneda</div>
                        <div class='campo-valor'>{pago.Moneda?.Nombre ?? "Guaraníes"} ({pago.Moneda?.Simbolo ?? "Gs."})</div>
                    </div>
                </div>
                <div>
                    <div class='campo'>
                        <div class='campo-label'>Cambio del Día</div>
                        <div class='campo-valor'>{pago.CambioDelDia:N2} Gs.</div>
                    </div>
                </div>
            </div>
        </div>

        <div class='seccion'>
            <div class='seccion-titulo'>Datos del Proveedor</div>
            <div class='datos-grid'>
                <div>
                    <div class='campo'>
                        <div class='campo-label'>Razón Social</div>
                        <div class='campo-valor'>{pago.Proveedor?.RazonSocial ?? "N/A"}</div>
                    </div>
                    <div class='campo'>
                        <div class='campo-label'>RUC</div>
                        <div class='campo-valor'>{pago.Proveedor?.RUC}-{pago.Proveedor?.DV}</div>
                    </div>
                </div>
                <div>
                    <div class='campo'>
                        <div class='campo-label'>Nombre Fantasía</div>
                        <div class='campo-valor'>{pago.Proveedor?.NombreFantasia ?? "-"}</div>
                    </div>
                    <div class='campo'>
                        <div class='campo-label'>Teléfono</div>
                        <div class='campo-valor'>{pago.Proveedor?.Telefono ?? "-"}</div>
                    </div>
                </div>
            </div>
        </div>

        <div class='seccion'>
            <div class='seccion-titulo'>Detalle de Medios de Pago</div>
            <table>
                <thead>
                    <tr>
                        <th>Medio de Pago</th>
                        <th class='text-end'>Monto</th>
                        <th>Referencia</th>
                    </tr>
                </thead>
                <tbody>
                    {mediosPagoHtml}
                </tbody>
            </table>
        </div>

        {(string.IsNullOrWhiteSpace(pago.Observaciones) ? "" : $@"
        <div class='observaciones'>
            <strong>OBSERVACIONES:</strong><br>
            {pago.Observaciones}
        </div>
        ")}

        <div class='totales'>
            <div class='total-final'>
                <div class='total-row'>
                    <span>TOTAL PAGADO:</span>
                    <span>{pago.MontoTotal:N0} {pago.Moneda?.Simbolo ?? "Gs."}</span>
                </div>
            </div>
        </div>

        <div class='firma-linea'>
            Firma y Aclaración del Receptor
        </div>

        <div class='footer'>
            <p>Este documento certifica el pago realizado al proveedor en la fecha indicada.</p>
            <p>Estado del comprobante: <strong>{pago.Estado.ToUpper()}</strong></p>
            <p style='margin-top: 10px;'>Documento generado electrónicamente - {DateTime.Now:dd/MM/yyyy HH:mm}</p>
        </div>
    </div>
</body>
</html>";

    return html;
}