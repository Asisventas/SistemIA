using System.Text.RegularExpressions;

namespace SistemIA.Services;

/// <summary>
/// Servicio que escanea y mantiene un mapa de todas las rutas del sistema
/// para que la IA pueda navegar correctamente.
/// </summary>
public class RutasSistemaService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<RutasSistemaService> _logger;
    private List<RutaSistema> _rutas = new();
    private bool _escaneado = false;

    // ========== MAPEO DIRECTO DE CONSULTAS COMUNES ==========
    // Estas tienen PRIORIDAD ABSOLUTA sobre el algoritmo de búsqueda
    private readonly Dictionary<string[], string> _mapeoDirecto = new()
    {
        // Tema / Apariencia
        { new[] { "tema", "temas", "cambiar tema", "color", "colores", "apariencia", "modo oscuro", "modo claro", "dark mode", "light mode" }, "/configuracion/tema" },
        
        // Correo / Email
        { new[] { "correo", "correos", "email", "emails", "smtp", "mail", "configurar correo", "envio correo", "envío correo" }, "/configuracion/correo" },
        
        // Permisos / Accesos
        { new[] { "permiso", "permisos", "accesos", "rol", "roles", "privilegios", "permisos usuarios" }, "/personal/permisos-usuarios" },
        
        // Usuarios
        { new[] { "usuario", "usuarios", "crear usuario", "nuevo usuario", "personal" }, "/usuarios" },
        
        // Ventas
        { new[] { "venta", "ventas", "factura", "facturas", "facturar", "nueva venta", "crear venta" }, "/ventas" },
        { new[] { "explorar ventas", "buscar ventas", "historial ventas", "ver ventas" }, "/ventas/explorar" },
        
        // Compras
        { new[] { "compra", "compras", "nueva compra", "registrar compra" }, "/compras" },
        { new[] { "explorar compras", "buscar compras", "historial compras" }, "/compras/explorar" },
        
        // Productos
        { new[] { "producto", "productos", "nuevo producto", "crear producto", "articulo", "articulos" }, "/productos" },
        { new[] { "explorar productos", "buscar productos", "catalogo" }, "/productos/explorar" },
        
        // Clientes
        { new[] { "cliente", "clientes", "nuevo cliente", "crear cliente" }, "/clientes" },
        { new[] { "explorar clientes", "buscar clientes" }, "/clientes/explorar" },
        
        // Proveedores
        { new[] { "proveedor", "proveedores", "nuevo proveedor" }, "/proveedores" },
        
        // Caja
        { new[] { "cierre caja", "cerrar caja", "cierre de caja", "arqueo" }, "/caja/cierre" },
        { new[] { "historial cierres", "cierres anteriores" }, "/caja/historial-cierres" },
        
        // Stock / Inventario
        { new[] { "stock", "inventario", "existencias", "ajuste stock", "ajustar stock" }, "/inventario/ajustes" },
        { new[] { "transferencia", "transferencias", "mover stock", "trasladar" }, "/inventario/transferencias" },
        { new[] { "deposito", "depositos", "almacen", "bodegas" }, "/depositos" },
        
        // Notas de Crédito
        { new[] { "nota credito", "notas credito", "nota de credito", "devolucion", "devoluciones" }, "/notas-credito" },
        { new[] { "nota credito compra", "nc compra", "devolucion proveedor" }, "/notas-credito-compra" },
        
        // Informes
        { new[] { "informe", "informes", "reporte", "reportes", "estadisticas" }, "/informes" },
        { new[] { "informe ventas", "reporte ventas" }, "/informes/ventas-detallado" },
        { new[] { "informe compras", "reporte compras" }, "/informes/compras-detallado" },
        
        // Cobros / Pagos
        { new[] { "cobro", "cobros", "cobrar", "cuentas cobrar", "cuentas por cobrar" }, "/cobros" },
        { new[] { "pago", "pagos", "pagar proveedor", "cuentas pagar" }, "/pagos-proveedores" },
        
        // Presupuestos
        { new[] { "presupuesto", "presupuestos", "cotizacion", "cotizar", "proforma" }, "/presupuestos" },
        
        // Configuración general
        { new[] { "configuracion sistema", "config sistema", "opciones sistema" }, "/configuracion-sistema" },
        { new[] { "sociedad", "empresa", "datos empresa", "mi empresa" }, "/sociedad" },
        { new[] { "sucursal", "sucursales", "mi sucursal" }, "/sucursales" },
        { new[] { "timbrado", "timbrados", "numeracion factura" }, "/timbrados" },
        
        // SIFEN
        { new[] { "sifen", "factura electronica", "facturacion electronica", "set" }, "/admin/sifen" },
        
        // Asistencia / RRHH
        { new[] { "asistencia", "control asistencia", "marcacion", "entrada salida" }, "/asistencia" },
        { new[] { "horario", "horarios", "turnos" }, "/horarios" },
        
        // Actualizaciones
        { new[] { "actualizar", "actualizacion", "nueva version", "update" }, "/actualizacion" },
        
        // Admin IA
        { new[] { "asistente ia", "configurar ia", "entrenar ia", "admin ia" }, "/admin/asistente-ia" },
        { new[] { "rutas sistema", "mapa rutas" }, "/admin/rutas-sistema" },
    };

    public RutasSistemaService(IWebHostEnvironment env, ILogger<RutasSistemaService> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Busca primero en el mapeo directo, luego usa el algoritmo de búsqueda
    /// </summary>
    public RutaSistema? BuscarMejorRuta(string consulta)
    {
        if (!_escaneado) EscanearRutas();

        var consultaLimpia = consulta.ToLower().Trim();

        // 1. PRIORIDAD: Buscar en mapeo directo
        foreach (var mapeo in _mapeoDirecto)
        {
            foreach (var patron in mapeo.Key)
            {
                // Coincidencia exacta o contenida
                if (consultaLimpia.Contains(patron) || patron.Contains(consultaLimpia))
                {
                    var rutaDirecta = _rutas.FirstOrDefault(r => 
                        r.Ruta.Equals(mapeo.Value, StringComparison.OrdinalIgnoreCase));
                    if (rutaDirecta != null)
                    {
                        _logger.LogInformation($"Mapeo directo: '{consulta}' → '{mapeo.Value}'");
                        return rutaDirecta;
                    }
                }
            }
        }

        // 2. Si no hay mapeo directo, usar algoritmo de búsqueda
        return BuscarPorAlgoritmo(consultaLimpia);
    }

    private RutaSistema? BuscarPorAlgoritmo(string consultaLimpia)
    {
        var palabrasConsulta = ExtraerPalabrasSignificativas(consultaLimpia).ToHashSet();

        RutaSistema? mejorCoincidencia = null;
        int mejorPuntaje = 0;

        foreach (var ruta in _rutas)
        {
            int puntaje = 0;

            // Coincidencia exacta en título (muy alto peso)
            if (ruta.Titulo.ToLower().Contains(consultaLimpia))
                puntaje += 100;

            // Coincidencia en palabras clave
            foreach (var palabra in palabrasConsulta)
            {
                if (ruta.PalabrasClave.Any(pk => pk.Equals(palabra, StringComparison.OrdinalIgnoreCase)))
                    puntaje += 20;
                if (ruta.PalabrasClave.Any(pk => pk.Contains(palabra, StringComparison.OrdinalIgnoreCase)))
                    puntaje += 10;
            }

            // Coincidencia en título parcial
            foreach (var palabra in palabrasConsulta)
            {
                if (ruta.Titulo.ToLower().Contains(palabra))
                    puntaje += 15;
            }

            // Coincidencia en ruta
            foreach (var palabra in palabrasConsulta)
            {
                if (ruta.Ruta.ToLower().Contains(palabra))
                    puntaje += 12;
            }

            // Bonus por especificidad (rutas más largas son más específicas)
            puntaje += ruta.Ruta.Count(c => c == '/') * 2;

            if (puntaje > mejorPuntaje)
            {
                mejorPuntaje = puntaje;
                mejorCoincidencia = ruta;
            }
        }

        // Solo devolver si hay una coincidencia razonable
        return mejorPuntaje >= 20 ? mejorCoincidencia : null;
    }

    public List<RutaSistema> ObtenerRutas()
    {
        if (!_escaneado)
        {
            EscanearRutas();
        }
        return _rutas;
    }

    /// <summary>
    /// Escanea todas las páginas Razor del sistema y extrae rutas, títulos y palabras clave
    /// </summary>
    public void EscanearRutas()
    {
        _rutas.Clear();
        var pagesPath = Path.Combine(_env.ContentRootPath, "Pages");
        var sharedPath = Path.Combine(_env.ContentRootPath, "Shared");

        // Escanear Pages
        if (Directory.Exists(pagesPath))
        {
            foreach (var file in Directory.GetFiles(pagesPath, "*.razor", SearchOption.AllDirectories))
            {
                var ruta = ExtraerRutaDePagina(file);
                if (ruta != null)
                {
                    _rutas.Add(ruta);
                }
            }
        }

        _escaneado = true;
        _logger.LogInformation($"Escaneadas {_rutas.Count} rutas del sistema");
    }

    // Rutas de sistema que NO deben aparecer en búsquedas de navegación
    private readonly HashSet<string> _rutasExcluidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "/acceso-denegado",
        "/error",
        "/login",
        "/logout",
        "/no-autorizado",
        "/unauthorized",
        "/404",
        "/500",
        "/_host",
        "/_framework"
    };

    // Archivos que no son páginas de funcionalidad
    private readonly HashSet<string> _archivosExcluidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "AccesoDenegado",
        "Error",
        "Login",
        "Logout",
        "_Host",
        "_Layout",
        "Index" // Página de inicio, no es destino de navegación
    };

    private RutaSistema? ExtraerRutaDePagina(string filePath)
    {
        try
        {
            var contenido = File.ReadAllText(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);

            // Excluir archivos de sistema
            if (_archivosExcluidos.Contains(fileName))
                return null;

            // Extraer @page directive
            var pageMatch = Regex.Match(contenido, @"@page\s+""([^""]+)""");
            if (!pageMatch.Success)
                return null;

            var ruta = pageMatch.Groups[1].Value;

            // Excluir rutas de sistema
            if (_rutasExcluidas.Any(excl => ruta.StartsWith(excl, StringComparison.OrdinalIgnoreCase)))
                return null;
            
            // Ignorar rutas con parámetros complejos para navegación directa
            if (ruta.Contains("{") && !ruta.EndsWith("{Id:int}"))
                return null;

            // Extraer título de <h1>, <h2>, <h3> o PageTitle
            var titulo = ExtraerTitulo(contenido, fileName);

            // Extraer descripción/contexto
            var descripcion = ExtraerDescripcion(contenido, fileName);

            // Generar palabras clave
            var palabrasClave = GenerarPalabrasClave(ruta, titulo, descripcion, fileName);

            // Determinar categoría
            var categoria = DeterminarCategoria(ruta, fileName);

            // Determinar icono
            var icono = DeterminarIcono(ruta, categoria);

            return new RutaSistema
            {
                Ruta = ruta.Split('{')[0].TrimEnd('/'), // Quitar parámetros
                RutaCompleta = ruta,
                Titulo = titulo,
                Descripcion = descripcion,
                PalabrasClave = palabrasClave,
                Categoria = categoria,
                Icono = icono,
                ArchivoOrigen = Path.GetFileName(filePath)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error al procesar {filePath}");
            return null;
        }
    }

    private string ExtraerTitulo(string contenido, string fileName)
    {
        // Buscar PageTitle
        var pageTitleMatch = Regex.Match(contenido, @"<PageTitle>([^<]+)</PageTitle>");
        if (pageTitleMatch.Success)
            return LimpiarTexto(pageTitleMatch.Groups[1].Value);

        // Buscar h1, h2, h3 con texto
        var headerMatch = Regex.Match(contenido, @"<h[123][^>]*>([^<]+)</h[123]>");
        if (headerMatch.Success)
            return LimpiarTexto(headerMatch.Groups[1].Value);

        // Buscar h1-h3 con icono y texto
        var headerIconMatch = Regex.Match(contenido, @"<h[123][^>]*>.*?<i[^>]*></i>\s*([^<\n]+)", RegexOptions.Singleline);
        if (headerIconMatch.Success)
            return LimpiarTexto(headerIconMatch.Groups[1].Value);

        // Usar nombre del archivo
        return ConvertirNombreArchivo(fileName);
    }

    private string ExtraerDescripcion(string contenido, string fileName)
    {
        // Buscar card-header, alert, o texto descriptivo
        var descMatch = Regex.Match(contenido, @"<p[^>]*class=""[^""]*text-muted[^""]*""[^>]*>([^<]+)</p>");
        if (descMatch.Success)
            return LimpiarTexto(descMatch.Groups[1].Value);

        // Buscar comentarios de descripción
        var commentMatch = Regex.Match(contenido, @"@\*\s*Descripción:\s*([^\*]+)\*@");
        if (commentMatch.Success)
            return LimpiarTexto(commentMatch.Groups[1].Value);

        return "";
    }

    private List<string> GenerarPalabrasClave(string ruta, string titulo, string descripcion, string fileName)
    {
        var palabras = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Del título
        foreach (var p in ExtraerPalabrasSignificativas(titulo))
            palabras.Add(p);

        // De la ruta
        foreach (var parte in ruta.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!parte.StartsWith("{"))
            {
                palabras.Add(parte.Replace("-", " "));
                foreach (var p in parte.Split('-'))
                    if (p.Length > 2) palabras.Add(p);
            }
        }

        // Del nombre de archivo
        foreach (var p in ExtraerPalabrasSignificativas(ConvertirNombreArchivo(fileName)))
            palabras.Add(p);

        // De la descripción
        foreach (var p in ExtraerPalabrasSignificativas(descripcion))
            palabras.Add(p);

        // Sinónimos comunes
        AgregarSinonimos(palabras);

        return palabras.Where(p => p.Length > 2).ToList();
    }

    private void AgregarSinonimos(HashSet<string> palabras)
    {
        var sinonimos = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "venta", new[] { "factura", "facturar", "facturacion", "vender" } },
            { "compra", new[] { "comprar", "adquisicion", "pedido" } },
            { "cliente", new[] { "clientes", "comprador" } },
            { "producto", new[] { "productos", "articulo", "articulos", "item", "items", "mercaderia" } },
            { "proveedor", new[] { "proveedores", "vendedor", "distribuidor" } },
            { "correo", new[] { "email", "mail", "emails", "correos", "smtp", "electronico" } },
            { "usuario", new[] { "usuarios", "user", "personal" } },
            { "permiso", new[] { "permisos", "acceso", "rol", "roles", "privilegio" } },
            { "informe", new[] { "informes", "reporte", "reportes", "listado", "estadistica" } },
            { "caja", new[] { "cajas", "efectivo", "dinero", "arqueo" } },
            { "cierre", new[] { "cerrar", "cuadre", "cuadrar" } },
            { "stock", new[] { "inventario", "existencia", "existencias" } },
            { "ajuste", new[] { "ajustes", "ajustar", "modificar", "corregir" } },
            { "transferencia", new[] { "transferir", "mover", "trasladar" } },
            { "deposito", new[] { "depositos", "almacen", "bodega" } },
            { "cobro", new[] { "cobros", "cobrar", "recaudar", "recaudo" } },
            { "pago", new[] { "pagos", "pagar", "abonar", "abono" } },
            { "nota", new[] { "notas" } },
            { "credito", new[] { "creditos", "devolucion", "devoluciones" } },
            { "configuracion", new[] { "configurar", "config", "ajustes", "opciones", "preferencias" } },
            { "sociedad", new[] { "empresa", "compania", "negocio", "emisor" } },
            { "sucursal", new[] { "sucursales", "local", "tienda", "punto" } },
            { "timbrado", new[] { "timbrados", "numeracion" } },
            { "presupuesto", new[] { "presupuestos", "cotizacion", "cotizar", "proforma" } },
            { "asistencia", new[] { "asistencias", "entrada", "salida", "marcacion" } },
            { "horario", new[] { "horarios", "turno", "turnos", "jornada" } },
            { "actualizacion", new[] { "actualizar", "update", "version" } },
        };

        var palabrasOriginales = palabras.ToList();
        foreach (var palabra in palabrasOriginales)
        {
            // Buscar si es clave
            if (sinonimos.TryGetValue(palabra, out var sins))
            {
                foreach (var sin in sins)
                    palabras.Add(sin);
            }
            // Buscar si es valor (sinónimo)
            foreach (var kvp in sinonimos)
            {
                if (kvp.Value.Contains(palabra, StringComparer.OrdinalIgnoreCase))
                {
                    palabras.Add(kvp.Key);
                    foreach (var sin in kvp.Value)
                        palabras.Add(sin);
                }
            }
        }
    }

    private string DeterminarCategoria(string ruta, string fileName)
    {
        var rutaLower = ruta.ToLower();
        var fileLower = fileName.ToLower();

        if (rutaLower.Contains("venta") || fileLower.Contains("venta")) return "Ventas";
        if (rutaLower.Contains("compra") || fileLower.Contains("compra")) return "Compras";
        if (rutaLower.Contains("producto") || fileLower.Contains("producto")) return "Productos";
        if (rutaLower.Contains("cliente") || fileLower.Contains("cliente")) return "Clientes";
        if (rutaLower.Contains("proveedor") || fileLower.Contains("proveedor")) return "Proveedores";
        if (rutaLower.Contains("informe") || fileLower.Contains("informe") || fileLower.Contains("listado")) return "Informes";
        if (rutaLower.Contains("caja") || fileLower.Contains("caja") || fileLower.Contains("cierre")) return "Caja";
        if (rutaLower.Contains("inventario") || fileLower.Contains("stock") || fileLower.Contains("deposito")) return "Inventario";
        if (rutaLower.Contains("configuracion") || fileLower.Contains("config")) return "Configuración";
        if (rutaLower.Contains("usuario") || fileLower.Contains("usuario") || rutaLower.Contains("permiso")) return "Usuarios";
        if (rutaLower.Contains("personal") || fileLower.Contains("asistencia") || fileLower.Contains("horario")) return "Personal";
        if (rutaLower.Contains("nota") && rutaLower.Contains("credito")) return "Notas de Crédito";
        if (rutaLower.Contains("cobro") || fileLower.Contains("cobro")) return "Cobros";
        if (rutaLower.Contains("pago") || fileLower.Contains("pago")) return "Pagos";
        if (rutaLower.Contains("presupuesto") || fileLower.Contains("presupuesto")) return "Presupuestos";

        return "General";
    }

    private string DeterminarIcono(string ruta, string categoria)
    {
        return categoria switch
        {
            "Ventas" => "bi-cart-check",
            "Compras" => "bi-bag",
            "Productos" => "bi-box-seam",
            "Clientes" => "bi-people",
            "Proveedores" => "bi-truck",
            "Informes" => "bi-file-earmark-bar-graph",
            "Caja" => "bi-cash-stack",
            "Inventario" => "bi-boxes",
            "Configuración" => "bi-gear",
            "Usuarios" => "bi-person-badge",
            "Personal" => "bi-person-workspace",
            "Notas de Crédito" => "bi-receipt-cutoff",
            "Cobros" => "bi-currency-dollar",
            "Pagos" => "bi-credit-card",
            "Presupuestos" => "bi-file-text",
            _ => "bi-arrow-right"
        };
    }

    private IEnumerable<string> ExtraerPalabrasSignificativas(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) yield break;

        var stopWords = new HashSet<string> { "de", "la", "el", "los", "las", "un", "una", "y", "o", "a", "en", "por", "para", "con", "del", "al" };
        
        foreach (var palabra in Regex.Split(texto.ToLower(), @"[\s\-_/]+"))
        {
            var limpia = Regex.Replace(palabra, @"[^a-záéíóúñü]", "");
            if (limpia.Length > 2 && !stopWords.Contains(limpia))
                yield return limpia;
        }
    }

    private string LimpiarTexto(string texto)
    {
        // Quitar tags HTML
        texto = Regex.Replace(texto, @"<[^>]+>", " ");
        // Quitar código Razor
        texto = Regex.Replace(texto, @"@[^@\s]+", " ");
        // Quitar caracteres especiales
        texto = Regex.Replace(texto, @"[^\w\sáéíóúñü]", " ");
        // Normalizar espacios
        return Regex.Replace(texto.Trim(), @"\s+", " ");
    }

    private string ConvertirNombreArchivo(string fileName)
    {
        // CamelCase a palabras separadas
        var resultado = Regex.Replace(fileName, @"([a-z])([A-Z])", "$1 $2");
        return resultado;
    }

    /// <summary>
    /// Genera un JSON con todas las rutas para debugging/admin
    /// </summary>
    public string GenerarJsonRutas()
    {
        if (!_escaneado) EscanearRutas();
        return System.Text.Json.JsonSerializer.Serialize(_rutas, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
}

public class RutaSistema
{
    public string Ruta { get; set; } = "";
    public string RutaCompleta { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public List<string> PalabrasClave { get; set; } = new();
    public string Categoria { get; set; } = "";
    public string Icono { get; set; } = "";
    public string ArchivoOrigen { get; set; } = "";
}
