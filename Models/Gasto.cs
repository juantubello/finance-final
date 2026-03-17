namespace FinanzasApp.Models;

public class Gasto
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; } 
    public string Descripcion { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public int MonedaId { get; set; }
    public int? CategoriaId { get; set; }
    public DateTime CreatedAt { get; set; } 
    public DateTime UpdatedAt { get; set; }
}

public class GastoResponse
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? CategoryIcon { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public int MonedaId { get; set; }
    public int? CategoriaId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}