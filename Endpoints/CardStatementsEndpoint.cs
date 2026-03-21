using FinanzasApp.Services;

namespace FinanzasApp.Endpoints;

public static class CardStatementsEndpoint
{
    public static void MapCardStatementsEndpoints(this WebApplication app)
    {
        app.MapPost("/cards/statements/parse", async (IFormFile pdf, BbvaPdfParser parser) =>
        {
            if (pdf.ContentType != "application/pdf" &&
                !pdf.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return Results.BadRequest("El archivo debe ser un PDF.");

            using var ms = new MemoryStream();
            await pdf.CopyToAsync(ms);

            return Results.Ok(parser.Parse(ms.ToArray()));
        })
        .DisableAntiforgery();
    }
}
