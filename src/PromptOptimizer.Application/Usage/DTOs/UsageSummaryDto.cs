namespace PromptOptimizer.Application.Usage.DTOs;

public record UsageSummaryDto(
    int TotalOptimizations,
    int TotalPromptTokens,
    int TotalCompletionTokens,
    int TotalTokens,
    decimal TotalEstimatedCost
);
