using ApiTemplate.Application.Core.Enums;
using ApiTemplate.Application.Core.Options;
using ApiTemplate.Application.Core.ValueObjects;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Infrastructure.MessagesCatalog;
using System.Text;

namespace ApiTemplate.Infrastructure.Auth;

public sealed class IsoPasswordSecurityProvider : IPasswordSecurityProvider
{
    private readonly PasswordSecurityOptions _options;

    public IsoPasswordSecurityProvider(PasswordSecurityOptions options)
    {
        _options = options;
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

        var missing = BuildMissingRequirementsForMinimum(
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
        {
            return PasswordStrengthStatus.Unacceptable;
        }

        var score = 0;

        if (length >= 12)
        {
            score++;
        }

        if (length >= 14)
        {
            score++;
        }

        if (hasLower)
        {
            score++;
        }

        if (hasUpper)
        {
            score++;
        }

        if (hasDigit)
        {
            score++;
        }

        if (hasSymbol)
        {
            score++;
        }

        return score switch
        {
            <= 2 => PasswordStrengthStatus.VeryWeak,
            3 => PasswordStrengthStatus.Weak,
            4 => PasswordStrengthStatus.Medium,
            5 => PasswordStrengthStatus.Strong,
            _ => PasswordStrengthStatus.VeryStrong
        };
    }

    private static string BuildMissingRequirementsForMinimum(
        PasswordStrengthStatus minimum,
        int length,
        bool hasLower,
        bool hasUpper,
        bool hasDigit,
        bool hasSymbol)
    {
        var builder = new StringBuilder();

        void AppendRequirement(string text)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(text);
        }

        if (minimum >= PasswordStrengthStatus.VeryWeak && length < 10)
        {
            AppendRequirement(Messages.PasswordSecurityMessages.PasswordAtLeast10Chars);
        }

        if (minimum >= PasswordStrengthStatus.Medium && length < 12)
        {
            AppendRequirement(Messages.PasswordSecurityMessages.PasswordAtLeast12Chars);
        }

        if (minimum >= PasswordStrengthStatus.Strong && length < 14)
        {
            AppendRequirement(Messages.PasswordSecurityMessages.PasswordAtLeast14Chars);
        }

        var categoryCount = (hasLower ? 1 : 0)
                            + (hasUpper ? 1 : 0)
                            + (hasDigit ? 1 : 0)
                            + (hasSymbol ? 1 : 0);

        if (minimum >= PasswordStrengthStatus.Medium && categoryCount < 3)
        {
            AppendRequirement(Messages.PasswordSecurityMessages.PasswordMustContainThreeCategories);
        }

        if (minimum >= PasswordStrengthStatus.Strong && (!hasDigit || !hasSymbol))
        {
            AppendRequirement(Messages.PasswordSecurityMessages.PasswordMustContainDigit);
            AppendRequirement(Messages.PasswordSecurityMessages.PasswordMustContainSymbol);
        }

        if (minimum >= PasswordStrengthStatus.VeryStrong && categoryCount < 4)
        {
            AppendRequirement(Messages.PasswordSecurityMessages.PasswordMustContainAllCategories);
        }

        if (builder.Length == 0)
        {
            AppendRequirement(Messages.PasswordSecurityMessages.PasswordBelowMinimum);
        }

        return builder.ToString();
    }
}

