using BibliotecaApi.Datos;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi.Jobs
{
    public class FacturasBackgroundService : BackgroundService
    {
        private readonly IServiceProvider services;

        public FacturasBackgroundService(IServiceProvider services)
        {
            this.services = services;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using (var scope = services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AplicationDbContext>();
                        //Console.WriteLine("Ejecutando proceso de emision de facturas");
                        await EmitirFacturas(context);
                        await SeterUsuariosMalaPaga(context);
                        await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                    }
                }
            }catch (OperationCanceledException ex)
            {
                //Ejecutar algo al detener el job
            }

        }

        private async Task SeterUsuariosMalaPaga(AplicationDbContext context)
        {
            await context.Database.ExecuteSqlAsync($"EXEC Usuarios_SetearMalaPaga");
        }

        private async Task EmitirFacturas(AplicationDbContext context)
        {
                var hoy = DateTime.Today;
                var fechaComparacion = hoy.AddMonths(-1);

                var facturasDelMesYaFueronEmitidas = await
                    context.FacturasEmitidas.AnyAsync(x => x.Año == fechaComparacion.Year
                    && x.Mes == fechaComparacion.Month);

                if (!facturasDelMesYaFueronEmitidas)
                {
                    var fechaInicio = new DateTime(fechaComparacion.Year, fechaComparacion.Month, 1);
                    var fechaFin = fechaInicio.AddMonths(-1);
                    await context.Database.ExecuteSqlAsync($"EXEC Facturas_Crear {fechaInicio.ToString("yyyy-MM-dd")},{fechaFin.ToString("yyyy-MM-dd")}");
                }
            
        }
    }
}
