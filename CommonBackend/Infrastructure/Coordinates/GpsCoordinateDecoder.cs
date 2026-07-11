using System.Text.Json;
using CommonBackend.Application.Dtos;
using CommonBackend.Application.Interfaces;

namespace CommonBackend.Infrastructure.Coordinates;

public class GpsCoordinateDecoder : IGpsCoordinateDecoder
{
    public string Decode(string json)
    {
        var dto = JsonSerializer.Deserialize<GpsCoordinateDto>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (dto is null)
            throw new ArgumentException("Некорректный GPS JSON");

        if (dto.Latitude == 0 && dto.Longitude == 0)
            throw new ArgumentException("GPS JSON не содержит валидных координат");

        var result = new
        {
            nodeId = dto.NodeId,
            timestamp = dto.UtcDateTime,
            alarm = false,
            coordinates = new
            {
                lat = dto.Latitude,
                lon = dto.Longitude
            },
            speedKmh = dto.SpeedKmh,
            course = dto.Course,
            altitude = dto.Altitude
        };

        return JsonSerializer.Serialize(result);
    }
}