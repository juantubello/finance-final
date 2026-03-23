using System.Net;
using System.Text.Json;
using FinanzasApp.Repositories;
using WebPush;

namespace FinanzasApp.Services;

public class WebPushService
{
    private readonly PushSubscriptionsRepository _repo;
    private readonly VapidDetails _vapid;
    private readonly ILogger<WebPushService> _logger;

    public WebPushService(
        PushSubscriptionsRepository repo,
        IConfiguration config,
        ILogger<WebPushService> logger)
    {
        _repo   = repo;
        _logger = logger;

        var subject    = config["Vapid:Subject"]    ?? throw new Exception("Vapid:Subject no configurado.");
        var publicKey  = config["Vapid:PublicKey"]  ?? throw new Exception("Vapid:PublicKey no configurado.");
        var privateKey = config["Vapid:PrivateKey"] ?? throw new Exception("Vapid:PrivateKey no configurado.");

        _vapid = new VapidDetails(subject, publicKey, privateKey);
    }

    /// <summary>
    /// Sends a push notification to all active subscribers except the sender
    /// (unless receive_own = 1). Fire-and-forget safe.
    /// </summary>
    public async Task SendAsync(
        string? senderDeviceId,
        string titleTemplate,
        string body,
        string? url = null)
    {
        // Resolve sender alias
        string alias = "Alguien";
        if (senderDeviceId is not null)
        {
            var found = await _repo.GetAliasAsync(senderDeviceId);
            if (found is not null) alias = found;
        }

        var title = titleTemplate.Replace("{alias}", alias);

        var payload = JsonSerializer.Serialize(new
        {
            title,
            body,
            icon  = "/icon-192.png",
            badge = "/icon-192.png",
            data  = new { url = url ?? "/" }
        });

        var subscriptions = (await _repo.GetActiveToNotifyAsync(senderDeviceId)).ToList();

        var client = new WebPushClient();
        var toDelete = new List<int>();

        foreach (var sub in subscriptions)
        {
            try
            {
                var pushSub = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                await client.SendNotificationAsync(pushSub, payload, _vapid);
            }
            catch (WebPushException ex) when (
                ex.StatusCode == HttpStatusCode.Gone ||
                ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation(
                    "Subscription {Id} expirada (HTTP {Code}), eliminando.", sub.Id, (int)ex.StatusCode);
                toDelete.Add(sub.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error enviando push a subscription {Id}.", sub.Id);
            }
        }

        foreach (var id in toDelete)
            await _repo.DeleteByIdAsync(id);
    }

    public async Task SendTestAsync(int subscriptionId)
    {
        var all = await _repo.GetAllAsync();
        var sub = all.FirstOrDefault(s => s.Id == subscriptionId);
        if (sub is null) return;

        var payload = JsonSerializer.Serialize(new
        {
            title = "Test — Pi Finance",
            body  = "Las notificaciones están funcionando ✓",
            icon  = "/icon-192.png",
            badge = "/icon-192.png",
            data  = new { url = "/" }
        });

        var client  = new WebPushClient();
        var pushSub = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);

        try
        {
            await client.SendNotificationAsync(pushSub, payload, _vapid);
        }
        catch (WebPushException ex) when (
            ex.StatusCode == HttpStatusCode.Gone ||
            ex.StatusCode == HttpStatusCode.NotFound)
        {
            await _repo.DeleteByIdAsync(subscriptionId);
            throw new Exception("Subscription expirada, fue eliminada.");
        }
    }
}
