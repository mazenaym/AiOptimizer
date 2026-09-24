using PromptOptimizer.Domain.Enums;

namespace PromptOptimizer.Application.Optimization.DTOs;

public record OptimizationResultDto(
    Guid OptimizationId,
    Guid PromptId,
    string OriginalContent,
    string OptimizedContent,
    string Explanation,
    OptimizationStatus Status,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    double ExecutionTimeMs,
    string? ModelName,
    DateTime CreatedAt
);
