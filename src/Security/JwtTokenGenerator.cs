using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;   // <<< add this

namespace VoteOp.AuthApi.Security;

public class JwtTokenGenerator
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _key;

    public JwtTokenGenerator(IConfiguration config)
    {
        _issuer = config["JwtIssuer"] ?? throw new InvalidOperationException("JwtIssuer not configured.");
        _audience = config["JwtAudience"] ?? throw new InvalidOperationException("JwtAudience not configured.");
        _key = config["JwtKey"] ?? throw new InvalidOperationException("JwtKey not configured.");
    }

    public string Generate(Guid userId, string email)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}