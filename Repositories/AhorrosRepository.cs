using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;

namespace FinanzasApp.Repositories;

public class AhorrosRepository
{
    private readonly ConexionDB _conexionDB;

    public AhorrosRepository(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    public async Task<IEnumerable<AhorroActivo>> ObtenerActivos()
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryAsync<AhorroActivo>("""
            SELECT id AS Id, nombre AS Nombre, ticker AS Ticker, tipo AS Tipo, decimales AS Decimales
            FROM ahorro_activos
            ORDER BY nombre
            """);
    }

    public async Task<IEnumerable<AhorroResponse>> ObtenerMovimientos(int? activoId = null)
    {
        using var con = _conexionDB.Abrir();
        var where = activoId.HasValue ? "WHERE a.id = @ActivoId" : "";
        return await con.QueryAsync<AhorroResponse>($"""
            SELECT s.id AS Id, s.fecha_hora AS DateTime, s.activo_id AS ActivoId,
                   aa.nombre AS Activo, aa.ticker AS Ticker, aa.tipo AS Tipo, aa.decimales AS Decimales,
                   s.cantidad AS Cantidad, s.precio_ars AS PrecioArs, s.descripcion AS Description,
                   s.created_at AS CreatedAt, s.updated_at AS UpdatedAt
            FROM ahorros s
            INNER JOIN ahorro_activos aa ON aa.id = s.activo_id
            {where}
            ORDER BY s.fecha_hora DESC
            """, new { ActivoId = activoId });
    }

    public async Task<IEnumerable<AhorroBalanceResponse>> ObtenerBalances()
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryAsync<AhorroBalanceResponse>("""
            SELECT aa.id AS ActivoId, aa.nombre AS Activo, aa.ticker AS Ticker,
                   aa.tipo AS Tipo, aa.decimales AS Decimales,
                   COALESCE(SUM(s.cantidad), 0.0) AS Balance
            FROM ahorro_activos aa
            LEFT JOIN ahorros s ON s.activo_id = aa.id
            GROUP BY aa.id, aa.nombre, aa.ticker, aa.tipo, aa.decimales
            ORDER BY aa.nombre
            """);
    }

    public async Task AgregarMovimiento(Ahorro ahorro)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("""
            INSERT INTO ahorros (fecha_hora, activo_id, cantidad, precio_ars, descripcion, created_at, updated_at)
            VALUES (@FechaHora, @ActivoId, @Cantidad, @PrecioArs, @Descripcion, @Now, @Now)
            """, new
        {
            FechaHora   = ahorro.DateTime,
            ActivoId    = ahorro.ActivoId,
            Cantidad    = ahorro.Cantidad,
            PrecioArs   = ahorro.PrecioArs,
            Descripcion = ahorro.Description,
            Now         = DateTime.UtcNow
        });
    }

    public async Task ActualizarMovimiento(Ahorro ahorro)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("""
            UPDATE ahorros
            SET fecha_hora = @FechaHora, activo_id = @ActivoId, cantidad = @Cantidad,
                precio_ars = @PrecioArs, descripcion = @Descripcion, updated_at = @Now
            WHERE id = @Id
            """, new
        {
            Id          = ahorro.Id,
            FechaHora   = ahorro.DateTime,
            ActivoId    = ahorro.ActivoId,
            Cantidad    = ahorro.Cantidad,
            PrecioArs   = ahorro.PrecioArs,
            Descripcion = ahorro.Description,
            Now         = DateTime.UtcNow
        });
    }

    public async Task EliminarMovimiento(int id)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("DELETE FROM ahorros WHERE id = @Id", new { Id = id });
    }
}
