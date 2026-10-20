using BibliotecaApi.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace BibliotecaApi.Servicios.V1
{
    public class GeneradorEnlaces : IGeneradorEnlaces
    {
        private readonly LinkGenerator linkGenerator;
        private readonly IAuthorizationService authorizationService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public GeneradorEnlaces(
            LinkGenerator linkGenerator, IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            this.linkGenerator = linkGenerator;
            this.authorizationService = authorizationService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<ColeccionDeRecursosDTO<AutorDTO>> GenerarEnlaces(List<AutorDTO> autores)
        {
            var resultados = new ColeccionDeRecursosDTO<AutorDTO> { Valores = autores };
            var usuario = httpContextAccessor.HttpContext!.User;
            var esAdmin = await authorizationService.AuthorizeAsync(usuario, "esadmin");

            foreach (var dto in autores)
            {
                GenerarEnlace(dto, esAdmin.Succeeded);
            }

            resultados.Enlaces.Add(new DatosHATEOASDTO(
                Enlace: linkGenerator.GetUriByRouteValues(
                        httpContextAccessor.HttpContext!, "ObtenerAutoresV1", new { })!,
                Descripcion: "self",
                Metodo: "GET"
                ));

            if (esAdmin.Succeeded)
            {
                resultados.Enlaces.Add(new DatosHATEOASDTO(
                    Enlace: linkGenerator.GetUriByRouteValues(
                        httpContextAccessor.HttpContext!, "CrearAutorV1", new { })!,
                    Descripcion: "autor-crear",
                    Metodo: "POST"
                    ));

                resultados.Enlaces.Add(new DatosHATEOASDTO(
                    Enlace: linkGenerator.GetUriByRouteValues(
                        httpContextAccessor.HttpContext!, "CrearAutorConFotoV1", new { })!,
                    Descripcion: "autor-crear-con-foto",
                    Metodo: "POST"
                    ));
            }

            return resultados;
        }

        public async Task GenerarEnlaces(AutorDTO autorDTO)
        {
            var usuario = httpContextAccessor.HttpContext!.User;
            var esAdmin = await authorizationService.AuthorizeAsync(usuario, "esadmin");
            GenerarEnlace(autorDTO, esAdmin.Succeeded);
        }

        private void GenerarEnlace(AutorDTO autorDTO, bool esAdmin)
        {
            autorDTO.Enlaces.Add(
                new DatosHATEOASDTO(
                    Enlace: linkGenerator.GetUriByRouteValues(
                        httpContextAccessor.HttpContext!, "ObtenerAutorV1", new { id = autorDTO.Id })!,
                    Descripcion: "self",
                    Metodo: "GET"));

            autorDTO.Enlaces.Add(
                new DatosHATEOASDTO(
                    Enlace: linkGenerator.GetUriByRouteValues(
                        httpContextAccessor.HttpContext!, "ActualizarAutorV1", new { id = autorDTO.Id })!,
                    Descripcion: "autor-actualizar",
                    Metodo: "PUT"));

            autorDTO.Enlaces.Add(
                new DatosHATEOASDTO(
                    Enlace: linkGenerator.GetUriByRouteValues(
                        httpContextAccessor.HttpContext!, "PatchAutorV1", new { id = autorDTO.Id })!,
                    Descripcion: "autor-patch",
                    Metodo: "PATCH"));

            autorDTO.Enlaces.Add(
                new DatosHATEOASDTO(
                    Enlace: linkGenerator.GetUriByRouteValues(
                        httpContextAccessor.HttpContext!, "BorrarAutorV1", new { id = autorDTO.Id })!,
                    Descripcion: "autor-borrar",
                    Metodo: "DELETE"));
        }
    }
}
