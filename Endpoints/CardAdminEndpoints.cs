using FinanzasApp.Models;
using FinanzasApp.Repositories;

namespace FinanzasApp.Endpoints;

public static class CardAdminEndpoints
{
    public static void MapCardAdminEndpoints(this WebApplication app)
    {
        // ── Cards ────────────────────────────────────────────────────────────
        app.MapGet("/cards", async (CardAdminRepository repo) =>
            Results.Ok(await repo.GetCardsAsync()));

        app.MapPost("/cards", async (CardUpsert body, CardAdminRepository repo) =>
        {
            var id = await repo.InsertCardAsync(body);
            return Results.Created($"/cards/{id}", new { id });
        });

        app.MapPut("/cards/{id}", async (int id, CardUpsert body, CardAdminRepository repo) =>
        {
            await repo.UpdateCardAsync(id, body);
            return Results.Ok(new { id });
        });

        app.MapDelete("/cards/{id}", async (int id, CardAdminRepository repo) =>
        {
            await repo.DeleteCardAsync(id);
            return Results.NoContent();
        });

        // ── Card Categories ──────────────────────────────────────────────────
        app.MapGet("/cards/categories", async (CardAdminRepository repo) =>
            Results.Ok(await repo.GetCategoriesAsync()));

        app.MapPost("/cards/categories", async (CardCategoryUpsert body, CardAdminRepository repo) =>
        {
            var id = await repo.InsertCategoryAsync(body);
            return Results.Created($"/cards/categories/{id}", new { id });
        });

        app.MapPut("/cards/categories/{id}", async (int id, CardCategoryUpsert body, CardAdminRepository repo) =>
        {
            await repo.UpdateCategoryAsync(id, body);
            return Results.Ok(new { id });
        });

        app.MapDelete("/cards/categories/{id}", async (int id, CardAdminRepository repo) =>
        {
            await repo.DeleteCategoryAsync(id);
            return Results.NoContent();
        });

        // ── Card Category Rules ──────────────────────────────────────────────
        app.MapGet("/cards/category-rules", async (CardAdminRepository repo) =>
            Results.Ok(await repo.GetCategoryRulesAsync()));

        app.MapPost("/cards/category-rules", async (CardCategoryRuleUpsert body, CardAdminRepository repo) =>
        {
            var id = await repo.InsertCategoryRuleAsync(body);
            return Results.Created($"/cards/category-rules/{id}", new { id });
        });

        app.MapPut("/cards/category-rules/{id}", async (int id, CardCategoryRuleUpsert body, CardAdminRepository repo) =>
        {
            await repo.UpdateCategoryRuleAsync(id, body);
            return Results.Ok(new { id });
        });

        app.MapDelete("/cards/category-rules/{id}", async (int id, CardAdminRepository repo) =>
        {
            await repo.DeleteCategoryRuleAsync(id);
            return Results.NoContent();
        });

        // ── Logo Rules ───────────────────────────────────────────────────────
        app.MapGet("/cards/logo-rules", async (CardAdminRepository repo) =>
            Results.Ok(await repo.GetLogoRulesAsync()));

        app.MapPost("/cards/logo-rules", async (LogoRuleUpsert body, CardAdminRepository repo) =>
        {
            var id = await repo.InsertLogoRuleAsync(body);
            return Results.Created($"/cards/logo-rules/{id}", new { id });
        });

        app.MapPut("/cards/logo-rules/{id}", async (int id, LogoRuleUpsert body, CardAdminRepository repo) =>
        {
            await repo.UpdateLogoRuleAsync(id, body);
            return Results.Ok(new { id });
        });

        app.MapDelete("/cards/logo-rules/{id}", async (int id, CardAdminRepository repo) =>
        {
            await repo.DeleteLogoRuleAsync(id);
            return Results.NoContent();
        });
    }
}
