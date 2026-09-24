//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using PromptOptimizer.Application.Common.Interfaces;
//using PromptOptimizer.Application.Common.Results;
//using PromptOptimizer.Application.Optimization.DTOs;

//namespace PromptOptimizer.Application.Prompts.Queries.GetPromptHistory;

//public record GetPromptHistoryQuery(Guid? PromptId, int PageNumber = 1, int PageSize = 20) : IRequest<Result<List<OptimizationResultDto>>>;

//public class GetPromptHistoryQueryHandler : IRequestHandler<GetPromptHistoryQuery, Result<List<OptimizationResultDto>>>
//{
//    private readonly IAppDbContext _context;
//    private readonly ICurrentUserService _currentUserService;

//    public GetPromptHistoryQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
//    {
//        _context = context;
//        _currentUserService = currentUserService;
//    }

//    public async Task<Result<List<OptimizationResultDto>>> Handle(GetPromptHistoryQuery request, CancellationToken cancellationToken)
//    {
//        var userId = _currentUserService.UserId;
//        if (!userId.HasValue) return Result.Failure<List<OptimizationResultDto>>("Unauthorized");

//        var query = _context.Optimizations
//            .AsNoTracking()
//            .Include(o => o.Prompt)
//            .Include(o => o.AIModel)
//            .Where(o => o.Prompt.UserId == userId.Value);

//        if (request.PromptId.HasValue)
//        {
//            query = query.Where(o => o.PromptId == request.PromptId.Value);
//        }

//        var items = await query
//            .OrderByDescending(o => o.CreatedAt)
//            .Skip((request.PageNumber - 1) * request.PageSize)
//            .Take(request.PageSize)
//            .Select(o => new OptimizationResultDto(
//                o.Id,
//                o.PromptId,
//                o.OriginalContent,
//                o.OptimizedContent,
//                o.Explanation,
//                o.Status,
//                o.TokenUsage.PromptTokens,
//                o.TokenUsage.CompletionTokens,
//                o.TokenUsage.TotalTokens,
//                o.ExecutionTimeMs,
//                o.AIModel.Name,
//                o.CreatedAt
//            ))
//            .ToListAsync(cancellationToken);

//        return Result.Success(items);
//    }
//}
