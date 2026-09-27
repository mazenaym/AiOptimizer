using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromptOptimizer.Application.Common.Models;
using PromptOptimizer.Application.Prompts.Commands.CreatePrompt;
using PromptOptimizer.Application.Prompts.Commands.DeletePrompt;
using PromptOptimizer.Application.Prompts.DTOs;
using PromptOptimizer.Application.Prompts.Queries.GetPrompt;
using PromptOptimizer.Application.Prompts.Queries.GetPrompts;

namespace PromptOptimizer.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/prompts")]
public sealed class PromptsController : ControllerBase
{
    private readonly ISender _sender;

    public PromptsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(PromptDto),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<PromptDto>> Create(
        [FromBody] CreatePromptCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<PromptListItemDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PromptListItemDto>>> GetAll(
        CancellationToken cancellationToken,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _sender.Send(
            new GetPromptsQuery(pageNumber, pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(PromptDto),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PromptDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetPromptQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeletePromptCommand(id),
            cancellationToken);

        return NoContent();
    }
}