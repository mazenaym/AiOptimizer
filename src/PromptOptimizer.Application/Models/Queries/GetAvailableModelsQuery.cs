//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using PromptOptimizer.Application.Common.Interfaces;
//using PromptOptimizer.Application.Common.Results;
//using PromptOptimizer.Application.Models.DTOs;

//namespace PromptOptimizer.Application.Models.Queries;

//public record GetAvailableModelsQuery : IRequest<Result<List<AIModelDto>>>;

//public class GetAvailableModelsQueryHandler : IRequestHandler<GetAvailableModelsQuery, Result<List<AIModelDto>>>
//{
//    private readonly IAppDbContext _context;

//    public GetAvailableModelsQueryHandler(IAppDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Result<List<AIModelDto>>> Handle(GetAvailableModelsQuery request, CancellationToken cancellationToken)
//    {
//        var models = await _context.AIModels
//            .AsNoTracking()
//            .Where(m => m.IsActive)
//            .Select(m => new AIModelDto(
//                m.Id,
//                m.Name,
//                m.ModelKey,
//                m.Provider,
//                m.ContextWindow,
//                m.InputCostPer1k,
//                m.OutputCostPer1k,
//                m.IsActive
//            ))
//            .ToListAsync(cancellationToken);

//        return Result.Success(models);
//    }
//}
