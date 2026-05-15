using ApiTemplate.Domain.Enums;

namespace ApiTemplate.Domain.ValueObjects;

public sealed class PasswordStrengthResult
{
    public PasswordStrengthStatus Strength { get; init; }

    public PasswordStrengthStatus MinimumRequired { get; init; }

    public bool MeetsMinimum => Strength >= MinimumRequired;

    public string? MissingRequirements { get; init; }
}
