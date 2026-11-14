using BibliotecaApi.Datos;
using BibliotecaApi.DTOs;
using BibliotecaApi.Entidades;
using BibliotecaApi.Servicios;
using BibliotecaApi.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Controllers.V1
{
    [ApiController]
    [Route("api/v1/restriccionesdominio")]
    [Authorize]
    [DeshabilitarLimitarPeticiones]
    public class RestriccionesDominioController : ControllerBase
    {
        private readonly AplicationDbContext context;
        private readonly IServiciosUsuarios serviciosUsuarios;

        public RestriccionesDominioController(AplicationDbContext context, IServiciosUsuarios serviciosUsuarios)
        {
            this.context = context;
            this.serviciosUsuarios = serviciosUsuarios;
        }

        [HttpPost]
        public async Task<ActionResult> Post(RestriccionDominioCreacionDTO restriccionDominioCreacionDTO)
        {
            var llaveDb = await context.LlavesAPI.FirstOrDefaultAsync(x => x.Id == restriccionDominioCreacionDTO.LlaveId);

            if (llaveDb is null)
            {
                return NotFound();
            }

            var usuarioID = serviciosUsuarios.ObtenerUsuarioId();

            if(llaveDb.UsuarioId != usuarioID)
            {
                return Forbid();
            }

            var restriccionDominio = new RestriccionDominio
            {
                LlaveId = restriccionDominioCreacionDTO.LlaveId,
                Dominio = restriccionDominioCreacionDTO.Dominio
            };

            context.Add(restriccionDominio);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, RestriccionDominioActualizacionDTO restriccionDominioActualizacionDTO)
        {
            var restriccionDB = await context.RestriccionDominio.Include(x => x.Llave).FirstOrDefaultAsync(x => x.Id == id);

            if (restriccionDB is null)
            {
                return NotFound();
            }

            var usuarioID = serviciosUsuarios.ObtenerUsuarioId();

            if (restriccionDB.Llave!.UsuarioId != usuarioID)
            {
                return Forbid();
            }

            restriccionDB.Dominio = restriccionDominioActualizacionDTO.Dominio;

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            var restriccionDB = await context.RestriccionDominio.Include(x => x.Llave).FirstOrDefaultAsync(x => x.Id == id);

            if (restriccionDB is null)
            {
                return NotFound();
            }

            var usuarioID = serviciosUsuarios.ObtenerUsuarioId();

            if (restriccionDB.Llave!.UsuarioId != usuarioID)
            {
                return Forbid();
            }

            context.Remove(restriccionDB);

            await context.SaveChangesAsync();
            return NoContent();
        }

    }
}
