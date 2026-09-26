using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(ApplicationUser user);
    RefreshToken GenerateRefreshToken(Guid userId);
}
