namespace PromptOptimizer.Application.Models.DTOs;

public sealed record AIModelDto(
    Guid Id,
    string Name,
    string ModelIdentifier,
    string ProviderName,
    string ProviderCode,
    int? ContextWindow,
    decimal? InputPricePerMillionTokens,
    decimal? OutputPricePerMillionTokens);