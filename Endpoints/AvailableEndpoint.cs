using System.Text.Json;
using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;
using FinanzasApp.Repositories;

namespace FinanzasApp.Endpoints;

public static class AvailableEndpoints
{
    public static void MapAvailableEndpoints(this WebApplication app)
    {
        app.MapGet("/available", async (IngresosRepository ingresosRepo, ConexionDB db, IHttpClientFactory httpClientFactory) =>
        {
            // 1. Ingresos ARS directos
            var totalIngresosArs = await ingresosRepo.ObtenerTotalArs();

            // 2. Ingresos en USD → convertir a ARS usando cotización USDC de CriptoYa
            var totalIngresosUsd = await ingresosRepo.ObtenerTotalUsd();
            var usdcArs = 0m;

            if (totalIngresosUsd > 0)
            {
                try
                {
                    var client = httpClientFactory.CreateClient();
                    var json = await client.GetStringAsync("https://criptoya.com/api/usdc/ars/1");
                    using var doc = JsonDocument.Parse(json);

                    var precios = new List<decimal>();
                    foreach (var exchange in doc.RootElement.EnumerateObject())
                    {
                        if (exchange.Value.TryGetProperty("totalBid", out var bid) && bid.ValueKind == JsonValueKind.Number)
                            precios.Add(bid.GetDecimal());
                    }

                    if (precios.Count > 0)
                        usdcArs = precios.Average();
                }
                catch
                {
                    // Si la API falla, los ingresos en USD no se suman
                    usdcArs = 0m;
                }
            }

            var totalIngresosUsdEnArs = totalIngresosUsd * usdcArs;
            var totalIngresos = totalIngresosArs + totalIngresosUsdEnArs;

            // 3. Gastos ARS
            using var con = db.Abrir();
            var totalGastos = await con.ExecuteScalarAsync<decimal>("""
                SELECT COALESCE(SUM(g.importe), 0.0)
                FROM gastos g
                INNER JOIN monedas m ON m.id = g.moneda_id
                WHERE m.codigo = 'ARS'
                """);

            return Results.Ok(new AvailableResponse
            {
                TotalIngresos        = Math.Round(totalIngresos, 2),
                TotalGastos          = Math.Round(totalGastos, 2),
                Disponible           = Math.Round(totalIngresos - totalGastos, 2),
                Moneda               = "ARS",
                UsdcArs              = Math.Round(usdcArs, 2),
                TotalIngresosUsd     = Math.Round(totalIngresosUsd, 2),
                TotalIngresosUsdArs  = Math.Round(totalIngresosUsdEnArs, 2)
            });
        });
    }
}
