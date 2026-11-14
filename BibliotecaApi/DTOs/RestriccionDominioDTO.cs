using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.DTOs
{
    public class RestriccionDominioDTO
    {
        public int Id { get; set; }

        public required string Dominio { get; set; }
    }
}
