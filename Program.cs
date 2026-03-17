using FinanzasApp.Database;
using FinanzasApp.Endpoints;
using FinanzasApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

var ruta = builder.Configuration["DatabasePath"]
           ?? throw new Exception("No está definida DatabasePath");

builder.Services.AddSingleton(new ConexionDB(ruta));
builder.Services.AddScoped<GastosRepository>();

var app = builder.Build();

app.MapGastosEndpoints();

app.Run();