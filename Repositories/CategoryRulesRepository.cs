using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;

namespace FinanzasApp.Repositories;

public class CategoryRulesRepository
{
    private readonly ConexionDB _conexionDB;

    public CategoryRulesRepository(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    public async Task<IEnumerable<CategoryRule>> ObtenerCategoryRules()
    {
        using var con = _conexionDB.Abrir();
        var sql = """
            SELECT cr.id AS Id, cr.keyword AS Keyword, cr.category_id AS CategoryId, c.nombre AS CategoryName
            FROM category_rules cr
            INNER JOIN categorias c ON c.id = cr.category_id
            ORDER BY cr.keyword
            """;
        return await con.QueryAsync<CategoryRule>(sql);
    }

    public async Task<CategoryRule> AgregarCategoryRule(CategoryRuleRequest request)
    {
        using var con = _conexionDB.Abrir();
        var sql = """
            INSERT INTO category_rules (keyword, category_id)
            VALUES (@Keyword, @CategoryId)
            """;
        await con.ExecuteAsync(sql, new { Keyword = request.Keyword.Trim().ToLower(), request.CategoryId });

        return await con.QueryFirstAsync<CategoryRule>("""
            SELECT cr.id AS Id, cr.keyword AS Keyword, cr.category_id AS CategoryId, c.nombre AS CategoryName
            FROM category_rules cr
            INNER JOIN categorias c ON c.id = cr.category_id
            WHERE cr.keyword = @Keyword
            """, new { Keyword = request.Keyword.Trim().ToLower() });
    }

    public async Task<CategoryRule> ActualizarCategoryRule(int id, CategoryRuleRequest request)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync(
            "UPDATE category_rules SET keyword = @Keyword, category_id = @CategoryId WHERE id = @Id",
            new { Id = id, Keyword = request.Keyword.Trim().ToLower(), request.CategoryId });

        return await con.QueryFirstAsync<CategoryRule>("""
            SELECT cr.id AS Id, cr.keyword AS Keyword, cr.category_id AS CategoryId, c.nombre AS CategoryName
            FROM category_rules cr
            INNER JOIN categorias c ON c.id = cr.category_id
            WHERE cr.id = @Id
            """, new { Id = id });
    }

    public async Task EliminarCategoryRule(int id)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("DELETE FROM category_rules WHERE id = @Id", new { Id = id });
    }
}
