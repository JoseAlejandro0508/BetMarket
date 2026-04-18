using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApuestasDeportivas.Application.Common.Interfaces;
using ApuestasDeportivas.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ApuestasDeportivas.Infrastructure.Services;

/// <summary>
/// Genera JWT para autenticación de usuarios.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(AppUser user, IList<string> roles)
    {
        var secret = _configuration["Jwt:Secret"]
                     ?? throw new InvalidOperationException("No se configuró Jwt:Secret");

        var issuer = _configuration["Jwt:Issuer"] ?? "ApuestasDeportivas.Api";
        var audience = _configuration["Jwt:Audience"] ?? "ApuestasDeportivas.Web";
        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var minutes) ? minutes : 240;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.DisplayName),
            new("balance", user.Balance.ToString("F2"))
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
