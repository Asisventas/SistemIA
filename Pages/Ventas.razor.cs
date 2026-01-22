using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using SistemIA.Models;
using SistemIA.Services;
using SistemIA.Utils;
using System.Globalization;
using System.Text;

namespace SistemIA.Pages;

public partial class Ventas
{
    [Inject] public IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] public ISucursalProvider SucursalProvider { get; set; } = default!;
    [Inject] public ICajaProvider CajaProvider { get; set; } = default!;
    [Inject] public IInventarioService Inventario { get; set; } = default!;
    [Inject] public IRucDnitService RucService { get; set; } = default!;
    [Inject] public NavigationManager Nav { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;
    [Inject] public Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] public PermisosService PermisosService { get; set; } = default!;
    [Inject] public Sifen SifenService { get; set; } = default!;
    [Inject] public DescuentoService DescuentoService { get; set; } = default!;
    [Inject] public ICorreoService CorreoService { get; set; } = default!;
    [Inject] public PdfFacturaService PdfService { get; set; } = default!;
    [Inject] public ITrackingService TrackingService { get; set; } = default!;
    [Inject] public AuditoriaService AuditoriaService { get; set; } = default!;
    [Inject] public ILoteService LoteService { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "presupuesto")]
    public int? PresupuestoId { get; set; }

    [SupplyParameterFromQuery(Name = "id")]
    public int? VentaId { get; set; }
    
    // Modo solo visualización (cuando se accede con ?id=X desde el explorador)
    protected bool ModoSoloVista { get; set; }
    
    // Permiso para editar precio (solo roles con permiso EDIT pueden modificar)
    protected bool PuedeEditarPrecio { get; set; } = false;
    
    // Permiso para modificar descuento (solo roles con permiso EDIT pueden cambiar)
    protected bool PuedeModificarDescuento { get; set; } = false;
    
    // Permiso para cambiar caja (solo roles con permiso EDIT pueden cambiar la caja)
    protected bool PuedeCambiarCaja { get; set; } = false;
    
    // Lista de cajas disponibles para selección
    protected List<Caja> CajasDisponibles { get; set; } = new();
    protected int? CajaSeleccionadaId { get; set; }

    // Cabecera en edición
    protected Venta Cab { get; set; } = new() { Fecha = DateTime.Today, TipoDocumento = "FACTURA" };
    protected List<VentaDetalle> Detalles { get; set; } = new();
    protected VentaDetalle NuevoDetalle { get; set; } = new() { Cantidad = 1, PrecioUnitario = 0 };
    
    // Descuento por ítem
    protected decimal NuevoDetalleDescuentoPorcentaje { get; set; } = 0;
    protected string? ErrorDescuento { get; set; }
    protected decimal PrecioOriginalSinDescuento { get; set; } = 0; // Para validar vs precio de costo

    // Catálogos mínimos
    protected List<Cliente> Clientes { get; set; } = new();
    protected List<Cliente> ClientesFiltrados { get; set; } = new();
    protected List<Producto> Productos { get; set; } = new();
    protected List<Producto> ProductosFiltrados { get; set; } = new();
    protected List<Moneda> Monedas { get; set; } = new();
    protected List<TipoPago> TiposPago { get; set; } = new();
    protected List<TipoDocumentoOperacion> TiposDocumento { get; set; } = new();
    private HashSet<int> _tiposServicioIds = new(); // IDs de TiposItem que son servicios (no controlan stock)
    private Dictionary<int, decimal> _stocksPorProducto = new(); // Stock total por producto (suma de depósitos)
    protected bool MostrarSugProductos { get; set; }
    protected bool MostrarSugClientes { get; set; }
    
    // Control de venta decimal por producto
    protected bool ProductoPermiteDecimal { get; set; } = false;
    
    // ========== VARIABLES PARA VENTA POR PAQUETE/UNIDAD ==========
    protected string ModoIngresoVenta { get; set; } = "paquete"; // "paquete" o "unidad"
    protected decimal _cantidadIngresadaVenta = 1;
    protected decimal? _cantidadPorPaqueteActual = null;
    protected bool _productoPermiteVentaPorUnidad = true;
    protected string _nombreUnidadVenta = "Unidad"; // "Paquete", "Caja" o "Unidad"
    protected decimal CantidadIngresadaVenta 
    { 
        get => _cantidadIngresadaVenta; 
        set 
        { 
            _cantidadIngresadaVenta = value;
            RecalcularCantidadPorModoVenta();
        } 
    }
    
    private void RecalcularCantidadPorModoVenta()
    {
        var cantPorPaq = _cantidadPorPaqueteActual ?? 1;
        if (ModoIngresoVenta == "paquete" && cantPorPaq > 1)
        {
            NuevoDetalle.Cantidad = _cantidadIngresadaVenta * cantPorPaq;
        }
        else
        {
            NuevoDetalle.Cantidad = _cantidadIngresadaVenta;
        }
        OnCantidadChanged();
    }
    
    private async void OnModoIngresoVentaChanged(ChangeEventArgs e)
    {
        ModoIngresoVenta = e.Value?.ToString() ?? "paquete";
        await RecalcularCantidadYPrecioPorModoVentaAsync();
    }
    
    /// <summary>
    /// Recalcula cantidad Y precio según el modo seleccionado (paquete/unidad).
    /// Usa el PrecioPaqueteGs cuando está en modo paquete y el producto lo tiene.
    /// </summary>
    private async Task RecalcularCantidadYPrecioPorModoVentaAsync()
    {
        if (NuevoDetalle.IdProducto <= 0) return;
        
        var prod = Productos.FirstOrDefault(p => p.IdProducto == NuevoDetalle.IdProducto);
        if (prod == null) return;
        
        var cantPorPaq = prod.CantidadPorPaquete ?? 1;
        
        // Recalcular cantidad según modo
        if (ModoIngresoVenta == "paquete" && cantPorPaq > 1)
        {
            NuevoDetalle.Cantidad = _cantidadIngresadaVenta * cantPorPaq;
        }
        else
        {
            NuevoDetalle.Cantidad = _cantidadIngresadaVenta;
        }
        
        // Recalcular precio según modo (esto ya considera PrecioPaqueteGs internamente)
        NuevoDetalle.PrecioUnitario = await CalcularPrecioRespetandoClientePrecioAsync(prod.IdProducto, Cab.IdCliente);
        
        // Guardar precio original para validaciones de descuento
        PrecioOriginalSinDescuento = NuevoDetalle.PrecioUnitario;
        
        // Recalcular el precio ministerio según modo
        if (ModoFarmaciaActivo)
        {
            if (ModoIngresoVenta == "paquete" && prod.PrecioMinisterioPaquete.HasValue && prod.PrecioMinisterioPaquete > 0 && cantPorPaq > 1)
            {
                // Precio ministerio por unidad dentro del paquete
                NuevoDetalle.PrecioMinisterio = prod.PrecioMinisterioPaquete.Value / cantPorPaq;
            }
            else
            {
                NuevoDetalle.PrecioMinisterio = prod.PrecioMinisterio;
            }
        }
        
        RecalcularNuevoDetalle();
        StateHasChanged();
    }
    
    /// <summary>
    /// Precio mostrado en la UI según el modo de ingreso (paquete/unidad).
    /// - Modo Paquete: Muestra el precio total del paquete (PrecioUnitario * CantidadPorPaquete)
    /// - Modo Unidad: Muestra el precio unitario directamente
    /// </summary>
    protected decimal PrecioMostrado
    {
        get
        {
            var cantPorPaq = _cantidadPorPaqueteActual ?? 1;
            
            // En modo paquete, mostrar el precio del paquete completo
            if (ModoIngresoVenta == "paquete" && cantPorPaq > 1)
            {
                return NuevoDetalle.PrecioUnitario * cantPorPaq;
            }
            return NuevoDetalle.PrecioUnitario;
        }
        set
        {
            var cantPorPaq = _cantidadPorPaqueteActual ?? 1;
            
            // En modo paquete, el usuario ingresa precio del paquete, guardar como unitario
            if (ModoIngresoVenta == "paquete" && cantPorPaq > 1)
            {
                NuevoDetalle.PrecioUnitario = value / cantPorPaq;
            }
            else
            {
                NuevoDetalle.PrecioUnitario = value;
            }
        }
    }

    // Presupuesto: validez
    protected int? ValidezDias { get; set; }
    protected DateTime? ValidoHasta { get; set; }

    // Números mostrados en UI
    protected string? NumeroFacturaUI { get; set; }
    protected string? NumeroPresupuestoUI { get; set; }
    protected string? TimbradoUI { get; set; }
    protected int? TurnoUI { get; set; }
    protected string? SucursalNombreUI { get; set; }
    protected string? EstablecimientoUI { get; set; }
    protected string? PuntoExpedicionUI { get; set; }
    protected string? NumeroSecuencialUI { get; set; }
    protected int? CajaIdUI { get; set; }
    protected string? CajaNombreUI { get; set; }
    protected string? FechaCajaUI { get; set; }
    protected DateTime? FechaEquipoAuditoria { get; set; }
    // Mensajes UI
    protected string? MensajeAdvertencia { get; set; }

    // ========== PROTECCIÓN CONTRA DOBLE GUARDADO ==========
    private bool _guardando = false;

    // UI: Vista Previa del Ticket (después de guardar)
    private bool _mostrarVistaPrevia;
    private int _idVentaParaVistaPrevia;
    
    // SIFEN: Tipo de facturación de la caja (ELECTRONICA o AUTOIMPRESOR)
    private string _tipoFacturacionCaja = "ELECTRONICA";

    // UI: Composición de Caja
    private bool _mostrarComposicionCaja;
    private List<ComposicionCajaDetalle> _cajaDetalles = new();
    private decimal _cajaTotalGs;
    private string? _cajaError;
    private string? _cajaInfo;
    private string _detalleCajaMedio = "EFECTIVO";
    private int _detalleCajaIdMoneda;
    private decimal _detalleCajaMonto;
    private string? _detalleCajaComprobante;
    private int? _cajaEditIndex;
    private ElementReference _montoInputRef;

    // UI: Modal para nuevo cliente rápido
    private bool _mostrarModalCliente;
    private bool _guardandoCliente;
    private string? _errorCliente;
    private string? _successCliente;
    private string _nuevoCliRazonSocial = string.Empty;
    private string _nuevoCliRuc = string.Empty;
    private int _nuevoCliDv;
    private string _nuevoCliTelefono = string.Empty;
    private string _nuevoCliEmail = string.Empty;
    private string _nuevoCliTipoDocumento = "RU"; // RUC por defecto
    private List<SistemIA.Models.TiposDocumentosIdentidad> _tiposDocumentosCliente = new();
    // SIFEN búsqueda de cliente
    private bool _buscandoClienteSifen;
    private string? _mensajeSifenCliente;
    private bool _esSifenClienteError;
    
    // UI: Búsqueda automática de RUC (al Tab)
    private bool _buscandoRucAuto;
    private string? _mensajeRucAuto;
    private bool _esRucAutoError;
    private RucBusquedaResultado? _resultadoRucAuto;

    // UI: Confirmación de stock cero
    private bool _mostrarConfirmStockCero;
    private Producto? _productoStockCeroPendiente;

    // UI: Advertencia de producto vencido
    private bool _mostrarAdvertenciaVencido;
    private Producto? _productoVencidoPendiente;
    
    // Control de escaneo de código de barras
    private DateTime _ultimoInputTime = DateTime.MinValue;
    private string _bufferCodigoBarras = string.Empty;
    private const int TIEMPO_MAXIMO_ENTRE_CHARS_MS = 50; // Los lectores de código escriben muy rápido
    
    // Usuario actual para registrar en movimientos de inventario
    private string _usuarioActual = "Sistema";
    private int? _idUsuarioActual;

    // UI: Modal para receta médica (productos controlados)
    private bool _mostrarModalReceta;
    private Producto? _productoRecetaPendiente;
    private string _recetaNumeroRegistro = string.Empty;
    private DateTime _recetaFecha = DateTime.Today;
    private string _recetaNombreMedico = string.Empty;
    private string _recetaNombrePaciente = string.Empty;
    private string? _recetaError;
    private List<RecetaVenta> _recetasPendientes = new(); // Recetas a guardar con la venta

    // Control de crédito
    protected bool MostrarCamposCredito { get; set; }
    protected bool TipoPagoEsCredito { get; set; }
    protected int NumeroCuotas { get; set; } = 1;

    // ========== CONFIGURACIÓN DE DESCUENTOS ==========
    // Configuración del sistema para descuentos
    protected bool PermitirVenderConDescuento { get; set; } = false;
    protected decimal? PorcentajeDescuentoMaximo { get; set; }
    
    /// <summary>
    /// Determina si el producto seleccionado permite descuento.
    /// Considera tanto la configuración del sistema como del producto.
    /// </summary>
    protected bool ProductoPermiteDescuento
    {
        get
        {
            if (!PermitirVenderConDescuento) return false;
            if (NuevoDetalle?.IdProducto <= 0) return false;
            var producto = Productos?.FirstOrDefault(p => p.IdProducto == NuevoDetalle.IdProducto);
            return producto?.PermiteDescuento ?? true;
        }
    }
    
    /// <summary>
    /// Retorna el descuento máximo permitido para el producto actual.
    /// Usa la regla configurada (producto, categoría o global).
    /// </summary>
    protected decimal? DescuentoMaximoActual
    {
        get
        {
            if (NuevoDetalle?.IdProducto <= 0) return PorcentajeDescuentoMaximo;
            // Usar el máximo calculado de la regla de descuento (base + margen cajero)
            if (ProductoTieneDescuentoConfigurado && DescuentoMaximoPermitidoProducto > 0)
                return DescuentoMaximoPermitidoProducto;
            return PorcentajeDescuentoMaximo;
        }
    }
    
    // ========== CONFIGURACIÓN FARMACIA ==========
    protected bool ModoFarmaciaActivo { get; set; } = false;
    protected bool MostrarPrecioMinisterio { get; set; } = false;
    protected bool DescuentoBasadoEnPrecioMinisterio { get; set; } = false;

    // ========== INFO DESCUENTO PRODUCTO ACTUAL ==========
    /// <summary>Indica si el producto actual tiene descuento configurado (permite modificar)</summary>
    protected bool ProductoTieneDescuentoConfigurado { get; set; } = false;
    /// <summary>Descuento base configurado para el producto actual</summary>
    protected decimal DescuentoBaseProducto { get; set; } = 0;
    /// <summary>Margen adicional que el cajero puede aplicar</summary>
    protected decimal MargenCajeroProducto { get; set; } = 0;
    /// <summary>Descuento máximo total permitido (base + margen)</summary>
    protected decimal DescuentoMaximoPermitidoProducto { get; set; } = 0;
    /// <summary>Origen de la configuración de descuento</summary>
    protected string OrigenDescuentoProducto { get; set; } = string.Empty;

    // Buscadores (con setter que aplica filtro en cada pulsación, igual que en Compras)
    private string? _buscarCliente;
    protected string BuscarCliente
    {
        get => _buscarCliente ?? string.Empty;
        set { _buscarCliente = value; AplicarFiltroClientes(); }
    }

    private string? _buscarProducto;
    protected string BuscarProducto
    {
        get => _buscarProducto ?? string.Empty;
        set { _buscarProducto = value; AplicarFiltroProductos(); }
    }
    protected string ClienteSeleccionadoLabel { get; set; } = string.Empty;
    protected string? EmailCliente { get; set; }
    protected string? ProductoImagenUrl { get; set; }
    
    // Método para formatear precios según moneda
    protected string FormatearPrecio(decimal valor)
    {
        // Si la venta es en moneda extranjera, mostrar decimales
        if (Cab?.EsMonedaExtranjera == true)
        {
            return valor.ToString("N2"); // 2 decimales para USD, etc.
        }
        else
        {
            return valor.ToString("N0"); // Sin decimales para Guaraníes
        }
    }
    
    // Totales
    protected decimal Gravado10 { get; set; }
    protected decimal Gravado5 { get; set; }
    protected decimal Exenta { get; set; }
    protected decimal IVATotal10 { get; set; }
    protected decimal IVATotal5 { get; set; }
    protected async void OnMonedaChanged()
    {
        // Con @bind, Cab.IdMoneda ya está actualizado
        if (Cab.IdMoneda.HasValue && Monedas.FirstOrDefault(m => m.IdMoneda == Cab.IdMoneda.Value) is { } mon)
        {
            Cab.SimboloMoneda = mon.Simbolo;
            Cab.EsMonedaExtranjera = !mon.EsMonedaBase;
            if (mon.EsMonedaBase)
            {
                Cab.CambioDelDia = 1m;
            }
            else
            {
                // Cargar tipo de cambio del día PARA VENTAS
                // Para ventas usamos TasaCompra (columna COMPRA en tabla = valor más alto)
                // Es el precio al cual el negocio valora los dólares al vender productos
                try
                {
                    using var ctx = await DbFactory.CreateDbContextAsync();
                    var monedaBase = Monedas.FirstOrDefault(m => m.EsMonedaBase);
                    if (monedaBase != null)
                    {
                        // Buscar conversión de moneda extranjera a guaraníes
                        // Por ejemplo: USD → PYG
                        var tc = await ctx.TiposCambio
                            .Where(t => t.IdMonedaOrigen == mon.IdMoneda
                                     && t.IdMonedaDestino == monedaBase.IdMoneda
                                     && t.Estado
                                     && t.FechaTipoCambio.Date == DateTime.Today)
                            .OrderByDescending(t => t.FechaCreacion)
                            .FirstOrDefaultAsync();
                        
                        // Usar TasaCompra (columna COMPRA = 6,850) si existe, sino TasaCambio
                        Cab.CambioDelDia = (tc?.TasaCompra ?? tc?.TasaCambio) ?? 1m;
                        Console.WriteLine($"[OnMonedaChanged] Cargado TC COMPRA para {mon.CodigoISO}: {Cab.CambioDelDia} (TasaCompra={tc?.TasaCompra}, TasaCambio={tc?.TasaCambio})");
                    }
                }
                catch { Cab.CambioDelDia = 1m; }
            }
        }
        StateHasChanged();
    }

    protected void OnCambioManualChanged()
    {
        // Cuando el usuario cambia manualmente el tipo de cambio,
        // recalcular el precio del producto seleccionado si hay uno
        if (NuevoDetalle.IdProducto > 0)
        {
            // Usa el nuevo método que respeta ClientePrecio
            // Nota: Esto es sincrónico, así que solo usa ConvertirPrecioSegunMonedaVenta
            var prod = Productos.FirstOrDefault(p => p.IdProducto == NuevoDetalle.IdProducto);
            if (prod != null)
            {
                NuevoDetalle.PrecioUnitario = ConvertirPrecioSegunMonedaVenta(prod.PrecioUnitarioGs);
                RecalcularNuevoDetalle();
            }
        }

        StateHasChanged();
    }

    protected void OnValidezChanged()
    {
        ActualizarValidez();
        StateHasChanged();
    }

    /// <summary>
    /// Calcula el precio unitario respetando precios diferenciados por cliente (ClientePrecio).
    /// Si existe un ClientePrecio activo para este cliente+producto:
    /// - Si tiene PrecioFijoGs: usa ese precio
    /// - Si tiene PorcentajeDescuento: aplica el descuento sobre el precio base
    /// Si no existe ClientePrecio: usa el precio base del producto
    /// NUEVO: Si está en modo paquete y el producto tiene PrecioPaqueteGs, calcula el precio unitario desde el paquete
    /// </summary>
    private async Task<decimal> CalcularPrecioRespetandoClientePrecioAsync(int idProducto, int? idCliente)
    {
        // Obtener producto
        var prod = Productos.FirstOrDefault(p => p.IdProducto == idProducto);
        if (prod == null) return 0m;

        // ========== SELECCIONAR PRECIO BASE SEGÚN MODO PAQUETE/UNIDAD ==========
        decimal precioBaseEnGs;
        var cantPorPaq = prod.CantidadPorPaquete ?? 1;
        
        // Si está en modo paquete y el producto tiene PrecioPaqueteGs configurado
        if (ModoIngresoVenta == "paquete" && prod.PrecioPaqueteGs.HasValue && prod.PrecioPaqueteGs > 0 && cantPorPaq > 1)
        {
            // PrecioPaqueteGs es el precio total del paquete
            // Para ventas, usamos el precio completo del paquete como base
            // (la cantidad ya se multiplica por cantPorPaq en RecalcularCantidadPorModoVenta)
            precioBaseEnGs = prod.PrecioPaqueteGs.Value / cantPorPaq;
            Console.WriteLine($"[CalcularPrecio] Modo PAQUETE: PrecioPaqueteGs={prod.PrecioPaqueteGs}, cantPorPaq={cantPorPaq}, precioUnitario={precioBaseEnGs}");
        }
        else
        {
            // Modo unidad o producto sin precio de paquete: usar precio unitario normal
            precioBaseEnGs = prod.PrecioUnitarioGs;
            Console.WriteLine($"[CalcularPrecio] Modo UNIDAD: PrecioUnitarioGs={precioBaseEnGs}");
        }

        // Si no hay cliente seleccionado, usar precio base
        if (!idCliente.HasValue || idCliente.Value <= 0)
        {
            return ConvertirPrecioSegunMonedaVenta(precioBaseEnGs);
        }

        // Buscar ClientePrecio activo para este cliente+producto en BD
        decimal precioFinalEnGs = precioBaseEnGs;
        
        try
        {
            using var ctx = await DbFactory.CreateDbContextAsync();
            var clientePrecio = await ctx.ClientesPrecios
                .AsNoTracking()
                .FirstOrDefaultAsync(cp => 
                    cp.IdCliente == idCliente.Value && 
                    cp.IdProducto == idProducto && 
                    cp.Activo);

            if (clientePrecio != null)
            {
                // Opción 1: Precio fijo específico
                if (clientePrecio.PrecioFijoGs.HasValue && clientePrecio.PrecioFijoGs.Value > 0)
                {
                    precioFinalEnGs = clientePrecio.PrecioFijoGs.Value;
                    Console.WriteLine($"[ClientePrecio] Usando PRECIO FIJO: Gs. {precioFinalEnGs} (Cliente={idCliente}, Producto={idProducto})");
                }
                // Opción 2: Descuento porcentual
                else if (clientePrecio.PorcentajeDescuento.HasValue && clientePrecio.PorcentajeDescuento.Value > 0)
                {
                    var descuentoGs = Math.Round(precioBaseEnGs * (clientePrecio.PorcentajeDescuento.Value / 100m), 4);
                    precioFinalEnGs = precioBaseEnGs - descuentoGs;
                    Console.WriteLine($"[ClientePrecio] Aplicando DESCUENTO: {clientePrecio.PorcentajeDescuento}% sobre Gs. {precioBaseEnGs} = Gs. {precioFinalEnGs} (Cliente={idCliente}, Producto={idProducto})");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] CalcularPrecioRespetandoClientePrecio: {ex.Message}");
        }

        // Convertir al precio final según la moneda de venta
        return ConvertirPrecioSegunMonedaVenta(precioFinalEnGs);
    }

    /// <summary>
    /// Convierte un precio en Guaraníes a la moneda de la venta.
    /// Si la venta es en USD: divide por el tipo de cambio
    /// Si la venta es en Guaraníes: retorna el mismo precio
    /// </summary>
    private decimal ConvertirPrecioSegunMonedaVenta(decimal precioEnGs)
    {
        var idMonVenta = Cab.IdMoneda;
        decimal precio = precioEnGs;

        if (idMonVenta.HasValue)
        {
            var monVenta = Monedas.FirstOrDefault(m => m.IdMoneda == idMonVenta.Value);

            if (monVenta?.EsMonedaBase == false)
            {
                // Venta en moneda extranjera (USD): convertir Gs → USD
                var cambioVenta = Cab.CambioDelDia ?? 1m;
                precio = Math.Round(precioEnGs / cambioVenta, 4);
                NuevoDetalle.CambioDelDia = cambioVenta;
            }
        }

        return precio;
    }

    private void ActualizarValidez()
    {
        if (string.Equals(Cab.TipoIngreso, "PRESUPUESTO", StringComparison.OrdinalIgnoreCase) && (ValidezDias ?? 0) > 0)
        {
            ValidoHasta = Cab.Fecha.Date.AddDays(ValidezDias!.Value);
        }
        else
        {
            ValidoHasta = null;
        }
    }

    // UI helpers
    private bool _mouseDownEnSugerenciaProducto;
    private bool _mouseDownEnSugerenciaCliente;

    private static string QuitarDiacriticos(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var norm = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(capacity: norm.Length);
        foreach (var ch in norm)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static bool EsRemisionNombre(string? nombre)
    {
        var sinAcento = QuitarDiacriticos(nombre).ToUpperInvariant();
        return sinAcento.Contains("REMISION");
    }

    private static async Task<Caja?> ObtenerCajaActualAsync(AppDbContext ctx, bool asNoTracking)
    {
        if (asNoTracking)
        {
            var actual = await ctx.Cajas.AsNoTracking().FirstOrDefaultAsync(c => c.CajaActual == 1);
            return actual ?? await ctx.Cajas.AsNoTracking().FirstOrDefaultAsync();
        }
        else
        {
            var actual = await ctx.Cajas.FirstOrDefaultAsync(c => c.CajaActual == 1);
            return actual ?? await ctx.Cajas.FirstOrDefaultAsync();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        var suc = SucursalProvider.GetSucursalId();
        if (!suc.HasValue)
        {
            var ret = System.Net.WebUtility.UrlEncode(Nav.Uri);
            Nav.NavigateTo($"/seleccionar-sucursal?returnUrl={ret}", true);
            return;
        }
        Cab.IdSucursal = suc.Value;
        await using var ctx = await DbFactory.CreateDbContextAsync();
        
        // Verificar permiso EDIT para poder modificar precios
        try
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                // Guardar nombre de usuario para movimientos de inventario
                _usuarioActual = user.Identity.Name ?? "Sistema";
                
                var idClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (idClaim != null && int.TryParse(idClaim.Value, out int idUsu))
                {
                    _idUsuarioActual = idUsu; // Guardar ID del usuario autenticado
                    PuedeEditarPrecio = await PermisosService.TienePermisoAsync(idUsu, "/ventas", "EDIT");
                    // El permiso de cambiar caja y modificar descuento es el mismo que editar precios (EDIT)
                    PuedeCambiarCaja = PuedeEditarPrecio;
                    PuedeModificarDescuento = PuedeEditarPrecio;
                }
            }
        }
        catch { PuedeEditarPrecio = false; PuedeCambiarCaja = false; PuedeModificarDescuento = false; }
        
        // Cargar configuración del sistema (descuentos y farmacia)
        try
        {
            var configSistema = await ctx.ConfiguracionSistema.AsNoTracking().FirstOrDefaultAsync();
            if (configSistema != null)
            {
                PermitirVenderConDescuento = configSistema.PermitirVenderConDescuento;
                PorcentajeDescuentoMaximo = configSistema.PorcentajeDescuentoMaximo;
                ModoFarmaciaActivo = configSistema.FarmaciaModoActivo;
                MostrarPrecioMinisterio = configSistema.FarmaciaModoActivo && configSistema.FarmaciaMostrarPrecioMinisterio;
                DescuentoBasadoEnPrecioMinisterio = configSistema.FarmaciaModoActivo && configSistema.FarmaciaDescuentoBasadoEnPrecioMinisterio;
            }
        }
        catch { /* Si no existe la tabla, usa valores por defecto */ }
        
        // Cargar cajas disponibles de la sucursal
        CajasDisponibles = await ctx.Cajas.AsNoTracking()
            .Where(c => c.IdSucursal == suc.Value || c.IdSucursal == null)
            .OrderBy(c => c.IdCaja)
            .ToListAsync();
        
        // Cargar configuración de la caja actual para determinar el tipo de facturación
        var cajaConfig = await ctx.Cajas.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CajaActual == 1);
        var tipoFacturacion = cajaConfig?.TipoFacturacion ?? "ELECTRONICA";
        _tipoFacturacionCaja = tipoFacturacion; // Guardar para uso posterior (envío SIFEN automático)
        
        Clientes = await ctx.Clientes.AsNoTracking().OrderBy(c => c.RazonSocial).ToListAsync();
        ClientesFiltrados = new(Clientes);
        _tiposDocumentosCliente = await ctx.TiposDocumentosIdentidad.AsNoTracking().ToListAsync();
        Productos = await ctx.Productos.Include(p => p.TipoIva).Include(p => p.MonedaPrecio).AsNoTracking().OrderBy(p => p.Descripcion).ToListAsync();
        
        // Cargar stocks solo del depósito principal (IdDeposito = 1) para ventas
        // El depósito 2 (Almacén) es para transferencias, no para venta directa
        _stocksPorProducto = await ctx.ProductosDepositos
            .Where(pd => pd.IdDeposito == 1) // Solo depósito principal
            .ToDictionaryAsync(pd => pd.IdProducto, pd => pd.Stock);
        
        // FIX 2026-01-19: Para productos con ControlaLote, el stock real es la SUMA de lotes válidos
        // IMPORTANTE: Solo contar lotes que tienen FechaVencimiento definida y no está vencida
        // Lotes sin fecha de vencimiento NO se pueden vender hasta que se cargue la fecha
        var hoy = DateTime.Today;
        
        // Obtener stock de lotes agrupado por producto (solo depósito 1, solo con fecha de vencimiento válida)
        var stocksLotes = await ctx.ProductosLotes
            .Where(l => l.IdDeposito == 1 
                     && l.Estado == "Activo" 
                     && l.Stock > 0
                     && l.FechaVencimiento.HasValue          // DEBE tener fecha de vencimiento
                     && l.FechaVencimiento.Value >= hoy)     // Y no estar vencido
            .GroupBy(l => l.IdProducto)
            .Select(g => new { IdProducto = g.Key, TotalStock = g.Sum(l => l.Stock) })
            .ToDictionaryAsync(x => x.IdProducto, x => x.TotalStock);
        
        // Actualizar el Stock en cada producto con el valor del depósito principal
        foreach (var p in Productos)
        {
            if (p.ControlaLote)
            {
                // Producto con lotes: usar stock de lotes CON FECHA DE VENCIMIENTO válida
                // Lotes sin fecha = stock 0 (bloquea venta hasta cargar fecha)
                p.Stock = stocksLotes.TryGetValue(p.IdProducto, out var stockLotes) ? stockLotes : 0;
            }
            else
            {
                // Producto sin lotes: usar stock general de ProductosDepositos
                p.Stock = _stocksPorProducto.TryGetValue(p.IdProducto, out var stockPrincipal) ? stockPrincipal : 0;
            }
        }
        
        ProductosFiltrados = new(Productos);
        Monedas = await ctx.Monedas.AsNoTracking().Where(m => m.Estado).OrderByDescending(m => m.EsMonedaBase).ThenBy(m => m.Orden).ToListAsync();
        
        // Debug: Verificar tipos de cambio disponibles
        var tcHoy = await ctx.TiposCambio.Where(t => t.Estado && t.FechaTipoCambio.Date == DateTime.Today).ToListAsync();
        Console.WriteLine($"[OnInitializedAsync] Tipos de cambio disponibles para hoy ({DateTime.Today:yyyy-MM-dd}): {tcHoy.Count}");
        foreach (var tc in tcHoy)
        {
            var monOrigen = Monedas.FirstOrDefault(m => m.IdMoneda == tc.IdMonedaOrigen)?.CodigoISO ?? tc.IdMonedaOrigen.ToString();
            var monDestino = Monedas.FirstOrDefault(m => m.IdMoneda == tc.IdMonedaDestino)?.CodigoISO ?? tc.IdMonedaDestino.ToString();
            Console.WriteLine($"  - {monOrigen} → {monDestino}: {tc.TasaCambio:N4} (Fecha: {tc.FechaTipoCambio:yyyy-MM-dd HH:mm})");
        }
        TiposPago = await ctx.TiposPago.AsNoTracking().Where(t => t.Activo).OrderBy(t => t.Orden).ThenBy(t => t.Nombre).ToListAsync();
        TiposDocumento = await ctx.TiposDocumentoOperacion.AsNoTracking().OrderBy(t => t.Orden).ThenBy(t => t.Nombre).ToListAsync();
        
        // Cargar IDs de tipos que son servicios (no controlan stock)
        _tiposServicioIds = (await ctx.TiposItem.AsNoTracking().Where(t => t.EsServicio).Select(t => t.IdTipoItem).ToListAsync()).ToHashSet();
        
        // Filtrar REMISION INTERNA
        TiposDocumento = TiposDocumento.Where(t => !string.Equals(t.Nombre?.Trim(), "REMISION INTERNA", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(QuitarDiacriticos(t.Nombre)?.Trim(), "REMISION INTERNA", StringComparison.OrdinalIgnoreCase)).ToList();
        
        // Si el tipo de facturación es AUTOIMPRESOR, eliminar FACTURA ELECTRONICA de la lista
        if (tipoFacturacion == "AUTOIMPRESOR")
        {
            TiposDocumento = TiposDocumento.Where(t => 
                !string.Equals(t.Nombre?.Trim(), "FACTURA ELECTRONICA", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(QuitarDiacriticos(t.Nombre)?.Trim(), "FACTURA ELECTRONICA", StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
        
        if (Monedas.FirstOrDefault(m => m.EsMonedaBase) is { } baseMon)
        {
            Cab.IdMoneda = baseMon.IdMoneda;
            Cab.SimboloMoneda = baseMon.Simbolo;
            Cab.CambioDelDia = 1m;
            Cab.EsMonedaExtranjera = false;
        }
        // Tipo Documento por defecto: FACTURA si existe
        if (!Cab.IdTipoDocumentoOperacion.HasValue)
        {
            var factura = TiposDocumento.FirstOrDefault(t => (t.Nombre ?? string.Empty).ToUpper().Contains("FACTURA"));
            if (factura != null) { Cab.IdTipoDocumentoOperacion = factura.IdTipoDocumentoOperacion; Cab.TipoDocumento = factura.Nombre; }
        }
        // Tipo de pago por defecto: CONTADO PREDETERMINADO > CONTADO > primero de la lista
        var contadoPred = TiposPago.FirstOrDefault(t => t.Nombre != null && t.Nombre.ToUpper().Contains("CONTADO") && t.Nombre.ToUpper().Contains("PREDETERMINADO"));
        var contado = contadoPred ?? TiposPago.FirstOrDefault(t => t.Nombre != null && t.Nombre.ToUpper().Contains("CONTADO"));
        var primero = contado ?? TiposPago.FirstOrDefault();
        if (primero != null)
        {
            Cab.IdTipoPago = primero.IdTipoPago;
            OnTipoPagoChanged();
        }

        // Si hay parámetro presupuesto, cargar datos del presupuesto
        if (PresupuestoId.HasValue && PresupuestoId.Value > 0)
        {
            await CargarPresupuesto(PresupuestoId.Value);
        }

        // Si hay parámetro id (venta existente), cargar la venta en modo solo vista
        if (VentaId.HasValue && VentaId.Value > 0)
        {
            await CargarVentaExistente(VentaId.Value);
            ModoSoloVista = true;
        }

        SucursalNombreUI = SucursalProvider.GetSucursalNombre();
        await CargarNumeracionesAsync(ctx);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Registrar atajo de teclado F2 para guardar la venta o confirmar composición
            await JS.InvokeVoidAsync("eval", @"
                if (!window._ventasF2Handler) {
                    window._ventasF2Handler = function(e) {
                        if (e.key === 'F2') {
                            e.preventDefault();
                            // Primero intentar confirmar composición si está abierta
                            var btnComposicion = document.querySelector('#btnConfirmarComposicion');
                            if (btnComposicion && btnComposicion.offsetParent !== null) {
                                btnComposicion.click();
                                return;
                            }
                            // Si no hay composición, guardar venta
                            var btnGuardar = document.querySelector('#btnGuardarVenta');
                            if (btnGuardar) btnGuardar.click();
                        }
                        // F3 - Buscar producto
                        else if (e.key === 'F3') {
                            e.preventDefault();
                            var inputProducto = document.querySelector('[placeholder*=""Buscar por nombre""]');
                            if (inputProducto) {
                                inputProducto.focus();
                                inputProducto.select();
                            }
                        }
                        // F4 - Buscar cliente
                        else if (e.key === 'F4') {
                            e.preventDefault();
                            var inputCliente = document.querySelector('[placeholder*=""Buscar por Razón Social""]');
                            if (inputCliente) {
                                inputCliente.focus();
                                inputCliente.select();
                            }
                        }
                    };
                    document.addEventListener('keydown', window._ventasF2Handler);
                }
            ");
        }
    }

    private async Task CargarNumeracionesAsync(AppDbContext ctx)
    {
        var caja = await ObtenerCajaActualAsync(ctx, asNoTracking: true);
        if (caja != null)
        {
            MensajeAdvertencia = null;
            var esRemision = EsRemisionNombre(Cab.TipoDocumento);
            var est = esRemision ? (caja.Nivel1R ?? caja.Nivel1 ?? "001") : (caja.Nivel1 ?? "001");
            var pto = esRemision ? (caja.Nivel2R ?? caja.Nivel2 ?? "001") : (caja.Nivel2 ?? "001");
            var nro = esRemision ? (caja.FacturaInicialR ?? "0000001") : (caja.FacturaInicial ?? "0000001");
            // Asegurar padding a 7 dígitos incluso si en BD está sin ceros a la izquierda
            var nroFmt = int.TryParse(nro, out var nroInt) ? nroInt.ToString("D7") : nro;
            NumeroFacturaUI = $"{est}-{pto}-{nroFmt}";
            EstablecimientoUI = est;
            PuntoExpedicionUI = pto;
            NumeroSecuencialUI = nroFmt;
            TimbradoUI = esRemision ? (caja.TimbradoR ?? caja.Timbrado) : caja.Timbrado;
            TurnoUI = caja.TurnoActual;
            CajaIdUI = caja.IdCaja;
            CajaSeleccionadaId = caja.IdCaja; // Inicializar el selector
            CajaNombreUI = caja.Nombre ?? $"Caja {caja.IdCaja}";
            FechaCajaUI = (caja.FechaActualCaja ?? DateTime.Today).ToString("dd/MM/yyyy");
            Cab.Fecha = caja.FechaActualCaja ?? DateTime.Today;
        }
        string? ult = null;
        try
        {
            ult = await ctx.Presupuestos.AsNoTracking()
                .Where(p => p.IdSucursal == Cab.IdSucursal)
                .OrderByDescending(p => p.IdPresupuesto)
                .Select(p => p.NumeroPresupuesto)
                .FirstOrDefaultAsync();
        }
        catch
        {
            // Fallback para esquemas donde la columna física se llama 'suc' y el modelo aún no recargó el mapeo
            try
            {
                ult = await ctx.Presupuestos
                    .FromSqlInterpolated($"SELECT TOP (1) * FROM Presupuestos WHERE suc = {Cab.IdSucursal} ORDER BY IdPresupuesto DESC")
                    .AsNoTracking()
                    .Select(p => p.NumeroPresupuesto)
                    .FirstOrDefaultAsync();
            }
            catch { /* Ignorar y dejar ult en null */ }
        }
        if (!string.IsNullOrWhiteSpace(ult))
        {
            if (int.TryParse(ult, out var n)) NumeroPresupuestoUI = (n + 1).ToString("D7");
            else NumeroPresupuestoUI = ult;
        }
        else
        {
            NumeroPresupuestoUI = "0000001";
        }
    }

    private async Task OnCajaSeleccionadaChanged()
    {
        if (!CajaSeleccionadaId.HasValue) return;
        
        var caja = CajasDisponibles.FirstOrDefault(c => c.IdCaja == CajaSeleccionadaId.Value);
        if (caja == null) return;
        
        // Actualizar UI
        CajaIdUI = caja.IdCaja;
        CajaNombreUI = caja.Nombre ?? $"Caja {caja.IdCaja}";
        FechaCajaUI = (caja.FechaActualCaja ?? DateTime.Today).ToString("dd/MM/yyyy");
        TurnoUI = caja.TurnoActual;
        Cab.Fecha = caja.FechaActualCaja ?? DateTime.Today;
        
        // Actualizar numeración según el tipo de documento
        var esRemision = EsRemisionNombre(Cab.TipoDocumento);
        var est = esRemision ? (caja.Nivel1R ?? caja.Nivel1 ?? "001") : (caja.Nivel1 ?? "001");
        var pto = esRemision ? (caja.Nivel2R ?? caja.Nivel2 ?? "001") : (caja.Nivel2 ?? "001");
        var nro = esRemision ? (caja.FacturaInicialR ?? "0000001") : (caja.FacturaInicial ?? "0000001");
        var nroFmt = int.TryParse(nro, out var nroInt) ? nroInt.ToString("D7") : nro;
        NumeroFacturaUI = $"{est}-{pto}-{nroFmt}";
        EstablecimientoUI = est;
        PuntoExpedicionUI = pto;
        NumeroSecuencialUI = nroFmt;
        TimbradoUI = esRemision ? (caja.TimbradoR ?? caja.Timbrado) : caja.Timbrado;
        
        StateHasChanged();
    }

    private async Task<bool> AsignarNumeracionVentaAsync(AppDbContext ctx)
    {
        // Obtener la caja seleccionada por el usuario desde el selector o los claims
        var cajaIdSeleccionada = CajaSeleccionadaId ?? CajaProvider.GetCajaId();
        
        Caja? caja = null;
        
        if (cajaIdSeleccionada.HasValue && cajaIdSeleccionada.Value > 0)
        {
            // BLOQUEO PESIMISTA: Leer la caja seleccionada con UPDLOCK
            caja = await ctx.Cajas
                .FromSqlRaw("SELECT * FROM Cajas WITH (UPDLOCK, ROWLOCK) WHERE id_caja = {0}", cajaIdSeleccionada.Value)
                .FirstOrDefaultAsync();
        }
        
        if (caja == null)
        {
            // Fallback: tomar la caja marcada como actual con bloqueo
            caja = await ctx.Cajas
                .FromSqlRaw("SELECT * FROM Cajas WITH (UPDLOCK, ROWLOCK) WHERE CajaActual = 1")
                .FirstOrDefaultAsync();
        }
        
        if (caja == null)
        {
            // Último fallback: tomar la primera caja disponible con bloqueo
            caja = await ctx.Cajas
                .FromSqlRaw("SELECT TOP 1 * FROM Cajas WITH (UPDLOCK, ROWLOCK)")
                .FirstOrDefaultAsync();
        }
        
        if (caja == null)
        {
            MensajeAdvertencia = "No se encontró ninguna caja configurada. Seleccione una caja en el menú de sucursales.";
            return false;
        }
        
        var esRemision = EsRemisionNombre(Cab.TipoDocumento);
        var esNotaCredito = Cab.TipoDocumento?.ToUpperInvariant().Contains("CREDITO") ?? false;
        var esRecibo = Cab.TipoDocumento?.ToUpperInvariant().Contains("RECIBO") ?? false;
        
        string est, pto, numStr; string? timbrado; int? serie;
        DateTime? vigDesde = null, vigHasta = null;
        
        if (esNotaCredito)
        {
            est = caja.Nivel1NC ?? caja.Nivel1 ?? "001";
            pto = caja.Nivel2NC ?? caja.Nivel2 ?? "001";
            numStr = caja.NumeroNC ?? "0000001";
            timbrado = caja.TimbradoNC ?? caja.Timbrado;
            serie = caja.SerieNC ?? caja.Serie;
            vigDesde = caja.VigenciaDelNC ?? caja.VigenciaDel;
            vigHasta = caja.VigenciaAlNC ?? caja.VigenciaAl;
        }
        else if (esRecibo)
        {
            est = caja.Nivel1Recibo ?? caja.Nivel1 ?? "001";
            pto = caja.Nivel2Recibo ?? caja.Nivel2 ?? "001";
            numStr = caja.NumeroRecibo ?? "0000001";
            timbrado = caja.TimbradoRecibo ?? caja.Timbrado;
            serie = caja.SerieRecibo ?? caja.Serie;
            vigDesde = caja.VigenciaDelRecibo ?? caja.VigenciaDel;
            vigHasta = caja.VigenciaAlRecibo ?? caja.VigenciaAl;
        }
        else if (esRemision)
        {
            est = caja.Nivel1R ?? caja.Nivel1 ?? "001";
            pto = caja.Nivel2R ?? caja.Nivel2 ?? "001";
            numStr = caja.FacturaInicialR ?? "0000001";
            timbrado = caja.TimbradoR ?? caja.Timbrado;
            serie = caja.SerieR ?? caja.Serie;
            vigDesde = caja.VigenciaDelR ?? caja.VigenciaDel;
            vigHasta = caja.VigenciaAlR ?? caja.VigenciaAl;
        }
        else
        {
            // Factura normal
            est = caja.Nivel1 ?? "001";
            pto = caja.Nivel2 ?? "001";
            numStr = caja.FacturaInicial ?? "0000001";
            timbrado = caja.Timbrado;
            serie = caja.Serie;
            vigDesde = caja.VigenciaDel;
            vigHasta = caja.VigenciaAl;
        }
        
        if (!int.TryParse(numStr, out var num)) num = 1;
        var fechaCaja = caja.FechaActualCaja ?? DateTime.Today;
        if (string.IsNullOrWhiteSpace(timbrado) || !vigDesde.HasValue || !vigHasta.HasValue || fechaCaja.Date < vigDesde.Value.Date || fechaCaja.Date > vigHasta.Value.Date)
        {
            MensajeAdvertencia = $"El timbrado no es válido para la fecha de caja ({fechaCaja:dd/MM/yyyy}). Vigencia: {vigDesde:dd/MM/yyyy} - {vigHasta:dd/MM/yyyy}.";
            return false;
        }
        
        var numeroFactura = num.ToString("D7");
        
        // Verificación adicional de seguridad (el índice único es la protección final)
        var existeDuplicado = await ctx.Ventas.AnyAsync(v => 
            v.Establecimiento == est && 
            v.PuntoExpedicion == pto && 
            v.NumeroFactura == numeroFactura && 
            v.Timbrado == timbrado && 
            v.Serie == serie);
        
        if (existeDuplicado)
        {
            // Si hay duplicado, intentar obtener el siguiente número disponible
            var ultimoNumero = await ctx.Ventas
                .Where(v => v.Establecimiento == est && v.PuntoExpedicion == pto && v.Timbrado == timbrado && v.Serie == serie)
                .OrderByDescending(v => v.NumeroFactura)
                .Select(v => v.NumeroFactura)
                .FirstOrDefaultAsync();
            
            if (!string.IsNullOrEmpty(ultimoNumero) && int.TryParse(ultimoNumero, out var ultimoNum))
            {
                num = ultimoNum + 1;
                numeroFactura = num.ToString("D7");
                MensajeAdvertencia = $"⚠️ Número ajustado automáticamente a {est}-{pto}-{numeroFactura} por conflicto de concurrencia.";
            }
            else
            {
                MensajeAdvertencia = $"⚠️ Ya existe una venta con la numeración {est}-{pto}-{numeroFactura}. Verifique la configuración de caja.";
                return false;
            }
        }
        
        Cab.Establecimiento = est;
        Cab.PuntoExpedicion = pto;
        Cab.NumeroFactura = numeroFactura;
        Cab.Serie = serie;
        Cab.IdCaja = caja?.IdCaja;
        Cab.Timbrado = timbrado;
        Cab.Turno = caja?.TurnoActual;
        NumeroFacturaUI = $"{est}-{pto}-{Cab.NumeroFactura}";
        EstablecimientoUI = est;
        PuntoExpedicionUI = pto;
        NumeroSecuencialUI = Cab.NumeroFactura;
        TimbradoUI = Cab.Timbrado;
        TurnoUI = Cab.Turno;
        CajaIdUI = caja?.IdCaja;
        CajaNombreUI = caja?.Nombre ?? (caja != null ? $"Caja {caja.IdCaja}" : null);
        FechaCajaUI = (fechaCaja).ToString("dd/MM/yyyy");
        Cab.Fecha = fechaCaja;
        if (caja != null)
        {
            var siguiente = (num + 1).ToString("D7");
            // Actualizar la caja directamente con SQL para evitar problemas de tracking
            string columnaActualizar;
            if (esNotaCredito) columnaActualizar = "NumeroNC";
            else if (esRecibo) columnaActualizar = "NumeroRecibo";
            else if (esRemision) columnaActualizar = "FacturaInicialR";
            else columnaActualizar = "FacturaInicial";
            
            var fechaCajaDb = caja.FechaActualCaja ?? DateTime.Today;
            await ctx.Database.ExecuteSqlRawAsync(
                $"UPDATE Cajas SET {columnaActualizar} = {{0}}, FechaActualCaja = {{1}} WHERE id_caja = {{2}}",
                siguiente, fechaCajaDb, caja.IdCaja);
            
            // Detach la entidad para evitar conflictos en futuras operaciones
            ctx.Entry(caja).State = EntityState.Detached;
        }
    return true;
    }

    protected void AplicarFiltroClientes()
    {
        var texto = (BuscarCliente ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(texto))
        {
            ClientesFiltrados = new(Clientes);
            MostrarSugClientes = false;
            StateHasChanged();
            return;
        }
        var digitos = new string(texto.Where(char.IsDigit).ToArray());
        ClientesFiltrados = Clientes
            .Where(c => (c.RazonSocial ?? string.Empty).ToLowerInvariant().Contains(texto)
                        || (!string.IsNullOrEmpty(digitos) && (c.RUC ?? string.Empty).Contains(digitos)))
            .OrderBy(c => c.RazonSocial)
            .Take(30)
            .ToList();
        MostrarSugClientes = ClientesFiltrados.Any();
        StateHasChanged();
    }

    protected async Task OnClienteKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" || e.Key == "Tab")
        {
            var texto = (BuscarCliente ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                // Mostrar todos los clientes cuando el buscador está vacío
                ClientesFiltrados = new(Clientes);
                MostrarSugClientes = ClientesFiltrados.Any();
                StateHasChanged();
                return;
            }
            
            // Intentar buscar por RUC exacto (sin DV) primero en lista local
            var soloDigitos = new string(texto.Where(char.IsDigit).ToArray());
            if (!string.IsNullOrEmpty(soloDigitos))
            {
                // Buscar coincidencia exacta por RUC (sin DV)
                var clientePorRuc = Clientes.FirstOrDefault(c => 
                    !string.IsNullOrEmpty(c.RUC) && c.RUC.Equals(soloDigitos, StringComparison.OrdinalIgnoreCase));
                
                if (clientePorRuc != null)
                {
                    await OnClienteSuggestionMouseDownAsync(clientePorRuc);
                    _mensajeRucAuto = null;
                    StateHasChanged();
                    return;
                }
                
                // Si parece ser un RUC (>= 5 dígitos), hacer búsqueda automática
                if (soloDigitos.Length >= 5)
                {
                    await BuscarRucAutoClienteAsync(soloDigitos);
                    return;
                }
            }
            
            // Si no hay coincidencia por RUC exacto, seleccionar el primero de la lista filtrada
            var c = ClientesFiltrados.FirstOrDefault();
            if (c != null)
            {
                await OnClienteSuggestionMouseDownAsync(c);
                StateHasChanged();
            }
        }
        else if (e.Key == "Escape")
        {
            MostrarSugClientes = false;
            _mensajeRucAuto = null;
            StateHasChanged();
        }
    }
    
    /// <summary>
    /// Búsqueda automática de RUC: Cliente local → RucDnit → SIFEN
    /// Si encuentra, registra automáticamente; si no, abre modal de creación
    /// </summary>
    private async Task BuscarRucAutoClienteAsync(string ruc)
    {
        _buscandoRucAuto = true;
        _mensajeRucAuto = "🔍 Buscando RUC...";
        _esRucAutoError = false;
        MostrarSugClientes = false;
        StateHasChanged();

        try
        {
            var resultado = await RucService.BuscarRucUnificadoClienteAsync(ruc);
            _resultadoRucAuto = resultado;

            if (resultado.Encontrado)
            {
                if (resultado.YaRegistrado && resultado.IdExistente.HasValue)
                {
                    // Ya existe en la BD - seleccionarlo directamente
                    var clienteExistente = Clientes.FirstOrDefault(c => c.IdCliente == resultado.IdExistente.Value);
                    if (clienteExistente != null)
                    {
                        await OnClienteSuggestionMouseDownAsync(clienteExistente);
                        _mensajeRucAuto = $"✓ {resultado.Mensaje}";
                        _esRucAutoError = false;
                    }
                }
                else
                {
                    // Encontrado pero no registrado - registrar automáticamente
                    _mensajeRucAuto = $"📝 {resultado.Mensaje} - Registrando...";
                    StateHasChanged();
                    
                    var nuevoCliente = await RegistrarClienteDesdeRucAsync(resultado);
                    if (nuevoCliente != null)
                    {
                        Clientes.Add(nuevoCliente);
                        await OnClienteSuggestionMouseDownAsync(nuevoCliente);
                        _mensajeRucAuto = $"✓ Cliente registrado: {nuevoCliente.RazonSocial}";
                        _esRucAutoError = false;
                    }
                    else
                    {
                        _mensajeRucAuto = "⚠️ Error al registrar cliente";
                        _esRucAutoError = true;
                    }
                }
            }
            else
            {
                // No encontrado - abrir modal de creación rápida con datos prellenados
                _mensajeRucAuto = resultado.Mensaje;
                _esRucAutoError = true;
                
                // Precargar datos en el modal
                _nuevoCliRuc = ruc;
                _nuevoCliDv = resultado.DV;
                _nuevoCliRazonSocial = string.Empty;
                AbrirModalNuevoCliente();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR BuscarRucAutoClienteAsync] {ex.Message}");
            _mensajeRucAuto = "⚠️ Error de conexión";
            _esRucAutoError = true;
        }
        finally
        {
            _buscandoRucAuto = false;
            StateHasChanged();
            
            // Limpiar mensaje después de 4 segundos
            _ = Task.Run(async () =>
            {
                await Task.Delay(4000);
                await InvokeAsync(() =>
                {
                    if (!_esRucAutoError) _mensajeRucAuto = null;
                    StateHasChanged();
                });
            });
        }
    }
    
    /// <summary>
    /// Registra un cliente automáticamente desde el resultado de búsqueda
    /// </summary>
    private async Task<Cliente?> RegistrarClienteDesdeRucAsync(RucBusquedaResultado resultado)
    {
        try
        {
            await using var ctx = await DbFactory.CreateDbContextAsync();
            
            // Verificar que no exista el RUC (evitar duplicados)
            var rucSinGuion = (resultado.RUC ?? "").Replace("-", "").Trim();
            var existente = await ctx.Clientes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.RUC == rucSinGuion || c.RUC == resultado.RUC);
            
            if (existente != null)
            {
                Console.WriteLine($"[INFO] Cliente con RUC {resultado.RUC} ya existe (ID: {existente.IdCliente})");
                return existente; // Retornar el existente en lugar de crear duplicado
            }
            
            // Generar código de cliente automático
            var maxCodigo = await ctx.Clientes
                .Where(c => c.CodigoCliente != null)
                .Select(c => c.CodigoCliente)
                .OrderByDescending(c => c)
                .FirstOrDefaultAsync();
            
            int siguienteCodigo = 1;
            if (!string.IsNullOrEmpty(maxCodigo) && int.TryParse(maxCodigo, out var ultimoCodigo))
            {
                siguienteCodigo = ultimoCodigo + 1;
            }
            
            var nuevoCliente = new Cliente
            {
                CodigoCliente = siguienteCodigo.ToString().PadLeft(6, '0'),
                RazonSocial = resultado.RazonSocial ?? "SIN NOMBRE",
                RUC = rucSinGuion,
                DV = resultado.DV,
                TipoDocumento = "RU", // RU = RUC (catálogo TiposDocumentosIdentidad)
                NumeroDocumento = rucSinGuion,
                Saldo = 0,
                Estado = true,
                IdTipoContribuyente = 2, // 2 = Persona Jurídica (típico para RUC empresarial)
                NaturalezaReceptor = 1, // 1 = Contribuyente
                // TipoOperacion según RUC: >= 50M = B2B, < 50M = B2C
                TipoOperacion = (long.TryParse(rucSinGuion, out var rucNum) && rucNum >= 50_000_000) ? "1" : "2",
                PermiteCredito = false,
                FechaAlta = DateTime.Now,
                CodigoPais = "PRY",
                EsExtranjero = false,
                IdCiudad = 1 // Asunción por defecto
            };
            
            ctx.Clientes.Add(nuevoCliente);
            await ctx.SaveChangesAsync();
            
            // Refrescar lista de clientes (inline)
            Clientes = await ctx.Clientes.AsNoTracking().OrderBy(c => c.RazonSocial).ToListAsync();
            
            return nuevoCliente;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR RegistrarClienteDesdeRucAsync] {ex.Message}");
            return null;
        }
    }

    protected void OnClienteFocus(FocusEventArgs _)
    {
        MostrarSugClientes = !string.IsNullOrWhiteSpace(BuscarCliente) && ClientesFiltrados.Any();
    }
    protected async Task OnClienteBlur(FocusEventArgs _)
    {
        await Task.Delay(120);
        if (!_mouseDownEnSugerenciaCliente)
        {
            // Si hay texto que parece RUC y no hay cliente seleccionado, buscar automáticamente
            var texto = (BuscarCliente ?? string.Empty).Trim();
            var soloDigitos = new string(texto.Where(char.IsDigit).ToArray());
            
            if (!Cab.IdCliente.HasValue && soloDigitos.Length >= 5 && !_buscandoRucAuto)
            {
                // Verificar si el RUC ya existe en la lista local
                var clienteExistente = Clientes.FirstOrDefault(c => 
                    !string.IsNullOrEmpty(c.RUC) && c.RUC.Equals(soloDigitos, StringComparison.OrdinalIgnoreCase));
                
                if (clienteExistente != null)
                {
                    await OnClienteSuggestionMouseDownAsync(clienteExistente);
                }
                else
                {
                    // Buscar en RucDnit y SIFEN
                    await BuscarRucAutoClienteAsync(soloDigitos);
                }
            }
            
            MostrarSugClientes = false; 
            StateHasChanged();
        }
    }
    protected async Task OnClienteSuggestionMouseDownAsync(Cliente c)
    {
        _mouseDownEnSugerenciaCliente = true;
        Cab.IdCliente = c.IdCliente;
        ClienteSeleccionadoLabel = $"{c.RazonSocial} (RUC {c.RUC}-{c.DV})";
        EmailCliente = c.Email;
        BuscarCliente = string.Empty;
        MostrarSugClientes = false;
        
        // Validar habilitación de crédito si el tipo de pago es crédito
        if (TipoPagoEsCredito && !c.PermiteCredito)
        {
            MensajeAdvertencia = $"⚠️ El cliente '{c.RazonSocial}' no está habilitado para venta a crédito. Por favor seleccione otro cliente o cambie a pago contado.";
        }
        else
        {
            MensajeAdvertencia = null;
        }
        
        _ = Task.Run(async () => { await Task.Delay(150); _mouseDownEnSugerenciaCliente = false; });
        await Task.CompletedTask;
    }

    // Se elimina la lógica de condición (CONTADO/CREDITO) y de vencimientos.

    protected void CambiarCliente()
    {
        Cab.IdCliente = null;
        ClienteSeleccionadoLabel = string.Empty;
        EmailCliente = null;
        StateHasChanged();
    }

    protected void OnCantidadChanged()
    {
        RecalcularNuevoDetalle();
        StateHasChanged();
    }

    protected void OnPrecioChanged()
    {
        RecalcularNuevoDetalle();
        StateHasChanged();
    }

    // Se elimina Plazo/Vencimiento; se usa Validez para Presupuesto.

    protected void AplicarFiltroProductos()
    {
        var texto = (BuscarProducto ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(texto)) { ProductosFiltrados = new(Productos); MostrarSugProductos = false; StateHasChanged(); return; }
        // Incluir código interno y código de barras
        ProductosFiltrados = Productos.Where(p =>
            (p.Descripcion ?? string.Empty).ToLowerInvariant().Contains(texto)
            || (p.CodigoInterno ?? string.Empty).ToLowerInvariant().Contains(texto)
            || (p.CodigoBarras ?? string.Empty).ToLowerInvariant().Contains(texto)
        ).OrderBy(p => p.Descripcion).ToList();
        MostrarSugProductos = ProductosFiltrados.Any();
        StateHasChanged();
    }

    protected async Task OnProductoKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            var texto = (BuscarProducto ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                ProductosFiltrados = new(Productos); MostrarSugProductos = ProductosFiltrados.Any(); StateHasChanged(); return;
            }
            
            // Buscar coincidencia exacta por código de barras primero
            var productoExacto = Productos.FirstOrDefault(p => 
                !string.IsNullOrEmpty(p.CodigoBarras) && 
                p.CodigoBarras.Equals(texto, StringComparison.OrdinalIgnoreCase));
            
            if (productoExacto != null)
            {
                // Es un código de barras escaneado - agregar directamente
                await AgregarProductoEscaneadoAsync(productoExacto);
                return;
            }
            
            // Si no es código de barras exacto, usar el primer resultado filtrado
            var p = ProductosFiltrados.FirstOrDefault();
            if (p != null) await SeleccionarProductoAsync(p);
        }
        else if (e.Key == "Escape")
        {
            MostrarSugProductos = false;
        }
    }
    
    /// <summary>
    /// Agrega un producto escaneado por código de barras directamente al detalle con cantidad 1
    /// </summary>
    private async Task AgregarProductoEscaneadoAsync(Producto p)
    {
        // Verificar si el producto está vencido
        if (p.ControlarVencimiento && p.FechaVencimiento.HasValue && p.FechaVencimiento.Value.Date < DateTime.Today)
        {
            _productoVencidoPendiente = p;
            _mostrarAdvertenciaVencido = true;
            BuscarProducto = string.Empty;
            MostrarSugProductos = false;
            StateHasChanged();
            return;
        }
        
        // Verificar stock (solo para productos físicos, no servicios)
        bool esServicio = _tiposServicioIds.Contains(p.TipoItem);
        if (p.Stock <= 0 && !esServicio)
        {
            _productoStockCeroPendiente = p;
            _mostrarConfirmStockCero = true;
            BuscarProducto = string.Empty;
            MostrarSugProductos = false;
            StateHasChanged();
            return;
        }
        
        // Verificar si es producto controlado con receta
        if (p.ControladoReceta)
        {
            var yaRegistrado = _recetasPendientes.Any(r => r.IdProducto == p.IdProducto);
            if (!yaRegistrado)
            {
                _productoRecetaPendiente = p;
                _recetaNumeroRegistro = string.Empty;
                _recetaFecha = DateTime.Today;
                _recetaNombreMedico = string.Empty;
                _recetaNombrePaciente = string.Empty;
                _recetaError = null;
                _mostrarModalReceta = true;
                // Guardar el producto para agregarlo después de completar la receta
                NuevoDetalle.IdProducto = p.IdProducto;
                NuevoDetalle.Cantidad = 1;
                NuevoDetalle.PrecioUnitario = await CalcularPrecioRespetandoClientePrecioAsync(p.IdProducto, Cab.IdCliente);
                RecalcularNuevoDetalle();
                BuscarProducto = string.Empty;
                MostrarSugProductos = false;
                StateHasChanged();
                return;
            }
        }
        
        // Agregar directamente al detalle con cantidad 1
        await AgregarProductoAlDetalleDirectoAsync(p);
    }
    
    /// <summary>
    /// Agrega un producto directamente al detalle de venta con cantidad 1
    /// </summary>
    private async Task AgregarProductoAlDetalleDirectoAsync(Producto p)
    {
        // Verificar si el producto ya está en la lista EN MODO UNIDAD - si existe, sumar cantidad
        // (el click rápido siempre es modo unidad, no agrupar con líneas de paquete)
        var detalleExistente = Detalles.FirstOrDefault(d => 
            d.IdProducto == p.IdProducto && 
            d.ModoIngresoPersistido == "unidad");
        if (detalleExistente != null)
        {
            // Sumar 1 a la cantidad existente y recalcular
            detalleExistente.Cantidad += 1;
            var ivaPorc = p.TipoIva?.Porcentaje ?? 0m;
            var importe = Math.Round(detalleExistente.Cantidad * detalleExistente.PrecioUnitario, 4);
            detalleExistente.Importe = importe;
            detalleExistente.IVA10 = 0; detalleExistente.IVA5 = 0; detalleExistente.Exenta = 0;
            detalleExistente.Grabado10 = 0; detalleExistente.Grabado5 = 0;
            if (ivaPorc >= 9.9m && ivaPorc <= 10.1m)
            {
                var grab = Math.Round(importe / 1.1m, 4);
                detalleExistente.Grabado10 = grab;
                detalleExistente.IVA10 = Math.Round(importe - grab, 4);
            }
            else if (ivaPorc >= 4.9m && ivaPorc <= 5.1m)
            {
                var grab = Math.Round(importe / 1.05m, 4);
                detalleExistente.Grabado5 = grab;
                detalleExistente.IVA5 = Math.Round(importe - grab, 4);
            }
            else
            {
                detalleExistente.Exenta = importe;
            }
            RecalcularTotales();
        }
        else
        {
            // Crear nuevo detalle
            var precio = await CalcularPrecioRespetandoClientePrecioAsync(p.IdProducto, Cab.IdCliente);
            var ivaPorc = p.TipoIva?.Porcentaje ?? 0m;
            var importe = Math.Round(1m * precio, 4);
            decimal iva10 = 0, iva5 = 0, exenta = 0, grab10 = 0, grab5 = 0;
            
            if (ivaPorc >= 9.9m && ivaPorc <= 10.1m)
            {
                grab10 = Math.Round(importe / 1.1m, 4);
                iva10 = Math.Round(importe - grab10, 4);
            }
            else if (ivaPorc >= 4.9m && ivaPorc <= 5.1m)
            {
                grab5 = Math.Round(importe / 1.05m, 4);
                iva5 = Math.Round(importe - grab5, 4);
            }
            else
            {
                exenta = importe;
            }
            
            var det = new VentaDetalle
            {
                IdProducto = p.IdProducto,
                IdTipoIva = p.IdTipoIva,
                Cantidad = 1,
                PrecioUnitario = precio,
                Importe = importe,
                IVA10 = iva10,
                IVA5 = iva5,
                Exenta = exenta,
                Grabado10 = grab10,
                Grabado5 = grab5,
                CambioDelDia = Cab.CambioDelDia,
                // Costo al momento de la venta (para informes)
                CostoUnitario = p.CostoUnitarioGs,
                // Precio Ministerio para modo farmacia
                PrecioMinisterio = ModoFarmaciaActivo && p.PrecioMinisterio.HasValue ? p.PrecioMinisterio : null,
                // ========== PERSISTIR INFO DE PAQUETE (en click rápido siempre es unidad) ==========
                ModoIngresoPersistido = "unidad",
                CantidadPorPaqueteMomento = p.CantidadPorPaquete
            };
            Detalles.Add(det);
            RecalcularTotales();
        }
        
        // Limpiar buscador
        BuscarProducto = string.Empty;
        MostrarSugProductos = false;
        NuevoDetalle = new VentaDetalle { Cantidad = 1, PrecioUnitario = 0 };
        StateHasChanged();
    }
    
    /// <summary>
    /// Cierra el modal de advertencia de producto vencido
    /// </summary>
    private void CerrarAdvertenciaVencido()
    {
        _mostrarAdvertenciaVencido = false;
        _productoVencidoPendiente = null;
    }

    protected void OnProductoFocus(FocusEventArgs _)
    {
        MostrarSugProductos = !string.IsNullOrWhiteSpace(BuscarProducto) && ProductosFiltrados.Any();
    }
    protected async Task OnProductoBlur(FocusEventArgs _)
    {
        await Task.Delay(120);
        if (!_mouseDownEnSugerenciaProducto)
        {
            MostrarSugProductos = false; StateHasChanged();
        }
    }
    protected async Task OnProductoSuggestionMouseDownAsync(Producto p)
    {
        _mouseDownEnSugerenciaProducto = true;
        
        // Verificar si el producto está vencido
        if (p.ControlarVencimiento && p.FechaVencimiento.HasValue && p.FechaVencimiento.Value.Date < DateTime.Today)
        {
            _productoVencidoPendiente = p;
            _mostrarAdvertenciaVencido = true;
            StateHasChanged();
            _ = Task.Run(async () => { await Task.Delay(150); _mouseDownEnSugerenciaProducto = false; });
            return;
        }
        
        // Verificar stock cero SOLO si NO es un servicio
        bool esServicio = _tiposServicioIds.Contains(p.TipoItem);
        if (p.Stock <= 0 && !esServicio)
        {
            // Producto físico sin stock - mostrar error y NO permitir agregar
            _productoStockCeroPendiente = p;
            _mostrarConfirmStockCero = true;
            StateHasChanged();
            _ = Task.Run(async () => { await Task.Delay(150); _mouseDownEnSugerenciaProducto = false; });
            return;
        }
        
        await SeleccionarProductoAsync(p);
        _ = Task.Run(async () => { await Task.Delay(150); _mouseDownEnSugerenciaProducto = false; });
    }
    
    private async Task ConfirmarSeleccionStockCero()
    {
        if (_productoStockCeroPendiente != null)
        {
            await SeleccionarProductoAsync(_productoStockCeroPendiente);
        }
        _mostrarConfirmStockCero = false;
        _productoStockCeroPendiente = null;
    }
    
    private void CancelarSeleccionStockCero()
    {
        _mostrarConfirmStockCero = false;
        _productoStockCeroPendiente = null;
        BuscarProducto = string.Empty;
        MostrarSugProductos = false;
    }

    // ═══════════════════ VISTA PREVIA DEL TICKET ═══════════════════
    
    /// <summary>
    /// Cierra el modal de vista previa del ticket
    /// </summary>
    private void CerrarVistaPrevia()
    {
        _mostrarVistaPrevia = false;
        _idVentaParaVistaPrevia = 0;
    }

    /// <summary>
    /// Se ejecuta cuando se completa la impresión desde la vista previa
    /// </summary>
    private void OnImprimirCompletado()
    {
        Console.WriteLine($"[Ventas] Impresión completada para venta: {_idVentaParaVistaPrevia}");
        // Opcionalmente cerrar el modal automáticamente
        // _mostrarVistaPrevia = false;
    }

    private async Task SeleccionarProductoAsync(Producto p)
    {
        NuevoDetalle.IdProducto = p.IdProducto;
        // Mostrar el producto seleccionado en el buscador (mantener lista según blur)
        BuscarProducto = p.Descripcion ?? string.Empty;
        
        // Cargar imagen del producto
        ProductoImagenUrl = p.Foto;
        
        // Verificar si el producto permite venta decimal
        ProductoPermiteDecimal = p.PermiteDecimal;
        
        // ========== CARGAR INFO DE PAQUETE/CAJA ==========
        _cantidadPorPaqueteActual = p.CantidadPorPaquete;
        _productoPermiteVentaPorUnidad = p.PermiteVentaPorUnidad;
        _nombreUnidadVenta = p.UnidadMedidaCodigo == "006" ? "Paquete" : (p.UnidadMedidaCodigo == "005" ? "Caja" : "Unidad");
        
        // Si es paquete/caja con cantidad configurada, iniciar en modo paquete
        var esPaqueteOCaja = (p.UnidadMedidaCodigo == "006" || p.UnidadMedidaCodigo == "005") && (_cantidadPorPaqueteActual ?? 0) > 1;
        if (esPaqueteOCaja)
        {
            ModoIngresoVenta = "paquete";
            _cantidadIngresadaVenta = 1;
            NuevoDetalle.Cantidad = _cantidadPorPaqueteActual ?? 1;
        }
        else
        {
            ModoIngresoVenta = "unidad";
            _cantidadIngresadaVenta = 1;
            NuevoDetalle.Cantidad = 1;
        }
        
        // Calcular el precio respetando ClientePrecio
        NuevoDetalle.PrecioUnitario = await CalcularPrecioRespetandoClientePrecioAsync(p.IdProducto, Cab.IdCliente);
        
        // Guardar precio original para validaciones de descuento
        PrecioOriginalSinDescuento = NuevoDetalle.PrecioUnitario;
        NuevoDetalleDescuentoPorcentaje = 0; // Reset descuento al seleccionar nuevo producto
        ErrorDescuento = null;
        
        // ========== CARGAR INFO DE DESCUENTO DEL PRODUCTO ==========
        // Obtener información completa de descuento configurado
        var infoDescuento = await DescuentoService.ObtenerInfoDescuentoCompletoAsync(p);
        ProductoTieneDescuentoConfigurado = infoDescuento.TieneDescuento;
        DescuentoBaseProducto = infoDescuento.DescuentoBase;
        MargenCajeroProducto = infoDescuento.MargenCajero;
        DescuentoMaximoPermitidoProducto = infoDescuento.MaximoTotal;
        OrigenDescuentoProducto = infoDescuento.Origen;
        
        Console.WriteLine($"[SeleccionarProducto] Info descuento: TieneDesc={ProductoTieneDescuentoConfigurado}, Base={DescuentoBaseProducto}%, Margen={MargenCajeroProducto}%, Max={DescuentoMaximoPermitidoProducto}%, Origen={OrigenDescuentoProducto}");
        
        // ========== APLICAR DESCUENTO AUTOMÁTICO ==========
        // PRIORIDAD 1: Si tiene regla configurada en "Precios y Descuentos" → usar esa regla
        if (PermitirVenderConDescuento && p.PermiteDescuento && ProductoTieneDescuentoConfigurado)
        {
            NuevoDetalleDescuentoPorcentaje = DescuentoBaseProducto;
            
            if (NuevoDetalleDescuentoPorcentaje > 0)
            {
                // Aplicar el descuento al precio
                var descuentoMultiplier = 1m - (NuevoDetalleDescuentoPorcentaje / 100m);
                NuevoDetalle.PrecioUnitario = Math.Round(PrecioOriginalSinDescuento * descuentoMultiplier, 2);
                NuevoDetalle.PorcentajeDescuento = NuevoDetalleDescuentoPorcentaje;
                
                Console.WriteLine($"[SeleccionarProducto] PRIORIDAD 1 - Descuento por REGLA: {DescuentoBaseProducto}% (Producto: {p.Descripcion})");
            }
        }
        // PRIORIDAD 2: Si NO tiene regla, pero está activo "Calcular descuento basado en Precio Ministerio" 
        // Y el producto tiene Precio Ministerio → calcular automáticamente
        else if (DescuentoBasadoEnPrecioMinisterio && p.PrecioMinisterio.HasValue && p.PrecioMinisterio.Value > 0)
        {
            // Fórmula: ((PrecioMinisterio - PrecioVenta) / PrecioMinisterio) * 100
            var precioMinisterio = p.PrecioMinisterio.Value;
            var precioVenta = NuevoDetalle.PrecioUnitario;
            
            if (precioVenta < precioMinisterio)
            {
                var descuentoCalculado = Math.Round(((precioMinisterio - precioVenta) / precioMinisterio) * 100, 2);
                NuevoDetalleDescuentoPorcentaje = descuentoCalculado;
                NuevoDetalle.PorcentajeDescuento = NuevoDetalleDescuentoPorcentaje;
                
                Console.WriteLine($"[SeleccionarProducto] PRIORIDAD 2 - Descuento MINISTERIO calculado: {NuevoDetalleDescuentoPorcentaje}% (Ministerio: {precioMinisterio}, Venta: {precioVenta})");
            }
        }
        // SIN DESCUENTO: No tiene regla Y (no está activo cálculo Ministerio O no tiene Precio Ministerio)
        
        // Guardar precio ministerio si aplica (modo farmacia)
        if (ModoFarmaciaActivo)
        {
            var cantPorPaq = p.CantidadPorPaquete ?? 1;
            // Si está en modo paquete y tiene precio ministerio de paquete configurado
            if (ModoIngresoVenta == "paquete" && p.PrecioMinisterioPaquete.HasValue && p.PrecioMinisterioPaquete > 0 && cantPorPaq > 1)
            {
                // Precio ministerio por unidad dentro del paquete
                NuevoDetalle.PrecioMinisterio = p.PrecioMinisterioPaquete.Value / cantPorPaq;
            }
            else if (p.PrecioMinisterio.HasValue)
            {
                NuevoDetalle.PrecioMinisterio = p.PrecioMinisterio;
            }
            else
            {
                NuevoDetalle.PrecioMinisterio = null;
            }
        }
        else
        {
            NuevoDetalle.PrecioMinisterio = null;
        }
        
        Console.WriteLine($"[SeleccionarProducto] Producto: {p.Descripcion}, Precio final: {NuevoDetalle.PrecioUnitario}, Modo: {ModoIngresoVenta}");
        RecalcularNuevoDetalle();
    }

    private async Task AgregarDetalleAsync()
    {
        if (NuevoDetalle.IdProducto <= 0 || NuevoDetalle.Cantidad <= 0) return;
        
        // Verificar si el producto es controlado con receta
        var productoActual = Productos.FirstOrDefault(x => x.IdProducto == NuevoDetalle.IdProducto);
        if (productoActual != null && productoActual.ControladoReceta)
        {
            // Verificar si ya tiene receta registrada para este producto
            var yaRegistrado = _recetasPendientes.Any(r => r.IdProducto == NuevoDetalle.IdProducto);
            if (!yaRegistrado)
            {
                // Mostrar modal para capturar datos de receta
                _productoRecetaPendiente = productoActual;
                _recetaNumeroRegistro = string.Empty;
                _recetaFecha = DateTime.Today;
                _recetaNombreMedico = string.Empty;
                _recetaNombrePaciente = string.Empty;
                _recetaError = null;
                _mostrarModalReceta = true;
                return; // No agregar hasta que complete el modal
            }
        }
        
        await AgregarDetalleInternoAsync();
    }
    
    private async Task AgregarDetalleInternoAsync()
    {
        if (NuevoDetalle.IdProducto <= 0 || NuevoDetalle.Cantidad <= 0) return;
        
        // Obtener producto para validar si permite decimal
        var productoValidar = Productos.FirstOrDefault(x => x.IdProducto == NuevoDetalle.IdProducto);
        if (productoValidar != null && !productoValidar.PermiteDecimal)
        {
            // Si no permite decimal, redondear a entero (hacia arriba)
            NuevoDetalle.Cantidad = Math.Ceiling(NuevoDetalle.Cantidad);
        }
        
        // Verificar si el producto ya está en la lista CON EL MISMO MODO DE INGRESO
        // Si el modo es diferente (paquete vs unidad), crear línea separada
        var detalleExistente = Detalles.FirstOrDefault(d => 
            d.IdProducto == NuevoDetalle.IdProducto && 
            d.ModoIngresoPersistido == ModoIngresoVenta);
        if (detalleExistente != null)
        {
            // Sumar cantidad y recalcular (mismo producto Y mismo modo)
            detalleExistente.Cantidad += NuevoDetalle.Cantidad;
            var prod = Productos.FirstOrDefault(x => x.IdProducto == detalleExistente.IdProducto);
            var ivaPorc = prod?.TipoIva?.Porcentaje ?? 0m;
            var importe = Math.Round(detalleExistente.Cantidad * detalleExistente.PrecioUnitario, 4);
            detalleExistente.Importe = importe;
            detalleExistente.IVA10 = 0; detalleExistente.IVA5 = 0; detalleExistente.Exenta = 0;
            detalleExistente.Grabado10 = 0; detalleExistente.Grabado5 = 0;
            if (ivaPorc >= 9.9m && ivaPorc <= 10.1m)
            {
                var grab = Math.Round(importe / 1.1m, 4);
                detalleExistente.Grabado10 = grab;
                detalleExistente.IVA10 = Math.Round(importe - grab, 4);
            }
            else if (ivaPorc >= 4.9m && ivaPorc <= 5.1m)
            {
                var grab = Math.Round(importe / 1.05m, 4);
                detalleExistente.Grabado5 = grab;
                detalleExistente.IVA5 = Math.Round(importe - grab, 4);
            }
            else
            {
                detalleExistente.Exenta = importe;
            }
            
            // ========== ACTUALIZAR LOTE FEFO SI EL PRODUCTO CONTROLA LOTE ==========
            if (prod != null && prod.ControlaLote)
            {
                try
                {
                    decimal cantidadUnidades = detalleExistente.Cantidad;
                    if (detalleExistente.ModoIngresoPersistido == "paquete" && (prod.CantidadPorPaquete ?? 1) > 1)
                    {
                        cantidadUnidades = detalleExistente.Cantidad * (prod.CantidadPorPaquete ?? 1);
                    }
                    var loteFEFO = await LoteService.ObtenerLoteFEFOAsync(prod.IdProducto, 1, cantidadUnidades);
                    if (loteFEFO != null)
                    {
                        detalleExistente.IdProductoLote = loteFEFO.IdProductoLote;
                        detalleExistente.NumeroLoteMomento = loteFEFO.NumeroLote;
                        detalleExistente.FechaVencimientoLoteMomento = loteFEFO.FechaVencimiento;
                    }
                }
                catch { /* Ignorar errores en preview */ }
            }
            
            RecalcularTotales();
            NuevoDetalle = new VentaDetalle { Cantidad = 1, PrecioUnitario = 0 };
            BuscarProducto = string.Empty; MostrarSugProductos = false;
            await Task.CompletedTask;
            return;
        }
        
        var det = new VentaDetalle
        {
            IdProducto = NuevoDetalle.IdProducto,
            Cantidad = NuevoDetalle.Cantidad,
            PrecioUnitario = NuevoDetalle.PrecioUnitario,
            Importe = NuevoDetalle.Importe,
            IVA10 = NuevoDetalle.IVA10,
            IVA5 = NuevoDetalle.IVA5,
            Exenta = NuevoDetalle.Exenta,
            Grabado10 = NuevoDetalle.Grabado10,
            Grabado5 = NuevoDetalle.Grabado5,
            CambioDelDia = NuevoDetalle.CambioDelDia,
            // Descuento y Farmacia
            PorcentajeDescuento = NuevoDetalle.PorcentajeDescuento,
            PrecioMinisterio = NuevoDetalle.PrecioMinisterio,
            // Modo de ingreso (paquete/unidad) - NotMapped para UI
            ModoIngreso = ModoIngresoVenta
        };
        // Completar Tipo de IVA del detalle desde el producto para evitar advertencia al enviar a SIFEN
        var prodFuente = Productos.FirstOrDefault(x => x.IdProducto == det.IdProducto);
        if (prodFuente != null)
        {
            det.IdTipoIva = prodFuente.IdTipoIva;
            // Guardar costo al momento de la venta para informes
            det.CostoUnitario = prodFuente.CostoUnitarioGs;
            
            // ========== CONSULTAR LOTE FEFO PARA PRODUCTOS CON CONTROL DE LOTE ==========
            if (prodFuente.ControlaLote)
            {
                try
                {
                    // Calcular cantidad total en unidades para consultar lote
                    decimal cantidadUnidades = det.Cantidad;
                    if (ModoIngresoVenta == "paquete" && (prodFuente.CantidadPorPaquete ?? 1) > 1)
                    {
                        cantidadUnidades = det.Cantidad * (prodFuente.CantidadPorPaquete ?? 1);
                    }
                    
                    // Consultar lote FEFO (depósito 1 = principal)
                    var loteFEFO = await LoteService.ObtenerLoteFEFOAsync(prodFuente.IdProducto, 1, cantidadUnidades);
                    if (loteFEFO != null)
                    {
                        det.IdProductoLote = loteFEFO.IdProductoLote;
                        det.NumeroLoteMomento = loteFEFO.NumeroLote;
                        det.FechaVencimientoLoteMomento = loteFEFO.FechaVencimiento;
                        Console.WriteLine($"[FEFO-Preview] Producto {prodFuente.Descripcion}: Lote {loteFEFO.NumeroLote} (vence: {loteFEFO.FechaVencimiento:dd/MM/yyyy})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FEFO-Preview] Error al consultar lote para {prodFuente.Descripcion}: {ex.Message}");
                }
            }
            
            // ========== PERSISTIR MODO DE INGRESO Y PRECIOS POR PAQUETE ==========
            det.ModoIngresoPersistido = ModoIngresoVenta;
            det.CantidadPorPaqueteMomento = prodFuente.CantidadPorPaquete;
            if (ModoIngresoVenta == "paquete" && prodFuente.CantidadPorPaquete > 1)
            {
                // Guardar precio por paquete (precio unitario * cantidad por paquete)
                det.PrecioPaqueteMomento = det.PrecioUnitario * prodFuente.CantidadPorPaquete;
                // Guardar precio ministerio por paquete si existe
                det.PrecioMinisterioPaqueteMomento = prodFuente.PrecioMinisterioPaquete;
            }
        }
        Detalles.Add(det);
        RecalcularTotales();
        NuevoDetalle = new VentaDetalle { Cantidad = 1, PrecioUnitario = 0 };
        NuevoDetalleDescuentoPorcentaje = 0; // Reset descuento
        PrecioOriginalSinDescuento = 0;
        ErrorDescuento = null;
        BuscarProducto = string.Empty; MostrarSugProductos = false;
        await Task.CompletedTask;
    }

    private void QuitarDetalle(VentaDetalle det)
    {
        if (Detalles.Remove(det))
        {
            // Si es un producto controlado, quitar también su receta pendiente
            _recetasPendientes.RemoveAll(r => r.IdProducto == det.IdProducto);
            RecalcularTotales();
        }
    }

    // ========== MÉTODOS PARA MODAL DE RECETA MÉDICA ==========
    
    private async Task ConfirmarRecetaAsync()
    {
        _recetaError = null;
        
        // Validar campos obligatorios
        if (string.IsNullOrWhiteSpace(_recetaNumeroRegistro))
        {
            _recetaError = "Ingrese el número de registro de la receta";
            return;
        }
        if (string.IsNullOrWhiteSpace(_recetaNombreMedico))
        {
            _recetaError = "Ingrese el nombre del médico";
            return;
        }
        if (string.IsNullOrWhiteSpace(_recetaNombrePaciente))
        {
            _recetaError = "Ingrese el nombre del paciente";
            return;
        }
        
        // Agregar receta a la lista pendiente
        if (_productoRecetaPendiente != null)
        {
            _recetasPendientes.Add(new RecetaVenta
            {
                IdProducto = _productoRecetaPendiente.IdProducto,
                NumeroRegistro = _recetaNumeroRegistro.Trim(),
                FechaReceta = _recetaFecha,
                NombreMedico = _recetaNombreMedico.Trim(),
                NombrePaciente = _recetaNombrePaciente.Trim(),
                FechaRegistro = DateTime.Now
            });
        }
        
        _mostrarModalReceta = false;
        _productoRecetaPendiente = null;
        
        // Ahora sí agregar el producto
        await AgregarDetalleInternoAsync();
    }
    
    private void CancelarReceta()
    {
        _mostrarModalReceta = false;
        _productoRecetaPendiente = null;
        _recetaError = null;
        // No agregar el producto, limpiar la selección
        NuevoDetalle = new VentaDetalle { Cantidad = 1, PrecioUnitario = 0 };
        BuscarProducto = string.Empty;
        MostrarSugProductos = false;
    }
    
    // ========== FIN MÉTODOS RECETA ==========

    private void SeleccionarDetalleGrid(VentaDetalle det)
    {
        var prod = Productos.FirstOrDefault(x => x.IdProducto == det.IdProducto);
        if (prod != null)
        {
            ProductoImagenUrl = prod.Foto;
            StateHasChanged();
        }
    }

    private void RecalcularNuevoDetalle()
    {
        var prod = Productos.FirstOrDefault(x => x.IdProducto == NuevoDetalle.IdProducto);
        var ivaPorc = prod?.TipoIva?.Porcentaje ?? 0m;
        var cantidad = NuevoDetalle.Cantidad;
        var precio = NuevoDetalle.PrecioUnitario;
        var importe = Math.Round(cantidad * precio, 4);
        NuevoDetalle.Importe = importe;
        NuevoDetalle.IVA10 = 0; NuevoDetalle.IVA5 = 0; NuevoDetalle.Exenta = 0; NuevoDetalle.Grabado10 = 0; NuevoDetalle.Grabado5 = 0;
        if (ivaPorc >= 9.9m && ivaPorc <= 10.1m)
        {
            var grab = Math.Round(importe / 1.1m, 4);
            NuevoDetalle.Grabado10 = grab;
            NuevoDetalle.IVA10 = Math.Round(importe - grab, 4);
        }
        else if (ivaPorc >= 4.9m && ivaPorc <= 5.1m)
        {
            var grab = Math.Round(importe / 1.05m, 4);
            NuevoDetalle.Grabado5 = grab;
            NuevoDetalle.IVA5 = Math.Round(importe - grab, 4);
        }
        else
        {
            NuevoDetalle.Exenta = importe;
        }
    }

    /// <summary>
    /// Obtiene el título/tooltip para el campo de descuento según el estado actual.
    /// </summary>
    protected string GetTituloDescuento()
    {
        if (NuevoDetalle?.IdProducto <= 0) return "Seleccione un producto";
        if (!ProductoPermiteDescuento) return "Este producto no permite descuentos";
        if (!ProductoTieneDescuentoConfigurado) 
            return "No hay regla de descuento configurada para este producto";
        if (ProductoTieneDescuentoConfigurado)
            return $"Descuento permitido: {DescuentoBaseProducto:N2}% base + {MargenCajeroProducto:N2}% margen = {DescuentoMaximoPermitidoProducto:N2}% máx. ({OrigenDescuentoProducto})";
        return "";
    }

    /// <summary>
    /// Aplica el descuento al precio unitario del nuevo detalle.
    /// Valida: 1) Producto permite descuento, 2) Existe config de descuento, 3) No supere el máximo permitido, 4) No quede por debajo del precio de costo
    /// </summary>
    protected void OnDescuentoChanged()
    {
        ErrorDescuento = null;
        
        // Si no está habilitado el descuento a nivel sistema, ignorar
        if (!PermitirVenderConDescuento)
        {
            NuevoDetalleDescuentoPorcentaje = 0;
            return;
        }
        
        // Verificar si el producto permite descuento
        var productoActual = Productos.FirstOrDefault(p => p.IdProducto == NuevoDetalle.IdProducto);
        if (productoActual != null && !productoActual.PermiteDescuento)
        {
            ErrorDescuento = "Este producto no permite descuentos";
            NuevoDetalleDescuentoPorcentaje = 0;
            return;
        }
        
        // Validar rango básico
        if (NuevoDetalleDescuentoPorcentaje < 0) NuevoDetalleDescuentoPorcentaje = 0;
        if (NuevoDetalleDescuentoPorcentaje > 100) NuevoDetalleDescuentoPorcentaje = 100;
        
        // ========== VALIDACIÓN DE DESCUENTO SEGÚN CONFIGURACIÓN ==========
        // Verificar si el producto tiene descuento configurado (aplica para modo normal y farmacia)
        if (!ProductoTieneDescuentoConfigurado)
        {
            // NO hay descuento configurado → el cajero NO puede modificar
            if (NuevoDetalleDescuentoPorcentaje > 0)
            {
                ErrorDescuento = "Este producto no tiene descuento configurado";
                NuevoDetalleDescuentoPorcentaje = 0;
            }
        }
        else
        {
            // SÍ hay descuento configurado → validar que no supere el máximo (base + margen)
            // Esto aplica TANTO para modo normal como para modo farmacia
            if (NuevoDetalleDescuentoPorcentaje > DescuentoMaximoPermitidoProducto)
            {
                ErrorDescuento = $"El descuento no puede superar {DescuentoMaximoPermitidoProducto:N2}% ({OrigenDescuentoProducto}: {DescuentoBaseProducto:N2}% + Margen: {MargenCajeroProducto:N2}%)";
                NuevoDetalleDescuentoPorcentaje = DescuentoMaximoPermitidoProducto;
            }
        }
        
        // Calcular precio con descuento
        var descuentoMultiplier = 1m - (NuevoDetalleDescuentoPorcentaje / 100m);
        var precioConDescuento = Math.Round(PrecioOriginalSinDescuento * descuentoMultiplier, 2);
        
        // ========== VALIDACIÓN DE PRECIO MÍNIMO/MÁXIMO ==========
        if (DescuentoBasadoEnPrecioMinisterio && NuevoDetalle.PrecioMinisterio.HasValue && NuevoDetalle.PrecioMinisterio.Value > 0)
        {
            // MODO FARMACIA: El precio NO puede ser MAYOR al Precio Ministerio
            var precioMinisterio = NuevoDetalle.PrecioMinisterio.Value;
            if (precioConDescuento > precioMinisterio)
            {
                // Calcular el descuento mínimo necesario para no superar el Precio Ministerio
                var descuentoMinimo = ((PrecioOriginalSinDescuento - precioMinisterio) / PrecioOriginalSinDescuento) * 100m;
                if (descuentoMinimo < 0) descuentoMinimo = 0;
                
                ErrorDescuento = $"El precio no puede ser mayor al Precio Ministerio ({precioMinisterio:N0}). Descuento mínimo: {descuentoMinimo:N2}%";
                NuevoDetalleDescuentoPorcentaje = descuentoMinimo;
                descuentoMultiplier = 1m - (NuevoDetalleDescuentoPorcentaje / 100m);
                precioConDescuento = Math.Round(PrecioOriginalSinDescuento * descuentoMultiplier, 2);
            }
        }
        else if (productoActual != null && productoActual.CostoUnitarioGs.HasValue && productoActual.CostoUnitarioGs.Value > 0)
        {
            // MODO NORMAL: El precio NO puede ser MENOR al costo (salvo que el producto lo permita)
            var costoUnitario = productoActual.CostoUnitarioGs.Value;
            
            // Solo validar si el producto NO permite venta bajo costo
            if (!productoActual.PermiteVentaBajoCosto && precioConDescuento < costoUnitario)
            {
                // Calcular el descuento máximo posible sin bajar del costo
                var descuentoMaxPorCosto = ((PrecioOriginalSinDescuento - costoUnitario) / PrecioOriginalSinDescuento) * 100m;
                if (descuentoMaxPorCosto < 0) descuentoMaxPorCosto = 0;
                
                ErrorDescuento = $"El precio no puede ser menor al costo ({costoUnitario:N0}). Máximo descuento: {descuentoMaxPorCosto:N2}%";
                NuevoDetalleDescuentoPorcentaje = descuentoMaxPorCosto;
                descuentoMultiplier = 1m - (NuevoDetalleDescuentoPorcentaje / 100m);
                precioConDescuento = Math.Round(PrecioOriginalSinDescuento * descuentoMultiplier, 2);
            }
        }
        
        // Aplicar precio con descuento
        NuevoDetalle.PrecioUnitario = precioConDescuento;
        NuevoDetalle.PorcentajeDescuento = NuevoDetalleDescuentoPorcentaje > 0 ? NuevoDetalleDescuentoPorcentaje : null;
        
        RecalcularNuevoDetalle();
    }

    private void RecalcularTotales()
    {
        Gravado10 = Detalles.Sum(d => d.Grabado10);
        Gravado5 = Detalles.Sum(d => d.Grabado5);
        Exenta = Detalles.Sum(d => d.Exenta);
        IVATotal10 = Detalles.Sum(d => d.IVA10);
        IVATotal5 = Detalles.Sum(d => d.IVA5);
        Cab.Total = Math.Round(Detalles.Sum(d => d.Importe), 4);
    }

    private async Task<decimal> ObtenerTasaAplicadaAsync(int idMonedaOrigen, int idMonedaDestino)
    {
        if (idMonedaOrigen == idMonedaDestino) return 1m;
        await using var ctx = await DbFactory.CreateDbContextAsync();
        var hoy = DateTime.Today;
        var monOri = await ctx.Monedas.AsNoTracking().FirstOrDefaultAsync(m => m.IdMoneda == idMonedaOrigen);
        var monDes = await ctx.Monedas.AsNoTracking().FirstOrDefaultAsync(m => m.IdMoneda == idMonedaDestino);
        var tc = await ctx.TiposCambio.AsNoTracking()
            .Where(t => t.IdMonedaOrigen == idMonedaOrigen && t.IdMonedaDestino == idMonedaDestino)
            .OrderByDescending(t => t.FechaTipoCambio)
            .FirstOrDefaultAsync();
        if (tc != null && tc.FechaTipoCambio.Date == hoy)
        {
            var tasa = (monDes?.CodigoISO == "PYG") ? (tc.TasaCompra ?? tc.TasaCambio) : tc.TasaCambio;
            if (tasa != 0) return tasa;
        }
        var tcInv = await ctx.TiposCambio.AsNoTracking()
            .Where(t => t.IdMonedaOrigen == idMonedaDestino && t.IdMonedaDestino == idMonedaOrigen)
            .OrderByDescending(t => t.FechaTipoCambio)
            .FirstOrDefaultAsync();
        if (tcInv != null && tcInv.FechaTipoCambio.Date == hoy)
        {
            var tasaInv = (monOri?.CodigoISO == "PYG") ? (tcInv.TasaCompra ?? tcInv.TasaCambio) : tcInv.TasaCambio;
            if (tasaInv != 0) return Math.Round(1m / tasaInv, 6);
        }
        if (tc != null)
        {
            var tasa = (monDes?.CodigoISO == "PYG") ? (tc.TasaCompra ?? tc.TasaCambio) : tc.TasaCambio;
            if (tasa != 0) return tasa;
        }
        if (tcInv != null)
        {
            var tasaInv = (monOri?.CodigoISO == "PYG") ? (tcInv.TasaCompra ?? tcInv.TasaCambio) : tcInv.TasaCambio;
            if (tasaInv != 0) return Math.Round(1m / tasaInv, 6);
        }
        return 1m;
    }

    public async Task GuardarAsync()
    {
        // ========== PROTECCIÓN CONTRA DOBLE CLIC ==========
        if (_guardando)
        {
            Console.WriteLine("[Ventas] ⚠️ Guardado en progreso, ignorando clic duplicado");
            return;
        }
        _guardando = true;
        await InvokeAsync(StateHasChanged);
        
        try
        {
            await GuardarInternoAsync();
        }
        finally
        {
            _guardando = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task GuardarInternoAsync()
    {
        // Validaciones mínimas
        if (Cab.IdSucursal <= 0)
        {
            MensajeAdvertencia = "⚠️ Sucursal inválida. Por favor seleccione una sucursal.";
            await InvokeAsync(StateHasChanged);
            return;
        }
        if (!Cab.IdCliente.HasValue || Cab.IdCliente.Value <= 0)
        {
            MensajeAdvertencia = "⚠️ Debe seleccionar un cliente antes de guardar.";
            await InvokeAsync(StateHasChanged);
            return;
        }
        if (!Detalles.Any())
        {
            MensajeAdvertencia = "⚠️ Debe agregar al menos 1 producto a la venta.";
            await InvokeAsync(StateHasChanged);
            return;
        }
        if (Cab.Total <= 0)
        {
            MensajeAdvertencia = "⚠️ El total de la venta debe ser mayor a cero. Verifique los precios de los productos.";
            await InvokeAsync(StateHasChanged);
            return;
        }
        
        // Validar composición de caja para ventas al contado (no crédito ni remisión)
        var esPresupuestoValidacion = string.Equals(Cab.TipoIngreso, "PRESUPUESTO", StringComparison.OrdinalIgnoreCase);
        var esRemisionValidacion = Cab.TipoDocumento?.ToUpperInvariant().Contains("REMISION") ?? false;
        
        if (!TipoPagoEsCredito && !esPresupuestoValidacion && !esRemisionValidacion)
        {
            // Es venta al contado - debe tener composición de caja
            if (!_cajaDetalles.Any())
            {
                // Abrir modal de composición de caja automáticamente
                _mostrarComposicionCaja = true;
                _cajaDetalles.Clear();
                _cajaTotalGs = 0;
                _cajaError = null;
                _cajaInfo = null;
                var monV = Monedas.FirstOrDefault(m => m.IdMoneda == Cab.IdMoneda);
                _detalleCajaIdMoneda = Cab.IdMoneda ?? monV?.IdMoneda ?? 0;
                _detalleCajaMedio = "EFECTIVO";
                _detalleCajaMonto = 0; // Iniciar en 0 para que el usuario ingrese el monto
                _detalleCajaComprobante = null;
                // NO agregar línea automáticamente - el usuario debe hacer clic en "Agregar detalle"
                await InvokeAsync(StateHasChanged);
                // Poner foco en el campo de monto
                await FocusMontoInputAsync();
                return;
            }
            
            // Validar que el total de la composición sea >= total de la venta (permite vuelto)
            var totalComposicion = _cajaDetalles.Sum(x => x.MontoGs);
            if (totalComposicion < Cab.Total - 1m) // Tolerancia de 1 Gs por redondeo
            {
                MensajeAdvertencia = $"⚠️ El total de la composición de caja ({totalComposicion:N0} Gs) es menor que el total de la venta ({Cab.Total:N0} Gs). Falta cobrar {(Cab.Total - totalComposicion):N0} Gs.";
                await InvokeAsync(StateHasChanged);
                return;
            }
        }

        await using var ctx = await DbFactory.CreateDbContextAsync();
        MensajeAdvertencia = null;
        
        // Obtener configuración de impresión automática de la caja ANTES de hacer cualquier cosa
        var cajaConfig = await ctx.Cajas.AsNoTracking().FirstOrDefaultAsync(c => c.CajaActual == 1);
        var debeImprimirAutomaticamente = cajaConfig?.Imprimir_Factura == 1;
        Console.WriteLine($"[Ventas] Caja config loaded: Imprimir_Factura={cajaConfig?.Imprimir_Factura}, DebeImprimir={debeImprimirAutomaticamente}");
        
        try
        {
            // Validar si es venta a crédito o remisión - verificar que el cliente esté habilitado
            var esCredito = TipoPagoEsCredito;
            var esRemision = Cab.TipoDocumento?.ToUpperInvariant().Contains("REMISION") ?? false;
            
            if (esCredito || esRemision)
            {
                var cliente = await ctx.Clientes.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IdCliente == Cab.IdCliente.Value);
                
                if (cliente != null && !cliente.PermiteCredito)
                {
                    MensajeAdvertencia = $"⚠️ El cliente '{cliente.RazonSocial}' no está habilitado para venta a crédito o emisión de remisiones. Habilite la opción 'Permite Crédito' en la ficha del cliente.";
                    await InvokeAsync(StateHasChanged);
                    return;
                }
                
                // Validar límite de crédito si es venta a crédito
                if (esCredito && cliente != null && cliente.LimiteCredito.HasValue)
                {
                    var disponible = cliente.LimiteCredito.Value - cliente.Saldo;
                    if (Cab.Total > disponible)
                    {
                        MensajeAdvertencia = $"⚠️ El cliente '{cliente.RazonSocial}' ha excedido su límite de crédito. Disponible: {disponible:N2}, Total venta: {Cab.Total:N2}";
                        await InvokeAsync(StateHasChanged);
                        return;
                    }
                }
            }
            
            // Respetar fecha de caja: Cab.Fecha se setea en CargarNumeracionesAsync; registrar fecha de equipo para auditoría
            FechaEquipoAuditoria = DateTime.Now;
            var esPresupuesto = string.Equals(Cab.TipoIngreso, "PRESUPUESTO", StringComparison.OrdinalIgnoreCase);
            Cab.Estado = esPresupuesto ? "Presupuesto" : "Confirmado";
            // Moneda
            var mon = Monedas.FirstOrDefault(m => m.IdMoneda == Cab.IdMoneda);
            Cab.SimboloMoneda = mon?.Simbolo ?? Cab.SimboloMoneda;
            Cab.EsMonedaExtranjera = (mon?.CodigoISO ?? "PYG") != "PYG";
            // Forma de pago
            if (Cab.IdTipoPago.HasValue && string.IsNullOrWhiteSpace(Cab.FormaPago))
            {
                var sel = TiposPago.FirstOrDefault(t => t.IdTipoPago == Cab.IdTipoPago);
                if (sel != null) Cab.FormaPago = sel.Nombre;
            }
            // No hay condición de crédito aquí.
            // Total en letras
            Cab.TotalEnLetras = NumeroEnLetras(Cab.Total, mon?.CodigoISO ?? "PYG");
            if (esPresupuesto)
            {
                var pre = new Presupuesto
                {
                    IdSucursal = Cab.IdSucursal,
                    IdCliente = Cab.IdCliente,
                    Fecha = Cab.Fecha,
                    NumeroPresupuesto = NumeroPresupuestoUI,
                    IdMoneda = Cab.IdMoneda,
                    SimboloMoneda = Cab.SimboloMoneda,
                    CambioDelDia = Cab.CambioDelDia,
                    EsMonedaExtranjera = Cab.EsMonedaExtranjera,
                    Total = Cab.Total,
                    TotalEnLetras = Cab.TotalEnLetras,
                    Comentario = Cab.Comentario,
                    Estado = Cab.Estado,
                    ValidezDias = ValidezDias,
                    ValidoHasta = ValidoHasta
                };
                ctx.Presupuestos.Add(pre);
                await ctx.SaveChangesAsync();

                foreach (var d in Detalles)
                {
                    var pd = new PresupuestoDetalle
                    {
                        IdPresupuesto = pre.IdPresupuesto,
                        IdProducto = d.IdProducto,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Importe = d.Importe,
                        IVA10 = d.IVA10,
                        IVA5 = d.IVA5,
                        Exenta = d.Exenta,
                        Grabado10 = d.Grabado10,
                        Grabado5 = d.Grabado5,
                        CambioDelDia = d.CambioDelDia,
                        IdTipoIva = d.IdTipoIva
                    };
                    ctx.PresupuestosDetalles.Add(pd);
                }
                await ctx.SaveChangesAsync();
                
                // Abrir ventana de impresión automáticamente después de guardar el presupuesto
                try 
                { 
                    await JS.InvokeVoidAsync("printPresupuesto", pre.IdPresupuesto); 
                } 
                catch (Exception ex) 
                { 
                    Console.WriteLine($"Error al abrir impresión de presupuesto: {ex.Message}"); 
                }
            }
            else
            {
                // Iniciar transacción ANTES de asignar numeración para evitar conflictos de concurrencia
                using var tx = await ctx.Database.BeginTransactionAsync();
                
                // Completar numeración de venta (valida timbrado y vigencia según tipo: Factura/Remisión)
                var ok = await AsignarNumeracionVentaAsync(ctx);
                if (!ok)
                {
                    // Mostrar advertencia y no continuar con guardado
                    await tx.RollbackAsync();
                    StateHasChanged();
                    return;
                }
                
                // Actualizar email del cliente si se modificó
                if (Cab.IdCliente.HasValue && !string.IsNullOrWhiteSpace(EmailCliente))
                {
                    var clienteDb = await ctx.Clientes.FirstOrDefaultAsync(c => c.IdCliente == Cab.IdCliente.Value);
                    if (clienteDb != null && clienteDb.Email != EmailCliente?.Trim())
                    {
                        clienteDb.Email = EmailCliente.Trim();
                        await ctx.SaveChangesAsync();
                    }
                }
                
                // Evitar adjuntar entidades navegadas duplicadas (EF tracking conflict)
                Cab.Caja = null;
                Cab.Sucursal = null;
                Cab.Cliente = null;
                Cab.TipoPago = null;
                Cab.Moneda = null;
                Cab.Usuario = null;
                Cab.TipoDocumentoOperacion = null;
                
                // Asegurar que IdVenta sea 0 para que EF genere uno nuevo
                Cab.IdVenta = 0;
                
                // Asignar usuario que realizó la venta
                Cab.IdUsuario = _idUsuarioActual;
                Cab.Vendedor = _usuarioActual;
                
                ctx.Ventas.Add(Cab);
                await ctx.SaveChangesAsync();

                foreach (var d in Detalles)
                {
                    d.IdVentaDetalle = 0; // Asegurar que EF genere un nuevo ID
                    d.IdVenta = Cab.IdVenta;
                    d.Producto = null; // Evitar tracking conflict
                    ctx.VentasDetalles.Add(d);
                }
                await ctx.SaveChangesAsync();

                // *** GUARDAR RECETAS MÉDICAS SI HAY PRODUCTOS CONTROLADOS ***
                if (_recetasPendientes.Any())
                {
                    foreach (var receta in _recetasPendientes)
                    {
                        receta.IdVenta = Cab.IdVenta;
                        ctx.RecetasVentas.Add(receta);
                    }
                    await ctx.SaveChangesAsync();
                    _recetasPendientes.Clear();
                }

                // *** CREAR CUENTA POR COBRAR SI ES CRÉDITO ***
                if (Cab.IdTipoPago.HasValue)
                {
                    var tipoPago = await ctx.TiposPago.AsNoTracking().FirstOrDefaultAsync(tp => tp.IdTipoPago == Cab.IdTipoPago.Value);
                    if (tipoPago != null && tipoPago.EsCredito)
                    {
                        var numeroCuotasCredito = NumeroCuotas > 0 ? NumeroCuotas : 1;
                        var cuentaCobrar = new CuentaPorCobrar
                        {
                            IdVenta = Cab.IdVenta,
                            IdCliente = Cab.IdCliente ?? 0,
                            IdSucursal = Cab.IdSucursal,
                            IdMoneda = Cab.IdMoneda,
                            FechaCredito = Cab.Fecha,
                            FechaVencimiento = Cab.FechaVencimiento ?? Cab.Fecha.AddDays(30),
                            MontoTotal = Cab.Total,
                            SaldoPendiente = Cab.Total,
                            NumeroCuotas = numeroCuotasCredito,
                            PlazoDias = Cab.Plazo ?? 30,
                            Estado = "PENDIENTE"
                        };
                        ctx.CuentasPorCobrar.Add(cuentaCobrar);
                        await ctx.SaveChangesAsync();

                        // Generar cuotas automáticamente
                        // La primera cuota vence en la FechaVencimiento ingresada por el usuario
                        // Las siguientes cuotas se calculan sumando plazoDias desde la primera
                        var plazoDias = cuentaCobrar.PlazoDias;
                        var fechaPrimerVencimiento = cuentaCobrar.FechaVencimiento ?? cuentaCobrar.FechaCredito.AddDays(plazoDias);
                        
                        var montoPorCuota = Math.Round(cuentaCobrar.MontoTotal / numeroCuotasCredito, 2);
                        var acumulado = 0m;
                        for (int i = 1; i <= numeroCuotasCredito; i++)
                        {
                            var monto = (i == numeroCuotasCredito) ? (cuentaCobrar.MontoTotal - acumulado) : montoPorCuota;
                            // Primera cuota: fecha de vencimiento ingresada, las siguientes cada "plazoDias" días desde la primera
                            var fechaVencimientoCuota = fechaPrimerVencimiento.AddDays((i - 1) * plazoDias);
                            var cuota = new CuentaPorCobrarCuota
                            {
                                IdCuentaPorCobrar = cuentaCobrar.IdCuentaPorCobrar,
                                NumeroCuota = i,
                                MontoCuota = monto,
                                SaldoCuota = monto,
                                FechaVencimiento = fechaVencimientoCuota,
                                Estado = "PENDIENTE"
                            };
                            ctx.CuentasPorCobrarCuotas.Add(cuota);
                            acumulado += monto;
                        }
                        await ctx.SaveChangesAsync();
                    }
                }

                // *** CREAR REMISIÓN INTERNA SI ES REMISIÓN ***
                var esRemisionInternal = EsRemisionNombre(Cab.TipoDocumento);
                if (esRemisionInternal)
                {
                    var remision = new RemisionInterna
                    {
                        IdVenta = Cab.IdVenta,
                        IdCliente = Cab.IdCliente ?? 0,
                        IdSucursal = Cab.IdSucursal,
                        FechaRemision = Cab.Fecha,
                        NumeroRemision = Cab.NumeroFactura ?? "R-0",
                        MontoTotal = Cab.Total,
                        Estado = "PENDIENTE"
                    };
                    ctx.RemisionesInternas.Add(remision);
                    await ctx.SaveChangesAsync();

                    // Copiar detalles de venta a remisión
                    foreach (var d in Detalles)
                    {
                        var detalle = new RemisionInternaDetalle
                        {
                            IdRemision = remision.IdRemision,
                            IdProducto = d.IdProducto,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitario,
                            Subtotal = d.Importe,
                            Gravado5 = d.Grabado5,
                            Gravado10 = d.Grabado10,
                            Exenta = d.Exenta,
                            IVA5 = d.IVA5,
                            IVA10 = d.IVA10
                        };
                        ctx.RemisionesInternasDetalles.Add(detalle);
                    }
                    await ctx.SaveChangesAsync();
                }

                // Si esta venta proviene de un presupuesto, marcar el presupuesto como convertido
                if (Cab.IdPresupuestoOrigen.HasValue && Cab.IdPresupuestoOrigen.Value > 0)
                {
                    var presupuestoOrigen = await ctx.Presupuestos.FirstOrDefaultAsync(p => p.IdPresupuesto == Cab.IdPresupuestoOrigen.Value);
                    if (presupuestoOrigen != null)
                    {
                        presupuestoOrigen.IdVentaConvertida = Cab.IdVenta;
                        presupuestoOrigen.Estado = "Convertido";
                        await ctx.SaveChangesAsync();
                    }
                }

                // Auditoría simple en comentario con fecha de equipo
                if (FechaEquipoAuditoria.HasValue)
                {
                    Cab.Comentario = string.IsNullOrWhiteSpace(Cab.Comentario)
                        ? $"FE_AUD:{FechaEquipoAuditoria:yyyy-MM-dd HH:mm:ss}"
                        : Cab.Comentario + $" | FE_AUD:{FechaEquipoAuditoria:yyyy-MM-dd HH:mm:ss}";
                    ctx.Ventas.Update(Cab);
                    await ctx.SaveChangesAsync();
                }
                await tx.CommitAsync();
                
                // Registrar en auditoría con contexto completo
                var cajaAudit = await ctx.Cajas.AsNoTracking().FirstOrDefaultAsync(c => c.IdCaja == Cab.IdCaja);
                var sucAudit = await ctx.Sucursal.AsNoTracking().FirstOrDefaultAsync(s => s.Id == Cab.IdSucursal);
                var rolUsuario = await ctx.Usuarios.AsNoTracking()
                    .Where(u => u.Id_Usu == _idUsuarioActual)
                    .Select(u => u.Rol != null ? u.Rol.NombreRol : null)
                    .FirstOrDefaultAsync();
                
                _ = AuditoriaService.RegistrarAccionAsync(
                    idUsuario: _idUsuarioActual,
                    nombreUsuario: _usuarioActual ?? "Sistema",
                    rolUsuario: rolUsuario,
                    accion: esPresupuesto ? "Crear Presupuesto" : "Crear Venta",
                    tipoAccion: "CREATE",
                    modulo: "Ventas",
                    entidad: esPresupuesto ? "Presupuesto" : "Venta",
                    idRegistroAfectado: Cab.IdVenta,
                    descripcion: $"{(esPresupuesto ? "Presupuesto" : "Venta")} #{Cab.NumeroFactura} - {Cab.SimboloMoneda} {Cab.Total:N0} - {Cab.FormaPago} - Cliente: {ClienteSeleccionadoLabel} - Items: {Detalles.Count}",
                    datosDespues: new { 
                        Cab.NumeroFactura, 
                        Cab.Total, 
                        Cab.FormaPago, 
                        Cliente = ClienteSeleccionadoLabel, 
                        Items = Detalles.Count
                    },
                    fechaHoraEquipo: FechaEquipoAuditoria,
                    fechaCaja: cajaAudit?.FechaActualCaja,
                    turno: cajaAudit?.TurnoActual,
                    idSucursal: Cab.IdSucursal,
                    nombreSucursal: sucAudit?.NombreSucursal,
                    idCaja: Cab.IdCaja,
                    nombreCaja: cajaAudit?.Nombre
                );
            }

            // Ajuste de stock: solo para VENTAS. Para PRESUPUESTO no afecta stock.
            // Los servicios no requieren control de stock.
            if (!esPresupuesto)
            {
                // Obtener los IDs de tipos de item que son servicios (no controlan stock)
                var tiposServicio = await ctx.TiposItem
                    .AsNoTracking()
                    .Where(t => t.EsServicio)
                    .Select(t => t.IdTipoItem)
                    .ToListAsync();

                // Filtrar detalles que NO son servicios (solo productos físicos)
                // También incluir información de control de lotes
                var productosIds = Detalles.Select(d => d.IdProducto).ToList();
                var productosConTipo = await ctx.Productos
                    .AsNoTracking()
                    .Where(p => productosIds.Contains(p.IdProducto))
                    .Select(p => new { p.IdProducto, p.TipoItem, p.ControlaLote, p.ControlaVencimiento })
                    .ToListAsync();

                var detallesFisicos = Detalles
                    .Where(d => {
                        var prod = productosConTipo.FirstOrDefault(p => p.IdProducto == d.IdProducto);
                        return prod != null && !tiposServicio.Contains(prod.TipoItem);
                    })
                    .ToList();

                // Salida de stock (2). Usar el primer depósito activo de la sucursal (depósito principal/general)
                var depositoPrincipal = await ctx.Depositos
                    .AsNoTracking()
                    .Where(d => d.IdSucursal == Cab.IdSucursal && d.Activo)
                    .OrderBy(d => d.IdDeposito)
                    .FirstOrDefaultAsync();

                var idDeposito = depositoPrincipal?.IdDeposito ?? 
                    (await ctx.Depositos.AsNoTracking().Where(d => d.Activo).OrderBy(d => d.IdDeposito).Select(d => d.IdDeposito).FirstOrDefaultAsync());

                if (idDeposito > 0)
                {
                    foreach (var d in detallesFisicos)
                    {
                        // Verificar si el producto controla lotes
                        var prodInfo = productosConTipo.FirstOrDefault(p => p.IdProducto == d.IdProducto);
                        
                        if (prodInfo != null && prodInfo.ControlaLote)
                        {
                            // ========== LÓGICA FEFO: Descontar de múltiples lotes en orden de vencimiento ==========
                            var cantidadPendiente = d.Cantidad;
                            
                            // Obtener todos los lotes ordenados por FEFO (una sola consulta)
                            var lotesFEFO = await LoteService.ObtenerLotesFEFOParaVentaAsync(d.IdProducto, idDeposito);
                            
                            if (lotesFEFO.Count == 0)
                            {
                                // No hay lotes con stock - usar stock general
                                Console.WriteLine($"[FEFO] Producto {d.IdProducto}: Sin lotes disponibles para {cantidadPendiente} unidades. Usando stock general.");
                                await Inventario.AjustarStockAsync(d.IdProducto, idDeposito, cantidadPendiente, 2, $"Venta #{Cab.IdVenta} (sin lote)", _usuarioActual,
                                    Cab.IdSucursal, Cab.IdCaja, Cab.Fecha.Date, Cab.Turno);
                            }
                            else
                            {
                                // Distribuir cantidad entre lotes en orden FEFO
                                foreach (var lote in lotesFEFO)
                                {
                                    if (cantidadPendiente <= 0) break;
                                    
                                    // Calcular cuánto descontar de este lote
                                    var cantidadDescontar = Math.Min(cantidadPendiente, lote.Stock);
                                    
                                    // Descontar del lote
                                    var resultado = await LoteService.DescontarStockLoteAsync(
                                        lote.IdProductoLote,
                                        cantidadDescontar,
                                        "Venta",
                                        Cab.IdVenta,
                                        d.IdVentaDetalle,
                                        $"Venta #{Cab.IdVenta}",
                                        _usuarioActual
                                    );
                                    
                                    if (resultado)
                                    {
                                        // Guardar info del primer lote usado (el más cercano a vencer)
                                        if (d.IdProductoLote == null)
                                        {
                                            d.IdProductoLote = lote.IdProductoLote;
                                            d.NumeroLoteMomento = lote.NumeroLote;
                                            d.FechaVencimientoLoteMomento = lote.FechaVencimiento;
                                            ctx.VentasDetalles.Update(d);
                                        }
                                        
                                        Console.WriteLine($"[FEFO] Producto {d.IdProducto}: Descontado {cantidadDescontar} del lote {lote.NumeroLote} (vence: {lote.FechaVencimiento:dd/MM/yyyy})");
                                    }
                                    
                                    cantidadPendiente -= cantidadDescontar;
                                }
                                
                                // Si quedó pendiente (stock insuficiente en lotes), usar stock general
                                if (cantidadPendiente > 0)
                                {
                                    Console.WriteLine($"[FEFO] Producto {d.IdProducto}: Stock de lotes insuficiente, {cantidadPendiente} unidades desde stock general.");
                                    await Inventario.AjustarStockAsync(d.IdProducto, idDeposito, cantidadPendiente, 2, $"Venta #{Cab.IdVenta} (sin lote)", _usuarioActual,
                                        Cab.IdSucursal, Cab.IdCaja, Cab.Fecha.Date, Cab.Turno);
                                }
                            }
                            
                            // También ajustar stock general (para mantener consistencia con MovimientosStock)
                            await Inventario.AjustarStockAsync(d.IdProducto, idDeposito, d.Cantidad, 2, $"Venta #{Cab.IdVenta}", _usuarioActual,
                                Cab.IdSucursal, Cab.IdCaja, Cab.Fecha.Date, Cab.Turno);
                        }
                        else
                        {
                            // Producto sin control de lotes - usar lógica tradicional
                            await Inventario.AjustarStockAsync(d.IdProducto, idDeposito, d.Cantidad, 2, $"Venta #{Cab.IdVenta}", _usuarioActual,
                                Cab.IdSucursal, Cab.IdCaja, Cab.Fecha.Date, Cab.Turno);
                        }
                    }
                    
                    await ctx.SaveChangesAsync(); // Guardar cambios en VentaDetalles (info de lotes)
                }

                // Nota: La auto-impresión ocurre después de confirmar la composición de caja
                
                // Verificar si es crédito consultando directamente el tipo de pago guardado
                var esVentaCredito = false;
                if (Cab.IdTipoPago.HasValue)
                {
                    var tipoPagoGuardado = await ctx.TiposPago.AsNoTracking()
                        .FirstOrDefaultAsync(tp => tp.IdTipoPago == Cab.IdTipoPago.Value);
                    esVentaCredito = tipoPagoGuardado?.EsCredito ?? false;
                }
                
                Console.WriteLine($"[Ventas] Evaluando tipo de pago: esVentaCredito={esVentaCredito}, IdTipoPago={Cab.IdTipoPago}");
                
                // Si NO es crédito, ahora confirmamos la composición de caja (ya validada antes de guardar)
                if (!esVentaCredito && _cajaDetalles.Any())
                {
                    // Guardar composición de caja
                    await GuardarComposicionCajaAsync(ctx);
                    
                    // Guardar IdVenta, IdCliente e IdSucursal antes de limpiar
                    var idVentaGuardada = Cab.IdVenta;
                    var idClienteGuardado = Cab.IdCliente;
                    var idSucursalGuardada = Cab.IdSucursal;
                    
                    // PRIMERO: Enviar a SIFEN (SYNC) y ESPERAR la respuesta
                    // Esto asegura que CDC y UrlQrSifen estén en la BD antes de imprimir
                    await EnviarSifenAutomaticoAsync(idVentaGuardada);
                    
                    // DESPUÉS: Enviar factura por correo al cliente (en segundo plano)
                    // El correo se envía después de SIFEN para que incluya los datos reales
                    _ = Task.Run(async () => await EnviarFacturaCorreoSiCorrespondeAsync(idVentaGuardada, idClienteGuardado, idSucursalGuardada));
                    
                    // Mostrar vista previa del ticket en lugar de abrir ventana nueva
                    // (conservamos el método anterior comentado por si se necesita recuperar)
                    if (debeImprimirAutomaticamente)
                    {
                        // NUEVO: Mostrar modal de vista previa
                        _idVentaParaVistaPrevia = idVentaGuardada;
                        _mostrarVistaPrevia = true;
                        _mostrarComposicionCaja = false;
                        await InvokeAsync(StateHasChanged);
                        
                        /* MÉTODO ANTERIOR - COMENTADO PARA RECUPERAR SI ES NECESARIO
                        try
                        {
                            string url = cajaConfig?.FormatoImpresion == "TICKET"
                                ? $"/ventas/ticket/{Cab.IdVenta}/1"
                                : $"/ventas/preview/{Cab.IdVenta}?print=1";
                            await JS.InvokeVoidAsync("safeOpen", url, "_blank");
                        }
                        catch { }
                        */
                        
                        // Preparar para nueva venta
                        Limpiar();
                        await using var ctxNuevo = await DbFactory.CreateDbContextAsync();
                        Clientes = await ctxNuevo.Clientes.AsNoTracking().OrderBy(c => c.RazonSocial).ToListAsync();
                        ClientesFiltrados = new(Clientes);
                        await RecargarProductosConStockAsync(ctxNuevo);
                        await CargarNumeracionesAsync(ctxNuevo);
                        await InvokeAsync(StateHasChanged);
                    }
                    else
                    {
                        // No imprime automáticamente - cerrar modal y limpiar
                        _mostrarComposicionCaja = false;
                        Limpiar();
                        await using var ctxNuevo = await DbFactory.CreateDbContextAsync();
                        Clientes = await ctxNuevo.Clientes.AsNoTracking().OrderBy(c => c.RazonSocial).ToListAsync();
                        ClientesFiltrados = new(Clientes);
                        await RecargarProductosConStockAsync(ctxNuevo);
                        await CargarNumeracionesAsync(ctxNuevo);
                        await InvokeAsync(StateHasChanged);
                    }
                }
                else if (esVentaCredito)
                {
                    // Para ventas a crédito/remisión: mostrar vista previa en lugar de abrir ventana nueva
                    Console.WriteLine($"[Ventas] Venta a crédito/remisión guardada. IdVenta={Cab.IdVenta}, DebeImprimir={debeImprimirAutomaticamente}");
                    
                    // Guardar IdVenta, IdCliente e IdSucursal antes de limpiar
                    var idVentaGuardada = Cab.IdVenta;
                    var idClienteGuardado = Cab.IdCliente;
                    var idSucursalGuardada = Cab.IdSucursal;
                    
                    // PRIMERO: Enviar a SIFEN (SYNC) y ESPERAR la respuesta
                    // Esto asegura que CDC y UrlQrSifen estén en la BD antes de imprimir
                    await EnviarSifenAutomaticoAsync(idVentaGuardada);
                    
                    // DESPUÉS: Enviar factura por correo al cliente (en segundo plano)
                    // El correo se envía después de SIFEN para que incluya los datos reales
                    _ = Task.Run(async () => await EnviarFacturaCorreoSiCorrespondeAsync(idVentaGuardada, idClienteGuardado, idSucursalGuardada));
                    
                    if (debeImprimirAutomaticamente)
                    {
                        // NUEVO: Mostrar modal de vista previa
                        _idVentaParaVistaPrevia = idVentaGuardada;
                        _mostrarVistaPrevia = true;
                        Console.WriteLine($"[Ventas] Mostrando vista previa de ticket para venta: {idVentaGuardada}");
                        
                        /* MÉTODO ANTERIOR - COMENTADO PARA RECUPERAR SI ES NECESARIO
                        try
                        {
                            string url;
                            if (cajaConfig?.FormatoImpresion == "TICKET")
                            {
                                url = $"/ventas/ticket/{Cab.IdVenta}/1";
                                Console.WriteLine($"[Ventas] Abriendo ticket térmico con auto-impresión: {url}");
                            }
                            else
                            {
                                url = $"/ventas/preview/{Cab.IdVenta}?print=1";
                                Console.WriteLine($"[Ventas] Abriendo formato {cajaConfig?.FormatoImpresion ?? "A4"}: {url}");
                            }
                            
                            await JS.InvokeVoidAsync("safeOpen", url, "_blank");
                            Console.WriteLine($"[Ventas] Impresión invocada exitosamente");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Ventas] Error al abrir impresión: {ex.Message}");
                        }
                        */
                    }
                    else
                    {
                        Console.WriteLine($"[Ventas] Impresión automática deshabilitada (Imprimir_Factura != 1)");
                    }
                    
                    // Limpiar y preparar nueva venta
                    Limpiar();
                    await using var ctxNuevo = await DbFactory.CreateDbContextAsync();
                    
                    // Recargar clientes y productos
                    Clientes = await ctxNuevo.Clientes.AsNoTracking().OrderBy(c => c.RazonSocial).ToListAsync();
                    ClientesFiltrados = new(Clientes);
                    await RecargarProductosConStockAsync(ctxNuevo);
                    
                    await CargarNumeracionesAsync(ctxNuevo);
                    await InvokeAsync(StateHasChanged);
                }
            }

            // Nota: No limpiar aquí para no perder Cab.Total antes de confirmar Composición de Caja.
        }
        catch (ValidationException vex)
        {
            MensajeAdvertencia = $"⚠️ {vex.Message}";
            _ = TrackingService.RegistrarErrorAsync(
                descripcion: "Error de validación en Ventas",
                mensajeError: vex.Message,
                componente: "Ventas.GuardarAsync",
                idUsuario: _idUsuarioActual,
                nombreUsuario: _usuarioActual);
            await InvokeAsync(StateHasChanged);
        }
        catch (DbUpdateException dbEx)
        {
            var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
            MensajeAdvertencia = $"❌ Error de base de datos al guardar: {innerMsg}";
            _ = TrackingService.RegistrarErrorAsync(
                descripcion: "Error de BD al guardar venta",
                mensajeError: innerMsg,
                stackTrace: dbEx.StackTrace,
                componente: "Ventas.GuardarAsync",
                idUsuario: _idUsuarioActual,
                nombreUsuario: _usuarioActual);
            await InvokeAsync(StateHasChanged);
            Console.WriteLine($"Error DbUpdate en GuardarAsync: {dbEx}");
            if (dbEx.InnerException != null)
                Console.WriteLine($"Inner Exception: {dbEx.InnerException}");
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            MensajeAdvertencia = $"❌ Error al guardar la venta: {innerMsg}";
            _ = TrackingService.RegistrarErrorAsync(
                descripcion: "Error general al guardar venta",
                mensajeError: innerMsg,
                stackTrace: ex.StackTrace,
                componente: "Ventas.GuardarAsync",
                idUsuario: _idUsuarioActual,
                nombreUsuario: _usuarioActual);
            await InvokeAsync(StateHasChanged);
            Console.WriteLine($"Error en GuardarAsync: {ex}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner Exception: {ex.InnerException}");
        }
    }

    /// <summary>
    /// Envía automáticamente la venta a SIFEN si está en modo Factura Electrónica.
    /// Usa el modo SYNC para obtener el CDC inmediatamente y poder imprimir con datos reales.
    /// </summary>
    private async Task EnviarSifenAutomaticoAsync(int idVenta)
    {
        // Solo enviar si el tipo de facturación es ELECTRONICA
        if (string.IsNullOrWhiteSpace(_tipoFacturacionCaja)) return;
        
        var tipoUpper = _tipoFacturacionCaja.ToUpperInvariant();
        var esFacturaElectronica = tipoUpper.Contains("ELECTR") || tipoUpper == "ELECTRONICA" || tipoUpper == "FE";
        
        if (!esFacturaElectronica)
        {
            Console.WriteLine($"[SIFEN Auto] Tipo de facturación '{_tipoFacturacionCaja}' - No es electrónica, no se envía a SIFEN automáticamente.");
            return;
        }
        
        try
        {
            Console.WriteLine($"[SIFEN Auto] Enviando venta {idVenta} a SIFEN modo SYNC...");
            
            // Llamar al endpoint de envío SIFEN SYNC usando HttpClient interno
            // El modo SYNC retorna el CDC inmediatamente, a diferencia del modo LOTE
            var handler = new System.Net.Http.HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            using var http = new System.Net.Http.HttpClient(handler);
            http.Timeout = TimeSpan.FromSeconds(120);
            
            // Usar la URL base del servidor actual - SYNC en lugar de LOTE
            var baseUrl = "http://localhost:5095"; // URL por defecto
            var url = $"{baseUrl}/ventas/{idVenta}/enviar-sifen-sync";
            
            var response = await http.PostAsync(url, null);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[SIFEN Auto] Venta {idVenta} enviada exitosamente (SYNC): {responseBody}");
                
                // Extraer datos del JSON de respuesta para logging
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(responseBody);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("cdc", out var cdcProp))
                        Console.WriteLine($"[SIFEN Auto] CDC obtenido: {cdcProp.GetString()}");
                    if (root.TryGetProperty("estado", out var estadoProp))
                        Console.WriteLine($"[SIFEN Auto] Estado: {estadoProp.GetString()}");
                }
                catch { }
            }
            else
            {
                Console.WriteLine($"[SIFEN Auto] Error al enviar venta {idVenta} a SIFEN: {response.StatusCode} - {responseBody}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SIFEN Auto] Excepción al enviar venta {idVenta} a SIFEN: {ex.Message}");
            // No propagar la excepción - el envío SIFEN automático no debe bloquear la venta
        }
    }

    private void CerrarComposicionCaja()
    {
        // Solo cerrar el modal sin limpiar los datos de la venta
        // El usuario puede volver a intentar guardar o modificar la composición
        _mostrarComposicionCaja = false;
        _cajaDetalles.Clear();
        _cajaTotalGs = 0;
        _cajaError = null;
        _cajaInfo = null;
        StateHasChanged();
    }

    /// <summary>
    /// Envía la factura por correo al cliente si tiene habilitada la opción y tiene email configurado.
    /// </summary>
    private async Task EnviarFacturaCorreoSiCorrespondeAsync(int idVenta, int? idCliente, int sucursalId)
    {
        if (!idCliente.HasValue || idCliente.Value <= 0) return;
        
        try
        {
            await using var ctx = await DbFactory.CreateDbContextAsync();
            var cliente = await ctx.Clientes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCliente == idCliente.Value);
            
            if (cliente == null || !cliente.EnviarFacturaPorCorreo || string.IsNullOrWhiteSpace(cliente.Email))
            {
                Console.WriteLine($"[Ventas] No se envía correo: Cliente={cliente?.RazonSocial ?? "NULL"}, EnviarFactura={cliente?.EnviarFacturaPorCorreo}, Email={cliente?.Email ?? "NULL"}");
                return;
            }
            
            // Verificar si el correo está configurado
            if (!await CorreoService.EstaConfiguradoAsync(sucursalId))
            {
                Console.WriteLine("[Ventas] Servicio de correo no configurado. No se puede enviar factura.");
                return;
            }
            
            // Obtener la venta para armar el nombre del archivo
            var venta = await ctx.Ventas.AsNoTracking()
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
            
            if (venta == null)
            {
                Console.WriteLine($"[Ventas] Venta {idVenta} no encontrada para envío de correo.");
                return;
            }
            
            // Generar PDF en formato A4
            var pdfBytes = await PdfService.GenerarPdfFactura(idVenta);
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                Console.WriteLine($"[Ventas] No se pudo generar PDF para venta {idVenta}.");
                return;
            }
            
            var numeroFactura = venta.NumeroFactura ?? $"Venta-{idVenta}";
            var nombreArchivo = $"Factura_{numeroFactura.Replace("-", "_").Replace(" ", "_")}.pdf";
            
            Console.WriteLine($"[Ventas] Enviando factura {numeroFactura} a {cliente.Email}...");
            var resultado = await CorreoService.EnviarFacturaClienteAsync(
                idVenta,
                cliente.Email,
                pdfBytes,
                nombreArchivo
            );
            
            if (resultado.Exito)
            {
                Console.WriteLine($"[Ventas] ✅ Correo enviado exitosamente a {cliente.Email}");
            }
            else
            {
                Console.WriteLine($"[Ventas] ❌ Error al enviar correo: {resultado.Mensaje}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Ventas] Error al enviar factura por correo: {ex.Message}");
            // No interrumpir el flujo principal por un error de correo
        }
    }

    /// <summary>
    /// Guarda la composición de caja y los pagos de la venta actual (ya guardada en BD)
    /// Este método es llamado después de guardar la venta con éxito
    /// </summary>
    private async Task GuardarComposicionCajaAsync(AppDbContext ctx)
    {
        if (!_cajaDetalles.Any()) return;
        
        // Calcular vuelto: si el total cobrado es mayor que la venta
        var vuelto = Math.Max(0, _cajaTotalGs - Cab.Total);
        
        var comp = new ComposicionCaja
        {
            IdVenta = Cab.IdVenta,
            Fecha = Cab.Fecha,
            IdMoneda = Cab.IdMoneda,
            MontoTotal = _cajaTotalGs,
            Vuelto = vuelto,
            TipoCambioAplicado = (Cab.IdMoneda.HasValue
                ? await ObtenerTasaAplicadaAsync(Cab.IdMoneda.Value, (Monedas.FirstOrDefault(m => m.EsMonedaBase) ?? Monedas.First(m => (m.CodigoISO ?? "").ToUpper() == "PYG")).IdMoneda)
                : 1m)
        };
        ctx.ComposicionesCaja.Add(comp);
        await ctx.SaveChangesAsync();
        
        foreach (var d in _cajaDetalles)
        {
            d.IdComposicionCajaDetalle = 0; // Asegurar que EF genere un nuevo ID
            d.IdComposicionCaja = comp.IdComposicionCaja;
            ctx.ComposicionesCajaDetalles.Add(d);
        }
        await ctx.SaveChangesAsync();

        // Crear/actualizar VentaPago (E7) usando los detalles de la composición
        var vp = await ctx.VentasPagos
            .Include(x => x.Detalles!)
            .FirstOrDefaultAsync(x => x.IdVenta == Cab.IdVenta);
        if (vp == null)
        {
            vp = new VentaPago
            {
                IdVenta = Cab.IdVenta,
                CondicionOperacion = 1, // contado
                IdMoneda = Cab.IdMoneda,
                TipoCambio = Cab.CambioDelDia,
                ImporteTotal = Cab.Total
            };
            ctx.VentasPagos.Add(vp);
            await ctx.SaveChangesAsync();
        }
        else
        {
            // Resetear cabecera y detalles
            vp.CondicionOperacion = 1;
            vp.IdMoneda = Cab.IdMoneda;
            vp.TipoCambio = Cab.CambioDelDia;
            vp.ImporteTotal = Cab.Total;
            if (vp.Detalles != null && vp.Detalles.Count > 0)
            {
                ctx.VentasPagosDetalles.RemoveRange(vp.Detalles);
                await ctx.SaveChangesAsync();
            }
        }

        foreach (var d in _cajaDetalles)
        {
            var nuevoDet = new VentaPagoDetalle
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
            ctx.VentasPagosDetalles.Add(nuevoDet);
        }
        await ctx.SaveChangesAsync();
        Console.WriteLine($"[Ventas] Composición de caja guardada para venta {Cab.IdVenta}");
    }

    private async Task ConfirmarComposicionCaja()
    {
        _cajaError = null;
        var totalGs = _cajaTotalGs;
        // Convertir el total de la venta a Gs para una comparación coherente
        decimal totalVentaGs = Cab.Total;
        try
        {
            var baseMon = Monedas.FirstOrDefault(m => m.EsMonedaBase) ?? Monedas.FirstOrDefault(m => (m.CodigoISO ?? "").ToUpper() == "PYG");
            if (baseMon != null && Cab.IdMoneda.HasValue)
            {
                var tcVenta = await ObtenerTasaAplicadaAsync(Cab.IdMoneda.Value, baseMon.IdMoneda);
                totalVentaGs = Math.Round(Cab.Total * tcVenta, 4);
            }
        }
        catch { /* usar Cab.Total si falla */ }
        // Permitir vuelto: el total cobrado puede ser >= total venta
        // Solo rechazar si el cobrado es MENOR que la venta
        if (totalGs < totalVentaGs - 0.5m)
        {
            _cajaError = $"El total cobrado (Gs {totalGs:N0}) es menor que el total de la venta (Gs {totalVentaGs:N0}). Falta cobrar Gs {(totalVentaGs - totalGs):N0}.";
            StateHasChanged();
            return;
        }
        
        // Verificar si la venta ya fue guardada (IdVenta > 0) o si es nueva
        if (Cab.IdVenta == 0)
        {
            // Venta nueva: cerrar modal y llamar a GuardarAsync que guardará venta + composición
            _mostrarComposicionCaja = false;
            await GuardarAsync();
            return;
        }
        
        // Venta existente: guardar composición directamente
        await using var ctx = await DbFactory.CreateDbContextAsync();
        
        // Calcular vuelto
        var vuelto = Math.Max(0, _cajaTotalGs - Cab.Total);
        
        var comp = new ComposicionCaja
        {
            IdVenta = Cab.IdVenta,
            Fecha = Cab.Fecha,
            IdMoneda = Cab.IdMoneda,
            MontoTotal = _cajaTotalGs,
            Vuelto = vuelto,
            // Guardar el TC hacia Gs utilizado para comparar totales
            TipoCambioAplicado = (Cab.IdMoneda.HasValue
                ? await ObtenerTasaAplicadaAsync(Cab.IdMoneda.Value, (Monedas.FirstOrDefault(m => m.EsMonedaBase) ?? Monedas.First(m => (m.CodigoISO ?? "").ToUpper() == "PYG")).IdMoneda)
                : 1m)
        };
        ctx.ComposicionesCaja.Add(comp);
        await ctx.SaveChangesAsync();
        foreach (var d in _cajaDetalles)
        {
            d.IdComposicionCaja = comp.IdComposicionCaja;
            ctx.ComposicionesCajaDetalles.Add(d);
        }
        await ctx.SaveChangesAsync();

        // Crear/actualizar VentaPago (E7) usando los detalles de la composición
        var vp = await ctx.VentasPagos
            .Include(x => x.Detalles!)
            .FirstOrDefaultAsync(x => x.IdVenta == Cab.IdVenta);
        if (vp == null)
        {
            vp = new VentaPago
            {
                IdVenta = Cab.IdVenta,
                CondicionOperacion = 1, // contado
                IdMoneda = Cab.IdMoneda,
                TipoCambio = Cab.CambioDelDia,
                ImporteTotal = Cab.Total
            };
            ctx.VentasPagos.Add(vp);
            await ctx.SaveChangesAsync();
        }
        else
        {
            // Resetear cabecera y detalles
            vp.CondicionOperacion = 1;
            vp.IdMoneda = Cab.IdMoneda;
            vp.TipoCambio = Cab.CambioDelDia;
            vp.ImporteTotal = Cab.Total;
            if (vp.Detalles != null && vp.Detalles.Count > 0)
            {
                ctx.VentasPagosDetalles.RemoveRange(vp.Detalles);
                await ctx.SaveChangesAsync();
            }
        }

        foreach (var d in _cajaDetalles)
        {
            var nuevoDet = new VentaPagoDetalle
            {
                IdVentaPago = vp.IdVentaPago,
                Medio = d.Medio,
                IdMoneda = d.IdMoneda,
                TipoCambio = d.TipoCambio,
                Monto = d.Monto,
                MontoGs = d.MontoGs,
                // Copiar metadatos según medio
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
            ctx.VentasPagosDetalles.Add(nuevoDet);
        }
        await ctx.SaveChangesAsync();
        
        // Cerrar modal y preparar nueva venta
        _mostrarComposicionCaja = false;
        
        // Auto-imprimir si la Caja tiene habilitado "Imprimir Facturas"
        var cajaActual = await ObtenerCajaActualAsync(ctx, asNoTracking: true);
        if ((cajaActual?.Imprimir_Factura ?? 0) == 1)
        {
            // Determinar URL según formato configurado
            string url;
            if (cajaActual?.FormatoImpresion == "TICKET")
            {
                // Pasar AutoPrint=1 para impresión automática
                url = $"/ventas/ticket/{Cab.IdVenta}/1";
            }
            else
            {
                // Formato A4 o Matricial
                url = $"/ventas/preview/{Cab.IdVenta}?print=1";
            }
            try { await JS.InvokeVoidAsync("safeOpen", url, "_blank"); } catch { /* ignorar */ }
        }
        
        // Preparar una nueva venta tras confirmar la composición
        Limpiar();
    _cajaInfo = null; _cajaError = null;
        await using (var ctx2 = await DbFactory.CreateDbContextAsync())
        {
            await CargarNumeracionesAsync(ctx2);
        }
        StateHasChanged();
    }

    private Task AgregarDetalleCaja() => AgregarDetalleCajaInternoAsync(preload: false);

    private async Task OnMontoKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            // Si no hay monto ingresado y ya tenemos suficiente cobrado, confirmar
            if (_detalleCajaMonto <= 0 && _cajaDetalles.Count > 0)
            {
                // Verificar si el total cobrado >= total venta
                decimal totalVentaGs = Cab.Total;
                try
                {
                    var baseMon = Monedas.FirstOrDefault(m => m.EsMonedaBase) ?? Monedas.FirstOrDefault(m => (m.CodigoISO ?? "").ToUpper() == "PYG");
                    if (baseMon != null && Cab.IdMoneda.HasValue)
                    {
                        var tcVenta = await ObtenerTasaAplicadaAsync(Cab.IdMoneda.Value, baseMon.IdMoneda);
                        totalVentaGs = Math.Round(Cab.Total * tcVenta, 4);
                    }
                }
                catch { }

                if (_cajaTotalGs >= totalVentaGs - 0.5m)
                {
                    // Ya se cobró lo suficiente, confirmar composición
                    await ConfirmarComposicionCaja();
                    return;
                }
            }

            // Agregar detalle si hay monto
            if (_detalleCajaMonto > 0)
            {
                await AgregarDetalleCajaInternoAsync(preload: false);
                // Después de agregar, hacer focus en el input de monto
                await FocusMontoInputAsync();
            }
        }
    }

    private async Task FocusMontoInputAsync()
    {
        await Task.Delay(50); // pequeño delay para que el DOM se actualice
        try
        {
            await _montoInputRef.FocusAsync();
        }
        catch { }
    }

    private async Task AgregarDetalleCajaInternoAsync(bool preload)
    {
        var mon = Monedas.FirstOrDefault(m => m.IdMoneda == _detalleCajaIdMoneda);
        if (mon == null) return;
        var monto = _detalleCajaMonto;
        if (!preload && monto <= 0)
        {
            _cajaError = "Ingrese un monto mayor a 0 para agregar el detalle.";
            StateHasChanged();
            return;
        }
        // convertir a Gs si corresponde
        decimal tc = 1m;
        try
        {
            var baseMon = Monedas.FirstOrDefault(m => m.EsMonedaBase) ?? Monedas.FirstOrDefault(m => (m.CodigoISO ?? "").ToUpper() == "PYG");
            if (baseMon != null)
            {
                tc = await ObtenerTasaAplicadaAsync(_detalleCajaIdMoneda, baseMon.IdMoneda);
            }
        }
        catch { tc = 1m; }
        var montoGs = Math.Round(monto * tc, 4);
        var medio = _detalleCajaMedio?.ToUpperInvariant() ?? "EFECTIVO";
        if (_cajaEditIndex.HasValue && _cajaEditIndex.Value >= 0 && _cajaEditIndex.Value < _cajaDetalles.Count)
        {
            // Actualizar detalle existente
            var d = _cajaDetalles[_cajaEditIndex.Value];
            d.Medio = medio switch
            {
                var s when s.Contains("TARJETA") => SistemIA.Models.Enums.MedioPago.Tarjeta,
                var s when s.Contains("CHEQUE") => SistemIA.Models.Enums.MedioPago.Cheque,
                var s when s.Contains("TRANSF") => SistemIA.Models.Enums.MedioPago.Transferencia,
                var s when s.Contains("QR") => SistemIA.Models.Enums.MedioPago.QR,
                _ => SistemIA.Models.Enums.MedioPago.Efectivo
            };
            d.IdMoneda = mon.IdMoneda;
            d.TipoCambio = tc;
            d.Monto = monto;
            d.MontoGs = montoGs;
            // Número de comprobante solo para medios distintos de efectivo
            d.NumeroComprobante = d.Medio != SistemIA.Models.Enums.MedioPago.Efectivo ? _detalleCajaComprobante?.Trim() : null;
            _cajaEditIndex = null; // salir de modo edición
        }
        else
        {
            // Agregar nuevo detalle
            var medioEnum = medio switch
            {
                var s when s.Contains("TARJETA") => SistemIA.Models.Enums.MedioPago.Tarjeta,
                var s when s.Contains("CHEQUE") => SistemIA.Models.Enums.MedioPago.Cheque,
                var s when s.Contains("TRANSF") => SistemIA.Models.Enums.MedioPago.Transferencia,
                var s when s.Contains("QR") => SistemIA.Models.Enums.MedioPago.QR,
                _ => SistemIA.Models.Enums.MedioPago.Efectivo
            };
            var d = new ComposicionCajaDetalle
            {
                Medio = medioEnum,
                IdMoneda = mon.IdMoneda,
                TipoCambio = tc,
                Monto = monto,
                MontoGs = montoGs,
                // Número de comprobante solo para medios distintos de efectivo
                NumeroComprobante = medioEnum != SistemIA.Models.Enums.MedioPago.Efectivo ? _detalleCajaComprobante?.Trim() : null
            };
            _cajaDetalles.Add(d);
        }
    _cajaTotalGs = _cajaDetalles.Sum(x => x.MontoGs);
    _cajaError = null;
    _cajaInfo = _cajaEditIndex.HasValue ? "Detalle actualizado" : "Detalle agregado";
        if (!preload)
        {
            _detalleCajaMonto = 0;
            _detalleCajaComprobante = null;
            _detalleCajaMedio = "EFECTIVO"; // Reset al medio por defecto
        }
        StateHasChanged();
    }

    private void QuitarDetalleCaja(int index)
    {
        if (index >= 0 && index < _cajaDetalles.Count)
        {
            _cajaDetalles.RemoveAt(index);
            if (_cajaEditIndex.HasValue && _cajaEditIndex.Value == index)
            {
                _cajaEditIndex = null;
            }
            _cajaTotalGs = _cajaDetalles.Sum(x => x.MontoGs);
            _cajaError = null;
            _cajaInfo = $"Detalle #{index + 1} eliminado";
            StateHasChanged();
        }
    }

    private void EditarDetalleCaja(int index)
    {
        if (index < 0 || index >= _cajaDetalles.Count) return;
        var d = _cajaDetalles[index];
        _cajaEditIndex = index;
        _detalleCajaIdMoneda = d.IdMoneda ?? 0;
        _detalleCajaMonto = d.Monto;
        _detalleCajaMedio = d.Medio switch
        {
            SistemIA.Models.Enums.MedioPago.Tarjeta => "TARJETA",
            SistemIA.Models.Enums.MedioPago.Cheque => "CHEQUE",
            SistemIA.Models.Enums.MedioPago.Transferencia => "TRANSFERENCIA",
            SistemIA.Models.Enums.MedioPago.QR => "QR",
            _ => "EFECTIVO"
        };
        _detalleCajaComprobante = d.NumeroComprobante;
        _cajaError = null;
        _cajaInfo = $"Editando detalle #{index + 1}";
        StateHasChanged();
    }

    private void LimpiarEdicionCaja()
    {
        _cajaEditIndex = null;
        _detalleCajaMonto = 0;
        _detalleCajaComprobante = null;
        _cajaError = null;
        _cajaInfo = "Edición cancelada";
        StateHasChanged();
    }

    protected void OnTipoPagoChanged()
    {
        var id = Cab.IdTipoPago;
        var sel = TiposPago.FirstOrDefault(t => t.IdTipoPago == id);
        Cab.FormaPago = sel?.Nombre;
        MensajeAdvertencia = null;
        
        // Verificar si el tipo de pago es crédito
        TipoPagoEsCredito = sel?.EsCredito ?? false;
        MostrarCamposCredito = TipoPagoEsCredito;
        
        // Valores por defecto para crédito
        if (TipoPagoEsCredito)
        {
            if (NumeroCuotas <= 0)
            {
                NumeroCuotas = 1;
            }
            if (!Cab.Plazo.HasValue || Cab.Plazo.Value <= 0)
            {
                Cab.Plazo = 30; // 30 días por defecto
            }
            if (!Cab.FechaVencimiento.HasValue)
            {
                Cab.FechaVencimiento = Cab.Fecha.AddDays(Cab.Plazo.Value);
            }
        }
        else
        {
            // Si no es crédito, limpiar campos
            NumeroCuotas = 1;
            Cab.Plazo = null;
            Cab.FechaVencimiento = null;
        }
        
        // Si el tipo de pago elegido sugiere el tipo de documento (ej.: 'REMISION'), sincronizamos el Tipo Doc.
        var fp = QuitarDiacriticos(Cab.FormaPago).ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(fp))
        {
            if (fp.Contains("REMISION"))
            {
                var docRem = TiposDocumento.FirstOrDefault(t => QuitarDiacriticos(t.Nombre).ToUpperInvariant().Contains("REMISION") && !QuitarDiacriticos(t.Nombre).ToUpperInvariant().Contains("INTERNA"));
                if (docRem != null)
                {
                    Cab.IdTipoDocumentoOperacion = docRem.IdTipoDocumentoOperacion;
                    Cab.TipoDocumento = docRem.Nombre;
                }
            }
            else if (fp.Contains("FACTURA") || fp.Contains("CONTADO") || fp.Contains("CREDITO"))
            {
                var docFac = TiposDocumento.FirstOrDefault(t => QuitarDiacriticos(t.Nombre).ToUpperInvariant().Contains("FACTURA"));
                if (docFac != null)
                {
                    Cab.IdTipoDocumentoOperacion = docFac.IdTipoDocumentoOperacion;
                    Cab.TipoDocumento = docFac.Nombre;
                }
            }
        }
        // Siempre refresca numeraciones al cambiar tipo de pago, para reflejar doc actual (Factura/Remisión)
        _ = Task.Run(async () =>
        {
            await using var ctx = await DbFactory.CreateDbContextAsync();
            await CargarNumeracionesAsync(ctx);
            await InvokeAsync(StateHasChanged);
        });
        StateHasChanged();
    }

    protected void OnPlazoChanged()
    {
        if (TipoPagoEsCredito && Cab.Plazo.HasValue && Cab.Plazo.Value > 0)
        {
            Cab.FechaVencimiento = Cab.Fecha.AddDays(Cab.Plazo.Value);
        }
    }

    protected void OnFechaVencimientoChanged()
    {
        if (TipoPagoEsCredito && Cab.FechaVencimiento.HasValue)
        {
            var dias = (Cab.FechaVencimiento.Value - Cab.Fecha).Days;
            if (dias > 0)
            {
                Cab.Plazo = dias;
            }
        }
    }

    protected void OnTipoDocumentoChanged()
    {
        var id = Cab.IdTipoDocumentoOperacion;
        var sel = TiposDocumento.FirstOrDefault(t => t.IdTipoDocumentoOperacion == id);
    Cab.TipoDocumento = sel?.Nombre ?? Cab.TipoDocumento ?? "FACTURA";
    MensajeAdvertencia = null;
        _ = Task.Run(async () =>
        {
            await using var ctx = await DbFactory.CreateDbContextAsync();
            await CargarNumeracionesAsync(ctx);
            await InvokeAsync(StateHasChanged);
        });
        StateHasChanged();
    }

    public void Limpiar()
    {
        // Crear una nueva venta en lugar de reutilizar la anterior (evita conflictos con IdVenta)
        var sucursalActual = Cab.IdSucursal;
        var fechaActual = Cab.Fecha;
        var idCajaActual = Cab.IdCaja;
        var turnoActual = Cab.Turno;
        var monedaActual = Cab.IdMoneda;
        var simboloMoneda = Cab.SimboloMoneda;
        var tipoDocActual = Cab.IdTipoDocumentoOperacion;
        var tipoDocNombre = Cab.TipoDocumento;
        
        Cab = new Venta
        {
            IdSucursal = sucursalActual,
            Fecha = fechaActual,
            IdCaja = idCajaActual,
            Turno = turnoActual,
            IdMoneda = monedaActual,
            SimboloMoneda = simboloMoneda,
            CambioDelDia = 1m,
            EsMonedaExtranjera = false,
            IdTipoDocumentoOperacion = tipoDocActual,
            TipoDocumento = tipoDocNombre
        };
        
        // Volver a tipo de pago CONTADO por defecto
        var contadoPred = TiposPago.FirstOrDefault(t => t.Nombre != null && t.Nombre.ToUpper().Contains("CONTADO") && t.Nombre.ToUpper().Contains("PREDETERMINADO"));
        var contado = contadoPred ?? TiposPago.FirstOrDefault(t => t.Nombre != null && t.Nombre.ToUpper().Contains("CONTADO"));
        var primero = contado ?? TiposPago.FirstOrDefault();
        if (primero != null)
        {
            Cab.IdTipoPago = primero.IdTipoPago;
            OnTipoPagoChanged();
        }
        
        Detalles.Clear();
        _cajaDetalles.Clear(); // Limpiar detalles de composición de caja
        _cajaTotalGs = 0;
        RecalcularTotales();
        NuevoDetalle = new VentaDetalle { Cantidad = 1, PrecioUnitario = 0 };
        BuscarProducto = string.Empty;
        MostrarSugProductos = false;
        ValidezDias = null; 
        ValidoHasta = null;
        MensajeAdvertencia = null;
        NumeroCuotas = 1;
        MostrarCamposCredito = false;
        TipoPagoEsCredito = false;
        ProductoImagenUrl = null; // Limpiar imagen del producto
        
        // Resetear campos de paquete/unidad
        ModoIngresoVenta = "paquete";
        _cantidadIngresadaVenta = 1;
        _cantidadPorPaqueteActual = null;
        _productoPermiteVentaPorUnidad = true;
        _nombreUnidadVenta = "Unidad";
        
        // Resetear campos de cliente
        ClienteSeleccionadoLabel = string.Empty;
        EmailCliente = null;
        BuscarCliente = string.Empty;
        ClientesFiltrados = new(Clientes);
        ProductosFiltrados = new(Productos);
        
        // Refrescar productos para actualizar stock después de la venta
        _ = RefrescarProductosAsync();
    }
    
    private async Task RefrescarProductosAsync()
    {
        try
        {
            await using var ctx = await DbFactory.CreateDbContextAsync();
            await RecargarProductosConStockAsync(ctx);
            await InvokeAsync(StateHasChanged);
        }
        catch { /* Ignorar errores de refresco */ }
    }
    
    private async Task RecargarProductosConStockAsync(AppDbContext ctx)
    {
        Productos = await ctx.Productos
            .Include(p => p.TipoIva)
            .Include(p => p.MonedaPrecio)
            .AsNoTracking()
            .OrderBy(p => p.Descripcion)
            .ToListAsync();
        
        // Cargar stocks SOLO del depósito principal (IdDeposito = 1)
        _stocksPorProducto = await ctx.ProductosDepositos
            .Where(pd => pd.IdDeposito == 1)
            .ToDictionaryAsync(pd => pd.IdProducto, pd => pd.Stock);
        
        // FIX 2026-01-19: Para productos con ControlaLote, solo contar lotes CON fecha de vencimiento válida
        // Lotes sin fecha = no vendibles hasta cargar la fecha
        var hoy = DateTime.Today;
        var stocksLotes = await ctx.ProductosLotes
            .Where(l => l.IdDeposito == 1 
                     && l.Estado == "Activo" 
                     && l.Stock > 0
                     && l.FechaVencimiento.HasValue          // DEBE tener fecha de vencimiento
                     && l.FechaVencimiento.Value >= hoy)     // Y no estar vencido
            .GroupBy(l => l.IdProducto)
            .Select(g => new { IdProducto = g.Key, TotalStock = g.Sum(l => l.Stock) })
            .ToDictionaryAsync(x => x.IdProducto, x => x.TotalStock);
        
        foreach (var p in Productos)
        {
            if (p.ControlaLote)
            {
                // Producto con lotes: solo stock de lotes CON fecha de vencimiento válida
                p.Stock = stocksLotes.TryGetValue(p.IdProducto, out var stockLotes) ? stockLotes : 0;
            }
            else
            {
                // Producto sin lotes: usar stock general de ProductosDepositos
                p.Stock = _stocksPorProducto.TryGetValue(p.IdProducto, out var st) ? st : 0;
            }
        }
        
        ProductosFiltrados = new(Productos);
    }

    private string NumeroEnLetras(decimal total, string codigoMoneda)
    {
        var entero = Math.Floor(total);
        var centavos = (int)Math.Round((total - entero) * 100, 0);
        var palabras = ToSpanishWords((long)entero);
        var nombreMoneda = codigoMoneda switch
        {
            "USD" => "dólares",
            "ARS" => "pesos argentinos",
            "BRL" => "reales",
            _ => "guaraníes"
        };
        return centavos > 0
            ? $"{palabras} {nombreMoneda} con {centavos:00}/100"
            : $"{palabras} {nombreMoneda}";
    }

    private static string ToSpanishWords(long number)
    {
        if (number == 0) return "cero";
        if (number < 0) return "menos " + ToSpanishWords(-number);
        string[] unidades = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve", "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve" };
        string[] decenas = { "", "diez", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
        string[] centenas = { "", "cien", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos" };
        string ConvertGroup(long n)
        {
            if (n == 0) return "";
            if (n < 20) return unidades[n];
            if (n < 100)
            {
                var d = n / 10; var u = n % 10;
                if (n < 30) return n == 20 ? "veinte" : "veinti" + unidades[u];
                return u == 0 ? decenas[d] : decenas[d] + " y " + unidades[u];
            }
            var c = n / 100; var r = n % 100;
            if (n == 100) return "cien";
            return centenas[c] + (r == 0 ? "" : " " + ConvertGroup(r));
        }
        string ConvertThousands(long n)
        {
            if (n < 1000) return ConvertGroup(n);
            var miles = n / 1000; var resto = n % 1000;
            var pref = miles == 1 ? "mil" : ConvertGroup(miles) + " mil";
            var suf = resto == 0 ? "" : " " + ConvertGroup(resto);
            return pref + suf;
        }
        string ConvertMillions(long n)
        {
            if (n < 1_000_000) return ConvertThousands(n);
            var mill = n / 1_000_000; var resto = n % 1_000_000;
            var pref = mill == 1 ? "un millón" : ConvertThousands(mill) + " millones";
            var suf = resto == 0 ? "" : " " + ConvertThousands(resto);
            return pref + suf;
        }
        string ConvertBillions(long n)
        {
            if (n < 1_000_000_000) return ConvertMillions(n);
            var bill = n / 1_000_000_000; var resto = n % 1_000_000_000;
            var pref = bill == 1 ? "mil millones" : ConvertMillions(bill) + " mil millones";
            var suf = resto == 0 ? "" : " " + ConvertMillions(resto);
            return pref + suf;
        }
        return ConvertBillions(number);
    }

    protected string GetCurrencyFlag(string? codigoISO)
    {
        switch ((codigoISO ?? string.Empty).ToUpperInvariant())
        {
            case "USD": return "\U0001F1FA\U0001F1F8"; // 🇺🇸
            case "ARS": return "\U0001F1E6\U0001F1F7"; // 🇦🇷
            case "BRL": return "\U0001F1E7\U0001F1F7"; // 🇧🇷
            case "PYG": return "\U0001F1F5\U0001F1FE"; // 🇵🇾
            case "EUR": return "\U0001F1EA\U0001F1FA"; // 🇪🇺
            default: return "\U0001F4B1"; // 💱
        }
    }

    private async Task CargarPresupuesto(int idPresupuesto)
    {
        await using var ctx = await DbFactory.CreateDbContextAsync();
        
        // Cargar presupuesto con sus relaciones
        var presupuesto = await ctx.Presupuestos
            .Include(p => p.Cliente)
            .Include(p => p.Moneda)
            .FirstOrDefaultAsync(p => p.IdPresupuesto == idPresupuesto);
            
        if (presupuesto == null) return;
        
        // Verificar si ya fue convertido
        if (presupuesto.IdVentaConvertida.HasValue)
        {
            await JS.InvokeVoidAsync("alert", "Este presupuesto ya fue convertido a venta.");
            return;
        }
        
        // Cargar detalles del presupuesto con información mínima necesaria
        var detallesPresupuesto = await ctx.PresupuestosDetalles
            .Include(d => d.Producto)
            .Where(d => d.IdPresupuesto == idPresupuesto)
            .ToListAsync();
            
        // Copiar datos de cabecera
        Cab.IdCliente = presupuesto.IdCliente;
        Cab.IdMoneda = presupuesto.IdMoneda;
        Cab.SimboloMoneda = presupuesto.SimboloMoneda;
        Cab.CambioDelDia = presupuesto.CambioDelDia;
        Cab.EsMonedaExtranjera = presupuesto.EsMonedaExtranjera;
        Cab.Fecha = DateTime.Now; // Fecha actual para la nueva venta
        Cab.TipoIngreso = "VENTAS"; // Forzar tipo VENTAS
        Cab.NroPedido = presupuesto.NumeroPresupuesto.ToString(); // Agregar número de presupuesto como referencia
        
        // Agregar detalles sin incluir entidades de navegación que puedan causar tracking
        Detalles.Clear();
        foreach (var det in detallesPresupuesto)
        {
            var nuevoDetalle = new VentaDetalle
            {
                IdProducto = det.IdProducto,
                // NO asignar Producto para evitar tracking de entidades relacionadas
                Cantidad = det.Cantidad,
                PrecioUnitario = det.PrecioUnitario,
                Importe = det.Importe,
                IVA10 = det.IVA10,
                IVA5 = det.IVA5,
                Exenta = det.Exenta,
                Grabado10 = det.Grabado10,
                Grabado5 = det.Grabado5,
                CambioDelDia = det.CambioDelDia,
                IdTipoIva = det.Producto.IdTipoIva,
                // Guardar costo al momento de la venta para informes
                CostoUnitario = det.Producto.CostoUnitarioGs,
                // Guardar precio ministerio si aplica
                PrecioMinisterio = det.Producto.PrecioMinisterio
            };
            Detalles.Add(nuevoDetalle);
        }
        
        // Recalcular totales
        RecalcularTotales();
        
        // Guardar referencia al presupuesto para marcarlo como convertido al guardar
        Cab.IdPresupuestoOrigen = idPresupuesto;
        
        StateHasChanged();
    }

    /// <summary>
    /// Carga una venta existente para visualización (modo solo vista).
    /// No permite modificaciones.
    /// </summary>
    private async Task CargarVentaExistente(int idVenta)
    {
        await using var ctx = await DbFactory.CreateDbContextAsync();
        
        // Cargar venta con sus relaciones
        var venta = await ctx.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Moneda)
            .Include(v => v.TipoPago)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta);
            
        if (venta == null)
        {
            await JS.InvokeVoidAsync("alert", "Venta no encontrada.");
            return;
        }
        
        // Cargar detalles de la venta
        var detallesVenta = await ctx.VentasDetalles
            .Include(d => d.Producto)
            .Where(d => d.IdVenta == idVenta)
            .ToListAsync();
        
        // Cargar los datos en Cab (cabecera)
        Cab.IdVenta = venta.IdVenta;
        Cab.IdSucursal = venta.IdSucursal;
        Cab.IdCliente = venta.IdCliente;
        Cab.IdMoneda = venta.IdMoneda;
        Cab.SimboloMoneda = venta.SimboloMoneda;
        Cab.CambioDelDia = venta.CambioDelDia;
        Cab.EsMonedaExtranjera = venta.EsMonedaExtranjera;
        Cab.Fecha = venta.Fecha;
        Cab.TipoIngreso = venta.TipoIngreso;
        Cab.TipoDocumento = venta.TipoDocumento;
        Cab.IdTipoDocumentoOperacion = venta.IdTipoDocumentoOperacion;
        Cab.IdTipoPago = venta.IdTipoPago;
        Cab.Establecimiento = venta.Establecimiento;
        Cab.PuntoExpedicion = venta.PuntoExpedicion;
        Cab.NumeroFactura = venta.NumeroFactura;
        Cab.Timbrado = venta.Timbrado;
        Cab.Total = venta.Total;
        Cab.Estado = venta.Estado;
        Cab.EstadoSifen = venta.EstadoSifen;
        Cab.CDC = venta.CDC;
        Cab.IdLote = venta.IdLote;
        Cab.FechaVencimiento = venta.FechaVencimiento;
        Cab.Plazo = venta.Plazo;
        
        // Actualizar UI de cliente
        if (venta.Cliente != null)
        {
            ClienteSeleccionadoLabel = $"{venta.Cliente.RazonSocial} - RUC: {venta.Cliente.RUC}-{venta.Cliente.DV}";
            EmailCliente = venta.Cliente.Email;
        }
        
        // Actualizar campos UI
        EstablecimientoUI = venta.Establecimiento;
        PuntoExpedicionUI = venta.PuntoExpedicion;
        NumeroSecuencialUI = venta.NumeroFactura;
        TimbradoUI = venta.Timbrado;
        NumeroFacturaUI = $"{venta.Establecimiento}-{venta.PuntoExpedicion}-{venta.NumeroFactura}";
        FechaCajaUI = venta.Fecha.ToString("dd/MM/yyyy");
        
        // Verificar tipo de pago para mostrar campos de crédito
        if (venta.IdTipoPago.HasValue)
        {
            var tipoPago = TiposPago.FirstOrDefault(t => t.IdTipoPago == venta.IdTipoPago.Value);
            if (tipoPago != null)
            {
                TipoPagoEsCredito = tipoPago.Nombre?.ToUpper().Contains("CREDITO") == true
                                 || tipoPago.Nombre?.ToUpper().Contains("CRÉDITO") == true;
                MostrarCamposCredito = TipoPagoEsCredito;
            }
        }
        
        // Cargar detalles
        Detalles.Clear();
        foreach (var det in detallesVenta)
        {
            // Restaurar campos NotMapped desde los persistidos
            det.ModoIngreso = det.ModoIngresoPersistido ?? "unidad";
            det.CantidadPorPaquete = det.CantidadPorPaqueteMomento ?? det.Producto?.CantidadPorPaquete;
            det.PermiteVentaPorUnidad = det.Producto?.PermiteVentaPorUnidad ?? true;
            det.PermiteDecimal = det.Producto?.PermiteDecimal ?? false;
            
            // Calcular cantidad ingresada original si fue por paquete
            if (det.ModoIngresoPersistido == "paquete" && det.CantidadPorPaqueteMomento.HasValue && det.CantidadPorPaqueteMomento > 0)
                det.CantidadIngresada = det.Cantidad / det.CantidadPorPaqueteMomento.Value;
            else
                det.CantidadIngresada = det.Cantidad;
            
            Detalles.Add(det);
        }
        
        // Recalcular totales
        RecalcularTotales();
        
        StateHasChanged();
    }

    // ============ Métodos para modal de nuevo cliente rápido ============
    private void AbrirModalNuevoCliente()
    {
        _mostrarModalCliente = true;
        _errorCliente = null;
        _successCliente = null;
        _nuevoCliRazonSocial = string.Empty;
        _nuevoCliRuc = string.Empty;
        _nuevoCliDv = 0;
        _nuevoCliTelefono = string.Empty;
        _nuevoCliEmail = string.Empty;
        _nuevoCliTipoDocumento = "RU"; // RUC por defecto
        _mensajeSifenCliente = null;
        _esSifenClienteError = false;
    }

    private void CerrarModalCliente()
    {
        _mostrarModalCliente = false;
    }

    private void CalcularDvCliente()
    {
        if (!string.IsNullOrWhiteSpace(_nuevoCliRuc) && _nuevoCliRuc.All(char.IsDigit))
        {
            _nuevoCliDv = Utils.RucHelper.CalcularDvRuc(_nuevoCliRuc);
        }
        else
        {
            _nuevoCliDv = 0;
        }
    }

    private async Task BuscarClienteEnSifenAsync()
    {
        if (string.IsNullOrWhiteSpace(_nuevoCliRuc) || _nuevoCliRuc.Length < 5)
        {
            _mensajeSifenCliente = "Ingrese un RUC válido (mínimo 5 dígitos)";
            _esSifenClienteError = true;
            return;
        }

        _buscandoClienteSifen = true;
        _mensajeSifenCliente = null;
        _esSifenClienteError = false;
        StateHasChanged();

        try
        {
            // Primero buscar en BD local
            await using var ctx = await DbFactory.CreateDbContextAsync();
            var clienteLocal = await ctx.Clientes.FirstOrDefaultAsync(c => c.RUC == _nuevoCliRuc);
            if (clienteLocal != null)
            {
                _nuevoCliRazonSocial = clienteLocal.RazonSocial ?? string.Empty;
                _nuevoCliDv = clienteLocal.DV;
                _nuevoCliTelefono = clienteLocal.Telefono ?? string.Empty;
                _nuevoCliEmail = clienteLocal.Email ?? string.Empty;
                _mensajeSifenCliente = $"✓ Cliente encontrado en BD local: {clienteLocal.RazonSocial}";
                _esSifenClienteError = false;
                return;
            }

            // Calcular DV
            _nuevoCliDv = Utils.RucHelper.CalcularDvRuc(_nuevoCliRuc);

            // Si no está en BD local, consultar SIFEN
            var sociedad = await ctx.Sociedades.FirstOrDefaultAsync();
            if (sociedad == null || string.IsNullOrEmpty(sociedad.PathCertificadoP12))
            {
                _mensajeSifenCliente = "⚠️ No hay certificado SIFEN configurado";
                _esSifenClienteError = true;
                return;
            }

            var url = sociedad.DeUrlConsultaRuc ?? SifenConfig.GetConsultaRucUrl("test");
            // Asegurar que URL termine en .wsdl (requerido por SIFEN)
            if (!string.IsNullOrWhiteSpace(url) && !url.EndsWith(".wsdl", StringComparison.OrdinalIgnoreCase))
            {
                url = url + ".wsdl";
            }
            var xmlRespuesta = await SifenService.Consulta(url, _nuevoCliRuc, "1", sociedad.PathCertificadoP12 ?? "", sociedad.PasswordCertificadoP12 ?? "");

            if (string.IsNullOrEmpty(xmlRespuesta))
            {
                _mensajeSifenCliente = "⚠️ Sin respuesta de SIFEN. Intente nuevamente.";
                _esSifenClienteError = true;
                return;
            }

            if (xmlRespuesta.Contains("0160") || xmlRespuesta.Contains("XML Mal Formado"))
            {
                _mensajeSifenCliente = "⚠️ Error de conexión con SIFEN. Intente nuevamente.";
                _esSifenClienteError = true;
                return;
            }

            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(xmlRespuesta);

            var codRes = xmlDoc.SelectSingleNode("//*[local-name()='dCodRes']")?.InnerText ?? "";
            var msgRes = xmlDoc.SelectSingleNode("//*[local-name()='dMsgRes']")?.InnerText ?? "";

            if (codRes == "0502")
            {
                var xContRUC = xmlDoc.SelectSingleNode("//*[local-name()='xContRUC']");
                if (xContRUC != null)
                {
                    var dRazCons = xContRUC.SelectSingleNode(".//*[local-name()='dRazCons']")?.InnerText;
                    var dDesEstCons = xContRUC.SelectSingleNode(".//*[local-name()='dDesEstCons']")?.InnerText;
                    
                    if (!string.IsNullOrEmpty(dRazCons))
                    {
                        _nuevoCliRazonSocial = dRazCons.Trim();
                    }
                    
                    _mensajeSifenCliente = $"✓ SIFEN: {dRazCons?.Trim()} - {dDesEstCons}";
                    _esSifenClienteError = false;
                }
            }
            else if (codRes == "0501")
            {
                _mensajeSifenCliente = "RUC no encontrado en SIFEN. Puede ingresarlo manualmente.";
                _esSifenClienteError = true;
            }
            else
            {
                _mensajeSifenCliente = $"SIFEN: [{codRes}] {msgRes}";
                _esSifenClienteError = codRes != "0500";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR BuscarClienteEnSifenAsync] {ex.Message}");
            _mensajeSifenCliente = "⚠️ Error de conexión. Intente nuevamente.";
            _esSifenClienteError = true;
        }
        finally
        {
            _buscandoClienteSifen = false;
            StateHasChanged();
        }
    }

    private async Task GuardarNuevoClienteAsync()
    {
        _errorCliente = null;
        _successCliente = null;

        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(_nuevoCliRazonSocial))
        {
            _errorCliente = "La Razón Social es obligatoria.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_nuevoCliRuc) || !_nuevoCliRuc.All(char.IsDigit) || _nuevoCliRuc.Length < 5)
        {
            _errorCliente = "El RUC/CI debe contener al menos 5 dígitos numéricos.";
            return;
        }

        _guardandoCliente = true;
        StateHasChanged();

        try
        {
            await using var ctx = await DbFactory.CreateDbContextAsync();

            // Verificar si ya existe el RUC
            var existe = await ctx.Clientes.AnyAsync(c => c.RUC == _nuevoCliRuc);
            if (existe)
            {
                _errorCliente = "Ya existe un cliente con este RUC/CI.";
                _guardandoCliente = false;
                return;
            }

            // Calcular DV si no se ha calculado
            if (_nuevoCliDv == 0)
            {
                _nuevoCliDv = Utils.RucHelper.CalcularDvRuc(_nuevoCliRuc);
            }

            // Usar transacción para evitar códigos duplicados en concurrencia
            using var transaction = await ctx.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                // Generar código de cliente automático
                var maxCodigo = await ctx.Clientes
                    .Where(c => c.CodigoCliente != null)
                    .Select(c => c.CodigoCliente)
                    .OrderByDescending(c => c)
                    .FirstOrDefaultAsync();
                
                int siguienteCodigo = 1;
                if (!string.IsNullOrEmpty(maxCodigo) && int.TryParse(maxCodigo, out var ultimoCodigo))
                {
                    siguienteCodigo = ultimoCodigo + 1;
                }

                // Crear el nuevo cliente con campos mínimos y valores por defecto
                var nuevoCliente = new Cliente
                {
                    CodigoCliente = siguienteCodigo.ToString().PadLeft(6, '0'),
                RazonSocial = _nuevoCliRazonSocial.Trim(),
                RUC = _nuevoCliRuc.Trim(),
                DV = _nuevoCliDv,
                NumeroDocumento = _nuevoCliRuc.Trim(), // Requerido por BD
                TipoDocumento = _nuevoCliTipoDocumento, // Tipo de documento seleccionado
                Telefono = string.IsNullOrWhiteSpace(_nuevoCliTelefono) ? null : _nuevoCliTelefono.Trim(),
                Email = string.IsNullOrWhiteSpace(_nuevoCliEmail) ? null : _nuevoCliEmail.Trim(),
                Estado = true,
                FechaAlta = DateTime.Now,
                // Campos requeridos con valores por defecto
                CodigoPais = "PRY", // Paraguay por defecto
                NaturalezaReceptor = 1, // 1 = Contribuyente
                IdTipoContribuyente = 1, // 1 = Persona Física o Empresa por defecto
                // TipoOperacion según RUC: >= 50M = B2B (empresas/extranjeros), < 50M = B2C (clientes)
                TipoOperacion = (long.TryParse(_nuevoCliRuc.Trim(), out var rucNum) && rucNum >= 50_000_000) ? "1" : "2",
                Saldo = 0,
                PermiteCredito = false,
                PrecioDiferenciado = false,
                EsExtranjero = false,
                IdCiudad = 1 // Asunción por defecto
            };

                ctx.Clientes.Add(nuevoCliente);
                await ctx.SaveChangesAsync();
                await transaction.CommitAsync();

            _successCliente = $"Cliente '{_nuevoCliRazonSocial}' creado exitosamente.";
            
            // Agregar a la lista local y seleccionar
            Clientes.Add(nuevoCliente);
            ClientesFiltrados = new List<Cliente>(Clientes);

            // Seleccionar automáticamente el nuevo cliente
            Cab.IdCliente = nuevoCliente.IdCliente;
            ClienteSeleccionadoLabel = $"{nuevoCliente.RazonSocial} (RUC {nuevoCliente.RUC}-{nuevoCliente.DV})";
            EmailCliente = nuevoCliente.Email;
            BuscarCliente = string.Empty;
            MostrarSugClientes = false;

            // Cerrar modal después de un momento
            await Task.Delay(1000);
            _mostrarModalCliente = false;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _errorCliente = $"Error al guardar: {ex.Message}";
        }
        finally
        {
            _guardandoCliente = false;
            StateHasChanged();
        }
    }
}
