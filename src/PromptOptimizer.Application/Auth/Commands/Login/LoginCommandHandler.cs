using MediatR;
using Microsoft.AspNetCore.Identity;
using PromptOptimizer.Application.Auth.DTOs;
using PromptOptimizer.Application.Auth.Interfaces;
using PromptOptimizer.Domain.Entities;
using PromptOptimizer.Application.Common.Exceptions;

namespace PromptOptimizer.Application.Auth.Commands.Login;

public class LoginCommandHandler
    : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthResponse> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Normalize email
        var email = command.Email
            .Trim()
            .ToLowerInvariant();

        // 2. Find user
        var user = await _userManager
            .FindByEmailAsync(email);

        if (user is null)
        {
            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        // 3. Check account status
        if (!user.IsActive)
        {
            throw new UnauthorizedException(
                "This account is inactive.");
        }

        // 4. Check password
        var result = await _signInManager
            .CheckPasswordSignInAsync(
                user,
                command.Password,
                lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            throw new UnauthorizedException(
                "Account is temporarily locked.");
        }

        if (!result.Succeeded)
        {
            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        // 5. Generate access token
        var accessToken =
            _jwtService.GenerateAccessToken(user);

        // 6. Generate and save refresh token
        var refreshToken =
            await _refreshTokenService.CreateAsync(
                user.Id,
                cancellationToken);

        // 7. Update last login
        user.LastLoginAt = DateTime.UtcNow;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(
                ", ",
                updateResult.Errors.Select(x => x.Description));

            throw new ApplicationException(
                $"Failed to update user: {errors}");
        }

        // 8. Return response
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