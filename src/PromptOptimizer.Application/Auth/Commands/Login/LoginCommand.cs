//using FluentValidation;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using PromptOptimizer.Application.Auth.DTOs;
//using PromptOptimizer.Application.Common.Interfaces;
//using PromptOptimizer.Application.Common.Results;

//namespace PromptOptimizer.Application.Auth.Commands.Login;

//public record LoginCommand(
//    string Email,
//    string Password
//) : IRequest<Result<AuthResultDto>>;

//public class LoginCommandValidator : AbstractValidator<LoginCommand>
//{
//    public LoginCommandValidator()
//    {
//        RuleFor(x => x.Email).NotEmpty().EmailAddress();
//        RuleFor(x => x.Password).NotEmpty();
//    }
//}

//public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResultDto>>
//{
//    private readonly IAppDbContext _context;
//    private readonly IPasswordHasher _passwordHasher;
//    private readonly IJwtTokenGenerator _jwtTokenGenerator;

//    public LoginCommandHandler(
//        IAppDbContext context,
//        IPasswordHasher passwordHasher,
//        IJwtTokenGenerator jwtTokenGenerator)
//    {
//        _context = context;
//        _passwordHasher = passwordHasher;
//        _jwtTokenGenerator = jwtTokenGenerator;
//    }

//    public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
//    {
//        var user = await _context.Users
//            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

//        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
//        {
//            return Result.Failure<AuthResultDto>("Invalid email or password.");
//        }

//        if (!user.IsActive)
//        {
//            return Result.Failure<AuthResultDto>("User account is deactivated.");
//        }

//        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
//        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

//        _context.RefreshTokens.Add(refreshToken);
//        await _context.SaveChangesAsync(cancellationToken);

//        var dto = new AuthResultDto(
//            user.Id,
//            user.Email,
//            user.FullName,
//            accessToken,
//            refreshToken.Token,
//            refreshToken.ExpiresAt
//        );

//        return Result.Success(dto);
//    }
//}
