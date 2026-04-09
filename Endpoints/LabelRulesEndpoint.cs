using FinanzasApp.Models;
using FinanzasApp.Repositories;
using FinanzasApp.Services;
using Microsoft.Data.Sqlite;

namespace FinanzasApp.Endpoints;

public static class LabelRulesEndpoints
{
    public static void MapLabelRulesEndpoints(this WebApplication app)
    {
        app.MapGet("/label-rules", async (LabelRulesRepository repo) =>
            Results.Ok(await repo.ObtenerLabelRules()));

        app.MapPost("/label-rules", async (LabelRuleRequest request, LabelRulesRepository repo) =>
        {
            try
            {
                var rule = await repo.AgregarLabelRule(request);
                return Results.Created($"/label-rules/{rule.Id}", rule);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return Results.Conflict(new { error = "El keyword ya existe para otra regla." });
            }
        });

        app.MapPut("/label-rules/{id}", async (int id, LabelRuleRequest request, LabelRulesRepository repo) =>
        {
            try
            {
                var rule = await repo.ActualizarLabelRule(id, request);
                return rule is null
                    ? Results.NotFound()
                    : Results.Ok(rule);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return Results.Conflict(new { error = "El keyword ya existe para otra regla." });
            }
        });

        app.MapDelete("/label-rules/{id}", async (int id, LabelRulesRepository repo) =>
        {
            var deleted = await repo.EliminarLabelRule(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        app.MapPost("/label-rules/apply", async (LabelRulesService service) =>
        {
            var updated = await service.ApplyToExistingGastosAsync();
            return Results.Ok(new { updated });
        });
    }
}
