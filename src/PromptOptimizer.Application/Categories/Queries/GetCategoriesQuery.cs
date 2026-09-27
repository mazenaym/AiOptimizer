using MediatR;
using Microsoft.EntityFrameworkCore;
using PromptOptimizer.Application.Common.Interfaces;

namespace PromptOptimizer.Application.Categories.Queries;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Code);

public sealed record GetCategoriesQuery
    : IRequest<List<CategoryDto>>;

public sealed class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IAppDbContext _context;

    public GetCategoriesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public Task<List<CategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return _context.PromptCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto(
                x.Id,
                x.Name,
                x.Code))
            .ToListAsync(cancellationToken);
    }
}