//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using PromptOptimizer.Application.Common.Exceptions;
//using PromptOptimizer.Application.Common.Interfaces;
//using PromptOptimizer.Application.Common.Results;
//using PromptOptimizer.Application.Prompts.DTOs;
//using PromptOptimizer.Domain.Entities;

//namespace PromptOptimizer.Application.Prompts.Queries.GetPrompt;

//public record GetPromptQuery(Guid Id) : IRequest<Result<PromptDto>>;

//public class GetPromptQueryHandler : IRequestHandler<GetPromptQuery, Result<PromptDto>>
//{
//    private readonly IAppDbContext _context;
//    private readonly ICurrentUserService _currentUserService;

//    public GetPromptQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
//    {
//        _context = context;
//        _currentUserService = currentUserService;
//    }

//    public async Task<Result<PromptDto>> Handle(GetPromptQuery request, CancellationToken cancellationToken)
//    {
//        var userId = _currentUserService.UserId;
//        if (!userId.HasValue) return Result.Failure<PromptDto>("Unauthorized");

//        var prompt = await _context.Prompts
//            .AsNoTracking()
//            .FirstOrDefaultAsync(p => p.Id == request.Id && p.UserId == userId.Value, cancellationToken)
//            ?? throw new NotFoundException(nameof(Prompt), request.Id);

//        var dto = new PromptDto(
//            prompt.Id,
//            prompt.UserId,
//            prompt.Title,
//            prompt.Content,
//            prompt.Category,
//            prompt.IsFavorite,
//            prompt.Tags,
//            prompt.CreatedAt,
//            prompt.LastModifiedAt
//        );

//        return Result.Success(dto);
//    }
//}
