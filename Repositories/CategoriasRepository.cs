namespace FinanzasApp.Repositories;
using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;

public class CategoriasRepository
{
    private readonly ConexionDB _conexionDB;

    public CategoriasRepository(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    public async Task<IEnumerable<Categorias>> ObtenerCategorias()
    {
        using var con = _conexionDB.Abrir();
         var sql = """
             SELECT id as CategoryId, 
                    nombre as CategoryName,
                    descripcion as CategoryDescription,
                    icono as CategoryIcon
               FROM categorias
               WHERE activa = 1
    """;
        var categorias = await con.QueryAsync<Categorias>(sql);
        return categorias;
    }

    public async Task AgregarCategoria(Categorias categoria)
    {
        using var con = _conexionDB.Abrir();
        var sql = @"INSERT INTO categorias (Nombre, Descripcion, Icono, Activa, CreatedAt, UpdatedAt) 
                    VALUES (@Nombre, @Descripcion, @Icono, @Activa, @CreatedAt, @UpdatedAt)";
        await con.ExecuteAsync(sql, categoria);
    }

    public async Task ActualizarCategoria(Categorias categoria)
    {
        using var con = _conexionDB.Abrir();
        var sql = @"UPDATE categorias SET Nombre = @Nombre, Descripcion = @Descripcion, Icono = @Icono, 
                    Activa = @Activa, UpdatedAt = @UpdatedAt WHERE Id = @Id";
        await con.ExecuteAsync(sql, categoria);
    }

    public async Task EliminarCategoria(int id)
    {
        using var con = _conexionDB.Abrir();
        var sql = "DELETE FROM categorias WHERE Id = @Id";
        await con.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<Categorias?> ObtenerCategoriaPorId(int id)
    {
        using var con = _conexionDB.Abrir();
        var sql = "SELECT * FROM categorias WHERE Id = @Id";
        var categoria = await con.QueryFirstOrDefaultAsync<Categorias>(sql, new { Id = id });
        return categoria;
    }
}