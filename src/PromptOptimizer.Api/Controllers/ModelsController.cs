using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromptOptimizer.Application.Models.DTOs;
using PromptOptimizer.Application.Models.Queries;

namespace PromptOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/models")]
public sealed class ModelsController : ControllerBase
{
    private readonly ISender _sender;

    public ModelsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(List<AIModelDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AIModelDto>>> Get(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetAvailableModelsQuery(),
            cancellationToken);

        return Ok(result);
    }
}