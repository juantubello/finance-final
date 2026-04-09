using Dapper;
using FinanzasApp.Database;

namespace FinanzasApp.Services;

public class LabelRulesService
{
    private readonly ConexionDB _conexionDB;

    public LabelRulesService(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    public async Task ApplyToGastoAsync(int gastoId, string? description)
    {
        var normalizedDescription = TextNormalizationService.NormalizeForSearch(description);
        if (string.IsNullOrEmpty(normalizedDescription))
            return;

        using var con = _conexionDB.Abrir();
        var rules = (await con.QueryAsync<LabelRuleMatchRow>("""
            SELECT lr.id AS RuleId, lr.keyword AS Keyword, lrl.label_id AS LabelId
            FROM label_rules lr
            INNER JOIN label_rule_labels lrl ON lrl.rule_id = lr.id
            ORDER BY lr.id, lrl.label_id
            """)).ToList();

        if (rules.Count == 0)
            return;

        var labelIds = GetMatchedLabelIds(normalizedDescription, rules);
        if (labelIds.Count == 0)
            return;

        foreach (var labelId in labelIds)
        {
            await con.ExecuteAsync(
                "INSERT OR IGNORE INTO gasto_labels (gasto_id, label_id) VALUES (@GastoId, @LabelId)",
                new { GastoId = gastoId, LabelId = labelId });
        }
    }

    public async Task<int> ApplyToExistingGastosAsync()
    {
        using var con = _conexionDB.Abrir();
        var rules = (await con.QueryAsync<LabelRuleMatchRow>("""
            SELECT lr.id AS RuleId, lr.keyword AS Keyword, lrl.label_id AS LabelId
            FROM label_rules lr
            INNER JOIN label_rule_labels lrl ON lrl.rule_id = lr.id
            ORDER BY lr.id, lrl.label_id
            """)).ToList();

        if (rules.Count == 0)
            return 0;

        var gastos = (await con.QueryAsync<GastoRow>("""
            SELECT id AS Id, descripcion AS Description
            FROM gastos
            """)).ToList();

        if (gastos.Count == 0)
            return 0;

        var updated = 0;
        using var tx = con.BeginTransaction();

        foreach (var gasto in gastos)
        {
            var normalizedDescription = TextNormalizationService.NormalizeForSearch(gasto.Description);
            if (string.IsNullOrEmpty(normalizedDescription))
                continue;

            var labelIds = GetMatchedLabelIds(normalizedDescription, rules);
            if (labelIds.Count == 0)
                continue;

            var insertedAny = false;
            foreach (var labelId in labelIds)
            {
                var inserted = await con.ExecuteAsync(
                    "INSERT OR IGNORE INTO gasto_labels (gasto_id, label_id) VALUES (@GastoId, @LabelId)",
                    new { GastoId = gasto.Id, LabelId = labelId },
                    tx);

                insertedAny |= inserted > 0;
            }

            if (insertedAny)
                updated++;
        }

        tx.Commit();
        return updated;
    }

    private static HashSet<int> GetMatchedLabelIds(
        string normalizedDescription,
        IEnumerable<LabelRuleMatchRow> rules)
    {
        var labelIds = new HashSet<int>();

        foreach (var group in rules.GroupBy(rule => rule.RuleId))
        {
            var normalizedKeyword = TextNormalizationService.NormalizeForSearch(group.First().Keyword);
            if (string.IsNullOrEmpty(normalizedKeyword))
                continue;

            if (!normalizedDescription.Contains(normalizedKeyword, StringComparison.Ordinal))
                continue;

            foreach (var rule in group)
                labelIds.Add(rule.LabelId);
        }

        return labelIds;
    }

    private sealed class LabelRuleMatchRow
    {
        public int RuleId { get; init; }
        public string Keyword { get; init; } = string.Empty;
        public int LabelId { get; init; }
    }

    private sealed class GastoRow
    {
        public int Id { get; init; }
        public string Description { get; init; } = string.Empty;
    }
}
