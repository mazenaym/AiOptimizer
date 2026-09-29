namespace PromptOptimizer.Application.Optimization.Models;

public record PromptOptimizationRequest(
    string PromptContent,
    string? CategoryName,
    string? Language,
    string ProviderCode,
    string ModelIdentifier,
    string? CustomInstructions = null
);
