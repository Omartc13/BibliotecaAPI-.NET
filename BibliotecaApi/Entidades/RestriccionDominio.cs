using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.Entidades
{
    public class RestriccionDominio
    {
        public int Id { get; set; }
        public int LlaveId { get; set; }
        [Required]
        public required string Dominio { get; set; }
        public LlaveAPI? Llave { get; set; }
    }
}
