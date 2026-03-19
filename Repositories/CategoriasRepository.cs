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
             SELECT id          as CategoryId,
                    nombre      as CategoryName,
                    descripcion as CategoryDescription,
                    icono       as CategoryIcon,
                    color       as CategoryColor
               FROM categorias
               WHERE activa = 1
    """;
        var categorias = await con.QueryAsync<Categorias>(sql);
        return categorias;
    }

    public async Task AgregarCategoria(Categorias categoria)
    {
        using var con = _conexionDB.Abrir();
        var sql = @"INSERT INTO categorias (nombre, descripcion, icono, color, activa, created_at, updated_at)
                    VALUES (@Nombre, @Descripcion, @Icono, @Color, 1, @Now, @Now)";
        await con.ExecuteAsync(sql, new
        {
            Nombre      = categoria.CategoryName,
            Descripcion = categoria.CategoryDescription,
            Icono       = categoria.CategoryIcon,
            Color       = categoria.CategoryColor,
            Now         = DateTime.UtcNow
        });
    }

    public async Task ActualizarCategoria(Categorias categoria)
    {
        using var con = _conexionDB.Abrir();
        var sql = @"UPDATE categorias SET nombre = @Nombre, descripcion = @Descripcion, icono = @Icono,
                    color = @Color, updated_at = @Now WHERE id = @Id";
        await con.ExecuteAsync(sql, new
        {
            Id          = categoria.CategoryId,
            Nombre      = categoria.CategoryName,
            Descripcion = categoria.CategoryDescription,
            Icono       = categoria.CategoryIcon,
            Color       = categoria.CategoryColor,
            Now         = DateTime.UtcNow
        });
    }

    public async Task EliminarCategoria(int id)
    {
        using var con = _conexionDB.Abrir();
        var sql = "DELETE FROM categorias WHERE id = @Id";
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