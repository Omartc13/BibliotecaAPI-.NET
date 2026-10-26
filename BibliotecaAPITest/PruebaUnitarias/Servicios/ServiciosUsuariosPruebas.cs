using BibliotecaApi.Entidades;
using BibliotecaApi.Servicios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITest.PruebaUnitarias.Servicios
{
    [TestClass]
    public class ServiciosUsuariosPruebas
    {
        private UserManager<Usuario> userManager = null!;
        private IHttpContextAccessor contextAccessor = null!;
        private ServiciosUsuarios servicioUsuarios = null!;

        [TestInitialize]
        public void SetUp()
        {
            userManager = Substitute.For<UserManager<Usuario>>(
                Substitute.For<IUserStore<Usuario>>(), null, null,null, null,null, null,null, null);

            contextAccessor = Substitute.For<IHttpContextAccessor>();
            servicioUsuarios = new ServiciosUsuarios(userManager, contextAccessor);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornaNulo_CuandoNoHayClaimEmail()
        {
            //Preparacion
            var httcontext = new DefaultHttpContext();
            contextAccessor.HttpContext.Returns(httcontext);

            //Prueba
            var usuario = await servicioUsuarios.ObtenerUsuario();

            //Verificacion
            Assert.IsNull(usuario);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornaUsuario_CuandoHayClaimEmail()
        {
            //Preparacion
            var email = "prueba@hotmail.com";
            var usuarioEsperado = new Usuario { Email = email };

            userManager.FindByEmailAsync(email)!.Returns(Task.FromResult(usuarioEsperado));

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httcontext = new DefaultHttpContext() { User = claims};
            contextAccessor.HttpContext.Returns(httcontext);

            //Prueba
            var usuario = await servicioUsuarios.ObtenerUsuario();

            //Verificacion
            Assert.IsNotNull(usuario);
            Assert.AreEqual(expected: email, actual: usuario.Email);
        }

        [TestMethod]
        public async Task ObtenerUsuario_RetornaNulo_CuandoUsuarioNoExiste()
        {
            //Preparacion
            var email = "prueba@hotmail.com";
            var usuarioEsperado = new Usuario { Email = email };

            userManager.FindByEmailAsync(email)!.Returns(Task.FromResult<Usuario>(null!));

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httcontext = new DefaultHttpContext() { User = claims };
            contextAccessor.HttpContext.Returns(httcontext);

            //Prueba
            var usuario = await servicioUsuarios.ObtenerUsuario();

            //Verificacion
            Assert.IsNull(usuario);
        }
    }
}
