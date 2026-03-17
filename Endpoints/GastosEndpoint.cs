using FinanzasApp.Models;
using FinanzasApp.Repositories;

namespace FinanzasApp.Endpoints;

public static class GastosEndpoints
{
    public static void MapGastosEndpoints(this WebApplication app)
    {
        app.MapGet("/gastos", async (GastosRepository repo) =>
            Results.Ok(await repo.ObtenerGastos()));

        app.MapGet("/gastos/{id}", async (int id, GastosRepository repo) =>
        {
            var gasto = await repo.ObtenerGastoPorId(id);
            return gasto is null ? Results.NotFound() : Results.Ok(gasto);
        });

        app.MapPost("/gastos", async (Gasto gasto, GastosRepository repo) =>
        {
            await repo.AgregarGasto(gasto);
            return Results.Created($"/gastos/{gasto.Id}", gasto);
        });

        app.MapPut("/gastos/{id}", async (int id, Gasto gasto, GastosRepository repo) =>
        {
            gasto.Id = id;
            await repo.ActualizarGasto(gasto);
            return Results.Ok(gasto);
        });

        app.MapDelete("/gastos/{id}", async (int id, GastosRepository repo) =>
        {
            await repo.EliminarGasto(id);
            return Results.NoContent();
        });
    }
}