using CommonBackend.Domain.Enums;

namespace CommonBackend.Domain.Entities;

public class ChannelState
{
    public ChannelType Channel { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime TimestampUtc { get; set; }

    public ChannelQuality? Quality { get; set; }
    public Dictionary<string, object>? Metrics { get; set; }
}