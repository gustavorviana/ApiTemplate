#if (UseValidation)
using ApiTemplate.Application.Validation;
using FluentValidation.Results;
#endif

namespace ApiTemplate.Application.UseCases.Auth.Login;

#if (UseValidation)
public class LoginRequest : IValidatableRequest
#else
public class LoginRequest
#endif
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

#if (UseValidation)
    public ValidationResult Validate() =>
        new LoginRequestValidator().Validate(this);
#endif
}