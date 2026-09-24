//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using PromptOptimizer.Application.Models.Queries;

//namespace PromptOptimizer.Api.Controllers;

//[Authorize]
//[ApiController]
//[Route("api/[controller]")]
//public class ModelsController : ControllerBase
//{
//    private readonly ISender _sender;

//    public ModelsController(ISender sender)
//    {
//        _sender = sender;
//    }

//    [HttpGet]
//    public async Task<IActionResult> GetModels()
//    {
//        var result = await _sender.Send(new GetAvailableModelsQuery());
//        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
//    }
//}
