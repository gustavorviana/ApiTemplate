using System.ComponentModel.DataAnnotations;
using ApiTemplate.Application.Core.Enums;

namespace ApiTemplate.Application.Core.Options;

public sealed class PasswordSecurityOptions
{
    public const string SectionName = "PasswordSecurity";

    [EnumDataType(typeof(PasswordStrengthStatus))]
    public PasswordStrengthStatus MinimumStrength { get; set; } = PasswordStrengthStatus.Medium;
}
