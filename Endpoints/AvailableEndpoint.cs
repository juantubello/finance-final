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
            var client = httpClientFactory.CreateClient();

            // ── Cotizaciones en paralelo ────────────────────────────────────
            var (blueVenta, usdcArs, fxRates) = await FetchRatesAsync(client);

            // ── Ingresos ────────────────────────────────────────────────────
            var totalIngresosArs = await ingresosRepo.ObtenerTotalArs();
            var totalIngresosUsd = await ingresosRepo.ObtenerTotalUsd();
            var totalIngresosUsdEnArs = totalIngresosUsd * usdcArs;
            var totalIngresos = totalIngresosArs + totalIngresosUsdEnArs;

            // ── Gastos por moneda ───────────────────────────────────────────
            using var con = db.Abrir();
            var gastosPorMoneda = (await con.QueryAsync<(string Codigo, decimal Total)>("""
                SELECT m.codigo AS Codigo, COALESCE(SUM(g.importe), 0.0) AS Total
                FROM gastos g
                INNER JOIN monedas m ON m.id = g.moneda_id
                GROUP BY m.codigo
                """)).ToDictionary(r => r.Codigo, r => r.Total);

            gastosPorMoneda.TryGetValue("ARS", out var gastosArs);
            gastosPorMoneda.TryGetValue("USD", out var gastosUsd);
            gastosPorMoneda.TryGetValue("EUR", out var gastosEur);
            gastosPorMoneda.TryGetValue("GBP", out var gastosGbp);

            // EUR y GBP → USD → ARS usando dólar blue venta
            // fxRates["EUR"] = cuántos EUR equivale 1 USD → 1 EUR = blueVenta / fxRates["EUR"]
            var gastosUsdEnArs  = gastosUsd * blueVenta;
            var gastosEurEnArs  = gastosEur > 0 && fxRates.TryGetValue("EUR", out var eurPerUsd) && eurPerUsd > 0
                                    ? gastosEur / eurPerUsd * blueVenta : 0;
            var gastosGbpEnArs  = gastosGbp > 0 && fxRates.TryGetValue("GBP", out var gbpPerUsd) && gbpPerUsd > 0
                                    ? gastosGbp / gbpPerUsd * blueVenta : 0;

            var totalGastos = gastosArs + gastosUsdEnArs + gastosEurEnArs + gastosGbpEnArs;

            return Results.Ok(new AvailableResponse
            {
                TotalIngresos       = Math.Round(totalIngresos, 2),
                TotalGastos         = Math.Round(totalGastos, 2),
                Disponible          = Math.Round(totalIngresos - totalGastos, 2),
                Moneda              = "ARS",
                UsdcArs             = Math.Round(usdcArs, 2),
                BlueVenta           = Math.Round(blueVenta, 2),
                TotalIngresosUsd    = Math.Round(totalIngresosUsd, 2),
                TotalIngresosUsdArs = Math.Round(totalIngresosUsdEnArs, 2),
            });
        });
    }

    // ── Obtiene blue venta, USDC/ARS y tasas EUR/GBP en paralelo ───────────
    private static async Task<(decimal BlueVenta, decimal UsdcArs, Dictionary<string, decimal> FxRates)>
        FetchRatesAsync(HttpClient client)
    {
        var blueVenta = 0m;
        var usdcArs   = 0m;
        var fxRates   = new Dictionary<string, decimal>();

        var blueTask = client.GetStringAsync("https://dolarapi.com/v1/dolares/blue");
        var usdcTask = client.GetStringAsync("https://criptoya.com/api/usdc/ars/1");
        var fxTask   = client.GetStringAsync("https://open.er-api.com/v6/latest/USD");

        await Task.WhenAll(blueTask, usdcTask, fxTask);

        try
        {
            using var doc = JsonDocument.Parse(await blueTask);
            blueVenta = doc.RootElement.GetProperty("venta").GetDecimal();
        }
        catch { }

        try
        {
            using var doc = JsonDocument.Parse(await usdcTask);
            var precios = new List<decimal>();
            foreach (var exchange in doc.RootElement.EnumerateObject())
                if (exchange.Value.TryGetProperty("totalBid", out var bid) && bid.ValueKind == JsonValueKind.Number)
                    precios.Add(bid.GetDecimal());
            if (precios.Count > 0) usdcArs = precios.Average();
        }
        catch { }

        try
        {
            using var doc = JsonDocument.Parse(await fxTask);
            if (doc.RootElement.TryGetProperty("rates", out var rates))
                foreach (var code in new[] { "EUR", "GBP", "CHF", "BRL" })
                    if (rates.TryGetProperty(code, out var val) && val.ValueKind == JsonValueKind.Number)
                        fxRates[code] = val.GetDecimal();
        }
        catch { }

        // Si blue falla, usar usdcArs como fallback
        if (blueVenta == 0) blueVenta = usdcArs;

        return (blueVenta, usdcArs, fxRates);
    }
}
