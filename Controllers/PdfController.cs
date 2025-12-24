using Microsoft.AspNetCore.Mvc;
using SistemIA.Services;

namespace SistemIA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly PdfFacturaService _pdfService;

        public PdfController(PdfFacturaService pdfService)
        {
            _pdfService = pdfService;
        }

        [HttpGet("factura/{idVenta}")]
        public async Task<IActionResult> DescargarFacturaPdf(int idVenta)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerarPdfFactura(idVenta);
                
                var fileName = $"Factura_{idVenta}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar el PDF", details = ex.Message });
            }
        }

        [HttpGet("factura/{idVenta}/preview")]
        public async Task<IActionResult> VisualizarFacturaPdf(int idVenta)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerarPdfFactura(idVenta);
                
                // Para visualización en el navegador (no descarga)
                return File(pdfBytes, "application/pdf");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar el PDF", details = ex.Message });
            }
        }
    }
}