namespace PromptOptimizer.Api.Contracts;

public sealed record OptimizePromptRequest(
    Guid ModelId,
    string? CustomInstructions = null);
