using FinanzasApp.Database;
using FinanzasApp.Endpoints;
using FinanzasApp.Repositories;
using FinanzasApp.Services;

var builder = WebApplication.CreateBuilder(args);

var ruta = builder.Configuration["DatabasePath"]
           ?? throw new Exception("No está definida DatabasePath");

builder.Services.AddSingleton(new ConexionDB(ruta));
builder.Services.AddScoped<GastosRepository>();
builder.Services.AddScoped<CategoriasRepository>();
builder.Services.AddScoped<MonedaRepository>();
builder.Services.AddScoped<LabelsRepository>();
builder.Services.AddScoped<CategoryRulesRepository>();
builder.Services.AddScoped<IngresosRepository>();
builder.Services.AddScoped<AhorrosRepository>();
builder.Services.AddScoped<CedearRepository>();

builder.Services.AddScoped<CardStatementsRepository>();
builder.Services.AddScoped<CardAdminRepository>();
builder.Services.AddSingleton<BbvaPdfParser>();
builder.Services.AddHttpClient();

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
app.MapLabelsEndpoints();
app.MapCategoryRulesEndpoints();
app.MapIngresosEndpoints();
app.MapAhorrosEndpoints();
app.MapAvailableEndpoints();
app.MapCedearEndpoints();
app.MapCardStatementsEndpoints();
app.MapCardAdminEndpoints();

app.Run();