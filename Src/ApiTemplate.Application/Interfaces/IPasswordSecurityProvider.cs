using ApiTemplate.Application.Core.ValueObjects;

namespace ApiTemplate.Application.Interfaces;

public interface IPasswordSecurityProvider
{
    PasswordStrengthResult Evaluate(string password);
}

