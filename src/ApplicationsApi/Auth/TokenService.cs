using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ApplicationsApi.Auth;

public class TokenService(string key)
{
    public const string Issuer = "applications-api";
    public const string Audience = "applications-admin";
    public const int LifetimeMinutes = 30;

    public string CreateToken(string login)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: new[] { new Claim("name", login), new Claim("role", "admin") },
            expires: DateTime.UtcNow.AddMinutes(LifetimeMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}