namespace FinanzasApp.Repositories;

using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;

public class MonedaRepository
{
    private readonly ConexionDB _conexionDB;

    public MonedaRepository(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    public IEnumerable<Moneda> ObtenerTodas()
    {
        using var conexion = _conexionDB.Abrir();
        string sql = "SELECT * FROM Monedas";
        return conexion.Query<Moneda>(sql).ToList();
    }

    public async Task<IEnumerable<Moneda>> ObtenerMonedas()
    {
        using var con = _conexionDB.Abrir();
        var sql = """
         SELECT id      as CurrencyId,
                codigo  as Currency,
                nombre  as CurrencyDescription,
                simbolo as CurrencySymbol
        FROM monedas
        """;
        var categorias = await con.QueryAsync<Moneda>(sql);
        return categorias;
    }
}