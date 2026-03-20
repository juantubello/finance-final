namespace FinanzasApp.Models;

public class CategoriaIngreso
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryDescription { get; set; }
    public string? CategoryIcon { get; set; }
    public string? CategoryColor { get; set; }
}

public class Ingreso
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public int? CategoryId { get; set; }
    public decimal? PrecioArs { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class IngresoResponse
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CurrencyId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public string? CategoryColor { get; set; }
    public decimal? PrecioArs { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AvailableResponse
{
    public decimal TotalIngresos { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal Disponible { get; set; }
    public string Moneda { get; set; } = "ARS";
    public decimal UsdcArs { get; set; }
    public decimal TotalIngresosUsd { get; set; }
    public decimal TotalIngresosUsdArs { get; set; }
}
