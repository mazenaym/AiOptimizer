using MediatR;
using Microsoft.EntityFrameworkCore;
using PromptOptimizer.Application.Common.Exceptions;
using PromptOptimizer.Application.Common.Extensions;
using PromptOptimizer.Application.Common.Interfaces;

namespace PromptOptimizer.Application.Prompts.Commands.DeletePrompt;

public sealed record DeletePromptCommand(Guid Id)
    : IRequest<Unit>;

public sealed class DeletePromptCommandHandler
    : IRequestHandler<DeletePromptCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeletePromptCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(
        DeletePromptCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetRequiredUserId();

        var prompt = await _context.Prompts
            .SingleOrDefaultAsync(
                x => x.Id == request.Id &&
                     x.UserId == userId,
                cancellationToken);

        if (prompt is null)
        {
            throw new NotFoundException(
                "Prompt",
                request.Id);
        }

        _context.Prompts.Remove(prompt);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}