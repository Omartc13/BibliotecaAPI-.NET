using BibliotecaApi.Controllers.V1;
using BibliotecaApi.DTOs;
using BibliotecaApi.Entidades;
using BibliotecaApi.Servicios;
using BibliotecaApi.Servicios.V1;
using BibliotecaAPITest.Utilidades;
using BibliotecaAPITest.Utilidades.Dobles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.CodeCoverage.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITest.PruebaUnitarias.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas : BasePruebas
    {
        IAlmacenadorArchivos almacenadorArchivos = null!;
        ILogger<AutoresController> logger = null!;
        IOutputCacheStore outputCacheStore = null!;
        IServicioAutores servicioAutores = null!;
        private string nombreBd = Guid.NewGuid().ToString();
        private AutoresController controller = null!;

        [TestInitialize]
        public void SetUp()
        {
            var context = ConstruirContext(nombreBd);
            var mapper = ConfigurarAutoMapper();
            almacenadorArchivos = Substitute.For<IAlmacenadorArchivos>();
            logger = Substitute.For<ILogger<AutoresController>>();
            outputCacheStore = Substitute.For<IOutputCacheStore>();
            servicioAutores = Substitute.For<IServicioAutores>();

            controller = new AutoresController(context, mapper, almacenadorArchivos, logger, outputCacheStore, servicioAutores);

        }


        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorConIdNoExiste()
        {

            //Prueba
            var respuesta = await controller.Get(1);

            //Verificacion
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);
        }


        [TestMethod]
        public async Task Get_RetornaAutor_CuandoAutorConIdExiste()
        {
            //Preparacion
            var context = ConstruirContext(nombreBd);
            context.Autores.Add(new Autor { Nombres = "Omar", Apellidos = "Trejo" });//Id = 1
            context.Autores.Add(new Autor { Nombres = "Macarena", Apellidos = "Bustamante" });//Id = 2

            await context.SaveChangesAsync();

            //Prueba
            var respuesta = await controller.Get(1);

            //Verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
        }

        [TestMethod]
        public async Task Get_RetornaAutor_ConLibros_CuandoAutorTieneLibros()
        {
            //Preparacion
            var context = ConstruirContext(nombreBd);
            var libro1 = new Libro { Titulo = "Libro1" };
            var libro2 = new Libro { Titulo = "Libro2" };

            var autor = new Autor()
            {
                Nombres = "Omar",
                Apellidos = "Trejo",
                Libros = new List<AutorLibro>
                {
                    new AutorLibro {Libro = libro1},
                    new AutorLibro {Libro = libro2},
                }
            };

            context.Add(autor);

            await context.SaveChangesAsync();

            //Prueba
            var respuesta = await controller.Get(1);

            //Verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
            Assert.AreEqual(expected: 2, actual: resultado.libros.Count);
        }

        [TestMethod]
        public async Task Get_DebeLlamarGetDelServicioAutores()
        {
            //Preparacion
            var paginacionDTO = new PaginacionDTO(2, 3);

            //Prueba
            await controller.Get(paginacionDTO);

            //Verificacion
            await servicioAutores.Received(1).Get(paginacionDTO);

        }

        [TestMethod]
        public async Task Post_DebeCrearAutor_CuandoEnviamosAutor()
        {
            //Preparacion
            var context = ConstruirContext(nombreBd);

            var nuevoAutor = new AutorCreacionDTO { Nombres = "nuevo", Apellidos = "autor" };

            //Prueba
            var respuesta = await controller.Post(nuevoAutor);

            //Verificacion
            var resultado = respuesta as CreatedAtRouteResult;
            Assert.IsNotNull(resultado);

            var context2 = ConstruirContext(nombreBd);
            var cantidad = await context2.Autores.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidad);
        }

        [TestMethod]
        public async Task Put_Retorna404_CuandoAutorNoExiste()
        {
            //Prueba
            var respuesta = await controller.Put(1, autorCreacionDTO: null!);

            //Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);
        }

        private const string contenedor = "autores";
        private const string cache = "autores-obtener";

        [TestMethod]
        public async Task Put_ActualizaAutor_CuandoEnviamosAutorSinFoto()
        {
            //Preparacion
            var context = ConstruirContext(nombreBd);

            context.Autores.Add(new Autor { Nombres = "Omar", Apellidos = "Trejo", Identificacion = "Id" });

            await context.SaveChangesAsync();

            var autorCreacionDTO = new AutorCreacionDTOConFoto { Nombres = "Omar2", Apellidos = "Trejo2", Identificacion = "Id2" };


            //Prueba
            var respuesta = await controller.Put(1, autorCreacionDTO);

            //Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);

            var context3 = ConstruirContext(nombreBd);
            var autorActualizado = await context3.Autores.SingleAsync();

            Assert.AreEqual(expected: "Omar2", actual: autorActualizado.Nombres);
            Assert.AreEqual(expected: "Trejo2", actual: autorActualizado.Apellidos);
            Assert.AreEqual(expected: "Id2", actual: autorActualizado.Identificacion);
            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.DidNotReceiveWithAnyArgs().Editar(default, default!, default!);
        }

        [TestMethod]
        public async Task Put_ActualizaAutor_CuandoEnviamosAutorConFoto()
        {
            //Preparacion
            var context = ConstruirContext(nombreBd);

            var urlAnterior = "URL-1";
            var urlNueva = "URL-2";
            almacenadorArchivos.Editar(default, default!, default!).ReturnsForAnyArgs(urlNueva);

            context.Autores.Add(new Autor 
            { 
                Nombres = "Omar",
                Apellidos = "Trejo", 
                Identificacion = "Id",
                Foto = urlAnterior
            });

            await context.SaveChangesAsync();

            var formFile = Substitute.For<IFormFile>();


            var autorCreacionDTO = new AutorCreacionDTOConFoto 
            { 
                Nombres = "Omar2", 
                Apellidos = "Trejo2", 
                Identificacion = "Id2",
                Foto = formFile
            };


            //Prueba
            var respuesta = await controller.Put(1, autorCreacionDTO);

            //Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);

            var context3 = ConstruirContext(nombreBd);
            var autorActualizado = await context3.Autores.SingleAsync();

            Assert.AreEqual(expected: "Omar2", actual: autorActualizado.Nombres);
            Assert.AreEqual(expected: "Trejo2", actual: autorActualizado.Apellidos);
            Assert.AreEqual(expected: "Id2", actual: autorActualizado.Identificacion);
            Assert.AreEqual(expected: urlNueva, actual: autorActualizado.Foto);
            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.Received(1).Editar(urlAnterior, contenedor, formFile);
        }

        [TestMethod]
        public async Task Patch_Retorna400_CuandoPatchDocEsNulo()
        {
            //Prueba
            var respuesta = await controller.Patch(1, patchDoc: null!);

            //Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(400, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Patch_Retorna404_CuandoAutorNoExiste()
        {
            //Preparacion
            var patchDoc = new JsonPatchDocument<AutorPatchDTO>();

            //Prueba
            var respuesta = await controller.Patch(1, patchDoc);

            //Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Patch_RetornaValidationProblem_CuandoHayErrorDeValidacion()
        {
            //Preparacion
            var context = ConstruirContext(nombreBd);
            context.Add(new Autor { Nombres = "Omar", Apellidos = "Trejo", Identificacion = "123" });
            
            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;
            var mensajeDeError = "mensaje de error";
            controller.ModelState.AddModelError("",mensajeDeError);

            var patchDoc = new JsonPatchDocument<AutorPatchDTO>();

            //Prueba
            var respuesta = await controller.Patch(1, patchDoc);

            //Verificacion
            var resultado = respuesta as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;
            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: mensajeDeError, actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Patch_ActualizaUnCampo_CuandoSeLeEnviaUnaOperacion()
        {
            //Preparacion
            var context = ConstruirContext(nombreBd);
            context.Add(new Autor { Nombres = "Omar", Apellidos = "Trejo", Identificacion = "123", Foto = "URL-1" });

            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;


            var patchDoc = new JsonPatchDocument<AutorPatchDTO>();
            patchDoc.Operations.Add(new Operation<AutorPatchDTO>("replace", "/nombres", null, "Omar2"));

            //Prueba
            var respuesta = await controller.Patch(1, patchDoc);

            //Verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 204, resultado!.StatusCode);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);

            var context2 = ConstruirContext(nombreBd);
            var autorBD = await context2.Autores.SingleAsync();

            Assert.AreEqual(expected: "Omar2", autorBD.Nombres);
            Assert.AreEqual(expected: "Trejo", autorBD.Apellidos);
            Assert.AreEqual(expected: "123", autorBD.Identificacion);
            Assert.AreEqual(expected: "URL-1", autorBD.Foto);
        }

        [TestMethod]
        public async Task Delete_Retorna404_CuandoAutorNoExiste()
        {
            //Prueba
            var respuesta = await controller.Delete(1);

            //Verificiacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(404, resultado!.StatusCode);
        }

        [TestMethod]
        public async Task Delete_BorraAutor_CuandoAutorExiste()
        {
            //Preparacion
            var urlFoto = "URL-1";

            var context = ConstruirContext(nombreBd);

            context.Autores.Add(new Autor { Nombres = "Autor1", Apellidos = "Autor1", Foto = urlFoto });
            context.Autores.Add(new Autor { Nombres = "Autor2", Apellidos = "Autor2"});

            await context.SaveChangesAsync();

            //Prueba
            var respuesta = await controller.Delete(1);

            //Verificiacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(204, resultado!.StatusCode);

            var contexto2 = ConstruirContext(nombreBd);
            var cantidadAutores = await contexto2.Autores.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidadAutores);

            var autor2Existe = await contexto2.Autores.AnyAsync(x => x.Nombres == "Autor2");
            Assert.IsTrue(autor2Existe);

            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            await almacenadorArchivos.Received(1).Borrar(urlFoto, contenedor);

        }
    }
}
