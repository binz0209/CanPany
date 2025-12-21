using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CanPany.Infrastructure.Identity.Jwt;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public (string accessToken, int expiresIn) GenerateToken(string userId, string email, string role, bool rememberMe = false)
    {
        var key = _config["Jwt:Key"];
        var issuer = _config["Jwt:Issuer"] ?? "CanPany";
        var audience = _config["Jwt:Audience"] ?? "CanPanyClient";

        var expires = rememberMe
            ? DateTime.UtcNow.AddDays(7)
            : DateTime.UtcNow.AddMinutes(5);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.WriteToken(token);
        var expiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds;

        return (accessToken, expiresIn);
    }
}

