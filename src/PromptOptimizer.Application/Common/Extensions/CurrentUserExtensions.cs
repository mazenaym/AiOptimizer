using PromptOptimizer.Application.Common.Exceptions;
using PromptOptimizer.Application.Common.Interfaces;

namespace PromptOptimizer.Application.Common.Extensions;

public static class CurrentUserExtensions
{
    public static Guid GetRequiredUserId(
        this ICurrentUserService currentUser)
    {
        var userId = currentUser.UserId;

        if (!currentUser.IsAuthenticated
            || !userId.HasValue
            || userId.Value == Guid.Empty)
        {
            throw new UnauthorizedException(
                "Authentication is required.");
        }

        return userId.Value;
    }
}