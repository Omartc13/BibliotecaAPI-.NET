using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.DTOs
{
    public class RestriccionDominioActualizacionDTO
    {
        [Required]
        public required string Dominio { get; set; }
    }
}
