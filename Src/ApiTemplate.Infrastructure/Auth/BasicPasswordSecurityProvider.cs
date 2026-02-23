using System.Text;
using ApiTemplate.Application.Core.Enums;
using ApiTemplate.Application.Core.Options;
using ApiTemplate.Application.Core.ValueObjects;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Infrastructure.MessagesCatalog;

namespace ApiTemplate.Infrastructure.Auth;

public sealed class BasicPasswordSecurityProvider(PasswordSecurityOptions options) : IPasswordSecurityProvider
{
    public PasswordStrengthResult Evaluate(string password)
    {
        var minimumRequired = options.MinimumStrength;

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
        if (length < 6)
        {
            return PasswordStrengthStatus.Unacceptable;
        }

        var score = 0;

        if (length >= 8)
        {
            score++;
        }

        if (length >= 12)
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
            <= 1 => PasswordStrengthStatus.VeryWeak,
            2 => PasswordStrengthStatus.Weak,
            3 or 4 => PasswordStrengthStatus.Medium,
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

        if (minimum >= PasswordStrengthStatus.VeryWeak && length < 8)
        {
            AppendRequirement(Messages.PasswordSecurityMessages.PasswordAtLeast8Chars);
        }

        if (minimum >= PasswordStrengthStatus.Medium && !hasDigit)
        {
            AppendRequirement(Messages.PasswordSecurityMessages.PasswordMustContainDigit);
        }

        if (minimum >= PasswordStrengthStatus.Strong && !(hasLower && hasUpper))
        {
            AppendRequirement(Messages.PasswordSecurityMessages.PasswordMustContainLowerAndUpper);
        }

        if (minimum >= PasswordStrengthStatus.VeryStrong && !hasSymbol)
        {
            AppendRequirement(Messages.PasswordSecurityMessages.PasswordMustContainSymbol);
        }

        if (builder.Length == 0)
        {
            AppendRequirement(Messages.PasswordSecurityMessages.PasswordBelowMinimum);
        }

        return builder.ToString();
    }
}

