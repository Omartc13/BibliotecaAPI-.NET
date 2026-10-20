using BibliotecaApi.Entidades;
using Microsoft.AspNetCore.Identity;

namespace BibliotecaApi.Servicios
{
    public interface IServiciosUsuarios
    {
        Task<Usuario?> ObtenerUsuario();
    }
}