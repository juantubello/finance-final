using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;

namespace FinanzasApp.Repositories;

public class CedearRepository
{
    private readonly ConexionDB _conexionDB;

    public CedearRepository(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    public async Task<CedearQuote?> ObtenerQuoteDelDia(string ticker, string date)
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryFirstOrDefaultAsync<CedearQuote>("""
            SELECT ticker, date, data, datetime
            FROM cedear_quotes
            WHERE ticker = @Ticker AND date = @Date
            """, new { Ticker = ticker, Date = date });
    }

    public async Task GuardarQuote(string ticker, string date, string data)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("""
            INSERT OR REPLACE INTO cedear_quotes (ticker, date, data, datetime)
            VALUES (@Ticker, @Date, @Data, @DateTime)
            """, new
        {
            Ticker   = ticker,
            Date     = date,
            Data     = data,
            DateTime = System.DateTime.UtcNow.ToString("o")
        });
    }
}
