using ApiTemplate.Application.MessagesCatalog;
using FluentValidation;

namespace ApiTemplate.Application.UseCases.Auth.Login;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(Messages.Auth.EmailRequired)
            .EmailAddress()
            .WithMessage(Messages.Auth.EmailInvalid);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(Messages.Auth.PasswordRequired);
    }
}