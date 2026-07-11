using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.WebSockets;
using System.Text.Json;
using CommonBackend;
using CommonBackend.Domain;
using CommonBackend.Domain.Entities;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using CommonBackend.Infrastructure.Persistence;
using CommonBackend.Infrastructure.WebSockets;
using CommonBackend.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using CommonBackend.Api.Controllers;
using CommonBackend.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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

// Сервисы
// DI + Services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    // Добавляем описание схемы безопасности для JWT Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Введите 'Bearer' и пробел, затем токен.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Требуем аутентификацию для всех операций (по необходимости)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme, 
                    Id = "Bearer" 
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddHttpClient("AudioService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AudioService:BaseUrl"]!);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
});



//аутентификация 
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JwtSettings:Secret is not configured."),
        ValidAudience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JwtSettings:Secret is not configured."),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// БД миграции
await DbInitializer.ApplyMigrationsAsync(app.Services, app.Logger);

// Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();

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
               "    <h2>Логи CommonBackend</h2>\n" +
               "    <pre id=\"log-area\">" + string.Join("\n", logs) + "</pre>\n" +
               "    <script>\n" +
               "        window.scrollTo(0, document.body.scrollHeight);\n" +
               "    </script>\n" +
               "</body>\n" +
               "</html>";

    return Results.Content(html, "text/html; charset=utf-8");
});


app.Run();