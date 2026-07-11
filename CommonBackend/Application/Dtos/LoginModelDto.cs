namespace CommonBackend.Application.Dtos;

public class LoginModelDto
{
    public required string Username { get; set; } = string.Empty;
    public required string Password { get; set; } = string.Empty;
}
