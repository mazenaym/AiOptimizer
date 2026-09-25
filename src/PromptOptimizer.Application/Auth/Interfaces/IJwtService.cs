using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Application.Auth.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user);

    DateTime GetAccessTokenExpiration();

    string GenerateRefreshToken();
}
