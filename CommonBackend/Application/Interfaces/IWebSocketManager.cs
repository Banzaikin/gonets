using System.Net.WebSockets;
using System.Threading.Tasks;

namespace CommonBackend.Application.Interfaces;

public interface IWebSocketManager
{
    void AddSocket(WebSocket socket);
    void RemoveSocket(WebSocket socket);
    Task BroadcastAsync(string message);
}