using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemIA.Models;
using SistemIA.Services;

namespace SistemIA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly PdfFacturaService _pdfService;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public PdfController(PdfFacturaService pdfService, IDbContextFactory<AppDbContext> dbFactory)
        {
            _pdfService = pdfService;
            _dbFactory = dbFactory;
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

        /// <summary>
        /// Genera una página HTML para imprimir como PDF el presupuesto del sistema
        /// </summary>
        [HttpGet("presupuesto-sistema/{id}")]
        public async Task<IActionResult> PresupuestoSistemaPdf(int id)
        {
            await using var ctx = await _dbFactory.CreateDbContextAsync();
            
            var presupuesto = await ctx.PresupuestosSistema.FindAsync(id);
            if (presupuesto == null)
                return NotFound(new { message = $"Presupuesto #{id} no encontrado" });

            // Redirigir a la página Razor que renderiza el presupuesto
            // Los datos de sucursal se cargan en la página
            return Redirect($"/presupuesto-sistema-pdf/{id}");
        }

        /// <summary>
        /// Descarga el PDF del presupuesto del sistema generado con QuestPDF
        /// </summary>
        [HttpGet("presupuesto-sistema/{id}/download")]
        public async Task<IActionResult> DescargarPresupuestoSistemaPdf(int id, [FromServices] PdfPresupuestoSistemaService pdfService)
        {
            try
            {
                var pdfBytes = await pdfService.GenerarPdfPresupuesto(id);
                var fileName = $"Presupuesto_{id}_{DateTime.Now:yyyyMMdd}.pdf";
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
    }
}