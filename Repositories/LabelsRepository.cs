using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;
using FinanzasApp.Services;

namespace FinanzasApp.Repositories;

public class LabelsRepository
{
    private readonly ConexionDB _conexionDB;

    public LabelsRepository(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    public async Task<IEnumerable<Label>> ObtenerLabels()
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryAsync<Label>("SELECT id AS Id, name AS Name FROM labels ORDER BY name");
    }

    public async Task<Label> CrearLabel(LabelUpsertRequest request)
    {
        var normalizedName = NormalizeLabelNameOrThrow(request.Name);

        using var con = _conexionDB.Abrir();
        var id = await con.ExecuteScalarAsync<long>(
            """
            INSERT INTO labels (name) VALUES (@Name);
            SELECT last_insert_rowid();
            """,
            new { Name = normalizedName });

        return new Label
        {
            Id = (int)id,
            Name = normalizedName
        };
    }

    public async Task<Label?> ActualizarLabel(int id, LabelUpsertRequest request)
    {
        var normalizedName = NormalizeLabelNameOrThrow(request.Name);

        using var con = _conexionDB.Abrir();
        var updated = await con.ExecuteAsync(
            "UPDATE labels SET name = @Name WHERE id = @Id",
            new { Id = id, Name = normalizedName });

        if (updated == 0)
            return null;

        return new Label
        {
            Id = id,
            Name = normalizedName
        };
    }

    public async Task<bool> EliminarLabel(int id)
    {
        using var con = _conexionDB.Abrir();
        var deleted = await con.ExecuteAsync("DELETE FROM labels WHERE id = @Id", new { Id = id });
        return deleted > 0;
    }

    public async Task<IEnumerable<Label>> ObtenerLabelsPorGasto(int gastoId)
    {
        using var con = _conexionDB.Abrir();
        var sql = """
            SELECT l.id AS Id, l.name AS Name
            FROM labels l
            INNER JOIN gasto_labels gl ON gl.label_id = l.id
            WHERE gl.gasto_id = @GastoId
            """;
        return await con.QueryAsync<Label>(sql, new { GastoId = gastoId });
    }

    public async Task<IEnumerable<Label>> ObtenerLabelsPorGastos(IEnumerable<int> gastoIds)
    {
        using var con = _conexionDB.Abrir();
        var sql = """
            SELECT gl.gasto_id AS GastoId, l.id AS Id, l.name AS Name
            FROM labels l
            INNER JOIN gasto_labels gl ON gl.label_id = l.id
            WHERE gl.gasto_id IN @GastoIds
            """;
        return await con.QueryAsync<Label>(sql, new { GastoIds = gastoIds });
    }

    public async Task<AgregarLabelsResponse> AgregarLabelsAGasto(int gastoId, List<string> nombres)
    {
        using var con = _conexionDB.Abrir();

        var labels = new List<Label>();
        foreach (var nombre in nombres
            .Select(TextNormalizationService.NormalizeLabelName)
            .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
            .Distinct())
        {
            await con.ExecuteAsync(
                "INSERT OR IGNORE INTO labels (name) VALUES (@Name)",
                new { Name = nombre });

            var label = await con.QueryFirstAsync<Label>(
                "SELECT id AS Id, name AS Name FROM labels WHERE name = @Name COLLATE NOCASE",
                new { Name = nombre });

            await con.ExecuteAsync(
                "INSERT OR IGNORE INTO gasto_labels (gasto_id, label_id) VALUES (@GastoId, @LabelId)",
                new { GastoId = gastoId, LabelId = label.Id });

            labels.Add(label);
        }

        return new AgregarLabelsResponse { GastoId = gastoId, Labels = labels };
    }

    public async Task<bool> ExistenLabels(IEnumerable<int> labelIds)
    {
        var ids = labelIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return true;

        using var con = _conexionDB.Abrir();
        var count = await con.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM labels WHERE id IN @Ids",
            new { Ids = ids });

        return count == ids.Count;
    }

    public async Task<List<Label>> AsociarLabelsExistentesAGasto(int gastoId, IEnumerable<int> labelIds)
    {
        var ids = labelIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return new List<Label>();

        using var con = _conexionDB.Abrir();
        var labels = (await con.QueryAsync<Label>(
            "SELECT id AS Id, name AS Name FROM labels WHERE id IN @Ids ORDER BY name",
            new { Ids = ids })).ToList();

        foreach (var label in labels)
        {
            await con.ExecuteAsync(
                "INSERT OR IGNORE INTO gasto_labels (gasto_id, label_id) VALUES (@GastoId, @LabelId)",
                new { GastoId = gastoId, LabelId = label.Id });
        }

        return labels;
    }

    public async Task EliminarLabelDeGasto(int gastoId, int labelId)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync(
            "DELETE FROM gasto_labels WHERE gasto_id = @GastoId AND label_id = @LabelId",
            new { GastoId = gastoId, LabelId = labelId });
    }

    private static string NormalizeLabelNameOrThrow(string? name)
    {
        var normalizedName = TextNormalizationService.NormalizeLabelName(name);
        if (string.IsNullOrWhiteSpace(normalizedName))
            throw new ArgumentException("El nombre es obligatorio.");

        return normalizedName;
    }
}
