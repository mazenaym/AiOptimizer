namespace PromptOptimizer.Application.Common.Exceptions;

public enum AIProviderFailure
{
    Configuration,
    Authentication,
    RateLimit,
    Unavailable,
    InvalidResponse
}

// Deliberately excludes upstream bodies, URLs and inner exceptions containing secrets.
public sealed class AIProviderException : Exception
{
    public AIProviderFailure Failure { get; }

    public AIProviderException(AIProviderFailure failure, string? message = null)
        : base(message ?? $"AI provider request failed: {failure}.")
    {
        Failure = failure;
    }
}
