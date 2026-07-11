using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CommonBackend.Domain.Entities;
using CommonBackend.Application.Dtos;
using CommonBackend.Application.Interfaces;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IAppDbContext _dbContext;

    public AuthController(IConfiguration config, IAppDbContext dbContext)
    {
        _config = config;
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModelDto model)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == model.Username);

        if (user != null && VerifyPassword(model.Password, user.PasswordHash))
        {
            var token = GenerateJwtToken(user.Id.ToString());
            return Ok(new { token });
        }

        return Unauthorized();
    }


    private string GenerateJwtToken(string userId)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"] ?? throw new InvalidOperationException("JwtSettings:Secret is not configured."),
            audience: jwtSettings["Audience"] ?? throw new InvalidOperationException("JwtSettings:Secret is not configured."),
            claims: new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

}