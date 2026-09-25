using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Application.Auth.Interfaces;

public interface IRefreshTokenService
{
    Task<RefreshToken> CreateAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task SaveAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken);
}