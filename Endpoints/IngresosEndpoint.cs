using FinanzasApp.Models;
using FinanzasApp.Repositories;

namespace FinanzasApp.Endpoints;

public static class IngresosEndpoints
{
    public static void MapIngresosEndpoints(this WebApplication app)
    {
        app.MapGet("/ingresos", async (int year, int month, IngresosRepository repo) =>
            Results.Ok(await repo.ObtenerIngresos(year, month)));

        app.MapGet("/ingresos/{id}", async (int id, IngresosRepository repo) =>
        {
            var ingreso = await repo.ObtenerIngresoPorId(id);
            return ingreso is null ? Results.NotFound() : Results.Ok(ingreso);
        });

        app.MapPost("/ingresos", async (Ingreso ingreso, IngresosRepository repo) =>
        {
            await repo.AgregarIngreso(ingreso);
            return Results.Created("/ingresos", ingreso);
        });

        app.MapPut("/ingresos/{id}", async (int id, Ingreso ingreso, IngresosRepository repo) =>
        {
            ingreso.Id = id;
            await repo.ActualizarIngreso(ingreso);
            return Results.Ok(ingreso);
        });

        app.MapDelete("/ingresos/{id}", async (int id, IngresosRepository repo) =>
        {
            await repo.EliminarIngreso(id);
            return Results.NoContent();
        });
    }
}
