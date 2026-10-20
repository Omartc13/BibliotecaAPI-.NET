using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.DTOs
{
    public class ComentarioCreacionDTO
    {
        [Required]
        public required string Cuerpo { get; set; }
    }
}
