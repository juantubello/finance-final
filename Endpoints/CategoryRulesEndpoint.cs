using FinanzasApp.Models;
using FinanzasApp.Repositories;
using Microsoft.Data.Sqlite;

namespace FinanzasApp.Endpoints;

public static class CategoryRulesEndpoints
{
    public static void MapCategoryRulesEndpoints(this WebApplication app)
    {
        app.MapGet("/category-rules", async (CategoryRulesRepository repo) =>
            Results.Ok(await repo.ObtenerCategoryRules()));

        app.MapPost("/category-rules", async (CategoryRuleRequest request, CategoryRulesRepository repo) =>
        {
            try
            {
                var rule = await repo.AgregarCategoryRule(request);
                return Results.Created($"/category-rules/{rule.Id}", rule);
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return Results.Conflict(new { error = "El keyword ya existe para otra categoría." });
            }
        });

        app.MapPut("/category-rules/{id}", async (int id, CategoryRuleRequest request, CategoryRulesRepository repo) =>
        {
            try
            {
                var rule = await repo.ActualizarCategoryRule(id, request);
                return Results.Ok(rule);
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return Results.Conflict(new { error = "El keyword ya existe para otra categoría." });
            }
        });

        app.MapDelete("/category-rules/{id}", async (int id, CategoryRulesRepository repo) =>
        {
            await repo.EliminarCategoryRule(id);
            return Results.NoContent();
        });
    }
}
