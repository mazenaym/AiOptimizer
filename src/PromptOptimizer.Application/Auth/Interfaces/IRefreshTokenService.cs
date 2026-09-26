
namespace PromptOptimizer.Application.Auth.Interfaces;

public interface IRefreshTokenService
{
    Task<string> CreateAsync(
        Guid userId,
        CancellationToken cancellationToken);
}