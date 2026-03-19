using FinanzasApp.Database;
using FinanzasApp.Endpoints;
using FinanzasApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

var ruta = builder.Configuration["DatabasePath"]
           ?? throw new Exception("No está definida DatabasePath");

builder.Services.AddSingleton(new ConexionDB(ruta));
builder.Services.AddScoped<GastosRepository>();
builder.Services.AddScoped<CategoriasRepository>();
builder.Services.AddScoped<MonedaRepository>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors();

app.MapGastosEndpoints();
app.MapCategoriasEndpoints();
app.MapMonedasEndpoints();

app.Run();