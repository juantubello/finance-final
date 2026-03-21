using Dapper;
using FinanzasApp.Database;

namespace FinanzasApp.Services;

public class CardCategorizationService
{
    private readonly ConexionDB _db;

    public CardCategorizationService(ConexionDB db)
    {
        _db = db;
    }

    /// <summary>
    /// Applies category rules (ordered by priority DESC) to all card_expenses
    /// where category_id IS NULL. Returns the number of expenses updated.
    /// </summary>
    public async Task<int> ApplyRulesAsync()
    {
        using var con = _db.Abrir();

        // Load rules ordered by priority DESC so higher priority wins
        var rules = (await con.QueryAsync<(int CategoryId, string Keyword)>("""
            SELECT category_id AS CategoryId, keyword AS Keyword
            FROM card_category_rules
            ORDER BY priority DESC, id ASC
            """)).ToList();

        if (rules.Count == 0) return 0;

        // Load uncategorized expenses
        var expenses = (await con.QueryAsync<(int Id, string Description)>("""
            SELECT id AS Id, description AS Description
            FROM card_expenses
            WHERE category_id IS NULL
            """)).ToList();

        if (expenses.Count == 0) return 0;

        int updated = 0;
        using var tx = con.BeginTransaction();

        foreach (var (expenseId, description) in expenses)
        {
            // First matching rule (highest priority first) wins
            int? matchedCategory = null;
            foreach (var (categoryId, keyword) in rules)
            {
                if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    matchedCategory = categoryId;
                    break;
                }
            }

            if (matchedCategory is null) continue;

            await con.ExecuteAsync(
                "UPDATE card_expenses SET category_id = @CategoryId WHERE id = @Id",
                new { CategoryId = matchedCategory, Id = expenseId }, tx);
            updated++;
        }

        tx.Commit();
        return updated;
    }
}
