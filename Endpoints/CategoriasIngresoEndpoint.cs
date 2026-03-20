using FinanzasApp.Models;
using FinanzasApp.Repositories;

namespace FinanzasApp.Endpoints;

public static class CategoriasIngresoEndpoints
{
    public static void MapCategoriasIngresoEndpoints(this WebApplication app)
    {
        app.MapGet("/income-categories", async (CategoriasIngresoRepository repo) =>
            Results.Ok(await repo.ObtenerCategorias()));

        app.MapPost("/income-categories", async (CategoriaIngreso categoria, CategoriasIngresoRepository repo) =>
        {
            await repo.AgregarCategoria(categoria);
            return Results.Created("/income-categories", categoria);
        });

        app.MapPut("/income-categories/{id}", async (int id, CategoriaIngreso categoria, CategoriasIngresoRepository repo) =>
        {
            categoria.CategoryId = id;
            await repo.ActualizarCategoria(categoria);
            return Results.Ok(categoria);
        });

        app.MapDelete("/income-categories/{id}", async (int id, CategoriasIngresoRepository repo) =>
        {
            await repo.EliminarCategoria(id);
            return Results.NoContent();
        });
    }
}
