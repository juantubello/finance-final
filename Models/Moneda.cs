namespace FinanzasApp.Models;

public class Moneda
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Simbolo { get; set; } = string.Empty;
}