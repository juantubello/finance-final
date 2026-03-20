namespace FinanzasApp.Models;

public class AhorroActivo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;  // fiat, crypto, cedear
    public int Decimales { get; set; }
}

public class Ahorro
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public int ActivoId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal? PrecioArs { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AhorroResponse
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public int ActivoId { get; set; }
    public string Activo { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Decimales { get; set; }
    public decimal Cantidad { get; set; }
    public decimal? PrecioArs { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AhorroBalanceResponse
{
    public int ActivoId { get; set; }
    public string Activo { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public int Decimales { get; set; }
    public decimal Balance { get; set; }
}
