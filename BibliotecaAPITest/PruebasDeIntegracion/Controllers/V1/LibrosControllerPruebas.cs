using BibliotecaApi.DTOs;
using BibliotecaAPITest.Utilidades;
using System.Net;

namespace BibliotecaAPITest.PruebasDeIntegracion.Controllers.V1
{
    [TestClass]
    public class LibrosControllerPruebas : BasePruebas
    {
        private readonly string url = "/api/v1/libros";
        private string nombreBd = Guid.NewGuid().ToString();

        [TestMethod]
        public async Task Post_Devuelve400_CuandoAutoresIdEsVacio()
        {
            //Preparacion
            var factory = ConstruirWebApplicationFactory(nombreBd);
            var cliente = factory.CreateClient();
            var libroCreacionDTO = new LibroCreacionDTO { Titulo = "Titulo" };

            //Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, libroCreacionDTO);

            //Verificacion
            Assert.AreEqual(expected: HttpStatusCode.BadRequest, actual: respuesta.StatusCode);
        }


    }
}
