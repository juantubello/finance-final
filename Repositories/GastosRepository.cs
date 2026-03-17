using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;

namespace FinanzasApp.Repositories;

public class GastosRepository
{
    private readonly ConexionDB _conexionDB;

    public GastosRepository(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    public async Task<IEnumerable<Gasto>> ObtenerGastos()
    {
        using var con = _conexionDB.Abrir();
        var gastos = await con.QueryAsync<Gasto>("""
         SELECT id AS Id, fecha_hora AS FechaHora, descripcion AS Descripcion, importe AS Importe,
                 moneda_id AS MonedaId, categoria_id AS CategoriaId, created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM gastos
            ORDER BY fecha_hora DESC
    """);
        return gastos;
    }

    public async Task AgregarGasto(Gasto gasto)
    {
        using var con = _conexionDB.Abrir();
        var sql = @"INSERT INTO gastos (FechaHora, Descripcion, Importe, MonedaId, CategoriaId, CreatedAt, UpdatedAt) 
                    VALUES (@FechaHora, @Descripcion, @Importe, @MonedaId, @CategoriaId, @CreatedAt, @UpdatedAt)";
        await con.ExecuteAsync(sql, gasto);
    }

    public async Task ActualizarGasto(Gasto gasto)
    {
        using var con = _conexionDB.Abrir();
        var sql = @"UPDATE gastos SET FechaHora = @FechaHora, Descripcion = @Descripcion, Importe = @Importe, 
                    MonedaId = @MonedaId, CategoriaId = @CategoriaId, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        await con.ExecuteAsync(sql, gasto);
    }

    public async Task EliminarGasto(int id)
    {
        using var con = _conexionDB.Abrir();
        var sql = "DELETE FROM gastos WHERE Id = @Id";
        await con.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Gasto?> ObtenerGastoPorId(int id)
    {
        using var con = _conexionDB.Abrir();
        var sql = "SELECT * FROM gastos WHERE Id = @Id";
        var gasto = await con.QueryFirstOrDefaultAsync<Gasto>(sql, new { Id = id });
        return gasto;
    }
}