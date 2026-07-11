namespace CommonBackend.Application.Dtos;

public sealed class GpsCoordinateDto
{
    public string? NodeId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double SpeedKmh { get; set; }
    public double Course { get; set; }
    public double Altitude { get; set; }
    public DateTime UtcDateTime { get; set; }
}