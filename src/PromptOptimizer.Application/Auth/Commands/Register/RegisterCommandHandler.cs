using MediatR;
using Microsoft.AspNetCore.Identity;
using PromptOptimizer.Application.Auth.DTOs;
using PromptOptimizer.Application.Auth.Interfaces;
using PromptOptimizer.Domain.Entities;

namespace PromptOptimizer.Application.Auth.Commands.Register;

public class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthResponse> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Normalize email
        var email = command.Email
            .Trim()
            .ToLowerInvariant();

        // 2. Check existing user
        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            throw new ApplicationException(
                "Email is already registered.");
        }

        // 3. Create user
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            IsActive = true
        };

        // 4. Let Identity create the user + hash password
        var result = await _userManager.CreateAsync(
            user,
            command.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ",
                result.Errors.Select(x => x.Description));

            throw new ApplicationException(errors);
        }

        // 5. Generate access token
        var accessToken =
            _jwtService.GenerateAccessToken(user);

        // 6. Generate + save refresh token
        var refreshToken =
            await _refreshTokenService.CreateAsync(
                user.Id,
                cancellationToken);

        // 7. Return response
        return new AuthResponse
        {
            AccessToken = accessToken,

            RefreshToken = refreshToken,

            AccessTokenExpiresAt =
                _jwtService.GetAccessTokenExpiration(),

            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName
            }
        };
    }
}