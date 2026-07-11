using CommonBackend.Application.Dtos;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public interface IMessageProcessor
{
    Task ProcessIncomingAsync(RabbitMessageDto dto);
    Task ProcessSentAsync(RabbitMessageDto dto);
}