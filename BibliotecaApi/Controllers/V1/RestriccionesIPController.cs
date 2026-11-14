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
    [Route("api/v1/restriccionip")]
    [Authorize]
    [DeshabilitarLimitarPeticiones]
    public class RestriccionesIPController : ControllerBase
    {
        private readonly AplicationDbContext context;
        private readonly IServiciosUsuarios serviciosUsuarios;

        public RestriccionesIPController(AplicationDbContext context, IServiciosUsuarios serviciosUsuarios)
        {
            this.context = context;
            this.serviciosUsuarios = serviciosUsuarios;
        }

        [HttpPost]
        public async Task<ActionResult> Post(RestriccionIPCreacionDTO restriccionIPCreacionDTO)
        {
            var llaveDb = await context.LlavesAPI.FirstOrDefaultAsync(x => x.Id == restriccionIPCreacionDTO.LlaveId);

            if (llaveDb is null)
            {
                return NotFound();
            }

            var usuarioID = serviciosUsuarios.ObtenerUsuarioId();

            if (llaveDb.UsuarioId != usuarioID)
            {
                return Forbid();
            }

            var restriccionIP = new RestriccionIP
            {
                LlaveId = restriccionIPCreacionDTO.LlaveId,
                IP = restriccionIPCreacionDTO.IP
            };

            context.Add(restriccionIP);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, RestriccionIPActualizacionDTO restriccionIPActualizacionDTO)
        {
            var restriccionDB = await context.RestriccionIP.Include(x => x.Llave).FirstOrDefaultAsync(x => x.Id == id);

            if (restriccionDB is null)
            {
                return NotFound();
            }

            var usuarioID = serviciosUsuarios.ObtenerUsuarioId();

            if (restriccionDB.Llave!.UsuarioId != usuarioID)
            {
                return Forbid();
            }

            restriccionDB.IP = restriccionIPActualizacionDTO.IP;

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            var restriccionDB = await context.RestriccionIP.Include(x => x.Llave).FirstOrDefaultAsync(x => x.Id == id);

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
