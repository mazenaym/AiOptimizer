using PromptOptimizer.Domain.ValueObjects;

namespace PromptOptimizer.Application.Common.Interfaces;

public record AIOptimizationResponse(string OptimizedContent, string Explanation, TokenUsage TokenUsage, double ExecutionTimeMs);

public interface IAIService
{
    string ProviderName { get; }
    Task<AIOptimizationResponse> OptimizePromptAsync(string originalPrompt, string systemInstructions, string modelKey, CancellationToken cancellationToken = default);
}

public interface IAIServiceFactory
{
    IAIService GetService(string providerName);
}
