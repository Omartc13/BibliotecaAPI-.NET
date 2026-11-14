using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaApi.Migrations
{
    /// <inheritdoc />
    public partial class CreaStoredProcedured_Facturas_Crear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"

				CREATE PROCEDURE Facturas_Crear
					@fechaInicio datetime,
					@fechaFin datetime
				AS
				BEGIN
					-- SET NOCOUNT ON added to prevent extra result sets from
					-- interfering with SELECT statements.
					SET NOCOUNT ON;

					-- Insert statements for procedure here
					DECLARE @montoPorCadaPeticion decimal(4,4) = 1.0/2 --1 dolar por cada 2 peticiones

					INSERT INTO Facturas(UsuarioId, Monto, FechaEmision, FechaLimiteDePago, Pagada)
					SELECT 
						UsuarioId,
						COUNT(*) * @montoPorCadaPeticion AS Monto,
						GETDATE() AS FechaEmision,
						DATEADD(d,60, GETDATE()) as FechaLimitePago,
						0 as Pagada
					FROM Peticiones
					INNER JOIN LlavesAPI
					ON LlavesAPI.id = Peticiones.Id
					WHERE LlavesAPI.TipoLlave != 1
					AND FechaPeticion >= @fechaInicio
					AND FechaPeticion < @fechaFin
					GROUP BY UsuarioId

					INSERT INTO FacturasEmitidas(Mes,Año)
					SELECT
						CASE MONTH(GETDATE())
						WHEN 1 THEN 12
						ELSE MONTH(GETDATE()) - 1 END AS MES,

						CASE MONTH(GETDATE())
						WHEN 1 THEN YEAR(GETDATE()) -1
						ELSE YEAR(GETDATE()) END AS Año
				END

            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("DROP PROCEDURE Facturas_Crear");
        }
    }
}
