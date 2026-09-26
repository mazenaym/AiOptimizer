using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PromptOptimizer.Application.Auth.Interfaces;
using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Infrastructure.Authentication;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        if (!string.IsNullOrWhiteSpace(user.UserName))
        {
            claims.Add(
                new Claim(ClaimTypes.Name, user.UserName));
        }

        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT Key is not configured.");

        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: GetAccessTokenExpiration(),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    public DateTime GetAccessTokenExpiration()
    {
        var minutes = _configuration
            .GetValue<int>("Jwt:AccessTokenExpirationMinutes");

        return DateTime.UtcNow.AddMinutes(minutes);
    }

    //public string GenerateRefreshToken()
    //{
    //    var randomBytes = RandomNumberGenerator.GetBytes(64);

    //    return Convert.ToBase64String(randomBytes);
    //}
}