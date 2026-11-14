using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.DTOs
{
    public class RestriccionIPCreacionDTO
    {
        public int LlaveId { get; set; }
        [Required]
        public required string IP { get; set; }
    }
}
