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

    public List<Moneda> ObtenerTodas()
    {
        using var conexion = _conexionDB.Abrir();
        string sql = "SELECT * FROM Monedas";
        return conexion.Query<Moneda>(sql).ToList();
    }
}