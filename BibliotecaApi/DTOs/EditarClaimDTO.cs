using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.DTOs
{
    public class EditarClaimDTO
    {
        [EmailAddress]
        [Required]
        public required string Email { get; set; }
    }
}
