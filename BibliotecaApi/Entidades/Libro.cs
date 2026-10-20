using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.Entidades
{
    public class Libro
    {
        public int Id { get; set; }
        [Required]
        [StringLength(150, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        public required string Titulo { get; set; }
        public List<AutorLibro> Autores { get; set; } = [];
        public List<Comentario> Comentarios { get; set; } = [];
    }
}
