using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.WebSockets;
using System.Text.Json;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using CommonBackend.Domain;
using CommonBackend.Domain.Entities;
using CommonBackend.Application.Dtos;
using CommonBackend.Application.Interfaces;
using System.Collections.Concurrent;


namespace CommonBackend.Infrastructure.WebSockets;

public class WebSocketManagers : IWebSocketManager
{
    private readonly ConcurrentDictionary<WebSocket, bool> _sockets = new();

    public void AddSocket(WebSocket socket)
    {
        _sockets.TryAdd(socket, true);
        Console.WriteLine($"WebSocket подключен. Всего: {_sockets.Count}");
    }

    public void RemoveSocket(WebSocket socket)
    {
        _sockets.TryRemove(socket, out _);
        Console.WriteLine($"WebSocket отключен. Осталось: {_sockets.Count}");
    }

    public async Task BroadcastAsync(string message)
    {
        Console.WriteLine($"Broadcasting: {message}");
        var bytes = Encoding.UTF8.GetBytes(message);

        var tasks = _sockets.Keys
            .Where(s => s.State == WebSocketState.Open)
            .Select(async s =>
            {
                try
                {
                    await s.SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
                catch
                {
                    RemoveSocket(s);
                }
            });

        await Task.WhenAll(tasks);
    }
}