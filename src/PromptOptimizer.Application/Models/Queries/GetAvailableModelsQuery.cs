using MediatR;
using Microsoft.EntityFrameworkCore;
using PromptOptimizer.Application.Common.Interfaces;
using PromptOptimizer.Application.Models.DTOs;

namespace PromptOptimizer.Application.Models.Queries;

public sealed record GetAvailableModelsQuery
    : IRequest<List<AIModelDto>>;

public sealed class GetAvailableModelsQueryHandler
    : IRequestHandler<GetAvailableModelsQuery, List<AIModelDto>>
{
    private readonly IAppDbContext _context;

    public GetAvailableModelsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public Task<List<AIModelDto>> Handle(
        GetAvailableModelsQuery request,
        CancellationToken cancellationToken)
    {
        return _context.AIModels
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.Provider.IsActive)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Select(x => new AIModelDto(
                x.Id,
                x.Name,
                x.ModelIdentifier,
                x.Provider.Name,
                x.Provider.Code,
                x.ContextWindow,
                x.InputPricePerMillionTokens,
                x.OutputPricePerMillionTokens))
            .ToListAsync(cancellationToken);
    }
}