using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CommonBackend.Domain.Entities;
using CommonBackend.Application.Interfaces;
using CommonBackend.Application.Dtos;
using CommonBackend.Infrastructure.Persistence;
using CommonBackend.Infrastructure.WebSockets;

namespace CommonBackend.Infrastructure.MessageProcessing;

public class MessageProcessor : IMessageProcessor
{
    private readonly AppDbContext _db;
    private readonly IWebSocketManager _webSocketManager;
    private readonly string _sentQueue;
    private readonly string _incomingQueue;
    private readonly ILogger<MessageProcessor> _logger;

    public MessageProcessor(
        AppDbContext db,
        IWebSocketManager webSocketManager,
        IConfiguration config,
        ILogger<MessageProcessor> logger)
    {
        _db = db;
        _webSocketManager = webSocketManager;
        _sentQueue = config["RabbitMQ:SentQueue"] ?? "sent_messages";
        _incomingQueue = config["RabbitMQ:IncomingQueue"] ?? "incoming_messages";
        _logger = logger;
    }

    public async Task ProcessIncomingAsync(RabbitMessageDto message)
    {
        bool alreadyExists = await _db.IncomingMessages.AnyAsync(m => m.MessageId == message.Id);
        if (alreadyExists)
        {
            _logger.LogWarning(
                "Дубликат сообщения в очереди {Queue}: MessageId={MessageId}. Сохранение пропущено.",
                _sentQueue,
                message.Id);
            return;
        }

        var incomingMessage = new IncomingMessage
        {
            DbId = Guid.NewGuid(),
            MessageId = message.Id,
            From = message.From,
            Content = message.Content,
            TypeMessage = message.TypeMessage,
            Date = message.Date,
            Time = message.Time,
        };

        await _db.IncomingMessages.AddAsync(incomingMessage);
        await _db.SaveChangesAsync();

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
    }

    public async Task ProcessSentAsync(RabbitMessageDto message)
    {
        bool alreadyExists = await _db.SentMessages.AnyAsync(m => m.MessageId == message.Id);
        if (alreadyExists)
        {
            _logger.LogWarning(
                "Дубликат сообщения в очереди {Queue}: MessageId={MessageId}. Сохранение пропущено.",
                _sentQueue,
                message.Id);
            return;
        }

        var sentMessage = new SentMessage
        {
            DbId = Guid.NewGuid(),
            MessageId = message.Id,
            To = message.To,
            Content = message.Content,
            TypeMessage = message.TypeMessage,
            Date = message.Date,
            Time = message.Time,
        };

        await _db.SentMessages.AddAsync(sentMessage);

        var outgoing = await _db.OutgoingMessages.FirstOrDefaultAsync(m => m.MessageId == message.Id);
        if (outgoing != null)
        {
            _db.OutgoingMessages.Remove(outgoing);
        }

        await _db.SaveChangesAsync();

        await _webSocketManager.BroadcastAsync(JsonSerializer.Serialize(new
        {
            type = _sentQueue,
            id = sentMessage.MessageId,
            to = sentMessage.To,
            content = sentMessage.Content,
            typeMessage = sentMessage.TypeMessage,
            date = sentMessage.Date,
            time = sentMessage.Time,
        }));
    }
}
