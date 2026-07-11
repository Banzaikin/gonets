using CommonBackend.Application.Dtos;
using CommonBackend.Domain.Entities;

namespace CommonBackend.Application.Mappers;

public static class ChannelStateMapper
{
    public static ChannelState ToDomain(ChannelStatusDto dto)
    {
        return new ChannelState
        {
            Channel = dto.Channel,
            IsAvailable = dto.IsAvailable,
            TimestampUtc = dto.TimestampUtc,

            Quality = dto.Quality is null ? null : new ChannelQuality
            {
                Level = dto.Quality.Level,
                LatencyMs = dto.Quality.LatencyMs,
                PacketLoss = dto.Quality.PacketLoss,
                BandwidthKbps = dto.Quality.BandwidthKbps
            },

            Metrics = dto.Metrics
        };
    }
}