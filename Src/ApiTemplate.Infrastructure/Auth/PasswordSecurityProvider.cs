using System.Text;
using ApiTemplate.Application.Core.Enums;
using ApiTemplate.Application.Core.Options;
using ApiTemplate.Application.Core.ValueObjects;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Infrastructure.Resources;
using Microsoft.Extensions.Options;

namespace ApiTemplate.Infrastructure.Auth;

public sealed class PasswordSecurityProvider : IPasswordSecurityProvider
{
    private readonly PasswordSecurityOptions _options;

    public PasswordSecurityProvider(IOptions<PasswordSecurityOptions> options)
    {
        _options = options.Value;
    }

    public PasswordStrengthResult Evaluate(string password)
    {
        var minimumRequired = _options.MinimumStrength;

        if (string.IsNullOrWhiteSpace(password))
        {
            return new PasswordStrengthResult
            {
                Strength = PasswordStrengthStatus.Unacceptable,
                MinimumRequired = minimumRequired,
                MissingRequirements = Messages.PasswordSecurityMessages.PasswordCannotBeEmpty
            };
        }

        var length = password.Length;
        var hasLower = password.Any(char.IsLower);
        var hasUpper = password.Any(char.IsUpper);
        var hasDigit = password.Any(char.IsDigit);
        var hasSymbol = password.Any(ch => !char.IsLetterOrDigit(ch));

        var strength = CalculateStrength(length, hasLower, hasUpper, hasDigit, hasSymbol);

        if (strength >= minimumRequired)
        {
            return new PasswordStrengthResult
            {
                Strength = strength,
                MinimumRequired = minimumRequired,
                MissingRequirements = null
            };
        }

        var missing = BuildMissingRequirements(
            minimumRequired,
            length,
            hasLower,
            hasUpper,
            hasDigit,
            hasSymbol);

        return new PasswordStrengthResult
        {
            Strength = strength,
            MinimumRequired = minimumRequired,
            MissingRequirements = missing
        };
    }

    private static PasswordStrengthStatus CalculateStrength(
        int length,
        bool hasLower,
        bool hasUpper,
        bool hasDigit,
        bool hasSymbol)
    {
        if (length < 8)
            return PasswordStrengthStatus.Unacceptable;

        var score = 0;
        if (length >= 12) score++;
        if (length >= 14) score++;
        if (hasLower) score++;
        if (hasUpper) score++;
        if (hasDigit) score++;
        if (hasSymbol) score++;

        return score switch
        {
            <= 2 => PasswordStrengthStatus.VeryWeak,
            3 => PasswordStrengthStatus.Weak,
            4 => PasswordStrengthStatus.Medium,
            5 => PasswordStrengthStatus.Strong,
            _ => PasswordStrengthStatus.VeryStrong
        };
    }

    private static string BuildMissingRequirements(
        PasswordStrengthStatus minimum,
        int length,
        bool hasLower,
        bool hasUpper,
        bool hasDigit,
        bool hasSymbol)
    {
        var builder = new StringBuilder();

        void Append(string text)
        {
            if (builder.Length > 0)
                builder.Append(' ');

            builder.Append(text);
        }

        var minimumLength = minimum switch
        {
            PasswordStrengthStatus.Unacceptable => 0,
            PasswordStrengthStatus.VeryWeak => 6,
            PasswordStrengthStatus.Weak => 8,
            PasswordStrengthStatus.Medium => 10,
            PasswordStrengthStatus.Strong => 12,
            PasswordStrengthStatus.VeryStrong => 14,
            _ => 6
        };

        if (length < minimumLength)
        {
            Append(minimumLength switch
            {
                6 => Messages.PasswordSecurityMessages.PasswordAtLeast6Chars,
                8 => Messages.PasswordSecurityMessages.PasswordAtLeast8Chars,
                10 => Messages.PasswordSecurityMessages.PasswordAtLeast10Chars,
                12 => Messages.PasswordSecurityMessages.PasswordAtLeast12Chars,
                14 => Messages.PasswordSecurityMessages.PasswordAtLeast14Chars,
                _ => Messages.PasswordSecurityMessages.PasswordBelowMinimum
            });
        }

        var categoryCount = (hasLower ? 1 : 0)
                          + (hasUpper ? 1 : 0)
                          + (hasDigit ? 1 : 0)
                          + (hasSymbol ? 1 : 0);

        if (minimum >= PasswordStrengthStatus.Weak && categoryCount < 2)
            Append(Messages.PasswordSecurityMessages.PasswordMustContainTwoCategories);

        if (minimum >= PasswordStrengthStatus.Medium && categoryCount < 3)
            Append(Messages.PasswordSecurityMessages.PasswordMustContainThreeCategories);

        if (minimum >= PasswordStrengthStatus.Strong)
        {
            if (!hasDigit)
                Append(Messages.PasswordSecurityMessages.PasswordMustContainDigit);

            if (!hasSymbol)
                Append(Messages.PasswordSecurityMessages.PasswordMustContainSymbol);
        }

        if (minimum >= PasswordStrengthStatus.VeryStrong && categoryCount < 4)
            Append(Messages.PasswordSecurityMessages.PasswordMustContainAllCategories);

        return builder.ToString();
    }
}
