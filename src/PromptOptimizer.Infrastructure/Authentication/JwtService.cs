//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Security.Cryptography;
//using System.Text;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using PromptOptimizer.Application.Common.Interfaces;
//using PromptOptimizer.Domain.Entities;

//namespace PromptOptimizer.Infrastructure.Authentication;

//public class JwtService : IJwtTokenGenerator
//{
//    private readonly IConfiguration _configuration;

//    public JwtService(IConfiguration configuration)
//    {
//        _configuration = configuration;
//    }

//    public string GenerateAccessToken(User user)
//    {
//        var secret = _configuration["Jwt:Secret"] ?? "SuperSecretKeyForPromptOptimizerNet10Core2026!";
//        var issuer = _configuration["Jwt:Issuer"] ?? "PromptOptimizerApi";
//        var audience = _configuration["Jwt:Audience"] ?? "PromptOptimizerUsers";
//        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationInMinutes"] ?? "60");

//        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
//        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//        var claims = new[]
//        {
//            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
//            new Claim(JwtRegisteredClaimNames.Email, user.Email),
//            new Claim("name", user.FullName),
//            new Claim("plan", user.Plan.ToString()),
//            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
//        };

//        var token = new JwtSecurityToken(
//            issuer: issuer,
//            audience: audience,
//            claims: claims,
//            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
//            signingCredentials: creds
//        );

//        return new JwtSecurityTokenHandler().WriteToken(token);
//    }

//    public RefreshToken GenerateRefreshToken(Guid userId)
//    {
//        var randomNumber = new byte[32];
//        using var rng = RandomNumberGenerator.Create();
//        rng.GetBytes(randomNumber);

//        return new RefreshToken
//        {
//            UserId = userId,
//            Token = Convert.ToBase64String(randomNumber),
//            ExpiresAt = DateTime.UtcNow.AddDays(7),
//            CreatedAt = DateTime.UtcNow
//        };
//    }
//}
