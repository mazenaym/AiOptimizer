namespace PromptOptimizer.Application.Models.DTOs;

public record AIModelDto(
    Guid Id,
    string Name,
    string ModelKey,
    string Provider,
    int ContextWindow,
    decimal InputCostPer1k,
    decimal OutputCostPer1k,
    bool IsActive
);
