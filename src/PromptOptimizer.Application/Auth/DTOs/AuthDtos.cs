namespace PromptOptimizer.Application.Auth.DTOs;

public record AuthResultDto(
    Guid UserId,
    string Email,
    string FullName,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);

public record RegisterRequestDto(
    string Email,
    string Password,
    string FullName
);

public record LoginRequestDto(
    string Email,
    string Password
);

public record RefreshTokenRequestDto(
    string AccessToken,
    string RefreshToken
);
