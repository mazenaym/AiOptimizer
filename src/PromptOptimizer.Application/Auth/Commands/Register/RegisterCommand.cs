//using FluentValidation;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using PromptOptimizer.Application.Auth.DTOs;
//using PromptOptimizer.Application.Common.Interfaces;
//using PromptOptimizer.Application.Common.Results;
//using PromptOptimizer.Domain.Entities;
//using PromptOptimizer.Domain.Enums;

//namespace PromptOptimizer.Application.Auth.Commands.Register;

//public record RegisterCommand(
//    string Email,
//    string Password,
//    string FullName
//) : IRequest<Result<AuthResultDto>>;

//public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
//{
//    public RegisterCommandValidator()
//    {
//        RuleFor(x => x.Email).NotEmpty().EmailAddress();
//        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
//        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
//    }
//}

//public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResultDto>>
//{
//    private readonly IAppDbContext _context;
//    private readonly IPasswordHasher _passwordHasher;
//    private readonly IJwtTokenGenerator _jwtTokenGenerator;

//    public RegisterCommandHandler(
//        IAppDbContext context,
//        IPasswordHasher passwordHasher,
//        IJwtTokenGenerator jwtTokenGenerator)
//    {
//        _context = context;
//        _passwordHasher = passwordHasher;
//        _jwtTokenGenerator = jwtTokenGenerator;
//    }

//    public async Task<Result<AuthResultDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
//    {
//        var existingUser = await _context.Users
//            .AnyAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

//        if (existingUser)
//        {
//            return Result.Failure<AuthResultDto>("User with this email already exists.");
//        }

//        var user = new User
//        {
//            Email = request.Email.ToLowerInvariant(),
//            PasswordHash = _passwordHasher.HashPassword(request.Password),
//            FullName = request.FullName,
//            Plan = SubscriptionPlan.Free,
//            CreatedBy = request.Email
//        };

//        _context.Users.Add(user);

//        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);
//        _context.RefreshTokens.Add(refreshToken);

//        await _context.SaveChangesAsync(cancellationToken);

//        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);

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
