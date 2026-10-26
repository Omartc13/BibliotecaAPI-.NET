using BibliotecaApi.DTOs;
using BibliotecaApi.Entidades;
using BibliotecaAPITest.Utilidades;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace BibliotecaAPITest.PruebasDeIntegracion.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas : BasePruebas
    {
        private static readonly string url = "/api/v1/autores";
        private string nombreBd = Guid.NewGuid().ToString();


        [TestMethod]
        public async Task Get_Devuelve404_CuandoAutorNoExiste()
        {
            //Preparacion
            var factory = ConstruirWebApplicationFactory(nombreBd);
            var cliente = factory.CreateClient();

            //Prueba
            var respuesta = await cliente.GetAsync($"{url}/1");

            //Verificacion
            var statusCode = respuesta.StatusCode;
            Assert.AreEqual(expected: HttpStatusCode.NotFound, actual: respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Get_DevuelveAutor_CuandoAutorExiste()
        {
            //Preparacion
            var context = ConstruirContext(nombreBd);
            context.Autores.Add(new Autor() { Nombres = "Omar", Apellidos = "Trejo" });
            context.Autores.Add(new Autor() { Nombres = "Macarena", Apellidos = "Bustamante" });
            await context.SaveChangesAsync();

            var factory = ConstruirWebApplicationFactory(nombreBd);
            var cliente = factory.CreateClient();

            //Prueba
            var respuesta = await cliente.GetAsync($"{url}/1");

            //Verificacion
            respuesta.EnsureSuccessStatusCode();

            var autor = JsonSerializer.Deserialize<AutorConLibrosDTO>(
                await respuesta.Content.ReadAsStringAsync(), jsonSerializerOptions)!;

            Assert.AreEqual(expected: 1, autor.Id);
        }

        [TestMethod]
        public async Task Post_Devuelve401_CuandoUsuarioNoEstaAutenticado()
        {
            //Preparacion
            var factory = ConstruirWebApplicationFactory(nombreBd, ignorarSeguridad: false);
            var cliente = factory.CreateClient();
            var autorCreacionDTO = new AutorCreacionDTO { Nombres = "Omar", Apellidos = "Trejo", Identificacion = "123" };

            //Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacionDTO);

            //Verificacion
            Assert.AreEqual(expected: HttpStatusCode.Unauthorized, respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Post_Devuelve403_CuandoUsuarioNoEsAdmin()
        {
            //Preparacion
            var factory = ConstruirWebApplicationFactory(nombreBd, ignorarSeguridad: false);
            var token = await CrearUsuario(nombreBd, factory);
            var cliente = factory.CreateClient();

            cliente.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var autorCreacionDTO = new AutorCreacionDTO { Nombres = "Omar", Apellidos = "Trejo", Identificacion = "123" };

            //Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacionDTO);

            //Verificacion
            Assert.AreEqual(expected: HttpStatusCode.Forbidden, respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Post_Devuelve201_CuandoUsuarioEsAdmin()
        {
            //Preparacion
            var factory = ConstruirWebApplicationFactory(nombreBd, ignorarSeguridad: false);

            var claims = new List<Claim> { adminClaim };

            var token = await CrearUsuario(nombreBd, factory, claims);
            var cliente = factory.CreateClient();

            cliente.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var autorCreacionDTO = new AutorCreacionDTO { Nombres = "Omar", Apellidos = "Trejo", Identificacion = "123" };

            //Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacionDTO);

            //Verificacion
            respuesta.EnsureSuccessStatusCode();
            Assert.AreEqual(expected: HttpStatusCode.Created, respuesta.StatusCode);
        }
    }
}
