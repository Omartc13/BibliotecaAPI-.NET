using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.DTOs
{
    public class RestriccionDominioCreacionDTO
    {
        public int LlaveId { get; set; }

        [Required]
        public required string Dominio { get; set; }
    }
}
