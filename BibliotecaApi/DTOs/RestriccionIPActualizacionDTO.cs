using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.DTOs
{
    public class RestriccionIPActualizacionDTO
    {
        [Required] public required string IP { get; set; }
    }
}
