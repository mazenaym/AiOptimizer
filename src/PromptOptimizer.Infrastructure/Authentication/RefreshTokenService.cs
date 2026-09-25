using Microsoft.Extensions.Configuration;
using PromptOptimizer.Application.Auth.Interfaces;
using PromptOptimizer.Domain.Entities;
using PromptOptimizer.Infrastructure.Persistence;
using System.Security.Cryptography;

namespace PromptOptimizer.Infrastructure.Authentication;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public RefreshTokenService(
        AppDbContext dbContext,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<string> CreateAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var token = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));

        var days = _configuration.GetValue<int>(
            "Jwt:RefreshTokenExpirationDays");

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(days)
        };

        await _dbContext.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return token;
    }
}