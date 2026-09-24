//using MediatR;
//using Microsoft.AspNetCore.Mvc;
//using PromptOptimizer.Application.Auth.Commands.Login;
//using PromptOptimizer.Application.Auth.Commands.Register;
//using PromptOptimizer.Application.Auth.DTOs;

//namespace PromptOptimizer.Api.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//public class AuthController : ControllerBase
//{
//    private readonly ISender _sender;

//    public AuthController(ISender sender)
//    {
//        _sender = sender;
//    }

//    [HttpPost("register")]
//    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
//    {
//        var result = await _sender.Send(new RegisterCommand(request.Email, request.Password, request.FullName));
//        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
//    }

//    [HttpPost("login")]
//    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
//    {
//        var result = await _sender.Send(new LoginCommand(request.Email, request.Password));
//        return result.IsSuccess ? Ok(result.Value) : Unauthorized(new { error = result.Error });
//    }
//}
