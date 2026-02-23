using ApiTemplate.Application.MessagesCatalog;
using FluentValidation;

namespace ApiTemplate.Application.UseCases.Auth.Register;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Messages.Auth.NameRequired)
            .MinimumLength(2)
            .WithMessage(Messages.Auth.NameMinLength)
            .MaximumLength(200)
            .WithMessage(Messages.Auth.NameMaxLength);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(Messages.Auth.EmailRequired)
            .EmailAddress()
            .WithMessage(Messages.Auth.EmailInvalid);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(Messages.Auth.PasswordRequired)
            .MinimumLength(8)
            .WithMessage(Messages.Auth.PasswordMinLength);
    }
}
