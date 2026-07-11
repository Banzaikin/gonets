using System.Text.Json.Serialization;
using CommonBackend.Domain.Enums;

namespace CommonBackend.Application.Dtos;

public class ChannelStatusDto
{
    [JsonPropertyName("channel")]
    public ChannelType Channel { get; set; }

    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    [JsonPropertyName("timestampUtc")]
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("quality")]
    public ChannelQualityDto? Quality { get; set; }

    [JsonPropertyName("metrics")]
    public Dictionary<string, object>? Metrics { get; set; }
}