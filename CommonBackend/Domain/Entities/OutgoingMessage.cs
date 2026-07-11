using CommonBackend.Domain.Enums;

namespace CommonBackend.Domain.Entities;

public class OutgoingMessage : BaseMessage
{
    public string To { get; set; } = string.Empty;
}