using System.Text;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using CommonBackend.Domain.Enums;
using CommonBackend.Application.Dtos;
using CommonBackend.Application.Interfaces;
using Microsoft.Extensions.Logging;

public class RabbitMqConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebSocketManager _webSocketManager;
    private readonly ICoordinateService _coordinateService;
    private readonly IGpsCoordinateDecoder _gpsCoordinateDecoder;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly string _incomingQueue;

    public RabbitMqConsumer(
        IConnection connection,
        IServiceProvider serviceProvider,
        IWebSocketManager webSocketManager,
        ILogger<RabbitMqConsumer> logger,
        ICoordinateService coordinateService,
        IGpsCoordinateDecoder gpsCoordinateDecoder,
        IConfiguration config)
    {
        _logger = logger;
        _connection = connection;
        _serviceProvider = serviceProvider;
        _webSocketManager = webSocketManager;
        _coordinateService = coordinateService;
        _gpsCoordinateDecoder = gpsCoordinateDecoder;
        _incomingQueue = config["RabbitMQ:IncomingQueue"] ?? "incoming_messages";
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _connection.CreateModel();
        channel.QueueDeclare(queue: _incomingQueue,
                            durable: true,
                            exclusive: false,
                            autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<RabbitMessageDto>(messageJson);

                if (message != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                    bool alreadyExists = await db.IncomingMessages.AnyAsync(m => m.MessageId == message.Id);

                    if (alreadyExists)
                    {
                        _logger.LogWarning(
                            "Дубликат incoming-сообщения в RabbitMqConsumer: MessageId={MessageId}. Обработка пропущена.",
                            message.Id);

                        channel.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    string content = message.Content;
                    _logger.LogInformation(content);

                    if (message.TypeMessage == TypeMessage.Coordinates)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(message.Content) &&
                                (message.Content.TrimStart().StartsWith("{") || message.Content.TrimStart().StartsWith("[")))
                            {
                                content = _gpsCoordinateDecoder.Decode(message.Content);
                            }
                            else
                            {
                                var rawData = Convert.FromHexString(message.Content);
                                content = _coordinateService.Decode(rawData);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Ошибка декодирования координат");
                        }
                    }

                    var incomingMessage = new CommonBackend.Domain.Entities.IncomingMessage
                    {
                        DbId = Guid.NewGuid(),
                        MessageId = message.Id,
                        From = message.From,
                        Content = content,
                        TypeMessage = message.TypeMessage,
                        Date = message.Date,
                        Time = message.Time,
                    };

                    await db.IncomingMessages.AddAsync(incomingMessage);
                    await db.SaveChangesAsync();

                    await _webSocketManager.BroadcastAsync(JsonSerializer.Serialize(new
                    {
                        type = _incomingQueue,
                        id = incomingMessage.MessageId,
                        from = incomingMessage.From,
                        content = incomingMessage.Content,
                        typeMessage = incomingMessage.TypeMessage,
                        date = incomingMessage.Date,
                        time = incomingMessage.Time,
                    }));

                    channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке сообщения");
            }
        };

        channel.BasicConsume(queue: _incomingQueue,
                            autoAck: false,
                            consumer: consumer);

        return Task.CompletedTask;
    }
}