using CommonBackend.Domain.Enums;

namespace CommonBackend.Domain.Entities;

public class IncomingMessage : BaseMessage
{
    public string From { get; set; } = string.Empty;
}