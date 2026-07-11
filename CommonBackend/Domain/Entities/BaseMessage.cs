using CommonBackend.Domain.Enums;

namespace CommonBackend.Domain.Entities;

public class BaseMessage
{
    public Guid DbId { get; set; } = Guid.NewGuid();
    public string MessageId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public TypeMessage TypeMessage { get; set; }
}