using AutoMapper;
using BibliotecaApi.Datos;
using BibliotecaApi.DTOs;
using BibliotecaApi.Utilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Servicios.V1
{
    public class ServicioAutores : IServicioAutores
    {
        private readonly AplicationDbContext context;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;

        public ServicioAutores(AplicationDbContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<AutorDTO>> Get(PaginacionDTO paginacionDTO)
        {
            var queryable = context.Autores.AsQueryable();
            await httpContextAccessor.HttpContext!.InsertarParametrosPaginacionEnCabecera(queryable);
            var autores = await queryable
                                .OrderBy(x => x.Nombres)
                                .Paginar(paginacionDTO)
                                .ToListAsync();

            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);

            return autoresDTO;
        }
    }
}
