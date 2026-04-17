using qaautomationsuite.Services;
using Microsoft.Playwright;

// ─── Hook para instalar navegadores Playwright desde CLI ───
if (args.Length >= 2 && args[0] == "playwright" && args[1] == "install")
{
    var browser = args.Length > 2 ? args[2] : "chromium";
    Console.WriteLine($"Instalando navegador Playwright: {browser}...");
    var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", browser });
    Environment.Exit(exitCode);
    return;
}

var builder = WebApplication.CreateBuilder(args);

// ─── Servicios ───
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddSingleton<JobManager>();
builder.Services.AddTransient<PlaywrightService>();
builder.Services.AddTransient<ExcelService>();

// ─── Puerto configurable ───
var puerto = builder.Configuration.GetValue<int>("AppSettings:Puerto", 5100);
builder.WebHost.UseUrls($"http://localhost:{puerto}");

var app = builder.Build();

// ─── Monolito SPA: C# sirve Angular desde wwwroot ───
// UseDefaultFiles() → busca index.html por defecto en wwwroot
// UseStaticFiles()  → sirve JS, CSS, assets compilados
// MapFallbackToFile → cualquier ruta no-API redirige a index.html (SPA routing)
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("index.html");

// ─── Abrir navegador al iniciar ───
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        var url = $"http://localhost:{puerto}";
        Console.WriteLine($"\n✅ qaautomationsuite iniciado en: {url}");
        Console.WriteLine("   Abriendo navegador...\n");
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
    catch { /* Si no puede abrir el browser, no es crítico */ }
});

app.Run();
