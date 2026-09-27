using MediatR;
using Microsoft.EntityFrameworkCore;
using PromptOptimizer.Application.Common.Exceptions;
using PromptOptimizer.Application.Common.Extensions;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Application.Prompts.DTOs;

namespace PromptOptimizer.Application.Prompts.Queries.GetPrompt;

public sealed record GetPromptQuery(Guid Id)
    : IRequest<PromptDto>;

public sealed class GetPromptQueryHandler
    : IRequestHandler<GetPromptQuery, PromptDto>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPromptQueryHandler(
        IAppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PromptDto> Handle(
        GetPromptQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();

        var prompt = await _context.Prompts
            .AsNoTracking()
            .Where(x =>
                x.Id == request.Id &&
                x.UserId == userId)
            .Select(x => new PromptDto(
                x.Id,
                x.Title,
                x.OriginalContent,
                x.CategoryId,
                x.Language,
                x.IsSaved,
                x.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        return prompt
            ?? throw new NotFoundException("Prompt", request.Id);
    }
}
