namespace FinanzasApp.Models;

public class PushSubscriptionRecord
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = "";
    public string Alias { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string P256dh { get; set; } = "";
    public string Auth { get; set; } = "";
    public bool Active { get; set; }
    public bool ReceiveOwn { get; set; }
    public string? CreatedAt { get; set; }
    public string? UpdatedAt { get; set; }
}

public record SubscribeRequest(
    string DeviceId,
    string Alias,
    string Endpoint,
    string P256dh,
    string Auth);

public record UpdateSubscriptionRequest(bool Active, bool ReceiveOwn);
