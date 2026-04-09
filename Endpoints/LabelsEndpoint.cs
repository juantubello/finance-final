using FinanzasApp.Models;
using FinanzasApp.Repositories;
using Microsoft.Data.Sqlite;

namespace FinanzasApp.Endpoints;

public static class LabelsEndpoints
{
    public static void MapLabelsEndpoints(this WebApplication app)
    {
        app.MapGet("/labels", async (LabelsRepository repo) =>
            Results.Ok(await repo.ObtenerLabels()));

        app.MapPost("/labels", async (LabelUpsertRequest request, LabelsRepository repo) =>
        {
            try
            {
                var label = await repo.CrearLabel(request);
                return Results.Created($"/labels/{label.Id}", label);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return Results.Conflict(new { error = "Ya existe una etiqueta con ese nombre." });
            }
        });

        app.MapPut("/labels/{id}", async (int id, LabelUpsertRequest request, LabelsRepository repo) =>
        {
            try
            {
                var label = await repo.ActualizarLabel(id, request);
                return label is null
                    ? Results.NotFound()
                    : Results.Ok(label);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return Results.Conflict(new { error = "Ya existe una etiqueta con ese nombre." });
            }
        });

        app.MapDelete("/labels/{id}", async (int id, LabelsRepository repo) =>
        {
            try
            {
                var deleted = await repo.EliminarLabel(id);
                return deleted ? Results.NoContent() : Results.NotFound();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return Results.Conflict(new
                {
                    error = "No se pudo eliminar la etiqueta porque la base no tiene configurado borrado en cascada para sus relaciones."
                });
            }
        });

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
