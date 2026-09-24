//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using PromptOptimizer.Application.Common.Interfaces;
//using PromptOptimizer.Application.Common.Results;
//using PromptOptimizer.Application.Usage.DTOs;

//namespace PromptOptimizer.Application.Usage.Queries;

//public record GetUserUsageQuery : IRequest<Result<UsageSummaryDto>>;

//public class GetUserUsageQueryHandler : IRequestHandler<GetUserUsageQuery, Result<UsageSummaryDto>>
//{
//    private readonly IAppDbContext _context;
//    private readonly ICurrentUserService _currentUserService;

//    public GetUserUsageQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
//    {
//        _context = context;
//        _currentUserService = currentUserService;
//    }

//    public async Task<Result<UsageSummaryDto>> Handle(GetUserUsageQuery request, CancellationToken cancellationToken)
//    {
//        var userId = _currentUserService.UserId;
//        if (!userId.HasValue) return Result.Failure<UsageSummaryDto>("Unauthorized");

//        var records = await _context.UsageRecords
//            .AsNoTracking()
//            .Where(r => r.UserId == userId.Value)
//            .ToListAsync(cancellationToken);

//        var totalOptimizations = records.Count;
//        var totalPromptTokens = records.Sum(r => r.TokenUsage.PromptTokens);
//        var totalCompletionTokens = records.Sum(r => r.TokenUsage.CompletionTokens);
//        var totalTokens = totalPromptTokens + totalCompletionTokens;
//        var totalCost = records.Sum(r => r.EstimatedCost);

//        var dto = new UsageSummaryDto(
//            totalOptimizations,
//            totalPromptTokens,
//            totalCompletionTokens,
//            totalTokens,
//            totalCost
//        );

//        return Result.Success(dto);
//    }
//}
