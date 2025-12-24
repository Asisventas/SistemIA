using Microsoft.AspNetCore.Mvc;
using SistemIA.Services;
using System.Runtime.Versioning;

namespace SistemIA.Controllers;

/// <summary>
/// Controlador para impresión directa sin diálogo.
/// </summary>
[ApiController]
[Route("api/impresion")]
[SupportedOSPlatform("windows")]
public class ImpresionController : ControllerBase
{
    private readonly ImpresionDirectaService _impresionService;
    private readonly ILogger<ImpresionController> _logger;

    public ImpresionController(ImpresionDirectaService impresionService, ILogger<ImpresionController> logger)
    {
        _impresionService = impresionService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene la lista de impresoras instaladas.
    /// </summary>
    [HttpGet("impresoras")]
    public ActionResult<List<string>> ObtenerImpresoras()
    {
        try
        {
            var impresoras = _impresionService.ObtenerImpresoras();
            return Ok(impresoras);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener impresoras");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene la impresora predeterminada.
    /// </summary>
    [HttpGet("impresora-predeterminada")]
    public ActionResult<string> ObtenerImpresoraPredeterminada()
    {
        try
        {
            var impresora = _impresionService.ObtenerImpresoraPredeterminada();
            return Ok(new { impresora });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener impresora predeterminada");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Imprime un ticket en formato 80mm.
    /// </summary>
    [HttpPost("ticket-80mm")]
    public async Task<ActionResult<ResultadoImpresion>> ImprimirTicket80mm(
        [FromBody] DatosTicket ticket,
        [FromQuery] string? impresora = null)
    {
        try
        {
            var resultado = await _impresionService.ImprimirTicket80mm(ticket, impresora);
            if (resultado.Exitoso)
                return Ok(resultado);
            else
                return BadRequest(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al imprimir ticket 80mm");
            return StatusCode(500, new ResultadoImpresion 
            { 
                Exitoso = false, 
                Mensaje = ex.Message 
            });
        }
    }

    /// <summary>
    /// Imprime una factura en formato A4.
    /// </summary>
    [HttpPost("factura-a4")]
    public async Task<ActionResult<ResultadoImpresion>> ImprimirFacturaA4(
        [FromBody] DatosTicket ticket,
        [FromQuery] string? impresora = null)
    {
        try
        {
            var resultado = await _impresionService.ImprimirFacturaA4(ticket, impresora);
            if (resultado.Exitoso)
                return Ok(resultado);
            else
                return BadRequest(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al imprimir factura A4");
            return StatusCode(500, new ResultadoImpresion 
            { 
                Exitoso = false, 
                Mensaje = ex.Message 
            });
        }
    }
}
