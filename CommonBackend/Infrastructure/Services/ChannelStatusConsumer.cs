using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommonBackend.Application.Dtos;
using CommonBackend.Application.Interfaces;
using CommonBackend.Application.Mappers;
using Microsoft.Extensions.Logging;

public class ChannelStatusConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IWebSocketManager _ws;
    private readonly ILogger<ChannelStatusConsumer> _logger;
    private readonly IChannelStateService _stateService;

    public ChannelStatusConsumer(
        IConnection connection,
        IWebSocketManager ws,
        ILogger<ChannelStatusConsumer> logger,
        IChannelStateService stateService)
    {
        _connection = connection;
        _ws = ws;
        _logger = logger;
        _stateService = stateService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _connection.CreateModel();

        channel.QueueDeclare("channel_status", true, false, false);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                var msg = JsonSerializer.Deserialize<ChannelStatusDto>(json, options);

                if (msg == null)
                    return;

                var state = ChannelStateMapper.ToDomain(msg);

                _stateService.Update(state);

                await _ws.BroadcastAsync(JsonSerializer.Serialize(new
                {
                    type = "channel_status",
                    channel = msg.Channel.ToString(),
                    isAvailable = msg.IsAvailable,
                    timestampUtc = msg.TimestampUtc,
                    quality = msg.Quality,
                    metrics = msg.Metrics
                }));

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка channel_status");
            }
        };

        channel.BasicConsume("channel_status", false, consumer);

        return Task.CompletedTask;
    }
}