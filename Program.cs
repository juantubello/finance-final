using FinanzasApp.Database;
using FinanzasApp.Repositories;
using FinanzasApp.Models;

var builder = WebApplication.CreateBuilder(args);

var ruta = builder.Configuration["DatabasePath"]
           ?? throw new Exception("No está definida DatabasePath");

builder.Services.AddSingleton(new ConexionDB(ruta));
builder.Services.AddScoped<GastosRepository>();

var app = builder.Build();

// GET todos los gastos
app.MapGet("/gastos", async (GastosRepository repo) =>
    Results.Ok(await repo.ObtenerGastos()));

// GET un gasto por id
app.MapGet("/gastos/{id}", async (int id, GastosRepository repo) =>
{
    var gasto = await repo.ObtenerGastoPorId(id);
    return gasto is null ? Results.NotFound() : Results.Ok(gasto);
});

// POST nuevo gasto
app.MapPost("/gastos", async (Gasto gasto, GastosRepository repo) =>
{
    await repo.AgregarGasto(gasto);
    return Results.Created($"/gastos/{gasto.Id}", gasto);
});

// PUT editar gasto
app.MapPut("/gastos/{id}", async (int id, Gasto gasto, GastosRepository repo) =>
{
    gasto.Id = id;
    await repo.ActualizarGasto(gasto);
    return Results.Ok(gasto);
});

// DELETE eliminar gasto
app.MapDelete("/gastos/{id}", async (int id, GastosRepository repo) =>
{
    await repo.EliminarGasto(id);
    return Results.NoContent();
});

app.Run();