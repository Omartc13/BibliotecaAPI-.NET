using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.DTOs
{
    public class LibroCreacionDTO
    {
        [Required]
        [StringLength(150, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        public required string Titulo { get; set; }
        public List<int> AutoresIds { get; set; } = [];
    }
}
