using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using CommonBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CommonBackend.Api.Controllers;

[ApiController]
[Route("ws")]
public class WebSocketController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IWebSocketManager _manager;

    public WebSocketController(IConfiguration config, IWebSocketManager manager)
    {
        _config = config;
        _manager = manager;
    }

    [HttpGet]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        var token = HttpContext.Request.Query["token"].ToString();
        if (string.IsNullOrEmpty(token) || !ValidateToken(token))
        {
            HttpContext.Response.StatusCode = 401; // Unauthorized
            return;
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        _manager.AddSocket(webSocket);

        var buffer = new byte[1024];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine($"WebSocket ошибка: {ex.Message}");
        }
        finally
        {
            _manager.RemoveSocket(webSocket);

            if (webSocket.State != WebSocketState.Closed)
            {
                try
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closed by server",
                        CancellationToken.None);
                }
                catch {}
            }
        }

        _manager.RemoveSocket(webSocket);
    }

    private bool ValidateToken(string token)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        if (string.IsNullOrEmpty(secret))
            throw new InvalidOperationException("JwtSettings:Secret is not configured.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),

                ValidateIssuer = true,
                ValidIssuer = issuer,

                ValidateAudience = true,
                ValidAudience = audience,

                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
