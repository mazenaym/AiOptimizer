using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PromptOptimizer.Application.Common.Extensions;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Application.Prompts.DTOs;
using PromptOptimizer.Domain.Entities;

using AppValidationException =
    PromptOptimizer.Application.Common.Exceptions.ValidationException;

namespace PromptOptimizer.Application.Prompts.Commands.CreatePrompt;

public sealed record CreatePromptCommand(
    string OriginalContent,
    string? Title,
    Guid? CategoryId,
    string? Language)
    : IRequest<PromptDto>;

public sealed class CreatePromptCommandValidator
    : AbstractValidator<CreatePromptCommand>
{
    public CreatePromptCommandValidator()
    {
        RuleFor(x => x.OriginalContent)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(10_000);

        RuleFor(x => x.Title)
            .MaximumLength(255);

        RuleFor(x => x.Language)
            .MaximumLength(20);

        RuleFor(x => x.CategoryId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("CategoryId must be a valid identifier.");
    }
}

public sealed class CreatePromptCommandHandler
    : IRequestHandler<CreatePromptCommand, PromptDto>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreatePromptCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PromptDto> Handle(
        CreatePromptCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();

        if (request.CategoryId.HasValue)
        {
            var categoryId = request.CategoryId.Value;

            var categoryExists = await _context.PromptCategories
                .AnyAsync(
                    x => x.Id == categoryId && x.IsActive,
                    cancellationToken);

            if (!categoryExists)
            {
                throw new AppValidationException(
                    new[]
                    {
                        new ValidationFailure(
                            nameof(request.CategoryId),
                            "The selected category does not exist or is inactive.")
                    });
            }
        }

        var prompt = new Prompt
        {
            UserId = userId,
            Title = NormalizeOptional(request.Title),

            // Preserve the user's original text.
            OriginalContent = request.OriginalContent,

            CategoryId = request.CategoryId,
            Language = NormalizeOptional(request.Language),
            IsSaved = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Prompts.Add(prompt);

        await _context.SaveChangesAsync(cancellationToken);

        return new PromptDto(
            prompt.Id,
            prompt.Title,
            prompt.OriginalContent,
            prompt.CategoryId,
            prompt.Language,
            prompt.IsSaved,
            prompt.CreatedAt);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
