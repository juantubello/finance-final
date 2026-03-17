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
        return new SqliteConnection(_connectionString);
    }
}