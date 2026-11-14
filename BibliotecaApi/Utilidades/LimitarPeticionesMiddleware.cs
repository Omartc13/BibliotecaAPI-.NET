using BibliotecaApi.Datos;
using BibliotecaApi.DTOs;
using BibliotecaApi.Entidades;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BibliotecaApi.Utilidades
{
    public static class LimitarPeticionesMiddlewareExtensions
    {
        public static IApplicationBuilder UseLimitarPeticiones(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LimitarPeticionesMiddleware>();
        }
    }

    public class LimitarPeticionesMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptionsMonitor<LimitarPeticionesDTO> optionsMonitorLimitarPeticiones;

        public LimitarPeticionesMiddleware(RequestDelegate next, 
            IOptionsMonitor<LimitarPeticionesDTO> optionsMonitorLimitarPeticiones)
        {
            this.next = next;
            this.optionsMonitorLimitarPeticiones = optionsMonitorLimitarPeticiones;
        }

        public async Task InvokeAsync(HttpContext httpContext, AplicationDbContext context)
        {
            var endpoint = httpContext.GetEndpoint();

            if(endpoint is null)
            {
                await next(httpContext);
                return;
            }

            var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

            if(actionDescriptor != null)
            {
                var accionTieneatributoIgnorarLimitarPeticiones =
                    actionDescriptor.MethodInfo
                    .GetCustomAttributes(typeof(DeshabilitarLimitarPeticionesAttribute), inherit: true)
                    .Any();

                var controladorTieneAtributoIgnorarLimitarPeticiones = 
                    actionDescriptor.ControllerTypeInfo
                    .GetCustomAttributes(typeof(DeshabilitarLimitarPeticionesAttribute), inherit: true)
                    .Any();

                if (accionTieneatributoIgnorarLimitarPeticiones ||
                    controladorTieneAtributoIgnorarLimitarPeticiones)
                {
                    await next(httpContext);
                    return;
                }
            }

            var limitarPeticionesDTO = optionsMonitorLimitarPeticiones.CurrentValue;

            var llaveStringValues = httpContext.Request.Headers["X-Api-Key"];

            if (llaveStringValues.Count() == 0)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Debe poner la lalve en la cabecera: X-Api-Key");
                return;
            }

            if (llaveStringValues.Count() > 1)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Solo debe mandar una llave");
                return;
            }

            var llave = llaveStringValues[0];

            var llaveDb = await context.LlavesAPI
                .Include(x => x.RestriccionesDominio)
                .Include(x => x.RestriccionesIP)
                .FirstOrDefaultAsync(x => x.Llave == llave);

            if(llaveDb is null)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("La llave no existe");
                return;
            }

            if (!llaveDb.Activa)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("La llave no esta activa");
                return;
            }

            var restriccionesSuperadas = PeticionSuperaAlgunaDeLasRestricciones(llaveDb, httpContext);

            if (!restriccionesSuperadas)
            {
                httpContext.Response.StatusCode = 403;
                return;
            }

            if (llaveDb.TipoLlave == TipoLlave.Gratuita)
            {
                var hoy = DateTime.UtcNow.Date;
                var cantidadPeticionesRealizadasHoy = await
                    context.Peticiones.CountAsync(x => x.LlaveId == llaveDb.Id &&
                    x.FechaPeticion >= hoy);

                if(limitarPeticionesDTO.PeticionesPorDiaGratuitos <= cantidadPeticionesRealizadasHoy)
                {
                    httpContext.Response.StatusCode = 429; //Too Many Request
                    await httpContext.Response.WriteAsync("Ha realizado todas las peticiones gratuitas el dia de hoy. Si desea realizar mas peticiones, actualize su suscripcion a una cuenta profesional");
                    return;
                }

                var peticion = new Peticion() { LlaveId = llaveDb.Id, FechaPeticion = DateTime.UtcNow };
                context.Add(peticion);
                await context.SaveChangesAsync();

                await next(httpContext);
            }

        }

        private bool PeticionSuperaAlgunaDeLasRestricciones(LlaveAPI llaveAPI, HttpContext httpContext)
        {
            var hayRestricciones = llaveAPI.RestriccionesDominio.Any() ||
                llaveAPI.RestriccionesIP.Any();

            if (!hayRestricciones)
            {
                return true;
            }

            var peticionSuperarLasRestriccionesDeDominio = PeticionSuperarLasRestriccionesDeDominio(llaveAPI.RestriccionesDominio, httpContext);

            var peticionSuperaLasRestriccionesDeIP = PeticionSuperaLasRestriccionesDeIP(llaveAPI.RestriccionesIP, httpContext);

            return peticionSuperarLasRestriccionesDeDominio || peticionSuperaLasRestriccionesDeIP;
        }

        private bool PeticionSuperarLasRestriccionesDeDominio(List<RestriccionDominio> restricciones, HttpContext context)
        {
            if(restricciones is null || restricciones.Count == 0)
            {
                return false;
            }

            var refer = context.Request.Headers["referer"].ToString();

            if(refer == string.Empty)
            {
                return false;
            }

            var miURI = new Uri(refer);
            var dominio = miURI.Host;

            var superaRestriccion = restricciones.Any(x => x.Dominio == dominio);

            return superaRestriccion;
        }

        private bool PeticionSuperaLasRestriccionesDeIP(List<RestriccionIP> restricciones, HttpContext context)
        {
            if (restricciones is null || restricciones.Count == 0)
            {
                return false;
            }

            var remoteIpAddress = context.Connection.RemoteIpAddress;

            if (remoteIpAddress is null)
            {
                return false;
            }

            var IP = remoteIpAddress.ToString();

            if (IP == string.Empty)
            {
                return false;
            }

            var superaRestriccion = restricciones.Any(x => x.IP == IP);

            return superaRestriccion;

        }

    }
}
