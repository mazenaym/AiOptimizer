using PromptOptimizer.Domain.Enums;

namespace PromptOptimizer.Application.Prompts.DTOs;

public record PromptDto(
    Guid Id,
    Guid UserId,
    string Title,
    string Content,
    PromptCategory Category,
    bool IsFavorite,
    List<string> Tags,
    DateTime CreatedAt,
    DateTime? LastModifiedAt
);
