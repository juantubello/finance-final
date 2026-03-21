using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;

namespace FinanzasApp.Repositories;

public class CardAdminRepository
{
    private readonly ConexionDB _conexionDB;

    public CardAdminRepository(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    // ── Cards ────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<CardDto>> GetCardsAsync()
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryAsync<CardDto>("""
            SELECT id, name, bank, type, created_at AS CreatedAt FROM cards ORDER BY id
            """);
    }

    public async Task<int> InsertCardAsync(CardUpsert c)
    {
        using var con = _conexionDB.Abrir();
        return await con.ExecuteScalarAsync<int>("""
            INSERT INTO cards (name, bank, type) VALUES (@Name, @Bank, @Type) RETURNING id
            """, c);
    }

    public async Task UpdateCardAsync(int id, CardUpsert c)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("""
            UPDATE cards SET name = @Name, bank = @Bank, type = @Type,
                updated_at = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')
            WHERE id = @Id
            """, new { c.Name, c.Bank, c.Type, Id = id });
    }

    public async Task DeleteCardAsync(int id)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("DELETE FROM cards WHERE id = @Id", new { Id = id });
    }

    // ── Card Categories ──────────────────────────────────────────────────────

    public async Task<IEnumerable<CardCategoryDto>> GetCategoriesAsync()
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryAsync<CardCategoryDto>("""
            SELECT id, name, description, color, logo_url AS LogoUrl
            FROM card_categories ORDER BY name
            """);
    }

    public async Task<int> InsertCategoryAsync(CardCategoryUpsert c)
    {
        using var con = _conexionDB.Abrir();
        return await con.ExecuteScalarAsync<int>("""
            INSERT INTO card_categories (name, description, color, logo_url)
            VALUES (@Name, @Description, @Color, @LogoUrl) RETURNING id
            """, c);
    }

    public async Task UpdateCategoryAsync(int id, CardCategoryUpsert c)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("""
            UPDATE card_categories SET name = @Name, description = @Description,
                color = @Color, logo_url = @LogoUrl,
                updated_at = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')
            WHERE id = @Id
            """, new { c.Name, c.Description, c.Color, c.LogoUrl, Id = id });
    }

    public async Task DeleteCategoryAsync(int id)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("DELETE FROM card_categories WHERE id = @Id", new { Id = id });
    }

    // ── Card Category Rules ──────────────────────────────────────────────────

    public async Task<IEnumerable<CardCategoryRuleDto>> GetCategoryRulesAsync()
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryAsync<CardCategoryRuleDto>("""
            SELECT r.id, r.keyword, r.category_id AS CategoryId, c.name AS CategoryName, r.priority
            FROM card_category_rules r
            JOIN card_categories c ON c.id = r.category_id
            ORDER BY r.priority DESC, r.keyword
            """);
    }

    public async Task<int> InsertCategoryRuleAsync(CardCategoryRuleUpsert r)
    {
        using var con = _conexionDB.Abrir();
        return await con.ExecuteScalarAsync<int>("""
            INSERT INTO card_category_rules (keyword, category_id, priority)
            VALUES (@Keyword, @CategoryId, @Priority) RETURNING id
            """, r);
    }

    public async Task UpdateCategoryRuleAsync(int id, CardCategoryRuleUpsert r)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("""
            UPDATE card_category_rules SET keyword = @Keyword, category_id = @CategoryId,
                priority = @Priority, updated_at = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')
            WHERE id = @Id
            """, new { r.Keyword, r.CategoryId, r.Priority, Id = id });
    }

    public async Task DeleteCategoryRuleAsync(int id)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("DELETE FROM card_category_rules WHERE id = @Id", new { Id = id });
    }

    // ── Logo Rules ───────────────────────────────────────────────────────────

    public async Task<IEnumerable<LogoRuleDto>> GetLogoRulesAsync()
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryAsync<LogoRuleDto>("""
            SELECT id, keyword, logo_url AS LogoUrl, priority
            FROM logo_rules ORDER BY priority DESC, keyword
            """);
    }

    public async Task<int> InsertLogoRuleAsync(LogoRuleUpsert r)
    {
        using var con = _conexionDB.Abrir();
        return await con.ExecuteScalarAsync<int>("""
            INSERT INTO logo_rules (keyword, logo_url, priority)
            VALUES (@Keyword, @LogoUrl, @Priority) RETURNING id
            """, r);
    }

    public async Task UpdateLogoRuleAsync(int id, LogoRuleUpsert r)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("""
            UPDATE logo_rules SET keyword = @Keyword, logo_url = @LogoUrl,
                priority = @Priority, updated_at = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')
            WHERE id = @Id
            """, new { r.Keyword, r.LogoUrl, r.Priority, Id = id });
    }

    public async Task DeleteLogoRuleAsync(int id)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("DELETE FROM logo_rules WHERE id = @Id", new { Id = id });
    }
}
