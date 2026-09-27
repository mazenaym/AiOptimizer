using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromptOptimizer.Application.Categories.Queries;

namespace PromptOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(List<CategoryDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryDto>>> Get(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetCategoriesQuery(),
            cancellationToken);

        return Ok(result);
    }
}