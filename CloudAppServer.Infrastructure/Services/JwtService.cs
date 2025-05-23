using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CloudAppServer.Application.Interfaces;
using CloudAppServer.ConfigModels;
using CloudAppServer.Infrastructure.ConfigModels;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CloudAppServer.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly JwtConfig _config;
    private readonly byte[] _key;

    public JwtService(IOptions<JwtConfig> config)
    {
        _config = config.Value;
        _key = Encoding.UTF8.GetBytes(_config.Secret);
    }
    
    public string CreateToken(IEnumerable<Claim> claims)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_config.ExpirationMinutes),
            Issuer = _config.Issuer,
            Audience = _config.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}