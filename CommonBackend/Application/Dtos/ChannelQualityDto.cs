using System.Text.Json.Serialization;

namespace CommonBackend.Application.Dtos;

public class ChannelQualityDto
{
    [JsonPropertyName("level")]
    public double? Level { get; set; }

    [JsonPropertyName("latencyMs")]
    public int? LatencyMs { get; set; }

    [JsonPropertyName("packetLoss")]
    public double? PacketLoss { get; set; }

    [JsonPropertyName("bandwidthKbps")]
    public int? BandwidthKbps { get; set; }
}