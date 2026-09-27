using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PromptOptimizer.Application.Common.Extensions;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Application.Common.Models;
using PromptOptimizer.Application.Prompts.DTOs;

namespace PromptOptimizer.Application.Prompts.Queries.GetPrompts;

public sealed record GetPromptsQuery(
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<PromptListItemDto>>;

public sealed class GetPromptsQueryValidator
    : AbstractValidator<GetPromptsQuery>
{
    public GetPromptsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x)
            .Must(x =>
                x.PageNumber < 1 ||
                x.PageSize < 1 ||
                ((long)x.PageNumber - 1) * x.PageSize
                    <= int.MaxValue)
            .WithMessage("The requested page is too large.")
            .OverridePropertyName(nameof(GetPromptsQuery.PageNumber));
    }
}

public sealed class GetPromptsQueryHandler
    : IRequestHandler<
        GetPromptsQuery,
        PagedResult<PromptListItemDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPromptsQueryHandler(
        IAppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PromptListItemDto>> Handle(
        GetPromptsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();

        var query = _context.Prompts
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        var totalCount = await query.CountAsync(
            cancellationToken);

        var skip = checked(
            (request.PageNumber - 1) * request.PageSize);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(x => new PromptListItemDto(
                x.Id,
                x.Title,
                x.CategoryId,
                x.Language,
                x.IsSaved,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<PromptListItemDto>(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount);
    }
}