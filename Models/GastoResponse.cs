namespace FinanzasApp.Models;

public class GastoResponse
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public int CategoriaId { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public int MonedaId { get; set; }
    public string Moneda { get; set; } = string.Empty;
}