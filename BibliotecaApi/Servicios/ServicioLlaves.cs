using BibliotecaApi.Datos;
using BibliotecaApi.Entidades;

namespace BibliotecaApi.Servicios
{
    public class ServicioLlaves : IServicioLlaves
    {
        private readonly AplicationDbContext context;

        public ServicioLlaves(AplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<LlaveAPI> CrearLlave(string usuarioId, TipoLlave tipoLlave)
        {
            var llave = GenerarLLave();

            var llaveAPI = new LlaveAPI
            {
                Activa = true,
                Llave = llave,
                TipoLlave = tipoLlave,
                UsuarioId = usuarioId,
            };

            context.Add(llaveAPI);
            await context.SaveChangesAsync();
            return llaveAPI;
        }

        public string GenerarLLave() => Guid.NewGuid().ToString().Replace("-", "");
    }
}
