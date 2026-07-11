using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AudioService.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.WebHost.ConfigureKestrel(options =>
{
    var kestrelSection = builder.Configuration.GetSection("Kestrel");
    options.Configure(kestrelSection);
});

// In-memory логгер
builder.Services.AddSingleton<IInMemoryLogStore, InMemoryLogStore>();
builder.Services.AddSingleton<ILoggerProvider, InMemoryLoggerProvider>();

builder.Logging.AddConsole();


builder.Services.AddControllers();
builder.Services.AddSingleton<WavConverter>();
builder.Services.AddSingleton<ProcessService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var processService = scope.ServiceProvider.GetRequiredService<ProcessService>();
    processService.Initialize();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapControllers();
// Эндпоинт логов
app.MapGet("/", (IInMemoryLogStore store) =>
{
    var logs = store.GetLogs();
    var html = "<!DOCTYPE html>\n" +
               "<html lang=\"ru\">\n" +
               "<head>\n" +
               "    <meta charset=\"UTF-8\">\n" +
               "    <meta http-equiv=\"refresh\" content=\"2\">\n" +
               "    <title>Логи</title>\n" +
               "    <style>\n" +
               "        body {\n" +
               "            font-family: monospace;\n" +
               "            background: #1e1e1e;\n" +
               "            color: #dcdcdc;\n" +
               "            padding: 10px;\n" +
               "        }\n" +
               "        pre {\n" +
               "            white-space: pre-wrap;\n" +
               "            word-break: break-word;\n" +
               "        }\n" +
               "    </style>\n" +
               "</head>\n" +
               "<body>\n" +
               "    <h2>Логи AudioService</h2>\n" +
               "    <pre id=\"log-area\">" + string.Join("\n", logs) + "</pre>\n" +
               "    <script>\n" +
               "        window.scrollTo(0, document.body.scrollHeight);\n" +
               "    </script>\n" +
               "</body>\n" +
               "</html>";

    return Results.Content(html, "text/html; charset=utf-8");
});


app.Run();
