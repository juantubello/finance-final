using Microsoft.Data.Sqlite;

namespace FinanzasApp.Database;

public class ConexionDB
{
    private readonly string _connectionString;

    public ConexionDB(string rutaArchivo)
    {
        _connectionString = $"Data Source={rutaArchivo};Mode=ReadWriteCreate;";
    }

    public SqliteConnection Abrir()
    {
        var con = new SqliteConnection(_connectionString);
        con.Open();
        using var cmd = con.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON";
        cmd.ExecuteNonQuery();
        return con;
    }
}