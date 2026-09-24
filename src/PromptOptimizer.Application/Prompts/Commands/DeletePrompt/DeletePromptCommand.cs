using MediatR;
using Microsoft.EntityFrameworkCore;
using PromptOptimizer.Application.Common.Exceptions;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Application.Common.Results;
using PromptOptimizer.Domain.Entities;
using PromptOptimizer.Infrastructure.Persistence;

namespace PromptOptimizer.Application.Prompts.Commands.DeletePrompt;

public record DeletePromptCommand(Guid Id) : IRequest<Result<bool>>;

//public class DeletePromptCommandHandler : IRequestHandler<DeletePromptCommand, Result<bool>>
//{
//    private readonly IAppDbContext _context;
//    private readonly ICurrentUserService _currentUserService;

//    public DeletePromptCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
//    {
//        _context = context;
//        _currentUserService = currentUserService;
//    }

//    //public async Task<Result<bool>> Handle(DeletePromptCommand request, CancellationToken cancellationToken)
//    //{
//    //    var userId = _currentUserService.UserId;
//    //    if (!userId.HasValue) return Result.Failure<bool>("Unauthorized");

//    //    var prompt = await _context.Prompts.FirstOrDefaultAsync(p => p.Id == request.Id && p.UserId == userId.Value, cancellationToken)
//    //        ?? throw new NotFoundException(nameof(Prompt), request.Id);

//    //    _context.Prompts.Remove(prompt);
//    //    await _context.SaveChangesAsync(cancellationToken);

//    //    return Result.Success(true);
//    //}
//}
