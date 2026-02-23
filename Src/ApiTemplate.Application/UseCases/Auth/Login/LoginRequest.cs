#if (UseDatabase)
using ApiTemplate.Application.Validation;
using FluentValidation.Results;
#endif

namespace ApiTemplate.Application.UseCases.Auth.Login;

#if (UseDatabase)
public class LoginRequest : IValidatableRequest
#else
public class LoginRequest
#endif
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

#if (UseDatabase)
    public ValidationResult Validate() =>
        new LoginRequestValidator().Validate(this);
#endif
}