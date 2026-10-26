using AutoMapper;
using BibliotecaApi.Datos;
using BibliotecaApi.Utilidades;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITest.Utilidades
{
    public class BasePruebas
    {
        protected AplicationDbContext ConstruirContext(string nombreBd)
        {
            var opciones = new DbContextOptionsBuilder<AplicationDbContext>()
                .UseInMemoryDatabase(nombreBd).Options;

            var dbContext = new AplicationDbContext(opciones);
            return dbContext;
        }

        protected IMapper ConfigurarAutoMapper()
        {
            var config = new MapperConfiguration(opciones =>
            {
                opciones.AddProfile(new AutoMapperProfiles());
            });

            return config.CreateMapper();
        }
    }
}
