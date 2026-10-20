using Microsoft.AspNetCore.Identity;

namespace BibliotecaApi.Entidades
{
    public class Usuario : IdentityUser
    {
        public DateTime FechaNacimiento { get; set; }
    }
}
