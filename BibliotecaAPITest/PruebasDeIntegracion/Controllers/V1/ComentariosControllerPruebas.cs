using BibliotecaApi.Entidades;
using BibliotecaAPITest.Utilidades;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Headers;

namespace BibliotecaAPITest.PruebasDeIntegracion.Controllers.V1
{
    [TestClass]
    public class ComentariosControllerPruebas : BasePruebas
    {
        private static readonly string url = "/api/v1/libros/1/comentarios";
        private string nombreBd = Guid.NewGuid().ToString();

        private async Task CrearDataDePrueba()
        {
            var context = ConstruirContext(nombreBd);
            var autor = new Autor { Nombres = "Omar", Apellidos = "Trejo" };
            context.Add(autor);
            await context.SaveChangesAsync();

            var libro = new Libro { Titulo = "titulo" };
            libro.Autores.Add(new AutorLibro { Autor = autor });
            context.Add(libro);
            await context.SaveChangesAsync();
        }

        [TestMethod]
        public async Task Delete_Devuelve204_CuandoUsuarioBorraSuPropioComentario()
        {
            //Preparacion
            await CrearDataDePrueba();
            var factory = ConstruirWebApplicationFactory(nombreBd, ignorarSeguridad: false);
            var token = await CrearUsuario(nombreBd, factory);
            var context = ConstruirContext(nombreBd);
            var usuario = await context.Users.FirstAsync();

            var comentario = new Comentario
            {
                Cuerpo = "contenido",
                UsuarioId = usuario!.Id,
                LibroId = 1
            };

            context.Add(comentario);
            await context.SaveChangesAsync();

            var cliente = factory.CreateClient();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            //Prueba
            var respuesta = await cliente.DeleteAsync($"{url}/{comentario.Id}");

            //Verificacion
            Assert.AreEqual(expected: HttpStatusCode.NoContent, actual: respuesta.StatusCode);
        }

        [TestMethod]
        public async Task Delete_Devuelve403_CuandoUsuarioIntentaBorrarComentarioDeOtro()
        {
            //Preparacion
            await CrearDataDePrueba();
            var factory = ConstruirWebApplicationFactory(nombreBd, ignorarSeguridad: false);

            var emailCreadorComentario = "creador-comentario@hotmail.com";
            await CrearUsuario(nombreBd, factory, [], emailCreadorComentario);

            var context = ConstruirContext(nombreBd);
            var usuarioCreadorComentario = await context.Users.FirstAsync();

            var comentario = new Comentario
            {
                Cuerpo = "contenido",
                UsuarioId = usuarioCreadorComentario!.Id,
                LibroId = 1
            };

            context.Add(comentario);
            await context.SaveChangesAsync();

            var tokenUsuarioDistinto = await CrearUsuario(nombreBd, factory, 
                [], "usuario-distinto@hotmail.com");

            var cliente = factory.CreateClient();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenUsuarioDistinto);

            //Prueba
            var respuesta = await cliente.DeleteAsync($"{url}/{comentario.Id}");

            //Verificacion
            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: respuesta.StatusCode);
        }
    }
}
