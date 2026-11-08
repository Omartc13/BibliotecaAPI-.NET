using BibliotecaApi.Entidades;

namespace BibliotecaApi.Servicios
{
    public interface IServicioLlaves
    {
        Task<LlaveAPI> CrearLlave(string usuarioId, TipoLlave tipoLlave);
        string GenerarLLave();
    }
}