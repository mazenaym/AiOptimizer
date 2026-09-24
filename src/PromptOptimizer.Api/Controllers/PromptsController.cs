//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using PromptOptimizer.Application.Prompts.Commands.DeletePrompt;
//using PromptOptimizer.Application.Prompts.Commands.OptimizePrompt;
//using PromptOptimizer.Application.Prompts.Commands.SavePrompt;
//using PromptOptimizer.Application.Prompts.Queries.GetPrompt;
//using PromptOptimizer.Application.Prompts.Queries.GetPromptHistory;

//namespace PromptOptimizer.Api.Controllers;

//[Authorize]
//[ApiController]
//[Route("api/[controller]")]
//public class PromptsController : ControllerBase
//{
//    private readonly ISender _sender;

//    public PromptsController(ISender sender)
//    {
//        _sender = sender;
//    }

//    [HttpPost("optimize")]
//    public async Task<IActionResult> Optimize([FromBody] OptimizePromptCommand command)
//    {
//        var result = await _sender.Send(command);
//        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
//    }

//    [HttpPost]
//    public async Task<IActionResult> Save([FromBody] SavePromptCommand command)
//    {
//        var result = await _sender.Send(command);
//        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
//    }

//    [HttpGet("{id:guid}")]
//    public async Task<IActionResult> GetById(Guid id)
//    {
//        var result = await _sender.Send(new GetPromptQuery(id));
//        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
//    }

//    [HttpGet("history")]
//    public async Task<IActionResult> GetHistory([FromQuery] Guid? promptId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
//    {
//        var result = await _sender.Send(new GetPromptHistoryQuery(promptId, pageNumber, pageSize));
//        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
//    }

//    [HttpDelete("{id:guid}")]
//    public async Task<IActionResult> Delete(Guid id)
//    {
//        var result = await _sender.Send(new DeletePromptCommand(id));
//        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error });
//    }
//}
