using FinanzasApp.Models;
using FinanzasApp.Repositories;

namespace FinanzasApp.Endpoints;

public static class LabelsEndpoints
{
    public static void MapLabelsEndpoints(this WebApplication app)
    {
        app.MapGet("/labels", async (LabelsRepository repo) =>
            Results.Ok(await repo.ObtenerLabels()));

        app.MapGet("/gastos/{id}/labels", async (int id, LabelsRepository repo) =>
            Results.Ok(await repo.ObtenerLabelsPorGasto(id)));

        app.MapPost("/gastos/{id}/labels", async (int id, AgregarLabelsRequest request, LabelsRepository repo) =>
        {
            var result = await repo.AgregarLabelsAGasto(id, request.Labels);
            return Results.Ok(result);
        });

        app.MapDelete("/gastos/{id}/labels/{labelId}", async (int id, int labelId, LabelsRepository repo) =>
        {
            await repo.EliminarLabelDeGasto(id, labelId);
            return Results.NoContent();
        });
    }
}
