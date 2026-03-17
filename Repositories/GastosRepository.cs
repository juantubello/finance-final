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

    // public async Task<IEnumerable<Gasto>> ObtenerGastos()
    // {
    //     using var con = _conexionDB.Abrir();
    //     var gastos = await con.QueryAsync<Gasto>("""
    //      SELECT id AS Id, fecha_hora AS FechaHora, descripcion AS Descripcion, importe AS Importe,
    //              moneda_id AS MonedaId, categoria_id AS CategoriaId, created_at AS CreatedAt, updated_at AS UpdatedAt
    //         FROM gastos
    //         ORDER BY fecha_hora DESC
    // """);
    //     return gastos;
    // }

       public async Task<IEnumerable<GastoResponse>> ObtenerGastos()
    {
        using var con = _conexionDB.Abrir();
        var gastos = await con.QueryAsync<GastoResponse>("""
        SELECT t1.id AS Id, fecha_hora AS DateTime, t1.descripcion AS Description, importe AS Amount, t3.nombre as Category, t3.icono as CategoryIcon,
               t2.codigo as Currency, t2.simbolo as CurrencySymbol, moneda_id AS CurrencyId, categoria_id AS CategoryId, t1.created_at AS CreatedAt, t1.updated_at AS UpdatedAt
            FROM gastos as t1
            INNER JOIN monedas as t2
                on t1.moneda_id = t2.id 
            INNER JOIN categorias as t3
                on t1.categoria_id = t3.id
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

    public async Task<GastoResponse?> ObtenerGastoPorId(int id)
    {
        using var con = _conexionDB.Abrir();
        // var sql = "SELECT * FROM gastos WHERE Id = @Id";
        var sql = """
        SELECT t1.id AS Id, fecha_hora AS DateTime, t1.descripcion AS Description, importe AS Amount, t3.nombre as Category, t3.icono as CategoryIcon,
               t2.codigo as Currency, t2.simbolo as CurrencySymbol, moneda_id AS CurrencyId, categoria_id AS CategoryId, t1.created_at AS CreatedAt, t1.updated_at AS UpdatedAt
            FROM gastos as t1
            INNER JOIN monedas as t2
                ON t1.moneda_id = t2.id 
            INNER JOIN categorias as t3
                ON t1.categoria_id = t3.id
            WHERE t1.id = @Id
            ORDER BY fecha_hora DESC

    """;
        var gasto = await con.QueryFirstOrDefaultAsync<GastoResponse>(sql, new { Id = id });
        return gasto;
    }
}