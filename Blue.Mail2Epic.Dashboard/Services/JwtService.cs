using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blue.Mail2Epic.Dashboard.Models;
using Blue.Mail2Epic.Infrastructure.Interfaces;
using Blue.Mail2Epic.Infrastructure.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Blue.Mail2Epic.Dashboard.Services;

public class JwtService(IOptions<JwtOptions> jwtOptions) : IJwtService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    
    public string GenerateToken(UserAccount user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: [
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.IsAdmin ? "admin" : "user"),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            ],
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresInMinutes),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
