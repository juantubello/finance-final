using FinanzasApp.Models;
using FinanzasApp.Repositories;

namespace FinanzasApp.Endpoints;

public static class CategoriasEndpoint
{
    public static void MapCategoriasEndpoints(this WebApplication app)
    {
        app.MapGet("/categories", async (CategoriasRepository repo) =>
            Results.Ok(await repo.ObtenerCategorias()));
    }
}