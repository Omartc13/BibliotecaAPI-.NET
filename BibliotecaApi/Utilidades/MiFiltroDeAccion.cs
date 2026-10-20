using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaApi.Utilidades
{
    public class MiFiltroDeAccion : IActionFilter
    {
        private readonly ILogger<MiFiltroDeAccion> logger;

        public MiFiltroDeAccion(ILogger<MiFiltroDeAccion> logger)
        {
            this.logger = logger;
        }

        //Se ejecuta antes de la accion
        public void OnActionExecuting(ActionExecutingContext context)
        {
            logger.LogInformation("Ejecutando la accion");
        }


        //Despues de la accion
        public void OnActionExecuted(ActionExecutedContext context)
        {
            logger.LogInformation("Accion Ejecutada");
        }
    }
}
