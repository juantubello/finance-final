using System.Text.Json;
using FinanzasApp.Models;
using FinanzasApp.Repositories;
using FinanzasApp.Services;

namespace FinanzasApp.Endpoints;

public static class CardStatementsEndpoint
{
    public static void MapCardStatementsEndpoints(this WebApplication app)
    {
        // ── Obtener statement por cardType + año/mes ────────────────────────
        app.MapGet("/cards/statements", async (string cardType, int year, int month, CardStatementsRepository repo) =>
        {
            var statement = await repo.GetStatementAsync(cardType, year, month);
            if (statement is null) return Results.NotFound();
            return Results.Ok(statement);
        });

        // ── Gastos de un statement ──────────────────────────────────────────
        app.MapGet("/cards/statements/{id}/expenses", async (int id, CardStatementsRepository repo) =>
            Results.Ok(await repo.GetExpensesAsync(id)));

        // ── Parse only (no save) ────────────────────────────────────────────
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

        // ── Parse + save to DB ──────────────────────────────────────────────
        // multipart/form-data:
        //   pdf      → archivo PDF
        //   data     → JSON con SaveCardStatementRequest (sin Expenses si el front lo parsea aparte)
        //              o bien el JSON completo ya parseado más cardId y exchangeRateUsd
        app.MapPost("/cards/statements", async (
            IFormCollection form,
            BbvaPdfParser parser,
            CardStatementsRepository repo,
            IConfiguration config) =>
        {
            // 1. Archivo PDF
            var pdf = form.Files.GetFile("pdf");
            if (pdf is null)
                return Results.BadRequest("Se requiere el archivo pdf.");

            if (pdf.ContentType != "application/pdf" &&
                !pdf.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return Results.BadRequest("El archivo debe ser un PDF.");

            // 2. JSON con el request
            var dataJson = form["data"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(dataJson))
                return Results.BadRequest("Se requiere el campo 'data' con el JSON.");

            SaveCardStatementRequest? req;
            try
            {
                req = JsonSerializer.Deserialize<SaveCardStatementRequest>(dataJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return Results.BadRequest("El campo 'data' no es JSON válido.");
            }

            if (req is null || req.CardId == 0)
                return Results.BadRequest("cardId es requerido.");

            // 3. Guardar PDF en disco
            var storagePath = config["StatementStoragePath"]
                ?? throw new Exception("StatementStoragePath no configurado.");

            Directory.CreateDirectory(storagePath);

            var fileName = $"{req.CardId}_{req.StatementYear}_{req.StatementMonth:D2}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.pdf";
            var filePath = Path.Combine(storagePath, fileName);

            using (var fs = File.Create(filePath))
                await pdf.CopyToAsync(fs);

            // 4. Insertar en DB
            var statementId = await repo.InsertStatementAsync(req, fileName);

            return Results.Ok(new { statementId, pdfPath = fileName });
        })
        .DisableAntiforgery();

        // ── Eliminar statement (cascade expenses + archivo PDF) ─────────────
        app.MapDelete("/cards/statements/{id}", async (int id, CardStatementsRepository repo, IConfiguration config) =>
        {
            var pdfPath = await repo.DeleteStatementAsync(id);
            if (pdfPath is null) return Results.NotFound();

            var storagePath = config["StatementStoragePath"]
                ?? throw new Exception("StatementStoragePath no configurado.");
            var filePath = Path.Combine(storagePath, pdfPath);
            if (File.Exists(filePath)) File.Delete(filePath);

            return Results.NoContent();
        });

        // ── Descargar PDF de un resumen ─────────────────────────────────────
        app.MapGet("/cards/statements/{id}/pdf", async (int id, CardStatementsRepository repo, IConfiguration config) =>
        {
            var info = await repo.GetPdfInfoAsync(id);
            if (info is null)
                return Results.NotFound();

            var storagePath = config["StatementStoragePath"]
                ?? throw new Exception("StatementStoragePath no configurado.");

            var filePath = Path.Combine(storagePath, info.Value.PdfPath);
            if (!File.Exists(filePath))
                return Results.NotFound();

            var friendlyName = $"Resumen_{info.Value.CardType}_{info.Value.Year}_{info.Value.Month:D2}.pdf";
            var stream = File.OpenRead(filePath);
            return Results.Stream(stream, "application/pdf", friendlyName, enableRangeProcessing: true);
        });
    }
}
