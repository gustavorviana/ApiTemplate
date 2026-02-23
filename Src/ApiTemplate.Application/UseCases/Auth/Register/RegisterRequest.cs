#if (UseDatabase)
using ApiTemplate.Application.Validation;
using FluentValidation.Results;
#endif

namespace ApiTemplate.Application.UseCases.Auth.Register;

#if (UseDatabase)
public class RegisterRequest : IValidatableRequest
#else
public class RegisterRequest
#endif
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

#if (UseDatabase)
    public ValidationResult Validate() =>
        new RegisterRequestValidator().Validate(this);
#endif
}