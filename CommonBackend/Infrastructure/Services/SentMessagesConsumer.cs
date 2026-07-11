using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CommonBackend.Application.Dtos;

// Фоновый сервис для обработки отправленных сообщений из RabbitMQ
public class SentMessagesConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _sentQueue;

    public SentMessagesConsumer(IConnection connection, IServiceProvider serviceProvider, IConfiguration config)
    {
        _connection = connection;
        _serviceProvider = serviceProvider;
        _sentQueue = config["RabbitMQ:SentQueue"] ?? "sent_messages";
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _connection.CreateModel();
        channel.QueueDeclare(_sentQueue, durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var messageJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<RabbitMessageDto>(messageJson);

                if (message != null)
                {
                    // Создаём новый scope для каждого сообщения
                    using var scope = _serviceProvider.CreateScope();
                    var messageProcessor = scope.ServiceProvider.GetRequiredService<IMessageProcessor>();
                    await messageProcessor.ProcessSentAsync(message);
                    
                    channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        };

        channel.BasicConsume(_sentQueue, autoAck: false, consumer);
        return Task.CompletedTask;
    }
}