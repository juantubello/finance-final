using FinanzasApp.Models;
using FinanzasApp.Repositories;

namespace FinanzasApp.Endpoints;

public static class GastosEndpoints
{
    public static void MapGastosEndpoints(this WebApplication app)
    {
        app.MapGet("/gastos", async (int year, int month, GastosRepository repo) =>
            Results.Ok(await repo.ObtenerGastos(year, month)));

        app.MapGet("/gastos/{id}", async (int id, GastosRepository repo) =>
        {
            var gasto = await repo.ObtenerGastoPorId(id);
            return gasto is null ? Results.NotFound() : Results.Ok(gasto);
        });

        app.MapGet("/gastosByCategories", async (int year, int month, GastosRepository repo) =>
            Results.Ok(await repo.GetGastosByCategories(year, month)));

        app.MapGet("/gastos/categorias/rango", async (int yearFrom, int monthFrom, int yearTo, int monthTo, GastosRepository repo) =>
        Results.Ok(await repo.GetGastosByCategoriesRange(yearFrom, monthFrom, yearTo, monthTo)));

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