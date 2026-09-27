namespace PromptOptimizer.Application.Prompts.DTOs;

public sealed record PromptDto(
    Guid Id,
    string? Title,
    string OriginalContent,
    Guid? CategoryId,
    string? Language,
    bool IsSaved,
    DateTime CreatedAt);

public sealed record PromptListItemDto(
    Guid Id,
    string? Title,
    Guid? CategoryId,
    string? Language,
    bool IsSaved,
    DateTime CreatedAt);