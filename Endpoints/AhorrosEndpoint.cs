using FinanzasApp.Models;
using FinanzasApp.Repositories;

namespace FinanzasApp.Endpoints;

public static class AhorrosEndpoints
{
    public static void MapAhorrosEndpoints(this WebApplication app)
    {
        // Activos
        app.MapGet("/savings/assets", async (AhorrosRepository repo) =>
            Results.Ok(await repo.ObtenerActivos()));

        // Balance por activo (saldo actual de cada uno)
        app.MapGet("/savings/balance", async (AhorrosRepository repo) =>
            Results.Ok(await repo.ObtenerBalances()));

        // Movimientos — todos o filtrados por activo
        app.MapGet("/savings", async (AhorrosRepository repo, int? activoId) =>
            Results.Ok(await repo.ObtenerMovimientos(activoId)));

        // Crear movimiento (cantidad positiva = ingreso, negativa = retiro)
        app.MapPost("/savings", async (Ahorro ahorro, AhorrosRepository repo) =>
        {
            await repo.AgregarMovimiento(ahorro);
            return Results.Created("/savings", ahorro);
        });

        app.MapPut("/savings/{id}", async (int id, Ahorro ahorro, AhorrosRepository repo) =>
        {
            ahorro.Id = id;
            await repo.ActualizarMovimiento(ahorro);
            return Results.Ok(ahorro);
        });

        app.MapDelete("/savings/{id}", async (int id, AhorrosRepository repo) =>
        {
            await repo.EliminarMovimiento(id);
            return Results.NoContent();
        });
    }
}
