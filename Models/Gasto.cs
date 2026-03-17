namespace FinanzasApp.Models;

public class Gasto
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public int? CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class GastoResponse
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int? CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public int CurrencyId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Categories
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryDescription { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
}

public class Currencies
{
    public int CurrencyId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CurrencyDescription { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
}

public class GastosByCategoryResponse
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryDescription { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;

}
public class GastosByCategoryRangeResponse
{
    public List<GastosByCategoryResponse> Totals { get; set; } = new();
    public List<MonthlyGastos> ByMonth { get; set; } = new();
}

public class MonthlyGastos
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<GastosByCategoryResponse> Categories { get; set; } = new();
}