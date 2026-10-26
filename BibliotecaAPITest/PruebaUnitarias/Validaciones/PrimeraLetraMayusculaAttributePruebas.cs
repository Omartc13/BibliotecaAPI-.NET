using BibliotecaApi.Validaciones;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITest.PruebaUnitarias.Validaciones
{
    [TestClass]
    public class PrimeraLetraMayusculaAttributePruebas
    {
        [TestMethod]
        [DataRow("")]
        [DataRow("    ")]
        [DataRow(null)]
        [DataRow("Felipe")]
        public void IsValid_RetornaExitoso_SiValueNoTieneLaPrimeraLetraMayuscula(string value)
        {
            //Preparacion
            var primeraLetraMayuscullaAttribute = new PrimeraLetraMayusculaAttribute();
            var validationContext = new ValidationContext(new object());
            

            //Prueba
            var resultado = primeraLetraMayuscullaAttribute.GetValidationResult(value, validationContext);


            //Verificacion
            Assert.AreEqual(expected: ValidationResult.Success, actual: resultado);
        }

        [TestMethod]
        [DataRow("felipe")]
        public void IsValid_RetornaExitoso_SiValueTieneLaPrimeraLetraMinuscula(string value)
        {
            //Preparacion
            var primeraLetraMayuscullaAttribute = new PrimeraLetraMayusculaAttribute();
            var validationContext = new ValidationContext(new object());


            //Prueba
            var resultado = primeraLetraMayuscullaAttribute.GetValidationResult(value, validationContext);


            //Verificacion
            Assert.AreEqual(expected: "La primera letra debe ser mayuscula", actual: resultado!.ErrorMessage);
        }
    }
}
