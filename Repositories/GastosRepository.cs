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

    public async Task<IEnumerable<GastoResponse>> ObtenerGastos(int year, int month)
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryAsync<GastoResponse>("""
        SELECT t1.id AS Id, fecha_hora AS DateTime, t1.descripcion AS Description, importe AS Amount, t3.nombre as Category, t3.icono as CategoryIcon,
               t2.codigo as Currency, t2.simbolo as CurrencySymbol, moneda_id AS CurrencyId, categoria_id AS CategoryId, t1.created_at AS CreatedAt, t1.updated_at AS UpdatedAt
            FROM gastos as t1
            INNER JOIN monedas as t2
                on t1.moneda_id = t2.id 
            INNER JOIN categorias as t3
                on t1.categoria_id = t3.id
        WHERE strftime('%Y', t1.fecha_hora) = @Year
          AND strftime('%m', t1.fecha_hora) = @Month
        ORDER BY fecha_hora DESC
        """, new { Year = year.ToString(), Month = month.ToString("D2") });
    }

    public async Task AgregarGasto(Gasto gasto)
    {
        using var con = _conexionDB.Abrir();
        var sql = @"INSERT INTO gastos (fecha_hora, descripcion, importe, moneda_id, categoria_id, created_at, updated_at)
                    VALUES (@FechaHora, @Descripcion, @Importe, @MonedaId, @CategoriaId, @CreatedAt, @UpdatedAt)";
        await con.ExecuteAsync(sql, new
        {
            FechaHora   = gasto.DateTime,
            Descripcion = gasto.Description,
            Importe     = gasto.Amount,
            MonedaId    = gasto.CurrencyId,
            CategoriaId = gasto.CategoryId,
            CreatedAt   = DateTime.UtcNow,
            UpdatedAt   = DateTime.UtcNow
        });
    }

    public async Task ActualizarGasto(Gasto gasto)
    {
        using var con = _conexionDB.Abrir();
        var sql = @"UPDATE gastos SET fecha_hora = @FechaHora, descripcion = @Descripcion, importe = @Importe,
                    moneda_id = @MonedaId, categoria_id = @CategoriaId, updated_at = @UpdatedAt WHERE id = @Id";
        await con.ExecuteAsync(sql, new
        {
            Id          = gasto.Id,
            FechaHora   = gasto.DateTime,
            Descripcion = gasto.Description,
            Importe     = gasto.Amount,
            MonedaId    = gasto.CurrencyId,
            CategoriaId = gasto.CategoryId,
            UpdatedAt   = DateTime.UtcNow
        });
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

   public async Task<IEnumerable<GastosByCategoryResponse>> GetGastosByCategories(int year, int month)
{
    var gastosByCategories = new List<GastosByCategoryResponse>();

    using var con = _conexionDB.Abrir();
    var sql = """
        SELECT id, fecha_hora as Datetime, descripcion as Description, importe as Amount, moneda_id as CurrencyId, categoria_id as CategoryId
        FROM gastos
        WHERE strftime('%Y', fecha_hora) = @Year
          AND strftime('%m', fecha_hora) = @Month
        ORDER BY fecha_hora DESC
        """;

    var gastos = await con.QueryAsync<Gasto>(sql, new { Year = year.ToString(), Month = month.ToString("D2") });
    var currencies = await GetCurrencies();
    var categories = await GetCategories();
    foreach (var gasto in gastos)
    {
        var currency = currencies.FirstOrDefault(g => g.CurrencyId == gasto.CurrencyId );
        if (currency is null) continue;

        var category = categories.FirstOrDefault(c => c.CategoryId == gasto.CategoryId);
        if (category is null) continue;

        var gastoByCategory = gastosByCategories.FirstOrDefault(c => c.CategoryId == gasto.CategoryId && c.CurrencyId == gasto.CurrencyId);

        if (gastoByCategory is null)
        {
            gastosByCategories.Add(new GastosByCategoryResponse
            {
                CategoryId = gasto.CategoryId ?? 0,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
                CategoryIcon = category.CategoryIcon,
                Amount = gasto.Amount,
                CurrencyId = gasto.CurrencyId,
                Currency = currency.Currency,
                CurrencySymbol = currency.CurrencySymbol
            });
        }
        else
        {
            gastoByCategory.Amount += gasto.Amount;
        }
    }
    return gastosByCategories;
}
    public async Task<IEnumerable<Currencies>> GetCurrencies()
    {
        using var con = _conexionDB.Abrir();
        var sql = """ 
        SELECT id as CurrencyId,
               codigo  as Currency, nombre  as CurrencyDescription,
               simbolo as CurrencySymbol
        FROM monedas
    """;
        var currencies = await con.QueryAsync<Currencies>(sql);
        return currencies;
    }
    public async Task<IEnumerable<Categories>> GetCategories()
    {
        using var con = _conexionDB.Abrir();
        var sql = """
        SELECT id as CategoryId,
               nombre      as CategoryName,
               descripcion as CategoryDescription,
               icono       as CategoryIcon
        FROM categorias
    """;
        var categories = await con.QueryAsync<Categories>(sql);
        return categories;
    }

public async Task<GastosByCategoryRangeResponse> GetGastosByCategoriesRange(int yearFrom, int monthFrom, int yearTo, int monthTo)
{
    var response = new GastosByCategoryRangeResponse();

    using var con = _conexionDB.Abrir();
    var sql = """
        SELECT id, fecha_hora as Datetime, descripcion as Description, importe as Amount, moneda_id as CurrencyId, categoria_id as CategoryId
        FROM gastos
        WHERE (strftime('%Y%m', fecha_hora) >= @From
          AND strftime('%Y%m', fecha_hora) <= @To)
        ORDER BY fecha_hora DESC
        """;

    var from = $"{yearFrom}{monthFrom:D2}";
    var to   = $"{yearTo}{monthTo:D2}";

    var gastos     = await con.QueryAsync<Gasto>(sql, new { From = from, To = to });
    var currencies = await GetCurrencies();
    var categories = await GetCategories();

    foreach (var gasto in gastos)
    {
        var currency = currencies.FirstOrDefault(g => g.CurrencyId == gasto.CurrencyId);
        if (currency is null) continue;

        var category = categories.FirstOrDefault(c => c.CategoryId == gasto.CategoryId);
        if (category is null) continue;

        // --- TOTALS ---
        var total = response.Totals.FirstOrDefault(t => t.CategoryId == gasto.CategoryId && t.CurrencyId == gasto.CurrencyId);
        if (total is null)
        {
            response.Totals.Add(new GastosByCategoryResponse
            {
                CategoryId          = gasto.CategoryId ?? 0,
                CategoryName        = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
                CategoryIcon        = category.CategoryIcon,
                Amount              = gasto.Amount,
                CurrencyId          = gasto.CurrencyId,
                Currency            = currency.Currency,
                CurrencySymbol      = currency.CurrencySymbol
            });
        }
        else
        {
            total.Amount += gasto.Amount;
        }

        // --- BY MONTH ---
        var gastoDate = gasto.DateTime;
        var monthlyEntry = response.ByMonth.FirstOrDefault(m => m.Year == gastoDate.Year && m.Month == gastoDate.Month);
        if (monthlyEntry is null)
        {
            monthlyEntry = new MonthlyGastos { Year = gastoDate.Year, Month = gastoDate.Month };
            response.ByMonth.Add(monthlyEntry);
        }

        var monthlyCategory = monthlyEntry.Categories.FirstOrDefault(c => c.CategoryId == gasto.CategoryId && c.CurrencyId == gasto.CurrencyId);
        if (monthlyCategory is null)
        {
            monthlyEntry.Categories.Add(new GastosByCategoryResponse
            {
                CategoryId          = gasto.CategoryId ?? 0,
                CategoryName        = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
                CategoryIcon        = category.CategoryIcon,
                Amount              = gasto.Amount,
                CurrencyId          = gasto.CurrencyId,
                Currency            = currency.Currency,
                CurrencySymbol      = currency.CurrencySymbol
            });
        }
        else
        {
            monthlyCategory.Amount += gasto.Amount;
        }
    }

    return response;
}
}