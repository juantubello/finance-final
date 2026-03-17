namespace FinanzasApp.Models;

public class Moneda
{
    public int CurrencyId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string CurrencyDescription { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
}