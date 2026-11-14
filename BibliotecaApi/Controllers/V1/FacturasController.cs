using BibliotecaApi.Datos;
using BibliotecaApi.DTOs;
using BibliotecaApi.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Controllers.V1
{
    [Route("api/v1/facturas")]
    [ApiController]
    [Authorize]
    [DeshabilitarLimitarPeticiones]
    public class FacturasController : ControllerBase
    {
        private readonly AplicationDbContext context;

        public FacturasController(AplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Pagar(FacturaPagarDTO facturaPagarDTO)
        {
            var facturaDb = await context.Facturas
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.Id == facturaPagarDTO.FacturaId);

            if(facturaDb == null)
            {
                return NotFound();
            }

            if (facturaDb.Pagada)
            {
                ModelState.AddModelError(nameof(facturaPagarDTO.FacturaId), "La factura ya fue pagada");
                return ValidationProblem();
            }

            facturaDb.Pagada = true;
            await context.SaveChangesAsync();

            var hayFacturasPendientesVencidas = await context.Facturas
                .AnyAsync(x => x.UsuarioId == facturaDb.UsuarioId &&
                !x.Pagada && x.FechaLimiteDePago < DateTime.Today);

            if (!hayFacturasPendientesVencidas)
            {
                facturaDb.Usuario!.MalaPaga = false;
                await context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
