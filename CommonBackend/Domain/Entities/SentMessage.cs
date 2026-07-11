using CommonBackend.Domain.Enums;

namespace CommonBackend.Domain.Entities;

public class SentMessage : BaseMessage
{
    public string To { get; set; } = string.Empty;
}