using AutoMapper;
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
    [Route("api/v1/llavesapi")]
    [Authorize]
    [ApiController]
    [DeshabilitarLimitarPeticiones]
    public class LlaveAPIController : ControllerBase
    {
        private readonly AplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IServicioLlaves servicioLlaves;
        private readonly IServiciosUsuarios serviciosUsuarios;

        public LlaveAPIController(AplicationDbContext context,
            IMapper mapper, IServicioLlaves servicioLlaves, IServiciosUsuarios serviciosUsuarios)
        {
            this.context = context;
            this.mapper = mapper;
            this.servicioLlaves = servicioLlaves;
            this.serviciosUsuarios = serviciosUsuarios;
        }

        [HttpGet]
        public async Task<IEnumerable<LlaveDTO>> Get()
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var llaves = await context.LlavesAPI
                .Include(x => x.RestriccionesDominio)
                .Where(x => x.UsuarioId == usuarioId).ToListAsync();
            return mapper.Map<IEnumerable<LlaveDTO>>(llaves);
        }

        [HttpGet("{id:int}", Name = "ObtenerLlavesV1")]
        public async Task<ActionResult<LlaveDTO>> Get(int id)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var llave = await context.LlavesAPI.FirstOrDefaultAsync(x => x.Id == id);

            if (llave is null)
            {
                return NotFound();
            }

            if (llave.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            return mapper.Map<LlaveDTO>(llave);
        }

        [HttpPost]
        public async Task<ActionResult> Post(LlaveCreacionDTO llaveCreacionDTO)
        {
            var usuarioID = serviciosUsuarios.ObtenerUsuarioId()!;
            
            if(llaveCreacionDTO.TipoLlave == TipoLlave.Gratuita)
            {
                var elUsuarioYaTieneLlaveGratuita = await context.
                    LlavesAPI.AnyAsync(x => x.UsuarioId == usuarioID && x.TipoLlave == TipoLlave.Gratuita);

                if (elUsuarioYaTieneLlaveGratuita)
                {
                    ModelState.AddModelError(nameof(llaveCreacionDTO.TipoLlave), "El usuario ya tiene una llave gratuita");
                    return ValidationProblem();
                }
            }

            var llaveApi = await servicioLlaves.CrearLlave(usuarioID, TipoLlave.Profesional);
            var llaveDTO = mapper.Map<LlaveDTO>(llaveApi);
            return CreatedAtRoute("ObtenerLlavesV1", new { id = llaveApi.Id }, llaveDTO);
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LlaveActualizacionDTO llaveActualizacionDTO)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId()!;
            var llaveDb = await context.LlavesAPI.FirstOrDefaultAsync(x => x.Id == id);

            if(llaveDb == null)
            {
                return NotFound();
            }

            if(usuarioId != llaveDb.UsuarioId)
            {
                return Forbid();
            }

            if (llaveActualizacionDTO.ActualizarLlave)
            {
                llaveDb.Llave = servicioLlaves.GenerarLLave();
            }

            llaveDb.Activa = llaveActualizacionDTO.Activa;
            await context.SaveChangesAsync();

            return NoContent();

        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var llaveDb = await context.LlavesAPI.FirstOrDefaultAsync(x => x.Id == id);

            if(llaveDb is null)
            {
                return NotFound();
            }

            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();

            if(usuarioId != llaveDb.UsuarioId)
            {
                return Forbid();
            }

            if(llaveDb.TipoLlave == TipoLlave.Gratuita)
            {
                ModelState.AddModelError("", "No puedes borrar una llave gratuita");
                return ValidationProblem();
            }

            context.Remove(llaveDb);
            await context.SaveChangesAsync();
            return NoContent();

        }

    }
}
