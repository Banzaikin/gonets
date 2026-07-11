namespace CommonBackend.Application.Interfaces
{
    public interface IGpsCoordinateDecoder
    {
        string Decode(string json);
    }
}