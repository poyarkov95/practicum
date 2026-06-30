using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Services.Interface;
using Application.User;
using Domain.Entities;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Implementation;

public class JWTGenerator(IOptions<TokenMetadata> tokenMetadata) : IJWTGenerator
{
    public string GenerateToken(LoginRequestDto request)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, request.UserId.ToString()),
            new(ClaimTypes.Role, request.Role == UserRole.Admin ? "Admin" : "User")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenMetadata.Value.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: tokenMetadata.Value.Issuer,
            audience: tokenMetadata.Value.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(tokenMetadata.Value.LifeTimeInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}