using MediatR;
using PromptOptimizer.Application.Auth.DTOs;

namespace PromptOptimizer.Application.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string? FirstName,
    string? LastName
) : IRequest<AuthResponse>;