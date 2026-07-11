using CommonBackend.Infrastructure.Coordinates;
using CommonBackend.Infrastructure.Persistence;
using CommonBackend.Infrastructure.MessageProcessing;
using CommonBackend.Infrastructure.WebSockets;
using CommonBackend.Infrastructure.RabbitMQ;
using CommonBackend.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace CommonBackend;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // AppDbContext
        var connectionString = configuration.GetConnectionString("Postgres") 
            ?? "Host=localhost;Port=5432;Database=messenger;Username=postgres;Password=123";

        services.AddDbContext<IAppDbContext, AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddSingleton<ICoordinateService, CoordinateDecoder>();
        services.AddSingleton<IGpsCoordinateDecoder, GpsCoordinateDecoder>();

        // Infrastructure service
        services.AddScoped<IMessageProcessor, MessageProcessor>();
        services.AddSingleton<IChannelStateService, ChannelStateService>();

        // WebSocket
        services.AddSingleton<IWebSocketManager, WebSocketManagers>();

        // RabbitMQ connection
        services.AddSingleton<IConnection>(sp =>
            RabbitMqConnectionFactory.Create(sp.GetRequiredService<IConfiguration>(), sp.GetRequiredService<ILogger<Program>>()));

        // Background services
        services.AddHostedService<RabbitMqConsumer>();
        services.AddHostedService<SentMessagesConsumer>();
        services.AddHostedService<ChannelStatusConsumer>();

        return services;
    }
}
