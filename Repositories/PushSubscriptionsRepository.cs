using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;

namespace FinanzasApp.Repositories;

public class PushSubscriptionsRepository
{
    private readonly ConexionDB _db;

    public PushSubscriptionsRepository(ConexionDB db)
    {
        _db = db;
    }

    public async Task<IEnumerable<PushSubscriptionRecord>> GetAllAsync()
    {
        using var con = _db.Abrir();
        return await con.QueryAsync<PushSubscriptionRecord>("""
            SELECT id, device_id AS DeviceId, alias, endpoint, p256dh, auth,
                   active, receive_own AS ReceiveOwn, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM push_subscriptions
            ORDER BY alias, id
            """);
    }

    public async Task<PushSubscriptionRecord> UpsertAsync(SubscribeRequest req)
    {
        using var con = _db.Abrir();
        return await con.QuerySingleAsync<PushSubscriptionRecord>("""
            INSERT INTO push_subscriptions (device_id, alias, endpoint, p256dh, auth)
            VALUES (@DeviceId, @Alias, @Endpoint, @P256dh, @Auth)
            ON CONFLICT(device_id) DO UPDATE SET
                alias      = excluded.alias,
                endpoint   = excluded.endpoint,
                p256dh     = excluded.p256dh,
                auth       = excluded.auth,
                updated_at = datetime('now')
            RETURNING id, device_id AS DeviceId, alias, endpoint, p256dh, auth,
                      active, receive_own AS ReceiveOwn, created_at AS CreatedAt, updated_at AS UpdatedAt
            """, new { req.DeviceId, req.Alias, req.Endpoint, req.P256dh, req.Auth });
    }

    public async Task UpdateAsync(int id, UpdateSubscriptionRequest req)
    {
        using var con = _db.Abrir();
        await con.ExecuteAsync("""
            UPDATE push_subscriptions
            SET active = @Active, receive_own = @ReceiveOwn, updated_at = datetime('now')
            WHERE id = @Id
            """, new { req.Active, req.ReceiveOwn, Id = id });
    }

    public async Task DeleteAsync(int id)
    {
        using var con = _db.Abrir();
        await con.ExecuteAsync("DELETE FROM push_subscriptions WHERE id = @Id", new { Id = id });
    }

    public async Task<IEnumerable<PushSubscriptionRecord>> GetActiveToNotifyAsync(string? senderDeviceId)
    {
        using var con = _db.Abrir();
        return await con.QueryAsync<PushSubscriptionRecord>("""
            SELECT id, device_id AS DeviceId, alias, endpoint, p256dh, auth,
                   active, receive_own AS ReceiveOwn
            FROM push_subscriptions
            WHERE active = 1
              AND (@DeviceId IS NULL OR device_id != @DeviceId OR receive_own = 1)
            """, new { DeviceId = senderDeviceId });
    }

    public async Task<string?> GetAliasAsync(string deviceId)
    {
        using var con = _db.Abrir();
        return await con.ExecuteScalarAsync<string?>(
            "SELECT alias FROM push_subscriptions WHERE device_id = @DeviceId",
            new { DeviceId = deviceId });
    }

    public async Task DeleteByIdAsync(int id)
    {
        using var con = _db.Abrir();
        await con.ExecuteAsync("DELETE FROM push_subscriptions WHERE id = @Id", new { Id = id });
    }
}
