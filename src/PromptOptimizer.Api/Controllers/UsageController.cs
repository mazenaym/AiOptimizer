//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using PromptOptimizer.Application.Usage.Queries;

//namespace PromptOptimizer.Api.Controllers;

//[Authorize]
//[ApiController]
//[Route("api/[controller]")]
//public class UsageController : ControllerBase
//{
//    private readonly ISender _sender;

//    public UsageController(ISender sender)
//    {
//        _sender = sender;
//    }

//    [HttpGet("summary")]
//    public async Task<IActionResult> GetUsageSummary()
//    {
//        var result = await _sender.Send(new GetUserUsageQuery());
//        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
//    }
//}
