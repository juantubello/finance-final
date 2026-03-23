using FinanzasApp.Models;
using FinanzasApp.Repositories;
using FinanzasApp.Services;

namespace FinanzasApp.Endpoints;

public static class PushEndpoints
{
    public static void MapPushEndpoints(this WebApplication app)
    {
        // ── VAPID public key ─────────────────────────────────────────────────
        app.MapGet("/push/vapid-public-key", (IConfiguration config) =>
            Results.Ok(new { publicKey = config["Vapid:PublicKey"] ?? "" }));

        // ── Lista de subscriptions ───────────────────────────────────────────
        app.MapGet("/push/subscriptions", async (PushSubscriptionsRepository repo) =>
            Results.Ok(await repo.GetAllAsync()));

        // ── Suscribirse / actualizar (UPSERT por device_id) ──────────────────
        app.MapPost("/push/subscribe", async (SubscribeRequest req, PushSubscriptionsRepository repo) =>
        {
            var sub = await repo.UpsertAsync(req);
            return Results.Ok(sub);
        });

        // ── Actualizar configuración de un device ────────────────────────────
        app.MapPut("/push/subscriptions/{id}", async (int id, UpdateSubscriptionRequest req, PushSubscriptionsRepository repo) =>
        {
            await repo.UpdateAsync(id, req);
            return Results.Ok(new { id });
        });

        // ── Eliminar subscription ─────────────────────────────────────────────
        app.MapDelete("/push/subscriptions/{id}", async (int id, PushSubscriptionsRepository repo) =>
        {
            await repo.DeleteAsync(id);
            return Results.NoContent();
        });

        // ── Enviar notificación de prueba ────────────────────────────────────
        app.MapPost("/push/test/{id}", async (int id, WebPushService pushService) =>
        {
            try
            {
                await pushService.SendTestAsync(id);
                return Results.Ok(new { sent = true });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }
}
