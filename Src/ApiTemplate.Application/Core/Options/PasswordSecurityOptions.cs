using ApiTemplate.Application.Core.Enums;

namespace ApiTemplate.Application.Core.Options;

public sealed class PasswordSecurityOptions
{
    public PasswordSecurityProviderType ProviderType { get; init; }

    public PasswordStrengthStatus MinimumStrength { get; init; }
}

