using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using CommonBackend.Domain.Entities;
using CommonBackend.Domain.Enums;
using CommonBackend.Application.Interfaces;
using CommonBackend.Application.Dtos;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace CommonBackend.Api.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class MessagesController : ControllerBase
{
    private readonly IAppDbContext _db;
    private readonly IConnection _rabbitConn;
    private readonly IWebSocketManager _manager;
    private readonly ILogger<MessagesController> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly string _outgoingQueue;
    private readonly string _audioServiceUrl;

    public MessagesController(
        IAppDbContext db,
        IConnection rabbitConn,
        IWebSocketManager manager,
        ILogger<MessagesController> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _db = db;
        _rabbitConn = rabbitConn;
        _manager = manager;
        _logger = logger;
        _config = config;
        _outgoingQueue = _config["RabbitMQ:OutgoingQueue"] ?? "outgoing_messages";
        _audioServiceUrl = _config["AudioService:BaseUrl"] ?? "http://localhost:8000";

        // Настройка HttpClient для AudioService
        _httpClient = httpClientFactory.CreateClient("AudioService");
    }

    [HttpGet("sent-messages")]
    public async Task<IActionResult> GetSentMessages()
    {
        var messages = await _db.SentMessages
            .OrderByDescending(m => m.Date)
            .ThenByDescending(m => m.Time)
            .ToListAsync();
        return Ok(messages);
    }

    [HttpGet("incoming-messages")]
    public async Task<IActionResult> GetIncomingMessages()
    {
        var messages = await _db.IncomingMessages
            .OrderByDescending(m => m.Date)
            .ThenByDescending(m => m.Time)
            .ToListAsync();
        return Ok(messages);
    }

    [HttpGet("outgoing-messages")]
    public async Task<IActionResult> GetOutgoingMessages()
    {
        var messages = await _db.OutgoingMessages
            .OrderByDescending(m => m.Date)
            .ThenByDescending(m => m.Time)
            .ToListAsync();
        return Ok(messages);
    }

    [HttpPost("send-message")]
    public async Task<IActionResult> SendMessage()
    {
        var form = await Request.ReadFormAsync();

        // Валидация входных данных
        var from = form.TryGetValue("from", out var fromValue) && !string.IsNullOrEmpty(fromValue)
            ? fromValue.ToString()
            : _config["LocalCallsign"]!;

        if (!form.TryGetValue("to", out var toValue) || string.IsNullOrEmpty(toValue))
            return BadRequest("Поле 'to' обязательно");

        if (!form.TryGetValue("typeMessage", out var typeMessageValue))
            return BadRequest("Поле 'typeMessage' обязательно");

        if (!Enum.TryParse<TypeMessage>(typeMessageValue, out var typeMessage))
            return BadRequest("Неверный формат 'typeMessage'");

        string messageId = form.TryGetValue("id", out var messageIdValue) && !string.IsNullOrWhiteSpace(messageIdValue)
            ? messageIdValue.ToString()
            : Guid.NewGuid().ToString();

        var date = form.TryGetValue("date", out var dateValue) && !string.IsNullOrWhiteSpace(dateValue)
            ? DateOnly.Parse(dateValue!)
            : DateOnly.FromDateTime(DateTime.Now);

        var time = form.TryGetValue("time", out var timeValue) && !string.IsNullOrWhiteSpace(timeValue)
            ? TimeOnly.Parse(timeValue!)
            : TimeOnly.FromDateTime(DateTime.Now);

        string content;

        // Обработка аудио сообщений
        if (typeMessage == TypeMessage.Audio)
        {
            var file = form.Files["file"];
            if (file == null || file.Length == 0)
                return BadRequest("Необходимо прикрепить файл аудио");

            if (file.Length > 2 * 1024 * 1024)
                return BadRequest("Максимальный размер аудиофайла — 2MB");

            try
            {
                // Отправка файла в AudioService (/Encode)
                using var formContent = new MultipartFormDataContent();
                using var fileStream = file.OpenReadStream();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                formContent.Add(fileContent, "file", file.FileName);

                var response = await _httpClient.PostAsync("Encode", formContent);
                response.EnsureSuccessStatusCode();

                content = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке аудио");
                return StatusCode(500, "Ошибка при обработке аудио");
            }
        }
        else
        {
            if (!form.TryGetValue("content", out var contentValue) || string.IsNullOrEmpty(contentValue))
                return BadRequest("Поле 'content' обязательно");

            content = contentValue!;
        }

        // Создание и сохранение сообщения
        var message = new OutgoingMessage
        {
            DbId = Guid.NewGuid(),
            MessageId = messageId,
            To = toValue.ToString()!,
            Content = content,
            TypeMessage = typeMessage,
            Date = date,
            Time = time
        };

        await _db.OutgoingMessages.AddAsync(message);
        await _db.SaveChangesAsync();

        // Отправка в RabbitMQ
        try
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new DateOnlyConverter(), new TimeOnlyConverter() }
            };

            var json = JsonSerializer.Serialize(new
            {
                id = message.MessageId,
                content = message.Content,
                from = from,
                to = message.To,
                typeMessage = message.TypeMessage,
                date = message.Date,
                time = message.Time,
            }, options);

            using var channel = _rabbitConn.CreateModel();
            channel.QueueDeclare(_outgoingQueue, durable: true, exclusive: false, autoDelete: false);
            channel.BasicPublish("", _outgoingQueue, null, Encoding.UTF8.GetBytes(json));

            // Отправка через WebSocket
            await _manager.BroadcastAsync(JsonSerializer.Serialize(new
            {
                type = _outgoingQueue,
                id = message.MessageId,
                from = from,
                to = message.To,
                content = message.Content,
                typeMessage = message.TypeMessage,
                date = message.Date,
                time = message.Time,
            }));

            return Created($"/api/outgoing-messages/{message.MessageId}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке сообщения в RabbitMQ");
            return StatusCode(500, "Ошибка при отправке сообщения");
        }
    }

    [HttpGet("audio/{messageId}")]
    public async Task<IActionResult> GetAudioMessage(string messageId)
    {
        try
        {
            // Ищем аудио во всех типах сообщений
            string? base64Audio = await _db.IncomingMessages
                .Where(m => m.MessageId == messageId && m.TypeMessage == TypeMessage.Audio)
                .Select(m => m.Content)
                .FirstOrDefaultAsync();

            base64Audio ??= await _db.OutgoingMessages
                .Where(m => m.MessageId == messageId && m.TypeMessage == TypeMessage.Audio)
                .Select(m => m.Content)
                .FirstOrDefaultAsync();

            base64Audio ??= await _db.SentMessages
                .Where(m => m.MessageId == messageId && m.TypeMessage == TypeMessage.Audio)
                .Select(m => m.Content)
                .FirstOrDefaultAsync();

            if (base64Audio == null)
                return NotFound();

            // Декодирование через AudioService (/Decode)
            var response = await _httpClient.PostAsJsonAsync("Decode", base64Audio);
            response.EnsureSuccessStatusCode();

            return File(
                await response.Content.ReadAsByteArrayAsync(),
                "audio/wav",
                $"{messageId}.wav");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка получения аудио сообщения {messageId}");
            return StatusCode(500, "Ошибка при получении аудио");
        }
    }

    [HttpPost("audio/upload")]
    public async Task<IActionResult> UploadAudio(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не прикреплен");

            if (file.Length > 2 * 1024 * 1024)
                return BadRequest("Максимальный размер файла — 2MB");

            // Отправка в AudioService (/Encode)
            using var formContent = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
            formContent.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("Encode", formContent);
            response.EnsureSuccessStatusCode();

            var base64Audio = await response.Content.ReadAsStringAsync();
            return Ok(new { Content = base64Audio });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке аудио");
            return StatusCode(500, "Ошибка при обработке аудио");
        }
    }

}