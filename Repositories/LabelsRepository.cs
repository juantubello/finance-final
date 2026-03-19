using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;

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
        foreach (var nombre in nombres)
        {
            await con.ExecuteAsync(
                "INSERT OR IGNORE INTO labels (name) VALUES (@Name)",
                new { Name = nombre.Trim().ToLower() });

            var label = await con.QueryFirstAsync<Label>(
                "SELECT id AS Id, name AS Name FROM labels WHERE name = @Name",
                new { Name = nombre.Trim().ToLower() });

            await con.ExecuteAsync(
                "INSERT OR IGNORE INTO gasto_labels (gasto_id, label_id) VALUES (@GastoId, @LabelId)",
                new { GastoId = gastoId, LabelId = label.Id });

            labels.Add(label);
        }

        return new AgregarLabelsResponse { GastoId = gastoId, Labels = labels };
    }

    public async Task EliminarLabelDeGasto(int gastoId, int labelId)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync(
            "DELETE FROM gasto_labels WHERE gasto_id = @GastoId AND label_id = @LabelId",
            new { GastoId = gastoId, LabelId = labelId });
    }
}
