using BibliotecaApi.Controllers.V1;
using BibliotecaApi.DTOs;
using BibliotecaAPITest.Utilidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITest.PruebaUnitarias.Controllers.V1
{
    [TestClass]
    public class LibrosControllerPruebas : BasePruebas
    {
        [TestMethod]
        public async Task Get_RestornaCeroLibros_CuandoNoHayLibros()
        {
            //Preparacion
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            IOutputCacheStore outputCacheStore = null!;

            var controller = new LibroController(context, mapper, outputCacheStore);

            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var paginacionDTO = new PaginacionDTO(1, 1);

            //Prueba
            var respuesta = await controller.Get(paginacionDTO);

            //Verificacion
            Assert.AreEqual(expected:0 , actual: respuesta.Count());
        }
    }
}
