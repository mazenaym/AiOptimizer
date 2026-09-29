namespace PromptOptimizer.Application.Optimization.DTOs;

public sealed record OptimizationResultDto(
    Guid OptimizationId,
    Guid PromptId,
    string OriginalContent,
    string OptimizedContent,
    Guid ModelId,
    string ModelName,
    string ModelIdentifier,
    string ProviderCode,
    int? ProcessingTimeMs,
    int? OriginalTokens,
    int? OptimizedTokens,
    int? TokensSaved,
    decimal? ReductionPercentage,
    int? ProviderInputTokens,
    int? ProviderOutputTokens,
    int? ProviderTotalTokens,
    decimal? EstimatedCost,
    DateTime CreatedAt
);
