using BibliotecaApi.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.DTOs
{
    public class AutorPatchDTO
    {
        [Required(ErrorMessage = "El campo {0} no debe ser vacio")]
        [StringLength(150, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        [PrimeraLetraMayuscula]
        public required string Nombres { get; set; }

        [Required(ErrorMessage = "El campo {0} no debe ser vacio")]
        [StringLength(150, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        [PrimeraLetraMayuscula]
        public required string Apellidos { get; set; }

        [StringLength(20, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        public string? Identificacion { get; set; }
    }
}
