using Dapper;
using FinanzasApp.Database;
using FinanzasApp.Models;

namespace FinanzasApp.Repositories;

public class CategoriasIngresoRepository
{
    private readonly ConexionDB _conexionDB;

    public CategoriasIngresoRepository(ConexionDB conexionDB)
    {
        _conexionDB = conexionDB;
    }

    public async Task<IEnumerable<CategoriaIngreso>> ObtenerCategorias()
    {
        using var con = _conexionDB.Abrir();
        return await con.QueryAsync<CategoriaIngreso>("""
            SELECT id          AS CategoryId,
                   nombre      AS CategoryName,
                   descripcion AS CategoryDescription,
                   icono       AS CategoryIcon,
                   color       AS CategoryColor
            FROM categorias_ingreso
            WHERE activa = 1
            """);
    }

    public async Task AgregarCategoria(CategoriaIngreso categoria)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("""
            INSERT INTO categorias_ingreso (nombre, descripcion, icono, color, activa, created_at, updated_at)
            VALUES (@Nombre, @Descripcion, @Icono, @Color, 1, @Now, @Now)
            """, new
        {
            Nombre      = categoria.CategoryName,
            Descripcion = categoria.CategoryDescription,
            Icono       = categoria.CategoryIcon,
            Color       = categoria.CategoryColor,
            Now         = DateTime.UtcNow
        });
    }

    public async Task ActualizarCategoria(CategoriaIngreso categoria)
    {
        using var con = _conexionDB.Abrir();
        await con.ExecuteAsync("""
            UPDATE categorias_ingreso
            SET nombre = @Nombre, descripcion = @Descripcion, icono = @Icono, color = @Color, updated_at = @Now
            WHERE id = @Id
            """, new
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
        await con.ExecuteAsync("DELETE FROM categorias_ingreso WHERE id = @Id", new { Id = id });
    }
}
