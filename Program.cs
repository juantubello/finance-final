using FinanzasApp.Database;
using FinanzasApp.Endpoints;
using FinanzasApp.Repositories;
using FinanzasApp.Services;
using WebPush;

var builder = WebApplication.CreateBuilder(args);

var ruta = builder.Configuration["DatabasePath"]
           ?? throw new Exception("No está definida DatabasePath");

// ── VAPID keys: generar si no están configuradas ─────────────────────────────
if (string.IsNullOrEmpty(builder.Configuration["Vapid:PublicKey"]))
{
    var keys = VapidHelper.GenerateVapidKeys();
    Console.WriteLine("=== VAPID KEYS (agregar a appsettings.json) ===");
    Console.WriteLine($"  PublicKey:  {keys.PublicKey}");
    Console.WriteLine($"  PrivateKey: {keys.PrivateKey}");
    Console.WriteLine("================================================");
    throw new Exception("Vapid:PublicKey no configurado. Copiá las keys del log y agregarlas a appsettings.json.");
}

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
builder.Services.AddScoped<CardCategorizationService>();
builder.Services.AddSingleton<BbvaPdfParser>();
builder.Services.AddScoped<PushSubscriptionsRepository>();
builder.Services.AddScoped<WebPushService>();
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
app.MapPushEndpoints();

app.Run();