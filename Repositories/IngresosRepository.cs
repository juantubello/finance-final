using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;

namespace FinanzasApp.Repositories;

public class IngresosRepository
{
    private readonly ConexionDB _conexionDB;

    public IngresosRepository(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    public async Task<IEnumerable<IngresoResponse>> ObtenerIngresos(int year, int month)
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryAsync<IngresoResponse>("""
            SELECT i.id AS Id, i.fecha_hora AS DateTime, i.descripcion AS Description, i.importe AS Amount,
                   i.moneda_id AS CurrencyId, m.codigo AS Currency, m.simbolo AS CurrencySymbol,
                   i.categoria_id AS CategoryId, c.nombre AS Category, c.icono AS CategoryIcon, c.color AS CategoryColor,
                   i.created_at AS CreatedAt, i.updated_at AS UpdatedAt
            FROM ingresos i
            INNER JOIN monedas m ON m.id = i.moneda_id
            LEFT JOIN categorias c ON c.id = i.categoria_id
            WHERE strftime('%Y', i.fecha_hora) = @Year
              AND strftime('%m', i.fecha_hora) = @Month
            ORDER BY i.fecha_hora DESC
            """, new { Year = year.ToString(), Month = month.ToString("D2") });
    }

    public async Task<IngresoResponse?> ObtenerIngresoPorId(int id)
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryFirstOrDefaultAsync<IngresoResponse>("""
            SELECT i.id AS Id, i.fecha_hora AS DateTime, i.descripcion AS Description, i.importe AS Amount,
                   i.moneda_id AS CurrencyId, m.codigo AS Currency, m.simbolo AS CurrencySymbol,
                   i.categoria_id AS CategoryId, c.nombre AS Category, c.icono AS CategoryIcon, c.color AS CategoryColor,
                   i.created_at AS CreatedAt, i.updated_at AS UpdatedAt
            FROM ingresos i
            INNER JOIN monedas m ON m.id = i.moneda_id
            LEFT JOIN categorias c ON c.id = i.categoria_id
            WHERE i.id = @Id
            """, new { Id = id });
    }

    public async Task AgregarIngreso(Ingreso ingreso)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("""
            INSERT INTO ingresos (fecha_hora, descripcion, importe, moneda_id, categoria_id, created_at, updated_at)
            VALUES (@FechaHora, @Descripcion, @Importe, @MonedaId, @CategoriaId, @Now, @Now)
            """, new
        {
            FechaHora   = ingreso.DateTime,
            Descripcion = ingreso.Description,
            Importe     = ingreso.Amount,
            MonedaId    = ingreso.CurrencyId,
            CategoriaId = ingreso.CategoryId,
            Now         = DateTime.UtcNow
        });
    }

    public async Task ActualizarIngreso(Ingreso ingreso)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("""
            UPDATE ingresos
            SET fecha_hora = @FechaHora, descripcion = @Descripcion, importe = @Importe,
                moneda_id = @MonedaId, categoria_id = @CategoriaId, updated_at = @Now
            WHERE id = @Id
            """, new
        {
            Id          = ingreso.Id,
            FechaHora   = ingreso.DateTime,
            Descripcion = ingreso.Description,
            Importe     = ingreso.Amount,
            MonedaId    = ingreso.CurrencyId,
            CategoriaId = ingreso.CategoryId,
            Now         = DateTime.UtcNow
        });
    }

    public async Task EliminarIngreso(int id)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("DELETE FROM ingresos WHERE id = @Id", new { Id = id });
    }

    public async Task<decimal> ObtenerTotalArs()
    {
        using var con = _conexionDB.Abrir();
        return await con.ExecuteScalarAsync<decimal>("""
            SELECT COALESCE(SUM(i.importe), 0.0)
            FROM ingresos i
            INNER JOIN monedas m ON m.id = i.moneda_id
            WHERE m.codigo = 'ARS'
            """);
    }

    public async Task<decimal> ObtenerTotalUsd()
    {
        using var con = _conexionDB.Abrir();
        return await con.ExecuteScalarAsync<decimal>("""
            SELECT COALESCE(SUM(i.importe), 0.0)
            FROM ingresos i
            INNER JOIN monedas m ON m.id = i.moneda_id
            WHERE m.codigo = 'USD'
            """);
    }
}
