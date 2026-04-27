using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TL.ExemploCQRS.Application.Interfaces;
using TL.ExemploCQRS.Domain.Entities;

namespace TL.ExemploCQRS.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationHours;

    public JwtService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey não configurada.");
        _issuer    = configuration["Jwt:Issuer"]   ?? "TL.ExemploCQRS";
        _audience  = configuration["Jwt:Audience"] ?? "TL.ExemploCQRS.Client";
        _expirationHours = int.Parse(configuration["Jwt:ExpirationHours"] ?? "8");
    }

    public string GenerateToken(User user)
    {
        var expiresAt   = GetTokenExpiration();
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name,           user.Username),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer:            _issuer,
            audience:          _audience,
            claims:            claims,
            notBefore:         DateTime.UtcNow,
            expires:           expiresAt,       // mesmo instante retornado ao caller
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Retorna o instante de expiração a partir do momento atual.
    /// Deve ser chamado UMA vez por fluxo de autenticação e o valor
    /// reutilizado tanto no token quanto no AuthResponse.
    /// </summary>
    public DateTime GetTokenExpiration()
        => DateTime.UtcNow.AddHours(_expirationHours);
}
