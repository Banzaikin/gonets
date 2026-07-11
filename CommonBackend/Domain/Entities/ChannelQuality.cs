namespace CommonBackend.Domain.Entities;

public class ChannelQuality
{
    public double? Level { get; set; }

    public int? LatencyMs { get; set; }

    public double? PacketLoss { get; set; }

    public int? BandwidthKbps { get; set; }
}