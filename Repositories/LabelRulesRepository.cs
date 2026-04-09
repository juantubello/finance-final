using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;

namespace FinanzasApp.Repositories;

public class LabelRulesRepository
{
    private readonly ConexionDB _conexionDB;

    public LabelRulesRepository(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    public async Task<IEnumerable<LabelRule>> ObtenerLabelRules()
    {
        using var con = _conexionDB.Abrir();
        var rows = await con.QueryAsync<LabelRuleRow>("""
            SELECT lr.id AS RuleId, lr.keyword AS Keyword, l.id AS LabelId, l.name AS LabelName
            FROM label_rules lr
            LEFT JOIN label_rule_labels lrl ON lrl.rule_id = lr.id
            LEFT JOIN labels l ON l.id = lrl.label_id
            ORDER BY lr.keyword, l.name
            """);

        return MapRows(rows);
    }

    public async Task<LabelRule> AgregarLabelRule(LabelRuleRequest request)
    {
        var keyword = ValidateKeyword(request.Keyword);
        var labelIds = NormalizeLabelIds(request.LabelIds);

        using var con = _conexionDB.Abrir();
        using var tx = con.BeginTransaction();

        await EnsureLabelsExistAsync(con, tx, labelIds);

        var id = await con.ExecuteScalarAsync<long>("""
            INSERT INTO label_rules (keyword, created_at, updated_at)
            VALUES (@Keyword, @Now, @Now);
            SELECT last_insert_rowid();
            """, new { Keyword = keyword, Now = DateTime.UtcNow }, tx);

        foreach (var labelId in labelIds)
        {
            await con.ExecuteAsync(
                "INSERT INTO label_rule_labels (rule_id, label_id) VALUES (@RuleId, @LabelId)",
                new { RuleId = (int)id, LabelId = labelId },
                tx);
        }

        tx.Commit();

        return await ObtenerLabelRulePorId((int)id)
               ?? throw new InvalidOperationException("No se pudo recuperar la regla creada.");
    }

    public async Task<LabelRule?> ActualizarLabelRule(int id, LabelRuleRequest request)
    {
        var keyword = ValidateKeyword(request.Keyword);
        var labelIds = NormalizeLabelIds(request.LabelIds);

        using var con = _conexionDB.Abrir();
        using var tx = con.BeginTransaction();

        await EnsureLabelsExistAsync(con, tx, labelIds);

        var updated = await con.ExecuteAsync(
            "UPDATE label_rules SET keyword = @Keyword, updated_at = @UpdatedAt WHERE id = @Id",
            new { Id = id, Keyword = keyword, UpdatedAt = DateTime.UtcNow },
            tx);

        if (updated == 0)
        {
            tx.Rollback();
            return null;
        }

        await con.ExecuteAsync("DELETE FROM label_rule_labels WHERE rule_id = @Id", new { Id = id }, tx);

        foreach (var labelId in labelIds)
        {
            await con.ExecuteAsync(
                "INSERT INTO label_rule_labels (rule_id, label_id) VALUES (@RuleId, @LabelId)",
                new { RuleId = id, LabelId = labelId },
                tx);
        }

        tx.Commit();
        return await ObtenerLabelRulePorId(id);
    }

    public async Task<bool> EliminarLabelRule(int id)
    {
        using var con = _conexionDB.Abrir();
        var deleted = await con.ExecuteAsync("DELETE FROM label_rules WHERE id = @Id", new { Id = id });
        return deleted > 0;
    }

    private async Task<LabelRule?> ObtenerLabelRulePorId(int id)
    {
        using var con = _conexionDB.Abrir();
        var rows = await con.QueryAsync<LabelRuleRow>("""
            SELECT lr.id AS RuleId, lr.keyword AS Keyword, l.id AS LabelId, l.name AS LabelName
            FROM label_rules lr
            LEFT JOIN label_rule_labels lrl ON lrl.rule_id = lr.id
            LEFT JOIN labels l ON l.id = lrl.label_id
            WHERE lr.id = @Id
            ORDER BY l.name
            """, new { Id = id });

        return MapRows(rows).SingleOrDefault();
    }

    private static List<LabelRule> MapRows(IEnumerable<LabelRuleRow> rows)
    {
        var rules = new Dictionary<int, LabelRule>();

        foreach (var row in rows)
        {
            if (!rules.TryGetValue(row.RuleId, out var rule))
            {
                rule = new LabelRule
                {
                    Id = row.RuleId,
                    Keyword = row.Keyword
                };
                rules.Add(rule.Id, rule);
            }

            if (row.LabelId.HasValue && !string.IsNullOrWhiteSpace(row.LabelName))
            {
                rule.Labels.Add(new Label
                {
                    Id = row.LabelId.Value,
                    Name = row.LabelName
                });
            }
        }

        return rules.Values.ToList();
    }

    private static string ValidateKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            throw new ArgumentException("El keyword es obligatorio.");

        return keyword.Trim();
    }

    private static List<int> NormalizeLabelIds(IEnumerable<int>? labelIds)
    {
        var normalized = labelIds?
            .Where(id => id > 0)
            .Distinct()
            .ToList() ?? new List<int>();

        if (normalized.Count == 0)
            throw new ArgumentException("Debe informar al menos un labelId.");

        return normalized;
    }

    private static async Task EnsureLabelsExistAsync(
        Microsoft.Data.Sqlite.SqliteConnection con,
        Microsoft.Data.Sqlite.SqliteTransaction tx,
        IReadOnlyCollection<int> labelIds)
    {
        var count = await con.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM labels WHERE id IN @Ids",
            new { Ids = labelIds },
            tx);

        if (count != labelIds.Count)
            throw new ArgumentException("Uno o más labels indicados no existen.");
    }

    private sealed class LabelRuleRow
    {
        public int RuleId { get; init; }
        public string Keyword { get; init; } = string.Empty;
        public int? LabelId { get; init; }
        public string? LabelName { get; init; }
    }
}
