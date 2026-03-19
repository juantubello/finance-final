using FinanzasApp.Models;
using FinanzasApp.Repositories;

namespace FinanzasApp.Endpoints;

public static class CategoriasEndpoint
{
    public static void MapCategoriasEndpoints(this WebApplication app)
    {
        app.MapGet("/categories", async (CategoriasRepository repo) =>
            Results.Ok(await repo.ObtenerCategorias()));

        app.MapPost("/categories", async (Categorias categoria, CategoriasRepository repo) =>
        {
            await repo.AgregarCategoria(categoria);
            return Results.Created("/categories", categoria);
        });

        app.MapPut("/categories/{id}", async (int id, Categorias categoria, CategoriasRepository repo) =>
        {
            categoria.CategoryId = id;
            await repo.ActualizarCategoria(categoria);
            return Results.Ok(categoria);
        });

        app.MapDelete("/categories/{id}", async (int id, CategoriasRepository repo) =>
        {
            await repo.EliminarCategoria(id);
            return Results.NoContent();
        });
    }
}