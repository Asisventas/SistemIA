using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SistemIA.Data;
using SistemIA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// --- INICIO DE LA SECCIÓN DE REGISTRO DE SERVICIOS ---
// Todos los servicios deben registrarse aquí, ANTES de builder.Build()

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://api.dnit.gov.py/") });
builder.Services.AddScoped<Sifen>();


builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 2 * 1024 * 1024;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7058, listen =>
    {
        listen.UseHttps("C:\\asis\\SistemIA\\certificados\\BlazorLocalDevCert.pfx", "1234");
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// --- FIN DE LA SECCIÓN DE REGISTRO DE SERVICIOS ---


// Aquí se "construye" la aplicación. A partir de aquí, no se pueden añadir más servicios.
var app = builder.Build();


// --- INICIO DE LA CONFIGURACIÓN DEL PIPELINE ---
// Aquí se configuran los middlewares y se inicializan tareas.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();