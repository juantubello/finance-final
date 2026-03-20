using System.Text.Json;
using System.Text.RegularExpressions;
using FinanzasApp.Repositories;

namespace FinanzasApp.Endpoints;

public static class CedearEndpoints
{
    public static void MapCedearEndpoints(this WebApplication app)
    {
        app.MapGet("/cedears/spy", async (CedearRepository repo, IHttpClientFactory httpClientFactory) =>
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            // Devolver datos del día si ya los tenemos
            var cached = await repo.ObtenerQuoteDelDia("SPY", today);
            if (cached is not null)
                return Results.Ok(JsonSerializer.Deserialize<object>(cached.Data));

            // Consultar Portfolio Personal
            try
            {
                var client = httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "Finance-App-Backend/1.0");
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/html, */*");

                var html = await client.GetStringAsync("https://www.portfoliopersonal.com/Cotizaciones/Cedears");

                var spyJson = ExtraerSpyDeHtml(html);
                if (spyJson is null)
                    return Results.NotFound(new { error = "SPY_NOT_FOUND", message = "No se pudo encontrar la cotización de SPY." });

                await repo.GuardarQuote("SPY", today, spyJson);

                return Results.Ok(JsonSerializer.Deserialize<object>(spyJson));
            }
            catch (TaskCanceledException)
            {
                return Results.Json(new { error = "TIMEOUT", message = "Timeout al consultar Portfolio Personal." }, statusCode: 504);
            }
            catch (HttpRequestException ex)
            {
                return Results.Json(new { error = "NETWORK_ERROR", message = ex.Message }, statusCode: 503);
            }
            catch (Exception ex)
            {
                return Results.Json(new { error = "INTERNAL_ERROR", message = ex.Message }, statusCode: 500);
            }
        });
    }

    private static string? ExtraerSpyDeHtml(string html)
    {
        // Método 1: buscar arrays JSON con ticker
        var arrayPatterns = new[]
        {
            @"(\[[\s]*\{[^\[\]]*""ticker""[^\[\]]*\}[^\]]*\])",
            @"var\s+\w+\s*=\s*(\[[^\]]+\])",
            @"window\.\w+\s*=\s*(\[[^\]]+\])",
            @"(?:const|let)\s+\w+\s*=\s*(\[[^\]]+\])"
        };

        foreach (var pattern in arrayPatterns)
        {
            foreach (Match match in Regex.Matches(html, pattern))
            {
                var jsonStr = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
                jsonStr = Regex.Replace(jsonStr, @"^(var|const|let)\s+\w+\s*=\s*", "");
                jsonStr = Regex.Replace(jsonStr, @"^window\.\w+\s*=\s*", "");

                try
                {
                    using var doc = JsonDocument.Parse(jsonStr);
                    if (doc.RootElement.ValueKind != JsonValueKind.Array) continue;

                    foreach (var item in doc.RootElement.EnumerateArray())
                    {
                        if (item.TryGetProperty("ticker", out var ticker) && ticker.GetString() == "SPY")
                            return item.GetRawText();
                    }
                }
                catch { /* seguir con el siguiente */ }
            }
        }

        // Método 2: buscar el objeto individual de SPY
        var spyIndex = html.IndexOf("\"ticker\":\"SPY\"", StringComparison.Ordinal);
        if (spyIndex == -1) return null;

        var objStart = spyIndex;
        while (objStart > 0 && objStart > spyIndex - 2000)
        {
            if (html[objStart] == '{') break;
            objStart--;
        }
        if (html[objStart] != '{') return null;

        var objEnd = objStart;
        var braceCount = 0;
        var inString = false;
        var escape = false;

        while (objEnd < html.Length && objEnd < objStart + 5000)
        {
            var c = html[objEnd];
            if (escape) { escape = false; objEnd++; continue; }
            if (c == '\\') { escape = true; objEnd++; continue; }
            if (c == '"') inString = !inString;
            if (!inString)
            {
                if (c == '{') braceCount++;
                else if (c == '}') { braceCount--; if (braceCount == 0) { objEnd++; break; } }
            }
            objEnd++;
        }

        if (braceCount != 0) return null;

        try
        {
            var candidate = html.Substring(objStart, objEnd - objStart);
            using var doc = JsonDocument.Parse(candidate);
            if (doc.RootElement.TryGetProperty("ticker", out var t) && t.GetString() == "SPY")
                return candidate;
        }
        catch { }

        return null;
    }
}
