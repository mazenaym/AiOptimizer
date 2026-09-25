using MediatR;
using PromptOptimizer.Application.Auth.DTOs;

namespace PromptOptimizer.Application.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;