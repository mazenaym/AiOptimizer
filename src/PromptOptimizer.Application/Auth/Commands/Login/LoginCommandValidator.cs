using FluentValidation;

namespace PromptOptimizer.Application.Auth.Commands.Login;

public sealed class LoginCommandValidator
    : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Email is required.")
            .Must(email =>
                email is not null
                && email.Trim().Length <= 255)
                .WithMessage("Email must not exceed 255 characters.")
            .Must(email =>
                email is not null
                && IsValidEmail(email.Trim()))
                .WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }

    private static bool IsValidEmail(string email)
    {
        var validator = new InlineValidator<string>();

        validator.RuleFor(value => value)
            .EmailAddress();

        return validator.Validate(email).IsValid;
    }
}