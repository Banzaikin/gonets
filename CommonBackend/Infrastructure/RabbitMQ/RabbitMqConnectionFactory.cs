using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace CommonBackend.Infrastructure.RabbitMQ;

public static class RabbitMqConnectionFactory
{
    public static IConnection Create(IConfiguration config, ILogger logger)
    {
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:Host"] ?? "localhost",
            Port = int.TryParse(config["RabbitMQ:Port"], out var port) ? port : 5672,
            UserName = config["RabbitMQ:User"] ?? "guest",
            Password = config["RabbitMQ:Password"] ?? "guest",
            DispatchConsumersAsync = true
        };

        const int maxRetries = 10;
        var delay = TimeSpan.FromSeconds(2);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var connection = factory.CreateConnection();
                logger.LogInformation("Подключение к RabbitMQ на {Host}:{Port}", factory.HostName, factory.Port);
                return connection;
            }
            catch (BrokerUnreachableException ex) when (ex.InnerException is SocketException)
            {
                logger.LogWarning("Попытка {Attempt}/{MaxAttempts}: {Message}", attempt, maxRetries, ex.Message);
                if (attempt == maxRetries)
                {
                    logger.LogError("RabbitMQ недоступен после {MaxAttempts} попыток", maxRetries);
                    throw;
                }
                Thread.Sleep(delay);
            }
        }

        throw new Exception("Не удалось создать подключение к RabbitMQ.");
    }
}
