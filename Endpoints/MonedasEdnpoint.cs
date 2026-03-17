using FinanzasApp.Models;
using FinanzasApp.Repositories;

namespace FinanzasApp.Endpoints;

public static class MonedasEndpoint
{
    public static void MapMonedasEndpoints(this WebApplication app)
    {
        app.MapGet("/currencies", async (MonedaRepository repo) =>
            Results.Ok(await repo.ObtenerMonedas()));
    }
}