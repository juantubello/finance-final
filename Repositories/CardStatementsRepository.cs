using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;

namespace FinanzasApp.Repositories;

public class CardStatementsRepository
{
    private readonly ConexionDB _conexionDB;

    public CardStatementsRepository(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    public async Task<dynamic?> GetStatementAsync(string cardType, int year, int month)
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryFirstOrDefaultAsync("""
            SELECT s.id, s.card_id AS cardId, c.type AS cardType,
                   s.statement_month AS statementMonth, s.statement_year AS statementYear,
                   s.close_date AS closeDate, s.due_date AS dueDate,
                   s.next_close_date AS nextCloseDate, s.next_due_date AS nextDueDate,
                   s.exchange_rate_usd AS exchangeRateUsd, s.pdf_path AS pdfPath
            FROM card_statements s
            JOIN cards c ON c.id = s.card_id
            WHERE c.type = @CardType
              AND s.statement_month = @Month
              AND s.statement_year = @Year
            LIMIT 1
            """, new { CardType = cardType, Month = month, Year = year });
    }

    public async Task<(string PdfPath, string CardType, int Month, int Year)?> GetPdfInfoAsync(int statementId)
    {
        using var con = _conexionDB.Abrir();
        var row = await con.QueryFirstOrDefaultAsync("""
            SELECT s.pdf_path AS PdfPath, c.type AS CardType,
                   s.statement_month AS Month, s.statement_year AS Year
            FROM card_statements s
            JOIN cards c ON c.id = s.card_id
            WHERE s.id = @Id
            """, new { Id = statementId });
        if (row is null) return null;
        return (row.PdfPath, row.CardType, (int)row.Month, (int)row.Year);
    }

    public async Task<IEnumerable<dynamic>> GetExpensesAsync(int statementId)
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryAsync("""
            SELECT e.id, e.statement_id AS statementId, e.cardholder_name AS cardholderName,
                   e.date, e.description, e.installment_number AS installmentNumber,
                   e.installment_total AS installmentTotal, e.amount_ars AS amountArs,
                   e.amount_usd AS amountUsd, e.category_id AS categoryId,
                   c.name AS categoryName, c.color AS categoryColor, c.logo_url AS categoryIcon
            FROM card_expenses e
            LEFT JOIN card_categories c ON c.id = e.category_id
            WHERE e.statement_id = @StatementId
            ORDER BY e.date DESC, e.id DESC
            """, new { StatementId = statementId });
    }

    public async Task<string?> DeleteStatementAsync(int statementId)
    {
        using var con = _conexionDB.Abrir();
        var pdfPath = await con.ExecuteScalarAsync<string?>(
            "SELECT pdf_path FROM card_statements WHERE id = @Id", new { Id = statementId });
        if (pdfPath is null) return null;
        await con.ExecuteAsync(
            "DELETE FROM card_statements WHERE id = @Id", new { Id = statementId });
        return pdfPath;
    }

    public async Task<int> InsertStatementAsync(SaveCardStatementRequest req, string pdfPath)
    {
        using var con = _conexionDB.Abrir();
        using var tx = con.BeginTransaction();

        var statementId = await con.ExecuteScalarAsync<int>("""
            INSERT INTO card_statements
                (card_id, statement_month, statement_year, close_date, due_date,
                 next_close_date, next_due_date, pdf_path, exchange_rate_usd)
            VALUES
                (@CardId, @StatementMonth, @StatementYear, @CloseDate, @DueDate,
                 @NextCloseDate, @NextDueDate, @PdfPath, @ExchangeRateUsd)
            RETURNING id
            """,
            new
            {
                req.CardId,
                req.StatementMonth,
                req.StatementYear,
                req.CloseDate,
                req.DueDate,
                req.NextCloseDate,
                req.NextDueDate,
                PdfPath    = pdfPath,
                req.ExchangeRateUsd
            }, tx);

        foreach (var e in req.Expenses)
        {
            await con.ExecuteAsync("""
                INSERT INTO card_expenses
                    (statement_id, cardholder_name, date, description,
                     installment_number, installment_total, amount_ars, amount_usd)
                VALUES
                    (@StatementId, @CardholderName, @Date, @Description,
                     @InstallmentNumber, @InstallmentTotal, @AmountArs, @AmountUsd)
                """,
                new
                {
                    StatementId       = statementId,
                    e.CardholderName,
                    e.Date,
                    e.Description,
                    e.InstallmentNumber,
                    e.InstallmentTotal,
                    e.AmountArs,
                    e.AmountUsd
                }, tx);
        }

        tx.Commit();
        return statementId;
    }
}
