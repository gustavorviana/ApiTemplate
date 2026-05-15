using ApiTemplate.Domain.Enums;

namespace ApiTemplate.Domain.Options;

public sealed class PasswordSecurityOptions
{
    public const string SectionName = "PasswordSecurity";

    public PasswordStrengthStatus MinimumStrength { get; set; } = PasswordStrengthStatus.Medium;
}
